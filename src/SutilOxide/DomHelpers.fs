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

    let setPaneFlexGrow (el : HTMLElement) (w : int) =
        el.style.flexGrow <- $"{w}"

    let getPaneWidth (el : HTMLElement) =
        (window.getComputedStyle el).width[..(-3)] |> System.Double.Parse |> int

    let setPaneWidth (el : HTMLElement) (w : int) =
        el.style.width <- $"{w}px"

    let getPaneHeight (el : HTMLElement) =
        console.log((window.getComputedStyle el).height)
        (window.getComputedStyle el).height[..(-3)] |> System.Double.Parse |> int

    let setPaneHeight (el : HTMLElement) (h : int) =
        el.style.height <- $"{h}px"

    let setPaneSizeUsingFlexGrow (getSize : HTMLElement -> int) (el : HTMLElement) (size : int) =
        let parentSz = getSize (el.parentElement)
        let pct = (float size) / (float parentSz)

        setPaneFlexGrow el (int (pct * 10000.0))
        setPaneFlexGrow (el.previousElementSibling |> toEl) (int ( (1.0 - pct) * 10000.0))

    let setPaneWidthUsingFlexGrow =
        setPaneSizeUsingFlexGrow getPaneWidth

    let setPaneHeightUsingFlexGrow  =
        setPaneSizeUsingFlexGrow getPaneHeight

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
            | CentreCentre -> "#dock-centre-centre"
        document.querySelector (contentId) :?> HTMLElement

    let getWrapperNode (name : string) =
        document.querySelector("#pane-" + name.ToLower())


    // https://jsfiddle.net/x9o7y561/
    let resizeController
            (pos : MouseEvent -> float)
            (getSize : HTMLElement -> int)
            (setSize : HTMLElement -> int -> unit)
            (commit : HTMLElement -> int -> unit)
            (direction : int) =
        Sutil.Html.Ev.onMouseDown (fun e ->
            e.preventDefault()
            let pane = ((targetEl e).parentElement) :?> HTMLDivElement
            let posOffset: float = pos e
            let startSize = float (getSize pane)
            let rec mouseDragHandler = fun (e : Browser.Types.MouseEvent) ->
                e.preventDefault()
                let primaryButtonPressed = e.buttons = 1

                if not primaryButtonPressed then
                    commit pane (int ((posOffset - pos e) * (float direction) + startSize))
                    document.body.removeEventListener("pointermove", !!mouseDragHandler)
                else
                    setSize pane (int ((posOffset - pos e) * (float direction) + startSize))

            document.body.addEventListener("pointermove", !!mouseDragHandler)
        )

    let resizeControllerEw (direction : int) =
        resizeController (fun e -> e.pageX) getPaneWidth setPaneWidth setPaneWidth direction

    let resizeControllerNs (direction : int) =
        resizeController (fun e -> e.pageY) getPaneHeight setPaneHeight setPaneHeight direction

    let resizeControllerNsFlex (direction : int) =
        resizeController (fun e -> e.pageY) getPaneHeight setPaneHeightUsingFlexGrow setPaneHeightUsingFlexGrow direction

    let resizeControllerEwFlex (direction : int) =
        resizeController (fun e -> e.pageX) getPaneWidth setPaneWidthUsingFlexGrow setPaneWidthUsingFlexGrow direction

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
