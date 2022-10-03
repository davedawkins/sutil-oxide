module Dock

open Browser.Dom
open Browser.CssExtensions
//open Browser.DomExtensions


open Sutil.DOM
open Sutil
open Sutil.Styling
open type Feliz.length
open Fable.Core
open Fable.Core.JsInterop
open Browser.Types

type UI =
    static member divc (cls:string) (items : seq<SutilElement>) =
        Html.div [ Attr.className cls ; yield! items ]

module Palette =
    let border = "#EEEEEE"
    let background = "#FFFFFF"
    let backgroundHover = "#EEEEEE"
    let backgroundSelected = "#CCCCCC"
    let handle = border
    let icon = "#AAAAAA"
    let preview = "#b9d3de"


type TabHalf =
    | FirstHalf | SecondHalf

type DockLocation =
    | LeftTop
    | LeftBottom
    | BottomLeft
    | BottomRight
    | RightTop
    | RightBottom
with
    static member All =
        [
            LeftTop; LeftBottom; BottomLeft; BottomRight; RightTop; RightBottom
        ]

type DockPane = {
    Name : string
}

type DockStation = {
    Panes : DockPane list
}
with
    static member Empty = { Panes = [] }

type DockCollection = {
    Stations : Map<DockLocation,DockStation>
}
with
    static member Empty =
        {
            Stations = DockLocation.All |> List.fold (fun s e -> s.Add(e, DockStation.Empty)) Map.empty
        }
    member __.GetPanes loc = __.Stations[loc].Panes

type DraggingTab = {
    BeingDragged : string
    Preview : (DockLocation * int) option
}

type Model = {
    Docks : DockCollection
    DraggingTab : DraggingTab option
    SelectedPanes : Map<DockLocation,string option>
}

