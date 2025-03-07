module SutilOxide.Cell

open Fable.Core

[<Mangle>]
type IReadable<'T> =
    inherit System.IObservable<'T>
    abstract Name : string
    abstract Value : 'T with get
    abstract Subscribe: ('T -> unit) -> System.IDisposable

type IWriteable<'T> =
    abstract Set : 'T -> unit

type ICell<'T> =
    inherit IReadable<'T>
    inherit IWriteable<'T>


type Cell<'T>(name : string, init:'T) =
    let clients = Subscription.Subscription<'T>()
    let mutable value = init
    let _set v = 
        value <- v
        clients.Notify(value)

    member _.Subscribe(handler) = clients.Subscribe(handler)
    member _.Value with get() = value and set(v) = _set(v)
    member _.Name = name
    interface ICell<'T> with
        member _.Name = name
        member _.Value with get() = value 
        member _.Set(v) = _set(v)
        member __.Subscribe (handler) = __.Subscribe(handler)
    interface System.IObservable<'T> with
        member _.Subscribe( observer : System.IObserver<'T> ) =
            observer.OnNext(value)
            clients.Subscribe( fun v -> observer.OnNext(v) )