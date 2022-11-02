module App

open Sutil
open Sutil.DOM
open Sutil.Styling
open type Feliz.length
open Feliz
open SutilOxide
open SutilOxide.Dock
open SutilOxide.Types
open Fable.Formatting.Markdown
open Fetch

type Theme =
    | Light
    | Dark

type Model = {
    SourceText : string
    Theme : Theme
}

let lorem = "Nunc dapibus tempus sapien, vitae efficitur nunc posuere non. Suspendisse in placerat turpis, at sodales nisl. Etiam in tempus nulla. Praesent sed interdum ligula. Sed non nisl est. Praesent vel metus magna. Morbi eget mi est. Nam volutpat purus ligula, ut convallis libero rhoncus ac. "

type Message =
    | Nop
    | SetTheme of Theme
    | SetSourceText of string

let init () = { Theme = Light; SourceText = "Sutil Oxide" }, Cmd.none

let update msg model =
    match msg with
    | Nop -> model, Cmd.none
    | SetTheme t -> { model with Theme = t }, Cmd.none
    | SetSourceText s ->
        { model with SourceText = s}, Cmd.none

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

open Fable.Core.JS

let dummy name colour =
    Html.div [
        Attr.style [
            Css.backgroundColor colour
            Css.width (percent 100)
            Css.height (px 2000)
        ]
        //text name
    ]


let dc = DockContainer()

let fetchSource url  =
    promise {
        //let url = sprintf "%s%s" urlBase tab
        let! res = fetch url []
        return! res.text()
    }


let bindUrl url (view : string  -> SutilElement) =
    Bind.promise (fetchSource url) view


let editor url model dispatch =
    bindUrl url (fun text ->
            dispatch text
            Html.textarea [
                Bind.attr("value", model, dispatch )
            ] |> withStyle [
                    rule "textarea" [
                        Css.borderStyleNone
                        Css.width (percent 100)
                        Css.height (percent 100)
                        Css.fontFamily "Courier New"
                        Css.resizeNone
                        Css.margin 0
                        Css.padding (rem 0.2)
                    ]
                    rule "textarea:focus" [
                        Css.outlineStyleNone
                    ]
                ]
            )

let mdCss = [
            rule ".md" [
                Css.fontFamily "Courier New"
                Css.backgroundColor "hsl(43, 100%, 95%)"
                Css.padding (rem 0.5)
                Css.width (percent 100)
                Css.height (percent 100)
            ]
    ]

let parsemd md =
    try
        let doc  = Markdown.Parse(md)
        let html = Markdown.ToHtml(doc)
        html
    with
        | x -> $"<pre>{x}</pre>"

let viewMd url =
    bindUrl url  (fun text ->
        Html.div [
            Attr.className "md"
            Attr.style [
                Css.backgroundColor "hsl(240, 100%, 95%)"
                Css.width (percent 100)
            ]
            text |> parsemd |> html
        ] |> withStyle mdCss
    )

let preview model =
    Html.div [
        Attr.className "md"
        Bind.el(model |> Store.map parsemd, html)
    ] |> withStyle mdCss


let dummyColor = "transparent"

let initPanes (model : IStore<Model>) dispatch =
    dc.AddPane( "Explorer",      LeftTop,     dummy "Explorer" "hsl(120, 100%, 95%)" )
    dc.AddPane( "Database",      LeftTop,     dummy "Database" "hsl(43, 100%, 95%)" )
    dc.AddPane( "Solution",      LeftTop,     dummy "Solution" "hsl(43, 100%, 95%)" )

    dc.AddPane( "Insights",      LeftBottom,  dummy "Insights" "hsl(80, 100%, 95%)" )
    dc.AddPane( "Translation",   LeftBottom,  dummy "Translation" "hsl(43, 100%, 95%)" )

    dc.AddPane( "README",        RightTop, preview (model |> Store.map (fun m -> m.SourceText)) )
    dc.AddPane( "Events",        RightTop,    dummy "Events" "hsl(43, 100%, 95%)" )
    dc.AddPane( "Files",         RightTop,    dummy "Files" "hsl(43, 100%, 95%)" )
    dc.AddPane( "Instructions",  RightTop,    dummy "Instructions" "hsl(43, 100%, 95%)" )

    dc.AddPane( "ISSUES",         RightBottom, viewMd "ISSUES.md" )
    dc.AddPane( "Links",         RightBottom, dummy "Links" "hsl(240, 100%, 95%)" )
    dc.AddPane( "Objects",       RightBottom, dummy "Objects" "hsl(43, 100%, 95%)" )

    dc.AddPane( "Console",       BottomLeft,  dummy "Console" "hsl(160, 100%, 95%)" )
    dc.AddPane( "Messages",      BottomLeft,  dummy "Messages" "hsl(43, 100%, 95%)" )
    dc.AddPane( "Help",          BottomLeft,  dummy "Help" "hsl(43, 100%, 95%)" )

    dc.AddPane( "Catalogs",      BottomRight, dummy "Catalogs" "hsl(200, 100%, 95%)" )
    dc.AddPane( "Components",    BottomRight, dummy "Components" "hsl(43, 100%, 95%)" )
    dc.AddPane( "Knowledgebase", BottomRight, dummy "Knowledgebase" "hsl(43, 100%, 95%)" )

    dc.AddPane( "Editor",        CentreCentre, editor "README.md" (model |> Store.map (fun m -> m.SourceText)) (dispatch << SetSourceText) )
    dc.AddPane( "Charts",        CentreCentre, dummy "Charts" "hsl(43, 100%, 95%)"  )
    ()

open Toolbar

let view () =
    let model, dispatch = () |> Store.makeElmish init update ignore
    let mutable styleCleanup = ignore

    model |> Store.map (fun t -> t.Theme) |> Observable.distinctUntilChanged |> Store.subscribe (fun t ->
        styleCleanup()
        let theme =
            match t with
            | Light -> SutilOxide.Css.LightTheme
            | Dark -> SutilOxide.Css.DarkTheme
        styleCleanup <- SutilOxide.Css.installStyling theme
    ) |> ignore

    Html.div [
        Attr.className "main-container"

        Sutil.Attr.onMount (fun e ->
            initPanes model dispatch
            ) [ Sutil.Attr.EventModifier.Once ]


        toolbar [] [
            dropDownItem [ Label "File"] [
                buttonItem [ Label "Open"; Icon "fa-folder-open"; OnClick (fun e -> dispatch Nop) ]
                buttonItem [ Label "Close"; Icon "fa-folder-close"; OnClick (fun e -> dispatch Nop) ]
                buttonItem [ Label "Save"; Icon "fa-save"; OnClick (fun e -> dispatch Nop) ]
            ]

            dropDownItem [ Label "View" ] [
                Bind.el( model |> Store.map (fun m -> m.Theme), fun t ->
                menuItem [ Label "Theme"] [
                    checkItem [ Label "Light"; IsChecked (t = Light); OnCheckChanged (fun b -> if b then dispatch (SetTheme Light)) ]
                    checkItem [ Label "Dark"; IsChecked (t = Dark); OnCheckChanged (fun b -> if b then dispatch (SetTheme Dark))]
                ])
            ]

            buttonItem [ Label "Help"; Icon "fa-life-ring"]
        ]

        dc.View

        Html.div [
            Attr.className "status-footer theme-control-bg theme-border"
        ]

    ] |> withStyle appCss


view() |> Program.mountElement "sutil-app"
