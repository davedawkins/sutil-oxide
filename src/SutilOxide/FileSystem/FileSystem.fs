module SutilOxide.FileSystem

//
// Copyright (c) 2022 David Dawkins
//

open System
open Browser
open PromiseResult

module Internal =

    type Uid = int

    type FileEntryType =
        | File
        | Folder


    type FileEntry = {
        Type : FileEntryType
        Name : string
        Uid : Uid
        Content : string
        Children : (string * Uid)[]
    }

    type FileContent =
        | Text of string
        | Binary of byte[]

    type Root = {
        NextUid : int
    }

    let isRooted (p : string) = p.StartsWith "/"

    let collapseDotDot (path : string) =

        match path with
        | "" | "/" ->
            path
        | _ ->
            let items = path.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries ) |> List.ofArray

            let rec go (items,pitems) =
                match items,pitems with
                | [],_ -> 
                    [], pitems

                | x::xs, _ when x = "." ->
                    go (xs, pitems)

                | x::_, [] when x = ".." ->
                    failwith ("Invalid path: " + path)

                | x::xs, y::ys when x = ".." ->
                    go (xs, ys)

                | x::xs,_ -> 
                    go (xs, x :: pitems)

            go (items,[]) |> snd |> List.rev |> String.concat "/" 
            |> fun p -> if isRooted path then "/" + p else p

    let parsePath (path:string) =
        path.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries)

    let buildPath (parts : seq<string>) =
        "/" + String.Join( "/", parts )

    let canonical (path : string ) =
        path |> parsePath |> buildPath

    let getFolderName path =
        match path with
        | "/" -> "/"
        | _ ->
            let items = path |> parsePath
            if (items.Length = 0) then failwith ("Invalid path for getFolderName: " + path)
            if (items.Length = 1) then "" else
            items |> Array.take (items.Length - 1) |> buildPath

    let getFileNameWithExt path =
        let items = path |> parsePath
        if items.Length = 0 then "" else items |> Array.last

    let getFileNameNoExt path =
        let items = path |> parsePath
        let fname =
            if items.Length = 0 then "" else items |> Array.last
        let dot = fname.LastIndexOf '.'
        if dot < 0 then fname else fname.Substring(0,dot)

    let cleanSlash (f:string) =
        f.Replace("\\", "/").Replace("//", "/")

    let combine (path:string) file =
        sprintf "%s/%s" (path.TrimEnd([|'/'|])) file |> canonical

type IKeyedStorage =
    abstract Exists: string -> bool
    abstract Get: string -> string
    abstract Put: string * string -> unit
    abstract Remove: string -> unit

type IKeyedStorageAsync =
    abstract Exists: string -> Promise<bool>
    abstract Get: string -> Promise<string>
    abstract Put: string * string -> Promise<unit>
    abstract Remove: string -> Promise<unit>
    abstract BeginBatch: unit -> unit
    abstract CommitBatch: unit -> Promise<unit>
    abstract Close: unit -> Promise<unit>

module private BrowserStorage =
    let mk rootKey key = sprintf "%s/%s" rootKey key

    let exists rootKey key =
        (window.localStorage.getItem (mk rootKey key)) <> null

    let getContents rootKey key =
        window.localStorage.getItem( (mk rootKey key) )

    let setContents rootKey key content =
        window.localStorage.setItem( (mk rootKey key), content)

    let remove rootKey key =
        window.localStorage.removeItem (mk rootKey key)

type LocalStorage(rootKey : string) =
    interface IKeyedStorage with
        member __.Exists (key: string): bool = 
            BrowserStorage.exists rootKey key
        member __.Get (key: string): string = 
            BrowserStorage.getContents rootKey key
        member __.Put(key, content) = BrowserStorage.setContents rootKey key content
        member __.Remove (key: string): unit = 
            BrowserStorage.remove rootKey key

module KeyedStorageIndexedDB =
    open Fable.Core

    let [<Import("createKeyedStorageIndexedDB", "./KeyedStorageIndexedDB.js")>] createKeyedStorageIndexedDB ( rootKey : string ) : IKeyedStorageAsync = jsNative

