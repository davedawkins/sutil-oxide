module SutilOxide.Subscription

#if FABLE_COMPILER
open Fable.Core
#endif

open System.Collections.Generic

// module Subscription =
//     type Client<'T> = 'T -> unit

// #if FABLE_COMPILER
// [<Mangle>]
// #endif

// type ISubscriptionWithResult<'T,'R> =
//     abstract Subscribe : ('T -> 'R) -> System.IDisposable
//     // abstract Count : int
//     inherit System.IDisposable

// type ISubscription<'T> = 
//     inherit ISubscriptionWithResult<'T,unit>
//     inherit System.IObservable<'T>

// type ISignal<'T> =
//     inherit ISubscription<'T>
//     abstract Value : 'T with get

// type ISubscriptionAsync<'T> = 
//     inherit ISubscriptionWithResult<'T,JS.Promise<unit>>

// [<AutoOpen>]
// module SubscriptionEx =
//     type ISubscription<'T> with

//         member self.Until( pred : ('T -> bool), f : ('T -> unit) ) =
//             let mutable sub : System.IDisposable option = None

//             let cleanup (s : System.IDisposable) =
//                 s.Dispose()
//                 sub <- None

//             sub <- (self.Subscribe( fun v ->
//                 sub |> Option.iter (fun s ->
//                     if pred v then
//                         f v
//                         cleanup s)
//             )) |> Some

//             (fun () -> sub |> Option.iter cleanup)

//         member self.Once( oneTime : ('T -> unit) ) =
//             self.Until( (fun _ -> true), oneTime )

// module ObservableExt =
//     open System

//     /// Turn an IObservable into a stream that always pushes the current value
//     /// to new subscribers upon subscribe. 
//     let initialize<'T> (init : 'T) (source:IObservable<'T>) : IObservable<'T> =
//         let mutable value = init

//         { new System.IObservable<'T> with
//             member _.Subscribe( h : IObserver<'T> ) =
//                 let dispose = source.Subscribe( fun next ->
//                     value <- next
//                     h.OnNext next
//                 )
//                 h.OnNext value
//                 dispose
//         }

// type SubscriptionWithResult<'T,'R>() =
//     let mutable nextId = 0
//     let clients = Dictionary<int, 'T -> 'R>()

//     let getNextId() =
//         let id = nextId
//         nextId <- nextId + 1
//         id

//     // Subscribe a function that takes 'T and returns 'R
//     abstract Subscribe: ('T -> 'R) -> System.IDisposable
//     default this.Subscribe(client : 'T -> 'R) : System.IDisposable =
//         let clientId = getNextId()
//         clients.[clientId] <- client
//         { new System.IDisposable with
//             member _.Dispose() =
//                 clients.Remove(clientId) |> ignore }

//     abstract Notify: 'T -> unit
//     default this.Notify(value : 'T) : unit =
//         for client in clients.Values do
//             client value |> ignore

//     member this.NotifyAndCollect(value : 'T) : 'R[] =
//         [|
//             for client in clients.Values do
//                 yield client value
//         |]

//     // member this.Count = clients.Count

//     interface ISubscriptionWithResult<'T,'R> with
//         member this.Subscribe(client) = this.Subscribe client
//         // member this.Count = clients.Count
//         member this.Dispose() = clients.Clear()

// type Subscription<'T>() =
//     inherit SubscriptionWithResult<'T,unit>()

//     interface System.IObservable<'T> with 
//         member this.Subscribe( ob ) = this.Subscribe( fun v -> ob.OnNext(v) )

//     interface ISubscription<'T>
    
// type Cell<'T>( init : 'T ) =
//     inherit SubscriptionWithResult<'T,unit>()

//     let mutable _value : 'T = init

//     override _.Subscribe(client: 'T -> unit) : System.IDisposable =
//         let r = base.Subscribe(client)
//         client(_value)
//         r

//     member __.Value with get() = _value and set(v) = __.Notify(v)
    
//     override _.Notify(value: 'T) =
//         _value <- value
//         base.Notify(value)

//     interface ISignal<'T> with
//         member this.Value = _value

//     interface System.IObservable<'T> with 
//         member this.Subscribe( ob ) = this.Subscribe( fun v -> ob.OnNext(v) )

// type PromiseSubscription<'T>() =
//     inherit SubscriptionWithResult<'T,JS.Promise<unit>>()

//     /// Notifies all subscribers *asynchronously*. 
//     /// Returns a Promise that resolves only after *all* callbacks complete.
//     member this.NotifyAsync(value: 'T) : JS.Promise<unit> =
//         let allPromises = base.NotifyAndCollect(value)
//         // Turn seq of promises into a single promise that completes once all are done
//         promise {
//             let! _ = allPromises |> Promise.all
//             // all finished, so we can just return
//             return ()
//         }

//     interface ISubscriptionAsync<'T> 