module SutilOxide.FileExplorer

//
// Copyright (c) 2022 David Dawkins
//

open System

open Browser.Dom
open Browser.Types
open Thoth.Json
open FileSystem
open Sutil
open Sutil.Core
open Sutil.CoreElements
open Sutil.Styling

open type Feliz.length
open PromiseResult

type UI =
    static member divc (cls:string) (items : seq<SutilElement>) =
        Html.div [ Attr.className cls ; yield! items ]

type Msg =
    | SelectPath of string
    | SetSelected of string
    | DeleteSelected
    | NewFile
    | SetError of Exception
    | ClearError
    | SetRenaming of bool
    | Created of string
    | RenameTo of string
    | Edit of string
    | Refresh
    | FetchListing
//    | TestIsFolder of string
    | SetCwdForce of string
    // | SetCwd of string
    | SetListing of (string[] * string[])

type SessionState = {
    Cwd : string
    Selected : string
    Editing : string
}

type Model = {
    RefreshId : int // Change this to force a redraw
    Cwd : string
    Fs : IFsAsync
    Selected : string
    Renaming : bool
    Error : Exception option
    Editing : string
    Files : string[]
    Folders : string[]
}

// let saveSessionState (m : Model) =
//     window.localStorage.setItem("file-explorer-session", Encode.Auto.toString { Cwd = m.Cwd; Selected = m.Selected; Editing = m.Editing})

// let loadSessionState() =
//     match (window.localStorage.getItem("file-explorer-session")) with
//     | null -> None
//     | s -> 
//         match Decode.Auto.fromString<SessionState>(s) with
//         | Ok r -> r |> Some
//         | Error _ -> None

let defaultSessionState() = {
    Cwd = "/"
    Selected = ""
    Editing = ""
}

let init (fs : IFsAsync, s : SessionState) =
    {
        RefreshId = 0
        Fs = fs
        Cwd = s.Cwd
        Selected = s.Selected
        Files = Array.empty
        Folders = Array.empty
        Error = None
        Renaming = false
        Editing = s.Editing
    }, 
        Cmd.batch [
            Cmd.ofMsg FetchListing
            if s.Editing <> "" then Cmd.ofMsg (Edit s.Editing) else Cmd.none
        ]

let fetchListing (fs : IFsAsync) (path : string) : Promise<string[]*string[]> =
    promise {
        let! exists =
            fs.IsFolder path
        if not exists then failwith ("Not a folder: " + path)
        let! files = fs.Files path
        let! folders = fs.Folders path
        return files, folders
    }

let update edit msg model =
//    Fable.Core.JS.console.log(sprintf "FileExplorer: %A" msg)
    match msg with

    | SetListing (files, folders) ->
        let selected = 
            if model.Selected = "" then
                ""
            else
                let name = Path.getFileName model.Selected
                if files |> Array.contains name || folders |> Array.contains name then
                    model.Selected
                else 
                    ""

        { model with Files = files; Folders = folders }, 
            Cmd.batch [
                Cmd.ofMsg Refresh
                Cmd.ofMsg (SetSelected selected)
            ]

    | FetchListing ->
        model, Cmd.OfPromise.either (fetchListing model.Fs) (model.Cwd) SetListing SetError

    | Refresh -> { model with RefreshId = model.RefreshId + 1 }, Cmd.none

    | SetCwdForce d ->
        { model with Cwd = d }, Cmd.ofMsg FetchListing

    // | SetCwd d ->
    //     model, Cmd.OfPromise.either (model.Fs.IsFolder) d (fun isFolder -> if isFolder then SetCwdForce d else (SetError (System.Exception ("Not a folder: " + d)))) (SetError)

    | Edit name ->
        if (name <> "") then
            edit (Path.combine model.Cwd name)
        { model with Editing = name }, Cmd.none

    | RenameTo name ->
        let tryRename () =
            promise {
                if (model.Selected <> "") then
                    do! model.Fs.RenameFile( model.Selected, name )
            }
        { model with Renaming = false }, Cmd.OfPromise.either tryRename () (fun _ -> SetSelected model.Selected) SetError

    | SetRenaming z ->
        { model with Renaming = z }, Cmd.none

    | ClearError ->
        { model with Error = None }, Cmd.none

    | SetError x ->
        { model with Error = Some x }, Cmd.none

    | SetSelected s ->
        { model with Selected = s; Renaming = false}, Cmd.none

    | SelectPath path ->
        let folder = Path.getFolderName(path)
        if folder = model.Cwd then
            model, Cmd.ofMsg (SetSelected path)
        else
            model, 
                Cmd.batch [
                    Cmd.ofMsg (SetSelected path)
                    Cmd.ofMsg (SetCwdForce (Path.getFolderName path))
                ]

    | DeleteSelected ->
        let tryDelete() =
            let fs = model.Fs

            promise {
                if model.Selected = "" then return ()

                let path = model.Selected
                let! isFile = fs.IsFile path  

                if isFile  then
                    do! fs.RemoveFile path
                else
                    let! isFolder = fs.IsFolder path  
                    if isFolder then
                        let! files = fs.Files path  
                        let! isEmpty = fs.Files path |> Promise.map (fun a -> a.Length = 0)

                        if isEmpty then
                            do! model.Fs.RemoveFile path
                        else
                            failwith ("Folder is not empty")
                    else
                        failwith ("Not a file: " + path)
            }

        model, Cmd.OfPromise.either tryDelete () (fun _ -> ClearError) SetError

    | Created name ->
        model, Cmd.batch [ Cmd.ofMsg (SetSelected name); Cmd.ofMsg (SetRenaming true) ]

    | NewFile ->

        let tryCreate() =
            promise {
                let name = "NewFile.md"
                do! model.Fs.SetFileContent( Path.combine (model.Cwd) name, "" )
                return Path.combine model.Cwd  name
            }

        model, Cmd.OfPromise.either tryCreate () Created SetError

