module App

open System
open Sutil
//open Sutil.Toastr
open Sutil.Core
open Sutil.CoreElements
open Sutil.Styling
open type Feliz.length
open Feliz
open SutilOxide
open SutilOxide.Dock
open SutilOxide.Types
open Fable.Formatting.Markdown
open Fetch
open SutilOxide.FileSystem

type Theme =
    | Light
    | Dark

type AppContext = {
    Fs : SutilOxide.FileSystem.IFileSystem
}


let mutable nodeStores : Map<string,IStore<string>> = Map.empty

let getNodeStore (name:string) =
    if not (nodeStores.ContainsKey(name)) then
        nodeStores <- nodeStores.Add( name, Store.make "" )
    nodeStores[name]

let clearNodeStores() = nodeStores <- Map.empty

type Model = {
    PreviewText : string
    Theme : Theme
    Editing : string option
    NeedsSave : bool
    Log : string
    Graph : SutilOxide.Flow.Types.Graph
}

let lorem = "Nunc dapibus tempus sapien, vitae efficitur nunc posuere non. Suspendisse in placerat turpis, at sodales nisl. Etiam in tempus nulla. Praesent sed interdum ligula. Sed non nisl est. Praesent vel metus magna. Morbi eget mi est. Nam volutpat purus ligula, ut convallis libero rhoncus ac. "

type Message =
    | AppendToLog of string
    | Nop
    | SetTheme of Theme
    | SetPreviewText of string
    | SimpleProgressBar
    | Edit of string
    | SaveEdits
    | SetEdited of bool
    | DeleteFile of bool * (unit->unit)
    | SetGraph of Flow.Types.Graph

let log = SutilOxide.Logging.log

let fetchSource url  =
    promise {
        //let url = sprintf "%s%s" urlBase tab
        let! res = fetch url []
        return! res.text()
    }

let uploadFile (url : string) (targetFileName : string) (fs : IFileSystem)  =
    promise {
        let! content = fetchSource url
        fs.SetFileContent( targetFileName, content )
    }

let init (app : AppContext, graph) =
    {
        Theme = Light
        PreviewText = "(no markdown to preview)"
        Editing = None
        NeedsSave = false
        Graph = graph
        Log = "" },
    //Cmd.none
    Cmd.OfPromise.perform (uploadFile "README.md" "README.md") app.Fs (fun _ -> Edit "README.md")

let update (app : AppContext) (textEditor : TextEditor.Editor) msg model =
    match msg with
    | SetGraph g -> { model with Graph = g }, Cmd.none
    | AppendToLog m ->
        { model with Log = model.Log + m + "\n" }, Cmd.none
    | DeleteFile (confirmed,delete) ->
        let confirm dispatch =
            { Modal.ModalOptions.Create() with
                Content = fun close ->
                    Html.div "Confirm delete?"
                Buttons = [
                    ("Cancel", fun close -> close())
                    ("Delete", fun close ->  close(); dispatch (DeleteFile (true,delete)) )

                ]
            } |> Modal.modal

        match confirmed with
        | true ->
            delete()
            model, Cmd.none
        | false ->
            model, [ confirm ]
    | SaveEdits ->
        model, Cmd.none
    | SetEdited z ->
        let cmd =
            if z then
                Cmd.ofMsg (SetPreviewText (textEditor.Text))
            else
                Cmd.none

        { model with NeedsSave = z}, cmd
    | Nop -> model, Cmd.none
    | SetTheme t -> { model with Theme = t }, Cmd.none
    | SetPreviewText s ->
        { model with PreviewText = s}, Cmd.none
    | SimpleProgressBar ->
        let cmd = Cmd.none
            // Toastr.message "Countdown has started"
            // |> Toastr.title "Be aware"
            // |> Toastr.withProgressBar
            // |> Toastr.warning
        model, cmd
    | Edit name ->
        //startEdit (app.Fs) name
        // if model.NeedsSave then
        //     safeSave()
        // dc.SetProperty( "Editor", Dock.Visible true)
        textEditor.Open name
        { model with Editing = Some name; NeedsSave = false }, Cmd.ofMsg (SetPreviewText (app.Fs.GetFileContent name))


