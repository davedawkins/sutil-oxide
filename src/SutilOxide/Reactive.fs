module SutilOxide.Reactive

open System
open Fable.Core

[<Mangle>]
type IEvent<'T> =
    inherit IObservable<'T>
    inherit System.IDisposable

[<Mangle>]
type IEventSource<'T> =
    inherit IEvent<'T>
    abstract Notify: 'T -> unit

type ISignal<'T> =
    inherit IEvent<'T>
    abstract Value: 'T with get
    
type ICell<'T> =
    inherit IEvent<'T>
    inherit ISignal<'T>
    abstract Set : 'T -> unit

type Promise<'T> = Fable.Core.JS.Promise<'T>

type IAsyncEventSource<'T> =
    abstract Subscribe: ('T -> Promise<unit>) -> System.IDisposable
    abstract NotifyAsync: 'T -> Promise<unit>

/// #782: Fable mangles the function-taking Subscribe, leaving the IObservable one as the only
/// plain `Subscribe` a JS caller can reach. These normalise whatever JS actually passed.
[<RequireQualifiedAccess>]
module private Observer =
    open Fable.Core.JsInterop

    let private hasCallable (o: obj) (name: string) : bool =
        emitJsExpr (o, name) "($0 != null && typeof $0[$1] === 'function')"

    let private isFunction (o: obj) : bool = emitJsExpr o "(typeof $0 === 'function')"

    /// Report and contain, matching fable-library's Observer: one bad subscriber must not abort
    /// delivery to the rest of the notify loop, but it must not vanish either (#782).
    let private report (context: string) (e: exn) =
        Fable.Core.JS.console.error("SutilOxide " + context + ".Subscribe: subscriber threw", e)

    /// Accept an observer, or wrap a bare callback, or fail here — never at the next Notify (#782).
    let ofJs<'T> (context: string) (observer: IObserver<'T>) : IObserver<'T> =
        // Observer first, so something callable that also carries OnNext keeps its observer behaviour.
        if hasCallable observer "OnNext" then
            if hasCallable observer "OnCompleted" && hasCallable observer "OnError" then
                observer
            else
                // Fill what a hand-written JS observer omits, so trace's error path cannot TypeError (#782).
                let hasCompleted = hasCallable observer "OnCompleted"
                let hasError = hasCallable observer "OnError"
                { new IObserver<'T> with
                    member _.OnNext(v) = observer.OnNext v
                    member _.OnCompleted() = if hasCompleted then observer.OnCompleted()
                    member _.OnError(e) = if hasError then observer.OnError e else report context e }
        elif isFunction observer then
            let f : 'T -> unit = unbox observer
            { new IObserver<'T> with
                member _.OnNext(v) = f v
                member _.OnCompleted() = ()
                member _.OnError(e) = report context e }
        else
            failwithf "%s.Subscribe expects an observer with OnNext, or a callback function — received %s (#782)"
                context (jsTypeof observer)

    /// The async surface takes a callback, not an observer — fail at subscribe, not at NotifyAsync (#782).
    let asCallback<'T,'R> (context: string) (handler: 'T -> 'R) : 'T -> 'R =
        if isFunction handler then
            handler
        else
            failwithf "%s.Subscribe expects a callback function — received %s (#782)"
                context (jsTypeof handler)

