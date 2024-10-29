module SutilOxide.FileSystem

//
// Copyright (c) 2022 David Dawkins
//

open System
//open Fable.SimpleJson
open Browser

type Uid = int
type FileEntryType =
    | File
    | Folder

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

let private parsePath (path:string) =
    path.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries)

let private buildPath (parts : seq<string>) =
    "/" + String.Join( "/", parts )

let private canonical (path : string ) =
    path |> parsePath |> buildPath

let getFolderName path =
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

let private combine (path:string) file =
    sprintf "%s/%s" (path.TrimEnd([|'/'|])) file |> canonical

type IKeyedStorage =
    abstract Exists: string -> bool
    abstract Get: string -> string
    abstract Put: string * string -> unit
    abstract Remove: string -> unit

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

// TODO
type TODO_IndexedDbStorage(rootKey : string) =
    let mutable db : Browser.Types.IDBDatabase = Unchecked.defaultof<_>
    let mutable store : Browser.Types.IDBObjectStore = Unchecked.defaultof<_>

    let init() =
        let openRequest = Browser.IndexedDB.indexedDB.``open``(rootKey, 1)
        openRequest.onsuccess <- fun ev ->
            db <- openRequest.result :> obj :?> Browser.Types.IDBDatabase
            ()
        openRequest.onerror <- fun ev ->
            ()
        openRequest.onupgradeneeded <- fun ev ->
            ()

    do
        init()

    interface IKeyedStorage with
        member __.Exists (key: string): bool = 
            BrowserStorage.exists rootKey key
        member __.Get (key: string): string = 
            BrowserStorage.getContents rootKey key
        member __.Put(key, content) = BrowserStorage.setContents rootKey key content
        member __.Remove (key: string): unit = 
            BrowserStorage.remove rootKey key

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

type Promise<'T> = Fable.Core.JS.Promise<'T>
type AsyncResult<'T> = Promise<Result<'T, string>>