open Fable.Core

// This is only really necessary because 'Unit cannot be given the type 'unit' in the type definitions
// below (related to the type parameter only being used in the return type)
// It's OK though, I quite like this kind of explicit tagging of the behaviour of the API

[<Erase>]
type SyncThrowable<'T> = 
    private
    | Throwable of ('T)
    with 
        member __.UnsafeValue = let (Throwable t) = __ in t
        static member Of (f : unit -> 'T) = Throwable (f())

type SyncResult<'T> = Result<'T,string>
type AsyncResult<'T> = PromiseResult<'T, string>
type AsyncPromise<'T> = Promise<'T>

[<RequireQualifiedAccess>]
module Path =
    open Internal
    let combine a b  = Internal.combine a b
    let getFolderName path = getFolderName path
    let getFileName path = getFileNameWithExt path
    let getFileNameWithExt path = getFileNameWithExt path
    let getFileNameNoExt path = getFileNameNoExt path
    let getExtension (path : string) =
        let fileName = getFileNameWithExt path
        let p = fileName.LastIndexOf('.')
        if p < 0 then "" else fileName.Substring(p)

type IReadOnlyFileSystemOf<'StringArray,'String,'Bool,'Unit,'Disposable> =
    abstract member Files : path : string -> 'StringArray
    abstract member Folders : path :string -> 'StringArray
    abstract member Exists : path : string -> 'Bool
    abstract member IsFile : path : string -> 'Bool
    abstract member IsFolder : path : string -> 'Bool
    abstract member GetFileContent : path : string  -> 'String
    abstract member OnChange : (string -> unit) -> 'Disposable

type IWriteOnlyFileSystemOf<'StringArray,'String,'Bool,'Unit> =
    abstract member SetFileContent : string * string -> 'Unit
    abstract member RemoveFile     : path : string -> 'Unit
    abstract member CreateFile     : string * string  -> 'Unit
    abstract member CreateFolder   : string -> 'Unit
    abstract member RenameFile     : string * string -> 'Unit

/// Synchronous read-only interface. Errors will be raised as exceptions
/// 
type IReadOnlyFileSystem = 
    inherit IReadOnlyFileSystemOf<SyncThrowable<string[]>, SyncThrowable<string>, SyncThrowable<bool>, SyncThrowable<unit>, SyncThrowable<IDisposable>>

/// Synchronous interface. Errors will be raised as exceptions
/// 
type IFileSystem = 
    inherit IWriteOnlyFileSystemOf<SyncThrowable<string[]>, SyncThrowable<string>, SyncThrowable<bool>, SyncThrowable<unit>>
    inherit IReadOnlyFileSystem

/// Promise-based interface where all results are expressed as AsyncResult<'T>
/// 
type IReadOnlyFileSystemAsyncR = 
    inherit IReadOnlyFileSystemOf<AsyncResult<string[]>, AsyncResult<string>, AsyncResult<bool>, AsyncResult<unit>, AsyncResult<IDisposable>>

/// Promise-based read-only interface where all results are expressed as AsyncResult<'T>
/// 
type IFileSystemAsyncR = 
    inherit IWriteOnlyFileSystemOf<AsyncResult<string[]>, AsyncResult<string>, AsyncResult<bool>, AsyncResult<unit>>
    inherit IReadOnlyFileSystemAsyncR

/// Promise-based read-only interface where all results are expressed as AsyncPromise<'T> (Promise<'T>)
/// 
type IReadOnlyFileSystemAsyncP = 
    inherit IReadOnlyFileSystemOf<AsyncPromise<string[]>, AsyncPromise<string>, AsyncPromise<bool>, AsyncPromise<unit>, AsyncPromise<IDisposable>>

/// Promise-based interface where all results are expressed as AsyncResult<'T> (Promise<'T>)
/// 
type IFileSystemAsyncP = 
    inherit IWriteOnlyFileSystemOf<AsyncPromise<string[]>, AsyncPromise<string>, AsyncPromise<bool>, AsyncPromise<unit>>
    inherit IReadOnlyFileSystemAsyncP

