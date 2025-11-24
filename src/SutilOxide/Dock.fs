module SutilOxide.Dock

//
// Copyright (c) 2022 David Dawkins
//

open Browser.Dom
open Browser.CssExtensions

open Sutil.Core
open Sutil.CoreElements
open Sutil
open type Feliz.length
open Fable.Core.JsInterop
open Browser.Types
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

type Configuration = Map<string,string>

type Options = 
    {
        Log : (string -> unit)
        OnTabShow : (string * bool -> unit)
        OnConfigurationChanged : (unit -> unit)
    }
    static member Create() = {
        Log = ignore
        OnTabShow = ignore
        OnConfigurationChanged = ignore
    }

module DockHelpers =
    let tabsContains name tabLabels=
        tabLabels |> List.exists (fun t -> t.Key = name)

    let removeFromPanesList (panes : DockPane list) name =
        panes |> List.filter (fun t -> t.Key <> name)

    let insertIntoPanes (panes : List<'a>) pane i =
        if i >= panes.Length then
            panes @ [ pane ]
        else
            panes |> List.insertAt i pane

    let allPaneNames (docks : DockCollection) =
        docks.Stations.Values
            |> Seq.collect (fun s -> s.Panes |> List.map (fun p -> p.Key) )
            |> Seq.toArray

    let findPaneLocationIndex (docks : DockCollection) name =
        docks.Stations
            |> Map.tryPick (fun loc station ->
                station.Panes
                |> List.tryFindIndex (fun t -> t.Key = name)
                |> Option.map (fun i -> loc,i)
            )

    let findPaneLocation(docks : DockCollection) name : DockLocation option =
        findPaneLocationIndex docks name |> Option.map fst

    let getPanes (docks : DockCollection) loc =
        docks.Stations[loc].Panes

    let setPanes (docks : DockCollection) loc value =
        let dock = docks.Stations[loc]
        { docks with Stations = docks.Stations.Add(loc, { dock with Panes = value}) }

    let tryGetPane docks name =
        findPaneLocation docks name
        |> Option.bind (fun loc -> getPanes docks loc |> List.tryFind (fun t -> t.Key = name))

    let getPane docks name: DockPane =
        match findPaneLocation docks name with
        | None -> 
            // Fable.Core.JS.console.log("Panes: ", allPaneNames(docks))
            failwith ("Not found: " + name)
        | Some (loc) ->
            getPanes docks loc |> List.find (fun t -> t.Key = name)

    let removeFromPanes docks name =
        match findPaneLocation docks name with
        | None -> 
            // Fable.Core.JS.console.log("Panes: ", allPaneNames(docks))
            failwith ("Not found: " + name)
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
        |> Array.map (fun loc ->
                let selectedPaneName =
                        m.SelectedPanes[loc]
                        |> Option.orElseWith (fun () -> m.Docks.GetPanes(loc) |> List.tryHead |> Option.map (fun p -> p.Key))
                loc, selectedPaneName)
        |> Map.ofArray
        |> (fun map -> { m with SelectedPanes = map } )

    let ensureCentreSelected (m : Model) =
        match m.SelectedPanes[CentreLeft] with
        | None ->
            { m with
                SelectedPanes =
                    m.SelectedPanes.Add(
                        CentreLeft,
                        m.Docks.GetPanes CentreLeft |> List.tryHead |> Option.map (fun p -> p.Key))
            }
        | _ -> m

    let minimizePane model pane =
        findPaneLocation model.Docks pane
        |> Option.map (fun loc ->
            { model with SelectedPanes = model.SelectedPanes.Add(loc,None) } )
        |> Option.defaultValue model

    let isPaneShowing (model : Model) (pane : string) =
        model.SelectedPanes.Values 
        |> Seq.choose (id)
        |> Seq.exists (fun name -> name = pane)

type DockProperty =
    | Visible of bool
    | Location of DockLocation

type private Message =
    | RemoveTab of string
    | AddTab of DockPane //(string*string*string*DockLocation*bool)
    | SetDragging of string
    | CancelDrag
    | PreviewDockLocation of (DockLocation * int) option
    | CommitDrag
    //| SelectPane of DockLocation*string option
    | TogglePane of string
    | TogglePaneWithNotify of string * bool
    | MinimizePane of string
    | ShowPane of string
    | ShowPaneWithNotify of string * bool
    | MoveTo of string*DockLocation
    | DockProp of (string *DockProperty)

let private init docks =
    {
        RefreshId = 0
        Docks = docks
        DraggingTab = None
        SelectedPanes = DockLocation.All |> Array.fold (fun s loc -> s.Add(loc, None)) Map.empty
    } |> DockHelpers.ensurePaneSelected, Cmd.none

let private cmdMonitorAll : Cmd<Message> =
    [ 
        fun d -> Toolbar.MenuMonitor.monitorAll() 
    ]


let private _update (options : unit -> Options) (unmount : string -> unit) msg (model : Model) =
    //Fable.Core.JS.console.log($"Dock: {msg}")

    let cmdOnTabShow currentPane pane = 

        let notifyTabShow (show : bool) (name : string) =
            options().Log("notifyTabShow: " + name + " " + show.ToString())
            DockHelpers.tryGetPane (model.Docks) name |> Option.iter (fun p -> p.OnShow(show))
            options().OnTabShow(name,show)

        [ fun _ ->
            if pane <> currentPane then
                currentPane |> Option.iter (notifyTabShow false)
                pane |> Option.iter (notifyTabShow true)
        ]

    match msg with

    | DockProp (name,p) ->
        match p with
        | Visible z ->
            model, Cmd.ofMsg (if z then ShowPane name else MinimizePane name)
        | Location l ->
            model, Cmd.ofMsg (MoveTo (name,l))

    | RemoveTab name ->
        let selectedPanes =
            DockHelpers.findPaneLocation model.Docks name
            |> Option.map (fun loc ->
                if (model.SelectedPanes.ContainsKey loc && model.SelectedPanes[loc] = Some name) then
                    model.SelectedPanes.Add( loc, None )
                else 
                    model.SelectedPanes
            )
            |> Option.defaultValue (model.SelectedPanes)

        let docks = DockHelpers.removeFromPanes model.Docks name
        {
            model with
                Docks = docks 
                SelectedPanes = selectedPanes
        } |> DockHelpers.ensureCentreSelected, [ fun _ -> unmount name ] @ cmdMonitorAll

    | AddTab pane ->
        let station = model.Docks.Stations[pane.Location]
        let panes = station.Panes @ [ pane ]
        let station' = { station with Panes = panes }
        {
            model with
                Docks = { Stations = model.Docks.Stations.Add( pane.Location, station' ) }
        } , Cmd.batch [ if pane.IsOpen then Cmd.ofMsg (ShowPane pane.Key) else Cmd.none; cmdMonitorAll ]

    // | SelectPane (loc,pane) ->
    //     let currentPane = model.SelectedPanes[loc]

    //     { model with SelectedPanes = model.SelectedPanes.Add(loc,pane) }, [ cmdOnTabShow currentPane pane ]

    | TogglePane pane ->
        model, Cmd.ofMsg (TogglePaneWithNotify (pane, false))

    | TogglePaneWithNotify (pane,notify) ->

        match DockHelpers.findPaneLocation model.Docks pane with

        | Some loc -> 
            let selected, show =
                match model.SelectedPanes[loc] with
                | Some name when name = pane -> None, false
                | _ -> Some pane, true

            let currentPane = model.SelectedPanes[loc]

            { model with SelectedPanes = model.SelectedPanes.Add(loc,selected) } |> DockHelpers.ensureCentreSelected, Cmd.batch [ if notify then cmdOnTabShow currentPane selected ; cmdMonitorAll ]

        | None -> 
            model, Cmd.none

    | ShowPane pane ->
        model, Cmd.ofMsg (ShowPaneWithNotify (pane,false))

    | ShowPaneWithNotify (pane,notify) ->

        match DockHelpers.findPaneLocation model.Docks pane with
        | Some loc -> 
            let currentPane = model.SelectedPanes[loc]
            { model with SelectedPanes = model.SelectedPanes.Add(loc,Some pane) }, Cmd.batch [ if notify then cmdOnTabShow currentPane (Some pane); cmdMonitorAll ]
        | None -> 
            model, Cmd.none

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

let private shouldNotifyConfiguration msg =
    match msg with
    | PreviewDockLocation _ | CancelDrag | SetDragging _ -> false
    | _ -> true

let private update (options : unit -> Options) (unmount : string -> unit) msg (model : Model) =
    //Fable.Core.JS.console.log(sprintf "DOCK: %A" msg)
    let m, c = _update options unmount msg model
    m, 
        if shouldNotifyConfiguration msg then
            Cmd.batch [ c; [ fun _ -> options().OnConfigurationChanged() ] ]
        else
            c

[<AutoOpen>]
module ModelHelpers =
    let beingDragged (m : Model) =
        m.DraggingTab |> Option.map (fun p -> p.BeingDragged)

    let childTabIsDragging (model: System.IObservable<Model>) (tabs : Model -> DockPane list) =
            model
            |> Store.map( fun m ->
                    match beingDragged m with
                    | None -> false
                    | Some name  -> tabs m |> List.exists (fun t -> t.Key = name))

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

        let (loc, dist) = DockLocation.All |> Array.map (fun loc -> loc, distanceTo loc) |> Array.minBy snd
        if System.Math.Abs dist < 200 then Some loc else None

    let dragOver dispatch (e : DragEvent) =
        let dragEl = document.querySelector(".dragging") |> toEl
        if dragEl <> null then
            try
                e.preventDefault()
                //let tabName = e.dataTransfer.getData("text/plain")
                let el = e.currentTarget |> toEl
                let r = el.getBoundingClientRect()

                clearPreview()

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

    let dragStart (tabLabel : DockPane) dispatch (e : DragEvent) =
        e.dataTransfer.setData("text/plain", tabLabel.Key) |> ignore
        dispatch (SetDragging (tabLabel.Key))
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
        | CentreLeft | CentreRight ->
            ShowPaneWithNotify (tabLabel.Key,true) |>  dispatch
        | _ ->
            TogglePaneWithNotify (tabLabel.Key,true) |>  dispatch


let private viewTabLabel (model : System.IObservable<Model>) dispatch dockLocation (pane : DockPane) =
    //console.log($"{tabLabel.Name} @ {dockLocation}")
    UI.divc "theme-tool-button tab-label" [

        Attr.custom ("data-tabname", pane.Key)

        Bind.toggleClass(
            model
            |> Store.map (fun m -> (beingDragged m |> Option.defaultValue "") = pane.Key),
            "preview")

        Bind.toggleClass(
            model
            |> Store.map (fun m -> (m.SelectedPanes[dockLocation] |> Option.defaultValue "") = pane.Key),
            "selected")

        if (pane.Icon <> "") then
            Html.i [ Attr.className (UI.Icon.makeFa pane.Icon) ]

        Html.span [ 
            match pane.Label with
            | LabelString s ->
               text s
            | LabelElement e -> e 
        ]
        if pane.CanClose then
            Html.divc "close-button" [
                Html.ic (UI.Icon.makeFa "fa-times") []
                Ev.onClick( fun _ -> pane.OnClose())
            ]
        Attr.draggable true
        Ev.onDragStart (EventHandlers.dragStart pane dispatch)
        Ev.onDragEnd (EventHandlers.dragEnd dispatch)
        Ev.onClick (EventHandlers.tabClick dockLocation pane dispatch)
    ]

let dragOverlay model (loc:DockLocation) =
    UI.divc $"drag-overlay {loc.Primary.LowerName} {loc.CssName}" [
        Bind.toggleClass(showOverlay model loc, "visible")
    ]

let dockContainer (options : unit -> Options) model (loc : DockLocation) =

    UI.divc $"dock-{loc.CssName}-container dock-{loc.Hand.LowerName}-hand" [

        Bind.toggleClass( model |> Store.map (fun m -> m.SelectedPanes[loc].IsNone), "hidden" )

        Attr.id $"dock-{loc.CssName}"

        match loc with
        | LeftBottom | RightBottom ->
            UI.divc $"dock-resize-handle top vertical" [
                resizeControllerNsFlex 1 (fun _ -> options().OnConfigurationChanged())
            ]
        | TopRight | BottomRight ->
            UI.divc $"dock-resize-handle left horizontal" [
                resizeControllerEwFlex 1 (fun _ -> options().OnConfigurationChanged())
            ]
        | _ -> ()
    ]

let dockLeftContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-left-container") :?> HTMLElement)

let dockRightContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-right-container") :?> HTMLElement)

let dockTopContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-top-container") :?> HTMLElement)

let dockBottomContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-bottom-container") :?> HTMLElement)

let dockLeftTopContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-left-top-container") :?> HTMLElement)

let dockLeftBottomContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-left-bottom-container") :?> HTMLElement)

let dockRightTopContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-right-top-container") :?> HTMLElement)

let dockRightBottomContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-right-bottom-container") :?> HTMLElement)

let dockTopLeftContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-top-left-container") :?> HTMLElement)

let dockTopRightContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-top-right-container") :?> HTMLElement)

let dockCentreLeftContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-centre-left-container") :?> HTMLElement)

let dockCentreRightContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-centre-right-container") :?> HTMLElement)

let dockBottomLeftContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-bottom-left-container") :?> HTMLElement)

let dockBottomRightContainer (rootElement : HTMLElement) =
    (rootElement.querySelector(".dock-bottom-right-container") :?> HTMLElement)

let paneEq (p1 : DockPane) (p2 : DockPane) =
    DockPane.Equals(p1,p2)

let paneListEq (p1 : DockPane list) (p2 : DockPane list) =
    let paneCmp a b = if paneEq a b then 0 else 1
    (List.compareWith paneCmp p1 p2) = 0

let paneDistinct = Observable.distinctUntilChangedCompare paneListEq

type DockContainer() =
    let mutable options = Options.Create()

    let mutable unmounters : Map<string,System.IDisposable> = Map.empty

    let unmount (paneKey : string) =
        unmounters[paneKey].Dispose()
        unmounters <- unmounters.Remove(paneKey)

    let model, dispatch = DockCollection.Empty |> Store.makeElmish init (update (fun _ -> options) unmount) ignore

    let mutable rootElement : HTMLElement option = None
    let mutable config : Configuration option = None

    let setOption (cfg : Configuration) (name : string) (setter : string -> unit) =
        match cfg.TryFind( name ) with
        | Some s when not (System.String.IsNullOrEmpty(s)) -> setter s
        | _ -> ()

    let applyOptions( rootElement : HTMLElement ) =
        // Fable.Core.JS.console.log("Pane: applyOptions")
        match config with
        | Some cfg ->
            setOption cfg "left.width" (fun v -> (dockLeftContainer rootElement).style.width <- v)
            setOption cfg "right.width" (fun v -> (dockRightContainer rootElement).style.width <- v)
            setOption cfg "top.height" (fun v -> (dockTopContainer rootElement).style.height <- v)
            setOption cfg "bottom.height" (fun v -> (dockBottomContainer rootElement).style.height <- v)

            setOption cfg "top-left.pct" (fun v -> (dockTopLeftContainer rootElement).style.flexGrow <- v)
            setOption cfg "top-right.pct" (fun v -> (dockTopRightContainer rootElement).style.flexGrow <- v)
            setOption cfg "centre-left.pct" (fun v -> (dockCentreLeftContainer rootElement).style.flexGrow <- v)
            setOption cfg "centre-right.pct" (fun v -> (dockCentreRightContainer rootElement).style.flexGrow <- v)
            setOption cfg "bottom-left.pct" (fun v -> (dockBottomLeftContainer rootElement).style.flexGrow <- v)
            setOption cfg "bottom-right.pct" (fun v -> (dockBottomRightContainer rootElement).style.flexGrow <- v)

            setOption cfg "left-top.pct" (fun v -> (dockLeftTopContainer rootElement).style.flexGrow <- v)
            setOption cfg "left-bottom.pct" (fun v -> (dockLeftBottomContainer rootElement).style.flexGrow <- v)
            setOption cfg "right-top.pct" (fun v -> (dockRightTopContainer rootElement).style.flexGrow <- v)
            setOption cfg "right-bottom.pct" (fun v -> (dockRightBottomContainer rootElement).style.flexGrow <- v)
            //config <- None
        | None -> ()

    let dockContainer() =
        UI.divc "dock-container" [

            Ev.onMount (fun e -> 
                let rootE = (e.target :?> HTMLElement)
                rootElement <-  rootE |> Some
                applyOptions(rootE)
            )

            Ev.onDragOver (EventHandlers.dragOver dispatch)
            Ev.onDrop (EventHandlers.drop dispatch)

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(TopLeft)) |> paneDistinct , fun tabs ->
                UI.divc "dock-tabs tabs-top tabs-top-left border border-bottom" [
                    yield! tabs |> List.map (viewTabLabel model dispatch TopLeft)
                ]
            )

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(TopRight)) |> paneDistinct, fun tabs ->
                UI.divc "dock-tabs tabs-top tabs-top-right border border-bottom" [
                    yield! tabs |> List.map (viewTabLabel model dispatch TopRight)
                ]
            )

            Html.divc "tabs-left-container" [
                Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(LeftTop)) |> paneDistinct, fun tabs ->
                    UI.divc "dock-tabs tabs-left tabs-left-top border border-right" [
                        yield! tabs |> List.map (viewTabLabel model dispatch LeftTop)
                    ]
                )

                Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(LeftBottom)) |> paneDistinct, fun tabs ->
                    UI.divc "dock-tabs tabs-left tabs-left-bottom border border-right" [
                        yield! tabs |> List.map (viewTabLabel model dispatch LeftBottom)
                    ]
                )
            ]

            UI.divc "dock-main-grid" [

                UI.divc "dock-top-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[TopLeft], m.SelectedPanes[TopRight]) = (None,None)), "hidden" )

                    dockContainer (fun _ -> options) model TopLeft
                    dockContainer (fun _ -> options) model TopRight

                    UI.divc $"dock-resize-handle bottom vertical" [
                        resizeControllerNs -1 (fun _ -> options.OnConfigurationChanged())
                    ]
                ]

                UI.divc "dock-centre-container" [

                    // Row 1
                    UI.divc "dock-left-container" [
                        Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[LeftTop], m.SelectedPanes[LeftBottom]) = (None,None)), "hidden" )

                        dockContainer (fun _ -> options) model LeftTop
                        dockContainer (fun _ -> options) model LeftBottom

                        UI.divc $"dock-resize-handle right horizontal" [
                            resizeControllerEw -1 (fun _ -> options.OnConfigurationChanged())
                        ]
                    ]

                    UI.divc "dock-centre-container2" [
                        // Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[CentreLeft], m.SelectedPanes[CentreRight]) = (None,None)), "hidden" )

                        // dockContainer (fun _ -> options) model CentreLeft
                        // dockContainer (fun _ -> options) model CentreRight

                        // UI.divc $"dock-resize-handle bottom vertical" [
                        //     resizeControllerNs -1 (fun _ -> options.OnConfigurationChanged())
                        // ]

                        Bind.el(
                            model |> Store.map (fun m -> m.Docks.GetPanes(CentreLeft)) |> paneDistinct,
                            fun tabs ->
                                UI.divc "dock-tabs tabs-centre border border-bottom" [
                                    // match tabs with
                                    // | [] | [ _ ] -> yield! []
                                    // | _ -> yield! tabs |> List.map (viewTabLabel model dispatch CentreCentre)
                                    yield! tabs |> List.map (viewTabLabel model dispatch CentreLeft)
                                ]
                        )


                        UI.divc "dock-main" [
                            dockContainer (fun _ -> options) model CentreLeft
                        ]
                    ]

                    UI.divc "dock-right-container" [
                        Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[RightTop], m.SelectedPanes[RightBottom]) = (None,None)), "hidden" )

                        dockContainer (fun _ -> options) model RightTop
                        dockContainer (fun _ -> options) model RightBottom

                        UI.divc $"dock-resize-handle left horizontal" [
                            resizeControllerEw 1 (fun _ -> options.OnConfigurationChanged())
                        ]
                    ]

                ]

                // Row 2
                UI.divc "dock-bottom-container" [
                    Bind.toggleClass( model |> Store.map (fun m -> (m.SelectedPanes[BottomLeft], m.SelectedPanes[BottomRight]) = (None,None)), "hidden" )

                    dockContainer (fun _ -> options) model BottomLeft
                    dockContainer (fun _ -> options) model BottomRight

                    UI.divc $"dock-resize-handle top vertical" [
                        resizeControllerNs 1 (fun _ -> options.OnConfigurationChanged())
                    ]
                ]
            ]

            UI.divc "overlays" [
                UI.divc "overlays-left" [
                    yield! DockLocation.All |> Array.filter (fun l -> l.Primary = Left || l.Secondary = Left) |> Array.map (fun l -> dragOverlay model l)
                ]

                UI.divc "overlays-right" [
                    yield! DockLocation.All |> Array.filter (fun l -> l.Primary = Right || l.Secondary = Right) |> Array.map (fun l -> dragOverlay model l)
                ]
            ]

            Html.divc "tabs-right-container" [
                Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(RightTop)) |> paneDistinct, fun tabs ->
                    UI.divc "dock-tabs tabs-right tabs-right-top border border-left" [
                        yield! tabs |> List.map (viewTabLabel model dispatch RightTop)
                    ]
                )

                Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(RightBottom)) |> paneDistinct, fun tabs ->
                    UI.divc "dock-tabs tabs-right tabs-right-bottom border border-left" [
                        yield! tabs |> List.map (viewTabLabel model dispatch RightBottom)
                    ]
                )
            ]

            // Bottom left corner, so we can place a border on top
            UI.divc "dock-tabs box-left border border-top" []

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(BottomLeft)) |> paneDistinct, fun tabs ->
                UI.divc "dock-tabs tabs-bottom tabs-bottom-left border border-top" [
                    yield! tabs |> List.map (viewTabLabel model dispatch BottomLeft)
                ]
            )

            Bind.el( model |> Store.map (fun m -> m.Docks.GetPanes(BottomRight)) |> paneDistinct, fun tabs ->
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
    member _.Options 
        with get() = options 
        and set opts = 
            options <- opts

    static member Create (init : DockContainer -> unit) =
        let dc = DockContainer()
        dc.View init

    member __.View (init: DockContainer -> unit)  =  view init __

    member __.GetPaneConfigurationLocation( paneId : string, defaultValue : DockLocation ) =
        config 
        |> Option.bind (fun d -> d.TryFind (sprintf "pane.%s.location" paneId))
        |> Option.bind (fun loc -> DockLocation.TryParse loc)
        |> Option.defaultValue defaultValue

    member __.GetPaneConfigurationShow( paneId : string, defaultValue : bool ) =
        config 
        |> Option.bind (fun d -> d.TryFind (sprintf "pane.%s.show" paneId))
        |> Option.bind (fun s -> try System.Boolean.Parse s |> Some with | _ -> None)
        |> Option.defaultWith (fun () -> 
            // Fable.Core.JS.console.log("DockContainer: GetPaneConfigurationShow: 'pane." + paneId + ".show' not found, using default value: " + defaultValue.ToString())
            // Fable.Core.JS.console.log("DockContainer: GetPaneConfigurationShow: config: ", config |> Option.defaultValue Map.empty |> Seq.toArray)
            defaultValue)

    member __.Configuration
        with get() : obj =
            match rootElement with
            | Some rootElement ->
                let panes = 
                    model.Value.Docks.Stations 
                    |> Seq.collect (fun x -> 
                        x.Value.Panes 
                        |> List.collect (fun p -> 
                                [
                                    "pane." + p.Key + ".location", x.Key.ToString() 
                                    "pane." + p.Key + ".show", (DockHelpers.isPaneShowing model.Value p.Key).ToString()
                                ] ))
                    |> Seq.toArray

                [|
                    "left.width", (dockLeftContainer rootElement).style.width
                    "right.width", (dockRightContainer rootElement).style.width 
                    "top.height", (dockTopContainer rootElement).style.height 
                    "bottom.height", (dockBottomContainer rootElement).style.height 
                    "left-top.pct", (dockLeftTopContainer rootElement).style.flexGrow
                    "left-bottom.pct", (dockLeftBottomContainer rootElement).style.flexGrow
                    "right-top.pct", (dockRightTopContainer rootElement).style.flexGrow
                    "right-bottom.pct", (dockRightBottomContainer rootElement).style.flexGrow
                    "top-left.pct", (dockTopLeftContainer rootElement).style.flexGrow
                    "top-right.pct", (dockTopRightContainer rootElement).style.flexGrow
                    "bottom-left.pct", (dockBottomLeftContainer rootElement).style.flexGrow
                    "bottom-right.pct", (dockBottomRightContainer rootElement).style.flexGrow
                    yield! panes
                |] |> fun x -> x |> JsHelpers.createObject
            | _ -> {| |}

        and set( cfg : obj) =
            let kvs = JsHelpers.objectToNameValues cfg
            config <- Some (Map kvs)
            rootElement |> Option.iter applyOptions

    member __.SetProperty (name:string, p : DockProperty) =
        dispatch (DockProp (name,p))

    member __.SetProperties (name:string, props : seq<DockProperty>) =
        props |> Seq.iter (fun p -> __.SetProperty( name, p))

    member __.RemovePane(name : string) =
        dispatch (RemoveTab name)

    member __.AddPane (name : string, initLoc : DockLocation, content : SutilElement ) =
        __.AddPane( name, name, initLoc, content, true )

    member __.AddPane (name : string, initLoc : DockLocation, content : SutilElement, show : bool ) =
        __.AddPane( name, name, initLoc, content, show )

    member __.AddPane (name : string, label : string, initLoc : DockLocation, content : SutilElement ) =
        __.AddPane( name, label, initLoc, text label, content, true )

    member __.AddPane (name : string, label: string, initLoc : DockLocation, content : SutilElement, show : bool ) =
        __.AddPane( name, label, initLoc, text label, content, show )

    member __.ContainsPane( name : string ) = 
        (DockHelpers.tryGetPane model.Value.Docks name).IsSome
        
    member __.ShowPane( name : string ) =
        dispatch (ShowPane name)

    member __.ShowingPanes =
        model.Value.SelectedPanes.Values |> Seq.toArray |> Array.choose id

    member __.AddPane (key : string, label : string, initLoc : DockLocation, header : SutilElement, content : SutilElement, show : bool ) =
        __.AddPane { DockPane.Default(key) with Label = LabelString label; Location = initLoc; Header = header; Content = content; IsOpen = show}

    member __.AddPane (key, options : PaneOptions list) =
        __.AddPane( DockPane.Create(key, options ))

    member private __.AddPane (cfg : DockPane) =

        let loc = model |> Store.map (fun m -> (DockHelpers.findPaneLocation m.Docks cfg.Key)) |> Observable.distinctUntilChanged

        let toolbar =
            buttonGroup [
                buttonItem [ Icon "fa-window-minimize"; ButtonProperty.Label ""; OnClick (fun _ -> MinimizePane cfg.Key |> dispatch) ]
                dropDownItem [ Icon "fa-cog"; ButtonProperty.Label ""] [
                    menuItem [
                        ButtonProperty.Label "Move To"
                    ] [
                        buttonItem [ Icon "fa-caret-square-left"; ButtonProperty.Label "Left Top"; OnClick (fun _ -> MoveTo (cfg.Key,LeftTop) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-left"; ButtonProperty.Label "Left Bottom"; OnClick (fun _ -> MoveTo (cfg.Key,LeftBottom) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-right"; ButtonProperty.Label "Right Top"; OnClick (fun _ -> MoveTo (cfg.Key,RightTop) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-right"; ButtonProperty.Label "Right Bottom"; OnClick (fun _ -> MoveTo (cfg.Key,RightBottom) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-down"; ButtonProperty.Label "Bottom Left"; OnClick (fun _ -> MoveTo (cfg.Key,BottomLeft) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-down"; ButtonProperty.Label "Bottom Right"; OnClick (fun _ -> MoveTo (cfg.Key,BottomRight) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-up"; ButtonProperty.Label "Top Left"; OnClick (fun _ -> MoveTo (cfg.Key,TopLeft) |> dispatch) ]
                        buttonItem [ Icon "fa-caret-square-up"; ButtonProperty.Label "Top Right"; OnClick (fun _ -> MoveTo (cfg.Key,TopRight) |> dispatch) ]
                        buttonItem [ Icon "fa-square"; ButtonProperty.Label "Centre Left"; OnClick (fun _ -> MoveTo (cfg.Key,CentreLeft) |> dispatch) ]
                        buttonItem [ Icon "fa-square"; ButtonProperty.Label "Centre Right"; OnClick (fun _ -> MoveTo (cfg.Key,CentreRight) |> dispatch) ]
                    ]
                ]
            ]

        let wrapper =
            UI.divc "dock-pane-wrapper" [

                Attr.id ("pane-" + cfg.Key.ToLower().Replace(".","_").Replace("#","_"))

                Bind.toggleClass(
                    model
                    |> Store.map (fun m -> (DockHelpers.findPaneLocation m.Docks cfg.Key
                    |> Option.bind (fun l -> m.SelectedPanes[l]) |> Option.defaultValue "") = cfg.Key),
                    "selected")

                Bind.visibility
                    (loc |> Store.map (fun optLoc -> cfg.Location <> CentreLeft || optLoc <> Some CentreLeft))
                    (UI.divc "pane-header" [
                        Html.div [
                            cfg.Header
                            toolbar
                            // Bind.visibility
                            //     (loc |> Store.map (fun optLoc -> cfg.Location <> CentreCentre || optLoc <> Some CentreCentre))
                            //     toolbar
                        ]
                    ])

                UI.divc "pane-content" [
                    cfg.Content
                ]
            ]

        let unmount = (DomHelpers.getContentParentNode cfg.Location, wrapper) |> Program.mountAppend

        unmounters <- unmounters.Add( cfg.Key, unmount )

        dispatch <| Message.AddTab cfg // (cfg.Key, cfg.Label,"",cfg.Location,cfg.IsOpen)

// let container (tabLabels : DockCollection) =
//     let c = DockContainer()
//     c.View
