module SutilOxide.Log

open Fable.Core
open SutilOxide.Reactive

[<Emit("window.console.log")>]
let _console_log : obj = jsNative

type ILog =
    abstract group: message:string -> unit
    abstract groupEnd: unit -> unit
    abstract log: ?message: obj * [<System.ParamArray>] optionalParams: obj[] -> unit
    abstract trace: ?message: obj * [<System.ParamArray>] optionalParams: obj[] -> unit
    abstract error: ?message: obj * [<System.ParamArray>] optionalParams: obj[] -> unit
    abstract warning: ?message: obj * [<System.ParamArray>] optionalParams: obj[] -> unit
    abstract info: ?message: obj * [<System.ParamArray>] optionalParams: obj[] -> unit
    abstract console: ?message: obj * [<System.ParamArray>] optionalParams: obj[] -> unit

let Console = 
    {|
        log = _console_log
    |} :> obj :?> ILog

let mutable private sources : Map<string,bool> = 
    [
        "Dom", true
    ] |> Map

let mutable private categories : Map<string,bool> = 
    [
        "Trace", true
    ] |> Map

let sourceIsEnabled src = not (sources.ContainsKey src) || sources[src]
let categoryIsEnabled src = not (categories.ContainsKey src) || categories[src]

type LogCategory =
    | Info 
    | Debug 
    | Error 
    | Warning
    | Trace
    | Custom of string
    with    
        override __.ToString() =
            match __ with
            | Info -> "Info"
            | Debug -> "Debug"
            | Error -> "Error"
            | Trace -> "Trace"
            | Warning -> "Warning"
            | Custom c -> c


let mutable private  _idCounter = 0
let private nextId() =
    let id = _idCounter + 1
    _idCounter <- id
    id

let mutable private groupStack : string list = [] 
let mutable private groupCount = 0

let private startGroup( label : string ) =
    groupStack <- label :: groupStack
    groupCount <- groupCount + 1
    Fable.Core.JS.console.group( label )

let private endGroup() =
    match groupStack with
    | x :: xs ->
        Fable.Core.JS.console.groupEnd()
        groupStack <- xs
        groupCount <- groupCount - 1
    | _ -> 
        Fable.Core.JS.console.error("Log: group stack underflow")

type LogMessage = 
    {
        Id : int
        Time: System.DateTime
        ModelTime : int
        Source : string
        Category : string
        Message : string
        Context : obj option
    }
    static member Create(msg : string) = { Source = ""; Id = nextId(); ModelTime = 0; Category = "-"; Message = msg; Context = None; Time = System.DateTime.Now }
    static member Create(cat : string, msg : string) = { Source = ""; Id = nextId(); ModelTime = 0; Category = cat; Message = msg; Context = None; Time = System.DateTime.Now }
    static member Create(src : string, cat : string, msg : string, ctx : obj) = 
        { 
            Id = nextId()
            Source = src
            ModelTime = 0
            Category = cat
            Message = msg
            Context = if ctx <> null then Some ctx else None
            Time = System.DateTime.Now
        }

let mutable private logListeners : IEventSource<LogMessage> = EventSource.make()

let logMessages : Reactive.ICell<ResizeArray<LogMessage>> = Reactive.Cell.make(ResizeArray())

let [<Import("trapConsoleLog", "./loghelper.js")>] trapConsoleLog (x : obj array -> unit) : unit = jsNative

let onLogClear : IEventSource<unit> = EventSource.make()
let onLogMessage : System.IObservable<LogMessage> = logListeners
let messages : System.IObservable<ResizeArray<LogMessage>> = logMessages

let clear() = 
    logMessages.Set(ResizeArray())
    onLogClear.Notify( () )

let enableSource (source : string) (enabled : bool) =
    sources <- sources.Add(source, enabled)

let enableCategory (cat : string) (enabled : bool) =
    categories <- categories.Add(cat, enabled)

let logmessage (m : LogMessage) =
    if categoryIsEnabled m.Category && sourceIsEnabled m.Source then
        // if (m.Message.StartsWith("Error") || m.Category = "Error") then
        //     Fable.Core.JS.debugger()

        logMessages.Value.Add(m)
        logMessages.Set( logMessages.Value )
        logListeners.Notify(m)
        Console.log(m.Category + ":" + m.Source + ":" + m.Message)

//        Console.log( m.Source, m.Category, m.Message)

let logm (src : string) (cat : string) (msg : string) (ctx : obj) =
    LogMessage.Create(src, cat, msg, ctx) |> logmessage

let log(s : string) =
    LogMessage.Create(s) |> logmessage
    
let logc (cat : string) (s : string) =
    LogMessage.Create(cat,s) |> logmessage

let listen ( cb : LogMessage -> unit ) =
    logListeners.Subscribe(cb)

let inline private  fmt msg args =
    ( args |> Array.append [| msg :> obj |] |> Array.map string |> String.concat " " )

let createWith logm (source : string) =
    {
        new ILog with
            member _.group(label : string) = 
                startGroup( source + ": " + label)
            member _.groupEnd() = endGroup()
            member _.log( arg0, argsN ) =
                logm source "Trace" (fmt arg0 argsN) null
            member _.trace( arg0, argsN ) =
                logm source "Trace" (fmt arg0 argsN) null
            member _.error( arg0, argsN ) =
                logm source "Error" (fmt arg0 argsN) null
            member _.warning( arg0, argsN ) =
                logm source "Warning" (fmt arg0 argsN) null
            member _.info( arg0, argsN ) =
                logm source "Info" (fmt arg0 argsN) null
            member _.console( arg0, argsN ) =
                Fable.Core.JS.console.log( source, argsN |> Array.append [| arg0 :> obj |] )
    }
    
let create (source : string) =
    //enableSource source true
    createWith (fun src cat msg ctx -> if sourceIsEnabled src then logm source cat msg ctx) source

let getEnabled() =
    sources |> Map.toList

let Trace = create "App"

let init() = 
    ()