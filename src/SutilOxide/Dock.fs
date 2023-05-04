module SutilOxide.Dock

//
// Copyright (c) 2022 David Dawkins
//

open Browser.Dom
open Browser.CssExtensions

open Sutil.Core
open Sutil.CoreElements
open Sutil.DomHelpers
open Sutil
open Sutil.Styling
open type Feliz.length
open Fable.Core
open Fable.Core.JsInterop
open Browser.Types
open SutilOxide.Css
open SutilOxide.DomHelpers
open SutilOxide.Types
open SutilOxide.Toolbar

type DraggingTab = {
    BeingDragged : string
    Preview : (DockLocation * int) option
}

type Model = {
    RefreshId : int
    Docks : DockCollection
    DraggingTab : DraggingTab option
    SelectedPanes : Map<DockLocation,string option>
}

type Options = 
    {
        OnTabShow : (string * bool -> unit)
    }
    static member Create() = {
        OnTabShow = ignore
    }

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
        docks.Stations
            |> Map.tryPick (fun loc station ->
                //SutilOxide.Logging.log(sprintf "Finding %A %A " loc station)
                station.Panes
                |> List.tryFindIndex (fun t ->
                    //SutilOxide.Logging.log("Finding " + t.Name + " ? " + name)
                    t.Name = name)
                |> Option.map (fun i -> loc,i)
            )

    let findPaneLocation(docks : DockCollection) name =
        findPaneLocationIndex docks name |> Option.map fst

    let getPanes (docks : DockCollection) loc =
        docks.Stations[loc].Panes

    let setPanes (docks : DockCollection) loc value =
        let dock = docks.Stations[loc]
        { docks with Stations = docks.Stations.Add(loc, { dock with Panes = value}) }

    let tryGetPane docks name =
        findPaneLocation docks name
        |> Option.bind (fun loc -> getPanes docks loc |> List.tryFind (fun t -> t.Name = name))

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

    let ensureCentreSelected (m : Model) =
        match m.SelectedPanes[CentreCentre] with
        | None ->
            { m with
                SelectedPanes =
                    m.SelectedPanes.Add(
                        CentreCentre,
                        m.Docks.GetPanes CentreCentre |> List.tryHead |> Option.map (fun p -> p.Name))
            }
        | _ -> m

    let minimizePane model pane =
        findPaneLocation model.Docks pane
        |> Option.map (fun loc ->
            { model with SelectedPanes = model.SelectedPanes.Add(loc,None) } )
        |> Option.defaultValue model


type DockProperty =
    | Visible of bool
    | Location of DockLocation

type private Message =
    | RemoveTab of string
    | AddTab of (string*string*DockLocation*bool)
    | SetDragging of string
    | CancelDrag
    | PreviewDockLocation of (DockLocation * int) option
    | CommitDrag
    | SelectPane of DockLocation*string option
    | TogglePane of DockLocation*string
    | MinimizePane of string
    | ShowPane of string
    | MoveTo of string*DockLocation
    | DockProp of (string *DockProperty)

let private init docks =
    {
        RefreshId = 0
        Docks = docks
        DraggingTab = None
        SelectedPanes = DockLocation.All |> List.fold (fun s loc -> s.Add(loc, None)) Map.empty
    } |> DockHelpers.ensurePaneSelected, Cmd.none

let private cmdMonitorAll : Cmd<Message> =
    [ 
        fun d -> Toolbar.MenuMonitor.monitorAll() 
    ]

