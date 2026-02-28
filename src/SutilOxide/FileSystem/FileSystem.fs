module SutilOxide.FileSystem

//
// Copyright (c) 2022 David Dawkins
//

open System
open Browser
open PromiseResult
open JsHelpers

[<AutoOpen>]
module Types =

    type EntryType =
        | File 
        | Folder 

    type EntryMetaData = {
        EntryType: EntryType
        CreatedAt: System.DateTime
        ModifiedAt: System.DateTime
    }
    with   
        static member Create(t : EntryType) = 
            let now = System.DateTime.UtcNow
            {
                EntryType = t
                CreatedAt = now
                ModifiedAt = now
            }

    type Entry = {
        Name : string
        Meta : EntryMetaData
    }
    with    member __.IsFile = __.Meta.EntryType = EntryType.File
            member __.IsFolder = __.Meta.EntryType = EntryType.Folder
    
    type Content =
        // | TextUtf8 of string
        | Bytes of ByteArray
        | Entries of Entry[]
        
module Internal =

    type Uid = int

    open Types

    type EntryContent =
        | ChildEntries of (string * Uid) []
        | TextBlob of string
        | ByteBlob of ByteArray 
    with
        member __.EntryType = match __ with ChildEntries _ -> Folder | _ -> File

    type EntryStorage = {
        Content : EntryContent
        Name : string
        Uid : Uid
        Meta : EntryMetaData
    }
    with 
        member __.Type = __.Content.EntryType
        member __.Children = 
            match __.Content with
            | ChildEntries ce -> ce 
            | _ -> Array.empty

        static member Create( entryContent) : EntryStorage = 
            {
                Content = entryContent
                Name = ""
                Uid = -1
                Meta = EntryMetaData.Create(match entryContent with ChildEntries _ -> Folder | _ -> File)
            }

    type EntryStorageDto = {
        Type : EntryType
        Name : string
        Uid : Uid
        Content : string option
        Children : (string * Uid)[]
        Meta : (string * string) [] option
    }
    with
        member __.ToEntryStorage( data : ByteArray option ) : EntryStorage = 
            let getKey key = __.Meta 
                            |> Option.bind (fun items -> items |> Array.tryFind (fun (k,_) -> k = key))
            {
                Content = 
                    match __.Type with
                    | Folder -> ChildEntries __.Children
                    | File ->
                        match data with
                        | Some bytes -> ByteBlob bytes 
                        | None -> TextBlob (__.Content |> Option.defaultValue "")
                Name = __.Name
                Uid = __.Uid
                Meta =
                    {
                        EntryType = __.Type
                        CreatedAt = 
                            getKey "CreatedAt"
                            |> Option.bind (fun (k,v) -> try System.DateTime.Parse v |> Some with x -> None)
                            |> Option.defaultValue (System.DateTime(2020,1,1,0,0,0,DateTimeKind.Utc))
                        ModifiedAt = 
                            getKey "ModifiedAt"
                            |> Option.bind (fun (k,v) -> try System.DateTime.Parse v |> Some with x -> None)
                            |> Option.defaultValue (System.DateTime(2020,1,1,0,0,0,DateTimeKind.Utc))
                    }
            }
        static member ToDto( fe : EntryStorage ) : EntryStorageDto = 
            {
                Type = fe.Type
                Name = fe.Name
                Uid = fe.Uid
                Content = None
                    // match fe.Content with
                    // | TextBlob s -> Some s
                    // | _ -> None
                Children = 
                    match fe.Content with
                    | ChildEntries entries -> entries
                    | _ -> [||]
                Meta = Some 
                        [| 
                            "CreatedAt", (fe.Meta.CreatedAt.ToString("o"))
                            "ModifiedAt", (fe.Meta.ModifiedAt.ToString("o"))
                        |]
            }

    // let fileEntryToJSON( fe : EntryStorage ) : string =
    //     EntryStorageDto.ToDto(fe) |> Thoth.Json.Encode.Auto.toString

    let fileEntryToByteArray( fe : EntryStorage ) : ByteArray =
        let dtoBytes = EntryStorageDto.ToDto(fe) |> Thoth.Json.Encode.Auto.toString |> ByteArray.textEncode
        match fe.Content with
        | ByteBlob data ->
            ByteArray.collectByteArrays [| dtoBytes; "\000" |> ByteArray.textEncode; data |]
        | TextBlob text ->
            ByteArray.collectByteArrays [| dtoBytes; "\000" |> ByteArray.textEncode; text |> ByteArray.textEncode |]
        | _ -> 
            dtoBytes

    let fileEntryFromJSON (js : string) (data : ByteArray option) =
        match Thoth.Json.Decode.Auto.fromString<EntryStorageDto>( js ) with
        | Ok dto -> 
            try
                Ok (dto.ToEntryStorage(data))
            with
            | x -> 
                Error( "Deserializing EntryStorageDto: " + dto.Name + ": " + x.Message + "\nJSON:" + js)                
        | Error msg ->
            Error msg

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
        String.Join( "/", parts )

    let buildPathRooted (parts : seq<string>) =
        "/" + buildPath(parts)

    let canonical (path : string ) =
        path |> parsePath |> buildPath

    let getFolderName (path : string) =
        match path.Trim() with
        | "/" | "" -> ""
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

    let combine (path:string) (file:string) =
        (path.TrimEnd([|'/'|]), file.TrimStart([|'/'|]))
        |> fun (p,f) ->
            if p = "" then f
            else sprintf "%s/%s" p f 
        |> canonical

