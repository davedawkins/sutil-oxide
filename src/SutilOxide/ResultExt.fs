module SutilOxide.ResultExt

[<RequireQualifiedAccess>]
module ResultExt =
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

[<AutoOpen>]
module ResultExtensions =

    type Result<'T,'Err> with
        member __.ToThrow() =
            match __ with Ok v -> v | Error e -> failwith (sprintf "%A" e)
        member __.ToUnit() : Result<unit, 'Err> =
            __ |> Result.map ignore
        member __.ToThrowU() : unit =
            match __ with Ok _ -> () | Error e -> failwith (sprintf "%A" e)

    let inline errorf format =
        Printf.ksprintf Result.Error format


[<AutoOpen>]
module OptionExtensions =

    type Option<'T> with
        member __.ToResult( e : string ) : Result<'T,string>   = match __ with Some x -> Ok x | None -> Error e