let private update (options : Options) msg model =
    //SutilOxide.Logging.log($"{msg}")

    match msg with

    | DockProp (name,p) ->
        match p with
        | Visible z ->
            model, Cmd.ofMsg (if z then ShowPane name else MinimizePane name)
        | Location l ->
            model, Cmd.ofMsg (MoveTo (name,l))

    | RemoveTab name ->
        let docks = DockHelpers.removeFromPanes (DockHelpers.minimizePane model name).Docks name
        {
            model with
                Docks = docks
        }, cmdMonitorAll

    | AddTab (name,icon,location,show) ->
        let station = model.Docks.Stations[location]
        let panes = station.Panes @ [ { Name = name }]
        let station' = { station with Panes = panes }
        {
            model with
                Docks = { Stations = model.Docks.Stations.Add( location, station' ) }
        } , Cmd.batch [ if show then Cmd.ofMsg (ShowPane name) else Cmd.none; cmdMonitorAll ]

    | SelectPane (loc,pane) ->
        { model with SelectedPanes = model.SelectedPanes.Add(loc,pane) }, Cmd.none

    | TogglePane (loc,pane) ->
        let selected, show =
            match model.SelectedPanes[loc] with
            | Some name when name = pane -> None, false
            | _ -> Some pane, true

        options.OnTabShow( pane, show )
        { model with SelectedPanes = model.SelectedPanes.Add(loc,selected) } |> DockHelpers.ensureCentreSelected, cmdMonitorAll

    | ShowPane pane ->
        let m =
            DockHelpers.findPaneLocation model.Docks pane
            |> Option.map (fun loc ->
                { model with SelectedPanes = model.SelectedPanes.Add(loc,Some pane) }
            )
            |> Option.defaultValue model
        options.OnTabShow( pane, true )
        m,  cmdMonitorAll

    | MinimizePane pane ->
        DockHelpers.minimizePane model pane, cmdMonitorAll

    | SetDragging d ->
        { model with DraggingTab = Some { BeingDragged = d; Preview = None} }, Cmd.none

    | MoveTo (pane,loc) ->
        let m = DockHelpers.moveTab model pane loc 999
        let wrapper = DomHelpers.getWrapperNode pane
        let parent = DomHelpers.getContentParentNode loc
        parent.appendChild wrapper |> ignore

        { m with
            DraggingTab = None
            SelectedPanes = m.SelectedPanes.Add(loc, Some pane)
        } |> DockHelpers.ensureCentreSelected, cmdMonitorAll

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
        m |> Option.defaultValue model |> DockHelpers.ensureCentreSelected, cmdMonitorAll

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

module private EventHandlers =

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
            | TopLeft when x < (r.width/2.0) -> y
            | TopRight when x > (r.width/2.0) -> y
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
                    | Centre ->
                        -1

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
        match dockLocation with
        | CentreCentre ->
            (dockLocation,Some (tabLabel.Name)) |> SelectPane |>  dispatch
        | _ ->
            (dockLocation,(tabLabel.Name)) |> TogglePane |>  dispatch

let private viewTabLabel (model : System.IObservable<Model>) dispatch dockLocation (tabLabel : DockPane) =
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


let dragOverlay model (loc:DockLocation) =
    UI.divc $"drag-overlay {loc.Primary.LowerName} {loc.CssName}" [
        Bind.toggleClass(showOverlay model loc, "visible")
    ]


let dockContainer model (loc : DockLocation) =

    UI.divc $"dock-{loc.CssName}-container dock-{loc.Hand.LowerName}-hand" [

        Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[loc].IsNone), "hidden" )

        Attr.id $"dock-{loc.CssName}"

        match loc with
        | LeftBottom | RightBottom ->
            UI.divc $"dock-resize-handle top vertical" [
                resizeControllerNsFlex 1
            ]
        | TopRight | BottomRight ->
            UI.divc $"dock-resize-handle left horizontal" [
                resizeControllerEwFlex 1
            ]
        | _ -> ()
    ]


