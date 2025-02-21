module SutilOxide.KeyedStorageFileSystemAsync

open SutilOxide.FileSystem
open SutilOxide.PromiseResult
open SutilOxide.FileSystem.Internal

type KeyedStorageFileSystemAsync( keyStorage : IKeyedStorageAsync ) =
    //let mutable root : FileEntry = { Type = Folder; Name = "/"; Uid = 0; Content = ""; Children = Array.empty }
    let mutable root = { NextUid = 1 }

    let mutable onChange : (string -> unit) list = []

    let notifyOnChange (path : string) = onChange |> List.iter (fun h -> h path)

    let uidKey uid = sprintf "uid:%d" uid

    let delEntry (e : FileEntry) =
        keyStorage.Remove (uidKey e.Uid)

    let putEntry (e : FileEntry) =
        keyStorage.Put( (uidKey e.Uid), (Thoth.Json.Encode.Auto.toString(e)) )

    let getEntry (uid : Uid) = 
        promise {
            let! entry = keyStorage.Get (uidKey uid)
            //Fable.Core.JS.console.log("getEntry: ", (uidKey uid), entry )
            match Thoth.Json.Decode.Auto.fromString<FileEntry>( entry ) with
            | Ok r -> 
                return Some r
            | Error msg ->
                Fable.Core.JS.console.log(sprintf "Error: getEntry %d: %s" uid msg)
                return None
        }

    let entryExists uid =
        keyStorage.Exists uid

    let nameOf (e:FileEntry) = e.Name

    let getEntries uid =
        promise {
            let! entry = getEntry uid

            let! result = 
                match entry with
                | None -> failwith ("Non-existent UID " + string uid)
                | Some e when e.Type <> FileEntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
                | Some e -> 
                    promise {
                        let! entries = 
                            e.Children 
                            |> Array.map (fun (a,b) -> getEntry b) 
                            |> Promise.all

                        return entries |> Array.choose id
                    }
            return result
        }

    let hasEntries uid =
        promise {
            let! entry = getEntry uid
            let result = 
                match entry with
                | None -> failwith ("Non-existent UID " + string uid)
                | Some e when e.Type <> FileEntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
                | Some e -> e.Children.Length > 0
            return result
        }


    let entryName uid =
        getEntry uid |> Promise.map (Option.map (fun e -> e.Name))

    let entryNameWithDefault defaultName uid =
        getEntry uid 
        |> Promise.map (fun e ->
            e
            |> Option.map (fun e -> e.Name) 
            |> Option.defaultValue defaultName
        )

    let entryChildren uid =
        getEntry uid
            |> Promise.map (Option.map (fun e -> e.Children))

    let entryChildNames uid =
        entryChildren uid |> Promise.map(Option.map (Array.map fst))

    let entryChildUids uid =
        entryChildren uid |> Promise.map(Option.map (Array.map snd))


