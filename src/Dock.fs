module Dock

open Browser.Dom
open Browser.CssExtensions

open Sutil.DOM
open Sutil
open Sutil.Styling
open type Feliz.length
open Fable.Core
open Fable.Core.JsInterop
open Browser.Types
open Css

type UI =
    static member divc (cls:string) (items : seq<SutilElement>) =
        Html.div [ Attr.className cls ; yield! items ]

type TabHalf =
    | FirstHalf | SecondHalf

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


type DockLocation =
    | LeftTop
    | LeftBottom
    | BottomLeft
    | BottomRight
    | RightTop
    | RightBottom
    //| TopLeft
    //| TopRight
with
    static member All =
        [
            LeftTop; LeftBottom; BottomLeft; BottomRight; RightTop; RightBottom; // TopLeft; TopRight
        ]

    member __.Primary =
        match __ with
        | LeftTop | LeftBottom -> Left
        | RightTop | RightBottom -> Right
        | BottomLeft | BottomRight -> Bottom
        //| TopLeft | TopRight -> Top

    member __.Secondary=
        match __ with
        | LeftTop | RightTop -> Top
        | LeftBottom | RightBottom -> Bottom
        | BottomLeft (* | TopLeft *) -> Left
        | BottomRight (* | TopRight *) -> Right

    member __.CssName =
        __.Primary.LowerName + "-" + __.Secondary.LowerName

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
            | LeftTop     -> "#dock-left-top-content-id"
            | LeftBottom  -> "#dock-left-bottom-content-id"
            | RightTop    -> "#dock-right-top-content-id"
            | RightBottom -> "#dock-right-bottom-content-id"
            | BottomLeft  -> "#dock-bottom-left-content-id"
            | BottomRight -> "#dock-bottom-right-content-id"
            // | TopLeft -> "#dock-top-left-content-id"
            // | TopRight -> "#dock-top-right-content-id"
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
        Ev.onMouseDown (fun e ->
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
                            SelectedPanes = m.SelectedPanes.Add(loc, Some dt.BeingDragged)
                        }
                ))
        m |> Option.defaultValue model, Cmd.none

    | CancelDrag ->
        { model with DraggingTab = None }, Cmd.none

    | PreviewDockLocation dockLoc ->
        let m =
            match model.DraggingTab with
            | None -> model
            | Some d ->
                { model with DraggingTab = Some { d with Preview = dockLoc } }
        m, Cmd.none

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

    let closestDock cx cy (r : ClientRect) =
        let x, y = cx - r.left, cy - r.top
        let distanceTo loc =
            match loc with
            | LeftTop when y < (r.height/2.0) -> x
            | LeftBottom when y >= (r.height/2.0) -> x
            | RightTop when y < (r.height/2.0) -> (r.width - x)
            | RightBottom when y >= (r.height/2.0) -> (r.width - x)
            | BottomLeft when x < (r.width/2.0) -> (r.height - y)
            | BottomRight when x > (r.width/2.0) -> (r.height - y)
            | _ -> System.Double.MaxValue
            //| TopLeft  -> if x < (r.width/2.0) then y else 999
            //| TopRight -> if x > (r.width/2.0) then y else 999

        let (loc, dist) = DockLocation.All |> List.map (fun loc -> loc, distanceTo loc) |> List.minBy snd
        if System.Math.Abs dist < 200 then Some loc else None

    let dragOver dispatch (e : DragEvent) =
        try
            e.preventDefault()
            //let tabName = e.dataTransfer.getData("text/plain")
            let el = e.currentTarget |> toEl
            let r = el.getBoundingClientRect()

            clearPreview()
            let dragEl = document.querySelector(".dragging") |> toEl

            let invert = function FirstHalf-> SecondHalf|_-> FirstHalf

            let previewOverLoc (loc : DockLocation) =
                previewOver dragEl $".tabs-{loc.CssName} > div"

            match closestDock e.clientX e.clientY r with
            | Some loc ->
                let i =
                    match loc.Primary with
                    | Left ->
                        previewOverLoc loc (e.clientY) containsByHeight (fun a b -> whichHalfY a b |> invert)
                    | Right ->
                        previewOverLoc loc (e.clientY) containsByHeight whichHalfY
                    | Bottom | Top ->
                        previewOverLoc loc (e.clientX) containsByWidth whichHalfX

                if i <> -1 then
                    (Some (loc,i)) |> PreviewDockLocation |> dispatch

            | _ ->
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
    //console.log($"{tabLabel.Name} @ {dockLocation}")
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