type DockContainer( options : Options ) =
    let model, dispatch = DockCollection.Empty |> Store.makeElmish init (update options) ignore


    let dockContainer() =
        UI.divc "dock-container" [

            Ev.onDragOver (EventHandlers.dragOver dispatch)
            Ev.onDrop (EventHandlers.drop dispatch)

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(TopLeft)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-top tabs-top-left border border-bottom" [
                    yield! tabs |> List.map (viewTabLabel model dispatch TopLeft)
                ]
            )

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(TopRight)) |> Observable.distinctUntilChanged, fun tabs ->
                UI.divc "dock-tabs tabs-top tabs-top-right border border-bottom" [
                    yield! tabs |> List.map (viewTabLabel model dispatch TopRight)
                ]
            )

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

                UI.divc "dock-top-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[TopLeft], m.SelectedPanes[TopRight]) = (None,None)), "hidden" )

                    dockContainer model TopLeft
                    dockContainer model TopRight

                    UI.divc $"dock-resize-handle bottom vertical" [
                        resizeControllerNs -1
                    ]
                ]

                UI.divc "dock-centre-container" [

                    // Row 1
                    UI.divc "dock-left-container" [
                        Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[LeftTop], m.SelectedPanes[LeftBottom]) = (None,None)), "hidden" )

                        dockContainer model LeftTop
                        dockContainer model LeftBottom

                        UI.divc $"dock-resize-handle right horizontal" [
                            resizeControllerEw -1
                        ]
                    ]

                    Bind.el(
                        model
                            |> Store.map (fun m -> m.Docks.GetPanes(CentreCentre))
                            |> Observable.distinctUntilChanged,
                        fun tabs ->
                            UI.divc "dock-tabs tabs-centre border border-bottom" [
                                match tabs with
                                | [] | [ _ ] -> yield! []
                                | _ -> yield! tabs |> List.map (viewTabLabel model dispatch CentreCentre)
                            ]
                    )


                    UI.divc "dock-main" [
                        dockContainer model CentreCentre
                    ]

                    UI.divc "dock-right-container" [
                        Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[RightTop], m.SelectedPanes[RightBottom]) = (None,None)), "hidden" )

                        dockContainer model RightTop
                        dockContainer model RightBottom

                        UI.divc $"dock-resize-handle left horizontal" [
                            resizeControllerEw 1
                        ]
                    ]

                ]

                // Row 2
                UI.divc "dock-bottom-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[BottomLeft], m.SelectedPanes[BottomRight]) = (None,None)), "hidden" )

                    dockContainer model BottomLeft
                    dockContainer model BottomRight

                    UI.divc $"dock-resize-handle top vertical" [
                        resizeControllerNs 1
                    ]
                ]
            ]

            UI.divc "overlays" [
                UI.divc "overlays-left" [
                    yield! DockLocation.All |> List.filter (fun l -> l.Primary = Left || l.Secondary = Left) |> List.map (fun l -> dragOverlay model l)
                ]

                UI.divc "overlays-right" [
                    yield! DockLocation.All |> List.filter (fun l -> l.Primary = Right || l.Secondary = Right) |> List.map (fun l -> dragOverlay model l)
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
        ]

    let view (init : DockContainer -> unit) (self : DockContainer)=
        fragment [

            Attr.style [
                Css.overflowHidden // Stop scrollbars appearing (during layout?), it seems they can then "stick" and stay latched on
            ]

            onMount (fun e ->
                init self
            ) [ Once ]

            dockContainer()
        ]

    do
        ()
with
    static member Create (init : DockContainer -> unit, options) =
        let dc = DockContainer(options)
        dc.View init

    member __.View (init: DockContainer -> unit)  =  view init __

    member __.SetProperty (name:string, p : DockProperty) =
        dispatch (DockProp (name,p))

    member __.SetProperties (name:string, props : seq<DockProperty>) =
        props |> Seq.iter (fun p -> __.SetProperty( name, p))

    // member __.ShowPane (name : string) =
    //     dispatch (ShowPane name)

    member __.RemovePane(name : string) =
        dispatch (RemoveTab name)

    member __.AddPane (name : string, initLoc : DockLocation, content : SutilElement ) =
        __.AddPane( name, initLoc, content, true )

    member __.AddPane (name : string, initLoc : DockLocation, content : SutilElement, show : bool ) =
        __.AddPane( name, initLoc, text name, content, show )

    member __.ContainsPane( name : string ) = 
        (DockHelpers.tryGetPane model.Value.Docks name).IsSome
        
    member __.ShowPane( name : string ) =
        dispatch (ShowPane name)

    member __.AddPane (name : string, initLoc : DockLocation, header : SutilElement, content : SutilElement, show : bool ) =

        let lname = name.ToLower()

        let loc = model |> Store.map (fun m -> (DockHelpers.findPaneLocation m.Docks name)) |> Observable.distinctUntilChanged

        let toolbar =
            buttonGroup [
                buttonItem [ Icon "fa-window-minimize"; Label ""; OnClick (fun _ -> MinimizePane name |> dispatch) ]
                dropDownItem [ Icon "fa-cog"; Label ""] [
                    menuItem [
                        Label "Move To"
                    ] [
                        buttonItem [ Icon "fa-caret-square-left"; Label "Left Top"; OnClick (fun _ -> MoveTo (name,LeftTop) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-left"; Label "Left Bottom"; OnClick (fun _ -> MoveTo (name,LeftBottom) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-right"; Label "Right Top"; OnClick (fun _ -> MoveTo (name,RightTop) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-right"; Label "Right Bottom"; OnClick (fun _ -> MoveTo (name,RightBottom) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-down"; Label "Bottom Left"; OnClick (fun _ -> MoveTo (name,BottomLeft) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-down"; Label "Bottom Right"; OnClick (fun _ -> MoveTo (name,BottomRight) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-up"; Label "Top Left"; OnClick (fun _ -> MoveTo (name,TopLeft) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-up"; Label "Top Right"; OnClick (fun _ -> MoveTo (name,TopRight) |> dispatch) ]
                        buttonItem [ Icon "fa-square"; Label "Centre"; OnClick (fun _ -> MoveTo (name,CentreCentre) |> dispatch) ]
                    ]
                ]
            ]

        let wrapper =
            UI.divc "dock-pane-wrapper" [

                Attr.id ("pane-" + lname)

                Bind.toggleClass(
                    model
                    |> Store.map (fun m -> (DockHelpers.findPaneLocation m.Docks name
                    |> Option.bind (fun l -> m.SelectedPanes[l]) |> Option.defaultValue "") = name),
                    "selected")

                UI.divc "pane-header" [
                    Html.div [
                        header
                        Bind.visibility
                            (loc |> Store.map (fun optLoc -> initLoc <> CentreCentre || optLoc <> Some CentreCentre))
                            toolbar
                    ]
                ]

                UI.divc "pane-content" [
                    content
                ]
            ]

        (DomHelpers.getContentParentNode initLoc, wrapper) |> Program.mountAppend |> ignore

        dispatch <| Message.AddTab (name,"",initLoc,show)

// let container (tabLabels : DockCollection) =
//     let c = DockContainer()
//     c.View
