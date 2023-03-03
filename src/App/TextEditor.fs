module TextEditor

open Fable.Core.JsInterop
open SutilOxide.AceEditor
open Sutil
open Sutil.Core
open Sutil.CoreElements
open SutilOxide.FileSystem
open type Feliz.length

let AceSdk : SutilOxide.AceEditor.IExports = importAll("../../public/ace-builds/src-min-noconflict/ace.js")

let initAce divId onChange =
    SutilOxide.AceEditor.config.set("basePath", Some "/ace-builds/src-min-noconflict" )
    let editor = AceSdk.edit( Fable.Core.U2.Case2 divId, {| basePath = "/ace-builds/src-min-noconflict" |} )
    editor.setTheme("ace/theme/textmate")
    editor.session.setMode( Fable.Core.U2.Case1 "ace/mode/text" )
    editor.on_change( fun _ -> onChange() )
    editor

type Editor(fs : IFileSystem ) =
    let mutable editor : Ace.Editor = Unchecked.defaultof<_>

    let mutable editing = ""
    let mutable onEditedChange : bool -> unit = ignore

    let save() =
        if editing <> "" then
            fs.SetFileContent( editing, editor.getValue() )
            onEditedChange false

    let createDiv() =
        Html.div [
            Attr.style [
                Css.width (percent 100)
                Css.height (percent 100)
            ]
            Attr.id "editor"

            Ev.onKeyDown (fun e ->
                if (e.ctrlKey || e.metaKey) && e.key = "s"  then
                    save()
                    e.preventDefault())

            do
                DomHelpers.rafu( fun _ ->
                    editor <- initAce "editor" (fun _ -> onEditedChange true)
                )
        ]

    let startEdit (e : SutilOxide.AceEditor.Ace.Editor) (fs : IFileSystem) (path:string) =
        e.setValue (fs.GetFileContent(path), -1) |> ignore
        e.session.getUndoManager().reset()

        match IFileSystem.GetExtension(path) with
        | ".css" ->
            e.session.setMode( Fable.Core.U2.Case1 "ace/mode/css" )
        | ".js" ->
            e.session.setMode( Fable.Core.U2.Case1 "ace/mode/javascript" )
        | ".html" ->
            e.session.setMode( Fable.Core.U2.Case1 "ace/mode/html" )
        | ".cfg" | ".json" | "" | ".proj" ->
            e.session.setMode( Fable.Core.U2.Case1 "ace/mode/json" )
        | ".md" ->
            e.session.setMode( Fable.Core.U2.Case1 "ace/mode/markdown" )
        | _ ->
            e.session.setMode( Fable.Core.U2.Case1 "ace/mode/text" )

    let load (path : string) =
        editing <- path
        startEdit editor fs path

    do
        ()

    with
        member _.View = createDiv()
        member _.Editor = editor
        member _.Open( path : string ) = load path
        member _.Save() = save()
        member _.OnEditedChange( handler : bool -> unit ) = onEditedChange <- handler
        member _.Text = editor.getValue()
        end