open JsHelpers

// type IKeyedStorage =
//     abstract Exists: string -> bool
//     abstract Get: string -> string
//     abstract Put: string * string -> unit
//     abstract Remove: string -> unit

type IKeyedStorageAsync =
    abstract Exists: string -> Promise<bool>
    abstract Get: string -> Promise<obj>
    abstract GetAll: unit -> Promise<obj[]>
    abstract Put: string * ByteArray -> Promise<unit>
    abstract Remove: string -> Promise<unit>
    abstract BeginBatch: unit -> unit
    abstract CommitBatch: unit -> Promise<unit>
    abstract Close: unit -> Promise<unit>
    abstract CheckConsistency: unit -> Promise<obj>
    abstract LogConsistencyCheck: unit -> Promise<bool>
    abstract FixDanglingReferences: unit -> Promise<int>
    abstract FixOrphanedEntries: unit -> Promise<int>

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

// type LocalStorage(rootKey : string) =
//     interface IKeyedStorage with
//         member __.Exists (key: string): bool = 
//             BrowserStorage.exists rootKey key
//         member __.Get (key: string): string = 
//             BrowserStorage.getContents rootKey key
//         member __.Put(key, content) = BrowserStorage.setContents rootKey key content
//         member __.Remove (key: string): unit = 
//             BrowserStorage.remove rootKey key

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

type FsDateTime = System.DateTime

[<RequireQualifiedAccess>]
module Path =
    open Internal
    let combine a b  = Internal.combine a b
    let getFolderName path = getFolderName path
    let getFileName path = getFileNameWithExt path
    let getFileNameWithExt path = getFileNameWithExt path
    let getFileNameNoExt path = getFileNameNoExt path
    
    let getFirstFolder( path : string ) : string =
        Internal.parsePath path |> Array.head

    let stripTrailingSlash (path : string) = path.TrimEnd([| '/'; '\\' |] )
    let stripLeadingSlash (path : string) = path.TrimStart([| '/'; '\\' |] )

    let getRelativePath (parent : string) (path : string) =
        if path.StartsWith parent then path.Substring( 0, parent.Length ) |> stripTrailingSlash else path

    let getExtension (path : string) =
        let fileName = getFileNameWithExt path
        let p = fileName.LastIndexOf('.')
        if p < 0 then "" else fileName.Substring(p)

module PathOperators =
    let (/+) a b = Path.combine a b
    