[<AutoOpen>]
module DomHelpers =

    let toEl (et : EventTarget) = et :?> HTMLElement
    let targetEl (e : Event) = e.target |> toEl

    let getPaneWidth (el : HTMLElement) =
        (window.getComputedStyle el).width[..(-3)] |> System.Double.Parse |> int

    let setPaneWidth (el : HTMLElement) (w : int) =
        el.style.width <- $"{w}px"

    let getPaneHeight (el : HTMLElement) =
        console.log((window.getComputedStyle el).height)
        (window.getComputedStyle el).height[..(-3)] |> System.Double.Parse |> int

    let setPaneHeight (el : HTMLElement) (h : int) =
        el.style.height <- $"{h}px"

    let getContentParentNode (location : DockLocation) =
        let contentId =
            match location with
            | LeftTop | LeftBottom -> "#dock-left-content-id"
            | RightTop | RightBottom -> "#dock-right-content-id"
            | BottomLeft | BottomRight -> "#dock-bottom-content-id"
        document.querySelector (contentId) :?> HTMLElement

    let getWrapperNode (name : string) =
        document.querySelector("#pane-" + name.ToLower())

    // https://jsfiddle.net/x9o7y561/
    let resizeController
            (pos : MouseEvent -> float)
            (getSize : HTMLElement -> int)
            (setSize : HTMLElement -> int -> unit)
            (direction : int) =
        Ev.onMouseDown (fun e ->
            e.preventDefault()
            let pane = ((targetEl e).parentElement) :?> HTMLDivElement
            let posOffset: float = pos e
            let startSize = float (getSize pane)
            let rec mouseDragHandler = fun (e : Browser.Types.MouseEvent) ->
                e.preventDefault()
                let primaryButtonPressed = e.buttons = 1

                if not primaryButtonPressed then
                    document.body.removeEventListener("pointermove", !!mouseDragHandler)
                else
                    setSize pane (int ((posOffset - pos e) * (float direction) + startSize))

            document.body.addEventListener("pointermove", !!mouseDragHandler)
        )

    let resizeControllerEw (direction : int) =
        resizeController (fun e -> e.pageX) getPaneWidth setPaneWidth direction

    let resizeControllerNs (direction : int) =
        resizeController (fun e -> e.pageY) getPaneHeight setPaneHeight direction
    let toListFromNodeList (l : NodeListOf<'a>) =
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

module DockHelpers =
    let tabsContains name tabLabels=
        tabLabels |> List.exists (fun t -> t.Name = name)

    let removeFromPanesList panes name =
        panes |> List.filter (fun t -> t.Name <> name)

    let insertIntoPanes (panes : List<'a>) pane i =
        if i >= panes.Length then
            panes @ [ pane ]
        else
            panes |> List.insertAt i pane

    let findPaneLocationIndex (docks : DockCollection) name =
        docks.Stations |> Map.tryPick (fun loc station -> station.Panes |> List.tryFindIndex (fun t -> t.Name = name) |> Option.map (fun i -> loc,i))

    let findPaneLocation(docks : DockCollection) name = findPaneLocationIndex docks name |> Option.map fst

    let getPanes (docks : DockCollection) loc =
        docks.Stations[loc].Panes

    let setPanes (docks : DockCollection) loc value =
        let dock = docks.Stations[loc]
        { docks with Stations = docks.Stations.Add(loc, { dock with Panes = value}) }

    let getPane docks name =
        match findPaneLocation docks name with
        | None -> failwith "Not found"
        | Some (loc) ->
            getPanes docks loc |> List.find (fun t -> t.Name = name)

    let removeFromPanes docks name =
        match findPaneLocation docks name with
        | None -> failwith "Not found"
        | Some (cloc) ->
            let tabLabels = removeFromPanesList (getPanes docks cloc) name
            setPanes docks cloc tabLabels

    let moveTab (model : Model) name (loc : DockLocation) index =
        match findPaneLocation model.Docks name with
        | None -> model
        | Some (currentLoc) ->
            let pane = getPane model.Docks name
            let docks' = removeFromPanes model.Docks name
            let panes = getPanes docks' loc
            let panes' = insertIntoPanes panes pane index
            { model with
                Docks = setPanes docks' loc panes'
                SelectedPanes =
                    model.SelectedPanes
                        .Add(currentLoc,None)
                        .Add(loc,Some name)
            }

    let ensurePaneSelected (m : Model) =
        DockLocation.All
        |> List.map (fun loc ->
                let selectedPaneName =
                        m.SelectedPanes[loc]
                        |> Option.orElseWith (fun () -> m.Docks.GetPanes(loc) |> List.tryHead |> Option.map (fun p -> p.Name))
                loc, selectedPaneName)
        |> Map.ofList
        |> (fun map -> { m with SelectedPanes = map } )

type Message =
    | AddTab of (string*string*DockLocation)
    | SetDragging of string
    | CancelDrag
    | PreviewDockLocation of (DockLocation * int) option
    | CommitDrag
    | SelectPane of DockLocation*string option
    | TogglePane of DockLocation*string

let init docks =
    {
        Docks = docks
        DraggingTab = None
        SelectedPanes = DockLocation.All |> List.fold (fun s loc -> s.Add(loc, None)) Map.empty
    } |> DockHelpers.ensurePaneSelected, Cmd.none

let update msg model =
    console.log($"{msg}")
    match msg with
    | AddTab (name,icon,location) ->
        let station = model.Docks.Stations[location]
        let panes = station.Panes @ [ { Name = name }]
        let station' = { station with Panes = panes }
        {
            model with
                Docks = { Stations = model.Docks.Stations.Add( location, station' ) }
        } |> DockHelpers.ensurePaneSelected, Cmd.none

    | SelectPane (loc,pane) ->
        { model with SelectedPanes = model.SelectedPanes.Add(loc,pane) }, Cmd.none

    | TogglePane (loc,pane) ->
        let selected =
            match model.SelectedPanes[loc] with
            | Some name when name = pane -> None
            | _ -> Some pane

        console.log(sprintf "Select: %A %A" loc selected)
        { model with SelectedPanes = model.SelectedPanes.Add(loc,selected) }, Cmd.none

    | SetDragging d ->
        { model with DraggingTab = Some { BeingDragged = d; Preview = None} }, Cmd.none

    | CommitDrag ->
        let m =
            model.DraggingTab
            |> Option.bind
                (fun dt ->
                    dt.Preview |> Option.map (fun (loc,i) ->
                        let m = DockHelpers.moveTab model dt.BeingDragged loc i

                        let wrapper = DomHelpers.getWrapperNode dt.BeingDragged
                        let parent = DomHelpers.getContentParentNode loc
                        parent.appendChild wrapper |> ignore

                        { m with
                            DraggingTab = None
                        }
                ))
        m |> Option.defaultValue model |> DockHelpers.ensurePaneSelected, Cmd.none

    | CancelDrag ->
        { model with DraggingTab = None }, Cmd.none

    | PreviewDockLocation dockLoc ->
        let m =
            match model.DraggingTab with
            | None -> model
            | Some d ->
                { model with DraggingTab = Some { d with Preview = dockLoc } }
        m, Cmd.none


let private css = [

    rule ".dock-container" [
        Css.width (percent 100)
        Css.height (percent 100)
        Css.backgroundColor Palette.background
        Css.displayGrid
        Css.custom("grid-template-columns", "max-content auto max-content")
        Css.custom("grid-template-rows", "auto max-content")
    ]

    rule ".dock-main-grid" [
        Css.displayGrid
        Css.gridRow ("1","1")
        Css.gridColumn ("2", "2")
        Css.custom("grid-template-columns", "max-content auto max-content")
        Css.custom("grid-template-rows", "auto max-content")
    ]

    rule ".dock-right-container" [
        Css.positionRelative
        Css.width (px 400)
        Css.minWidth (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("3", "3")
    ]

    rule ".dock-left-container" [
        Css.positionRelative
        Css.width (px 400)
        Css.minWidth (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("1", "1")
    ]


    rule ".dock-main" [
        Css.positionRelative
        Css.height (percent 100)
        Css.width (percent 100)
        Css.minHeight (rem 2)
        Css.gridRow ("1","1")
        Css.gridColumn ("2", "2")
    ]

    rule ".dock-bottom-container" [
        Css.positionRelative
        Css.height (px 200)
        Css.minHeight (rem 2)
        Css.gridRow ("2","2")
        Css.gridColumn ("1", "4")
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

    rule ".tabs-left" [
        Css.gridRow ("1","1")
        Css.gridColumn ("1", "1")
        Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-lr")
        Css.custom ("justify-content", "end")
    ]

    rule ".tabs-right" [
        Css.gridRow ("1","1")
        Css.gridColumn ("3", "3")
        Css.minWidth (rem 1)

        Css.custom ("writing-mode", "sideways-rl")
        Css.custom ("justify-content", "start")
    ]

    rule ".tabs-bottom" [
        Css.gridRow ("2","2")
        Css.gridColumn ("2", "2")
        Css.minHeight (rem 1.5)
    ]

    rule ".tabs-bottom-left" [
        Css.gridRow ("2","2")
        Css.gridColumn ("1", "1")
        Css.minHeight (rem 1.5)
    ]

    rule ".tabs-bottom-right" [
        Css.gridRow ("2","2")
        Css.gridColumn ("3", "3")
        Css.minHeight (rem 1.5)
    ]


    rule ".xxdock-left-tabs>div" [
        Css.displayFlex
        Css.flexDirectionColumn
        Css.alignItemsCenter
        //Css.gap (px 4)
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
        Css.zIndex 99
    ]

    rule ".drag-overlay.left-top" [
        Css.top (px 0)
        Css.left (px 0)
        Css.width (px 150)
        Css.height (percent 100)
    ]

    rule ".drag-overlay.right-top" [
        Css.top (px 0)
        Css.right (px 0)
        Css.width (px 150)
        Css.height (percent 100)
    ]

    rule ".drag-overlay.bottom-left" [
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

    let beforeAfter (sideways) = [
        Css.custom("content","\"\"")
        if sideways then
            Css.width (rem 1)
            Css.height (px 80)
        else
            Css.height (rem 1)
            Css.width (px 80)
        Css.backgroundColor Palette.preview
    ]

    rule ".tabs-left .tab-label.preview-insert-before::before" (beforeAfter true)
    rule ".tabs-left .tab-label.preview-insert-after::after"   (beforeAfter true)

    rule ".tabs-bottom .tab-label.preview-insert-before::before" (beforeAfter false)
    rule ".tabs-bottom .tab-label.preview-insert-after::after"   (beforeAfter false)

    rule ".dragimage" [
        Css.positionAbsolute
        Css.left (px -200)
        Css.top (px -200)
    ]

    rule ".hidden" [
        Css.displayNone
    ]
]



[<AutoOpen>]
module ModelHelpers =
    let beingDragged (m : Model) =
        m.DraggingTab |> Option.map (fun p -> p.BeingDragged)

    let childTabIsDragging (model: System.IObservable<Model>) (tabs : Model -> DockPane list) =
            model
            |> Store.map( fun m ->
                    match beingDragged m with
                    | None -> false
                    | Some name  -> tabs m |> List.exists (fun t -> t.Name = name))

    let showOverlay (model : System.IObservable<Model>) (target: DockLocation) =
            model
            |> Store.map( fun (m : Model) ->
                    match m.DraggingTab |> Option.bind (fun (p: DraggingTab) -> p.Preview) with
                    | Some (loc,_) -> loc = target
                    | _ -> false)

module EventHandlers =

    let previewOver dragEl query clientXY contains whichHalf =
        let tabs = document.querySelectorAll( query ) |> toListFromNodeList

        let over =
            tabs
            |> List.tryPick (fun (el,i) -> if contains clientXY (el :?> HTMLElement) then Some (el,i) else None )

        over |> Option.map (fun (el,i) ->
            match whichHalf clientXY (toEl el) with
            | FirstHalf ->
                el.parentElement.insertBefore(dragEl,el) |> ignore
                i
            | SecondHalf ->
                el.parentElement.insertBefore(dragEl,el.nextSibling) |> ignore
                i
        ) |> Option.defaultValue -1

    let dragOver dispatch (e : DragEvent) =
        try
            e.preventDefault()
            //let tabName = e.dataTransfer.getData("text/plain")
            let el = e.currentTarget |> toEl
            let r = el.getBoundingClientRect()
            let x, y = e.clientX - r.left, e.clientY -  r.top

            clearPreview()
            let dragEl = document.querySelector(".dragging") |> toEl

            if (y > (r.height - 200.0)) then
                let i = previewOver dragEl ".tabs-bottom> div" (e.clientX) containsByWidth whichHalfX
                console.log($"BottomLeft {i}")
                if i <> -1 then (Some (BottomLeft,i)) |> PreviewDockLocation |> dispatch

            elif (x < 200) then
                let invert = function FirstHalf-> SecondHalf|_-> FirstHalf
                let i = previewOver dragEl ".tabs-left> div" (e.clientY) containsByHeight (fun a b -> whichHalfY a b |> invert)
                console.log($"LeftTop {i}")
                if i <> -1 then (Some (LeftTop,i)) |> PreviewDockLocation |> dispatch

            elif (x > (r.width - 200.0)) then
                let i = previewOver dragEl ".tabs-right> div" (e.clientY) containsByHeight whichHalfY
                console.log($"RightTop {i}")
                if i <> -1 then (Some (RightTop,i)) |> PreviewDockLocation |> dispatch
            else
                None|> PreviewDockLocation |> dispatch
        with
        | x -> console.log(x.Message)

    let drop dispatch (e : DragEvent) =
        dispatch CommitDrag

    let dragStart tabLabel dispatch (e : DragEvent) =
        e.dataTransfer.setData("text/plain", tabLabel.Name) |> ignore
        dispatch (SetDragging (tabLabel.Name))
        let el = targetEl e
        let img = el.cloneNode(true) |> toEl
        img.classList.add("dragimage")
        document.body.appendChild(img) |> ignore
        el.classList.add("dragging")
        e.dataTransfer?setDragImage(img,0,0)

    let dragEnd dispatch (e : DragEvent) =
        clearPreview()
        document.querySelectorAll(".dragimage") |> toListFromNodeList |> List.iter (fun (el,_) -> (el.remove()))
        dispatch (CancelDrag)
        None |> PreviewDockLocation |> dispatch
        let el = targetEl e
        el.classList.remove("dragging")

    let tabClick dockLocation (tabLabel : DockPane) dispatch (e : MouseEvent) =
        (dockLocation,(tabLabel.Name)) |> TogglePane |>  dispatch

let viewTabLabel (model : System.IObservable<Model>) dispatch dockLocation (tabLabel : DockPane) =
    console.log($"{tabLabel.Name} @ {dockLocation}")
    UI.divc "tab-label" [
        Bind.toggleClass(
            model
            |> Store.map (fun m -> (beingDragged m |> Option.defaultValue "") = tabLabel.Name),
            "preview")

        Bind.toggleClass(
            model
            |> Store.map (fun m -> (m.SelectedPanes[dockLocation] |> Option.defaultValue "") = tabLabel.Name),
            "selected")

        Html.i [ Attr.className "fa fa-folder" ]
        Html.span [ text tabLabel.Name ]

        Attr.draggable true
        Ev.onDragStart (EventHandlers.dragStart tabLabel dispatch)
        Ev.onDragEnd (EventHandlers.dragEnd dispatch)
        Ev.onClick (EventHandlers.tabClick dockLocation tabLabel dispatch)
    ]


type Orientation =
    | Horizontal | Vertical
with
    member __.Opposite = if __ = Horizontal then Vertical else Horizontal

type BasicLocation =
    | Left
    | Right
    | Top
    | Bottom
with
    member __.LowerName = __.ToString().ToLower()
    member __.Orientation =
        match __ with Left|Right -> Horizontal | _ -> Vertical
    member __.Opposite =
        match __ with
        |Left -> Right
        |Right -> Left
        |Top -> Bottom
        |Bottom -> Top

let primaryFromLocation (loc : DockLocation) =
    match loc with
    | LeftTop | LeftBottom -> Left
    | RightTop | RightBottom -> Right
    | BottomLeft | BottomRight -> Bottom
    //| TopLeft | TopRight -> Top

let secondaryFromLocation (loc : DockLocation) =
    match loc with
    | LeftTop | RightTop -> Top
    | LeftBottom | RightBottom -> Bottom
    | BottomLeft (* | TopLeft *) -> Left
    | BottomRight (* | TopRight *) -> Right

let oppositeOf (loc : BasicLocation) = loc.Opposite

let dockContainer model (loc : DockLocation) =
    let primary = (primaryFromLocation loc)
    let secondary = (secondaryFromLocation loc)
    let pname = primary.LowerName
    let sname = secondary.LowerName

    UI.divc $"dock-{pname}-container" [

        Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[loc].IsNone), "hidden" )

        UI.divc $"drag-overlay {pname}-{sname}" [
            Bind.toggleClass(showOverlay model loc, "visible")
        ]

        UI.divc $"dock-{pname}-content" [
            Attr.id $"dock-{pname}-content-id"
        ]

        UI.divc $"dock-resize-handle {primary.Opposite.LowerName}" [
            match primary with
            | Left ->
                resizeControllerEw -1
            | Right ->
                resizeControllerEw 1
            | Top ->
                resizeControllerNs -1
            | Bottom ->
                resizeControllerNs 1
        ]
    ]


type DockContainer() =
    let model, dispatch = DockCollection.Empty |> Store.makeElmish init update ignore

    let view =

        UI.divc "dock-container" [
            Ev.onDragOver (EventHandlers.dragOver dispatch)
            Ev.onDrop (EventHandlers.drop dispatch)

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(LeftTop)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-left border border-right" [
                    yield! tabs |> List.map (viewTabLabel model dispatch LeftTop)
                ]
            )

            UI.divc "dock-main-grid" [

                // Row 1
                dockContainer model LeftTop
                // UI.divc "dock-left-container" [

                //     Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[LeftTop].IsNone), "hidden" )

                //     UI.divc "drag-overlay left-top" [
                //         Bind.toggleClass(showOverlay model LeftTop, "visible")
                //     ]
                //     UI.divc "dock-left-content" [
                //         Attr.id "dock-left-content-id"
                //     ]
                //     UI.divc "dock-resize-handle right" [
                //         resizeControllerEw -1
                //     ]
                // ]

                UI.divc "dock-main" []

                dockContainer model RightTop
                // UI.divc "dock-right-container" [
                //     Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[RightTop].IsNone), "hidden" )

                //     UI.divc "drag-overlay right-top" [
                //         Bind.toggleClass(showOverlay model RightTop, "visible")
                //     ]
                //     UI.divc "dock-right-content" [
                //         Attr.id "dock-right-content-id"
                //     ]
                //     UI.divc "dock-resize-handle left" [
                //         resizeControllerEw 1
                //     ]
                // ]

                // Row 2
                dockContainer model BottomLeft
                // UI.divc "dock-bottom-container" [
                //     Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[BottomLeft].IsNone), "hidden" )

                //     UI.divc "drag-overlay bottom" [
                //         Bind.toggleClass(showOverlay model BottomLeft, "visible")
                //     ]
                //     UI.divc "dock-bottom-content" [
                //         Attr.id "dock-bottom-content-id"
                //     ]
                //     UI.divc "dock-resize-handle top" [
                //         resizeControllerNs 1
                //     ]
                // ]

            ]

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(RightTop)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-right border border-left" [
                    yield! tabs |> List.map (viewTabLabel model dispatch RightTop)
                ]
            )

            // Bottom left corner, so we can place a border on top
            UI.divc "dock-tabs tabs-bottom-left border border-top" []

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(BottomLeft)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-bottom border border-top" [
                    yield! tabs |> List.map (viewTabLabel model dispatch BottomLeft)
                ]
            )
            // Bottom right corner, so we can place a border on top
            UI.divc "dock-tabs tabs-bottom-right border border-top" []

        ] |> withStyle css

    do
        ()