let updateWithSaveSession edit msg model =
    let result = update edit msg model
    //saveSessionState (fst result)
    result

let css = [
    rule ".file-explorer" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.gap (rem 0.5)
        Css.userSelectNone
    ]

    rule ".file-explorer-buttons" [
        Css.displayFlex
        Css.flexDirectionRow
        Css.gap (rem 0.15)
    ]

    rule ".file-explorer-entries" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.gap (rem 0.2)
        Css.fontSize (px 12)
    ]

    rule ".fx-folder" [
        Css.cursorPointer
        Css.padding (px 2)
        Css.paddingLeft (rem 0.5)
    ]

    rule ".fx-file" [
        Css.cursorPointer
        Css.padding (px 2)
        Css.paddingLeft (rem 0.5)
    ]

    rule ".file-explorer-entries .selected" [
        Css.backgroundColor "#DDDDDD"
    ]

    rule ".file-explorer i" [
        Css.displayInlineBlock
        Css.width (rem 1.2)
//        Css.color ("#888888")
    ]

    rule ".file-explorer .dragging" [
        Css.opacity 0.5
    ]

]
let buttons m dispatch=
    UI.divc "file-explorer-buttons" [
        Html.button [
            text "Up"
            Ev.onClick (fun _ -> ())
        ]
        Html.button [
            text "New File"
            Ev.onClick (fun _ -> dispatch NewFile)
        ]
        Html.button [
            text "New Folder"
            Ev.onClick (fun _ -> ())
        ]
        Html.button [
            text "Rename"
            Ev.onClick (fun _ -> dispatch (SetRenaming true))
        ]
        Html.button [
            text "Edit"
            Ev.onClick (fun _ -> dispatch (Edit m.Selected))
        ]
        Html.button [
            text "Delete"
            Ev.onClick (fun _ -> dispatch DeleteSelected)
        ]
    ]

let isRoot f = f = "/" || f = ""