type IReadOnlyFileSystemOf<'EntryOption,'ContentOption,'Disposable> =
    abstract member GetEntry : path :string -> 'EntryOption
    abstract member GetContent : path : string  -> 'ContentOption
    abstract member OnChanged : (string -> unit) -> 'Disposable

type IReadOnlyBatchingFileSystemOf<'EntryOption, 'ContentOption, 'Disposable> =
    inherit IReadOnlyFileSystemOf<AsyncPromise<'EntryOption>,AsyncPromise<'ContentOption>,AsyncPromise<'Disposable> >
    abstract member GetEntryBatch : path :string[] -> AsyncPromise<'EntryOption[]>
    abstract member GetContentBatch : path : string[]  -> AsyncPromise<'ContentOption[]>

type IWriteOnlyFileSystemOf<'Unit> =
    abstract member WriteEntry  : string * Content -> 'Unit
    abstract member RemoveEntry : path : string -> 'Unit
    abstract member RenameEntry : string * string -> 'Unit

type IReadOnlyFileSystem = 
    inherit IReadOnlyFileSystemOf<SyncThrowable<Entry option>, SyncThrowable<Content option>, SyncThrowable<IDisposable>>

type IFileSystem = 
    inherit IWriteOnlyFileSystemOf<SyncThrowable<unit>>
    inherit IReadOnlyFileSystem

type IReadOnlyFileSystemAsync= 
    inherit IReadOnlyFileSystemOf<AsyncPromise<Entry option>, AsyncPromise<Content option>, AsyncPromise<IDisposable>>

type IReadOnlyBatchingFileSystemAsync= 
    inherit IReadOnlyBatchingFileSystemOf<Entry option, Content option, IDisposable>

type IFileSystemAsync= 
    inherit IWriteOnlyFileSystemOf<AsyncPromise<unit>>
    inherit IReadOnlyBatchingFileSystemAsync

type IFileSystemAsync with
    member self.GetEntryBatchDefault (paths: string array): AsyncPromise<Entry option array> = 
        paths |> Array.map self.GetEntry |> Promise.all

    member self.GetContentBatchDefault (paths: string array): AsyncPromise<Content option array> = 
        paths |> Array.map self.GetContent |> Promise.all

