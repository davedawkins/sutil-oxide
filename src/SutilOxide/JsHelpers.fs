module SutilOxide.JsHelpers

open Fable.Core

[<Emit("$0 === $1")>]
let fastEquals (x: 'T) (y: 'T): bool = jsNative

type TimeoutFn = int -> (unit -> unit) -> unit

let createTimeout() : TimeoutFn =
    let mutable delayHandle = -1

    fun (timeoutMs : int) (f : unit -> unit) ->
        if delayHandle >= 0 then
            Fable.Core.JS.clearTimeout delayHandle
            delayHandle <- -1
        delayHandle <- Sutil.DomHelpers._setTimeout f timeoutMs

[<Emit("performance.now()")>]
let performanceNow() : double = jsNative

