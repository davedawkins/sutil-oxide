module SutilOxide.DomHelpers

//
// Copyright (c) 2022 David Dawkins
//

open Browser.Dom
open Browser.CssExtensions

open type Feliz.length
open Fable.Core
open Fable.Core.JsInterop
open Browser.Types
open SutilOxide.Types
open Sutil
open Sutil.DomHelpers

[<AutoOpen>] 
module ResizeController =
    let toEl (et : EventTarget) = et :?> HTMLElement

    let setPaneFlexGrow (el : HTMLElement) (w : int) =
        el.style.flexGrow <- $"{w}"

    let setPaneFlexBasis (el : HTMLElement) (w : int) =
        el.style.flexBasis <- $"{w}px"

    let getPaneWidth (el : HTMLElement) =
        let widthStr = (window.getComputedStyle el).width
        try
            if widthStr = "auto" then // Probably hidden
                el.clientWidth |> int
            else
                widthStr[..(-3)] |> System.Double.Parse |> int
        with
        | x ->
            Fable.Core.JS.console.error( sprintf "Could not parse width: '%s'" widthStr, el )
            el.clientWidth |> int

    let setPaneWidth (el : HTMLElement) (w : int) =
        el.style.width <- $"{w}px"

    let getPaneHeight (el : HTMLElement) =
        // console.log((window.getComputedStyle el).height)
        (window.getComputedStyle el).height[..(-3)] |> System.Double.Parse |> int

    let setPaneHeight (el : HTMLElement) (h : int) =
        el.style.height <- $"{h}px"

    let setPaneSizeUsingFlexGrow (getSize : HTMLElement -> int) ((el,el2) : HTMLElement * HTMLElement) (size : int) =
        let parentSz = getSize (el.parentElement)
        let pct = (float size) / (float parentSz)

        setPaneFlexGrow el (int (pct * 10000.0))
        setPaneFlexGrow el2 (int ( (1.0 - pct) * 10000.0))
        // setPaneFlexGrow (el.previousElementSibling |> toEl) (int ( (1.0 - pct) * 10000.0))

    let setPaneWidthUsingFlexGrow =
        setPaneSizeUsingFlexGrow getPaneWidth

    let setPaneHeightUsingFlexGrow  =
        setPaneSizeUsingFlexGrow getPaneHeight

    // https://jsfiddle.net/x9o7y561/
    let resizeController
            (pos : MouseEvent -> float)
            (getPanes : HTMLElement -> (HTMLElement * HTMLElement))
            (getSize : (HTMLElement * HTMLElement) -> int)
            (setSize : (HTMLElement * HTMLElement) -> int -> unit)
            //(commit : (HTMLElement * HTMLElement) -> int -> unit)
            (commit : unit -> unit)
            (direction : int) =

        Ev.onMouseDown (fun e ->
            e.preventDefault()
            let (pane, pane2) as panes = (e.targetHtmlElement) |> getPanes
            let posOffset: float = pos e
            let startSize = float (getSize panes)
            let rec mouseDragHandler = fun (e : Browser.Types.MouseEvent) ->
                e.preventDefault()
                let primaryButtonPressed = e.buttons = 1

                if not primaryButtonPressed then
                    setSize panes (int ((posOffset - pos e) * (float direction) + startSize))
                    commit() // panes (int ((posOffset - pos e) * (float direction) + startSize))
                    document.body.removeEventListener("pointermove", !!mouseDragHandler)
                    Toolbar.MenuMonitor.monitorAll()
                else
                    setSize panes (int ((posOffset - pos e) * (float direction) + startSize))

            document.body.addEventListener("pointermove", !!mouseDragHandler)
        )

    // https://jsfiddle.net/x9o7y561/
    let resizeControllerXY
            (pos : string -> MouseEvent -> float)
            (getPanes : HTMLElement -> (HTMLElement * HTMLElement))
            (getSize : string -> (HTMLElement * HTMLElement) -> int)
            (setSize : string -> (HTMLElement * HTMLElement) -> int -> unit)
            //(commit : (HTMLElement * HTMLElement) -> int -> unit)
            (commit : float -> unit)
            (direction : int) =

        Ev.onMouseDown (fun e ->
            e.preventDefault()
            let el = e.targetHtmlElement
            let xy = if el.clientHeight > el.clientWidth then "x" else "y"

            let (pane, pane2) as panes = (e.targetHtmlElement) |> getPanes
            let posOffset: float = pos xy e
            let startSize = float (getSize xy panes)
            let rec mouseDragHandler = fun (e : Browser.Types.MouseEvent) ->
                e.preventDefault()
                let primaryButtonPressed = e.buttons = 1

                if not primaryButtonPressed then
                    // let sz = int ((posOffset - pos xy e) * (float direction) + startSize)
                    // setSize xy panes sz
                    commit (getSize xy panes)
                    document.body.removeEventListener("pointermove", !!mouseDragHandler)
                    Toolbar.MenuMonitor.monitorAll()
                else
                    setSize xy panes (int ((posOffset - pos xy e) * (float direction) + startSize))

            document.body.addEventListener("pointermove", !!mouseDragHandler)
        )

    let prevSibling (e : HTMLElement) = e.previousElementSibling :?> HTMLElement
    let nextSibling (e : HTMLElement) = e.nextElementSibling :?> HTMLElement
    let getPanes (e : HTMLElement) = prevSibling e, nextSibling e

    module PrevSibling =

        let resizeControllerEw (direction : int) commit =
            resizeController (fun e -> e.pageX) getPanes (fst>>getPaneWidth) (fst>>setPaneWidth) commit -direction

        let resizeControllerNs (direction : int) commit =
            resizeController (fun e -> e.pageY) getPanes (fst>>getPaneHeight) (fst>>setPaneHeight) commit -direction

        let resizeControllerNsFlex (direction : int) commit =
            resizeController (fun e -> e.pageY) getPanes (fst>>getPaneHeight) setPaneHeightUsingFlexGrow commit -direction

        let resizeControllerEwFlex (direction : int) commit =
            resizeController (fun e -> e.pageX) getPanes (fst>>getPaneWidth) setPaneWidthUsingFlexGrow commit -direction

        let resizeControllerFlexXY (direction : int) commit =
            resizeControllerXY
                (fun xy e -> match xy with "x" -> e.pageX | _ -> e.pageY) 
                getPanes
                (fun xy (p1,p2) -> match xy with "x" -> p1 |> getPaneWidth |_ -> p1 |> getPaneHeight)
                (fun xy (p1,p2) sz -> match xy with "x" -> setPaneWidthUsingFlexGrow (p1,p2) sz |_ -> setPaneHeightUsingFlexGrow (p1,p2) sz)
                commit -direction

        let resizeControllerFlexBasisXY (direction : int) (commit : float -> unit) =
            resizeControllerXY
                (fun xy e -> match xy with "x" -> e.pageX | _ -> e.pageY) 
                getPanes
                (fun xy (p1,p2) -> match xy with "x" -> p1 |> getPaneWidth |_ -> p1 |> getPaneHeight)
                (fun xy (p1,p2) sz -> setPaneFlexBasis p1 sz)
                commit -direction

        let resizeControllerXY (direction : int) commit =
            resizeControllerXY
                (fun xy e -> match xy with "x" -> e.pageX | _ -> e.pageY) 
                getPanes
                (fun xy (p1,p2) -> match xy with "x" -> p1 |> getPaneWidth |_ -> p1 |> getPaneHeight)
                (fun xy (p1,p2) sz -> match xy with "x" -> setPaneWidth p1 sz |_ -> setPaneHeight p1 sz)
                commit -direction

    module NextSibling =

        let resizeControllerFlexBasisXY (direction : int) (commit : float -> unit) =
            resizeControllerXY
                (fun xy e -> match xy with "x" -> e.pageX | _ -> e.pageY) 
                getPanes
                (fun xy (_,p2) -> match xy with "x" -> p2 |> getPaneWidth |_ -> p2 |> getPaneHeight)
                (fun xy (_,p2) sz -> setPaneFlexBasis p2 sz)
                commit direction

    module ParentPane =

        let getPanes (e : HTMLElement) = e.parentElement, prevSibling (e.parentElement)

        let resizeControllerEw (direction : int) commit =
            resizeController (fun e -> e.pageX) getPanes (fst>>getPaneWidth) (fst>>setPaneWidth) commit direction

        let resizeControllerNs (direction : int) commit =
            resizeController (fun e -> e.pageY) getPanes (fst>>getPaneHeight) (fst>>setPaneHeight) commit direction

        let resizeControllerNsFlex (direction : int) commit =
            resizeController (fun e -> e.pageY) getPanes (fst>>getPaneHeight) setPaneHeightUsingFlexGrow commit direction

        let resizeControllerEwFlex (direction : int) commit =
            resizeController (fun e -> e.pageX) getPanes (fst>>getPaneWidth) setPaneWidthUsingFlexGrow commit direction


