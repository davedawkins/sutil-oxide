module SutilOxide.JsHelpers

open Fable.Core

// [<Emit("$0 === $1")>]
// let fastEquals (x: 'T) (y: 'T): bool = jsNative

// type TimeoutFn = int -> (unit -> unit) -> unit

// let createTimeout() : TimeoutFn =
//     let mutable delayHandle = -1

//     fun (timeoutMs : int) (f : unit -> unit) ->
//         if delayHandle >= 0 then
//             Fable.Core.JS.clearTimeout delayHandle
//             delayHandle <- -1
//         delayHandle <- Sutil.DomHelpers._setTimeout f timeoutMs

// [<Emit("performance.now()")>]
// let performanceNow() : double = jsNative


[<Emit("$0 === $1")>]
let fastEquals (x: 'T) (y: 'T): bool = jsNative

[<Emit("Object.keys($0)")>]
let jsObjectKeys (x : obj) : string[] = jsNative

[<Emit("$1.hasOwnProperty($0)")>]
let jsExists( name : string, o : obj ) : bool = jsNative

[<Emit("$0[$1]")>]
let jsGet( data : obj, name : string ) = jsNative

[<Emit("delete $0[$1]")>]
let inline _delete( data : 'a, name : string ) : unit = jsNative

let inline jsDelete( data : 'a, name : string ) : 'a = 
    _delete (data, name)
    data

[<Emit("$0[$1] = $2")>]
let jsSet<'T>( data : obj, name : string, v : 'T ) : unit= jsNative

[<Emit("{ }")>]
let jsEmptyObject : obj= jsNative

[<Emit("typeof($0)")>]
let jsTypeOf(x : obj) : string = jsNative

[<Emit("Object.assign($0, $1)")>]
let jsObjectAssign(target : 'a, source : 'b) : 'c = jsNative

[<Emit("Object.assign({}, $0 )")>]
let jsCloneObject(data : 'a) : 'a = jsNative

[<Emit("Object.assign({}, $0 )")>]
let jsCloneObjectAs<'b>(data : obj) : 'b = jsNative

let jsGetDefault<'T> (data: obj, name : string, defaultValue:'T) : 'T =
    if jsExists(name,data) then
        jsGet(data, name)
    else
        defaultValue

let jsGetObj<'T> (data: obj, name : string, defaultValue:'T) : 'T =
    if jsExists(name,data) then
        let v = jsGet(data, name)
        if JsInterop.isNullOrUndefined v then defaultValue else v
    else
        defaultValue

let getProperty<'T> (data: obj) (name : string) (defaultValue:'T) : 'T =
    jsGetDefault(data, name, defaultValue)

let getObjProperty<'T> (data: obj) (name : string) (defaultValue:'T) : 'T =
    jsGetObj(data, name, defaultValue)

let stripTrailingSlash (name : string) =
    name.TrimEnd([| '\\'; '/' |])

let pathFileName (name : string) =
    let parts = (stripTrailingSlash name).Split( [| '/'; '\\' |])
    let fileWithExt = parts[ parts.Length-1 ]
    let lastDot = fileWithExt.LastIndexOf('.')
    if lastDot < 0 then fileWithExt else fileWithExt.Substring(0,lastDot)

let pathFileNameWithExt (name : string) =
    let parts = (stripTrailingSlash name).Split( [| '/'; '\\' |])
    parts[ parts.Length-1 ]

let pathFileExt (name : string) =
    let parts = (stripTrailingSlash name).Split( [| '/'; '\\' |])
    let fileWithExt = parts[ parts.Length-1 ]
    let lastDot = fileWithExt.LastIndexOf('.')
    if lastDot < 0 then "" else fileWithExt.Substring(lastDot)

let parseFloat (s : string) =
    match System.Double.TryParse(s) with 
    | true, n -> Some n
    | _ -> None

let objectToNameValues ( d : obj ) =
    jsObjectKeys d
    |> Array.map (fun k -> k, jsGet( d, k ) )

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
