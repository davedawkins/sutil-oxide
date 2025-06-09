module SutilOxide.FileSystemExt

open SutilOxide.FileSystem
open SutilOxide.PromiseResult
open SutilOxide.MountedFileSystem

let private mkAsyncPFromSync( fs : IFileSystem ) : IFileSystemAsyncP =

    let inline mkAsyncP( value : unit -> SyncThrowable<'t> ) : AsyncPromise<'t> = 
        promise {
            return value().UnsafeValue
        }

    { new IFileSystemAsyncP with
          member _.CreateFile(arg1: string, arg2: string) = 
            mkAsyncP (fun () -> fs.CreateFile(arg1, arg2))

          member _.CreateFolder(arg1: string) = 
            mkAsyncP (fun () -> fs.CreateFolder(arg1))

          member _.Exists(path: string) = 
            mkAsyncP (fun () -> fs.Exists(path))

          member _.Files(path: string) = 
            mkAsyncP (fun () -> fs.Files(path))

          member _.Folders(path: string) = 
            mkAsyncP (fun () -> fs.Folders(path))

          member _.GetFileContent(path: string)  = 
            mkAsyncP (fun () -> fs.GetFileContent(path))

          member _.GetCreatedAt (path: string): AsyncPromise<FsDateTime> = 
            mkAsyncP (fun () -> fs.GetCreatedAt(path))

          member _.GetModifiedAt (path: string): AsyncPromise<FsDateTime> = 
            mkAsyncP (fun () -> fs.GetModifiedAt(path))

          member _.IsFile(path: string) = 
            mkAsyncP (fun () -> fs.IsFile(path))

          member _.IsFolder(path: string) = 
            mkAsyncP (fun () -> fs.IsFolder(path))

          member _.OnChange(arg1: string -> unit)  = 
            mkAsyncP (fun () -> fs.OnChange(arg1))

          member _.RemoveFile(path: string) = 
            mkAsyncP (fun () -> fs.RemoveFile(path))

          member _.RenameFile(arg1: string, arg2: string) = 
            mkAsyncP (fun () -> fs.RenameFile(arg1, arg2))

          member _.SetFileContent(arg1: string, arg2: string) = 
            mkAsyncP (fun () -> fs.SetFileContent(arg1, arg2))
    }




[<AutoOpen>]
module Extensions = 

    // type IFileSystemAsyncR with
    //     member __.GetAsyncP() : IFileSystemAsyncP =
    //         mkAsyncPFromAsync __

    module Promise =

        let ifThen ( cond : Promise<bool>) (action : unit -> Promise<'T>) (defaultValue: unit -> 'T) =
            promise {
                let! c = cond
                if c then return! action() else return defaultValue()
            }

    type IFileSystemAsyncP with

        member __.DoIfExists( path : string, action : unit -> Promise<unit> ) =
            Promise.ifThen (__.Exists path) action (fun _ -> ())

        member __.DoIfNotExists( path : string, action : unit -> Promise<unit> ) =
            Promise.ifThen (__.Exists path |> Promise.map not) action (fun _ -> ())

        member __.IfExists( path : string, action : unit -> Promise<'T>, elseWith : unit -> 'T ) =
            Promise.ifThen (__.Exists path) action elseWith

        member __.IfNotExists( path : string, action : unit -> Promise<'T>, elseWith : unit -> 'T  ) =
            Promise.ifThen (__.Exists path |> Promise.map not) action elseWith

        member __.FilesRecursiveWith( path : string, filter : string -> bool, content : string -> Promise<'a> ) =
            promise {
                let! filtered = __.FilesRecursive path |> Promise.map (Array.filter filter)

                return!
                    filtered 
                    |> Array.map (fun file -> 
                        promise {
                            let! cont = content file
                            return file, cont
                        }
                    )
                    |> Promise.all
            }

        member __.FilesRecursiveWithContent( path : string, filter : string -> bool ) =
            __.FilesRecursiveWith( path, filter, __.GetFileContent )

        member __.EnsureFolder( path : string ) =
            promise {
                let! exists = __.Exists path
                let! isFolder = __.IsFolder path
                match exists, isFolder with
                | false, _ -> do! __.CreateFolder path
                | true, true -> return ()
                | true, false -> failwithf "Not a folder: %s" path
            }
                        
        member __.CopyFile( src : string, tgt : string ) =
            promise {
                let! srcExists = __.Exists src
                if srcExists then
                    let! tgrExists = __.Exists tgt
                    if tgrExists then
                        failwith ("Cannot copy, file exists: " + tgt)
                    let! item = __.GetFileContent src
                    do! __.SetFileContent(tgt,item)
                else
                    failwith ("File does not exist: " + src)
            }

        member __.Remove( path : string ) =
            promise {
                let! isFolder = __.IsFolder path

                if isFolder then
                    let! files = __.Files(path)

                    for name in files do
                        do! __.RemoveFile( Path.combine path name )                        

                    let! folders = __.Folders path
                    for name in folders do
                        do! __.Remove (Path.combine path name)

                do! __.RemoveFile( path )
            }

        member __.CreateFolderRecursive (path : string) =
            promise {
                let! pathIsFile = __.IsFile path
                if pathIsFile then
                    failwith ("File exists: " + path)
                else 
                    let! pathIsFolder = __.IsFolder path
                    if pathIsFolder then
                        ()
                    else
                        match Path.getFolderName path with 
                        | "" -> ()
                        | parent -> 
                            do! __.CreateFolderRecursive parent
                        do! __.CreateFolder path
            }

        member fs.Copy( src : string, dst : string ) =
    
            let copyFileFs path (fs : IFileSystemAsyncP) (fs2 : IFileSystemAsyncP) =
                promise {
                    do! fs2.CreateFolderRecursive (Path.getFolderName path)
                    let! content = fs.GetFileContent path
                    do! fs2.SetFileContent( path, content )
                }

            let copyFolderFs path (fs : IFileSystemAsyncP) (fs2 : IFileSystemAsyncP) =
                fs.FilesRecursive path 
                    |> Promise.bind (Array.map (fun file -> copyFileFs file fs fs2)>>Promise.all )  
                    |> Promise.map ignore

            promise {
                let! isFolder = fs.IsFolder src
                if isFolder then
                    let! isFile = fs.IsFile dst
                    if isFile then 
                        failwith "Attempt to copy folder to file"

                    let! isDstFolder = fs.IsFolder dst
                    let dst = if isDstFolder then Path.combine dst (Path.getFileName src) else dst

                    let! dstExists = fs.Exists dst
                    if dstExists then   
                        failwith ("File/folder exists: " + dst)

                    if src.StartsWith dst || dst.StartsWith src then
                        failwith "Attempt to copy folder to itself"

                    do! fs.CreateFolder dst

                    let dstFs = fs.Mount(dst) // MountedFileSystem(fs, dst)
                    let srcFs = fs.Mount(src) //MountedFileSystem(fs, src)

                    do! copyFolderFs "/" srcFs dstFs
                else
                    let! isDstFolder = fs.IsFolder dst

                    let dst = if isDstFolder then Path.combine dst (Path.getFileName src) else dst
                    do! fs.CopyFile( src, dst )
            }

        member __.FoldersRecursive (path : string) : Promise<string []> =
            promise {
                let! isFolderPath = __.IsFolder path
                if not isFolderPath then
                    return Array.empty
                else
                    let! folders = __.Folders path

                    let! nestedResults =
                        folders
                        |> Array.map (fun name ->
                            promise {
                                let p = Path.combine path name
                                let! subfolders = __.FoldersRecursive p
                                return Array.append [| p |] subfolders
                            })
                        |> Promise.all

                    return nestedResults |> Array.collect id
            }

        member __.FilesRecursive (path : string) : Promise<string []> =
            promise {
                let! isFolderPath = __.IsFolder path
                if not isFolderPath then
                    return Array.empty
                else
                    let! folders = __.Folders path
                    let! files = __.Files path

                    let! nestedResults =
                        files
                        |> Array.append folders
                        |> Array.map (fun name ->
                            promise {
                                let p = Path.combine path name
                                let! isFile = __.IsFile p
                                if isFile then
                                    return [| p |]
                                else
                                    return! __.FilesRecursive p
                            })
                        |> Promise.all

                    return nestedResults |> Array.collect id
            }

        
    type IReadOnlyFileSystem with

        member __.FilesRecursive (path : string) : string []=            
            if (__.IsFolder path).UnsafeValue then
                Array.append (__.Folders path).UnsafeValue (__.Files path).UnsafeValue
                |> Array.collect ( fun f -> 
                    let p = Path.combine path f
                    if (__.IsFile p).UnsafeValue then
                        [| p |]
                    else
                        __.FilesRecursive p
                )
            else
                Array.empty

        member __.FoldersRecursive (path : string) : string [] =
            if __.IsFolder path |> _.UnsafeValue then
                __.Folders path |> _.UnsafeValue
                |> Array.collect (fun name ->
                        let p = Path.combine path name
                        Array.append [| p |] (__.FoldersRecursive p)
                )
            else
                Array.empty

    type IFileSystem with

        /// Wraps the non-async methods so that this instance can be passed to an 
        /// API that wants IFileSystemAsyncR
        // member __.GetAsyncR() : IFileSystemAsyncR =
        //     mkAsyncFromSync __

        member __.GetAsyncP() : IFileSystemAsyncP =
            mkAsyncPFromSync __

        member __.CreateFolderRecursive (path : string) : unit =
            if (__.IsFile path).UnsafeValue then
                failwith ("File exists: " + path)
            else if (__.IsFolder path).UnsafeValue then
                ()
            else
                match Path.getFolderName path with 
                | "" -> ()
                | parent -> __.CreateFolderRecursive parent
                __.CreateFolder path |> _.UnsafeValue
                
        member __.CopyFile( src : string, tgt : string ) : unit =
            if __.Exists src |> _.UnsafeValue then
                if __.Exists tgt |> _.UnsafeValue then
                    failwith ("Cannot copy, file exists: " + tgt)
                let item = __.GetFileContent src |> _.UnsafeValue
                __.SetFileContent(tgt,item) |> _.UnsafeValue
            else
                failwith ("File does not exist: " + src)

        member fs.Copy( src : string, dst : string ) : unit =
            let copyFileFs path (fs : IFileSystem) (fs2 : IFileSystem) : unit =
                fs2.CreateFolderRecursive (Path.getFolderName path)
                fs2.SetFileContent( path, fs.GetFileContent path |> _.UnsafeValue ) |> _.UnsafeValue

            let copyFolderFs path (fs : IFileSystem) (fs2 : IFileSystem) =
                let ifs : IReadOnlyFileSystem = fs
                ifs.FilesRecursive path |> Array.iter (fun file -> copyFileFs file fs fs2)

            if fs.IsFolder src |> _.UnsafeValue then
                if fs.IsFile dst |> _.UnsafeValue then 
                    failwith "Attempt to copy folder to file"

                let dst = if (fs.IsFolder dst).UnsafeValue then Path.combine dst (Path.getFileName src) else dst

                if (fs.Exists dst).UnsafeValue then   
                    failwith ("File/folder exists: " + dst)

                if src.StartsWith dst || dst.StartsWith src then
                    failwith "Attempt to copy folder to itself"

                fs.CreateFolder dst |> _.UnsafeValue

                let dstFs = MountedFileSystem(fs, dst)
                let srcFs = MountedFileSystem(fs, src)

                copyFolderFs "/" srcFs dstFs
            else
                let dst = if (fs.IsFolder dst).UnsafeValue then Path.combine dst (Path.getFileName src) else dst
                fs.CopyFile( src, dst )

        /// Remove file/folder recursively
        member __.Remove( path : string ): unit =
            if (__.IsFolder path ).UnsafeValue then
                __.Files(path) |> _.UnsafeValue
                |> Array.iter( fun name -> __.RemoveFile( Path.combine path name ).UnsafeValue)
                __.Folders(path) |> _.UnsafeValue
                |> Array.iter( fun name -> __.Remove( Path.combine path name ) )
            __.RemoveFile( path ) |> _.UnsafeValue

        member __.MakeUnique( folder : string, basename : string, ?ext : string) =
            let ext' = ext |> Option.defaultValue ""

            let rec findUnique name (i : int) =
                if __.Exists (Path.combine folder name) |> _.UnsafeValue then 
                    findUnique (sprintf "%s%d%s" basename i ext') (i+1)
                else
                    name

            findUnique (basename + ext') 1