let dockContainer model (loc : DockLocation) =

    UI.divc $"dock-{loc.CssName}-container" [

        Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[loc].IsNone), "hidden" )

        UI.divc $"drag-overlay {loc.Primary.LowerName}" [
            Bind.toggleClass(showOverlay model loc, "visible")
        ]

        UI.divc $"dock-{loc.CssName}-content" [
            Attr.id $"dock-{loc.CssName}-content-id"
        ]

        match loc with
        | LeftBottom | RightBottom ->
            UI.divc $"dock-resize-handle top" [
                resizeControllerNsFlex 1
            ]
        | (*TopRight |*) BottomRight ->
            UI.divc $"dock-resize-handle left" [
                resizeControllerEwFlex 1
            ]
        | _ -> ()
    ]


type DockContainer() =
    let model, dispatch = DockCollection.Empty |> Store.makeElmish init update ignore

    let view =

        UI.divc "dock-container" [
            Ev.onDragOver (EventHandlers.dragOver dispatch)
            Ev.onDrop (EventHandlers.drop dispatch)

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(LeftTop)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-left tabs-left-top border border-right" [
                    yield! tabs |> List.map (viewTabLabel model dispatch LeftTop)
                ]
            )

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(LeftBottom)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-left tabs-left-bottom border border-right" [
                    yield! tabs |> List.map (viewTabLabel model dispatch LeftBottom)
                ]
            )

            UI.divc "dock-main-grid" [

                // Row 1
                UI.divc "dock-left-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[LeftTop], m.SelectedPanes[LeftBottom]) = (None,None)), "hidden" )

                    dockContainer model LeftTop
                    dockContainer model LeftBottom

                    UI.divc $"dock-resize-handle right" [
                        resizeControllerEw -1
                    ]
                ]

                UI.divc "dock-main" []

                UI.divc "dock-right-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[RightTop], m.SelectedPanes[RightBottom]) = (None,None)), "hidden" )

                    dockContainer model RightTop
                    dockContainer model RightBottom

                    UI.divc $"dock-resize-handle left" [
                        resizeControllerEw 1
                    ]
                ]

                // Row 2
                UI.divc "dock-bottom-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[BottomLeft], m.SelectedPanes[BottomRight]) = (None,None)), "hidden" )

                    dockContainer model BottomLeft
                    dockContainer model BottomRight

                    UI.divc $"dock-resize-handle top" [
                        resizeControllerNs 1
                    ]
                ]
            ]

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(RightTop)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-right tabs-right-top border border-left" [
                    yield! tabs |> List.map (viewTabLabel model dispatch RightTop)
                ]
            )

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(RightBottom)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-right tabs-right-bottom border border-left" [
                    yield! tabs |> List.map (viewTabLabel model dispatch RightBottom)
                ]
            )

            // Bottom left corner, so we can place a border on top
            UI.divc "dock-tabs box-left border border-top" []

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(BottomLeft)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-bottom tabs-bottom-left border border-top" [
                    yield! tabs |> List.map (viewTabLabel model dispatch BottomLeft)
                ]
            )

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(BottomRight)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-bottom tabs-bottom-right border border-top" [
                    yield! tabs |> List.map (viewTabLabel model dispatch BottomRight)
                ]
            )

            // Bottom right corner, so we can place a border on top
            UI.divc "dock-tabs box-right border border-top" []

        ] |> withStyle css

    do
        ()
with
    member __.View  = view

    member __.AddPane (name : string, location : DockLocation, content : SutilElement ) =

        let debugWrapper =
            UI.divc "dock-pane-wrapper" [
                Attr.id ("pane-" + name.ToLower())
                Html.div [
                    Attr.style [
                        Css.width (percent 100)
                        Css.height (percent 100)
                        Css.overflowAuto
                    ]
                    content
                ]
                Bind.toggleClass(
                    model |> Store.map (fun m -> (DockHelpers.findPaneLocation m.Docks name |> Option.bind (fun l -> m.SelectedPanes[l]) |> Option.defaultValue "") = name),
                    "selected")
            ]|> withStyle [
                rule ".dock-pane-wrapper" [
                    Css.displayNone
                    Css.width (percent 100)
                    Css.height (percent 100)
                ]

                rule ".dock-pane-wrapper.selected" [
                    Css.displayBlock
                ]
            ]

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
                    Css.width (percent 100)
                    Css.height (percent 100)
                ]

                rule ".dock-pane-wrapper.selected" [
                    Css.displayFlex
                    Css.flexDirectionColumn
                ]
            ]

        DomHelpers.getContentParentNode location |> DOM.mountOn wrapper |> ignore

        dispatch <| Message.AddTab (name,"",location)

let container (tabLabels : DockCollection) =
    let c = DockContainer()
    c.View
