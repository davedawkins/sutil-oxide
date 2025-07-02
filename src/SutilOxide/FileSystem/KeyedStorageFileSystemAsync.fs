module SutilOxide.KeyedStorageFileSystemAsync

open SutilOxide.FileSystem
open SutilOxide.PromiseResult
open SutilOxide.FileSystem.Internal

let inline private encode (e : 't) = Thoth.Json.Encode.Auto.toString e
let inline private decode<'t> s : Result<'t,string> = Thoth.Json.Decode.Auto.fromString<'t> s

type KeyedStorageFileSystemAsync( keyStorage : IKeyedStorageAsync ) =

    let mutable initialized = false    
    let mutable root = { NextUid = 1 }
    let mutable onChange : (string -> unit) list = []
    
    // Add a cache for frequently accessed entries
    let entryCache = System.Collections.Generic.Dictionary<int, FileEntry option>()
    let [<Literal>]  CacheLimit = 100
    let [<Literal>]  RootKeyName = "(root)"
    let [<Literal>]  RootName = "/"
    let [<Literal>]  RootUid : Uid = 0

    let uidKey uid = sprintf "uid:%d" uid

    let notifyOnChange (path : string) = onChange |> List.iter (fun h -> h path)

    // Function to clear cache when it gets too large
    let trimCache() =
        if entryCache.Count > CacheLimit then
            entryCache.Clear()

    let beginBatch() =
        keyStorage.BeginBatch()
        
    let commitBatch() =
        keyStorage.CommitBatch()

    // ------------------------------------------------------------------------
    // Initialization

    let putEntryUnsafe (e : FileEntry) =
        entryCache.[e.Uid] <- Some e
        trimCache()
        keyStorage.Put( uidKey e.Uid, fileEntryToJSON e )

    let putRoot() =
        keyStorage.Put( RootKeyName,  encode root)        

    let initRoot() =
        promise {
            let! exists = keyStorage.Exists(uidKey 0)
            if not exists then
                do! 
                    let rootE : FileEntry =
                        { Type = Folder; Name = RootName; Uid = RootUid; Content = ""; Children = Array.empty; Meta = FileMetaData.Create() }
                    rootE
                    |> putEntryUnsafe

            let! rootEntry = keyStorage.Get(RootKeyName)
            
            rootEntry
                |> function
                | s when s <> null -> 
                    match decode<Root>(s) with
                    | Ok r -> root <- r
                    | Error msg ->
                        Fable.Core.JS.console.error("Root entry corrupted: " + msg)
                |_ -> ()

            do! putRoot()
        }

    let init() = 
        promise {
            if not initialized then
                initialized <- true
                do! initRoot()
        }

    // ------------------------------------------------------------------------
    // Entry primitives, with caching, batch operations and initialization

    let delEntry (e : FileEntry) =
        promise {
            do! init()
            entryCache.Remove(e.Uid) |> ignore
            do! keyStorage.Remove (uidKey e.Uid)
        }

    let putEntry (e : FileEntry) =
        promise {
            do! init()
            do! putEntryUnsafe e
        }

    let getEntry (uid : Uid) : PromiseResult<FileEntry,string> = 
        // Check cache first
        if entryCache.ContainsKey(uid) then
            entryCache.[uid] |> (function Some x -> Ok x | None -> Error "Missing") |> Promise.lift
        else
            promise {
                do! init()

                let! entry = keyStorage.Get (uidKey uid)

                if entry = null then
                    return (Error ("Not entry for UID: " + string uid))
                else
                    match fileEntryFromJSON entry with
                    | Ok r -> 
                        // Cache the result
                        entryCache.[uid] <- Some r
                        trimCache()
                        return Ok r
                    | Error msg ->
                        entryCache.[uid] <- None
                        return Error msg
            }

    let entryExists uid =
        promise {
            if entryCache.ContainsKey(uid) then
                return entryCache.[uid].IsSome
            else
                do! init()
                return! keyStorage.Exists (uidKey uid)
        }

    // ------------------------------------------------------------------------

    let nameOf (e:FileEntry) = e.Name

    let getEntries uid =
        promise {
            let! entry = getEntry uid

            match entry with
            | Error s -> return failwith s // ("Non-existent UID " + string uid)
            | Ok e when e.Type <> FileEntryType.Folder -> return failwith (sprintf "Not a folder: %d" uid)
            | Ok e -> 
                // Batch operation for multiple get operations
                beginBatch()
                try
                    let! entries = 
                        e.Children 
                        |> Array.map (fun (a,b) -> getEntry b) 
                        |> Promise.all
                    
                    do! commitBatch()
                    return entries |> Array.map Result.toOption |> Array.choose id
                with ex ->
                    do! commitBatch()
                    return raise ex

        }

    let hasEntries uid =
        promise {
            let! entry = getEntry uid
            let result = 
                match entry with
                | Error s -> failwith s //("Non-existent UID " + string uid)
                | Ok e when e.Type <> FileEntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
                | Ok e -> e.Children.Length > 0
            return result
        }

    let entryName uid =
        getEntry uid |> Promise.map (Result.map (fun e -> e.Name))

    let entryNameWithDefault defaultName uid =
        getEntry uid 
        |> Promise.map (fun e ->
            e
            |> Result.map (fun e -> e.Name) 
            |> Result.defaultValue defaultName
        )

    let entryChildren uid =
        getEntry uid
            |> Promise.map (Result.map (fun e -> e.Children))

    let entryChildNames uid =
        entryChildren uid |> Promise.map(Result.map (Array.map fst))

    let entryChildUids uid =
        entryChildren uid |> Promise.map(Result.map (Array.map snd))

    let rec uidOf path =
        let parts = path |> parsePath

        let rec findUid (parent : Uid) (parts : string[]) i : PromiseResult<Uid,string> =
            match i with
            | n when n >= parts.Length -> (Ok parent) |> Promise.lift
            | _ ->
                promise {
                    let! entry = getEntry parent
                    match entry with
                    | Error e ->
                        return Error ("No entry found for part of path " + path + " at " + string i + ": '" + (parts[i]) + "': " + e)
                    | Ok e ->
                        match e.Children |> Array.tryFind (fun (name,_) -> name = parts[i]) with
                        | None -> return (Error ("Child not found: " + parts[i]))
                        | Some (_,uid) -> return! findUid uid parts (i+1)
                }

        findUid 0 parts 0

    let getEntryByPath path = 
        promise {
            let! uid = uidOf path

            match uid with
            | Error e -> return (Error e)
            | Ok id ->
                return! getEntry id
        }

    let isEntry (path:string) =
        path |> getEntryByPath |> Promise.map (function Ok _ -> true | Error _ -> false)

    let isFile (path:string) =
        path |> getEntryByPath |> Promise.map (fun e -> e |> Result.map (fun e -> e.Type = File) |> Result.defaultValue false)

    let isFolder (path:string) =
        path |> getEntryByPath |> Promise.map (fun e -> e |> Result.map (fun e -> e.Type = Folder) |> Result.defaultValue false)

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
            | Ok uid -> return! hasEntries uid
            | Error _ -> return false
        }

    let getEntriesWhere (filter: FileEntry -> bool) (path : string) =
        promise {
            let! uid = 
                path
                |> Internal.canonical
                |> uidOf

            match uid with
            | Ok uid ->
                let! entries = getEntries uid 
                return entries 
                    |> Array.filter filter
                    |> Array.map (fun f -> f.Name)

            | Error _ -> return Array.empty
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

    let assertExists (fname : string) =
        assertTrue (isEntry fname) ("Not found: " + fname)

    let assertIsFolder (fname : string)  =
        assertTrue (isFolder fname)  ("Not a folder: " + fname)

    let assertIsFile (fname : string) =
        assertTrue (isFile fname) ("Not a file: " + fname)

    let rec createFolderRecursive path notify : Promise<unit> =
        promise {
            let! pathIsFile = isFile path
            if pathIsFile then
                failwith ("File exists: " + path)
            else 
                let! pathIsFolder = isFolder path
                if not pathIsFolder then
                    match Path.getFolderName path with 
                    | "" -> ()
                    | parent -> 
                        do! createFolderRecursive parent notify
                    do! createChildEntry path notify Folder
        }

    // Optimized with batch operations
    and createChildEntry path notify entryType =
        let fname = path |> Internal.canonical
        let cpath = getFolderName fname
        let name = getFileNameWithExt fname
        validateFileName name

        promise {
            beginBatch()
            try
                do! assertNotIsEntry fname ("File already exists: " + fname)

                let! isFolder = isFolder cpath
                if not isFolder then
                    do! createFolderRecursive cpath notify
                // do! assertIsFolder cpath 

                let! entryOpt = getEntryByPath cpath

                match entryOpt with
                | Ok entry ->
                    let! uid = newUid()

                    let parentEntry =
                        { entry with Children = entry.Children |> Array.append [| name, uid |] }

                    do! putEntry parentEntry

                    let childEntry =
                        {
                            FileEntry.Create() with
                                Type = entryType
                                Content = ""
                                Children = Array.empty
                                Uid = uid
                                Name = name
                        }

                    do! putEntry childEntry
                
                | Error s -> 
                    failwith ("Parent folder does not exist: " + s)
                
                do! commitBatch()
                
                if notify then
                    notifyOnChange fname
            with ex ->
                do! commitBatch()
                return raise ex
        }

    let createFolder folderPath notify = 
        // let name = Path.getFileNameWithExt folderPath
        // let parent = Path.getFolderName folderPath
        promise {
            let! exists = isFolder folderPath
            if not exists then 
                do! createChildEntry folderPath notify Folder
        }

    let createFile path notify = createChildEntry path notify File

    let getFileContent(path:string) =
        let cpath = path |> Internal.canonical

        promise {
            do! assertIsFile cpath 

            return!
                getEntryByPath cpath 
                |> Promise.map (fun entry ->
                    entry |> Result.map (fun e -> e.Content) |> Result.defaultValue "")
        }

    let getMeta(path:string) =
        let cpath = path |> Internal.canonical

        promise {
            do! assertExists cpath 

            return!
                getEntryByPath cpath 
                |> Promise.map (fun entry ->
                    entry |> Result.map (fun e -> e.Meta) |> Result.defaultWith (fun x -> failwithf "Not found: %s: %s" path x))
        }


    // Optimized with batch operations
    let setFileContent(path:string, content:string) =
        let cpath = path |> Internal.canonical

        promise {
            beginBatch()
            try
                do! assertFalse(isFolder cpath) ("Cannot set contents of a folder")

                let! _isFile = isFile cpath
                if not _isFile then            
                    do! createFile cpath false

                let! entryOpt = getEntryByPath cpath

                match entryOpt with
                | Ok e ->
                    do! { e with Content = content; Meta.ModifiedAt = System.DateTime.UtcNow } |> putEntry
                | Error s -> 
                    failwith ("setFileContent failed: " + s)

                do! commitBatch()
                notifyOnChange path
            with ex ->
                do! commitBatch()
                return raise ex
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
        
    let (>>=) a b = 
        PromiseResult.bind b a
        // opt_pr_bind b a

    // Optimized with batch operations
    let removeFile (path : string) =
        promise {
            beginBatch()
            try
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
                                    return (Ok ())
                                }

                match remove with
                | Ok _ -> ()
                | Error s -> 
                    failwith ("Remove failed: " + path + ": " + s)

                do! commitBatch()
                notifyOnChange path
            with ex ->
                do! commitBatch()
                return raise ex
        }

    // Optimized with batch operations
    let renameFile(path : string, newNameOrPath : string) =
        promise {
            beginBatch()
            try
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
                                    return Ok ()
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

                                        return Ok ()
                                    }
                
                do! commitBatch()
                notifyOnChange path
            with ex ->
                do! commitBatch()
                return raise ex
        }
    
    // Add a cleanup method to dispose resources
    let cleanupResources() =
        keyStorage.Close() 

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

    let mkResult (name: string) (f : unit -> Promise<'t>) = 
        promise {
            do! getLock()
            try 
                return! f()
            finally
                releaseLock()
        }

    let mutable handlerIds = 0
    let handlers = System.Collections.Generic.Dictionary<int, string -> unit>()

with
    member this.Initialise() = initRoot()

    interface IFileSystemAsyncP with
        member _.OnChange (cb : string -> unit) = 
            let run() =
                // Generate a unique ID for this handler
                let id = handlerIds
                handlerIds <- handlerIds + 1
                
                // Store the handler with its ID
                handlers.Add(id, cb)
                
                // Add to the onChange list
                onChange <- onChange @ [cb]
                
                // Return a disposable that uses the ID to properly remove the handler
                { new System.IDisposable with 
                    member __.Dispose() = 
                        if handlers.ContainsKey(id) then
                            // Get the handler by ID
                            let handler = handlers.[id]
                            // Remove from the handlers dictionary
                            handlers.Remove(id) |> ignore
                            // Remove from the onChange list
                            onChange <- onChange |> List.filter (fun h -> not (obj.ReferenceEquals(h, handler)))
                } 
                |> Promise.lift
            
            run |> mkResult "OnChange"

        member _.Files (path : string) = (fun () -> getEntriesWhere (fun e -> e.Type = File) path)  |> mkResult "Files"
        member _.Folders (path :string) = (fun () -> getEntriesWhere (fun e -> e.Type = Folder) path) |> mkResult "Folders"

        member __.Exists(path : string) = mkResult "Exists" <| fun () -> isEntry(path)
        member __.IsFile(path : string) = mkResult "IsFile" <| fun () -> isFile(path)
        member __.IsFolder(path : string) = mkResult "IsFolder" <| fun () -> isFolder(path)

        member _.GetFileContent( path : string ) =
            mkResult "GetFileContent" <| fun () -> getFileContent path

        member _.GetModifiedAt( path : string ) =
            mkResult "GetModifiedAt" <| fun () -> path |> getMeta |> Promise.map _.ModifiedAt

        member _.GetCreatedAt (path: string): AsyncPromise<FsDateTime> = 
            mkResult "GetCreatedAt" <| fun () -> path |> getMeta |> Promise.map _.CreatedAt

        member __.SetFileContent( path : string, content : string ) =
            mkResult "SetFileContent" <| fun () -> setFileContent(path, content)

        member __.RemoveFile( path : string ) =
            mkResult (sprintf "RemoveFile('%s')" path) <| 
            fun () ->  removeFile path

        member _.CreateFolder( path : string ) =
            mkResult "CreateFolder" <| fun () -> createFolder path true

        // member __.CreateFile( path : string ) =
        //     mkResult "CreateFile" <| fun () -> createFile path true

        member __.RenameFile( path : string, newNameOrPath : string ) =
            mkResult "RenameFile" <| fun () -> renameFile(path, newNameOrPath)

    interface System.IDisposable with
        member __.Dispose() =
            cleanupResources() |> Promise.start