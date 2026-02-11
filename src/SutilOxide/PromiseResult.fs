module SutilOxide.PromiseResult

type Promise<'T> = Fable.Core.JS.Promise<'T>
type PromiseResult<'T,'E> = Promise<Result<'T, 'E>>

/// Convenience alias for `Promise<Result<'T, string>>`
type PsResult<'T> = PromiseResult<'T,string>

type PromiseResultBuilder() =
    /// 'return x'
    member _.Return(x: 'T) : PromiseResult<'T,_> =
        Promise.lift (Ok x)

    // /// 'return! x' for `Promise<Result<'T, 'E>>`
    // member _.ReturnFrom(p: Promise<Result<'T, 'E>>) : PromiseResult<'T, 'E> =
    //     p

    // 'return! x' for `Result<'T, 'E>` (lifts into a resolved Promise)
    member _.ReturnFrom(r: Result<'T, 'E>) : PromiseResult<'T, 'E> =
        Promise.lift r

    // 'return! x' for `Promise<'T>` (automatically wraps result in `Ok`)
    member _.ReturnFrom(p: Promise<'T>) : PromiseResult<'T, _> =
        p.``then``(fun t -> Ok t)

    /// 'let! v = expr'  and  'do! expr'
    member _.Bind
        (
            v: PromiseResult<'T,_>,
            f: 'T -> PromiseResult<'U,_>
        ) : PromiseResult<'U,_> =
        promise {
            let! result = v
            match result with
            | Ok v    -> return! f v
            | Error e -> return Error e
        }

    /// Needed for multi-statement blocks
    member _.Delay(generator: unit -> PromiseResult<'T,_>) : PromiseResult<'T,_> =
        // Typically just invoke the function
        generator()

    /// Provides a "neutral" value when you have an 'if' with no 'else',
    /// or a block that ends without a 'return'.
    member _.Zero() : PromiseResult<unit,_> =
        Promise.lift (Ok())

    /// Sequences two subcomputations if they return 'unit' and then 'T'.
    /// For example:
    ///    expr1
    ///    expr2
    /// is compiled as Combine(expr1, expr2).
    member _.Combine
        (
            first: PromiseResult<unit,_>,
            second: PromiseResult<'T,_>
        ) : PromiseResult<'T,_> =
        promise {
            let! r1 = first
            match r1 with
            | Ok ()   -> return! second
            | Error e -> return Error e
        }

    /// 'let! x = v' where `v: Result<Promise<T>, E>` (unwraps and sequences the promise)
    member _.Bind(rp: Result<Promise<'T>, 'E>, f: 'T -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
        match rp with
        | Ok p -> p |> Promise.bind f
        | Error e -> Promise.lift (Error e)

    // 'let! x = p' where `p: Promise<T>` (treats as `Ok (Promise<T>)`)
    // member _.Bind(p: Promise<'T>, f: 'T -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
    //     p |> Promise.bind(f)

    /// 'let! x = r' where `r: Result<T, E>` (directly binds to the function)
    member _.Bind(r: Result<'T, 'E>, f: 'T -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
        match r with
        | Ok t -> f t
        | Error e -> Promise.lift (Error e)

    // /// 'let! x = v' where `v: 'T` (automatically lifts into `Promise<Result<T, E>>`)
    // member _.Bind(v: 'T, f: 'T -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
    //     f v

    /// 'let! xs = seq<Promise<T>>' - Preserves input collection type, sequences Promises
    member _.Bind(ps: array<Promise<'T>>, f:array<'T> -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
        Promise.all ps
        |> Promise.bind f

    // /// 'let! xs = seq<Promise<Result<T, E>>>' - Preserves input collection type, sequences Promises and handles errors
    // member _.Bind(ps: array<Promise<Result<'T, 'E>>>, f: array<'T> -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
    //     let t1 = Promise.all ps |> Promise.bind f
    //     ()

    /// 'do! seq<Promise<unit>>' - Runs all promises but discards results
    member _.Bind(ps: array<Promise<unit>>, f: unit -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
        Promise.all ps |> Promise.map ignore |> Promise.bind f

    //// 'do! seq<Promise<Result<unit, E>>>' - Runs all promises, discards results but collects errors
    // member _.Bind(ps: #seq<Promise<Result<unit, 'E>>>, f: unit -> PromiseResult<'U, 'E>) : PromiseResult<'U, 'E> =
    //     Promise.all ps.Then(fun results ->
    //         let _, errs = Seq.partitionMap id results
    //         if Seq.isEmpty errs then f ()
    //         else Promise.lift (Error (Seq.toList errs))
    //     )

/// An instance of the builder
let promiseResult = PromiseResultBuilder()
let presult = promiseResult

[<RequireQualifiedAccess>]
module PromiseResult =

    let map (f : 'a -> 'b) (pr : PromiseResult<'a,string>) : PromiseResult<'b,string> =
        pr |> Promise.map (Result.map f)

    let liftBind (f : 'a -> PromiseResult<'b,string>) (r : Result<'a,string>) : PromiseResult<'b,string> =
        promise {
            return!
                match r with
                | Ok a -> f a
                | Error s -> Promise.lift (Error s)
        }

    let bind (f : 'a -> PromiseResult<'b,string>) (pr : PromiseResult<'a,string>) : PromiseResult<'b,string> =
        promise {
            let! pa = pr

            return!
                match pa with
                | Ok a -> f a
                | Error s -> Promise.lift (Error s)
        }

    let fromPromise<'T> ( p : Promise<'T> ) : PromiseResult<'T,string> =
        p |> Promise.map (Ok) |> Promise.catch (fun x -> Error x.Message)

    /// Lift a Promise into a PromiseResult
    let liftp<'T>( p : Promise<'T> ) : PromiseResult<'T,string> = fromPromise p

    let liftr<'T>( r : Result<'T,string> ) : PromiseResult<'T,string> = Promise.lift r

    /// Lift a value into a PromiseResult
    let lift<'T>( v : 'T ) : PromiseResult<'T,string> = v |> Promise.lift |> liftp

    /// Convert a PromiseResult<'T, string> into a Promise<'T>
    let toPromise<'T>( p : PromiseResult<'T,string> ) : Promise<'T> =
        p |> Promise.bind (function
            | Ok v -> Promise.lift v
            | Error e -> Promise.reject (System.Exception e))
        
    
    let Ok v = Result.Ok v |> Promise.lift
    let Error e = Result.Error e |> Promise.lift