module App

open Sutil
open Sutil.DOM
open Sutil.Styling
open type Feliz.length
open Feliz

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
        Css.borderBottom (px 1, borderStyle.solid, Dock.Css.Palette.border)
    ]
    rule ".status-footer" [
        Css.backgroundColor "#EEEEEE"
        Css.width (percent 100)
        Css.height (rem 1.5)
        Css.borderTop (px 1, borderStyle.solid, Dock.Css.Palette.border)
    ]
]

open Dock
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

    //dc.AddPane( "Editor",        CentreCentre, dummy "Editor" "white" )
    ()

let view () =
    Html.div [
        Sutil.Attr.onMount (fun e ->
            initPanes()
            ) [ Sutil.Attr.EventModifier.Once ]
        Attr.className "main-container"
        Html.div [ Attr.className "toolbar"]
        dc.View
        Html.div [ Attr.className "status-footer"]
    ] |> withStyle appCss

view() |> Program.mountElement "sutil-app"
