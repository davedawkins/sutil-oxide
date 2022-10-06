module Css

open Sutil
open Sutil.Styling

open type Feliz.length


module Palette =
    let border = "#EEEEEE"
    let background = "#FFFFFF"
    let backgroundHover = "#EEEEEE"
    let backgroundSelected = "#CCCCCC"
    let handle = border
    let icon = "#AAAAAA"
    let preview = "#b9d3de"


let primaryIds = [ "left"; "right"; "top"; "bottom" ]
let containerIds = [
    "left-top"
    "left-bottom"
    "right-top"
    "right-bottom"
    "bottom-left"
    "bottom-right"
    "top-left"
    "top-right"
]

let css = [

    rule ".dock-container" [
        Css.width (percent 100)
        Css.height (percent 100)
        Css.backgroundColor Palette.background
        Css.displayGrid
        Css.custom("grid-template-columns", "max-content 1fr 1fr max-content")
        Css.custom("grid-template-rows", "1fr 1fr max-content")
    ]

    rule ".dock-main-grid" [
        Css.displayGrid
        Css.gridRow ("1","3")
        Css.gridColumn ("2", "4")
        Css.custom("grid-template-columns", "max-content auto max-content")
        Css.custom("grid-template-rows", "auto max-content")
    ]

    rule ".dock-left-container" [
        Css.positionRelative
        Css.width (px 400)
        Css.height (percent 100)
        Css.minWidth (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("1", "1")
        Css.displayFlex
        Css.flexDirectionColumn
    ]

    rule ".dock-right-container" [
        Css.positionRelative
        Css.width (px 400)
        Css.height (percent 100)
        Css.minWidth (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("3", "3")
        Css.displayFlex
        Css.flexDirectionColumn
    ]

    rule ".dock-bottom-container" [
        Css.positionRelative
        Css.height (px 200)
        Css.minHeight (rem 2)
        Css.gridRow ("2","2")
        Css.gridColumn ("1", "4")
        Css.displayFlex
        Css.flexDirectionRow
    ]

    yield! containerIds |> List.map (fun id ->

        // Selecting containers such as "dock-left-top-container"

        rule $".dock-{id}-container" [
            Css.positionRelative // For the drag overlay
            Css.maxHeight (percent 100)
            Css.minHeight (percent 0)
            Css.flexBasis 0
            Css.flexShrink 1
            Css.flexGrow 1
        ])

    yield! containerIds |> List.map (fun id ->

        // Selecting parent for user content
        rule $".dock-{id}-content" [
            Css.height (percent 100)
        ])


    rule ".dock-main" [
        Css.positionRelative
        Css.height (percent 100)
        Css.width (percent 100)
        Css.minHeight (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("2", "2")
    ]

    // ------------------------------------------------------------------------
    // Tabs

    rule ".dock-tabs" [
        Css.positionRelative
        Css.displayGrid
        Css.custom("grid-template-columns", "repeat(10,max-content)")
        Css.userSelectNone
        //Css.gap (rem 1)
    ]

    rule ".tabs-left-top" [

        Css.gridRow ("1","1")
        Css.gridColumn ("1", "1")
        Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-lr")
        Css.custom ("justify-content", "end")
    ]

    rule ".tabs-left-bottom" [

        Css.gridRow ("2","2")
        Css.gridColumn ("", "1")
        Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-lr")
        Css.custom ("justify-content", "start")
    ]

    rule ".tabs-right-top" [
        Css.gridRow ("1","1")
        Css.gridColumn ("4", "4")
        Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-rl")
        Css.custom ("justify-content", "start")
    ]

    rule ".tabs-right-bottom" [
        Css.gridRow ("2","2")
        Css.gridColumn ("4", "4")
        Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-rl")
        Css.custom ("justify-content", "end")
    ]

    rule ".tabs-bottom-left" [
        Css.gridRow ("3","3")
        Css.gridColumn ("2", "2")
        Css.minHeight (rem 1.5)
    ]

    rule ".tabs-bottom-right" [
        Css.gridRow ("3","3")
        Css.gridColumn ("3", "3")
        Css.minHeight (rem 1.5)
        Css.custom ("justify-content", "end")
    ]

    rule ".dock-tabs.box-left" [
        Css.gridRow ("3","3")
        Css.gridColumn ("1", "1")
        Css.minHeight (rem 1.5)
    ]

    rule ".dock-tabs.box-right" [
        Css.gridRow ("3","3")
        Css.gridColumn ("4", "4")
        Css.minHeight (rem 1.5)
    ]

    // ------------------------------------------------------------------------
    // Tab labels

    rule ".tab-label" [
        Css.positionRelative
        Css.fontSize (percent 75)
        Css.cursorPointer
        Css.displayFlex
        Css.flexDirectionRow
        Css.alignItemsCenter
        Css.gap (rem 0.2)
    ]

    rule ".tab-label i" [
        Css.color Palette.icon
    ]

    rule ".dragging.tab-label i" [
        Css.color Palette.preview
    ]

    rule ".tab-label:hover" [
        Css.backgroundColor Palette.backgroundHover
    ]

    rule ".tab-label.selected" [
        Css.backgroundColor Palette.backgroundSelected
    ]


    rule ".tabs-bottom .tab-label" [
        Css.paddingRight (rem 0.5)
        Css.paddingTop (rem 0)
        Css.paddingLeft (rem 0.5)
        Css.paddingBottom (px 2)
//        Css.custom("writing-mode","sideways-lr")
    ]

    rule ".tabs-left .tab-label" [
        Css.paddingBottom (rem 0.5)
        Css.paddingRight (rem 0)
        Css.paddingTop (rem 0.5)
        Css.paddingLeft (px 2)
//        Css.custom("writing-mode","sideways-lr")
    ]

    rule ".tabs-right .tab-label" [
        Css.paddingBottom (rem 0.5)
        Css.paddingRight (rem 0)
        Css.paddingTop (rem 0.5)
        Css.paddingLeft (px 2)
//        Css.custom("writing-mode","sideways-lr")
    ]

    rule ".tabs-left .tab-label>i" [
        Css.custom("writing-mode","tb")
    ]

    rule ".tabs-right .tab-label>i" [
        Css.custom("writing-mode","tb")
    ]


    rule ".dock-resize-handle" [
        Css.positionAbsolute
        Css.backgroundColor Palette.handle
        Css.custom( "--resize-handle-thickness", "1px" )
        Css.zIndex (99)
    ]

    rule ".dock-resize-handle.top" [
        Css.top (px 0)
        Css.custom( "height", "var(--resize-handle-thickness)")
        Css.width (percent 100)
        Css.cursorNorthSouthResize
    ]

    rule ".dock-resize-handle.bottom" [
        Css.bottom (px 0)
        Css.width (percent 100)
        Css.custom( "height", "var(--resize-handle-thickness)")
        Css.cursorNorthSouthResize
    ]

    rule ".dock-resize-handle.right" [
        Css.right (px 0)
        Css.height (percent 100)
        Css.custom( "width", "var(--resize-handle-thickness)")
        Css.cursorEastWestResize
    ]

    rule ".dock-resize-handle.left" [
        Css.left (px 0)
        Css.height (percent 100)
        Css.custom( "width", "var(--resize-handle-thickness)")
        Css.cursorEastWestResize
    ]

    // Borders

    rule ".border" [
        Css.borderStyleSolid
        Css.borderColor Palette.border
        Css.borderWidth (px 0)
    ]

    rule ".border-top" [
        Css.borderTopWidth  (px 1)
    ]

    rule ".border-bottom" [
        Css.borderBottomWidth  (px 1)
    ]

    rule ".border-left" [
        Css.borderLeftWidth  (px 1)
    ]

    rule ".border-right" [
        Css.borderRightWidth  (px 1)
    ]

    // Miscellaneous

    rule "outline-debug" [
        Css.outlineStyleSolid
        Css.outlineWidth (px 1)
        Css.outlineColor "red"
    ]

    rule ".sideways-lr" [
        Css.custom("writing-mode","sideways-lr")
    ]

    rule ".dock-tabs .preview" [
        Css.backgroundColor Palette.preview
    ]

    rule ".preview" [
        Css.backgroundColor Palette.preview
    ]

    rule ".drag-overlay" [
        Css.displayNone
        Css.positionAbsolute
        Css.backgroundColor (Palette.preview)
        Css.zIndex 999
    ]

    rule ".drag-overlay.left" [
        Css.top (px 0)
        Css.left (px 0)
        Css.width (px 150)
        Css.height (percent 100)
    ]

    rule ".drag-overlay.right" [
        Css.top (px 0)
        Css.right (px 0)
        Css.width (px 150)
        Css.height (percent 100)
    ]

    rule ".drag-overlay.bottom" [
        Css.width (percent 100)
        Css.left (px 0)
        Css.right (px 0)
        Css.bottom (px 0)
        Css.height (px 150)
    ]

    rule ".drag-overlay.visible" [
        Css.displayBlock
    ]

    rule ".tab-label.dragging" [
        //Css.displayNone
        Css.backgroundColor Palette.preview
        Css.color Palette.preview
    ]


    rule ".dragimage" [
        Css.positionAbsolute
        Css.left (px -200)
        Css.top (px -200)
    ]

    rule ".hidden" [
        Css.displayNone
    ]
]
