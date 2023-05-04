module SutilOxide.FileSystem

//
// Copyright (c) 2022 David Dawkins
//

open System
open Fable.SimpleJson
open Browser

type Uid = int
type FileEntryType =
    | File
    | Folder

let private parsePath (path:string) =
    path.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries)

let private buildPath (parts : seq<string>) =
    "/" + String.Join( "/", parts )

let private canonical (path : string ) =
    path |> parsePath |> buildPath

let getFolderName path =
    let items = path |> parsePath
    items |> Array.take (items.Length - 1) |> buildPath

let getFileName path =
    let items = path |> parsePath
    items |> Array.last

let private combine path file =
    sprintf "%s/%s" path file |> canonical

module Storage =
    let mk rootKey key = sprintf "%s/%s" rootKey key

    let exists rootKey key =
        (window.localStorage.getItem (mk rootKey key)) <> null

    let getContents rootKey key =
        window.localStorage.getItem( (mk rootKey key) )

    let setContents rootKey key content =
        window.localStorage.setItem( (mk rootKey key), content)

    let remove rootKey key =
        window.localStorage.removeItem (mk rootKey key)

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

type IReadOnlyFileSystem =
    abstract member Files : path : string -> string[]
    abstract member Folders : path :string -> string[]
    abstract member Exists : path : string -> bool
    abstract member IsFile : path : string -> bool
    abstract member IsFolder : path : string -> bool
    abstract member GetFileContent : path : string  -> string
    abstract member OnChange : (string -> unit) -> unit

type IFileSystem =
    inherit IReadOnlyFileSystem
    abstract member SetFileContent : string * string -> unit
    abstract member RemoveFile     : path : string -> unit
    abstract member CreateFile     : string * string  -> unit
    abstract member CreateFolder   : string -> unit
    abstract member RenameFile     : string * string -> unit

[<AutoOpen>]
module IFileSystemExt =
    type IFileSystem with
        static member Combine( a, b ) = combine a b |> canonical
        static member GetFolderName path = getFolderName path
        static member GetFileName path = getFileName path
        static member GetExtension (path : string) =
            let fileName = getFileName path
            let p = fileName.LastIndexOf('.')
            if p < 0 then "" else fileName.Substring(p)

type LocalStorageFileSystem( rootKey : string ) =

    //let mutable root : FileEntry = { Type = Folder; Name = "/"; Uid = 0; Content = ""; Children = Array.empty }
    let mutable root = { NextUid = 1 }

    let mutable onChange : (string -> unit) list = []

    let notifyOnChange (path : string) = onChange |> List.iter (fun h -> h path)

    let uidKey uid = sprintf "uid:%d" uid

    let delEntry (e : FileEntry) =
        Storage.remove rootKey (uidKey e.Uid)

    let putEntry (e : FileEntry) =
        Storage.setContents rootKey (uidKey e.Uid) (Json.serialize(e))

    let getEntry uid =
        try
            Json.parseAs<FileEntry>( Storage.getContents rootKey (uidKey uid) ) |> Some
        with _ -> None

    let entryExists uid =
        Storage.exists rootKey uid

    let nameOf (e:FileEntry) = e.Name

    let getEntries uid =
        let result = 
            match getEntry uid with
            | None -> failwith ("Non-existent UID " + string uid)
            | Some e when e.Type <> FileEntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
            | Some e -> e.Children |> Array.map (snd>>getEntry) |> Array.choose id

        //Fable.Core.JS.console.log("genEntries", uid, result)
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

    let getEntriesWhere (filter: FileEntry -> bool) (path : string) =
        path
        |> canonical
        |> uidOf
        |> Option.map (fun id -> getEntries id
                                |> Array.filter filter
                                |> Array.map (fun f -> f.Name))
        |> Option.defaultValue Array.empty

    let putRoot() =
        Storage.setContents rootKey "(root)" (Json.serialize root)

    let initRoot() =

        if not (Storage.exists rootKey (uidKey 0)) then
            { Type = Folder; Name = "/"; Uid = 0; Content = ""; Children = Array.empty }
            |> putEntry

        Storage.getContents rootKey "(root)"
            |> function
            | s when s <> null -> root <- Json.parseAs<Root>(s)
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
        let name = getFileName folderPath
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
                createFile (cpath |> getFolderName) (cpath |> getFileName) false

            getEntryByPath cpath
            |> Option.iter (fun e -> { e with Content = content } |> putEntry)

            SutilOxide.Logging.log( "SetFileContent " + path )
            notifyOnChange path

        member __.RemoveFile( path : string ) =
            getEntryByPath path |>
            Option.map (fun entry ->
                let folderName = getFolderName path
                let fileName = getFileName path

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
            createFolder path false

        member __.CreateFile( path : string, name : string ) =
            createFile path name true

        member __.RenameFile( path : string, newNameOrPath : string ) =
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
            let cname = getFileName cpath
            let nname = getFileName npath

            if not (isEntry nparent) then
                failwith ("Parent folder for rename target does not exist: " + nparent)

            getEntryByPath path
            |> Option.map (fun entry ->
                if cparent = nparent then
                    cparent |> getEntryByPath
                    |> Option.map (fun parentEntry ->
                        { parentEntry with Children = parentEntry.Children |> Array.map (fun (name, uid) -> if name = cname then (nname, uid) else (name, uid) )}
                            |> putEntry
                        { entry with Name = nname }
                            |> putEntry
                    )
                    |> Option.defaultWith (fun _ ->
                        failwith ("Cannot find entry for parent " + cparent)
                    )
            )
            |> Option.defaultWith (fun _ ->
                failwith ("Cannot find entry for " + path)
            )

            notifyOnChange path