let appCss = [

    rule ".main-container" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.height (vh 100)
        Css.width (vw 100)
    ]

    rule ".status-footer" [
        Css.width (percent 100)
        Css.height (rem 1.5)
        Css.borderWidth 0
        Css.borderTopWidth (px 1)
        Css.borderTopStyle borderStyle.solid
    ]
]


let dummy name colour =
    Html.div [
        Attr.style [
            Css.backgroundColor colour
            Css.width (percent 100)
            Css.height (px 2000)
        ]
        text "Example pane"
    ]


let mdCss = [
            rule ".md" [
                Css.fontFamily "Courier New"
                Css.backgroundColor "hsl(53.2, 100%, 91.4%)"
                Css.padding (rem 0.5)
                Css.width (percent 100)
                Css.custom("height", "auto")
            ]
            rule ".md a" [
                Css.textDecorationLineUnderline
            ]
    ]

let markdownToHtml md =
    try
        md |> Markdown.Parse |> Markdown.ToHtml
    with
        | x -> $"<pre>{x}</pre>"


let bindMd markdown =
    Html.div [
        Attr.className "md"
        Bind.el(markdown |> Store.map markdownToHtml, html)
    ] |> withStyle mdCss

let bindUrl url (view : string  -> SutilElement) =
    Bind.promise (fetchSource url, view)


let viewMd url =
    bindUrl url  (fun text ->
        Html.div [
            Attr.className "md"
            text |> markdownToHtml |> html
        ] |> withStyle mdCss
    )

let dummyColor = "transparent"


let logStyle = [
    Css.height (percent 100)
    Css.width (percent 100)
    Css.fontFamily "'Courier New', Courier, monospace"
    Css.borderStyleNone
    Css.margin 0
    Css.resizeNone
]

let mainLog (model : IObservable<Model>) =
    let logS = model |> Store.map (fun m -> m.Log) |> Observable.distinctUntilChanged

    Html.textarea [

        Attr.style logStyle

        Attr.id "log"
        Attr.readOnly true
        Bind.attr("value", logS)

        hookParent( fun n ->
            let e = n :?> Browser.Types.HTMLTextAreaElement
            let stop = logS.Subscribe( fun _ ->
                DomHelpers.rafu( fun _ -> e.setSelectionRange(99999,99999))
            )
            SutilEffect.RegisterDisposable( n, stop )
            ()
        )
    ]

open Flow
let exampleGraph : Graph = {
    Nodes = Map [
        "n1", Node.Create("n1", "Hello", 250.0, 50.0)
        "n2", Node.Create("n2", "World", 250.0, 150.0)
        "n3", Node.Create("n3", "No Select", 50.0, 50.0)
        "n4", Node.Create("n4", "No Move", 50.0, 150.0)
    ]
    Edges = Map [
        "e1", Edge.Create( "e1", "n1", "out", "n2", "in")
    ]
}

let catalogStyle = [
    rule ".flow-catalog" [
        Css.displayFlex
        Css.flexDirectionRow
        Css.flexWrapWrap
        Css.alignItemsFlexStart
        Css.alignContentFlexStart
        //Css.justifyContentSpaceAround
        Css.gap (rem 1)
        Css.padding (rem 1)
        Css.backgroundColor ("white")
        Css.height (percent 100)
        Css.width (percent 100)
    ]
    rule ".flow-catalog-item" [
        Css.displayInlineFlex
        Css.border (px 1, Feliz.borderStyle.solid, "#888888")
        Css.borderRadius (px 4)
        Css.backgroundColor "white"
        Css.color "black"
        Css.height (auto)
        Css.padding (rem 0.35)
        Css.cursorPointer
    ]
]

let catalogItem name = 
    Html.divc "flow-catalog-item" [
        text name
    ]

let catalogTypes = [ 
            "Start"
            "Pause"
            "Rewind"
            "Forward"
            "Reset"
            "Clock"
            "Stop"
        ] 

let clockS = Store.make (System.DateTime.Now)

