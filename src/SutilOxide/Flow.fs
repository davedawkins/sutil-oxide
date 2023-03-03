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

[<AutoOpen>]
module Types =

    type FlowId = string

    type Node<'NodeData> = {
        Id : FlowId
        Type : string
        X : float
        Y : float
        ZIndex : int
        ClassName : string
        Data : 'NodeData
        SourceLocation : BasicLocation
        TargetLocation : BasicLocation
        CanSelect : bool
        CanMove : bool }
        with
        static member Create( id : FlowId, x : float, y : float, data : 'NodeData ) = {
            Id = id
            Type = "default"
            X = x
            Y = y
            ZIndex = 0
            ClassName = ""
            Data = data
            CanSelect = true
            CanMove = true
            SourceLocation = Bottom
            TargetLocation = Top
        }

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

    type Edge<'EdgeData> = {
        Id : FlowId
        Source : NodePort
        Target : NodePort
        Data : 'EdgeData
    }
    with
        static member Create( id : FlowId, node1 : FlowId, port1 : string, node2 : FlowId, port2 : string, data : 'EdgeData ) =
        {
            Id = id
            Source = { NodeId = node1; PortId = port1 }
            Target = { NodeId = node2; PortId = port2 }
            Data = data
        }

    type Graph<'NodeData,'EdgeData> = {
        Nodes : Map<FlowId,Node<'NodeData> >
        Edges : Map<FlowId,Edge<'EdgeData> >
    }

    type Background =
        | Clear
        | Dotted

    type NodeRenderer<'a> = Node<'a> -> Core.SutilElement

    type ChartOptions<'a> = {
        SnapToGrid : bool
        SnapToGridSize : int
        NodeRenderers : Map<string,NodeRenderer<'a>>
        Css : StyleSheetDefinitions
        NodeTypes : Map<string,Port array>
    }
    with
        static member Create() =
            {
                SnapToGrid = true
                SnapToGridSize = 5
                NodeRenderers = Map.empty
                Css = []
                NodeTypes = Map [
                    "input",  [| { Id = "in"; Type = "default"; Mode = Input } |]
                    "output", [| { Id = "out"; Type = "default"; Mode = Output } |]
                    "default",
                        [|
                            { Id = "in"; Type = "default"; Mode = Input }
                            { Id = "out"; Type = "default"; Mode = Output }
                        |]
                ]
            }


[<AutoOpen>]
module Helpers =
    let asEl (e : Node) = e :?> HTMLElement
    let targetEl (e : Event) = e.target :?> HTMLElement
    let currentEl (e : Event) = e.currentTarget :?> HTMLElement

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

    let clientXY (e : MouseEvent) =
        let br = (e |> currentEl).getBoundingClientRect()
        (e.x - br.left, e.y - br.top)

    let parentXY (e : MouseEvent) =
        let br = (e |> currentEl).parentElement.getBoundingClientRect()
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

    let findPortFromEvent (e:MouseEvent) (g:Graph<'a,'b>) =
        e
        |> targetEl
        |> climbToPortNode
        //|> Option.map (fun nodeEl -> nodeEl)

    let findNodeFromEvent (e:MouseEvent) (g:Graph<'a,'b>) =
        e
        |> targetEl
        |> climbToGraphNode
        |> Option.map (fun nodeEl ->
            nodeEl, g.Nodes[nodeEl.getAttribute("x-node-id")]
        )

    let containerNodeFromNodeEl (nodeEl : HTMLElement) =
        nodeEl.parentElement