module Internal =

    open System.Collections.Generic
    open Fable.Core

    /// Non-generic base exposing live subscriber count for leak instrumentation (#394).
    [<AbstractClass>]
    type SubscriberCounted() =
        abstract SubscriberCount : int

    [<Mangle>]
    type EventSourceWithResult<'T,'R>() =
        inherit SubscriberCounted()
        let mutable nextId = 0
        let clients = Dictionary<int, 'T -> 'R>()

        let getNextId() =
            let id = nextId
            nextId <- nextId + 1
            id

        // Subscribe a function that takes 'T and returns 'R
        abstract Subscribe: ('T -> 'R) -> System.IDisposable
        default this.Subscribe(client : 'T -> 'R) : System.IDisposable =
            this.DefaultSubscribe(client)

        member private this.DefaultSubscribe(client : 'T -> 'R) : IDisposable =
            let clientId = getNextId()
            clients.[clientId] <- client
            { new System.IDisposable with
                member _.Dispose() =
                    clients.Remove(clientId) |> ignore }

        abstract Notify: 'T -> unit
        default this.Notify(value : 'T) : unit =
            for client in clients.Values do
                client value |> ignore

        member _.Dispose() = clients.Clear()

        override _.SubscriberCount = clients.Count

        member this.NotifyAndCollect(value : 'T) : 'R[] =
            [|
                for client in clients.Values do
                    yield client value
            |]

    type EventSource<'T>() = 
        inherit EventSourceWithResult<'T,unit>()
        
        interface IObservable<'T> with
            member this.Subscribe (observer: IObserver<'T>): IDisposable =
                let observer = Observer.ofJs "EventSource" observer
                this.Subscribe( fun v -> observer.OnNext(v) )

        interface IEventSource<'T> with
            member this.Dispose() = this.Dispose()
            member this.Notify(v) = this.Notify(v)

    type Cell<'T>(init: (unit -> 'T) option) =
        inherit SubscriberCounted()
        let disposeListeners = new ResizeArray<unit -> unit>()
        let clients = new EventSourceWithResult<'T,unit>()
        
        let mutable _value_initialized = false
        let mutable _value = Unchecked.defaultof<'T> // init()

        let _set v = 
            _value <- v
            _value_initialized <- true

        let _set_notify v = 
            _set v
            clients.Notify(_value)
            
        let _init( assert_is_set : bool ) = 
            if not _value_initialized then
                match init with
                | Some i -> _set(i())
                | None -> if assert_is_set then failwithf "Cell read before initialized"
        
        let _get( assert_is_set : bool ) = 
            _init assert_is_set
            _value

        member _.Subscribe(handler) = 
            let unsub = clients.Subscribe(handler)

            _init(false)
            if _value_initialized then handler(_value)

            unsub

        member _.Set(v) = _set_notify v
        member _.Value with get() = _get(true)

        override _.SubscriberCount = clients.SubscriberCount

        member _.OnDispose( f : unit -> unit ) =
            disposeListeners.Add(f)

        interface ICell<'T> with
            member _.Value with get() = _get(true) 
            member __.Set(v) = __.Set(v)
            member _.Dispose() = 
                clients.Dispose()
                disposeListeners |> Seq.iter (fun f -> f())
                disposeListeners.Clear()

        interface System.IObservable<'T> with
            member _.Subscribe( observer : System.IObserver<'T> ) =
                let observer = Observer.ofJs "Cell" observer
                _init(false)
                if _value_initialized then observer.OnNext(_value)
                clients.Subscribe( fun v -> observer.OnNext(v) )

    type Promise<'T> = Fable.Core.JS.Promise<'T>

    open Fable.Core
    type PromiseEventSource<'T>() =
        let _es = new EventSourceWithResult< 'T, Promise<unit> >()

        interface IAsyncEventSource<'T> with

            member __.Subscribe (arg: 'T -> Promise<unit>)=
                _es.Subscribe( Observer.asCallback "AsyncEventSource" arg )

            member this.NotifyAsync(value: 'T) : Promise<unit> =
                let allPromises = _es.NotifyAndCollect(value)
                // Turn seq of promises into a single promise that completes once all are done
                (allPromises |> Promise.all).``then``(ignore)

[<RequireQualifiedAccess>]
module CellInternal =
    let make<'T> (init : 'T) = new Internal.Cell<'T>(Some (fun () -> init))
    let makef<'T> (init : unit -> 'T) = new Internal.Cell<'T>(Some init)
    let makeu<'T> () = new Internal.Cell<'T>(None)

module Cell =
    let set (cell : ICell<'a>) (v : 'a) = cell.Set v

    let make<'T> (init : 'T) : ICell<'T> = CellInternal.make init

    let makef<'T> (init : unit -> 'T) : ICell<'T> = CellInternal.makef init

    /// <summary>
    /// Make with uninitialized value.
    /// Will throw if read before initialized.
    /// Won't initialize new subscribers until initialized.
    /// </summary>
    let makeu<'T> () : ICell<'T> = CellInternal.makeu ()

    // Set/modify always notify, even for an identical value — Log.fs's `logMessages.Set(logMessages.Value)`
    // relies on exactly that to force a re-render after an in-place mutation (#596). setDistinct/modifyDistinct
    // below are the guarded forms; client/src/ts/framework.ts's MemoryCell guards unconditionally instead,
    // so the two primitives differ deliberately (#596).
    let modify (f : 'T -> 'T) (cell : ICell<'T>) = cell.Value |> f |> cell.Set

    /// Guarded set: skips the notify when v equals the cell's current value (#596).
    let setDistinct<'T when 'T : equality> (v : 'T) (cell : ICell<'T>) =
        if cell.Value <> v then cell.Set v

    /// Guarded modify: skips the notify when f's result equals the current value (#596).
    let modifyDistinct<'T when 'T : equality> (f : 'T -> 'T) (cell : ICell<'T>) =
        let v = cell.Value
        let v' = f v
        if v <> v' then cell.Set v'


[<RequireQualifiedAccess>]
module EventSource =
    let inline make<'t>() : IEventSource<'t> = new Internal.EventSource<'t>()


[<RequireQualifiedAccess>]
module Event =

    let fromObservable<'T> (source:IObservable<'T>) : IEvent<'T> =
        { new IEvent<_> with
            member _.Subscribe (observer: IObserver<'T>): IDisposable = source.Subscribe observer
            member _.Dispose() = () }

[<RequireQualifiedAccess>]
module Signal =

    let make (init : 'T) : ISignal<'T> = new Internal.Cell<_>(Some(fun () -> init))

    let fromSingle( v : 'T ) = make v
    
    let fromPromise (init : 'T)  (p : JS.Promise<'T>) =
        let s = Cell.make init
        p |> Promise.iter(s.Set)
        s

    let fromObservable<'T> (init : 'T) (source:IObservable<'T>) : ISignal<'T> =
        let mutable value = init
        let dispose0 = source.Subscribe( fun next -> value <- next )
        { new ISignal<'T> with
            member _.Value = value
            member _.Dispose() = dispose0.Dispose()
            member _.Subscribe( h : IObserver<'T> ) =
                let h = Observer.ofJs "Signal.fromObservable" h
                let dispose = source.Subscribe( fun next ->
                    h.OnNext next
                )
                h.OnNext value
                dispose
        }

    let fromStore<'T> (source:Sutil.IStore<'T>) : ISignal<'T> =
        let mutable value = source.Value
        { new ISignal<'T> with
            member _.Value = value
            member _.Dispose() = ()
            member _.Subscribe( h : IObserver<'T> ) =
                let h = Observer.ofJs "Signal.fromStore" h
                let dispose = source.Subscribe( fun next ->
                    value <- next
                    h.OnNext next
                )
                h.OnNext value
                dispose
        }

    let map (f : 'T -> 'U) (source : ISignal<'T>) : ISignal<'U> =
        let cell = CellInternal.make( f(source.Value) )
        let unsub = source.Subscribe( fun v -> cell.Set(f v ) )
        cell.OnDispose(fun _ -> unsub.Dispose()) 
        cell

    let bind (f : 'T -> ISignal<'U>) (source : ISignal<'T>) : ISignal<'U> =
        let cell = CellInternal.make( (f(source.Value)).Value )
        let mutable unsub2 : System.IDisposable option = None

        let unsub1 = source.Subscribe( fun v -> 
            let s2 = f v
            unsub2 |> Option.iter _.Dispose()
            unsub2 <- Some (s2.Subscribe(cell.Set))
        )

        cell.OnDispose(fun _ -> unsub1.Dispose(); unsub2 |> Option.iter _.Dispose()) 
        cell

    let mapDistinct<'T,'U when 'U : equality> (f : 'T -> 'U) (source : ISignal<'T>) : ISignal<'U> =
        let cell = CellInternal.make( f(source.Value) )

        let unsub = source.Subscribe( fun v -> 
            let u = f v
            if cell.Value <> u then cell.Set(u) )

        cell.OnDispose(fun _ -> unsub.Dispose()) 
        cell

    let filter (f : 'T -> bool) (source : ISignal<'T>) : ISignal<'T option> =
        let fopt v = if f v then Some v else None
        source |> map fopt

    let map2<'A, 'B, 'Res> (f: 'A -> 'B -> 'Res) (a: ISignal<'A>) (b: ISignal<'B>) : ISignal<'Res> =
        let cell = CellInternal.make (f (a.Value) (b.Value) )
        let unsub_a = a.Subscribe( fun v_a -> cell.Set(f v_a     b.Value ) )
        let unsub_b = b.Subscribe( fun v_b -> cell.Set(f a.Value v_b ) )
        cell.OnDispose(fun _ -> unsub_a.Dispose(); unsub_b.Dispose()) 
        cell

    let zip<'A,'B> (a:ISignal<'A>) (b:ISignal<'B>) : ISignal<'A*'B> =
        map2<'A, 'B, 'A * 'B> (fun a b -> a, b) a b

    type TraceEvent<'T> =
        | Subscribed of int
        | NotifyStarted of int * 'T * double 
        | NotifyCompleted of int * 'T * double * double
        | Unsubscribed of int

    let trace<'T> (log: TraceEvent<'T> -> unit) (src : ISignal<'T>) : ISignal<'T> =
        let mutable nextId = 0
        { new ISignal<'T> with 
            member __.Dispose() = src.Dispose()
            member __.Value with get() = src.Value
            member __.Subscribe (observer: IObserver<'T>): IDisposable =
                let observer = Observer.ofJs "Signal.trace" observer

                let clientId = nextId
                nextId <- nextId + 1   // drive-by: was `nextId <- nextId`, so every traced subscriber was id 0

                let dispose = src.Subscribe(
                    { new IObserver<'T> with
                        member __.OnCompleted (): unit = observer.OnCompleted()
                        member __.OnError (error: exn): unit = observer.OnError(error)
                        member __.OnNext (value: 'T): unit = 
                            let started = JsHelpers.performanceNow()
                            try
                                log (NotifyStarted (clientId, value, started))
                                observer.OnNext(value)
                            with
                            | x ->
                                observer.OnError(x)
                            let completed = JsHelpers.performanceNow()
                            log (NotifyCompleted (clientId, value, started, completed))
                    })
                
                log (Subscribed clientId)
                { new IDisposable with
                    member __.Dispose() = 
                        dispose.Dispose()
                        log (Unsubscribed clientId)
                }
        }



module Observable =
    /// Give a plain Fable observable the same JS-argument normalisation as SutilOxide's own
    /// surfaces — fable-library's combinators build observers with no guard of their own (#782).
    let forJs<'T> (context: string) (src : System.IObservable<'T>) : System.IObservable<'T> =
        { new System.IObservable<'T> with
            member _.Subscribe( observer : IObserver<'T> ) =
                src.Subscribe( Observer.ofJs context observer ) }

    let trace<'T> (log: Signal.TraceEvent<'T> -> unit) (src : System.IObservable<'T>) : IObservable<'T> =
        let mutable nextId = 0
        { new IObservable<'T> with
            member __.Subscribe (observer: IObserver<'T>): IDisposable =
                let observer = Observer.ofJs "Observable.trace" observer

                let clientId = nextId
                nextId <- nextId + 1   // drive-by: was `nextId <- nextId`, so every traced subscriber was id 0

                let dispose = src.Subscribe(
                    { new IObserver<'T> with
                        member __.OnCompleted (): unit = observer.OnCompleted()
                        member __.OnError (error: exn): unit = observer.OnError(error)
                        member __.OnNext (value: 'T): unit = 
                            let started = JsHelpers.performanceNow()
                            try
                                log (Signal.NotifyStarted (clientId, value, started))
                                observer.OnNext(value)
                            with
                            | x ->
                                observer.OnError(x)
                            let completed = JsHelpers.performanceNow()
                            log (Signal.NotifyCompleted (clientId, value, started, completed))
                    })
                
                log (Signal.Subscribed clientId)
                { new IDisposable with
                    member __.Dispose() = 
                        dispose.Dispose()
                        log (Signal.Unsubscribed clientId)
                }
        }


module SignalOperators =
    let (.>) s f = Signal.map f s

    /// <summary>
    /// Alias for <c>Store.mapDistinct</c>
    /// </summary>
    let (.>>) s f = Signal.mapDistinct f s

[<AutoOpen>]
module ReactiveEx =
    type IEvent<'T> with

        member self.Until( pred : ('T -> bool), f : ('T -> unit) ) =
            let mutable sub : System.IDisposable option = None

            let cleanup (s : System.IDisposable) =
                s.Dispose()
                sub <- None

            sub <- (self.Subscribe( fun v ->
                sub |> Option.iter (fun s ->
                    if pred v then
                        f v
                        cleanup s)
            )) |> Some

            (fun () -> sub |> Option.iter cleanup)

        member self.Once( oneTime : ('T -> unit) ) =
            self.Until( (fun _ -> true), oneTime )


[<RequireQualifiedAccess>]
module AsyncEventSource =
    let make<'a>(): IAsyncEventSource<'a> = new Internal.PromiseEventSource<'a>()

