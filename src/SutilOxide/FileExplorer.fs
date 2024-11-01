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

type UI =
    static member divc (cls:string) (items : seq<SutilElement>) =
        Html.div [ Attr.className cls ; yield! items ]

type Msg =
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
    | SetCwd of string

type SessionState = {
    Cwd : string
    Selected : string
    Editing : string
}

type Model = {
    RefreshId : int // Change this to force a redraw
    Cwd : string
    Fs : IFileSystem
    Selected : string
    Renaming : bool
    Error : Exception option
    Editing : string
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

let init (fs : IFileSystem, s : SessionState) =
    {
        RefreshId = 0
        Fs = fs
        Cwd = s.Cwd
        Selected = s.Selected
        Error = None
        Renaming = false
        Editing = s.Editing
    }, if s.Editing <> "" then Cmd.ofMsg (Edit s.Editing) else Cmd.none

let update edit msg model =
    Fable.Core.JS.console.log (sprintf "update %A selected=%s" msg (model.Selected))
    match msg with
    | Refresh -> { model with RefreshId = model.RefreshId + 1 }, Cmd.none
    | SetCwd d -> { model with Cwd = d }, Cmd.none
    | Edit name ->
        if (name <> "") then
            edit (IFileSystem.Combine(model.Cwd, name))
        { model with Editing = name }, Cmd.none

    | RenameTo name ->
        let tryRename () =
            if (model.Selected <> "") then
                model.Fs.RenameFile( model.Selected, name )
        { model with Renaming = false }, Cmd.OfFunc.either tryRename () (fun _ -> SetSelected model.Selected) SetError

    | SetRenaming z ->
        { model with Renaming = z }, Cmd.none

    | ClearError ->
        { model with Error = None }, Cmd.none

    | SetError x ->
        { model with Error = Some x }, Cmd.none

    | SetSelected s ->
        { model with Selected = s; Renaming = false}, Cmd.none

    | DeleteSelected ->
        let tryDelete() =
            if model.Selected <> "" then
                let path = model.Selected
                if model.Fs.IsFile path then
                    model.Fs.RemoveFile path
                elif model.Fs.IsFolder path then
                    if (model.Fs.Files path).Length = 0 then
                        model.Fs.RemoveFile path
                    else
                        failwith ("Folder is not empty")
                else
                    failwith ("Not a file: " + path)

        model, Cmd.OfFunc.either tryDelete () (fun _ -> ClearError) SetError

    | Created name ->
        model, Cmd.batch [ Cmd.ofMsg (SetSelected name); Cmd.ofMsg (SetRenaming true) ]

    | NewFile ->

        let tryCreate() =
            let name = "NewFile.md"
            model.Fs.CreateFile( model.Cwd, name )
            IFileSystem.Combine( model.Cwd, name )

        model, Cmd.OfFunc.either tryCreate () Created SetError

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

    UI.divc "file-explorer" [
        //buttons m dispatch
        UI.divc "file-explorer-entries" [

            if (not (isRoot cwd)) then
                let parent = IFileSystem.GetFolderName(cwd)
                UI.divc ("fx-folder " + classifier "[parent]") [
                    Html.ic (icon "[parent]" "fa fa-arrow-up") []
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
                        dispatch (SetCwd parent)
                    )
                    if m.Selected = parent then
                        Attr.className "selected"
                ]

            fs.Folders(cwd)
                |> Array.map (fun name ->
                    let path = IFileSystem.Combine(cwd, name)
                    UI.divc ("fx-folder " + classifier path) [
                        Html.ic (icon path "fa fa-folder") []
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
                            dispatch (SetCwd (IFileSystem.Combine (cwd, name)))
                        )
                        if m.Selected = path then
                            Attr.className "selected"
                    ])
                |> fragment

            fs.Files(cwd)
                |> Array.sortBy (fun s -> s.ToLower())
                |> Array.map (fun name ->
                    let path = IFileSystem.Combine(cwd, name)
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

                            Html.ic (icon path "fa fa-file-o") []
                            text name

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

type FileExplorer( fs : IFileSystem ) =

    let mutable onEdit : string -> unit = ignore

    let create fs =
        //let sessionState = loadSessionState() |> Option.defaultWith defaultSessionState
        (fs,defaultSessionState()) |> Sutil.Store.makeElmish init (updateWithSaveSession (fun f -> onEdit f)) ignore

    let model, dispatch = create fs

    let view  classifier iconselector =
        Bind.el( model, fileExplorer classifier iconselector dispatch )

    do
        fs.OnChange( fun _ -> dispatch Refresh )

    with
        member _.View( classifier : string -> string, iconselector : string -> string ) = view classifier iconselector 
        member _.OnEdit( h : string -> unit) = onEdit <- h
        member _.Dispatch = dispatch
        member _.Selected = model.Value.Selected
        member _.CurrentFolder = model.Value.Cwd        