module SutilOxide.MountedFileSystem

open SutilOxide.FileSystem

type MountedFileSystem( fs : IFileSystem, mountPoint : string) =
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
        member this.CreateFile(path: string, name: string) = 
            fs.CreateFile( makePath path, name )

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


type MountedFileSystemAsyncP( fs : IFileSystemAsyncP, mountPoint : string) =
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

        member this.CreateFile(path: string, name: string) = 
            fs.CreateFile( makePath path, name )

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


[<AutoOpen>]
module MountedFileSystemExt = 

    type IFileSystemAsyncP with

        member __.Mount( path : string ) : IFileSystemAsyncP =
            MountedFileSystemAsyncP( __, path )