let fileExplorer (classifier : string -> string) iconselector dispatch (m : Model) =

    let cwd = m.Cwd
    let fs = m.Fs

    let icon path def =
        match iconselector path with
        | "" -> def
        | s -> s
        |> UI.Icon.makeFa
        
    UI.divc "file-explorer" [
        //buttons m dispatch
        UI.divc "file-explorer-entries" [

            if (not (isRoot cwd)) then
                let parent = Path.getFolderName(cwd)
                UI.divc ("fx-folder " + classifier "[parent]") [
                    Html.ic (icon "[parent]" "fa-arrow-up") []
                    text "[parent]"
                    Ev.onMouseDown( fun e ->
                        e.stopPropagation()
                        e.preventDefault()
                    )
                    Ev.onMouseUp( fun e ->
                        DomHelpers.rafu (fun _ -> parent |> SetSelected |> dispatch)
                    )
                    Ev.onDblClick (fun e ->
                        e.preventDefault(); 
                        dispatch (SetCwdForce parent)
                    )
                    if m.Selected = parent then
                        Attr.className "selected"
                ]

            m.Folders
                |> Array.sortBy (fun s -> s.ToLower())
                |> Array.map (fun name ->
                    let path = Path.combine cwd name
                    UI.divc ("fx-folder " + classifier path) [
                        Html.ic (icon path "fa-folder") []
                        text name
                        Ev.onMouseDown( fun e ->
                            e.stopPropagation()
                            e.preventDefault()
                        )
                        Ev.onMouseUp( fun e ->
                            DomHelpers.rafu (fun _ -> path |> SetSelected |> dispatch)
                        )
                        Ev.onDblClick (fun e ->
                            e.preventDefault(); 
                            dispatch (SetCwdForce (Path.combine cwd name))
                        )
                        if m.Selected = path then
                            Attr.className "selected"
                    ])
                |> fragment

            m.Files
                |> Array.sortBy (fun s -> s.ToLower())
                |> Array.map (fun name ->
                    let path = Path.combine cwd name
                    if m.Selected = path && m.Renaming then
                        Html.input [
                            autofocus
                            Attr.value name
                            Ev.onKeyDown (fun e ->
                                let value = (e.target :?> HTMLInputElement).value
                                match e.key with
                                | "Enter" ->
                                    if value <> "" && value <> name then
                                        (dispatch (RenameTo value))
                                    else
                                        dispatch (SetRenaming false)
                                | "Escape" ->
                                    dispatch (SetRenaming false)
                                | _ -> ()
                            )
                        ]
                    else
                        UI.divc ("fx-file " + classifier path) [
                            Attr.draggable true

                            Html.ic (icon path "fa-file-o") []
                            Html.spanc "file-name" [ text name ]
                            // Html.spanc "file-created" 
                            //     [ Bind.promise(
                            //         fs.GetCreatedAt(path), (fun dt -> text (dt.ToString())), text "", fun s -> text (s.ToString())
                            //     ) ]

                            // Html.spanc "file-modified" [ text (fs.GetCreatedAt(path).ToString())  ]

                            Ev.onClick (fun e ->
                                DomHelpers.timeout (fun _ -> path |> SetSelected |> dispatch) 10
                                    |> ignore
                            )

                            Ev.onDblClick (fun e ->
                                name |> Edit |> dispatch
                            )



                            // Ev.onDragStart (fun e ->
                            //     (e.target :?> HTMLElement).classList.add "dragging"
                            // )

                            // Ev.onDragEnd( fun e ->
                            //     (e.target :?> HTMLElement).classList.remove "dragging"
                            // )
                            if m.Selected = path then
                                Attr.className "selected"
                ])
                |> fragment
        ]
        UI.divc "file-explorer-error" [
            text (m.Error |> Option.map (fun e -> e.Message) |> Option.defaultValue "")
        ]
    ] |> withStyle css


///
/// Show files and folders, with ability to create, rename, remove
/// 
type FileExplorer( fs : IFsAsync ) =

    let mutable onEdit : string -> unit = ignore

    let create fs =
        //let sessionState = loadSessionState() |> Option.defaultWith defaultSessionState
        (fs,defaultSessionState()) |> Sutil.Store.makeElmish init (updateWithSaveSession (fun f -> onEdit f)) ignore

    let model, dispatch = create fs

    let view  classifier iconselector =
        Bind.el( model, fileExplorer classifier iconselector dispatch )

    let exists (items : string[]) (path : string) =
        items |> Array.exists (fun f -> path = Path.combine (model.Value.Cwd) f)

    do
        fs.OnChanged( fun _ -> dispatch FetchListing ) |> Promise.start

    with
        member _.View( classifier : string -> string, iconselector : string -> string ) = view classifier iconselector 
        member _.OnEdit( h : string -> unit) = onEdit <- h
        member _.Dispatch = dispatch
        member _.Selected = model.Value.Selected
        member _.CurrentFolder = model.Value.Cwd

        member __.SelectPath( path : string ) =
            dispatch (SelectPath path)

        member _.Files = model.Value.Files
        member _.Folders = model.Value.Folders
        member __.ExistsInCwd( path : string ) = 
            exists (__.Files) path || exists (__.Folders) path
        member __.IsFileInCwd( path : string ) = 
            exists (__.Files) path
        member __.IsFolderInCmd( path : string ) = 
            // Fable.Core.JS.console.log("IsFolder: ", path, __.Folders)
            path = "/" || exists (__.Folders) path

