module SutilOxide.Logging

//
// Copyright (c) 2022 David Dawkins
//

let private jslog(s : string) =
    Fable.Core.JS.console.log(s)

let mutable private loggingHandler = ignore //jslog

let log(s : string) =
    //if (s.StartsWith("Error")) then
    //    Fable.Core.JS.console.log("break")
    //    Fable.Core.JS.debugger()
    loggingHandler s

let private prependHandler log =
    let currentHandler = loggingHandler
    loggingHandler <- fun s -> log s; currentHandler s

let appendHandler log =
    let currentHandler = loggingHandler
    loggingHandler <- fun s -> currentHandler s; log s

let private setHandler log = loggingHandler <- log