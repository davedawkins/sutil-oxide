module SutilOxide.SubFolderFileSystem

open SutilOxide.FileSystem

type SubFolderFileSystem( fs : IFileSystem, mountPoint : string) =
    let makePath( path : string ) = Path.combine mountPoint path

    let fixPath ( path :string ) = 
        let start = path.IndexOf mountPoint
        path.Substring( start + mountPoint.Length )

    let fixFiles ( files :string[] ) = files

    let fixFolders ( folders :string[] ) = folders 

    let trimSlash (path : string) = path.TrimStart( [| '/' |] )

    let belongs (path : string) =
        (trimSlash path).StartsWith( trimSlash mountPoint )

    interface IFileSystem with
        // member this.CreateFile(path: string ) = 
        //     fs.CreateFile(makePath path )

        member this.CreateFolder(path: string)= 
            fs.CreateFolder(makePath path)

        member this.Exists(path: string) = 
            fs.Exists(makePath path)

        member this.Files(path: string) = 
            fs.Files( makePath path )
            
        member this.Folders(path: string) = 
            fs.Folders( makePath path )

        member this.GetFileContent(path: string) = 
            fs.GetFileContent(makePath path)
            
        member this.GetCreatedAt(path: string) = 
            fs.GetCreatedAt(makePath path)
            
        member this.GetModifiedAt(path: string) = 
            fs.GetModifiedAt(makePath path)
            
        member this.IsFile(path: string) = 
            fs.IsFile( makePath path )
            
        member this.IsFolder(path: string) = 
            fs.IsFolder( makePath path )

        member this.OnChange(callback: string -> unit) = 
            fs.OnChange( fun path -> 
                if belongs path then callback(fixPath path))

        member this.RemoveFile(path: string) = 
            fs.RemoveFile( makePath path )

        member this.RenameFile(src: string, tgt: string) = 
            fs.RenameFile( makePath src, makePath tgt )

        member this.SetFileContent(path: string, content: string) = 
            fs.SetFileContent( makePath path, content)


type SubFolderFileSystemAsyncP( fs : IFileSystemAsyncP, mountPoint : string) =
    let makePath( path : string ) = Path.combine mountPoint path

    let fixPath ( path :string ) = 
        let start = path.IndexOf mountPoint
        path.Substring( start + mountPoint.Length )

    let fixFiles ( files :string[] ) = files

    let fixFolders ( folders :string[] ) = folders 

    let trimSlash (path : string) = path.TrimStart( [| '/' |] )

    let belongs (path : string) =
        (trimSlash path).StartsWith( trimSlash mountPoint )

    interface IFileSystemAsyncP with

        // member this.CreateFile(path: string) = 
        //     fs.CreateFile(makePath path )

        member this.CreateFolder(path: string)  = 
            fs.CreateFolder(makePath path)

        member this.Exists(path: string) = 
            fs.Exists(makePath path)

        member this.Files(path: string)  = 
            promise {
                let! files = fs.Files( makePath path ) 
                return files |> fixFiles
            }

        member this.Folders(path: string) = 
            promise {
                let! folders = fs.Folders( makePath path ) 
                return folders |> fixFolders
            }

        member this.GetFileContent(path: string)  = 
            fs.GetFileContent(makePath path)
            
        member this.GetCreatedAt(path: string) = 
            fs.GetCreatedAt(makePath path)
            
        member this.GetModifiedAt(path: string) = 
            fs.GetModifiedAt(makePath path)
            
        member this.IsFile(path: string) = 
            fs.IsFile( makePath path )
            
        member this.IsFolder(path: string) = 
            fs.IsFolder( makePath path )

        member this.OnChange(callback: string -> unit)  = 
            fs.OnChange( fun path -> 
                if belongs path then callback(fixPath path))

        member this.RemoveFile(path: string)  = 
            fs.RemoveFile( makePath path )

        member this.RenameFile(src: string, tgt: string)  = 
            fs.RenameFile( makePath src, makePath tgt )
        member this.SetFileContent(path: string, content: string)  = 
            fs.SetFileContent( makePath path, content)


type MountHostSystemAsyncP( fs : IFileSystemAsyncP, mountPoint : string, mountFs : IFileSystemAsyncP ) =
    // let makePath( path : string ) = Path.combine mountPoint path

    // let fixPath ( path :string ) = 
    //     let start = path.IndexOf mountPoint
    //     path.Substring( start + mountPoint.Length )

    // let fixFolders ( folders :string[] ) = folders 

    // let trimSlash (path : string) = path.TrimStart( [| '/' |] )

    // let belongs (path : string) =
    //     (trimSlash path).StartsWith( trimSlash mountPoint )


    let _p (path : string) = Path.combine "" path

    let isMount (path : string) = 
        let path = _p path
        path.StartsWith mountPoint

    // let fixFiles path ( files :string[] ) = 
    //     if isMount path then files |> Array.map (Path.combine mountPoint) else files

    let getPath (path : string) = 
        let path = _p path
        if isMount path then path.Substring(mountPoint.Length) else path

    let getFs (path : string) = 
        let path = _p path
        if isMount path then mountFs else fs


    interface IFileSystemAsyncP with

        // member this.CreateFile(path: string) = 
        //     fs.CreateFile(makePath path )

        member this.CreateFolder(path: string)  = 
            (getFs path).CreateFolder(getPath path)

        member this.Exists(path: string) = 
            Fable.Core.JS.console.log("Exists: " + path + " -> " + getPath(path))
            (getFs path).Exists(getPath path)

        member this.Files(path: string)  = 
            promise {
                let! files = (getFs path).Files( getPath path ) 
                return files //|> fixFiles path
            }

        member this.Folders(path: string) = 
            promise {
                let! folders = (getFs path).Folders( getPath path ) 
                return folders //|> fixFiles path
            }

        member this.GetFileContent(path: string)  = 
            Fable.Core.JS.console.log("GetFileContent: " + path + " -> " + getPath(path))
            (getFs path).GetFileContent(getPath path)
            
        member this.GetCreatedAt(path: string) = 
            (getFs path).GetCreatedAt(getPath path)
            
        member this.GetModifiedAt(path: string) = 
            (getFs path).GetModifiedAt(getPath path)
            
        member this.IsFile(path: string) = 
            Fable.Core.JS.console.log("IsFile: " + path + " -> " + getPath(path))
            (getFs path).IsFile( getPath path )
            
        member this.IsFolder(path: string) = 
            Fable.Core.JS.console.log("IsFolder: " + path + " -> " + getPath(path))
            (getFs path).IsFolder( getPath path )

        member this.OnChange(callback: string -> unit)  = 
            promise {
                let! d1 = fs.OnChange( fun path -> callback(path) )
                let! d2 = mountFs.OnChange( fun path -> callback( Path.combine mountPoint path) )
                return { new System.IDisposable with member __.Dispose() = d1.Dispose(); d2.Dispose() }
            }

        member this.RemoveFile(path: string)  = 
            (getFs path).RemoveFile( getPath path )

        member this.RenameFile(src: string, tgt: string)  = 
            (getFs src).RenameFile( getPath src, getPath tgt )

        member this.SetFileContent(path: string, content: string)  = 
            (getFs path).SetFileContent( getPath path, content)

[<AutoOpen>]
module SubFolderFileSystemExt = 

    type IFileSystemAsyncP with

        member __.MakeRoot( path : string ) : IFileSystemAsyncP =
            SubFolderFileSystemAsyncP( __, path )

        member __.Mount( path : string, mountFs : IFileSystemAsyncP ) =
            new MountHostSystemAsyncP( __, path, mountFs )