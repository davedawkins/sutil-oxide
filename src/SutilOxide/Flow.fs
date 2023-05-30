module SutilOxide.Flow

open Feliz
open type Feliz.length
open Sutil
open Sutil.CoreElements
open Sutil.Styling
open Browser.Types
open DomHelpers
open Browser.DomExtensions
open Browser.CssExtensions
open Types


module SutilKeyed =
    open Sutil.Core
    open Fable.Core.JsInterop

    let getNodeKey (node : Node) (key : string) (create : unit -> 'T)=
        let v: obj = node?key

        if isNull v then
            let result = create()
            node?key <- result
            result
        else
            v :?> 'T

    type KeyedInfo<'T> = {
        Node : SutilEffect
        Value : IStore<'T>
    }

    let keyedUnordered (items : System.IObservable<'T seq>) (view: IReadOnlyStore<'T> -> SutilElement) (key : 'T -> 'K) = 
        SutilElement.Define("keyedUnordered",

        fun ctx ->
            let mutable keyMap : Map<'K,KeyedInfo<'T>> = Map.empty
            let group = SutilEffect.MakeGroup("keyed",ctx.Parent,ctx.Previous)
            let keyedNode = Group group
            let keyedCtx = ctx |> ContextHelpers.withParent keyedNode

            // Listen for changes to collection
            let unsub = items.Subscribe( fun newItems ->

                let mutable newKeyMap : Map<'K,KeyedInfo<'T>> = Map.empty

                // Process collection items
                newItems |> Seq.iter (fun (item : 'T) ->
                    let k = key item

                    let itemRec =
                        match keyMap.TryFind k with
                        | None ->
                            let store = Store.make item
                            let sutilNode = keyedCtx |> build (view store)
                            {
                                Node = sutilNode
                                Value =store
                            }
                        | Some r ->
                            item |> Store.set r.Value
                            r

                    newKeyMap <- newKeyMap.Add(k, itemRec) )

                // Remove missing items from document
                keyMap |> Seq.iter( fun kv ->if not (newKeyMap.ContainsKey kv.Key) then kv.Value.Node.Dispose())

                // Update current items
                keyMap <- newKeyMap
            )

            group.RegisterUnsubscribe ( fun () -> 
                unsub.Dispose()
            )

            keyedNode
        )
    
[<AutoOpen>]
module Types =

    type FlowId = string

    type Node = {
        Id : FlowId
        Type : string
        Rect : Rect
        ZIndex : int
        ClassName : string
        SourceLocation : BasicLocation
        TargetLocation : BasicLocation }
        with
        static member Create( id : FlowId, typ: string, x : float, y : float ) = {
            Id = id
            Type =typ
            Rect = Rect.Create(x, y, 100, 62)
            ZIndex = 0
            ClassName = ""
            SourceLocation = Bottom
            TargetLocation = Top
        }
        member __.SetXY( x, y ) = { __ with Rect = { __.Rect with X = x; Y = y } }

    type PortMode =
        | Input
        | Output

    type Port = {
        Id : FlowId
        Type : string
        Mode : PortMode
    }

    type NodePort = {
        NodeId : FlowId
        PortId : FlowId
    }

    type Edge = {
        Id : FlowId
        Source : NodePort
        Target : NodePort
    }
    with
        static member Create( id : FlowId, node1 : FlowId, port1 : string, node2 : FlowId, port2 : string ) =
            {
                Id = id
                Source = { NodeId = node1; PortId = port1 }
                Target = { NodeId = node2; PortId = port2 }
            }

    type Graph = 
        {
            Nodes : Map<FlowId,Node >
            Edges : Map<FlowId,Edge >
        }
        static member Empty = { Nodes = Map.empty; Edges = Map.empty}

    type Background =
        | Clear
        | Dotted

    type NodeRenderer = Node -> Core.SutilElement

    type GraphOptions = {

        SnapToGrid : bool
        SnapToGridSize : int

        Css : StyleSheetDefinitions

        NodePorts : string -> Port array
        NodeFactory : string*string -> Node
        EdgeFactory : string*string*string*string*string -> Edge

        ViewNode: IReadOnlyStore<Node> -> Sutil.Core.SutilElement

        OnChange : Graph -> unit
        OnAddNode: Node -> unit
        OnRemoveNode: Node -> unit
        OnAddEdge: Edge -> unit
        OnRemoveEdge: Edge -> unit
        OnSelectionChange: (Node list * Edge list -> unit)
        CanSelect: (Node -> bool)
        CanMove: (Node -> bool)
        CanResize: (Node -> bool)
    }
    with
        static member DefaultInPorts = 
                        [|
                            { Id = "In"; Type = "default"; Mode = Input }
                        |]
        static member DefaultOutPorts = 
                        [|
                            { Id = "Out"; Type = "default"; Mode = Output }
                        |]
        static member DefaultPorts = 
                        [|
                            { Id = "In"; Type = "default"; Mode = Input }
                            { Id = "Out"; Type = "default"; Mode = Output }
                        |]
        static member Create() =

            let renderNodeDefault (node : IReadOnlyStore<Node>) =
                Html.div [
                    Html.span [
                        Attr.className "data"
                        text (sprintf "%A" node.Value.Id)
                    ]
                ]
            let defaultPortTypes =
                Map [
                    "input",  GraphOptions.DefaultInPorts
                    "output", GraphOptions.DefaultOutPorts
                    "default",GraphOptions.DefaultPorts
                ]
            {
                SnapToGrid = true
                SnapToGridSize = 5
                Css = []
                NodePorts = (fun typ ->
                    defaultPortTypes 
                    |> Map.tryFind typ 
                    |> Option.defaultValue defaultPortTypes["default"]
                )
                NodeFactory = (fun (name,typ) -> Node.Create(name, typ, 0, 0))
                EdgeFactory = (fun (name,n1,p1,n2,p2) -> Edge.Create(name, n1,p1, n2, p2))
                ViewNode = renderNodeDefault
                OnChange = ignore
                OnSelectionChange = ignore
                OnAddNode = ignore
                OnRemoveNode = ignore
                OnAddEdge = ignore
                OnRemoveEdge = ignore
                CanSelect = (fun _ -> true)
                CanMove = (fun _ -> true)
                CanResize = (fun _ -> true)
            }


[<AutoOpen>]
module Helpers =
    let asEl (e : Browser.Types.Node) = e :?> HTMLElement
    let targetEl (e : Event) = e.target :?> HTMLElement
    let currentEl (e : Event) = e.currentTarget :?> HTMLElement

    // let nodeContainsPoint (x,y) (node : Node) =
    //     x >= node.X && x <= node.X + node.Width &&
    //     y >= node.Y && y <= node.Y + node.Height

    let isGraphNode (e : HTMLElement) =
        e.classList.contains("node")

    let isPortNode (e : HTMLElement) =
        e.classList.contains("port")

    let climbToGraphNode (e : HTMLElement) : HTMLElement option=
        let mutable gn = e
        while not (isNull gn) && not (isGraphNode gn) do
            gn <- gn.parentElement
        if isNull gn then None else Some gn

    let climbToPortNode (e : HTMLElement) : HTMLElement option=
        let mutable gn = e
        while not (isNull gn) && not (isPortNode gn) do
            gn <- gn.parentElement
        if isNull gn then None else Some gn

    let getElementXY (el : HTMLElement) =
        let parentB = el.parentElement.getBoundingClientRect()
        let br = el.getBoundingClientRect()
        (br.left - parentB.left, br.top - parentB.top)

    let xyToLr x1 x2 =
        if (x2 > x1) then x1,x2 else x2,x1

    let rectIntersect (ax,ay,ax2,ay2) (bx,by,bx2,by2) =
        let a_left, a_right = xyToLr ax ax2
        let a_top, a_bottom = xyToLr ay ay2
        let b_left, b_right = xyToLr bx bx2
        let b_top, b_bottom = xyToLr by by2

        a_left <= b_right &&
        b_left <= a_right &&
        a_top <= b_bottom &&
        b_top <= a_bottom

    let clientXY (e : MouseEvent) =
        let br = (e |> targetEl).getBoundingClientRect()
        (e.x - br.left, e.y - br.top)

    let parentXY (e : MouseEvent) =
        let br = (e |> targetEl).parentElement.getBoundingClientRect()
        (e.clientX - br.left, e.clientY - br.top)

    let centreXY (el : HTMLElement) =
        let r = el.getBoundingClientRect()
        r.left + r.width / 2.0, r.top + r.height / 2.0

    let toLocalXY (el : HTMLElement) (clientX,clientY) =
        let r = el.getBoundingClientRect()
        clientX - r.left, clientY - r.top

    let deselectAll (container : HTMLElement) =
        container.parentElement.querySelectorAll(".node").toSeq() |> Seq.iter (fun el -> (el :?> HTMLElement).classList.remove("selected"))

    let select (el : HTMLElement) =
        el.classList.add("selected")

    let findPortFromEvent (e:MouseEvent) (g:Graph) =
        e
        |> targetEl
        |> climbToPortNode
        //|> Option.map (fun nodeEl -> nodeEl)

    let findNodeFromEvent (e:MouseEvent) (g:Graph) =
        e
        |> targetEl
        |> climbToGraphNode
        |> Option.map (fun nodeEl ->
            nodeEl, g.Nodes[nodeEl.getAttribute("x-node-id")]
        )

    let containerElFromNodeEl (nodeEl : HTMLElement) =
        nodeEl.parentElement

    let nodeElFromPortEl (portEl : HTMLElement) =
        portEl.parentElement.parentElement

    
    let toJsonString (value : 'T) = Fable.Core.JS.JSON.stringify(value)
    let fromJsonString (value : string) : 'T = Fable.Core.JS.JSON.parse(value) :?> 'T


module Updates =
    open Browser
    open Fable.Core.JsInterop

    // This was intended to watch for any movement of the ports, even when dragging. 
    // It does at least solve the problem upon initial viewing where ports were not yet
    // placed.
    type PortXYs() =
        let mutable callbacks : (unit -> unit) list = []
        let makeObserver(callback) =
            let options =
                {| root = document :> Browser.Types.Node; rootMargin = ""; threshold = 0.0 |}
            IntersectionObserver.Create(callback, !! options)

//        MutationObserver.Create(callback)
        let mutable nodePorts : Map<string,IStore<float*float>> = Map.empty

        let observer = makeObserver( fun (entries) _ -> 
            entries |> Array.iter (fun entry -> 
                entry.target?_flowcb()
        ))

        member __.GetStore(key) =
            if not (nodePorts.ContainsKey key) then
                nodePorts <- nodePorts.Add(key, Store.make (0.0,0.0))
            nodePorts[key]

        member __.Monitor( el : HTMLElement, callback : unit -> unit ) =
            el?_flowcb <- callback
            callbacks <- callback :: callbacks
            callback()
            observer.observe(el)

        member __.UpdateAll() =
            callbacks |> List.iter (fun f -> f())

        member __.Update( key, xy : float*float ) =
            xy |> Store.set (__.GetStore key)

        member __.GetStore( key1, key2 ) =
            __.GetStore key2 |> Store.zip (__.GetStore key1)

        member __.Clear() = 
            nodePorts <- Map.empty
            observer.disconnect()

    type Model = {
        Graph : Graph
        Selection : Set<string>
        MovingNode : bool
        _ViewTransform : Transform2D
        Origin : float * float
        Scale : float
        Bounds : Rect
    }

    let localToScreenT (t:Transform2D) (x,y) = 
        //(x,y)
        t.Transform(x,y)

    let screenToLocalT (t:Transform2D) (x,y) =
        //(x,y)
        t.TransformInverse(x,y)

    let localToScreen (m : Model) (x,y) = 
        localToScreenT (m._ViewTransform) (x,y)

    let screenToLocal (m : Model) (x,y) =
        screenToLocalT (m._ViewTransform) (x,y)

    type Message =
        | DeleteNode of string
        | MoveNodes of (string * float * float) list
        | MoveResizeNode of string * float * float * float * float
        | DeleteSelection
        | ClearSelection
        | Select of string list
        | SelectOnly of string list
        | SetLocation of (string * float * float)
        | AddNode of (string * float * float)
        | AddEdge of (string * string * string * string)
        | NotifyChange
        | SetMovingNode of bool
        | SetScale of float
        | SetOrigin of (float * float)
        | CalculateBounds

    let init g =
        {
            Graph = g
            Selection = Set.empty
            MovingNode = false
            _ViewTransform = Transform2D.Empty
            Origin = 0,0
            Scale = 1
            Bounds = Rect.Empty
        }, Cmd.ofMsg CalculateBounds

    let findNodeEdges (g : Graph) (nodeId) =
        g.Edges.Values |> Seq.filter (fun e -> e.Source.NodeId = nodeId || e.Target.NodeId = nodeId)

    let deleteNode options (g : Graph) (nodeId) =
        let node = g.Nodes[nodeId]
        let edges = findNodeEdges g nodeId

        let nodes' = g.Nodes.Remove(nodeId)
        let edges' : Map<FlowId,Edge> = edges |> Seq.fold (fun edges e -> edges.Remove(e.Id) ) g.Edges

        { g with Nodes = nodes'; Edges = edges' }, 
            [ 
                fun _ -> edges |> Seq.iter (options.OnRemoveEdge )
                fun _ -> options.OnRemoveNode(node) 
            ]

    let deleteNodes options (g : Graph) (nodes : string seq) =
        nodes |> Seq.fold (
            fun (g,cmd) n -> 
                let g', c' = deleteNode options g n
                (g', cmd @ c')
            ) (g, [])

    let makeName (name : string) (exists : string -> bool) =
        let mutable i = 0
        let mutable result = ""

        let nextName() =
            i <- i + 1
            result <- sprintf "%s%d" name i

        nextName()

        while exists result do
            nextName()

        result

    let makeNode options m nodeType x y =
        let name = makeName nodeType (fun s -> m.Graph.Nodes.ContainsKey(s))
        let newNode : Node = options.NodeFactory(name,nodeType)
        {
            newNode with
                Id = name
                Rect = { newNode.Rect with X = x; Y = y }
        }

    let withNode (n : Node) (g : Graph) =
        { g with Nodes = g.Nodes.Add(n.Id, n)}

    let withEdge (n : Edge) (g : Graph) =
        { g with Edges = g.Edges.Add(n.Id, n)}

    let selectedNodes model = 
        model.Selection |> Seq.map (fun s -> model.Graph.Nodes[s])

    let isNodeSelected (node : Node) (model : Model) =
        model.Selection.Contains(node.Id)

    let msgSelectionChange (model : Model) options dispatch =
        let nodes = selectedNodes model |> Seq.toList
        options.OnSelectionChange( nodes, [ ])


    let calculateBounds (nodes : Node seq)  =
        nodes 
        |> Seq.fold (fun ((minx,miny),(maxx,maxy)) n -> 
            (System.Math.Min(n.Rect.X, minx),System.Math.Min(n.Rect.Y,miny)),
            (System.Math.Max(n.Rect.X2,maxx),System.Math.Max(n.Rect.Y2,maxy))
            )
            ((0.0,0.0),(0.0,0.0))
        |> Rect.Create

    let update options (portxys : PortXYs) (msg : Message) model =
        Fable.Core.JS.console.log( sprintf "Flow: Update: %A" msg)
        match msg with

        | CalculateBounds ->
            let bounds = calculateBounds (model.Graph.Nodes.Values)
            let ox, oy = System.Math.Min(0, bounds.X), System.Math.Min(0, bounds.Y)
            if bounds <> model.Bounds then
                { model with 
                    Bounds = bounds
                }, Cmd.ofMsg (SetOrigin (ox,oy))
            else
                model, Cmd.none

        | SetOrigin (ox,oy) ->
            { model with Origin = (ox,oy); _ViewTransform = Transform2D.TranslateScale(-ox,-oy,model.Scale)}, Cmd.none

        | SetScale s ->
            let ox,oy = model.Origin
            { model with Scale = s; _ViewTransform = Transform2D.TranslateScale(-ox,-oy,s)}, Cmd.none

        | SetMovingNode f ->
            { model with MovingNode = f }, Cmd.none

        | NotifyChange ->
            model, [ fun _ -> options.OnChange model.Graph ]

        | DeleteSelection ->
            let graph, cmd = deleteNodes options (model.Graph) (model.Selection)
            { model with Graph = graph  }, Cmd.batch [ Cmd.ofMsg ClearSelection; cmd; Cmd.ofMsg NotifyChange ]

        | DeleteNode n ->
            let node = model.Graph.Nodes[ n ]
            let graph, cmd = deleteNode options (model.Graph) n
            { model with Graph = graph }, 
                Cmd.batch [ 
                    Cmd.ofMsg ClearSelection; 
                    cmd; 
                    Cmd.ofMsg CalculateBounds
                    Cmd.ofMsg NotifyChange 
                ]

        | AddEdge (n1,p1,n2,p2) ->
            let name = sprintf "e-%s-%s-%s-%s" n1 p1 n2 p2
            let e = options.EdgeFactory( name, n1, p1, n2,p2 )
            { model with Graph = model.Graph |> withEdge e }, 
                Cmd.batch [ [fun _ -> options.OnAddEdge(e)]; Cmd.ofMsg NotifyChange ]

        | SetLocation (nodeId, x, y) ->
            let node = model.Graph.Nodes[nodeId].SetXY(x,y)
            { model with Graph = model.Graph |> withNode node }, 
            Cmd.batch [
                Cmd.ofMsg CalculateBounds
                Cmd.ofMsg NotifyChange
            ]

        | AddNode (nodeType, x, y) -> 
            let node = makeNode options model nodeType x y

            let autoConnectCmd = 
                if (model.Selection.Count = 1) then
                    let selectedNode = model.Graph.Nodes[ model.Selection |> Seq.head ]
                    let portFrom = options.NodePorts (selectedNode.Type) |> Array.tryFind (fun p -> p.Mode = Output)
                    let portTo = options.NodePorts (node.Type) |> Array.tryFind (fun p -> p.Mode = Input)
                    match (portFrom, portTo) with
                    | Some a, Some b -> Cmd.ofMsg (AddEdge (selectedNode.Id,a.Id,node.Id,b.Id))
                    | _ -> Cmd.none
                else
                    Cmd.none

            { model with Graph = model.Graph |> withNode node }, 
                Cmd.batch [ 
                    [ fun _ -> options.OnAddNode(node) ] 
                    autoConnectCmd; 
                    Cmd.ofMsg (SelectOnly [ node.Id ]) 
                    Cmd.ofMsg CalculateBounds
                    Cmd.ofMsg NotifyChange
                    [ fun _ -> portxys.UpdateAll() ]
                ]
    
        | ClearSelection ->
            let m = { model with Selection = Set.empty }
            m, [ msgSelectionChange m options ]

        | Select ids ->
            let m = { model with Selection = ids |> List.fold (fun s id -> s.Add(id)) model.Selection }
            m, [ msgSelectionChange m options ]
   
        | SelectOnly ids ->
            let m = { model with Selection = Set ids }

            m, [ msgSelectionChange m options ]

        | MoveNodes nodeXys ->
            let moveNodes : Map<FlowId,Node> =
                nodeXys 
                |> List.fold 
                    (fun nodes (id,x,y) -> 
                        nodes.Add( id, nodes[id].SetXY(x,y)) 
                    ) 
                    (model.Graph.Nodes)

            { model with Graph = { model.Graph with Nodes = moveNodes } }, 
                Cmd.batch [
                    Cmd.ofMsg CalculateBounds
                    Cmd.ofMsg NotifyChange
                    [ fun _ -> portxys.UpdateAll() ]
                ]

        | MoveResizeNode (id,x,y,w,h) ->
            let g = model.Graph
            let n = g.Nodes[id]
            let g' = { g with Nodes = g.Nodes.Add( id, { n with Rect = Rect.Create(x,y,w,h) }) }
            { model with Graph = g' },
                Cmd.batch [
                    Cmd.ofMsg CalculateBounds
                    Cmd.ofMsg NotifyChange
                    [ fun _ -> portxys.UpdateAll() ]
                ]

module Edges =

    let drawEdgeSvg (pos1:BasicLocation) (x1:float) (y1:float) (pos2:BasicLocation) (x2:float) (y2:float) (edgeStyle:string) =
        let isLR = function |Left|Right-> true|_->false
        let isTB = not<<isLR

        let pathData =
            match edgeStyle with
            | "straight" -> sprintf "M %f,%fL %f,%f" x1 y1 x2 y2

            | "bezier" ->
                let dx, dy = x2 - x1, y2 - y1
                let mx, my = x1 + (dx / 2.0), y1 + (dy / 2.0)
                let points = //px0,py0,px1,py1 =
                    match pos1, pos2 with
                    | Left,Right -> [ mx,y1; mx,y2 ]
                    | Right,Left -> 
                        [ mx,y1; mx,y2 ]
                    | Top,Bottom -> 
                        [ x1, my; x2,my ]
                    | Bottom,Top -> 
                        if y2 > (y1 + 40.0) then
                             [x1,my; x2,my]
                        else
                            [ 
                                x1, y1 + 20.0
                                ((x1 + x2) / 2.0), y1 + 20.0
                                ((x1 + x2) / 2.0), y2 - 20.0
                                x2, y2 - 20.0
                            ]
                    // | src, dst when isLR src && isTB dst -> x2,y1, x2,y2
                    // | src, dst when isTB src && isLR dst -> x1,y2, x2,y2
                    | _ -> [ x1,my; x2,my ]
                //sprintf "M%f,%f C%f,%f %f,%f %f,%f" x1 y1 px0 py0 px1 py1 x2 y2

                let pstr = points |> List.map ( fun (x,y) -> sprintf "%f,%f" x y ) |> String.concat " "
                sprintf "M%f,%f L%s %f,%f" x1 y1 pstr x2 y2

            | _ -> sprintf "M %f,%fL %f,%f" x1 y1 x2 y2

        Svg.g [
            Attr.className "edge"
            Svg.path [
                Attr.className "edge-click"
                Attr.style [
                    Css.custom("pointer-events","auto")
                ]
                Attr.d pathData
                Ev.onClick (fun _ -> SutilOxide.Logging.log("click on path"))
            ]
            Svg.path [
                Attr.className "edge-stroke animated"
                Attr.custom("marker-end","none")
                Attr.d pathData
            ]
        ]

module EventHandlers =

    open Updates
    open DomHelpers

    let resizeHandler (model : IStore<Model>) dispatch (nodeId : string) (rectS : IStore<Rect>) (x:int,y:int) = 

        Ev.onMouseDown( fun e -> 
            let handleEl = targetEl e
            let containerEl = containerElFromNodeEl handleEl

            e.preventDefault()
            e.stopImmediatePropagation()
            e.stopPropagation()

            let handleStartX, handleStartY = getElementXY(handleEl)
            let sx,sy = parentXY e
            let sxOffset, syOffset = sx - handleStartX, sy - handleStartY

            let snap (dx:float,dy:float) = 
                (dx * float(System.Math.Abs(x)), dy * float(System.Math.Abs(y)))

            let newXY (nx,ny) =
                let dx, dy = (nx - sx, ny - sy) |> snap
                (handleStartX + dx + sxOffset, handleStartY + dy + syOffset)

            let stop = listen "mousemove" containerEl (fun e ->
                let nx, ny = parentXY (e :?> MouseEvent) |> newXY
                let rect = rectS.Value.Transform(model.Value._ViewTransform)

                let newRectX, newRectW = 
                    match x with 
                    | -1 -> nx, rect.Width + (rect.X - nx)
                    | 1 -> rect.X, (nx - rect.X)
                    | _ -> rect.X, rect.Width

                let newRectY, newRectH = 
                    match y with 
                    | -1 -> ny, rect.Height + (rect.Y - ny)
                    | 1 -> rect.Y, (ny - rect.Y)
                    | _ -> rect.Y, rect.Height

                let newRect :Rect = 
                    {
                        rect with 
                            X = newRectX; Width = newRectW
                            Y = newRectY; Height = newRectH
                    }.TransformInverse(model.Value._ViewTransform)

                newRect |> Store.set rectS

                let nodeRect = 
                    newRect |> (fun r ->
                        if r.Width < 0 then 
                            { r with X = r.X + r.Width; Width = -r.Width }
                        else    
                            r )
                    |> (fun r ->
                        if r.Height < 0 then 
                            { r with Y = r.Y + r.Height; Height = -r.Height }
                        else    
                            r )

                dispatch (MoveResizeNode (nodeId,nodeRect.X,nodeRect.Y, nodeRect.Width, nodeRect.Height))
            )

            once "mouseup" containerEl (fun e ->
                stop()
                e.stopPropagation()
            )
        )

    let selectHandler dispatch (node : Node) = [
        Ev.onClick (fun e ->
            let el : HTMLElement = e.target |> asElement
            e.stopPropagation()
            deselectAll (el.parentElement)
            select el
        )
    ]


    let drawSelectionRect (x1:float) (y1:float) (x2:float) (y2:float) =

        Svg.g [
            Attr.className "selection-rect"
            Svg.rect [
                Attr.x (px x1)
                Attr.y (px y1)
                Attr.width (px (x2 - x1))
                Attr.height (px (y2 - y1))
            ]
        ]

    let distSq dx dy = 
        dx*dx + dy*dy
    let handleContainerMouseDown (containerEvent : MouseEvent) options (model : IStore<Model>) dispatch =
        let containerEl = currentEl containerEvent
        //Fable.Core.JS.console.log( containerEl )

        let sx,sy = parentXY containerEvent
        let mousexy = Store.make (sx,sy)

        let dragRect =
            Bind.el( mousexy, fun (x,y) ->
                drawSelectionRect sx sy x y
            )

        let mutable unmount : System.IDisposable option = None
        let stop = listen "mousemove" containerEl (fun e ->
            let me = (e :?> MouseEvent)
            //Fable.Core.JS.console.log("move", me.x, me.y)
            let nx,ny = (me.x,me.y) |> toLocalXY containerEl

            if unmount.IsNone && (distSq (nx-sx) (ny-sy) > 64) then
                unmount <- (Browser.Dom.document.getElementById("graph-edges-id"),dragRect) |> Program.mountAppend |> Some

            (nx,ny) |> Store.set mousexy

            let nodeIds = 
                model.Value.Graph.Nodes.Values 
                |> Seq.filter (fun n ->
                    let x1,y1 = localToScreen model.Value (n.Rect.X, n.Rect.Y )
                    let x2,y2 = localToScreen model.Value (n.Rect.X2, n.Rect.Y2 )
                    rectIntersect (x1,y1,x2,y2) (sx, sy, nx, ny)
                ) 
                |> Seq.map (fun n -> n.Id) |> Seq.toList
            //Fable.Core.JS.console.log(sprintf "%A" nodeIds)
            dispatch (SelectOnly nodeIds)
            ()
        )

        once "mouseup" containerEl (fun e ->
            stop()

            match unmount with
            | Some d ->
                d.Dispose()
            | None ->
                dispatch ClearSelection

            e.stopPropagation()
        )

    let handleNodeMouseDown (containerEvent : MouseEvent) options (model : IStore<Model>) dispatch (nodeEl : HTMLElement) (node : Node) =
        let containerEl = containerElFromNodeEl nodeEl

        if (options.CanSelect node && not (model.Value.Selection.Contains node.Id)) then
            dispatch (SelectOnly [ node.Id ])
            // deselectAll (containerEl)
            // select (nodeEl)

        if (options.CanMove node) then
            let sx,sy = parentXY containerEvent
            let selectedNodeIds = 
                model.Value.Selection
                |> Seq.map (fun id -> model.Value.Graph.Nodes[id])
                |> Seq.toList

            let snap (nx,ny) =
                if options.SnapToGrid then
                    let gs = float options.SnapToGridSize
                    gs * System.Math.Round(nx/gs), gs * System.Math.Round(ny / gs)
                    //(nx,ny)
                else
                    (nx,ny)

            let newXY (cx,cy) (node : Node) =
                // To screen space
                let nodeX,nodeY = localToScreen model.Value (node.Rect.X, node.Rect.Y )

                // Calculate in screen space
                let screen_nx, screen_ny = ((nodeX + cx - sx, nodeY + cy - sy) |> snap)

                let nx, ny = screenToLocal model.Value ( screen_nx, screen_ny )                
                node.Id, nx, ny

            dispatch (SetMovingNode true)

            let stop = listen "mousemove" containerEl (fun e ->
                let (cx,cy) = parentXY (e :?> MouseEvent)

                let moves =
                    selectedNodeIds 
                    |> List.map (newXY (cx,cy))

                //let nx, ny = parentXY (e :?> MouseEvent) |> newXY
                dispatch (MoveNodes moves)
                // nodeEl.style.left <- $"{nx}px"
                // nodeEl.style.top <- $"{ny}px"
            )

            once "mouseup" containerEl (fun e ->
                stop()
                e.stopPropagation()

                // let nx,ny = parentXY (e :?> MouseEvent) |> newXY

                // dispatch (MoveNode (node.Id, nx, ny))
                // if (options.CanSelect node) then
                //     dispatch (SelectOnly [node.Id])
                dispatch (SetMovingNode false)
            )
        else
            dispatch ClearSelection
            if (options.CanSelect node) then
                dispatch (Select [ node.Id ])

    let handlePortMouseDown (e : MouseEvent) dispatch (portEl : HTMLElement) =
        let nodeEl =  nodeElFromPortEl portEl
        let containerEl = containerElFromNodeEl nodeEl
//        let containerR = containerEl.getBoundingClientRect()

        let sx, sy = centreXY(portEl) |> toLocalXY containerEl
        SutilOxide.Logging.log("sx, sy: " + string sx + ", " + string sy)

        let mousexy = Store.make (sx,sy)

        let dragLine =
            Bind.el( mousexy, fun (x,y) ->
                Edges.drawEdgeSvg Bottom sx sy Top x y "bezier"
            )

        let unmount = (Browser.Dom.document.getElementById("graph-edges-id"),dragLine) |> Program.mountAppend

        let stop = listen "mousemove" containerEl (fun e ->
            if (targetEl e).classList.contains("port") then
                (targetEl e) |> centreXY |> toLocalXY containerEl |> Store.set mousexy
            else
                (e :?> MouseEvent) |> clientXY |> Store.set mousexy

        )

        once "mouseup" containerEl (fun e ->
//            SutilOxide.Logging.log("mouseup")
            let el = targetEl e
            if el.classList.contains("port") then
                let sourceNodeId = portEl.getAttribute("x-node-id")
                let sourcePortId = portEl.getAttribute("x-port-id")
                let targetNodeId = el.getAttribute("x-node-id")
                let targetPortId = el.getAttribute("x-port-id")
//                SutilOxide.Logging.log($"connection: {sourceNodeId}:{sourcePortId} -> {targetNodeId}:{targetPortId}")
//                Fable.Core.JS.console.log(el)
                el |> centreXY |> toLocalXY containerEl |> Store.set mousexy
                dispatch (AddEdge (sourceNodeId,sourcePortId,targetNodeId,targetPortId))
            else
                SutilOxide.Logging.log("no connection")
            stop()
            unmount.Dispose()
        )


    let private handleDrop (options : GraphOptions) model dispatch (e : Browser.Types.DragEvent) =
        let x,y = clientXY e

        let nodeType = e.dataTransfer.getData("x/type")
        let nodeName = e.dataTransfer.getData("x/name")

        let offsetX, offsetY : float*float = e.dataTransfer.getData("x/offset") |> Helpers.fromJsonString
        let nodeX = x - offsetX 
        let nodeY = y - offsetY 
        
        if nodeType <> "" then
            dispatch (AddNode (nodeType, nodeX, nodeY))

        if nodeName <> "" then
            ( nodeName, nodeX, nodeY ) |> SetLocation |> dispatch
            dispatch (SelectOnly [nodeName])

        e.preventDefault()

    let containerEventHandlers options model dispatch = [

        Attr.tabIndex 0

        Ev.onKeyDown (fun e ->
            if e.key = "Backspace" then
                dispatch DeleteSelection
                e.preventDefault()
                e.stopPropagation()
        )

        Ev.onMouseDown (fun e ->
            match findPortFromEvent e ((model |> Store.get).Graph) with
            | Some (portEl) ->
                handlePortMouseDown e dispatch portEl
            | None ->
                match findNodeFromEvent e ((model |> Store.get).Graph) with
                | None ->
                    handleContainerMouseDown e options model dispatch
                | Some (nodeEl, node) ->
                    handleNodeMouseDown e options model dispatch nodeEl node
        )

        Ev.onDrop (handleDrop options model dispatch)
    ]

module Styles =

    let styleDefault = [

        rule ".graph-container" [
            Css.positionRelative
            Css.width (percent 100)
            Css.height (percent 100)
            //Css.cursorGrab
        ]

        rule ".node" [
            Css.positionAbsolute
            Css.displayFlex
            Css.flexDirectionColumn
            Css.alignItemsCenter
            Css.justifyContentCenter
            Css.padding (rem 1)
            Css.width auto
            Css.height auto
            Css.backgroundColor "white"
            Css.cursorGrab
        ]

        rule ".node:after" [
            Css.positionAbsolute
            Css.displayBlock
            Css.top 0
            Css.left 0
            Css.right 0
            Css.bottom 0
            Css.custom("content", "''")
            Css.border( px 1, borderStyle.solid, "#888888")
            Css.borderRadius (px 4)
        ]

        rule ".node.selected:after" [
            Css.borderWidth (px 2)
        ]

        let portSize = 9.0

        rule ".port-group" [
            Css.positionAbsolute
            Css.displayFlex
            Css.custom("justify-content", "space-evenly")
        ]

        rule ".port-group.top" [
            Css.flexDirectionRow
            Css.top (px -(portSize / 2.0 - 0.5))
            Css.width (percent 100)
        ]

        rule ".port-group.bottom" [
            Css.flexDirectionRow
            Css.bottom (px -(portSize / 2.0 - 0.5))
            Css.width (percent 100)
        ]

        rule ".port-group.left" [
            Css.flexDirectionColumn
            Css.left (px -(portSize / 2.0 - 0.5))
            Css.height (percent 100)
        ]

        rule ".port-group.right" [
            Css.flexDirectionColumn
            Css.right (px -(portSize / 2.0 - 0.5))
            Css.height (percent 100)
        ]

        rule ".port" [
            Css.cursorCrosshair
            Css.displayBlock
            Css.width (px (portSize))
            Css.height (px (portSize))
            Css.border (px 1, borderStyle.solid, "white")
            Css.borderRadius (percent 50)
            Css.backgroundColor "#888888"
        ]

        rule ".port:hover" [
            Css.backgroundColor "black"
        ]

        // -- Edges -----------------------------------------------------------

        rule ".edge" [
            Css.custom("stroke","#999")
            Css.custom("stroke-width","1")
            Css.custom("fill", "none")
        ]

        rule ".edge-click" [
            Css.custom("stroke","transparent")
            Css.custom("stroke-width","5")
            Css.custom("fill", "none")
        ]

        keyframes "dashdraw" [
            keyframe 0 [
                Css.custom("stroke-dashoffset", "10")
            ]
        ]

        rule ".edge-stroke" [
            Css.custom("stroke","#999")
            Css.custom("stroke-width","1")
            Css.custom("fill", "none")
        ]

        rule ".selection-rect rect" [
            Css.custom("stroke","#999")
            Css.custom("stroke-width","1")
            Css.custom("fill", "none")
        ]

        rule ".edge:hover .edge-stroke" [
            Css.custom("stroke","orange")
            Css.custom("stroke-width","2")
        ]

        rule "path.animated" [
            Css.custom("stroke-dasharray", "5")
            Css.custom("animation", "dashdraw .5s linear infinite")
        ]

        rule ".graph-edges" [
            Css.custom("pointer-events","none")
            Css.positionAbsolute
            Css.custom("z-index","4")
            Css.width (percent 100)
            Css.height (percent 100)
        ]

        rule ".resize-handle" [
            Css.positionAbsolute
            Css.backgroundColor "#ffffff"
            Css.border (px 1, Feliz.borderStyle.solid, "#000000")
            Css.width (px 8)
            Css.height (px 8)
            Css.zIndex 1
            Css.custom( "transform" , "translate(-50%, -50%)")
        ]

        rule ".resize-handle.top" [
            Css.cursorNorthSouthResize
        ]
        rule ".resize-handle.bottom" [
            Css.cursorNorthSouthResize
        ]
        rule ".resize-handle.left" [
            Css.cursorEastWestResize
        ]
        rule ".resize-handle.right" [
            Css.cursorEastWestResize
        ]
        rule ".resize-handle.top.left" [
            Css.cursorNorthWestSouthEastResize
        ]
        rule ".resize-handle.top.right" [
            Css.cursorNorthEastSouthWestResize
        ]
        rule ".resize-handle.bottom.left" [
            Css.cursorNorthEastSouthWestResize
        ]
        rule ".resize-handle.bottom.right" [
            Css.cursorNorthWestSouthEastResize
        ]
    ]

module Views =
    open EventHandlers
    open Updates

    let setBounds(widthHeightS : System.IObservable<(float*float)*Rect*Transform2D>) =
        Bind.style(widthHeightS, 
            fun style ( ((ox,oy),r,t) : (float*float) * Rect * Transform2D) ->

            //Fable.Core.JS.console.log(sprintf "bounds (model space): %f %f %f %f" r.X r.Y r.Width r.Height)

            let screenRect = r.Transform(t)

            //Fable.Core.JS.console.log(sprintf "bounds (screen space): %f %f %f %f" screenRect.X screenRect.Y screenRect.Width screenRect.Height)

            let w = System.Math.Abs(screenRect.Width)
            let h = System.Math.Abs(screenRect.Height)

            if w <> 0.0 && h <> 0.0 then
                style.width <- sprintf "max(100%%,%fpx)" w
                style.height <- sprintf "max(100%%,%fpx)" h
            
            //style.transform <- sprintf "translate(%fpx,%fpx)" -ox -oy
        )

    let background boundsView (bg: Background)=
        match bg with
        | Clear -> fragment []
        | Dotted ->
            Svg.svg [
                Attr.style [
                    Css.width (percent 100)
                    Css.height (percent 100)
                    Css.positionAbsolute
                ]
                Svg.pattern [
                    Attr.id "pattern-bg"
                    Svg.x 0
                    Svg.y 0
                    Svg.width 15
                    Svg.height 15
                    Attr.custom("patternUnits", "userSpaceOnUse")
                    Svg.circle [
                        Attr.cx (px 0.4)
                        Attr.cy (px 0.4)
                        Attr.r (px 0.4)
                        Attr.custom("fill","#81818a")
                    ]
                ]
                Svg.rect [
                    Svg.x 0
                    Svg.y 0
                    Svg.width (percent 100)
                    Svg.height (percent 100)
                    Attr.custom ("fill", "url(#pattern-bg)")
                ]
                //setBounds boundsView // Bind bounds to container width / height
            ]

    let renderPort (portxys : PortXYs) (node : Node)  (port : Port) =
        let loc = if port.Mode = Input then node.TargetLocation else node.SourceLocation
        Html.div [
            Attr.style [ Css.zIndex (node.ZIndex + 1) ]
            Attr.custom( "x-node-id", node.Id )
            Attr.custom( "x-port-id", port.Id )
            Attr.custom( "x-port-loc", loc.LowerName )
            Attr.className( $"port {port.Mode.ToString().ToLower()} {loc.LowerName}" )
            Ev.onMount (fun e ->
                let portEl = Helpers.targetEl e
                let nodeEl =  nodeElFromPortEl portEl
                let containerEl = containerElFromNodeEl nodeEl

    //            Fable.Core.JS.console.log("Container", containerEl.getBoundingClientRect())
                let key = sprintf "%s-%s" (node.Id) (port.Id)

                portxys.Monitor( portEl, fun _ -> 
                    let xy = portEl |> centreXY |> toLocalXY containerEl
                    //Fable.Core.JS.console.log(sprintf "** %s-%s: %A" (node.Id) (port.Id) xy)       
                    portxys.Update(key, xy)
                )
            )
        ]

    let renderPorts (options : GraphOptions) portxys (node : Node) =
        [
            Html.divc (sprintf "port-group input %s" node.TargetLocation.LowerName) [
                yield! options.NodePorts(node.Type) |> Array.filter (fun p -> p.Mode = Input) |> Array.map (renderPort portxys node)
            ]
            Html.divc (sprintf "port-group output %s" node.SourceLocation.LowerName) [
                yield! options.NodePorts(node.Type) |> Array.filter (fun p -> p.Mode = Output) |> Array.map (renderPort portxys node)
            ]
        ]

    let handleXY (x,y) (rect : Rect) =
        let dx = rect.Width / 2.0
        let dy = rect.Height / 2.0
        let cx = rect.X + dx
        let cy = rect.Y + dy

        let handlex = cx + (float x) * dx
        let handley = cy + (float y) * dy
        (handlex,handley)

    let resizeHandle (model : IStore<Model>) dispatch (nodeId : string) (rect : IStore<Rect>) (x,y) =
        let lr = match x with -1 -> " left"| 1 -> " right" | _ -> ""
        let tb = match y with -1 -> " top"| 1 -> " bottom" | _ -> ""

        Html.divc (sprintf "resize-handle%s%s" tb lr )
            [
                EventHandlers.resizeHandler model dispatch nodeId rect (x,y)
                Bind.style( rect |> Store.map (handleXY (x,y)), fun style (hx,hy) -> 
                    let screenHx, screenHy = localToScreen model.Value (hx,hy)
                    style.left <- (string screenHx) + "px"
                    style.top <- (string screenHy) + "px"
                )
            ]

    let renderResizeHandles model dispatch (node : Node) =
        let rectS = Store.make (node.Rect)
        [
            yield! [
                (-1,-1); (-1,0); (-1,1)
                ( 0,-1);         ( 0,1)
                ( 1,-1); ( 1,0); ( 1,1)
                ] 
                |> List.map (resizeHandle model dispatch (node.Id) rectS)
        ]
        
    let injectNodeDefaults (model:IStore<Model>) dispatch (isSelected : System.IObservable<bool>) options portxys (nodeS : IReadOnlyStore<Node>) view =

        let node = nodeS.Value

        view |> CoreElements.inject [

            Attr.custom("x-node-id", node.Id)
            ([
                "node"
                if node.ClassName <> "" then node.ClassName
            ] |> String.concat " " |> Attr.className)

            Bind.toggleClass( isSelected, "selected")

            Bind.style( nodeS |> Store.zip (model .>> (fun m -> m._ViewTransform)), 
                fun style (vt,node) ->
                let x1, y1 = localToScreenT vt ( node.Rect.X, node.Rect.Y )
                let x2, y2 = localToScreenT vt ( node.Rect.X2, node.Rect.Y2 )

                style.left <- sprintf "%fpx" x1
                style.top <- sprintf "%fpx" y1
                style.width <- sprintf "%fpx" (x2 - x1)
                style.height <- sprintf "%fpx" (y2 - y1)
            )

            yield! renderPorts options portxys node
        ]

    let renderNode (model:IStore<Model>) dispatch (isSelected : System.IObservable<bool>) options portxys (node : IReadOnlyStore<Node>) =
        options.ViewNode(node) |> injectNodeDefaults model dispatch isSelected options portxys node

    let findPortXY (model : Model) options (np : NodePort) =
        let node = model.Graph.Nodes[np.NodeId]
        let ports = options.NodePorts( node.Type )
        let port = ports |> Array.find (fun p -> p.Id = np.PortId)

        let loc = if port.Mode = Input then node.TargetLocation else node.SourceLocation

        let rect = node.Rect
        match loc with 
        | Left   -> rect.X,                    rect.Y + rect.Height / 2.0
        | Right  -> rect.X + rect.Width,       rect.Y + rect.Height / 2.0
        | Top    -> rect.X + rect.Width / 2.0, rect.Y
        | Bottom -> rect.X + rect.Width / 2.0, rect.Y + rect.Height
        | Centre -> rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0

    let renderEdge (model : Model) options (portxys : PortXYs) (edge : Edge) =
        let x1, y1 = findPortXY model options edge.Source
        let x2, y2 = findPortXY model options edge.Target
        let key1 = sprintf "%s-%s" (edge.Source.NodeId) (edge.Source.PortId)
        let key2 = sprintf "%s-%s" (edge.Target.NodeId) (edge.Target.PortId)
        Bind.el( portxys.GetStore(key1,key2), (fun ((x1,y1), (x2,y2)) ->
            Edges.drawEdgeSvg (BasicLocation.Bottom) x1 y1 (BasicLocation.Top) x2 y2 "bezier"
        ))

    let resizeNode options (m : Model) =
        match selectedNodes m |> List.ofSeq with
        | [ n ] when options.CanResize n && not (m.MovingNode) -> Some n
        | _ -> None

    let renderGraph (graph : Graph ) (options : GraphOptions) =

        let portXYs = PortXYs()
        let model, dispatch = graph |> Store.makeElmish (Updates.init) (Updates.update options portXYs) ignore
        let boundsView = model .>> (fun m -> m.Origin, m.Bounds, m._ViewTransform)

        Html.divc "graph-container" [
            
            background boundsView Dotted
            setBounds boundsView // Bind bounds to container width / height

            // Install the event handlers
            yield! containerEventHandlers options model dispatch

            // Render the nodes
            SutilKeyed.keyedUnordered
                (model .> (fun m -> m.Graph.Nodes.Values))
                (fun n -> renderNode model dispatch (model .>> (isNodeSelected n.Value)) options portXYs n)
                (fun n -> n.Id)

            // Bind.each(
            //     (model .> (fun m -> m.Graph.Nodes.Values |> Seq.toList)),
            //     (fun n -> renderNode dispatch (model .>> (isNodeSelected n)) options portXYs n),
            //     (fun n -> n.Id)
            // )

            Bind.el( model .>> (resizeNode options), fun optNode ->
                match optNode with
                | None -> 
                        fragment []
                | Some (node) -> 
                        fragment <| renderResizeHandles model dispatch node
            )

            // Render the edges
            Svg.svg [
                Attr.id "graph-edges-id"
                Attr.className "graph-edges"
                Bind.el( model, fun m ->
                    Svg.g (m.Graph.Edges.Values |> Seq.map (renderEdge m options portXYs))
                )
            ]
        ] |> withStyle options.Css

    let makeCatalogItem (nodeType : string, el : Sutil.Core.SutilElement) =
        el |> CoreElements.inject [
            Attr.custom("x-node-type", nodeType)
            Attr.draggable true
            Ev.onDragStart (fun e ->
                e.dataTransfer.setData("x/offset", toJsonString(clientXY e)) |> ignore
                e.dataTransfer.setData("x/type", nodeType) |> ignore)
        ]

type FlowChart( options : GraphOptions ) =
    do
        addGlobalStyleSheet Browser.Dom.document Styles.styleDefault |> ignore

    member __.Render( graph : Graph ) =
        Views.renderGraph graph options
