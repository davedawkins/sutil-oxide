module SutilOxide.ResultExt

[<RequireQualifiedAccess>]
module Result =
    open Fable.Core

    let fromMapOption (map : 'a -> 'b) (msg:string) (o : Option<'a>) : Result<'b,string> =
        o |> Option.map (map>>Result.Ok) |> Option.defaultValue (Error msg)

    let fromOption (msg:string) (o : Option<'a>) : Result<'a,string> =
        fromMapOption id msg o

    let mapBoth (okFn: 'T -> 'U) (errorFn: 'E -> 'F) (result: Result<'T, 'E>) : Result<'U, 'F> =
        match result with
        | Ok x -> Ok (okFn x)
        | Error e -> Error (errorFn e)

    let sequence (xs: seq<Result<'T, 'E>>) : Result<'T list, 'E list> =
        let folder (accOk, accErr) = function
            | Ok t -> (t :: accOk, accErr)
            | Error e -> (accOk, e :: accErr)
        
        let (oks, errs) = Seq.fold folder ([], []) xs
        if List.isEmpty errs then Ok (List.rev oks) else Error (List.rev errs)

    let transpose (rp: Result<JS.Promise<'T>, 'E>) : JS.Promise<Result<'T, 'E>> =
        match rp with
        | Ok p -> p.``then``(fun t -> Ok t)
        | Error e -> Promise.lift (Error e)

    let ofOption (err: 'E) = function Some v -> Ok v | None -> Error err
    let toOption = function Ok v -> Some v | Error _ -> None

    /// Short-circuit traverse: first Error wins
    let traverse (f: 'a -> Result<'b, 'E>) (xs: seq<'a>) : Result<'b[] , 'E> =
        let mutable err = None
        let acc = ResizeArray<'b>()
        use e = xs.GetEnumerator()
        while err.IsNone && e.MoveNext() do
            match f e.Current with
            | Ok v    -> acc.Add v
            | Error x -> err <- Some x
        match err with
        | Some x -> Error x
        | None   -> Ok (Array.ofSeq acc)

    let sequenceShortCircuit xs = traverse id xs
    
    let ofPair (a : Result<'A,string>, b : Result<'B,string>) =
        match a, b with 
        | Ok a', Ok b' -> Ok (a',b') 
        | Error e1, Error e2 -> Error (e1 + "," + e2) 
        | Error e, _ | _, Error e -> Error e

    let toThrow (r : Result<'a,'b>) =
        r |> Result.mapError (fun err -> failwith (string err))
        
    let toIgnore (r : Result<'a,'b>) =
        r |> Result.map ignore

    let toIgnoreThrow (r : Result<'a,'b>) =
        r |> function Ok _ -> () | Error err -> failwith (string err)
        
    let inline errorf format =
        Printf.ksprintf Result.Error format

    let getValueOrThrow (result: Result<'T, 'E>) : 'T =
        match result with
        | Ok v -> v
        | Error e -> failwith (sprintf "%A" e)
        
[<AutoOpen>]
module ResultExtensions =

    type Result<'T,'Err> with
        member __.ToThrow() =
            __ |> Result.toThrow
        member __.ToUnit() : Result<unit, 'Err> =
            __ |> Result.toIgnore
        member __.ToThrowU() : unit =
            __ |> Result.toIgnoreThrow

    // let inline errorf format = Result.errorf format

[<AutoOpen>]
module OptionExtensions =

    type Option<'T> with
        member __.ToResult( e : string ) : Result<'T,string>   = match __ with Some x -> Ok x | None -> Error e


