module SutilOxide.Css

open Sutil
open Sutil.Styling
open type Feliz.length

type Theme = {
    TextColor : string
    Border : string
    ControlBackground  : string;
    ContentBackground  : string;
    BackgroundHover    : string;
    BackgroundSelected : string;
    Handle    :string;
    Icon      :string;
    Highlight :string;
    Overlay   :string;
}

let LightTheme =
    let border = "#DBDBDB"
    let highlight = "#B7DAFA"
    {
        TextColor = "black"
        Border = border
        ControlBackground = "#EBEBEB"
        ContentBackground = "#F4F4F4"
        BackgroundHover = "#DDDDDD"
        BackgroundSelected = "#CCCCCC"
        Handle = border
        Icon = "#50719B"
        Highlight = highlight
        Overlay = highlight
    }

let DarkTheme =
    let border = "rgb(40,40,40)"
    let highlight = "#495764" //"#6D8296"
    {
        TextColor = "rgb(207,207,207)"
        Border = border
        ControlBackground = "rgb(45,45,45)"
        ContentBackground = "rgb(35,35,35)"
        BackgroundHover = "#222222"
        BackgroundSelected = "#333333"
        Handle = border
        Icon = "#506baa"
        Highlight = highlight
        Overlay = highlight
    }



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
    "centre-centre"
]

let private baseStyling (theme : Theme) = [

    rule "*" [
        Css.color theme.TextColor
    ]

    rule "a" [
        Css.custom("color", "inherit")
        Css.textDecorationNone
        Css.whiteSpaceNowrap
    ]

    rule ".content-vcentre" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.justifyContentCenter
    ]

    rule ".theme-control-bg" [
        Css.backgroundColor theme.ControlBackground
    ]

    rule ".theme-content-bg" [
        Css.backgroundColor theme.ContentBackground
    ]

    rule ".theme-border" [
        Css.borderColor theme.Border
    ]
]

