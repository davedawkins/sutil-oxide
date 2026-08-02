module SutilOxide.SubFolderFileSystem

open System
open SutilOxide.FileSystem

type SubFolderFileSystemAsync( fs : IFsAsync, mountPoint : string) =
    let makePath( path : string ) = Path.combine mountPoint path

    let fixPath ( path :string ) = 
        let start = path.IndexOf mountPoint
        path.Substring( start + mountPoint.Length )

    let fixFiles ( files :string[] ) = files

    let fixFolders ( folders :string[] ) = folders 

    let trimSlash (path : string) = path.TrimStart( [| '/' |] )

    let belongs (path : string) =
        (trimSlash path).StartsWith( trimSlash mountPoint )

    interface IFsAsync with
        // IWriteOnlyFileSystemOf<AsyncPromise<unit>> members
        member this.WriteEntry(path: string, content: Content): AsyncPromise<unit> =
            fs.WriteEntry(makePath path, content)

        // fsimgo#569: forward, don't loop -- import into a sub-root must keep the underlying batching.
        member this.WriteEntries(entries: (string * Content)[]): AsyncPromise<unit> =
            fs.WriteEntries(entries |> Array.map (fun (path, content) -> makePath path, content))

        member this.RemoveEntry(path: string): AsyncPromise<unit> =
            fs.RemoveEntry(makePath path)

        member this.RenameEntry(src: string, tgt: string): AsyncPromise<unit> =
            fs.RenameEntry(makePath src, makePath tgt)

        // IReadOnlyFileSystemOf members  
        member this.GetEntry(path: string): AsyncPromise<Entry option> =
            fs.GetEntry(makePath path)

        member this.GetContent(path: string): AsyncPromise<Content option> =
            fs.GetContent(makePath path)

        member this.OnChanged(callback: FileSystemEvent -> unit): AsyncPromise<IDisposable> =
            fs.OnChanged(fun ev -> 
                if belongs ev.path then callback( ev.map(fixPath) )
            )

        // IReadOnlyBatchingFileSystemOf members
        member this.GetEntryBatch(paths: string array): AsyncPromise<Entry option array> =
            fs.GetEntryBatch(paths |> Array.map makePath)

        member this.GetContentBatch(paths: string array): AsyncPromise<Content option array> =
            fs.GetContentBatch(paths |> Array.map makePath)


[<AutoOpen>]
module SubFolderFileSystemExt = 
            
    type IFsAsync with

        member __.MakeRoot( path : string ) : IFsAsync =
            SubFolderFileSystemAsync( __, path ) :> IFsAsync
            