[<AutoOpen>]
module FileSystemExt =

    type IFileSystemAsync with

        member __.Exists( path : string ) : AsyncPromise<bool> =
            promise {
                let! e = __.GetEntry(path)
                return e.IsSome
            }

        member __.IsFile( path : string ) : AsyncPromise<bool> =
            promise {
                let! e = __.GetEntry(path)
                return e |> Option.map (fun e -> e.Meta.EntryType = EntryType.File) |> Option.defaultValue false
            }

        member __.IsFolder( path : string ) : AsyncPromise<bool> =
            promise {
                let! e = __.GetEntry(path)
                return e |> Option.map (fun e -> e.Meta.EntryType = EntryType.Folder) |> Option.defaultValue false
            }

        member __.GetEntries( path : string ) : AsyncPromise<Entry[]> =
            let _path = path
            promise {
                let! e = __.GetEntry(path)
                match e with
                | Some entry when entry.Meta.EntryType = EntryType.Folder ->
                    let! c = __.GetContent(path)
                    match c with
                    | Some (Content.Entries entries) ->
                        return entries
                    | _ -> 
                        return failwithf "Internal error: Not a folder: %s" path
                | x ->
                    return failwithf "Not a folder: '%s' '%s' (%A)" _path path x
            }

        member private __.EntryNamesWhere( path : string, pred : Entry -> bool ) : AsyncPromise<string[]> =
            __.GetEntries path |> Promise.map(Array.filter pred>>Array.map _.Name)
            // let _path = path
            // promise {
            //     let! e = __.GetEntry(path)
            //     match e with
            //     | Some entry when entry.Meta.EntryType = EntryType.Folder ->
            //         let! c = __.GetContent(path)
            //         match c with
            //         | Some (Content.Entries entries) ->
            //             return entries |> Array.filter pred |> Array.map _.Name
            //         | _ -> 
            //             return failwithf "Internal error: Not a folder: %s" path
            //     | x ->
            //         return failwithf "Not a folder: '%s' '%s' (%A)" _path path x
            // }

        member __.EntryNames( path : string ) : AsyncPromise<string[]> =
            promise {
               let! names = __.EntryNamesWhere(path, fun _ -> true )
               return names
            }

        member __.TryFiles( path : string ) : AsyncPromise<string[]> =
            promise {
                let! isFolder = __.IsFolder path
                if isFolder then
                    try
                        return! __.EntryNamesWhere(path, _.IsFile )
                    with
                    | x -> 
                        console.error( "TryFiles: " + path, x.Message )
                        return [||]
                else
                    return [||]
            }

        member __.TryFolders( path : string ) : AsyncPromise<string[]> =
            promise {
                let! isFolder = __.IsFolder path
                if isFolder then
                    try
                        return! __.EntryNamesWhere(path, _.IsFolder )
                    with
                    | x -> 
                        console.error( "TryFolders: " + path, x.Message )
                        return [||]
                else
                    return [||]
            }

        member __.Files( path : string ) : AsyncPromise<string[]> =
            __.EntryNamesWhere(path, _.IsFile )

        member __.Folders( path : string ) : AsyncPromise<string[]> =
            __.EntryNamesWhere(path, _.IsFolder )

        member __.TryGetFileText( path : string ) =
            promise {
                let! c = __.GetContent( path )
                match c with 
                | Some (Content.Bytes data) -> return Ok(data |> JsHelpers.ByteArray.textDecode)
                | None -> return Error (sprintf "File not found: %s" path)
                | _ -> return Error (sprintf "Not a file: %s" path)
            }

        member __.GetFileText(path : string) =
            promise {
                let! c = __.GetContent( path )
                match c with 
                | Some (Content.Bytes data) -> return data |> JsHelpers.ByteArray.textDecode
                | None -> return failwithf "File not found: %s" path
                | _ -> return failwithf "Not a file: %s" path
            }

        member __.GetFileBytes(path : string) =
            promise {
                let! c = __.GetContent( path )
                match c with 
                // | Some (Content.TextUtf8 text) -> return text |> ByteArray.textEncode
                | Some (Content.Bytes data) -> return data 
                | None -> return failwithf "File not found: %s" path
                | _ -> return failwithf "Not a file: %s" path
            }

        member __.GetFileContent(path : string) =
            __.GetFileText path

        // member __.TryGetFileBytes(path : string) =
        //     promise {
        //         let! f = __.IsFile(path)
        //         if f then 
        //             let! data = __.GetFileBytes path
        //             return Some data
        //         else
        //             return None
        //     }

        // member __.TryGetFileText(path : string) =
        //     promise {
        //         let! f = __.IsFile(path)
        //         if f then 
        //             let! text = __.GetFileText path
        //             return Some text
        //         else
        //             return None
        //     }

        member __.GetCreatedAt(path : string) =
            promise {
                let! e = __.GetEntry(path)
                return e |> Option.map (fun e -> e.Meta.CreatedAt) |> Option.defaultWith (fun _ -> failwithf "Not a file: %s" path)
            }

        member __.GetModifiedAt(path : string) =
            promise {
                let! e = __.GetEntry(path)
                return e |> Option.map (fun e -> e.Meta.ModifiedAt) |> Option.defaultWith (fun _ -> failwithf "Not a file: %s" path)
            }

        member __.SetFileContent(path : string, content : string) = 
            __.WriteEntry( path, Content.Bytes (content |> ByteArray.textEncode))

        member __.CreateFolder(path : string ) = 
            __.WriteEntry( path, Content.Entries [||] )

        member __.RenameFile(path : string, newPath : string) = 
            __.RenameEntry( path, newPath )

        member __.RemoveFile(path : string) = 
            __.RemoveEntry(path)

        member __.GetFileContentBatch( paths : string[] ) =
            paths |> Array.map __.GetFileContent |> Promise.all

type IFsAsync = 
    inherit IFileSystemAsync