//        getEntry uid
//            |> Option.map (fun e -> e.Children |> Array.map entryName |> Array.choose id)

    let rec uidOf path =
        let parts = path |> Internal.parsePath

        let rec findUid (parent : Uid) (parts : string[]) i : Promise<Uid option> =
            match i with
            | n when n >= parts.Length -> (Some parent) |> Promise.lift
            | _ ->
                promise {
                    let! entry = getEntry parent
                    match entry with
                    | None -> return failwith ("No entry found for part of path " + path)
                    | Some e ->
                        match e.Children |> Array.tryFind (fun (name,_) -> name = parts[i]) with
                        | None -> return None
                        | Some (_,uid) -> return! findUid uid parts (i+1)
                }

        let result = findUid 0 parts 0
        
        // result |> Promise.map (fun r ->
        //     Fable.Core.JS.console.log("Path UID", path, r)
        //     r)
        result 

    let getEntryByPath path = 
        promise {
            let! uid = uidOf path

            match uid with
            | None -> return None
            | Some id ->
                return! getEntry id
        }

    let isEntry (path:string) =
        path |> getEntryByPath |> Promise.map (_.IsSome)

    let isFile (path:string) =
        path |> getEntryByPath |> Promise.map (fun e -> e |> Option.map (fun e -> e.Type = File) |> Option.defaultValue false)

    let isFolder (path:string) =
        path |> getEntryByPath |> Promise.map (fun e -> e |> Option.map (fun e -> e.Type = Folder) |> Option.defaultValue false)

    let makeKey (path:string) =
        "fs:" + path

    let validateFileName (file:string) =
        if file.Contains("..") || file.Contains("/") || file.Contains("\\") then
            failwith ("Invalid file name: " + file)

    let hasEntries (path : string) =
        promise {
            let! uid = 
                path
                |> Internal.canonical
                |> uidOf
            match uid with
            | Some uid -> return! hasEntries uid
            | None -> return false
        }

    let getEntriesWhere (filter: FileEntry -> bool) (path : string) =
        promise {
            let! uid = 
                path
                |> Internal.canonical
                |> uidOf

            match uid with
            | Some uid ->
                let! entries = getEntries uid 
                return entries 
                    |> Array.filter filter
                    |> Array.map (fun f -> f.Name)

            | None -> return Array.empty
        }

    let putRoot() =
        keyStorage.Put( "(root)", (Thoth.Json.Encode.Auto.toString root) )

    let initRoot() =
        promise {
            let! exists = keyStorage.Exists(uidKey 0)
            if not exists then
                do! 
                    { Type = Folder; Name = "/"; Uid = 0; Content = ""; Children = Array.empty }
                    |> putEntry

            let! rootEntry = keyStorage.Get("(root)")
            
            rootEntry
                |> function
                | s when s <> null -> 
                    match Thoth.Json.Decode.Auto.fromString<Root>(s) with
                    | Ok r -> root <- r
                    | Error msg ->
                        Fable.Core.JS.console.log("Root entry corrupted: " + msg)
                |_ -> ()

            do! putRoot()
        }

    let newUid() =
        let uid = root.NextUid
        root <- { root with NextUid = uid + 1 }
        promise {
            do! putRoot()
            return uid
        }

    let assertTrue (c : Promise<bool>) (message : string) =
        promise {
            let! _test = c
            if (not _test) then failwith message
        }

    let assertFalse (c : Promise<bool>) (message : string) =
        promise {
            let! _test = c
            if (_test) then failwith message
        }

    let assertNotIsEntry (fname : string) (message : string) =
        assertFalse (isEntry fname) message

    let assertIsFolder (fname : string)  =
        assertTrue (isFolder fname)  ("Not a folder: " + fname)

    let assertIsFile (fname : string) =
        assertTrue (isFile fname) ("Not a file: " + fname)


    let createChildEntry path name notify entryType =
        validateFileName name
        let cpath = path |> Internal.canonical
        let fname = Path.combine cpath name

        promise {
            do! assertNotIsEntry fname ("File already exists: " + fname)
            do! assertIsFolder cpath 

            let! entryOpt = getEntryByPath cpath

            match entryOpt with
            | Some entry ->
                let! uid = newUid()

                let parentEntry =
                    { entry with Children = entry.Children |> Array.append [| name, uid |] }

                do! putEntry parentEntry

                let childEntry =
                    {
                        Type = entryType
                        Content = ""
                        Children = Array.empty
                        Uid = uid
                        Name = name
                    }

                do! putEntry childEntry

            | None -> 
                failwith "Parent folder does not exist"

            if notify then
                notifyOnChange fname
        }

    let createFile path name notify = createChildEntry path name notify File

    let createFolder folderPath notify = 
        let name = Path.getFileNameWithExt folderPath
        let parent = Path.getFolderName folderPath
        createChildEntry parent name notify Folder

    let getFileContent(path:string) =
        let cpath = path |> Internal.canonical

        promise {
            do! assertIsFile cpath 

            return!
                getEntryByPath cpath 
                |> Promise.map (fun entry ->
                    entry |> Option.map (fun e -> e.Content) |> Option.defaultValue "")
        }

    let setFileContent(path:string, content:string) =
        let cpath = path |> Internal.canonical

        promise {
            do! assertFalse(isFolder cpath) ("Cannot set contents of a folder")

            let! _isFile = isFile cpath
            if not _isFile then            
                do! createFile (cpath |> Path.getFolderName) (cpath |> Path.getFileNameWithExt) false

            let! entryOpt = getEntryByPath cpath

            match entryOpt with
            | Some e ->
                do! { e with Content = content } |> putEntry
            | None -> ()

            SutilOxide.Logging.log( "SetFileContent " + path )
            notifyOnChange path
        }

    let assertEmptyFolder path =
        promise {
            let! _isFolder = isFolder path
            if _isFolder then
                do! assertFalse( hasEntries path) ("Folder is not empty" )
        }

    let assertExists path =
        assertTrue (isEntry path) ("Path does not exist: " + path)

    let opt_pr_map (map : 't -> 'u) (p : Promise<'t option>) : Promise<'u option> =
        p |> Promise.map (fun value -> value |> Option.map map)
        
    let opt_pr_bind (map : 't -> Promise<'u option>) (p : Promise<'t option>) : Promise<'u option> =
        p 
        |> Promise.bind (function Some t -> map t | _ -> Promise.lift None)
        
    let (>>=) a b = opt_pr_bind b a

    let mutable isLocked = false
    let lockClients = ResizeArray<unit -> unit>()

    let nextLockClient() =
        if not isLocked && lockClients.Count > 0 then
            isLocked <- true
            let next = lockClients[0]
            lockClients.RemoveAt(0)
            next( () )

    let getLock() : Promise<unit> =
        let p = Promise.create( fun resolve _ -> lockClients.Add(resolve) )
        nextLockClient()
        p

    let releaseLock() : unit =
        isLocked <- false
        nextLockClient()

    let removeFile (path : string) =
        promise {
            do! assertExists path
            do! assertEmptyFolder path
            
            let! remove = 

                getEntryByPath path
                >>= fun entry ->
                    let folderName = Path.getFolderName path
                    let fileName = Path.getFileNameWithExt path

                    getEntryByPath folderName 
                    >>= fun parentEntry ->

                        entryChildren (parentEntry.Uid)
                        >>= fun children ->

                            let newParentEntry =
                                { parentEntry with 
                                    Children = 
                                    children |>
                                    Array.filter (fun (name,uid) -> name <> fileName) }

                            promise {
                                do! delEntry entry

                                do! putEntry newParentEntry

                                return Some ()
                            }

            match remove with
            | Some _ -> ()
            | None -> 
                failwith ("Remove failed: " + path)

            notifyOnChange path
        }

    let renameFile(path : string, newNameOrPath : string) =
        promise {
            let cpath = path |> Internal.canonical

            let npath =
                if newNameOrPath.StartsWith("/") then
                    newNameOrPath |> Internal.canonical
                else
                    validateFileName newNameOrPath
                    Path.combine (Path.getFolderName cpath) newNameOrPath

            if cpath = "/" || npath = "/" then
                failwith "Cannot rename to/from '/'"

            do! assertTrue (isEntry cpath) ("Cannot rename non-existent file: " + cpath)
            do! assertFalse (isEntry npath) ("Cannot rename to existing file " + npath)

            let cparent = Path.getFolderName cpath
            let nparent = Path.getFolderName npath
            let cname = Path.getFileNameWithExt cpath
            let nname = Path.getFileNameWithExt npath

            do! assertTrue (isEntry nparent) ("Parent folder for rename target does not exist: " + nparent)

            let! rename =
                getEntryByPath cpath
                >>= fun entry ->
                    cparent 
                    |> getEntryByPath
                    >>= fun parentEntry ->

                        if nparent = cparent then
                            promise {
                                do! 
                                    { parentEntry with Children = parentEntry.Children |> Array.map (fun (name, uid) -> if name = cname then (nname, uid) else (name, uid) )}
                                    |> putEntry

                                do! { entry with Name = nname } |> putEntry

                                return Some ()
                            }
                        else
                            nparent
                            |> getEntryByPath
                            >>= fun destParentEntry ->
                                promise {
                                    do! { parentEntry with Children = parentEntry.Children |> Array.filter (fun (name, _) -> name <> cname )}
                                        |> putEntry

                                    do! { destParentEntry with Children = Array.append destParentEntry.Children [| nname, entry.Uid |]  }
                                        |> putEntry

                                    do! { entry with Name = nname }
                                        |> putEntry

                                    return Some ()
                                }
            notifyOnChange path
        }

    do
        initRoot() |> Promise.start

    let mkResult (name: string) (f : unit -> Promise<'t>) = 
        promise {
            do! getLock()
            try 
                //Fable.Core.JS.console.log("FS: " + name)
                return! f()
            finally
                releaseLock()
        }

with
    interface IFileSystemAsyncP with

        member _.OnChange (cb : string -> unit) = 
            let run() =
                onChange <- onChange @ [cb]
                { new System.IDisposable with member __.Dispose() = () (* FIXME *) } 
                |> Promise.lift
            run |> mkResult "OnChange"

        member _.Files (path : string) = (fun () -> getEntriesWhere (fun e -> e.Type = File) path)  |> mkResult "Files"
        member _.Folders (path :string) = (fun () -> getEntriesWhere (fun e -> e.Type = Folder) path) |> mkResult "Folders"

        member __.Exists(path : string) = mkResult "Exists" <| fun () -> isEntry(path)
        member __.IsFile(path : string) = mkResult "IsFile" <| fun () -> isFile(path)
        member __.IsFolder(path : string) = mkResult "IsFolder" <| fun () -> isFolder(path)

        member _.GetFileContent( path : string ) =
            mkResult "GetFileContent" <| fun () -> getFileContent path

        member __.SetFileContent( path : string, content : string ) =
            mkResult "SetFileContent" <| fun () -> setFileContent(path, content)

        member __.RemoveFile( path : string ) =
            mkResult (sprintf "RemoveFile('%s')" path) <| 
            fun () ->  removeFile path

        member _.CreateFolder( path : string ) =
            mkResult "CreateFolder" <| fun () -> createFolder path true

        member __.CreateFile( path : string, name : string ) =
            mkResult "CreateFile" <| fun () -> createFile path name true

        member __.RenameFile( path : string, newNameOrPath : string ) =
            mkResult "RenameFile" <| fun () -> renameFile(path, newNameOrPath)


