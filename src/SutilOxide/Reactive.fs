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

module Internal =

    open System.Collections.Generic
    open Fable.Core

    [<Mangle>]
    type EventSourceWithResult<'T,'R>() =
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

        member this.NotifyAndCollect(value : 'T) : 'R[] =
            [|
                for client in clients.Values do
                    yield client value
            |]

    type EventSource<'T>() = 
        inherit EventSourceWithResult<'T,unit>()
        
        interface IObservable<'T> with
            member this.Subscribe (observer: IObserver<'T>): IDisposable = 
                this.Subscribe( fun v -> observer.OnNext(v) )

        interface IEventSource<'T> with
            member this.Dispose() = this.Dispose()
            member this.Notify(v) = this.Notify(v)

    type Cell<'T>(init:'T) =
        let clients = new EventSourceWithResult<'T,unit>()
        let mutable value = init
        let _set v = 
            value <- v
            clients.Notify(value)

        member _.Subscribe(handler) = 
            let unsub = clients.Subscribe(handler)
            handler(value)
            unsub

        interface ICell<'T> with
            member _.Value with get() = value 
            member _.Set(v) = _set(v)
            member _.Dispose() = clients.Dispose()

        interface System.IObservable<'T> with
            member _.Subscribe( observer : System.IObserver<'T> ) =
                observer.OnNext(value)
                clients.Subscribe( fun v -> observer.OnNext(v) )

    type Promise<'T> = Fable.Core.JS.Promise<'T>

    open Fable.Core
    type PromiseEventSource<'T>() =
        let _es = new EventSourceWithResult< 'T, Promise<unit> >()

        interface IAsyncEventSource<'T> with

            member __.Subscribe (arg: 'T -> Promise<unit>)= 
                _es.Subscribe(arg)

            member this.NotifyAsync(value: 'T) : Promise<unit> =
                let allPromises = _es.NotifyAndCollect(value)
                // Turn seq of promises into a single promise that completes once all are done
                (allPromises |> Promise.all).``then``(ignore)

[<RequireQualifiedAccess>]

module Cell =
    let make<'a> (init : 'a) : ICell<'a> = new Internal.Cell<'a>(init)

[<RequireQualifiedAccess>]
module EventSource =
    let inline make<'t>() : IEventSource<'t> = new Internal.EventSource<'t>()

[<RequireQualifiedAccess>]
module Signal =
    let fromObservable<'T> (init : 'T) (source:IObservable<'T>) : ISignal<'T> =
        let mutable value = init
        { new ISignal<'T> with
            member _.Value = value
            member _.Dispose() = ()
            member _.Subscribe( h : IObserver<'T> ) =
                let dispose = source.Subscribe( fun next ->
                    value <- next
                    h.OnNext next
                )
                h.OnNext value
                dispose
        }

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