type VirtualFileSystem( mounts : (string * IFsAsync) [] ) =
    let mountPoints = Map mounts

    let getMountName (path : string) = 
        path |> FileSystem.Internal.parsePath |> Array.tryHead 
        
    let getMountFs (path : string) = 
        path |> FileSystem.Internal.parsePath |> Array.tryHead |> Option.bind (mountPoints.TryFind)
        
    let pathToInternal ( path :string ) = 
        path |> FileSystem.Internal.parsePath |> Array.skip 1 |> String.concat "/"

    let dispatch1 (path : string) (cmd : IFsAsync -> string -> AsyncPromise<'r>) =
        match getMountFs path with
        | Some fs -> cmd fs (pathToInternal path)
        | None -> failwithf "Path not found: %s" path

    let dispatch2 (path1 : string) (path2 : string) (cmd : IFsAsync -> string -> IFsAsync -> string -> AsyncPromise<'r>) =
        match getMountFs path1, getMountFs path2 with
        | Some fs1, Some fs2 -> cmd fs1 (pathToInternal path1) fs2 (pathToInternal path2)
        | None, _ -> failwithf "Path not found: %s" path1
        | _, None -> failwithf "Path not found: %s" path2

    // carry the caller's index, not the path -- a batch may legitimately repeat a path
    let batchFnOf (f) (fs : IFsAsync option) (indexedPaths : (int * string)[])   =
        match fs with
        | Some fs ->
            f fs ( indexedPaths |> Array.map (snd >> pathToInternal) )
            |> Promise.map (Array.mapi (fun i result -> fst indexedPaths[i], result))
        | None ->
            indexedPaths |> Array.map (fun (i,_) -> i, None) |> Promise.lift

    let orderedBatch (paths : string array) (batchFn) =
        paths
        |> Array.mapi (fun i path -> i, path, (path |> getMountName |> Option.defaultValue ""))
        |> Array.groupBy (fun (_,_,mount) -> mount)
        |> Array.map (fun (mount, group) -> batchFn (getMountFs mount) (group |> Array.map (fun (i,path,_) -> i, path)))
        |> Promise.all
        |> Promise.map (Array.collect id >> Array.sortBy fst >> Array.map snd)

    interface IFsAsync with
        // IWriteOnlyFileSystemOf<AsyncPromise<unit>> members
        member this.WriteEntry(path: string, content: Content): AsyncPromise<unit> =
            dispatch1 path (fun fs path -> fs.WriteEntry(path, content))

        // fsimgo#569: fan out per mount, one batched call each -- don't degrade to per-entry writes.
        member this.WriteEntries(entries: (string * Content)[]): AsyncPromise<unit> =
            entries
            |> Array.groupBy (fun (path,_) -> path |> getMountName |> Option.defaultValue "")
            |> Array.map (fun (mount, group) ->
                match getMountFs mount with
                | Some fs -> group |> Array.map (fun (path, content) -> pathToInternal path, content) |> fs.WriteEntries
                | None -> failwithf "Path not found: %s" (fst group[0]))
            |> Promise.all
            |> Promise.map ignore

        member this.RemoveEntry(path: string): AsyncPromise<unit> =
            dispatch1 path (fun fs path -> fs.RemoveEntry(path))

        // rename is only meaningful within one mount -- across mounts it is a copy-then-remove
        member this.RenameEntry(src: string, tgt: string): AsyncPromise<unit> =
            dispatch2 src tgt (fun fs1 src fs2 tgt ->
                if System.Object.ReferenceEquals(fs1, fs2) then fs1.RenameEntry(src, tgt)
                else failwithf "Cannot rename across filesystems: copy and then remove")

        // IReadOnlyFileSystemOf members  
        member this.GetEntry(path: string): AsyncPromise<Entry option> =
            if path = "" || path = "/" then
                { Name = ""; Meta = { EntryType = EntryType.Folder; CreatedAt = DateTime.MinValue; ModifiedAt = DateTime.MinValue; Size = 0} } |> Some |> Promise.lift
            else
                dispatch1 path (fun fs path -> fs.GetEntry(path))

        member this.GetContent(path: string): AsyncPromise<Content option> =
            if path = "" || path = "/" then
                mounts |> Array.map (fun (name,fs) -> fs.GetEntry("/") |> Promise.map (Option.map (fun e -> { e with Name = name }))) |> Promise.all |> Promise.map (Array.choose id>>Entries>>Some)
            else
                dispatch1 path (fun fs path -> fs.GetContent(path))

        member this.OnChanged(callback: FileSystemEvent -> unit): AsyncPromise<IDisposable> =
            let fixevent (mount : string) (ev : FileSystemEvent) = 
                ev.map (fun p1 -> Path.combine mount p1)

            promise {
                let! disposables = mounts |> Array.map (fun (mount, fs : IFsAsync) -> fs.OnChanged( fun ev -> callback( fixevent mount ev  ))) |> Promise.all
                return { new IDisposable with member _.Dispose() = disposables |> Array.iter (fun d -> d.Dispose()) }
            }
        
        // IReadOnlyBatchingFileSystemOf members
        member this.GetEntryBatch(paths: string array): AsyncPromise<Entry option array> =
            orderedBatch paths (batchFnOf (fun fs paths -> fs.GetEntryBatch(paths)))

        member this.GetContentBatch(paths: string array): AsyncPromise<Content option array> =
            orderedBatch paths (batchFnOf (fun fs paths -> fs.GetContentBatch(paths)))


