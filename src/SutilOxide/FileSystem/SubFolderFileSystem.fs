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

        member this.RemoveEntry(path: string): AsyncPromise<unit> =
            fs.RemoveEntry(makePath path)

        member this.RenameEntry(src: string, tgt: string): AsyncPromise<unit> =
            fs.RenameEntry(makePath src, makePath tgt)

        // IReadOnlyFileSystemOf members  
        member this.GetEntry(path: string): AsyncPromise<Entry option> =
            fs.GetEntry(makePath path)

        member this.GetContent(path: string): AsyncPromise<Content option> =
            fs.GetContent(makePath path)

        member this.OnChanged(callback: string -> unit): AsyncPromise<IDisposable> =
            fs.OnChanged(fun path -> 
                if belongs path then callback(fixPath path))

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