DomHelpers.interval (fun _ -> DateTime.Now |> Store.set clockS) 500 |> ignore

let flowCatalog() =
    Html.divc "flow-catalog" [
        yield! catalogTypes |> List.map (fun t -> Flow.Views.makeCatalogItem(t,catalogItem t ))
    ] |> withStyle catalogStyle

let flowGraph graph dispatch =
    let options' = GraphOptions.Create()
    let nodeFactory(name,typ) =
        options'.NodeFactory(name,typ)
    let viewNode (nodeS : IReadOnlyStore<Node>) =
        let node = nodeS.Value
        if (node.Id.StartsWith("Clock")) then
            Html.div [
                Bind.el( clockS, fun c -> Html.span (c.ToLongTimeString()))
            ]
        else if (node.Id.StartsWith("Start")) then
            Html.div [
                Bind.el( getNodeStore node.Id, Html.span)
            ]
        else
            options'.ViewNode(nodeS)
    let fc = Flow.FlowChart( 
        { options' with 
                NodeFactory = nodeFactory 
                ViewNode = viewNode
                OnChange = (dispatch<<SetGraph)
                } )

    fc.Render(graph)

let runGraph (g : Graph) =
    let threads = g.Nodes.Values |> Seq.map (fun n ->
        if n.Id.StartsWith("Start") then
            promise {
                let mutable i = 0
                while true do
                    do! Promise.sleep 1000
                    i <- i + 1 
                    (string i) |> Store.set (getNodeStore n.Id)

            }
        else 
            promise { return () }
    )
    threads |> Promise.all |> Promise.start

let initPanes  (fileExplorer : FileExplorer.FileExplorer) (textEditor : TextEditor.Editor) (model : IStore<Model>) dispatch (dc : DockContainer)  =

    let editorTitle model =
        model
        |> Store.map (fun m ->
            match m.Editing with
            | None -> "Editor"
            | Some fileName ->
                    IFileSystem.GetFileName(fileName) + (if m.NeedsSave then " (edited)" else "")
            )
        |> Html.span

    dc.AddPane( "Explorer",      LeftTop,     fileExplorer.View)
    dc.AddPane( "Database",      LeftTop,     dummy "Database" "hsl(43, 100%, 95%)", false )
    dc.AddPane( "Solution",      LeftTop,     dummy "Solution" "hsl(43, 100%, 95%)", false )

    dc.AddPane( "Insights",      LeftBottom,  dummy "Insights" "hsl(80, 100%, 95%)", false )
    dc.AddPane( "Translation",   LeftBottom,  dummy "Translation" "hsl(43, 100%, 95%)", false )

    // dc.AddPane( "Events",        RightTop,    dummy "Events" "hsl(43, 100%, 95%)" )
    // dc.AddPane( "Files",         RightTop,    dummy "Files" "hsl(43, 100%, 95%)" )
    dc.AddPane( "Instructions",  RightTop,    dummy "Instructions" "hsl(43, 100%, 95%)" )

    dc.AddPane( "Links",         RightBottom, dummy "Links" "hsl(240, 100%, 95%)", false )
    dc.AddPane( "Objects",       RightBottom, dummy "Objects" "hsl(43, 100%, 95%)", false )

    dc.AddPane( "Console",       BottomLeft,  dummy "Console" "hsl(160, 100%, 95%)", false )
    dc.AddPane( "Messages",      BottomLeft,  mainLog model, true )

    // dc.AddPane( "Catalogs",      BottomRight, dummy "Catalogs" "hsl(200, 100%, 95%)", false )
    // dc.AddPane( "Components",    BottomRight, dummy "Components" "hsl(43, 100%, 95%)", false )
    dc.AddPane( "Knowledgebase", BottomRight, dummy "Knowledgebase" "hsl(43, 100%, 95%)", false )
    dc.AddPane( "Help",          BottomRight,  viewMd "HELP.md", false )

    dc.AddPane( "Preview",       RightTop, bindMd (model |> Store.map (fun m -> m.PreviewText)), true )
    dc.AddPane(
        "Ace", "Ace",
        CentreCentre,
        editorTitle model,
        textEditor.View,
        true )

    dc.AddPane( "Catalog",       LeftTop, flowCatalog(), true )
    dc.AddPane( "Graph",         CentreCentre, flowGraph (model |> Store.getMap (fun m -> m.Graph)) dispatch , true )

    ()

