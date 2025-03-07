module SutilOxide.KeyedStorageFileSystem

open SutilOxide.FileSystem
open SutilOxide.FileSystem.Internal

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
        let parts = path |> Internal.parsePath

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
        |> Internal.canonical
        |> uidOf
        |> Option.map hasEntries
        |> Option.defaultValue false

    let getEntriesWhere (filter: FileEntry -> bool) (path : string) =
        path
        |> Internal.canonical
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
        let cpath = path |> Internal.canonical
        let fname = Path.combine cpath name

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
            notifyOnChange fname

    let createFolder folderPath notify=
        let name = Path.getFileNameWithExt folderPath
        let parent = Path.getFolderName folderPath

        validateFileName name
        let cpath = parent |> Internal.canonical
        let fname = Internal.combine cpath name

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
            notifyOnChange fname            

    let getFileContent(path:string) =
        let cpath = path |> Internal.canonical

        if not (isFile cpath) then
            failwith ("Not a file: " + cpath)

        getEntryByPath cpath |> Option.map (fun e -> e.Content) |> Option.defaultValue ""

    let setFileContent(path:string, content:string) =
        let cpath = path |> Internal.canonical

        if (isFolder cpath) then
            failwith ("Not a file: " + cpath)

        if not (isFile cpath) then
            createFile (cpath |> Path.getFolderName) (cpath |> Path.getFileNameWithExt) false

        getEntryByPath cpath
        |> Option.iter (fun e -> { e with Content = content } |> putEntry)

        notifyOnChange path

    let removeFile (path : string) =
        if isFolder path && hasEntries path then
            failwith ("Folder is not empty")

        getEntryByPath path |>
        Option.map (fun entry ->
            let folderName = Path.getFolderName path
            let fileName = Path.getFileNameWithExt path

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

        notifyOnChange path

    let renameFile(path : string, newNameOrPath : string) =
            let cpath = path |> Internal.canonical

            let npath =
                if newNameOrPath.StartsWith("/") then
                    newNameOrPath |> Internal.canonical
                else
                    validateFileName newNameOrPath
                    Internal.combine (Path.getFolderName cpath) newNameOrPath

            if cpath = "/" || npath = "/" then
                failwith "Cannot rename to/from '/'"

            if not (isEntry cpath) then
                failwith ("Cannot rename non-existent file: " + cpath)

            if (isEntry npath) then
                failwith ("Cannot rename to existing file " + npath)

            let cparent = Path.getFolderName cpath
            let nparent = Path.getFolderName npath
            let cname = Path.getFileNameWithExt cpath
            let nname = Path.getFileNameWithExt npath

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

    let mkSyncThrowable( value : unit -> 't ) : SyncThrowable<'t> = SyncThrowable<'t>.Of(value)


    do
        initRoot()
with
    interface IFileSystem with

        member _.OnChange (cb : string -> unit) = 
            let run() =
                onChange <- onChange @ [cb]
                { new System.IDisposable with member __.Dispose() = () (* FIXME *) }
            run |> mkSyncThrowable

        member _.Files (path : string) = (fun () -> getEntriesWhere (fun e -> e.Type = File) path) |> mkSyncThrowable
        member _.Folders (path :string) = (fun () -> getEntriesWhere (fun e -> e.Type = Folder) path) |> mkSyncThrowable

        member __.Exists(path : string) = mkSyncThrowable <| fun () -> isEntry(path)
        member __.IsFile(path : string) = mkSyncThrowable <| fun () -> isFile(path)
        member __.IsFolder(path : string) = mkSyncThrowable <| fun () -> isFolder(path)

        member _.GetFileContent( path : string ) =
            mkSyncThrowable <| fun () -> getFileContent path

        member __.SetFileContent( path : string, content : string ) =
            mkSyncThrowable <| fun () -> setFileContent(path, content)

        member __.RemoveFile( path : string ) =
            mkSyncThrowable <| fun () -> removeFile path

        member _.CreateFolder( path : string ) =
            mkSyncThrowable <| fun () -> createFolder path true

        member __.CreateFile( path : string, name : string ) =
            mkSyncThrowable <| fun () -> createFile path name true

        member __.RenameFile( path : string, newNameOrPath : string ) =
            mkSyncThrowable <| fun () -> renameFile(path, newNameOrPath)


type LocalStorageFileSystem( rootKey : string ) =
    inherit KeyedStorageFileSystem( new LocalStorage(rootKey) )

