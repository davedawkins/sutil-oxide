module SutilOxide.ResultCE

/// Convenience alias
type SResult<'T> = Result<'T, string>

type ResultBuilder() =
    /// 'return x'
    member _.Return(x: 'T) : Result<'T, 'E> = Ok x

    /// 'return! x' for Result
    member _.ReturnFrom(r: Result<'T, 'E>) : Result<'T, 'E> = r

    /// 'let! v = expr'  and  'do! expr'
    member _.Bind(r: Result<'T, 'E>, f: 'T -> Result<'U, 'E>) : Result<'U, 'E> =
        match r with
        | Ok v -> f v
        | Error e -> Error e

    /// Neutral value for if-without-else / blocks ending without 'return'
    member _.Zero() : Result<unit, 'E> = Ok ()

    /// Required for multi-statement blocks. Result is eager so we just invoke.
    member _.Delay(f: unit -> Result<'T, 'E>) : unit -> Result<'T, 'E> = f

    member _.Run(f: unit -> Result<'T, 'E>) : Result<'T, 'E> = f ()

    /// Sequences   expr1 \n expr2   where expr1 : Result<unit,_>
    member this.Combine(first: Result<unit, 'E>, second: unit -> Result<'T, 'E>) : Result<'T, 'E> =
        match first with
        | Ok ()   -> second ()
        | Error e -> Error e

    member _.TryWith(body: unit -> Result<'T, 'E>, handler: exn -> Result<'T, 'E>) : Result<'T, 'E> =
        try body () with e -> handler e

    member _.TryFinally(body: unit -> Result<'T, 'E>, compensation: unit -> unit) : Result<'T, 'E> =
        try body () finally compensation ()

    member this.Using(resource: #System.IDisposable, body: #System.IDisposable -> Result<'T, 'E>) : Result<'T, 'E> =
        this.TryFinally(
            (fun () -> body resource),
            (fun () -> if not (isNull (box resource)) then resource.Dispose())
        )

    member this.While(guard: unit -> bool, body: unit -> Result<unit, 'E>) : Result<unit, 'E> =
        if not (guard ()) then Ok ()
        else
            match body () with
            | Ok ()   -> this.While(guard, body)
            | Error e -> Error e

    member this.For(xs: seq<'T>, body: 'T -> Result<unit, 'E>) : Result<unit, 'E> =
        use e = xs.GetEnumerator()
        this.While((fun () -> e.MoveNext()), (fun () -> body e.Current))

let result = ResultBuilder()