module Updates =

    type Model<'NodeData,'EdgeData> = {
        Graph : Graph<'NodeData,'EdgeData>
        Selection : Set<string>
    }

    type Message =
        | MoveNode of string * float * float
        | ClearSelection
        | Select of string
        | SelectOnly of string

    let init g =
        {
            Graph = g
            Selection = Set.empty
        }, Cmd.none

    let update (msg : Message) model =
        match msg with
        | ClearSelection ->
            { model with Selection = Set.empty }, Cmd.none
        | Select id ->
            { model with Selection = model.Selection.Add(id) }, Cmd.none
        | SelectOnly id ->
            { model with Selection = Set.empty.Add(id) }, Cmd.none
        | MoveNode (id,x,y) ->
            let g = model.Graph
            let n = g.Nodes[id]
            let g' = { g with Nodes = g.Nodes.Add( id, { n with X = x; Y = y }) }
            { model with Graph = g' }, Cmd.none


module Edges =

    let drawEdgeSvg (pos1:BasicLocation) (x1:float) (y1:float) (pos2:BasicLocation) (x2:float) (y2:float) (edgeStyle:string) =
        //let (|LeftRight|TopBottom|) = function |Left|Right-> LeftRight|_ -> TopBottom
        let isLR = function |Left|Right-> true|_->false
        let isTB = not<<isLR

        let pathData =
            match edgeStyle with
            | "straight" -> sprintf "M %f,%fL %f,%f" x1 y1 x2 y2

            | "bezier" ->
                let dx, dy = x2 - x1, y2 - y1
                let mx, my = x1 + (dx / 2.0), y1 + (dy / 2.0)
                let px0,py0,px1,py1 =
                    match pos1, pos2 with
                    | Left,Right | Right,Left -> mx,y1,mx,y2
                    | Top,Bottom | Bottom,Top -> x1,my,x2,my
                    | src, dst when isLR src && isTB dst -> x2,y1, x2,y2
                    | src, dst when isTB src && isLR dst -> x1,y2, x2,y2
                    | _ -> x1,my,x2,my
                sprintf "M%f,%f C%f,%f %f,%f %f,%f" x1 y1 px0 py0 px1 py1 x2 y2

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

    let selectHandler dispatch (node : Node<'NodeData>) = [
        Ev.onClick (fun e ->
            let el : HTMLElement = e.target |> asElement
            e.stopPropagation()
            deselectAll (el.parentElement)
            select el
        )
    ]

    let handleNodeMouseDown (e : MouseEvent) options dispatch (nodeEl : HTMLElement) (node : Node<'a>) =
        let containerEl = containerNodeFromNodeEl nodeEl

        if (node.CanSelect) then
            deselectAll (containerEl)
            select (nodeEl)

        if (node.CanMove) then
            let sx,sy = parentXY e

            let snap (nx,ny) =
                if options.SnapToGrid then
                    let gs = float options.SnapToGridSize
                    gs * System.Math.Round(nx/gs), gs * System.Math.Round(ny / gs)
                    //(nx,ny)
                else
                    (nx,ny)

            let newXY (cx,cy) =
                (node.X + cx - sx, node.Y + cy - sy) |> snap

            let stop = listen "mousemove" containerEl (fun e ->
                let nx, ny = parentXY (e :?> MouseEvent) |> newXY
                nodeEl.style.left <- $"{nx}px"
                nodeEl.style.top <- $"{ny}px"
            )

            once "mouseup" containerEl (fun e ->
                stop()
                e.stopPropagation()

                let nx,ny = parentXY (e :?> MouseEvent) |> newXY

                dispatch (MoveNode (node.Id, nx, ny))
                if (node.CanSelect) then
                    dispatch (SelectOnly node.Id)
            )
        else
            dispatch ClearSelection
            if (node.CanSelect) then
                dispatch (Select node.Id)

    let handlePortMouseDown (e : MouseEvent) (portEl : HTMLElement) =
        let nodeEl = portEl.parentElement
        let containerEl = containerNodeFromNodeEl nodeEl
        let containerR = containerEl.getBoundingClientRect()

//        let sx,sy = clientXY e

//        let portRect = portEl.getBoundingClientRect();
//        let sx, sy = (portRect.left + portRect.width / 2.0) - containerR.left, (portRect.top + portRect.height / 2.0) - containerR.top
        let sx, sy = centreXY(portEl) |> toLocalXY containerEl

//        Fable.Core.JS.console.log("port down", e, portRect, (sx,sy) )

        let mousexy = Store.make (sx,sy)

        let dragLine =
            Bind.el( mousexy, fun (x,y) ->
                Edges.drawEdgeSvg Top sx sy Bottom x y "bezier"
                // Svg.line [
                //     Attr.x1 (px sx)
                //     Attr.y1 (px sy)
                //     Attr.x2 (px x)
                //     Attr.y2 (px y)
                //     Attr.stroke "black"
                // ]
            )

        let unmount = ("graph-edges-id",dragLine) |> Program.mount

        let stop = listen "mousemove" containerEl (fun e ->
            if (targetEl e).classList.contains("port") then
                (targetEl e) |> centreXY |> toLocalXY containerEl |> Store.set mousexy
            else
                (e :?> MouseEvent) |> clientXY |> Store.set mousexy

        )

        once "mouseup" containerEl (fun e ->
            SutilOxide.Logging.log("mouseup")
            if (targetEl e).classList.contains("port") then
                (targetEl e) |> centreXY |> toLocalXY containerEl |> Store.set mousexy
            stop()
            unmount.Dispose()
        )

    let containerEventHandlers options model dispatch = [
        Ev.onMouseDown (fun e ->
            match findPortFromEvent e ((model |> Store.get).Graph) with
            | Some (portEl) ->
                handlePortMouseDown e portEl
            | None ->
                match findNodeFromEvent e ((model |> Store.get).Graph) with
                | None ->
                    dispatch ClearSelection
                | Some (nodeEl, node) ->
                    handleNodeMouseDown e options dispatch nodeEl node
        )
    ]

module Styles =

    let styleDefault = [

        rule ".graph-container" [
            Css.positionRelative
            Css.width (percent 100)
            Css.height (percent 100)
            Css.cursorGrab
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

        rule ".port" [
            Css.cursorCrosshair
            Css.positionAbsolute
            Css.displayBlock
            Css.width (px (portSize))
            Css.height (px (portSize))
            Css.border (px 1, borderStyle.solid, "white")
            Css.borderRadius (percent 50)
            Css.backgroundColor "#888888"
        ]

        rule ".port.left" [
            Css.left (px -(portSize / 2.0 - 0.5))
        ]

        rule ".port.right" [
            Css.right (px -(portSize / 2.0 - 0.5))
        ]

        rule ".port.top" [
            Css.top (px -(portSize / 2.0 - 0.5))
        ]

        rule ".port.bottom" [
            Css.bottom (px -(portSize / 2.0 - 0.5))
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

        rule ".edge:hover .edge-stroke" [
            Css.custom("stroke","orange")
        ]

        rule "path.animated" [
            Css.custom("stroke-dasharray", "5")
            Css.custom("animation", "dashdraw .5s linear infinite")
        ]


    ]

module Views =
    open EventHandlers
    open Updates

    let background (bg: Background)=
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
            ]

    let container elements =
        Html.div [
            Attr.className "graph-container"
            background Dotted

            yield! elements
        ]

    let renderPort(node : Node<'a>)  (port : Port) =
        let loc = if port.Mode = Input then node.TargetLocation else node.SourceLocation
        Html.div [
            Attr.style [ Css.zIndex (node.ZIndex + 1) ]
            Attr.custom( "x-node-id", node.Id )
            Attr.custom( "x-port-id", port.Id )
            Attr.className( $"port {port.Mode.ToString().ToLower()} {loc.ToString().ToLower()}" )
        ]

    let renderPorts (options : ChartOptions<'a>) (node : Node<'a>) =
        options.NodeTypes.TryFind(node.Type)
        |> Option.map (fun ports -> ports |> Array.map (renderPort node))
        |> Option.defaultValue [| |]

    let injectNodeDefaults  (model : Model<'a,'b>) options (node : Node<'a>) (view) =
        view |> CoreElements.inject [
            Attr.custom("x-node-id", node.Id)
            ([
                "node"
                if node.ClassName <> "" then node.ClassName
                if model.Selection.Contains (node.Id) then "selected"
            ] |> String.concat " " |> Attr.className)

            Attr.style [
                Css.left (px (node.X))
                Css.top (px (node.Y))
            ]

            yield! renderPorts options node
        ]

    let renderNodeDefault (node : Node<'a>) =
        Html.div [
            Html.span [
                Attr.className "data"
                text (sprintf "%A" node.Data)
            ]
        ]

    let renderNodeType (options : ChartOptions<'a>) (node : Node<'a>) =
        options.NodeRenderers.TryFind( node.Type )
        |> Option.map (fun render -> render node)
        |> Option.defaultValue (renderNodeDefault node)

    let renderNode (model : Model<'a,'b>) options (node : Node<'a>) =
        renderNodeType options node |> injectNodeDefaults model options node

    let findPortXY (np : NodePort) =
        let el = Browser.Dom.document.querySelector( sprintf ".port[x-node-id='%s'][x-port-id='%s']" np.NodeId np.PortId )
        if isNull el then
            (0.0,0.0)
        else
            let portEl = el :?> HTMLElement
            let containerEl = portEl.parentElement.parentElement
            centreXY portEl |> toLocalXY containerEl

    let renderEdge (model : Model<'a,'b>) options (edge : Edge<'b>) =
        let x1, y1 = findPortXY( edge.Source )
        let x2, y2 = findPortXY( edge.Target )
        Edges.drawEdgeSvg (BasicLocation.Bottom) x1 y1 (BasicLocation.Top) x2 y2 "bezier"

    let renderGraph (graph : Graph<'NodeData,'EdgeData> ) (options : ChartOptions<'NodeData>) =

        let model, dispatch = graph |> Store.makeElmish (Updates.init) (Updates.update) ignore

        container [
            yield! containerEventHandlers options model dispatch

            Bind.el( model, fun m ->
                fragment (m.Graph.Nodes.Values |> Seq.map (renderNode m options))
            )

            Svg.svg [
                Attr.id "graph-edges-id"
                Attr.className "graph-edges"
                Attr.style [
                    Css.custom("pointer-events","none")
                    Css.positionAbsolute
                    Css.custom("z-index","4")
                    Css.width (percent 100)
                    Css.height (percent 100)
                ]
                Bind.el( model, fun m ->
                    fragment (m.Graph.Edges.Values |> Seq.map (renderEdge m options))
                )
            ]
        ] |> withStyle options.Css

type FlowChart<'NodeData>() =
    let mutable options =
        ChartOptions<'NodeData>.Create()


    do
        addGlobalStyleSheet Browser.Dom.document Styles.styleDefault |> ignore

    member __.Render( graph : Graph<'NodeData,'b> ) =
        Views.renderGraph graph options
