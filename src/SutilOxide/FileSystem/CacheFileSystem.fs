module SutilOxide.CacheFileSystem

open SutilOxide.PromiseResult
open SutilOxide.FileSystem
open SutilOxide.FileSystemExt

type CacheFileSystem( files : string[], folders : string[], ?contents : Map<string,string> ) =
    let fileSet = files |> Set
    let folderSet = folders |> Set

    let mkSyncThrowable( value : unit -> 't ) : SyncThrowable<'t> = SyncThrowable<'t>.Of(value)

    // do
    //     Fable.Core.JS.console.log("-- Cache --", folders, files)

    interface IReadOnlyFileSystem with        
        member this.Exists(path: string) = 
            mkSyncThrowable( fun () -> fileSet.Contains path || folderSet.Contains path )

        member this.Files(path: string) = 
            let run () =
                let path = path |> Internal.canonical |> _.TrimEnd( [| '/' |] )

                files 
                |> Array.map (fun file ->
                    Path.getFolderName file, Path.getFileName file )
                |> Array.filter (fun (dir, name) -> 
                    dir = path )
                |> Array.map (snd) 
            run |> mkSyncThrowable

        member this.Folders(path: string) = 
            let run () =
                let path = path |> Internal.canonical |> _.TrimEnd( [| '/' |] )
                folders 
                |> Array.map (fun file ->
                    Path.getFolderName file, Path.getFileName file )
                |> Array.filter (fun (dir, name) -> 
                    dir = path )
                |> Array.map (snd) 
            run |> mkSyncThrowable

        member this.GetFileContent(path: string) = 
            let run () =
                match contents with
                | Some m -> m[path]
                | None -> failwith "Cache was not populated with file content"
            run |> mkSyncThrowable

        member this.IsFile(path: string) = 
            let run () =
                fileSet.Contains path
            run |> mkSyncThrowable

        member this.IsFolder(path: string) = 
            let run () =
                folderSet.Contains path || path = "/"
            run |> mkSyncThrowable

        member this.OnChange(arg1: string -> unit) = 
            let run () =
                failwith "Not Implemented"
            run |> mkSyncThrowable

[<AutoOpen>]
module CacheFileSystemExt =
    type IFileSystemAsyncP with
        
        member __.Cache() : Promise<IReadOnlyFileSystem> =
            promise {
                let! files = __.FilesRecursive "/"
                let! folders = __.FoldersRecursive "/"
                let fs : IReadOnlyFileSystem = CacheFileSystem(files, folders)
                return fs
            }