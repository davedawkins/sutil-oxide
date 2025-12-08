module SutilOxide.FileSystemExt

open SutilOxide.FileSystem
open SutilOxide.PromiseResult
open SutilOxide.SubFolderFileSystem

[<AutoOpen>]
module Extensions = 

    module Promise =

        let ifThen ( cond : Promise<bool>) (action : unit -> Promise<'T>) (defaultValue: unit -> 'T) =
            promise {
                let! c = cond
                if c then return! action() else return defaultValue()
            }

    type IFsAsync with

        member __.DoIfExists( path : string, action : unit -> Promise<unit> ) =
            Promise.ifThen (__.Exists path) action (fun _ -> ())

        member __.DoIfNotExists( path : string, action : unit -> Promise<unit> ) =
            Promise.ifThen (__.Exists path |> Promise.map not) action (fun _ -> ())

        member __.IfExists( path : string, action : unit -> Promise<'T>, elseWith : unit -> 'T ) =
            Promise.ifThen (__.Exists path) action elseWith

        member __.IfNotExists( path : string, action : unit -> Promise<'T>, elseWith : unit -> 'T  ) =
            Promise.ifThen (__.Exists path |> Promise.map not) action elseWith

        member __.GetFileTextIfExists( path : string, defaultValue : string ) =
            Promise.ifThen (__.Exists path) (fun _ -> __.GetFileText(path)) (fun _ -> defaultValue)

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

        member __.ResetFolder( path : string ) =
            promise {
                do! __.DoIfExists( 
                        path, 
                        fun _ -> __.Remove(path)
                )
                do! __.CreateFolder(path)
            }

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

        member fs.Copy( src : string, dst : string ) =
    
            let copyFileFs path (fs : IFsAsync) (fs2 : IFsAsync) =
                promise {
                    // do! fs2.CreateFolderRecursive (Path.getFolderName path)
                    let! content = fs.GetFileContent path
                    do! fs2.SetFileContent( path, content )
                }

            let copyFolderFs path (fs : IFsAsync) (fs2 : IFsAsync) =
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

                    // if src.StartsWith dst || dst.StartsWith src then
                    //     failwith "Attempt to copy folder to itself"

                    do! fs.CreateFolder dst

                    let dstFs = fs.MakeRoot(dst) // SubFolderFileSystem(fs, dst)
                    let srcFs = fs.MakeRoot(src) //SubFolderFileSystem(fs, src)

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

        member __.Entries (path : string) : Promise<string []> =
            promise {
                let! isFolderPath = __.IsFolder path
                if not isFolderPath then
                    return Array.empty
                else
                    let! folders = __.Folders path
                    let! files = __.Files path

                    return
                        files
                        |> Array.append folders
            }

        member __.FilesRecursive (path : string) : Promise<string []> =
            promise {
                let! isFolderPath = __.IsFolder path
                if not isFolderPath then
                    return Array.empty
                else
                    let! folders = __.Folders path
                    // Fable.Core.JS.console.log("Folders:path:" + path + ":", folders)
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