with
    member __.View  = view

    member __.AddPane (name : string, location : DockLocation, content : SutilElement ) =
        let contentId =
            match location with
            | LeftTop | LeftBottom -> "#dock-left-content-id"
            | RightTop | RightBottom -> "#dock-right-content-id"
            | BottomLeft | BottomRight -> "#dock-bottom-content-id"

        let wrapper =
            UI.divc "dock-pane-wrapper" [
                Attr.id ("pane-" + name.ToLower())
                Bind.toggleClass(
                    model |> Store.map (fun m -> (DockHelpers.findPaneLocation m.Docks name |> Option.bind (fun l -> m.SelectedPanes[l]) |> Option.defaultValue "") = name),
                    "selected")

                Html.div [
                    Attr.style [
                        Css.padding (px 2)
                        Css.paddingLeft (rem 0.5)
                        Css.paddingRight (rem 0.5)
                        //Css.height (rem 1.5)
                        Css.backgroundColor (Palette.backgroundHover)
                        Css.fontSize (percent 75)
                    ]
                    Html.div [
                        Attr.style [
                            Css.displayFlex
                            Css.flexDirectionRow
                            Css.justifyContentSpaceBetween
                        ]
                        text name
                        Html.i [ Attr.className "fa fa-window-minimize"]
                    ]
                ]

                Html.div [
                    Attr.style [
                        Css.width (percent 100)
                        Css.height (percent 100)
                        Css.overflowAuto
                    ]
                    content
                ]
            ] |> withStyle [
                rule ".dock-pane-wrapper" [
                    Css.displayNone
                    Css.flexDirectionColumn
                    Css.positionAbsolute
                    Css.width (percent 100)
                    Css.height (percent 100)
                ]

                rule ".dock-pane-wrapper.selected" [
                    Css.displayFlex
                ]
            ]

        DomHelpers.getContentParentNode location |> DOM.mountOn wrapper |> ignore

        dispatch <| Message.AddTab (name,"",location)

let container (tabLabels : DockCollection) =
    let c = DockContainer()
    c.View