open Toolbar

let view () =
    let dc = DockContainer( Options.Create() )
    let app = { Fs = LocalStorageFileSystem("oxide-demo") }
    let graph = exampleGraph

    // Text editor control
    let textEditor = TextEditor.Editor( app.Fs )
    let fileExplorer = FileExplorer.FileExplorer(app.Fs)

    //let (modelFx,dispatchFx) = SutilOxide.FileExplorer.create (dispatch << Edit) app.Fs

    // Main model and dispatch
    let model, dispatch = (app,graph) |> Store.makeElmish (init) (update app textEditor) ignore

    textEditor.OnEditedChange( dispatch << SetEdited )
    SutilOxide.Logging.appendHandler( dispatch << AppendToLog )

    // Time widget in status bae
    let timeS = Store.make ""
    let stopClock = DomHelpers.interval (fun _ -> Store.set timeS (System.DateTime.Now.ToLongTimeString())) 1000

    // File Explorer control

    //let fileExplorer = SutilOxide.FileExplorer.view (modelFx, dispatchFx)

    // Handle theme changes
    let mutable styleCleanup = ignore
    model |> Store.map (fun t -> t.Theme) |> Observable.distinctUntilChanged |> Store.subscribe (fun t ->
        styleCleanup()
        let theme =
            match t with
            | Light -> SutilOxide.Css.LightTheme
            | Dark -> SutilOxide.Css.DarkTheme
        styleCleanup <- SutilOxide.Css.installStyling theme
    ) |> ignore

    log "SutilOxide Demo started"

    // Main view
    Html.div [
        Attr.className "main-container"

        toolbar [] [

            dropDownItem [ Label "File"] [
                //buttonItem [ Label "Open"; Icon "fa-folder-open"; OnClick (fun e -> dispatch SimpleProgressBar) ]
                //buttonItem [ Label "Close"; Icon "fa-folder-close"; OnClick (fun e -> dispatch Nop) ]
                buttonItem [ Label "New File"; Icon "fa-file-o"; OnClick (fun e -> fileExplorer.Dispatch FileExplorer.NewFile) ]
                buttonItem [ Label "Save"; Icon "fa-save"; OnClick (fun e -> textEditor.Save() ) ]
                buttonItem [ Label "Rename"; Icon "fa-i-cursor"; OnClick (fun e -> fileExplorer.Dispatch (FileExplorer.SetRenaming true)) ]
                hseparator
                buttonItem [
                    Label "Delete"
                    Icon "fa-trash-o"
                    OnClick (fun e ->
                        let deleteSelected() = fileExplorer.Dispatch FileExplorer.DeleteSelected
                        dispatch (DeleteFile (false,deleteSelected))
                    )
                ]
            ]

            dropDownItem [ Label "View" ] [
                Bind.el( model |> Store.map (fun m -> m.Theme), fun t ->
                menuItem [ Label "Theme"] [
                    checkItem [ Label "Light"; IsChecked (t = Light); OnCheckChanged (fun b -> if b then dispatch (SetTheme Light)) ]
                    checkItem [ Label "Dark"; IsChecked (t = Dark); OnCheckChanged (fun b -> if b then dispatch (SetTheme Dark))]
                ])
            ]
            buttonItem [ Label "Run"; Icon "fa-play"; OnClick (fun _ -> runGraph(model |> Store.getMap (fun m->m.Graph)))]

            buttonItem [ Label "Help"; Icon "fa-life-ring"; OnClick (fun _ -> dc.SetProperty( "Help", Visible true))]
        ]

        dc.View (initPanes fileExplorer textEditor model dispatch)

        statusbar [] [
            text "Time:"
            gap
            Bind.el(timeS, Html.span)
            //vseparator
            //text "test"
        ]

    ] |> withStyle appCss


view() |> Program.mount