let private dockStyling (theme : Theme) = [

    rule ".dock-container" [
        Css.width (percent 100)
        Css.height (percent 100)
        Css.backgroundColor theme.ControlBackground
        Css.displayGrid
        Css.custom("grid-template-columns", "max-content 1fr 1fr max-content")
        Css.custom("grid-template-rows", "max-content 1fr 1fr max-content")
    ]

    rule ".dock-main-grid" [
        Css.displayGrid
        Css.gridRow ("2","4")
        Css.gridColumn ("2", "4")

        // dock-top-container
        // dock-centre-container
        // dock-bottom-container
        Css.custom("grid-template-rows", "max-content auto max-content")
    ]

    rule ".dock-centre-container" [
        Css.displayGrid
        Css.gridRow ("2","2")
        //Css.gridColumn ("2", "4")
        // left container
        // centre tabs
        // centre-container
        // right-container
        Css.custom("grid-template-columns", "max-content max-content auto max-content")
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

    rule ".dock-main" [
        Css.positionRelative
        Css.height (percent 100)
        Css.width (percent 100)
        Css.minHeight (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("3", "3")
        Css.displayFlex
        Css.flexDirectionColumn
    ]

    rule ".dock-right-container" [
        Css.positionRelative
        Css.width (px 400)
        Css.height (percent 100)
        Css.minWidth (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("4", "4")
        Css.displayFlex
        Css.flexDirectionColumn
    ]

    rule ".dock-top-container" [
        Css.positionRelative
        Css.height (px 200)
        Css.minHeight (rem 2)
        Css.gridRow ("1","1")
        //Css.gridColumn ("1", "4")
        Css.displayFlex
        Css.flexDirectionRow
    ]

    rule ".dock-bottom-container" [
        Css.positionRelative
        Css.height (px 200)
        Css.minHeight (rem 2)
        Css.gridRow ("3","3")
        //Css.gridColumn ("1", "4")
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


    // ------------------------------------------------------------------------
    // Tabs


    rule ".dock-tabs" [
        Css.positionRelative
        Css.displayGrid
        Css.custom("grid-template-columns", "repeat(10,max-content)")
        Css.userSelectNone
        //Css.gap (rem 1)
    ]

    rule ".tabs-centre" [
        Css.custom ("writing-mode", "sideways-lr")
        Css.custom ("justify-content", "end")
    ]

    rule ".tabs-left-top" [

        Css.gridRow ("2","2")
        Css.gridColumn ("1", "1")
        //Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-lr")
        Css.custom ("justify-content", "end")
    ]

    rule ".tabs-left-bottom" [

        Css.gridRow ("3","3")
        Css.gridColumn ("", "1")
        //Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-lr")
        Css.custom ("justify-content", "start")
    ]

    rule ".tabs-right-top" [
        Css.gridRow ("2","2")
        Css.gridColumn ("4", "4")
        //Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-rl")
        Css.custom ("justify-content", "start")
    ]

    rule ".tabs-right-bottom" [
        Css.gridRow ("3","3")
        Css.gridColumn ("4", "4")
        //Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-rl")
        Css.custom ("justify-content", "end")
    ]


    rule ".tabs-top-left" [
        Css.gridRow ("1","1")
        Css.gridColumn ("2", "2")
        //Css.minHeight (rem 1.5)
    ]

    rule ".tabs-top-right" [
        Css.gridRow ("1","1")
        Css.gridColumn ("3", "3")
        //Css.minHeight (rem 1.5)
        Css.custom ("justify-content", "end")
    ]


    rule ".tabs-bottom-left" [
        Css.gridRow ("4","4")
        Css.gridColumn ("2", "2")
        //Css.minHeight (rem 1.5)
    ]

    rule ".tabs-bottom-right" [
        Css.gridRow ("4","4")
        Css.gridColumn ("3", "3")
        //Css.minHeight (rem 1.5)
        Css.custom ("justify-content", "end")
    ]

    rule ".dock-tabs.box-left" [
        Css.gridRow ("4","4")
        Css.gridColumn ("1", "1")
        //Css.minHeight (rem 1.5)
    ]

    rule ".dock-tabs.box-right" [
        Css.gridRow ("4","4")
        Css.gridColumn ("4", "4")
        //Css.minHeight (rem 1.5)
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
        Css.color theme.Icon
    ]

    rule ".dragging.tab-label i" [
        Css.color theme.Overlay
    ]

    rule ".tab-label:hover" [
        Css.backgroundColor theme.BackgroundHover
    ]

    rule ".tab-label.selected" [
        Css.backgroundColor theme.BackgroundSelected
    ]

    rule ".tabs-top .tab-label" [
        Css.paddingRight (rem 0.5)
        Css.paddingBottom (rem 0)
        Css.paddingLeft (rem 0.5)
        Css.paddingTop (px 2)
    ]

    rule ".tabs-bottom .tab-label" [
        Css.paddingRight (rem 0.5)
        Css.paddingTop (rem 0)
        Css.paddingLeft (rem 0.5)
        Css.paddingBottom (px 2)
    ]

    rule ".tabs-left .tab-label" [
        Css.paddingBottom (rem 0.5)
        Css.paddingRight (rem 0)
        Css.paddingTop (rem 0.5)
        Css.paddingLeft (px 2)
    ]

    rule ".tabs-centre .tab-label" [
        Css.paddingBottom (rem 0.5)
        Css.paddingRight (rem 0)
        Css.paddingTop (rem 0.5)
        Css.paddingLeft (px 2)
    ]


    rule ".tabs-right .tab-label" [
        Css.paddingBottom (rem 0.5)
        Css.paddingRight (rem 0)
        Css.paddingTop (rem 0.5)
        Css.paddingLeft (px 2)
    ]

    rule ".tabs-left .tab-label>i" [
        Css.custom("writing-mode","tb")
    ]

    rule ".tabs-right .tab-label>i" [
        Css.custom("writing-mode","tb")
    ]


    rule ".dock-resize-handle" [
        Css.positionAbsolute
        Css.backgroundColor theme.Handle
        Css.custom( "--resize-handle-thickness", "1px" )
        Css.zIndex (99)
    ]

    rule ".dock-resize-handle.vertical" [
        Css.custom( "height", "var(--resize-handle-thickness)")
        Css.width (percent 100)
        Css.cursorNorthSouthResize
    ]

    rule ".dock-resize-handle.vertical::after" [
        Css.custom("content", "\"\"")
        Css.positionAbsolute
        Css.custom("height", "8px" )
        Css.marginTop(px -4)
        Css.left 0
        Css.right 0
        Css.cursorNorthSouthResize
        Css.backgroundColor "transparent"
        Css.zIndex 100
    ]


    rule ".dock-resize-handle.horizontal" [
        //Css.top (px 0)
        Css.height (percent 100)
        Css.custom( "width", "var(--resize-handle-thickness)")
        Css.cursorEastWestResize
    ]

    rule ".dock-resize-handle.horizontal::after" [
        Css.custom("content", "\"\"")
        Css.positionAbsolute
        Css.custom("width", "8px" )
        Css.marginLeft(px -4)
        Css.top 0
        Css.bottom 0
        Css.cursorEastWestResize
        Css.backgroundColor "transparent"
        Css.zIndex 100
    ]

    rule ".dock-resize-handle.right" [
        Css.right (px 0)
    ]

    rule ".dock-resize-handle.left" [
        Css.left (px 0)
    ]

    rule ".dock-resize-handle.top" [
        Css.top (px 0)
    ]

    rule ".dock-resize-handle.bottom" [
        Css.bottom (px 0)
    ]


    // Borders

    rule ".border" [
        Css.borderStyleSolid
        Css.borderColor theme.Border
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
        Css.backgroundColor theme.Overlay
    ]

    rule ".preview" [
        Css.backgroundColor theme.Overlay
    ]


    // ------------------------------------------------------------------------------
    // Overlays

    rule ".overlays" [ // Sibling to main-grid, child of top-level dock-grid
        Css.displayGrid
        Css.gridRow ("2","4")
        Css.gridColumn ("2", "4")
        Css.width (percent 100)
        Css.height (percent 100)
        Css.custom("grid-template-columns", "1fr 1fr")
    ]

    rule ".overlays-left" [
        Css.displayGrid
        Css.width (percent 100)
        Css.height (percent 100)
        Css.custom("grid-template-rows", "200px 1fr 1fr 200px")
        Css.custom("grid-template-columns", "200px auto")
    ]

    rule ".overlays-right" [
        Css.displayGrid
        Css.width (percent 100)
        Css.height (percent 100)
        Css.custom("grid-template-rows", "200px 1fr 1fr 200px")
        Css.custom("grid-template-columns", "auto 200px")
    ]


    rule ".drag-overlay" [
        Css.displayNone
        Css.positionRelative
        Css.backgroundColor (theme.Overlay)
        Css.zIndex 999
        Css.width (percent 100)
        Css.height (percent 100)
    ]

    rule ".drag-overlay.left-top" [
        Css.gridRow ("1","3")
        Css.gridColumn ("1", "1")
    ]

    rule ".drag-overlay.left-bottom" [
        Css.gridRow ("3","5")
        Css.gridColumn ("1", "1")
    ]

    rule ".drag-overlay.right-top" [
        Css.gridRow ("1","3")
        Css.gridColumn ("2", "2")
    ]

    rule ".drag-overlay.right-bottom" [
        Css.gridRow ("3","5")
        Css.gridColumn ("2", "2")
    ]

    rule ".drag-overlay.bottom-left" [
        Css.gridRow ("4","4")
        Css.gridColumn ("1", "3")
    ]

    rule ".drag-overlay.bottom-right" [
        Css.gridRow ("4","4")
        Css.gridColumn ("1", "3")
    ]

    rule ".drag-overlay.top-left" [
        Css.gridRow ("1","1")
        Css.gridColumn ("1", "3")
    ]

    rule ".drag-overlay.top-right" [
        Css.gridRow ("1","1")
        Css.gridColumn ("1", "3")
    ]

    rule ".drag-overlay.visible" [
        Css.displayBlock
    ]

    rule ".tab-label.dragging" [
        //Css.displayNone
        Css.backgroundColor theme.Overlay
        Css.color theme.Overlay
    ]


    rule ".dragimage" [
        Css.positionAbsolute
        Css.left (px -200)
        Css.top (px -200)
    ]

    rule ".pane-header" [
        Css.padding (px 2)
        Css.paddingLeft (rem 0.5)
        Css.paddingRight (rem 0.5)
        //Css.height (rem 1.5)
        Css.backgroundColor (theme.ControlBackground)
        Css.fontSize (percent 75)
        Css.displayFlex
        Css.flexDirectionColumn
        Css.justifyContentCenter
        Css.height (rem 1.5)
    ]

    rule ".pane-header>div" [
        Css.displayFlex
        Css.flexDirectionRow
        Css.justifyContentSpaceBetween
        Css.alignItemsCenter
    ]

    rule ".pane-content" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.width (percent 100)
        Css.height (percent 100)
        Css.overflowAuto
        Css.backgroundColor theme.ContentBackground
    ]

]

let private toolbarStyling (theme : Theme) = [

    rule ".xd-toolbar" [
        Css.fontSize (percent 75)
        Css.displayFlex
        Css.flexDirectionRow
        Css.backgroundColor theme.ControlBackground
    ]

    rule ".button-group" [
        Css.displayFlex
        Css.flexDirectionRow
        Css.gap (px 4)
    ]

    rule ".menu-stack" [
        Css.positionAbsolute
        Css.displayNone
        Css.flexDirectionColumn
        Css.gap (px 4)
        Css.backgroundColor (theme.ControlBackground)
        Css.boxShadow "0 2px 5px 0 rgba(0,0,0,.5)"
        Css.zIndex 999
//        Css.custom("top", "calc(100% + 0px)")
    ]

    rule ".xd-toolbar .menu-stack" [
        Css.left 0
        Css.top (percent 100)
    ]

    // Menus on left hand
    rule ".dock-left-hand .menu-stack" [
        Css.left 0
        Css.top (percent 100)
    ]

    rule ".dock-left-hand .item-menu>.menu-stack" [
        Css.left (percent 50)
        Css.top (percent 100)
    ]

    // Menus on right side
    rule ".dock-right-hand .menu-stack" [
        Css.right 0
        Css.top (percent 100)
    ]

    rule ".dock-bottom-left-container .menu-stack" [
        Css.left 0
        Css.bottom (0)
        Css.custom("top", "unset")
    ]

    rule ".dock-bottom-left-container .item-menu>.menu-stack" [
        Css.left (percent 50)
        Css.bottom (0)
        Css.custom("top", "unset")
    ]

    rule ".dock-bottom-right-container .item-menu>.menu-stack" [
        Css.bottom (0)
        Css.custom("top", "unset")
    ]

    rule "*:focus-within>.menu-stack" [
        Css.displayFlex
    ]

    rule ".item-dropdown" [
        Css.positionRelative
    ]

    rule ".item-button" [
        Css.cursorPointer
        Css.positionRelative
        Css.padding (px 4)
        Css.displayGrid
        Css.custom("grid-template-columns", "1.2rem auto")
        Css.alignItemsCenter
        Css.borderStyleNone
    ]

    rule ".item-group-right" [
        Css.displayFlex
        Css.flexDirectionRow
        Css.justifyContentFlexEnd
        Css.flexGrow 1
        Css.marginRight (rem 1)
    ]

    rule ".menu-stack>.item-button" [
        Css.custom("grid-template-columns", "1.2rem auto 1rem")
    ]

    rule ".xd-toolbar>.item-button" [
        Css.displayFlex
        Css.flexDirectionRow
        Css.gap (rem 0.25)
        Css.custom("grid-template-columns", "unset")
        Css.paddingLeft (rem 0.5)
        Css.paddingRight (rem 0.5)
    ]

    rule ".menu-stack>.item-button.checkbox" [
        Css.custom("grid-template-columns", "1.2rem auto 1rem")
    ]

    rule ".menu-stack>.item-button>span" [
        Css.paddingRight (rem 0.5)
    ]

    rule ".item-button>i" [
        Css.color theme.Icon
        Css.textAlignCenter
    ]

    rule ".item-button:hover" [
        Css.backgroundColor theme.BackgroundHover
    ]

    rule ".menu-stack>.item-button:hover" [
        Css.backgroundColor theme.Highlight
    ]

    rule ".item-button:active" [
        Css.backgroundColor theme.BackgroundSelected
        Css.color ("inherit")
    ]

    rule ".fa-check" [
        Css.opacity 0
    ]
    rule ".fa-check.checked" [
        Css.opacity 1
    ]

    rule ".xd-toolbar .separator" [
        Css.borderStyleSolid
        Css.borderWidth (px 0)
        Css.borderLeftWidth (px 1)
        Css.borderColor theme.Border
    ]

    rule ".item-text" [
        Css.alignSelfCenter
        Css.displayInlineElement
        Css.paddingLeft (rem 0.5)
        Css.paddingRight (rem 0.5)
    ]

]

let overrides (theme : Theme) = [
    rule ".hidden" [
        Css.displayNone
    ]
    rule ".dock-pane-wrapper" [
        Css.displayNone
        Css.width (percent 100)
        Css.height (percent 100)
    ]

    rule ".dock-pane-wrapper.selected" [
        Css.displayFlex
        Css.flexDirectionColumn
    ]
]

let Styling (t : Theme) = baseStyling t @ dockStyling t @ toolbarStyling t @ overrides t

open Browser.DomExtensions
open Browser.CssExtensions
open Browser.Dom

//let oxide (e : Sutil.DOM.SutilElement) = e |> withStyle Styling

let installStyling (t : Theme) =
    addStyleSheet Browser.Dom.document "" (Styling t)