[<AutoOpen>]
module DomHelpers =

    let toEl (et : EventTarget) = et :?> HTMLElement
    let targetEl (e : Event) = e.target |> toEl
    let currentEl (e : Event) = e.currentTarget |> toEl

    let getPaneFlexGrow (el : HTMLElement) =
        let cs = (window.getComputedStyle el)
        try
            cs.flexGrow |> System.Double.Parse |> int
        with
        | _ -> 0

    let getContentParentNode (location : DockLocation) =
        let contentId =
            match location with
            | LeftTop     -> "#dock-left-top"
            | LeftBottom  -> "#dock-left-bottom"
            | RightTop    -> "#dock-right-top"
            | RightBottom -> "#dock-right-bottom"
            | BottomLeft  -> "#dock-bottom-left"
            | BottomRight -> "#dock-bottom-right"
            | TopLeft -> "#dock-top-left"
            | TopRight -> "#dock-top-right"
            | CentreLeft -> "#dock-centre-left"
            | CentreRight -> "#dock-centre-right"
        document.querySelector (contentId) :?> HTMLElement

    let getWrapperNode (name : string) =
        document.querySelector("#pane-" + name.ToLower())


    let toListFromNodeList (l : NodeListOf<'a>) =
        [0..l.length-1] |> List.map (fun i -> l.item(i),i)


    let attributesAsList (e : HTMLElement) =
        let l = e.attributes
        [0..l.length-1] |> List.map (fun i -> l.item(i),i)

    let containsByWidth clientX (el : HTMLElement) =
        let r = el.getBoundingClientRect()
        clientX >= r.left && clientX <= r.right

    let whichHalfX clientX (el : HTMLElement) =
        let r = el.getBoundingClientRect()
        if clientX < (r.left + r.width / 2.0) then FirstHalf else SecondHalf

    let whichHalfY clientY (el : HTMLElement) =
        let r = el.getBoundingClientRect()
        if clientY < (r.top + r.height / 2.0) then FirstHalf else SecondHalf

    let containsByHeight clientY (el : HTMLElement) =
        let r = el.getBoundingClientRect()
        clientY >= r.top && clientY <= r.bottom

    let clearPreview() =
        document.querySelectorAll(".tab-label")
        |> toListFromNodeList
        |> List.iter (fun (el,_) ->
                el.classList.remove("preview-insert-before")
                el.classList.remove("preview-insert-after")
        )
