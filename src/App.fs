module App

open Sutil
open Sutil.DOM
open Sutil.Styling
open type Feliz.length

let appCss = [
    rule ".main-container" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.height (vh 100)
        Css.width (vw 100)
    ]
    rule ".ribbon" [
        Css.backgroundColor "darkgray"
        Css.width (percent 100)
        Css.height (rem 2)
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

let initPanes() =
    dc.AddPane( "Explorer", LeftTop, dummy "Explorer" "azure" )
    dc.AddPane( "Database", LeftTop, dummy "Database" "pink" )
    dc.AddPane( "Solution", LeftTop, dummy "Solution" "lightgreen" )

    dc.AddPane( "Insights", LeftBottom, dummy "Insights" "pink" )
    dc.AddPane( "Translation", LeftBottom, dummy "Translation" "lightgreen" )

    dc.AddPane( "Events", RightTop, dummy "Events" "azure" )
    dc.AddPane( "Files", RightTop, dummy "Files" "pink" )
    dc.AddPane( "Instructions", RightTop, dummy "Instructions" "lightgreen" )

    dc.AddPane( "Links", RightBottom, dummy "Links" "azure" )
    dc.AddPane( "Objects", RightBottom, dummy "Objects" "pink" )

    dc.AddPane( "Console", BottomLeft, dummy "Console" "azure" )
    dc.AddPane( "Messages", BottomLeft, dummy "Messages" "pink" )
    dc.AddPane( "Help", BottomLeft, dummy "Help" "lightgreen" )

    dc.AddPane( "Catalogs", BottomRight, dummy "Catalogs" "lightblue" )
    dc.AddPane( "Components", BottomRight, dummy "Components" "teal" )
    dc.AddPane( "Knowledgebase", BottomRight, dummy "Knowledgebase" "lightpink" )
    ()

let view () =
    Html.div [
        Sutil.Attr.onMount (fun e ->
            initPanes()
            ) [ Sutil.Attr.EventModifier.Once ]
        Attr.className "main-container"
        Html.div [ Attr.className "ribbon"]
        dc.View
    ] |> withStyle appCss

view() |> Program.mountElement "sutil-app"
