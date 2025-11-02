module SutilOxide.KeyedStorageFileSystemAsync

open SutilOxide.FileSystem
open SutilOxide.PromiseResult
open SutilOxide.FileSystem.Internal
open SutilOxide.FileSystem.Types

open JsHelpers

let inline private encode (e : 't) = Thoth.Json.Encode.Auto.toString e
let inline private decode<'t> s : Result<'t,string> = Thoth.Json.Decode.Auto.fromString<'t> s

open Fable.Core

type KeyedStorageFileSystemAsync( keyStorage : IKeyedStorageAsync ) =

    let mutable initialized = false    
    let mutable root = { NextUid = 1 }
    let mutable onChange : (string -> unit) list = []
    
    // Add a cache for frequently accessed entries
    let entryCache = System.Collections.Generic.Dictionary<int, EntryStorage option>()
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

    let putEntryUnsafe (e : EntryStorage) =
        entryCache.[e.Uid] <- Some e
        trimCache()
        keyStorage.Put( uidKey e.Uid, fileEntryToByteArray e)

    let putRoot() =
        keyStorage.Put( RootKeyName,  encode root |> ByteArray.textEncode)        

    let getRootKeyJson() =
        promise {
            let! rootEntry = keyStorage.Get(RootKeyName)
            if rootEntry = null then
                return null
            else
                match jsTypeOf rootEntry with
                | "string" -> 
                    return rootEntry :?> string
                | _ -> 
                    let json = (rootEntry :?> ByteArray) |> ByteArray.textDecode
                    // Fable.Core.JS.console.log("RootObject: ", rootEntry, json)
                    return json
        }

    let getDecodedEntry( key : string ) : Promise<Result<EntryStorage,string>> =
        promise {
            let! e = keyStorage.Get(key)
            // Fable.Core.JS.console.log("getDecodedEntry:" + key + ":type=" + jsTypeOf(e) + ":" + sprintf "%A" e,  e)

            if e = null then
                return (Error ("No entry for key: " + key))
            elif JsInterop.jsTypeof e = "string" then
                return fileEntryFromJSON( unbox e )  None
            else
                let data : ByteArray = unbox e
                let nul = data.indexOf( 0uy )
                if nul < 0 then
                    return fileEntryFromJSON( data |> ByteArray.textDecode ) None
                else
                    let jsonPart = data.slice(0,nul) |> ByteArray.textDecode
                    let dataPart = data.slice(nul+1, data.length)
                    return fileEntryFromJSON jsonPart (Some dataPart)
        }

    let initRoot() =
        promise {
            let! exists = keyStorage.Exists(uidKey 0)

            if not exists then
                do! 
                    let rootE : EntryStorage =
                        { 
                            Name = RootName
                            Uid = RootUid
                            Content = ChildEntries Array.empty
                            Meta = EntryMetaData.Create(Folder) 
                        }
                    rootE |> putEntryUnsafe

            let! rootEntry = getRootKeyJson() // keyStorage.Get(RootKeyName)
            
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
                // let! _ = keyStorage.LogConsistencyCheck()
                // let! _ = keyStorage.FixDanglingReferences()
                // let! _ = keyStorage.FixOrphanedEntries()
                ()
            return ()
        }

    // ------------------------------------------------------------------------
    // Entry primitives, with caching, batch operations and initialization

    let delEntry (e : EntryStorage) =
        promise {
            do! init()
            entryCache.Remove(e.Uid) |> ignore
            do! keyStorage.Remove (uidKey e.Uid)
        }

    let putEntry (e : EntryStorage) =
        promise {
            do! init()
            do! putEntryUnsafe e
        }

    let getEntry (uid : Uid) : PromiseResult<EntryStorage,string> = 
        // Check cache first
        if entryCache.ContainsKey(uid) then
            entryCache.[uid] |> (function Some x -> Ok x | None -> Error "Missing") |> Promise.lift
        else
            promise {
                do! init()

                let! entry = getDecodedEntry (uidKey uid) // keyStorage.Get (uidKey uid)

                // if entry = null then
                //     return (Error ("Not entry for UID: " + string uid))
                // else
                //     match fileEntryFromJSON entry with
                //     | Ok r -> 
                //         // Cache the result
                //         entryCache.[uid] <- Some r
                //         trimCache()
                //         return Ok r
                //     | Error msg ->
                //         entryCache.[uid] <- None
                //         return Error msg


                match entry with
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

    let nameOf (e:EntryStorage) = e.Name

    let getEntries path uid =
        promise {
            let! entry = getEntry uid

            match entry with
            | Error s -> return failwith s // ("Non-existent UID " + string uid)
            // | Ok e when e.Type <> EntryType.Folder -> return failwith (sprintf "Not a folder: entry=%s: %d, path=%s" e.Name uid path)
            | Ok e -> 
                match e.Content with
                | ChildEntries childEntries ->
                    // Batch operation for multiple get operations
                    beginBatch()
                    try
                        let! entries = 
                            childEntries 
                            |> Array.map (fun (a,b) -> getEntry b) 
                            |> Promise.all
                        
                        do! commitBatch()
                        return entries |> Array.map Result.toOption |> Array.choose id
                    with ex ->
                        do! commitBatch()
                        return raise ex
                | _ ->
                    return failwith (sprintf "Not a folder: entry=%s: %d, path=%s" e.Name uid path)
        }

    let hasEntries uid =
        promise {
            let! entry = getEntry uid
            let result = 
                match entry with
                | Error s -> failwith s //("Non-existent UID " + string uid)
                // | Ok e when e.Type <> EntryType.Folder -> failwith (sprintf "Not a folder: %d" uid)
                | Ok e -> 
                    match e.Content with
                    | ChildEntries childEntries -> childEntries.Length > 0
                    | _ -> failwith (sprintf "Not a folder: %d" uid)
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
            |> Promise.map (Result.map (fun e -> match e.Content with ChildEntries ce -> ce | _ -> Array.empty))

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

    let getEntriesWhere (filter: EntryStorage -> bool) (path : string) =
        promise {
            let! uid = 
                path
                |> Internal.canonical
                |> uidOf

            match uid with
            | Ok uid ->
                let! entries = getEntries path uid 
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
                        { entry with Content = entry.Children |> Array.append [| name, uid |] |> ChildEntries }

                    // Fable.Core.JS.console.log("Put parent entry ", path, uid, sprintf "%A" entryType)
                    do! putEntry parentEntry

                    let childEntry =
                        {
                            EntryStorage.Create(match entryType with Folder -> ChildEntries [||] | _ -> TextBlob "") with
                                Uid = uid
                                Name = name
                                Meta = EntryMetaData.Create(entryType)
                        }

                    // Fable.Core.JS.console.log("Put child entry ", path, uid, sprintf "%A" entryType)
                    do! putEntry childEntry

                    // Fable.Core.JS.console.log("Created child entry ", path, uid, sprintf "%A" entryType)
                
                | Error s -> 
                    failwith ("Parent folder does not exist: " + s)
                
                do! commitBatch()
                // Fable.Core.JS.console.log("Batch committed ", path, sprintf "%A" entryType)
                

                if notify then
                    notifyOnChange fname
            with ex ->
                // Don't commit partial batch to avoid dangling references
                // The batch will be automatically discarded
                // Fable.Core.JS.console.error("Creating child entry: ", path, ex.Message)
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

    // let getFileContent(path:string) =
    //     let cpath = path |> Internal.canonical

    //     promise {
    //         do! assertIsFile cpath 

    //         return!
    //             getEntryByPath cpath 
    //             |> Promise.map (fun entry ->
    //                 entry |> Result.map (fun e -> e.Content) |> Result.defaultValue "")
    //     }

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
    let setFileContent(path:string, content:EntryContent) =
        let cpath = path |> Internal.canonical

        promise {
            beginBatch()
            try
                do! assertFalse(isFolder cpath) ("Cannot set contents of a folder: " + cpath)

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
                                        Content = 
                                        children |>
                                        Array.filter (fun (name,uid) -> name <> fileName) |> ChildEntries                                        
                                    }

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

    let rec removeDeep (path : string) : Promise<unit> =
        promise {
            let! entry = getEntryByPath path

            match entry with
            | Ok e ->

                if e.Type = EntryType.Folder then
                    let! childFolders = getEntriesWhere (fun e -> e.Type = Folder) path
                    let! childFiles   = getEntriesWhere (fun e -> e.Type = File) path
                    let! _ =  childFolders |> Array.map (fun name -> removeDeep (Path.combine path name)) |> Promise.all
                    let! _ =  childFiles |> Array.map (fun name -> removeFile (Path.combine path name)) |> Promise.all
                    ()
                    
                do! removeFile path

            | _ -> return ()
        }

    // Optimized with batch operations
    let renameFile(path : string, newNameOrPath : string) =
        promise {
            beginBatch()
            try
                let cpath = path |> Internal.canonical

                let npath = newNameOrPath |> Internal.canonical
                    // if newNameOrPath.StartsWith("/") then
                    //     newNameOrPath |> Internal.canonical
                    // else
                    //     validateFileName newNameOrPath
                    //     Path.combine (Path.getFolderName cpath) newNameOrPath

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
                                        {   
                                            parentEntry 
                                                with Content = 
                                                        parentEntry.Children 
                                                        |> Array.map (fun (name, uid) -> if name = cname then (nname, uid) else (name, uid) ) 
                                                        |> ChildEntries
                                        }
                                        |> putEntry

                                    do! { entry with Name = nname } |> putEntry
                                    return Ok ()
                                }
                            else
                                nparent
                                |> getEntryByPath
                                >>= fun destParentEntry ->
                                    promise {
                                        do! { parentEntry with Content = parentEntry.Children |> Array.filter (fun (name, _) -> name <> cname ) |> ChildEntries}
                                            |> putEntry

                                        do! { destParentEntry with Content = Array.append destParentEntry.Children [| nname, entry.Uid |] |> ChildEntries }
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

    let getLock(name:string) : Promise<unit> =
        // Fable.Core.JS.console.log("getLock: ", name)
        let p = Promise.create( fun resolve _ -> lockClients.Add(resolve) )
        nextLockClient()
        p

    let releaseLock(name:string) : unit =
        // Fable.Core.JS.console.log("releaseLock: ", name)
        isLocked <- false
        nextLockClient()

    let mutable _lockid = 0

    let mkResult (name: string) (f : unit -> Promise<'t>) = 
        let _id = _lockid
        _lockid <- _lockid + 1
        promise {
            do! getLock(name + string _lockid)
            // Fable.Core.JS.console.log(" - gained lock: ", name + string _lockid)
            try 
                return! f()
            finally
                releaseLock(name + string _lockid)
        }

    let mutable handlerIds = 0
    let handlers = System.Collections.Generic.Dictionary<int, string -> unit>()


with
    member this.Initialise() = initRoot()

    // member this.LogCheckConsistency() =
    //     keyStorage.LogConsistencyCheck()

    // member this.FixDanglingReferences() = 
    //     promise {
    //         SutilOxide.Log.Trace.log("Starting dangling reference cleanup...")
    //         let! fixedCount = keyStorage.FixDanglingReferences()
    //         SutilOxide.Log.Trace.log(sprintf "Dangling reference cleanup completed. Fixed %d references." fixedCount)
    //         return fixedCount
    //     }

    member this.GetEntry(path: string): AsyncPromise<Entry option> = 
        promise {
            let! e = getEntryByPath path
            match e with
            | Ok e -> 
                return Some { Name = e.Name; Meta = e.Meta }
            | Error s -> 
                // Fable.Core.JS.console.log("GetEntry: Non-existent: ", path)
                return None
        }   

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

    member this.GetContent(path: string): AsyncPromise<Content option> = 
        promise {
            let! e = getEntryByPath path
            // Fable.Core.JS.console.log("GetContent:" + path + ":" + sprintf("%A") e, e)
            match e with
            | Ok entryStorage ->
                match entryStorage.Content with
                | TextBlob s -> 
                    return Some (Content.Bytes (s |> ByteArray.textEncode))
                | ByteBlob s -> 
                    return Some (Content.Bytes s)
                | ChildEntries childEntries ->
                    let! childrenResults = 
                        childEntries 
                        |> Array.map (fun (name, uid) -> getEntry uid) 
                        |> Promise.all
                    let children = childrenResults |> Array.choose (function Ok entry -> Some { Name = entry.Name; Meta = entry.Meta } | _ -> None)
                    return Some (Content.Entries children)
            | Error s ->
                return None

        }

    interface IFsAsync
    
    interface IFileSystemAsync with

        member this.GetContent(path: string): AsyncPromise<Content option> = 
            (fun _ -> this.GetContent path) |> mkResult "GetContent"

        member this.GetEntry(path: string): AsyncPromise<Entry option> =
            (fun _ -> this.GetEntry path) |> mkResult "GetEntry"

        member this.RemoveEntry(path: string): AsyncPromise<unit> = 
            (fun _ -> removeDeep path) |> mkResult "RemoveEntry"

        member this.RenameEntry(path: string, newPath: string): AsyncPromise<unit> = 
            (fun _ -> renameFile(path, newPath)) |> mkResult "RenameEntry"

        member this.WriteEntry(path: string, content: Content): AsyncPromise<unit> = 
            match content with 
            | Content.Bytes data -> 
                mkResult "SetFileContent" <| fun () -> setFileContent(path, (ByteBlob data))

            | Content.Entries empty ->
                if empty.Length <> 0 then
                    failwithf "Entries must be an empty array for folder creation: %s" path
                else
                    mkResult "CreateFolder" <| fun () -> createFolder path true

        member this.GetEntryBatch(path: string array): AsyncPromise<Entry option array> = 
            (fun _ -> path |> Array.map this.GetEntry |> Promise.all) |> mkResult "GetEntryBatch"

        member this.GetContentBatch(path: string array): AsyncPromise<Content option array> = 
            (fun _ -> path |> Array.map this.GetContent |> Promise.all) |> mkResult "GetContentBatch"

        member __.OnChanged (cb : string -> unit) = __.OnChange(cb)

    interface System.IDisposable with
        member __.Dispose() =
            cleanupResources() |> Promise.start