module internal ResultHelpers =
    let inline mkResult (value : unit -> 't) : Result<'t,string> =
        try
            value() |> Ok
        with
        | x -> x.Message |> Error

    let inline mkAsyncResult (value : unit -> 't) : AsyncResult<'t> =
        value |> mkResult |> Promise.lift
        
    let inline mkAsyncResultP (value : unit -> Promise<'t>) : AsyncResult<'t> =
        value() 
        |> Promise.result
        |> Promise.mapResultError (fun x -> x.Message)

open ResultHelpers

type IReadOnlyFileSystem =
    abstract member Files : path : string -> string[]
    abstract member Folders : path :string -> string[]
    abstract member Exists : path : string -> bool
    abstract member IsFile : path : string -> bool
    abstract member IsFolder : path : string -> bool
    abstract member GetFileContent : path : string  -> string
    abstract member OnChange : (string -> unit) -> unit

type IReadOnlyFileSystemAsync =
    abstract member Files : path : string -> AsyncResult<string[]>
    abstract member Folders : path :string -> AsyncResult<string[]>
    abstract member Exists : path : string -> AsyncResult<bool>
    abstract member IsFile : path : string -> AsyncResult<bool>
    abstract member IsFolder : path : string -> AsyncResult<bool>
    abstract member GetFileContent : path : string  -> AsyncResult<string>
    abstract member OnChange : (string -> unit) -> AsyncResult<unit>

type IFileSystem =
    inherit IReadOnlyFileSystem
    abstract member SetFileContent : string * string -> unit
    abstract member RemoveFile     : path : string -> unit
    abstract member CreateFile     : string * string  -> unit
    abstract member CreateFolder   : string -> unit
    abstract member RenameFile     : string * string -> unit

type IFileSystemAsync =
    inherit IReadOnlyFileSystemAsync
    abstract member SetFileContent : string * string -> AsyncResult<unit>
    abstract member RemoveFile     : path : string -> AsyncResult<unit>
    abstract member CreateFile     : string * string  -> AsyncResult<unit>
    abstract member CreateFolder   : string -> AsyncResult<unit>
    abstract member RenameFile     : string * string -> AsyncResult<unit>

let internal mkAsync( fs : IFileSystem ) : IFileSystemAsync =
    { new IFileSystemAsync with
          member _.CreateFile(arg1: string, arg2: string): AsyncResult<unit> = 
            mkAsyncResult (fun () -> fs.CreateFile(arg1, arg2))

          member _.CreateFolder(arg1: string): AsyncResult<unit> = 
            mkAsyncResult (fun () -> fs.CreateFolder(arg1))

          member _.Exists(path: string): AsyncResult<bool> = 
            mkAsyncResult (fun () -> fs.Exists(path))

          member _.Files(path: string): AsyncResult<string array> = 
            mkAsyncResult (fun () -> fs.Files(path))

          member _.Folders(path: string): AsyncResult<string array> = 
            mkAsyncResult (fun () -> fs.Folders(path))

          member _.GetFileContent(path: string): AsyncResult<string> = 
            mkAsyncResult (fun () -> fs.GetFileContent(path))

          member _.IsFile(path: string): AsyncResult<bool> = 
            mkAsyncResult (fun () -> fs.IsFile(path))

          member _.IsFolder(path: string): AsyncResult<bool> = 
            mkAsyncResult (fun () -> fs.IsFolder(path))

          member _.OnChange(arg1: string -> unit): AsyncResult<unit> = 
            mkAsyncResult (fun () -> fs.OnChange(arg1))

          member _.RemoveFile(path: string): AsyncResult<unit> = 
            mkAsyncResult (fun () -> fs.RemoveFile(path))

          member _.RenameFile(arg1: string, arg2: string): AsyncResult<unit> = 
            mkAsyncResult (fun () -> fs.RenameFile(arg1, arg2))

          member _.SetFileContent(arg1: string, arg2: string): AsyncResult<unit> = 
            mkAsyncResult (fun () -> fs.SetFileContent(arg1, arg2))
    }

[<AutoOpen>]
module IFileSystemExt =
    type IFileSystem with
        static member Combine( a, b ) = combine a b |> canonical
        static member GetFolderName path = getFolderName path
        static member GetFileName path = getFileNameWithExt path
        static member GetFileNameNoExt path = getFileNameNoExt path
        static member GetExtension (path : string) =
            let fileName = getFileNameWithExt path
            let p = fileName.LastIndexOf('.')
            if p < 0 then "" else fileName.Substring(p)

type KeyedStorageFileSystem( keyStorage : IKeyedStorage ) =
    //let mutable root : FileEntry = { Type = Folder; Name = "/"; Uid = 0; Content = ""; Children = Array.empty }
    let mutable root = { NextUid = 1 }

    let mutable onChange : (string -> unit) list = []

    let notifyOnChange (path : string) = onChange |> List.iter (fun h -> h path)

    let uidKey uid = sprintf "uid:%d" uid

    let delEntry (e : FileEntry) =
        keyStorage.Remove (uidKey e.Uid)

    let putEntry (e : FileEntry) =
        keyStorage.Put( (uidKey e.Uid), (Thoth.Json.Encode.Auto.toString(e)) )

    let getEntry uid =
        match Thoth.Json.Decode.Auto.fromString<FileEntry>( keyStorage.Get (uidKey uid) ) with
        | Ok r -> Some r
        | Error msg ->
            Fable.Core.JS.console.log(sprintf "Error: getEntry %A: %A" uid msg)
            None

    let entryExists uid =
        keyStorage.Exists uid

    let nameOf (e:FileEntry) = e.Name

    let getEntries uid =
        let result = 
            match getEntry uid with
            | None -> failwith ("Non-existent UID " + string uid)
            | Some e when e.Type <> FileEntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
            | Some e -> e.Children |> Array.map (snd>>getEntry) |> Array.choose id
        result

    let hasEntries uid =
        let result = 
            match getEntry uid with
            | None -> failwith ("Non-existent UID " + string uid)
            | Some e when e.Type <> FileEntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
            | Some e -> e.Children.Length > 0
        result

    let entryName uid =
        getEntry uid |> Option.map (fun e -> e.Name)

    let entryNameWithDefault defaultName uid =
        getEntry uid |> Option.map (fun e -> e.Name) |> Option.defaultValue defaultName

    let entryChildren uid =
        getEntry uid
            |> Option.map (fun e -> e.Children)

    let entryChildNames uid =
        entryChildren uid |> Option.map (Array.map fst)

    let entryChildUids uid =
        entryChildren uid |> Option.map (Array.map snd)


//        getEntry uid
//            |> Option.map (fun e -> e.Children |> Array.map entryName |> Array.choose id)

    let rec uidOf path =
        let parts = path |> parsePath

        let rec findUid (parent : Uid) (parts : string[]) i : Uid option=
            match i with
            | n when n >= parts.Length -> Some parent
            | _ ->
                match getEntry parent with
                | None -> failwith ("No entry found for part of path " + path)
                | Some e ->
                    match e.Children |> Array.tryFind (fun (name,_) -> name = parts[i]) with
                    | None -> None
                    | Some (_,uid) -> findUid uid parts (i+1)

        let result = findUid 0 parts 0
        //Fable.Core.JS.console.log("Path UID", path, result)
        result 

    let getEntryByPath path = path |> uidOf |> Option.bind getEntry

    let isEntry (path:string) =
        match path |> getEntryByPath with
        | Some _ -> true
        | _ -> false

    let isFile (path:string) =
        match getEntryByPath path with
        | Some e when e.Type = File ->  true
        | _ -> false

    let isFolder (path:string) =
        match getEntryByPath path with
        | Some e when e.Type = Folder ->  true
        | _ -> false

    let makeKey (path:string) =
        "fs:" + path

    //let saveRoot() =
    //    putEntry root
        //Storage.setContents "(root)" (Json.serialize(root))

    let validateFileName (file:string) =
        if file.Contains("..") || file.Contains("/") || file.Contains("\\") then
            failwith ("Invalid file name: " + file)

    let hasEntries (path : string) =
        path
        |> canonical
        |> uidOf
        |> Option.map hasEntries
        |> Option.defaultValue false

    let getEntriesWhere (filter: FileEntry -> bool) (path : string) =
        path
        |> canonical
        |> uidOf
        |> Option.map (fun id -> getEntries id
                                |> Array.filter filter
                                |> Array.map (fun f -> f.Name))
        |> Option.defaultValue Array.empty

    let putRoot() =
        keyStorage.Put( "(root)", (Thoth.Json.Encode.Auto.toString root) )

    let initRoot() =

        if not (keyStorage.Exists(uidKey 0)) then
            { Type = Folder; Name = "/"; Uid = 0; Content = ""; Children = Array.empty }
            |> putEntry

        keyStorage.Get("(root)")
            |> function
            | s when s <> null -> 
                match Thoth.Json.Decode.Auto.fromString<Root>(s) with
                | Ok r -> root <- r
                | Error msg ->
                    Fable.Core.JS.console.log("Root entry corrupted: " + msg)
            |_ -> ()

        putRoot()

    let newUid() =
        let uid = root.NextUid
        root <- { root with NextUid = uid + 1 }
        putRoot()
        uid

    let createFile path name notify=
        validateFileName name
        let cpath = path |> canonical
        let fname = combine cpath name

        if isEntry fname then
            failwith ("File already exists: " + fname)

        if not (isFolder cpath) then
            failwith ("Not a folder: " + cpath)

        cpath
        |> getEntryByPath
        |> Option.map (fun entry ->
            let uid = newUid()
            { entry with Children = entry.Children |> Array.append [| name, uid |] } |> putEntry
            {
                Type = File
                Content = ""
                Children = Array.empty
                Uid = uid
                Name = name
            } |> putEntry
        )
        |> Option.defaultWith (fun _ ->
            failwith "Parent folder does not exist"
        )

        if notify then
            SutilOxide.Logging.log( "CreateFile " + path + " " + name )
            notifyOnChange fname

    let createFolder folderPath notify=
        let name = getFileNameWithExt folderPath
        let parent = getFolderName folderPath

        validateFileName name
        let cpath = parent |> canonical
        let fname = combine cpath name

        if isEntry fname then
            failwith ("File already exists: " + fname)

        if not (isFolder cpath) then
            failwith ("Not a folder: " + cpath)

        cpath
        |> getEntryByPath
        |> Option.map (fun entry ->
            let uid = newUid()
            { entry with Children = entry.Children |> Array.append [| name, uid |] } |> putEntry
            {
                Type = Folder
                Content = ""
                Children = Array.empty
                Uid = uid
                Name = name
            } |> putEntry
        )
        |> Option.defaultWith (fun _ ->
            failwith "Parent folder does not exist"
        )

        if notify then
            SutilOxide.Logging.log( "CreateFolder " + fname + " " + name )
            notifyOnChange fname            
    do
        initRoot()
with
    interface IFileSystem with

        member _.OnChange (cb : string -> unit) = onChange <- onChange @ [cb]
        member _.Files (path : string) = getEntriesWhere (fun e -> e.Type = File) path
        member _.Folders (path :string) = getEntriesWhere (fun e -> e.Type = Folder) path

        member __.Exists(path : string) = isEntry(path)
        member __.IsFile(path : string) = isFile(path)
        member __.IsFolder(path : string) = isFolder(path)

        member _.GetFileContent( path : string ) =
            let cpath = path |> canonical

            if not (isFile cpath) then
                failwith ("Not a file: " + cpath)

            getEntryByPath cpath |> Option.map (fun e -> e.Content) |> Option.defaultValue ""

        member __.SetFileContent( path : string, content : string ) =
            let cpath = path |> canonical

            if (isFolder cpath) then
                failwith ("Not a file: " + cpath)

            if not (isFile cpath) then
                createFile (cpath |> getFolderName) (cpath |> getFileNameWithExt) false

            getEntryByPath cpath
            |> Option.iter (fun e -> { e with Content = content } |> putEntry)

            SutilOxide.Logging.log( "SetFileContent " + path )
            notifyOnChange path

        member __.RemoveFile( path : string ) =
            if isFolder path && hasEntries path then
                failwith ("Folder is not empty")

            getEntryByPath path |>
            Option.map (fun entry ->
                let folderName = getFolderName path
                let fileName = getFileNameWithExt path

                folderName |> getEntryByPath
                |> Option.iter (fun parentEntry ->
                    parentEntry.Uid
                    |> entryChildren
                    |> Option.map (Array.filter (fun (name,uid) -> name <> fileName))
                    |> Option.iter (fun entries ->
                        entry |> delEntry
                        { parentEntry with Children = entries } |> putEntry
                    )
                )
            )
            |> Option.defaultWith (fun _ ->
                failwith (sprintf "Cannot remove non-existent file '%s'" path)
            )

            SutilOxide.Logging.log( "RemoveFile " + path )
            notifyOnChange path

        member _.CreateFolder( path : string ) =
            createFolder path true

        member __.CreateFile( path : string, name : string ) =
            createFile path name true

        member __.RenameFile( path : string, newNameOrPath : string ) =
            Fable.Core.JS.console.log("RenameFile", path, newNameOrPath)

            let cpath = path |> canonical

            let npath =
                if newNameOrPath.StartsWith("/") then
                    newNameOrPath |> canonical
                else
                    validateFileName newNameOrPath
                    combine (getFolderName cpath) newNameOrPath

            if cpath = "/" || npath = "/" then
                failwith "Cannot rename to/from '/'"

            if not (isEntry cpath) then
                failwith ("Cannot rename non-existent file: " + cpath)

            if (isEntry npath) then
                failwith ("Cannot rename to existing file " + npath)

            let cparent = getFolderName cpath
            let nparent = getFolderName npath
            let cname = getFileNameWithExt cpath
            let nname = getFileNameWithExt npath

            if not (isEntry nparent) then
                failwith ("Parent folder for rename target does not exist: " + nparent)

            //Fable.Core.JS.console.log("RenameFile: ", cparent, cname, nparent, nname)
            getEntryByPath cpath
            |> Option.map (fun entry ->

                    cparent 
                    |> getEntryByPath
                    |> Option.map (fun parentEntry ->

                        if nparent = cparent then

                            { parentEntry with Children = parentEntry.Children |> Array.map (fun (name, uid) -> if name = cname then (nname, uid) else (name, uid) )}
                                |> putEntry

                            { entry with Name = nname }
                                |> putEntry
                        else
                            nparent
                            |> getEntryByPath
                            |> Option.map (fun destParentEntry ->

                                { parentEntry with Children = parentEntry.Children |> Array.filter (fun (name, _) -> name <> cname )}
                                    |> putEntry

                                { destParentEntry with Children = Array.append destParentEntry.Children [| nname, entry.Uid |]  }
                                    |> putEntry

                                { entry with Name = nname }
                                    |> putEntry

                            )
                            |> Option.defaultWith (fun _ ->
                                failwith ("Cannot find entry for target " + nparent)
                            )

                    )
                    |> Option.defaultWith (fun _ ->
                        failwith ("Cannot find entry for parent " + cparent)
                    )

            )
            |> Option.defaultWith (fun _ ->
                failwith ("Cannot find entry for " + path)
            )

            notifyOnChange path

type LocalStorageFileSystem( rootKey : string ) =
    inherit KeyedStorageFileSystem( new LocalStorage(rootKey) )

type MountedFileSystem( fs : IFileSystem, mountPoint : string) =
    let makePath( path : string ) = IFileSystem.Combine(mountPoint, path)

    let fixPath ( path :string ) = 
//        Fable.Core.JS.console.log( "fixPath: ", path, mountPoint )
        path.Substring( mountPoint.Length )

    let fixFiles ( files :string[] ) = files

    let fixFolders ( folders :string[] ) = folders 

    interface IFileSystem with
        member this.CreateFile(path: string, name: string): unit = 
            fs.CreateFile( makePath path, name )

        member this.CreateFolder(path: string): unit = 
            fs.CreateFolder(makePath path)

        member this.Exists(path: string): bool = 
            fs.Exists(makePath path)

        member this.Files(path: string): string array = 
            fs.Files( makePath path ) |> fixFiles
            
        member this.Folders(path: string): string array = 
            fs.Folders( makePath path ) |> fixFolders

        member this.GetFileContent(path: string): string = 
            fs.GetFileContent(makePath path)
            
        member this.IsFile(path: string): bool = 
            fs.IsFile( makePath path )
            
        member this.IsFolder(path: string): bool = 
            fs.IsFolder( makePath path )

        member this.OnChange(callback: string -> unit): unit = 
            fs.OnChange( fun path -> if path.StartsWith(mountPoint) then callback(fixPath path))

        member this.RemoveFile(path: string): unit = 
            fs.RemoveFile( makePath path )

        member this.RenameFile(src: string, tgt: string): unit = 
            fs.RenameFile( makePath src, makePath tgt )
        member this.SetFileContent(path: string, content: string): unit = 
            fs.SetFileContent( makePath path, content)


[<AutoOpen>]
module Extensions = 

    type IFileSystem with

        /// Wraps the non-async methods so that this instance can be passed to an 
        /// API that wants IFileSystemAsync
        member __.GetAsync() : IFileSystemAsync =
            mkAsync __

        member __.CreateFolderRecursive (path : string) =
            if __.IsFile path then
                failwith ("File exists: " + path)
            else if __.IsFolder path then
                ()
            else
                match IFileSystem.GetFolderName path with 
                | "" -> ()
                | parent -> __.CreateFolderRecursive parent
                __.CreateFolder path
                
        member __.FilesRecursive (path : string) : string []=
            Array.append (__.Folders path) (__.Files path) 
            |> Array.collect ( fun f -> 
                let p = combine path f
                if __.IsFile p then
                    [| p |]
                else
                    __.FilesRecursive p
            )

        member __.CopyFile( src : string, tgt : string ) =
            if __.Exists src then
                if __.Exists tgt then
                    failwith ("Cannot copy, file exists: " + tgt)
                let item = __.GetFileContent src
                __.SetFileContent(tgt,item)
            else
                failwith ("File does not exist: " + src)

        member fs.Copy( src : string, dst : string ) =
            let copyFileFs path (fs : IFileSystem) (fs2 : IFileSystem) =
                fs2.CreateFolderRecursive (IFileSystem.GetFolderName path)
                fs2.SetFileContent( path, fs.GetFileContent path )

            let copyFolderFs path (fs : IFileSystem) (fs2 : IFileSystem) =
                fs.FilesRecursive path |> Array.iter (fun file -> copyFileFs file fs fs2)

            if fs.IsFolder src then
                if fs.IsFile dst then 
                    failwith "Attempt to copy folder to file"

                let dst = if fs.IsFolder dst then combine dst (IFileSystem.GetFileName src) else dst

                if fs.Exists dst then   
                    failwith ("File/folder exists: " + dst)

                if src.StartsWith dst || dst.StartsWith src then
                    failwith "Attempt to copy folder to itself"

                fs.CreateFolder dst

                let dstFs = MountedFileSystem(fs, dst)
                let srcFs = MountedFileSystem(fs, src)

                copyFolderFs "/" srcFs dstFs
            else
                let dst = if fs.IsFolder dst then combine dst (IFileSystem.GetFileName src) else dst
                fs.CopyFile( src, dst )

        member __.Remove( path : string ) =
            if (__.IsFolder path ) then
                __.Files(path) 
                |> Array.iter( fun name -> __.RemoveFile( IFileSystem.Combine( path, name )) )
                __.Folders(path)
                |> Array.iter( fun name -> __.Remove( IFileSystem.Combine( path, name )) )
            __.RemoveFile( path )

        member __.MakeUnique( folder : string, basename : string, ?ext : string) =
            let ext' = ext |> Option.defaultValue ""

            let rec findUnique name (i : int) =
                if __.Exists (IFileSystem.Combine(folder, name)) then 
                    findUnique (sprintf "%s%d%s" basename i ext') (i+1)
                else
                    name

            findUnique (basename + ext') 1
