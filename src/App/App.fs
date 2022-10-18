module App

open Sutil
open Sutil.DOM
open Sutil.Styling
open type Feliz.length
open Feliz
open SutilOxide
open SutilOxide.Dock
open SutilOxide.Types


let appCss = [
    rule ".main-container" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.height (vh 100)
        Css.width (vw 100)
    ]
    rule ".toolbar" [
        Css.backgroundColor "#EEEEEE"
        Css.width (percent 100)
        Css.height (rem 1.5)
        Css.borderBottom (px 1, borderStyle.solid, SutilOxide.Css.LightTheme.Border)
    ]
    rule ".status-footer" [
        Css.backgroundColor "#EEEEEE"
        Css.width (percent 100)
        Css.height (rem 1.5)
        Css.borderTop (px 1, borderStyle.solid, SutilOxide.Css.LightTheme.Border)
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
        text name
    ]


let dc = DockContainer()


let dummyColor = "transparent"

let initPanes() =
    dc.AddPane( "Explorer",      LeftTop,     dummy "Explorer" dummyColor )
    dc.AddPane( "Database",      LeftTop,     dummy "Database" dummyColor )
    dc.AddPane( "Solution",      LeftTop,     dummy "Solution" dummyColor )

    dc.AddPane( "Insights",      LeftBottom,  dummy "Insights" dummyColor )
    dc.AddPane( "Translation",   LeftBottom,  dummy "Translation" dummyColor )

    dc.AddPane( "Events",        RightTop,    dummy "Events" dummyColor )
    dc.AddPane( "Files",         RightTop,    dummy "Files" dummyColor )
    dc.AddPane( "Instructions",  RightTop,    dummy "Instructions" dummyColor )

    dc.AddPane( "Links",         RightBottom, dummy "Links" dummyColor )
    dc.AddPane( "Objects",       RightBottom, dummy "Objects" dummyColor )

    dc.AddPane( "Console",       BottomLeft,  dummy "Console" dummyColor )
    dc.AddPane( "Messages",      BottomLeft,  dummy "Messages" dummyColor )
    dc.AddPane( "Help",          BottomLeft,  dummy "Help" dummyColor )

    dc.AddPane( "Catalogs",      BottomRight, dummy "Catalogs" dummyColor )
    dc.AddPane( "Components",    BottomRight, dummy "Components" dummyColor )
    dc.AddPane( "Knowledgebase", BottomRight, dummy "Knowledgebase" dummyColor )

    dc.AddPane( "Editor",        CentreCentre, dummy "Editor" "white" )
    dc.AddPane( "Charts",        CentreCentre, dummy "Charts" "white" )
    ()

type Theme =
    | Light
    | Dark

type Model = {
    Theme : Theme
}

type Msg =
    | SetTheme of Theme

let init () = { Theme = Light }, Cmd.none

let update msg model =
    match msg with
    | SetTheme t -> { model with Theme = t }, Cmd.none

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
        Sutil.Attr.onMount (fun e ->
            initPanes()
            ) [ Sutil.Attr.EventModifier.Once ]
        Attr.className "main-container"
        Toolbar.toolbar [] [
            Toolbar.buttonItem [ Toolbar.Label "File"]

            Toolbar.dropDownItem [ Toolbar.Label "View" ] [
                Bind.el( model |> Store.map (fun m -> m.Theme), fun t ->
                Toolbar.menuItem [ Toolbar.Label "Theme"] [
                    Toolbar.checkItem [ Toolbar.Label "Light"; Toolbar.IsChecked (t = Light); Toolbar.OnCheckChanged (fun b -> if b then dispatch (SetTheme Light)) ]
                    Toolbar.checkItem [ Toolbar.Label "Dark"; Toolbar.IsChecked (t = Dark); Toolbar.OnCheckChanged (fun b -> if b then dispatch (SetTheme Dark))]
                ])
            ]

            Toolbar.buttonItem [ Toolbar.Label "Help"]
        ]
        dc.View
        Html.div [ Attr.className "status-footer"]
    ] |> withStyle appCss


view() |> Program.mountElement "sutil-app"
