module SutilOxide.UICss 

open Sutil.Styling
open type Feliz.length
open Sutil

let style = [

    rule ".ui-toolbar" [
        Css.fontSize (percent 70)
        Css.displayFlex
        Css.flexDirectionRow
        Css.gap (rem 0.2)
        Css.padding (rem 0.2)
    ]

    rule ".ui-toolbar .vertical" [
        Css.flexDirectionColumn
    ]

    rule ".ui-ribbon" [
        Css.fontSize (percent 70)
        Css.height (px 104)
        Css.maxHeight (px 104)
        Css.borderRadius (px 8)
        Css.backgroundColor "white"
        Css.custom("filter", "drop-shadow(2px 2px 2px rgb(220,220,220))")
        Css.displayFlex
        Css.flexDirectionRow
        Css.gap (rem 0.5)
        Css.padding (rem 0.2)
    ]

    rule ".ui-vsep" [
        let margin = 3
        Css.custom("height", sprintf "calc(100%% - %dpx)" (2 * margin))
        Css.margin( px margin, px 0 )
        Css.width (px 0)
        Css.custom ("border-left", "1px solid #cccccc")
    ]

    rule ".ui-ribbon i" [
        Css.color "#50719B"
        Css.textAlignCenter
    ]

    rule ".ui-ribbon i.left" [
        Css.marginRight (rem 0.25)
    ]

    rule ".ui-ribbon i.right" [
        Css.marginLeft (rem 0.25)
    ]

    rule ".ui-control" [
        Css.positionRelative
        Css.padding (rem 0.3)
        Css.cursorPointer
        Css.borderRadius (px 3)
        Css.custom("transition", "background-color 100ms ease-in-out")
        Css.whiteSpaceNowrap
    ]

    rule ".ui-toolbar .ui-control" [
        Css.padding (rem 0.2)
    ]


    rule ".ui-menu-stack > .ui-control" [
        Css.paddingLeft (rem 1)
        Css.paddingRight (rem 1.5)
    ]

    rule ".ui-control:hover" [
        // Css.backgroundColor "#f0f0f0"
        Css.backgroundColor "#f8f8f8"
        Css.custom("transition", "background-color 100ms ease-in-out")
    ]

    rule ".ui-group" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.justifyContentSpaceBetween
    ]

    rule ".ui-stacks" [
        Css.displayFlex
        Css.flexDirectionRow
    ]

    rule ".ui-stack" [
        Css.displayFlex
        Css.flexDirectionColumn
    ]

    rule ".ui-menu-stack" [
        includeRule("ui-stack")

        Css.maxHeight(px 300)
//        Css.overflowScroll

        Css.positionAbsolute
        Css.backgroundColor "white"
        Css.opacity 0
        Css.custom ("pointer-events", "none")
        Css.gap (px 4)
        Css.boxShadow "0 2px 5px 0 rgba(0,0,0,.5)"
        Css.zIndex 999

        Css.custom("width", "max-content")
        Css.top 0
        Css.left (percent 100)
    ]

    rule "*:focus-within>.ui-menu-stack" [
        Css.transitionDuration (System.TimeSpan.FromSeconds(0.3))
        Css.opacity 1
        Css.custom ("pointer-events", "auto")
    ]

    rule ".ui-group-items" [
        Css.padding (rem 0.5)
        Css.displayFlex
        Css.flexDirectionColumn
        Css.flexWrapWrap
        Css.minHeight 0
    ]

    rule ".ui-group-label" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.alignItemsCenter
        Css.justifyContentCenter
        Css.marginBottom (px 2)
        Css.fontSize (percent 90)
    ]
]


let styleInput = [

    rule ".ui-input" [
        Css.displayFlex        
    ]

    rule ".ui-input label" [
        Css.fontWeightBold
        Css.minWidth (rem 1)
    ]

    rule ".ui-input input" [
        Css.borderStyleNone
        Css.borderBottom (px 1, Feliz.borderStyle.solid, "transparent")
        Css.width (percent 100)
    ]

    rule ".ui-input textarea" [
        Css.border (px 1, Feliz.borderStyle.solid, "transparent")
        Css.minWidth (percent 100)
        Css.maxWidth (percent 100)
    ]

    rule ".ui-input input:focus" [
        Css.outlineStyleNone
        Css.borderColor "#eeeeee"
    ]

    rule ".ui-input textarea:focus" [
        Css.outlineStyleNone
        Css.borderColor "#eeeeee"
    ]

    rule ".ui-input.vertical" [
        Css.flexDirectionColumn
        Css.width (percent 100)
        Css.alignItemsFlexStart
    ]

    rule ".ui-input.horizontal" [
        Css.flexDirectionRow
        Css.alignItemsCenter
    ]
]

let styleRibbonMenu = [

    rule ".ui-ribbonmenu" [
        Css.marginTop (rem 0.1)
        // Css.marginLeft (rem 0.5)
        // Css.marginRight (rem 0.5)
        Css.marginBottom (rem 0.5)
    ]

    rule ".ui-rm-ribboncontainer" [
        Css.positionRelative
        Css.zIndex 999
    ]

    rule ".ui-rm-menu" [
        Css.marginLeft (rem 0.5)
        Css.displayFlex
        Css.flexDirectionRow
        Css.gap (rem 1)
    ]

    rule ".ui-rm-label" [
        Css.color "gray"
        Css.positionRelative
        Css.cursorPointer
        Css.custom("width", "fit-content")
        Css.marginBottom (rem 0.5)
    ]

    rule ".ui-rm-label" [
        Css.color "black"
        Css.fontWeightBold
    ]

    rule ".ui-rm-label:after" [
        Css.positionAbsolute
        Css.top 0
        Css.left 0
        Css.right 0
        Css.bottom 0
        Css.custom("content","''") 
        Css.displayNone
        Css.custom("border-bottom", "2px solid #dddddd")
    ]

    rule ".ui-rm-label:hover::after" [
        Css.displayBlock
    ]

    rule ".ui-rm-label.selected::after" [
        Css.displayBlock
        Css.custom("border-bottom", "2px solid blue")
        Css.transitionDurationMilliseconds (200)
    ]

    rule ".ui-rm-label.selected:hover::after" [
        Css.left (px -5)
        Css.right (px -5)
        Css.transitionDurationMilliseconds (200)
    ]
]
