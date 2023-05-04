import { toString, Union, Record } from "../fable_modules/fable-library.3.7.20/Types.js";
import { unit_type, tuple_type, lambda_type, array_type, list_type, class_type, union_type, record_type, bool_type, int32_type, float64_type, string_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { BasicLocation__get_LowerName, BasicLocation, BasicLocation$reflection } from "./Types.js";
import { FSharpMap__Remove, FSharpMap__get_Values, FSharpMap__Add, FSharpMap__ContainsKey, FSharpMap__get_Item, tryFind, ofSeq, empty } from "../fable_modules/fable-library.3.7.20/Map.js";
import { StyleSheetDefinition$reflection } from "../Sutil/src/Sutil/Types.js";
import { SutilElement$reflection } from "../Sutil/src/Sutil/Core.js";
import { HtmlEngine$1__span_BB573A, HtmlEngine$1__div_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { AttrEngine$1__draggable_Z1FBCCD16, AttrEngine$1__r_Z445F6BAF, AttrEngine$1__cy_Z445F6BAF, AttrEngine$1__cx_Z445F6BAF, AttrEngine$1__id_Z721C83C5, AttrEngine$1__tabIndex_Z524259A4, AttrEngine$1__custom_Z384F8060, AttrEngine$1__d_Z721C83C5, AttrEngine$1__className_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { SutilHtmlEngine__divc, SutilEventEngine__onMount_7DDE0344, EngineHelpers_Ev, EngineHelpers_Css, SutilAttrEngine__style_68BDC580, EngineHelpers_Html, EngineHelpers_Attr } from "../Sutil/src/Sutil/Html.js";
import { disposeOnUnmount, inject, fragment, text } from "../Sutil/src/Sutil/CoreElements.js";
import { join, printf, toText } from "../fable_modules/fable-library.3.7.20/String.js";
import { int32ToString, disposeSafe, round, equals, comparePrimitives } from "../fable_modules/fable-library.3.7.20/Util.js";
import { map as map_2, singleton, ofArray, empty as empty_1 } from "../fable_modules/fable-library.3.7.20/List.js";
import { map, defaultArg } from "../fable_modules/fable-library.3.7.20/Option.js";
import { empty as empty_3, singleton as singleton_1, append, delay, head, map as map_1, toList, fold, filter, iterate } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { once, listen, asElement, Browser_Types_NodeListOf$1__NodeListOf$1_toSeq } from "../Sutil/src/Sutil/DomHelpers.js";
import { StoreOperators_op_DotGreaterGreater, Store_makeElmish, Store_get, Store_zip, Store_set, Store_make } from "../Sutil/src/Sutil/Store.js";
import { FSharpSet__Contains, FSharpSet__Add, FSharpSet__get_Count, empty as empty_2 } from "../fable_modules/fable-library.3.7.20/Set.js";
import { Cmd_batch, Cmd_ofMsg, Cmd_none } from "../Sutil/src/Sutil/Cmd.js";
import { map as map_3, tryFind as tryFind_1 } from "../fable_modules/fable-library.3.7.20/Array.js";
import { SvgEngine$1__rect_BB573A, SvgEngine$1__circle_BB573A, SvgEngine$1__pattern_BB573A, SvgEngine$1__svg_BB573A, SvgEngine$1__path_BB573A, SvgEngine$1__g_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/SvgEngine.fs.js";
import { CssEngine$1__zIndex_Z524259A4, CssEngine$1__get_cursorCrosshair, CssEngine$1__right_Z445F6BAF, CssEngine$1__left_Z445F6BAF, CssEngine$1__bottom_Z445F6BAF, CssEngine$1__top_Z445F6BAF, CssEngine$1__get_flexDirectionRow, CssEngine$1__borderWidth_18A029B5, CssEngine$1__borderRadius_Z445F6BAF, CssEngine$1__border_Z6C024E7B, CssEngine$1__bottom_Z524259A4, CssEngine$1__right_Z524259A4, CssEngine$1__left_Z524259A4, CssEngine$1__top_Z524259A4, CssEngine$1__get_displayBlock, CssEngine$1__backgroundColor_Z721C83C5, CssEngine$1__padding_Z445F6BAF, CssEngine$1__get_justifyContentCenter, CssEngine$1__get_alignItemsCenter, CssEngine$1__get_flexDirectionColumn, CssEngine$1__get_displayFlex, CssEngine$1__get_positionAbsolute, CssEngine$1__get_cursorGrab, CssEngine$1__height_Z445F6BAF, CssEngine$1__width_Z445F6BAF, CssEngine$1__get_positionRelative, CssEngine$1__custom_Z384F8060 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { EventEngine$1__onDragStart_Z3384A56C, EventEngine$1__onDrop_Z3384A56C, EventEngine$1__onMouseDown_58BC8925, EventEngine$1__onKeyDown_Z2153A397, EventEngine$1__onClick_58BC8925 } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { log } from "./Logging.js";
import { SutilSvgEngine__get_height, SutilSvgEngine__get_width, SutilSvgEngine__get_y, SutilSvgEngine__get_x, SvgEngineHelpers_Svg } from "../Sutil/src/Sutil/Svg.js";
import { Bind_el_ZF0512D0 } from "../Sutil/src/Sutil/Bind.js";
import { Program_mountAppend_Z427DD8DF } from "../Sutil/src/Sutil/Program.js";
import { addGlobalStyleSheet, withStyle, keyframe, keyframes, rule } from "../Sutil/src/Sutil/Styling.js";
import { subscribe } from "../fable_modules/fable-library.3.7.20/Observable.js";

export class Types_Node extends Record {
    constructor(Id, Type, X, Y, Width, Height, ZIndex, ClassName, SourceLocation, TargetLocation, CanSelect, CanMove) {
        super();
        this.Id = Id;
        this.Type = Type;
        this.X = X;
        this.Y = Y;
        this.Width = Width;
        this.Height = Height;
        this.ZIndex = (ZIndex | 0);
        this.ClassName = ClassName;
        this.SourceLocation = SourceLocation;
        this.TargetLocation = TargetLocation;
        this.CanSelect = CanSelect;
        this.CanMove = CanMove;
    }
}

export function Types_Node$reflection() {
    return record_type("SutilOxide.Flow.Types.Node", [], Types_Node, () => [["Id", string_type], ["Type", string_type], ["X", float64_type], ["Y", float64_type], ["Width", float64_type], ["Height", float64_type], ["ZIndex", int32_type], ["ClassName", string_type], ["SourceLocation", BasicLocation$reflection()], ["TargetLocation", BasicLocation$reflection()], ["CanSelect", bool_type], ["CanMove", bool_type]]);
}

export function Types_Node_Create_3D3F00C0(id, typ, x, y) {
    return new Types_Node(id, typ, x, y, 100, 62, 0, "", new BasicLocation(4), new BasicLocation(3), true, true);
}

export class Types_PortMode extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Input", "Output"];
    }
}

export function Types_PortMode$reflection() {
    return union_type("SutilOxide.Flow.Types.PortMode", [], Types_PortMode, () => [[], []]);
}

export class Types_Port extends Record {
    constructor(Id, Type, Mode) {
        super();
        this.Id = Id;
        this.Type = Type;
        this.Mode = Mode;
    }
}

export function Types_Port$reflection() {
    return record_type("SutilOxide.Flow.Types.Port", [], Types_Port, () => [["Id", string_type], ["Type", string_type], ["Mode", Types_PortMode$reflection()]]);
}

export class Types_NodePort extends Record {
    constructor(NodeId, PortId) {
        super();
        this.NodeId = NodeId;
        this.PortId = PortId;
    }
}

export function Types_NodePort$reflection() {
    return record_type("SutilOxide.Flow.Types.NodePort", [], Types_NodePort, () => [["NodeId", string_type], ["PortId", string_type]]);
}

export class Types_Edge extends Record {
    constructor(Id, Source, Target) {
        super();
        this.Id = Id;
        this.Source = Source;
        this.Target = Target;
    }
}

export function Types_Edge$reflection() {
    return record_type("SutilOxide.Flow.Types.Edge", [], Types_Edge, () => [["Id", string_type], ["Source", Types_NodePort$reflection()], ["Target", Types_NodePort$reflection()]]);
}

export function Types_Edge_Create_6BD52AFB(id, node1, port1, node2, port2) {
    return new Types_Edge(id, new Types_NodePort(node1, port1), new Types_NodePort(node2, port2));
}

export class Types_Graph extends Record {
    constructor(Nodes, Edges) {
        super();
        this.Nodes = Nodes;
        this.Edges = Edges;
    }
}

export function Types_Graph$reflection() {
    return record_type("SutilOxide.Flow.Types.Graph", [], Types_Graph, () => [["Nodes", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, Types_Node$reflection()])], ["Edges", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, Types_Edge$reflection()])]]);
}

export function Types_Graph_get_Empty() {
    return new Types_Graph(empty(), empty());
}

export class Types_Background extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Clear", "Dotted"];
    }
}

export function Types_Background$reflection() {
    return union_type("SutilOxide.Flow.Types.Background", [], Types_Background, () => [[], []]);
}

export class Types_GraphOptions extends Record {
    constructor(SnapToGrid, SnapToGridSize, Css, NodePorts, NodeFactory, EdgeFactory, ViewNode, OnChange, OnSelectionChange) {
        super();
        this.SnapToGrid = SnapToGrid;
        this.SnapToGridSize = (SnapToGridSize | 0);
        this.Css = Css;
        this.NodePorts = NodePorts;
        this.NodeFactory = NodeFactory;
        this.EdgeFactory = EdgeFactory;
        this.ViewNode = ViewNode;
        this.OnChange = OnChange;
        this.OnSelectionChange = OnSelectionChange;
    }
}

export function Types_GraphOptions$reflection() {
    return record_type("SutilOxide.Flow.Types.GraphOptions", [], Types_GraphOptions, () => [["SnapToGrid", bool_type], ["SnapToGridSize", int32_type], ["Css", list_type(StyleSheetDefinition$reflection())], ["NodePorts", lambda_type(string_type, array_type(Types_Port$reflection()))], ["NodeFactory", lambda_type(tuple_type(string_type, string_type), Types_Node$reflection())], ["EdgeFactory", lambda_type(tuple_type(string_type, string_type, string_type, string_type, string_type), Types_Edge$reflection())], ["ViewNode", lambda_type(Types_Node$reflection(), SutilElement$reflection())], ["OnChange", lambda_type(Types_Graph$reflection(), unit_type)], ["OnSelectionChange", lambda_type(tuple_type(list_type(Types_Node$reflection()), list_type(Types_Edge$reflection())), unit_type)]]);
}

export function Types_GraphOptions_get_DefaultInPorts() {
    return [new Types_Port("In", "default", new Types_PortMode(0))];
}

export function Types_GraphOptions_get_DefaultOutPorts() {
    return [new Types_Port("Out", "default", new Types_PortMode(1))];
}

export function Types_GraphOptions_get_DefaultPorts() {
    return [new Types_Port("In", "default", new Types_PortMode(0)), new Types_Port("Out", "default", new Types_PortMode(1))];
}

export function Types_GraphOptions_Create() {
    const renderNodeDefault = (node) => HtmlEngine$1__div_BB573A(EngineHelpers_Html, [HtmlEngine$1__span_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "data"), text(toText(printf("%A"))(node.Id))])]);
    const defaultPortTypes = ofSeq([["input", Types_GraphOptions_get_DefaultInPorts()], ["output", Types_GraphOptions_get_DefaultOutPorts()], ["default", Types_GraphOptions_get_DefaultPorts()]], {
        Compare: comparePrimitives,
    });
    return new Types_GraphOptions(true, 5, empty_1(), (typ) => defaultArg(tryFind(typ, defaultPortTypes), FSharpMap__get_Item(defaultPortTypes, "default")), (tupledArg) => {
        const name = tupledArg[0];
        const typ_1 = tupledArg[1];
        return Types_Node_Create_3D3F00C0(name, typ_1, 0, 0);
    }, (tupledArg_1) => {
        const name_1 = tupledArg_1[0];
        const n1 = tupledArg_1[1];
        const p1 = tupledArg_1[2];
        const n2 = tupledArg_1[3];
        const p2 = tupledArg_1[4];
        return Types_Edge_Create_6BD52AFB(name_1, n1, p1, n2, p2);
    }, renderNodeDefault, (value_1) => {
    }, (value_2) => {
    });
}

export function Helpers_asEl(e) {
    return e;
}

export function Helpers_targetEl(e) {
    return e.target;
}

export function Helpers_currentEl(e) {
    return e.currentTarget;
}

export function Helpers_isGraphNode(e) {
    return e.classList.contains("node");
}

export function Helpers_isPortNode(e) {
    return e.classList.contains("port");
}

export function Helpers_climbToGraphNode(e) {
    let gn = e;
    while ((!(gn == null)) && (!Helpers_isGraphNode(gn))) {
        gn = gn.parentElement;
    }
    if (gn == null) {
        return void 0;
    }
    else {
        return gn;
    }
}

export function Helpers_climbToPortNode(e) {
    let gn = e;
    while ((!(gn == null)) && (!Helpers_isPortNode(gn))) {
        gn = gn.parentElement;
    }
    if (gn == null) {
        return void 0;
    }
    else {
        return gn;
    }
}

export function Helpers_clientXY(e) {
    const br = Helpers_currentEl(e).getBoundingClientRect();
    return [e.x - br.left, e.y - br.top];
}

export function Helpers_parentXY(e) {
    const br = Helpers_currentEl(e).parentElement.getBoundingClientRect();
    return [e.clientX - br.left, e.clientY - br.top];
}

export function Helpers_centreXY(el) {
    const r = el.getBoundingClientRect();
    return [r.left + (r.width / 2), r.top + (r.height / 2)];
}

export function Helpers_toLocalXY(el, clientX, clientY) {
    const r = el.getBoundingClientRect();
    return [clientX - r.left, clientY - r.top];
}

export function Helpers_deselectAll(container) {
    iterate((el) => {
        el.classList.remove("selected");
    }, Browser_Types_NodeListOf$1__NodeListOf$1_toSeq(container.parentElement.querySelectorAll(".node")));
}

export function Helpers_select(el) {
    el.classList.add("selected");
}

export function Helpers_findPortFromEvent(e, g) {
    return Helpers_climbToPortNode(Helpers_targetEl(e));
}

export function Helpers_findNodeFromEvent(e, g) {
    return map((nodeEl) => [nodeEl, FSharpMap__get_Item(g.Nodes, nodeEl.getAttribute("x-node-id"))], Helpers_climbToGraphNode(Helpers_targetEl(e)));
}

export function Helpers_containerElFromNodeEl(nodeEl) {
    return nodeEl.parentElement;
}

export function Helpers_nodeElFromPortEl(portEl) {
    return portEl.parentElement.parentElement;
}

export function Helpers_toJsonString(value) {
    return JSON.stringify(value);
}

export function Helpers_fromJsonString(value) {
    return JSON.parse(value);
}

export class Updates_PortXYs {
    constructor() {
        this.nodePorts = empty();
        this.observer = Updates_PortXYs__makeObserver_1DE5BA96(this, (entries, _arg) => {
            entries.forEach((entry) => {
                entry.target._flowcb();
            });
        });
    }
}

export function Updates_PortXYs$reflection() {
    return class_type("SutilOxide.Flow.Updates.PortXYs", void 0, Updates_PortXYs);
}

export function Updates_PortXYs_$ctor() {
    return new Updates_PortXYs();
}

export function Updates_PortXYs__GetStore_Z721C83C5(__, key) {
    if (!FSharpMap__ContainsKey(__.nodePorts, key)) {
        __.nodePorts = FSharpMap__Add(__.nodePorts, key, Store_make([0, 0]));
    }
    return FSharpMap__get_Item(__.nodePorts, key);
}

export function Updates_PortXYs__Monitor_Z164694C9(__, el, callback) {
    el._flowcb = callback;
    __.observer.observe(el);
}

export function Updates_PortXYs__Update_Z188742C(__, key, xy) {
    Store_set(Updates_PortXYs__GetStore_Z721C83C5(__, key), xy);
}

export function Updates_PortXYs__GetStore_Z384F8060(__, key1, key2) {
    const source_1 = Updates_PortXYs__GetStore_Z721C83C5(__, key2);
    return Store_zip(Updates_PortXYs__GetStore_Z721C83C5(__, key1), source_1);
}

export function Updates_PortXYs__Clear(__) {
    __.nodePorts = empty();
    __.observer.disconnect();
}

function Updates_PortXYs__makeObserver_1DE5BA96(this$, callback) {
    const options = {
        root: document,
        rootMargin: "",
        threshold: 0,
    };
    return new IntersectionObserver(callback, options);
}

export class Updates_Model extends Record {
    constructor(Graph, Selection$) {
        super();
        this.Graph = Graph;
        this.Selection = Selection$;
    }
}

export function Updates_Model$reflection() {
    return record_type("SutilOxide.Flow.Updates.Model", [], Updates_Model, () => [["Graph", Types_Graph$reflection()], ["Selection", class_type("Microsoft.FSharp.Collections.FSharpSet`1", [string_type])]]);
}

export class Updates_Message extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["DeleteNode", "MoveNode", "DeleteSelection", "ClearSelection", "Select", "SelectOnly", "SetLocation", "AddNode", "AddEdge"];
    }
}

export function Updates_Message$reflection() {
    return union_type("SutilOxide.Flow.Updates.Message", [], Updates_Message, () => [[["Item", string_type]], [["Item1", string_type], ["Item2", float64_type], ["Item3", float64_type]], [], [], [["Item", string_type]], [["Item", string_type]], [["Item", tuple_type(string_type, float64_type, float64_type)]], [["Item", tuple_type(string_type, float64_type, float64_type)]], [["Item", tuple_type(string_type, string_type, string_type, string_type)]]]);
}

export function Updates_init(g) {
    return [new Updates_Model(g, empty_2({
        Compare: comparePrimitives,
    })), Cmd_none()];
}

export function Updates_findNodeEdges(g, nodeId) {
    return filter((e) => {
        if (e.Source.NodeId === nodeId) {
            return true;
        }
        else {
            return e.Target.NodeId === nodeId;
        }
    }, FSharpMap__get_Values(g.Edges));
}

export function Updates_deleteNode(g, nodeId) {
    const edges = Updates_findNodeEdges(g, nodeId);
    const nodes$0027 = FSharpMap__Remove(g.Nodes, nodeId);
    const edges$0027 = fold((edges_1, e) => FSharpMap__Remove(edges_1, e.Id), g.Edges, edges);
    return new Types_Graph(nodes$0027, edges$0027);
}

export function Updates_deleteNodes(g, nodes) {
    return fold(Updates_deleteNode, g, nodes);
}

export function Updates_makeName(name, exists) {
    let i = 0;
    let result = "";
    const nextName = () => {
        let arg_1;
        i = ((i + 1) | 0);
        result = ((arg_1 = (i | 0), toText(printf("%s%d"))(name)(arg_1)));
    };
    nextName();
    while (exists(result)) {
        nextName();
    }
    return result;
}

export function Updates_makeNode(options, m, nodeType, x, y) {
    const name = Updates_makeName(nodeType, (s) => FSharpMap__ContainsKey(m.Graph.Nodes, s));
    const newNode = options.NodeFactory([name, nodeType]);
    return new Types_Node(name, newNode.Type, x, y, newNode.Width, newNode.Height, newNode.ZIndex, newNode.ClassName, newNode.SourceLocation, newNode.TargetLocation, newNode.CanSelect, newNode.CanMove);
}

export function Updates_withNode(n, g) {
    return new Types_Graph(FSharpMap__Add(g.Nodes, n.Id, n), g.Edges);
}

export function Updates_withEdge(n, g) {
    return new Types_Graph(g.Nodes, FSharpMap__Add(g.Edges, n.Id, n));
}

export function Updates_msgSelectionChange(model, options, dispatch) {
    const nodes = toList(map_1((s) => FSharpMap__get_Item(model.Graph.Nodes, s), model.Selection));
    options.OnSelectionChange([nodes, empty_1()]);
}

export function Updates_update(options, msg, model) {
    switch (msg.tag) {
        case 0: {
            const n = msg.fields[0];
            return [new Updates_Model(Updates_deleteNode(model.Graph, n), model.Selection), Cmd_ofMsg(new Updates_Message(3))];
        }
        case 8: {
            const p2 = msg.fields[0][3];
            const p1 = msg.fields[0][1];
            const n2 = msg.fields[0][2];
            const n1 = msg.fields[0][0];
            const name = toText(printf("e-%s-%s-%s-%s"))(n1)(p1)(n2)(p2);
            const e = options.EdgeFactory([name, n1, p1, n2, p2]);
            return [new Updates_Model(Updates_withEdge(e, model.Graph), model.Selection), Cmd_none()];
        }
        case 6: {
            const y = msg.fields[0][2];
            const x = msg.fields[0][1];
            const nodeId = msg.fields[0][0];
            let node;
            const inputRecord = FSharpMap__get_Item(model.Graph.Nodes, nodeId);
            node = (new Types_Node(inputRecord.Id, inputRecord.Type, x, y, inputRecord.Width, inputRecord.Height, inputRecord.ZIndex, inputRecord.ClassName, inputRecord.SourceLocation, inputRecord.TargetLocation, inputRecord.CanSelect, inputRecord.CanMove));
            return [new Updates_Model(Updates_withNode(node, model.Graph), model.Selection), Cmd_none()];
        }
        case 7: {
            const y_1 = msg.fields[0][2];
            const x_1 = msg.fields[0][1];
            const nodeType = msg.fields[0][0];
            const node_1 = Updates_makeNode(options, model, nodeType, x_1, y_1);
            let autoConnectCmd;
            if (FSharpSet__get_Count(model.Selection) === 1) {
                const selectedNode = FSharpMap__get_Item(model.Graph.Nodes, head(model.Selection));
                const portFrom = tryFind_1((p) => equals(p.Mode, new Types_PortMode(1)), options.NodePorts(selectedNode.Type));
                const portTo = tryFind_1((p_1) => equals(p_1.Mode, new Types_PortMode(0)), options.NodePorts(node_1.Type));
                const matchValue = [portFrom, portTo];
                let pattern_matching_result, a, b;
                if (matchValue[0] != null) {
                    if (matchValue[1] != null) {
                        pattern_matching_result = 0;
                        a = matchValue[0];
                        b = matchValue[1];
                    }
                    else {
                        pattern_matching_result = 1;
                    }
                }
                else {
                    pattern_matching_result = 1;
                }
                switch (pattern_matching_result) {
                    case 0: {
                        autoConnectCmd = Cmd_ofMsg(new Updates_Message(8, [selectedNode.Id, a.Id, node_1.Id, b.Id]));
                        break;
                    }
                    case 1: {
                        autoConnectCmd = Cmd_none();
                        break;
                    }
                }
            }
            else {
                autoConnectCmd = Cmd_none();
            }
            return [new Updates_Model(Updates_withNode(node_1, model.Graph), model.Selection), Cmd_batch(ofArray([autoConnectCmd, Cmd_ofMsg(new Updates_Message(5, node_1.Id))]))];
        }
        case 3: {
            const m = new Updates_Model(model.Graph, empty_2({
                Compare: comparePrimitives,
            }));
            return [m, singleton((dispatch) => {
                Updates_msgSelectionChange(m, options, dispatch);
            })];
        }
        case 4: {
            const id = msg.fields[0];
            const m_1 = new Updates_Model(model.Graph, FSharpSet__Add(model.Selection, id));
            return [m_1, singleton((dispatch_1) => {
                Updates_msgSelectionChange(m_1, options, dispatch_1);
            })];
        }
        case 5: {
            const id_1 = msg.fields[0];
            const m_2 = new Updates_Model(model.Graph, FSharpSet__Add(empty_2({
                Compare: comparePrimitives,
            }), id_1));
            return [m_2, singleton((dispatch_2) => {
                Updates_msgSelectionChange(m_2, options, dispatch_2);
            })];
        }
        case 1: {
            const y_4 = msg.fields[2];
            const x_4 = msg.fields[1];
            const id_2 = msg.fields[0];
            const g_3 = model.Graph;
            const n_1 = FSharpMap__get_Item(g_3.Nodes, id_2);
            const g$0027 = new Types_Graph(FSharpMap__Add(g_3.Nodes, id_2, new Types_Node(n_1.Id, n_1.Type, x_4, y_4, n_1.Width, n_1.Height, n_1.ZIndex, n_1.ClassName, n_1.SourceLocation, n_1.TargetLocation, n_1.CanSelect, n_1.CanMove)), g_3.Edges);
            return [new Updates_Model(g$0027, model.Selection), Cmd_none()];
        }
        default: {
            return [new Updates_Model(Updates_deleteNodes(model.Graph, model.Selection), model.Selection), Cmd_ofMsg(new Updates_Message(3))];
        }
    }
}

export function Edges_drawEdgeSvg(pos1, x1, y1, pos2, x2, y2, edgeStyle) {
    const isLR = (_arg) => {
        switch (_arg.tag) {
            case 0:
            case 1: {
                return true;
            }
            default: {
                return false;
            }
        }
    };
    const isTB = (arg) => (!isLR(arg));
    let pathData;
    switch (edgeStyle) {
        case "straight": {
            pathData = toText(printf("M %f,%fL %f,%f"))(x1)(y1)(x2)(y2);
            break;
        }
        case "bezier": {
            const patternInput = [x2 - x1, y2 - y1];
            const dy = patternInput[1];
            const dx = patternInput[0];
            const patternInput_1 = [x1 + (dx / 2), y1 + (dy / 2)];
            const my = patternInput_1[1];
            const mx = patternInput_1[0];
            let points;
            const matchValue = [pos1, pos2];
            let pattern_matching_result;
            if (matchValue[0].tag === 0) {
                if (matchValue[1].tag === 1) {
                    pattern_matching_result = 0;
                }
                else {
                    pattern_matching_result = 4;
                }
            }
            else if (matchValue[0].tag === 1) {
                if (matchValue[1].tag === 0) {
                    pattern_matching_result = 1;
                }
                else {
                    pattern_matching_result = 4;
                }
            }
            else if (matchValue[0].tag === 3) {
                if (matchValue[1].tag === 4) {
                    pattern_matching_result = 2;
                }
                else {
                    pattern_matching_result = 4;
                }
            }
            else if (matchValue[0].tag === 4) {
                if (matchValue[1].tag === 3) {
                    pattern_matching_result = 3;
                }
                else {
                    pattern_matching_result = 4;
                }
            }
            else {
                pattern_matching_result = 4;
            }
            switch (pattern_matching_result) {
                case 0: {
                    points = ofArray([[mx, y1], [mx, y2]]);
                    break;
                }
                case 1: {
                    points = ofArray([[mx, y1], [mx, y2]]);
                    break;
                }
                case 2: {
                    points = ofArray([[x1, my], [x2, my]]);
                    break;
                }
                case 3: {
                    points = ((y2 > (y1 + 40)) ? ofArray([[x1, my], [x2, my]]) : ofArray([[x1, y1 + 20], [(x1 + x2) / 2, y1 + 20], [(x1 + x2) / 2, y2 - 20], [x2, y2 - 20]]));
                    break;
                }
                case 4: {
                    points = ofArray([[x1, my], [x2, my]]);
                    break;
                }
            }
            const pstr = join(" ", map_2((tupledArg) => {
                const x = tupledArg[0];
                const y = tupledArg[1];
                return toText(printf("%f,%f"))(x)(y);
            }, points));
            pathData = toText(printf("M%f,%f L%s %f,%f"))(x1)(y1)(pstr)(x2)(y2);
            break;
        }
        default: {
            pathData = toText(printf("M %f,%fL %f,%f"))(x1)(y1)(x2)(y2);
        }
    }
    return SvgEngine$1__g_BB573A(SvgEngineHelpers_Svg, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "edge"), SvgEngine$1__path_BB573A(SvgEngineHelpers_Svg, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "edge-click"), SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "pointer-events", "auto")]), AttrEngine$1__d_Z721C83C5(EngineHelpers_Attr, pathData), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_1) => {
        log("click on path");
    })]), SvgEngine$1__path_BB573A(SvgEngineHelpers_Svg, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "edge-stroke animated"), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "marker-end", "none"), AttrEngine$1__d_Z721C83C5(EngineHelpers_Attr, pathData)])]);
}

export function EventHandlers_selectHandler(dispatch, node) {
    return singleton(EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e) => {
        const el = asElement(e.target);
        e.stopPropagation();
        Helpers_deselectAll(el.parentElement);
        Helpers_select(el);
    }));
}

export function EventHandlers_handleNodeMouseDown(e, options, dispatch, nodeEl, node) {
    const containerEl = Helpers_containerElFromNodeEl(nodeEl);
    if (node.CanSelect) {
        Helpers_deselectAll(containerEl);
        Helpers_select(nodeEl);
    }
    if (node.CanMove) {
        const patternInput = Helpers_parentXY(e);
        const sy = patternInput[1];
        const sx = patternInput[0];
        const snap = (tupledArg) => {
            const nx = tupledArg[0];
            const ny = tupledArg[1];
            if (options.SnapToGrid) {
                const gs = options.SnapToGridSize;
                return [gs * round(nx / gs), gs * round(ny / gs)];
            }
            else {
                return [nx, ny];
            }
        };
        const newXY = (tupledArg_1) => {
            const cx = tupledArg_1[0];
            const cy = tupledArg_1[1];
            return snap([(node.X + cx) - sx, (node.Y + cy) - sy]);
        };
        let stop;
        const clo = listen("mousemove", containerEl, (e_1) => {
            const patternInput_1 = newXY(Helpers_parentXY(e_1));
            const ny_1 = patternInput_1[1];
            const nx_1 = patternInput_1[0];
            (nodeEl.style).left = (`${nx_1}px`);
            (nodeEl.style).top = (`${ny_1}px`);
        });
        stop = (() => {
            clo();
        });
        once("mouseup", containerEl, (e_2) => {
            stop();
            e_2.stopPropagation();
            const patternInput_2 = newXY(Helpers_parentXY(e_2));
            const ny_2 = patternInput_2[1];
            const nx_2 = patternInput_2[0];
            dispatch(new Updates_Message(1, node.Id, nx_2, ny_2));
            if (node.CanSelect) {
                dispatch(new Updates_Message(5, node.Id));
            }
        });
    }
    else {
        dispatch(new Updates_Message(3));
        if (node.CanSelect) {
            dispatch(new Updates_Message(4, node.Id));
        }
    }
}

export function EventHandlers_handlePortMouseDown(e, dispatch, portEl) {
    const nodeEl = Helpers_nodeElFromPortEl(portEl);
    const containerEl = Helpers_containerElFromNodeEl(nodeEl);
    let patternInput;
    const tupledArg = Helpers_centreXY(portEl);
    patternInput = Helpers_toLocalXY(containerEl, tupledArg[0], tupledArg[1]);
    const sy = patternInput[1];
    const sx = patternInput[0];
    log((("sx, sy: " + sx.toString()) + ", ") + sy.toString());
    const mousexy = Store_make([sx, sy]);
    const dragLine = Bind_el_ZF0512D0(mousexy, (tupledArg_1) => {
        const x = tupledArg_1[0];
        const y = tupledArg_1[1];
        return Edges_drawEdgeSvg(new BasicLocation(4), sx, sy, new BasicLocation(3), x, y, "bezier");
    });
    let unmount;
    const tupledArg_2 = [document.getElementById("graph-edges-id"), dragLine];
    unmount = Program_mountAppend_Z427DD8DF(tupledArg_2[0], tupledArg_2[1]);
    let stop;
    const clo = listen("mousemove", containerEl, (e_1) => {
        let tupledArg_3;
        if (Helpers_targetEl(e_1).classList.contains("port")) {
            Store_set(mousexy, (tupledArg_3 = Helpers_centreXY(Helpers_targetEl(e_1)), Helpers_toLocalXY(containerEl, tupledArg_3[0], tupledArg_3[1])));
        }
        else {
            Store_set(mousexy, Helpers_clientXY(e_1));
        }
    });
    stop = (() => {
        clo();
    });
    once("mouseup", containerEl, (e_3) => {
        let tupledArg_4;
        const el_1 = Helpers_targetEl(e_3);
        if (el_1.classList.contains("port")) {
            const sourceNodeId = portEl.getAttribute("x-node-id");
            const sourcePortId = portEl.getAttribute("x-port-id");
            const targetNodeId = el_1.getAttribute("x-node-id");
            const targetPortId = el_1.getAttribute("x-port-id");
            Store_set(mousexy, (tupledArg_4 = Helpers_centreXY(el_1), Helpers_toLocalXY(containerEl, tupledArg_4[0], tupledArg_4[1])));
            dispatch(new Updates_Message(8, [sourceNodeId, sourcePortId, targetNodeId, targetPortId]));
        }
        else {
            log("no connection");
        }
        stop();
        disposeSafe(unmount);
    });
}

function EventHandlers_handleDrop(options, model, dispatch, e) {
    const patternInput = Helpers_clientXY(e);
    const y = patternInput[1];
    const x = patternInput[0];
    const nodeType = e.dataTransfer.getData("x/type");
    const nodeName = e.dataTransfer.getData("x/name");
    const patternInput_1 = Helpers_fromJsonString(e.dataTransfer.getData("x/offset"));
    const offsetY = patternInput_1[1];
    const offsetX = patternInput_1[0];
    const nodeX = x - offsetX;
    const nodeY = y - offsetY;
    if (nodeType !== "") {
        dispatch(new Updates_Message(7, [nodeType, nodeX, nodeY]));
    }
    if (nodeName !== "") {
        dispatch(new Updates_Message(6, [nodeName, nodeX, nodeY]));
        dispatch(new Updates_Message(5, nodeName));
    }
    e.preventDefault();
}

export function EventHandlers_containerEventHandlers(options, model, dispatch) {
    return ofArray([AttrEngine$1__tabIndex_Z524259A4(EngineHelpers_Attr, 0), EventEngine$1__onKeyDown_Z2153A397(EngineHelpers_Ev, (e) => {
        if (e.key === "Backspace") {
            dispatch(new Updates_Message(2));
            e.preventDefault();
            e.stopPropagation();
        }
    }), EventEngine$1__onMouseDown_58BC8925(EngineHelpers_Ev, (e_1) => {
        const matchValue = Helpers_findPortFromEvent(e_1, Store_get(model).Graph);
        if (matchValue == null) {
            const matchValue_1 = Helpers_findNodeFromEvent(e_1, Store_get(model).Graph);
            if (matchValue_1 != null) {
                const nodeEl = matchValue_1[0];
                const node = matchValue_1[1];
                EventHandlers_handleNodeMouseDown(e_1, options, dispatch, nodeEl, node);
            }
            else {
                dispatch(new Updates_Message(3));
            }
        }
        else {
            const portEl = matchValue;
            EventHandlers_handlePortMouseDown(e_1, dispatch, portEl);
        }
    }), EventEngine$1__onDrop_Z3384A56C(EngineHelpers_Ev, (e_2) => {
        EventHandlers_handleDrop(options, model, dispatch, e_2);
    })]);
}

export const Styles_styleDefault = toList(delay(() => append(singleton_1(rule(".graph-container", ofArray([CssEngine$1__get_positionRelative(EngineHelpers_Css), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__get_cursorGrab(EngineHelpers_Css)]))), delay(() => append(singleton_1(rule(".node", ofArray([CssEngine$1__get_positionAbsolute(EngineHelpers_Css), CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__get_alignItemsCenter(EngineHelpers_Css), CssEngine$1__get_justifyContentCenter(EngineHelpers_Css), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, int32ToString(1) + "rem"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, "auto"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, "auto"), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "white"), CssEngine$1__get_cursorGrab(EngineHelpers_Css)]))), delay(() => append(singleton_1(rule(".node:after", ofArray([CssEngine$1__get_positionAbsolute(EngineHelpers_Css), CssEngine$1__get_displayBlock(EngineHelpers_Css), CssEngine$1__top_Z524259A4(EngineHelpers_Css, 0), CssEngine$1__left_Z524259A4(EngineHelpers_Css, 0), CssEngine$1__right_Z524259A4(EngineHelpers_Css, 0), CssEngine$1__bottom_Z524259A4(EngineHelpers_Css, 0), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "content", "\u0027\u0027"), CssEngine$1__border_Z6C024E7B(EngineHelpers_Css, int32ToString(1) + "px", "solid", "#888888"), CssEngine$1__borderRadius_Z445F6BAF(EngineHelpers_Css, int32ToString(4) + "px")]))), delay(() => append(singleton_1(rule(".node.selected:after", singleton(CssEngine$1__borderWidth_18A029B5(EngineHelpers_Css, int32ToString(2) + "px")))), delay(() => {
    const portSize = 9;
    return append(singleton_1(rule(".port-group", ofArray([CssEngine$1__get_positionAbsolute(EngineHelpers_Css), CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "justify-content", "space-evenly")]))), delay(() => {
        let value_6;
        return append(singleton_1(rule(".port-group.top", ofArray([CssEngine$1__get_flexDirectionRow(EngineHelpers_Css), CssEngine$1__top_Z445F6BAF(EngineHelpers_Css, (value_6 = (-((portSize / 2) - 0.5)), value_6.toString() + "px")), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")]))), delay(() => {
            let value_8;
            return append(singleton_1(rule(".port-group.bottom", ofArray([CssEngine$1__get_flexDirectionRow(EngineHelpers_Css), CssEngine$1__bottom_Z445F6BAF(EngineHelpers_Css, (value_8 = (-((portSize / 2) - 0.5)), value_8.toString() + "px")), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")]))), delay(() => {
                let value_10;
                return append(singleton_1(rule(".port-group.left", ofArray([CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__left_Z445F6BAF(EngineHelpers_Css, (value_10 = (-((portSize / 2) - 0.5)), value_10.toString() + "px")), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")]))), delay(() => {
                    let value_12;
                    return append(singleton_1(rule(".port-group.right", ofArray([CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__right_Z445F6BAF(EngineHelpers_Css, (value_12 = (-((portSize / 2) - 0.5)), value_12.toString() + "px")), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")]))), delay(() => append(singleton_1(rule(".port", ofArray([CssEngine$1__get_cursorCrosshair(EngineHelpers_Css), CssEngine$1__get_displayBlock(EngineHelpers_Css), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, portSize.toString() + "px"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, portSize.toString() + "px"), CssEngine$1__border_Z6C024E7B(EngineHelpers_Css, int32ToString(1) + "px", "solid", "white"), CssEngine$1__borderRadius_Z445F6BAF(EngineHelpers_Css, int32ToString(50) + "%"), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "#888888")]))), delay(() => append(singleton_1(rule(".port:hover", singleton(CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "black")))), delay(() => append(singleton_1(rule(".edge", ofArray([CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke", "#999"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke-width", "1"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "fill", "none")]))), delay(() => append(singleton_1(rule(".edge-click", ofArray([CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke", "transparent"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke-width", "5"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "fill", "none")]))), delay(() => append(singleton_1(keyframes("dashdraw", singleton(keyframe(0, singleton(CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke-dashoffset", "10")))))), delay(() => append(singleton_1(rule(".edge-stroke", ofArray([CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke", "#999"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke-width", "1"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "fill", "none")]))), delay(() => append(singleton_1(rule(".edge:hover .edge-stroke", ofArray([CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke", "orange"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke-width", "2")]))), delay(() => append(singleton_1(rule("path.animated", ofArray([CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "stroke-dasharray", "5"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "animation", "dashdraw .5s linear infinite")]))), delay(() => singleton_1(rule(".graph-edges", ofArray([CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "pointer-events", "none"), CssEngine$1__get_positionAbsolute(EngineHelpers_Css), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "z-index", "4"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")])))))))))))))))))))));
                }));
            }));
        }));
    }));
}))))))))));

export function Views_background(bg) {
    if (bg.tag === 1) {
        return SvgEngine$1__svg_BB573A(SvgEngineHelpers_Svg, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__get_positionAbsolute(EngineHelpers_Css)]), SvgEngine$1__pattern_BB573A(SvgEngineHelpers_Svg, [AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, "pattern-bg"), SutilSvgEngine__get_x(SvgEngineHelpers_Svg)(0), SutilSvgEngine__get_y(SvgEngineHelpers_Svg)(0), SutilSvgEngine__get_width(SvgEngineHelpers_Svg)(15), SutilSvgEngine__get_height(SvgEngineHelpers_Svg)(15), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "patternUnits", "userSpaceOnUse"), SvgEngine$1__circle_BB573A(SvgEngineHelpers_Svg, [AttrEngine$1__cx_Z445F6BAF(EngineHelpers_Attr, (0.4).toString() + "px"), AttrEngine$1__cy_Z445F6BAF(EngineHelpers_Attr, (0.4).toString() + "px"), AttrEngine$1__r_Z445F6BAF(EngineHelpers_Attr, (0.4).toString() + "px"), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "fill", "#81818a")])]), SvgEngine$1__rect_BB573A(SvgEngineHelpers_Svg, [SutilSvgEngine__get_x(SvgEngineHelpers_Svg)(0), SutilSvgEngine__get_y(SvgEngineHelpers_Svg)(0), SutilSvgEngine__get_width(SvgEngineHelpers_Svg)(int32ToString(100) + "%"), SutilSvgEngine__get_height(SvgEngineHelpers_Svg)(int32ToString(100) + "%"), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "fill", "url(#pattern-bg)")])]);
    }
    else {
        return fragment([]);
    }
}

export function Views_container(elements) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton_1(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "graph-container")), delay(() => append(singleton_1(Views_background(new Types_Background(1))), delay(() => elements)))))));
}

export function Views_renderPort(portxys, node, port) {
    const loc = equals(port.Mode, new Types_PortMode(0)) ? node.TargetLocation : node.SourceLocation;
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__zIndex_Z524259A4(EngineHelpers_Css, node.ZIndex + 1)]), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "x-node-id", node.Id), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "x-port-id", port.Id), AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "x-port-loc", BasicLocation__get_LowerName(loc)), AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, `port ${toString(port.Mode).toLocaleLowerCase()} ${BasicLocation__get_LowerName(loc)}`), SutilEventEngine__onMount_7DDE0344(EngineHelpers_Ev, (e) => {
        const portEl = Helpers_targetEl(e);
        const nodeEl = Helpers_nodeElFromPortEl(portEl);
        const containerEl = Helpers_containerElFromNodeEl(nodeEl);
        const key = toText(printf("%s-%s"))(node.Id)(port.Id);
        Updates_PortXYs__Monitor_Z164694C9(portxys, portEl, () => {
            let xy;
            const tupledArg = Helpers_centreXY(portEl);
            xy = Helpers_toLocalXY(containerEl, tupledArg[0], tupledArg[1]);
            Updates_PortXYs__Update_Z188742C(portxys, key, xy);
        });
    })]);
}

export function Views_renderPorts(options, portxys, node) {
    let arg_1, arg_2, arg_4, arg_5;
    return ofArray([(arg_1 = toList(delay(() => {
        let array;
        return map_3((port) => Views_renderPort(portxys, node, port), (array = options.NodePorts(node.Type), array.filter((p) => equals(p.Mode, new Types_PortMode(0)))));
    })), SutilHtmlEngine__divc(EngineHelpers_Html, (arg_2 = BasicLocation__get_LowerName(node.TargetLocation), toText(printf("port-group input %s"))(arg_2)), arg_1)), (arg_4 = toList(delay(() => {
        let array_2;
        return map_3((port_1) => Views_renderPort(portxys, node, port_1), (array_2 = options.NodePorts(node.Type), array_2.filter((p_1) => equals(p_1.Mode, new Types_PortMode(1)))));
    })), SutilHtmlEngine__divc(EngineHelpers_Html, (arg_5 = BasicLocation__get_LowerName(node.SourceLocation), toText(printf("port-group output %s"))(arg_5)), arg_4))]);
}

export function Views_injectNodeDefaults(model, options, portxys, node, view) {
    return inject(toList(delay(() => append(singleton_1(AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "x-node-id", node.Id)), delay(() => append(singleton_1(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, join(" ", toList(delay(() => append(singleton_1("node"), delay(() => append((node.ClassName !== "") ? singleton_1(node.ClassName) : empty_3(), delay(() => (FSharpSet__Contains(model.Selection, node.Id) ? singleton_1("selected") : empty_3())))))))))), delay(() => append(singleton_1(SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__left_Z445F6BAF(EngineHelpers_Css, node.X.toString() + "px"), CssEngine$1__top_Z445F6BAF(EngineHelpers_Css, node.Y.toString() + "px"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, node.Width.toString() + "px"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, node.Height.toString() + "px")])), delay(() => Views_renderPorts(options, portxys, node))))))))), view);
}

export function Views_renderNode(model, options, portxys, node) {
    return Views_injectNodeDefaults(model, options, portxys, node, options.ViewNode(node));
}

export function Views_findPortXY(model, options, np) {
    const node = FSharpMap__get_Item(model.Graph.Nodes, np.NodeId);
    const ports = options.NodePorts(node.Type);
    const port = ports.find((p) => (p.Id === np.PortId));
    const loc = equals(port.Mode, new Types_PortMode(0)) ? node.TargetLocation : node.SourceLocation;
    switch (loc.tag) {
        case 1: {
            return [node.X + node.Width, node.Y + (node.Height / 2)];
        }
        case 3: {
            return [node.X + (node.Width / 2), node.Y];
        }
        case 4: {
            return [node.X + (node.Width / 2), node.Y + node.Height];
        }
        case 2: {
            return [node.X + (node.Width / 2), node.Y + (node.Height / 2)];
        }
        default: {
            return [node.X, node.Y + (node.Height / 2)];
        }
    }
}

export function Views_renderEdge(model, options, portxys, edge) {
    const patternInput = Views_findPortXY(model, options, edge.Source);
    const y1 = patternInput[1];
    const x1 = patternInput[0];
    const patternInput_1 = Views_findPortXY(model, options, edge.Target);
    const y2 = patternInput_1[1];
    const x2 = patternInput_1[0];
    const key1 = toText(printf("%s-%s"))(edge.Source.NodeId)(edge.Source.PortId);
    const key2 = toText(printf("%s-%s"))(edge.Target.NodeId)(edge.Target.PortId);
    return Bind_el_ZF0512D0(Updates_PortXYs__GetStore_Z384F8060(portxys, key1, key2), (tupledArg) => {
        const _arg = tupledArg[0];
        const _arg_1 = tupledArg[1];
        const y1_1 = _arg[1];
        const x1_1 = _arg[0];
        const y2_1 = _arg_1[1];
        const x2_1 = _arg_1[0];
        return Edges_drawEdgeSvg(new BasicLocation(4), x1_1, y1_1, new BasicLocation(3), x2_1, y2_1, "bezier");
    });
}

export function Views_renderGraph(graph, options) {
    const patternInput = Store_makeElmish(Updates_init, (msg, model) => Updates_update(options, msg, model), (value) => {
    })(graph);
    const model_1 = patternInput[0];
    const dispatch = patternInput[1];
    const portXYs = Updates_PortXYs_$ctor();
    return withStyle(options.Css, Views_container(toList(delay(() => append(singleton_1(disposeOnUnmount(singleton(subscribe(options.OnChange, StoreOperators_op_DotGreaterGreater(model_1, (m) => m.Graph))))), delay(() => append(EventHandlers_containerEventHandlers(options, model_1, dispatch), delay(() => append(singleton_1(Bind_el_ZF0512D0(model_1, (m_1) => {
        Updates_PortXYs__Clear(portXYs);
        return fragment(map_1((node) => Views_renderNode(m_1, options, portXYs, node), FSharpMap__get_Values(m_1.Graph.Nodes)));
    })), delay(() => singleton_1(SvgEngine$1__svg_BB573A(SvgEngineHelpers_Svg, [AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, "graph-edges-id"), AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "graph-edges"), Bind_el_ZF0512D0(model_1, (m_2) => SvgEngine$1__g_BB573A(SvgEngineHelpers_Svg, map_1((edge) => Views_renderEdge(m_2, options, portXYs, edge), FSharpMap__get_Values(m_2.Graph.Edges))))]))))))))))));
}

export function Views_makeCatalogItem(nodeType, el) {
    return inject([AttrEngine$1__custom_Z384F8060(EngineHelpers_Attr, "x-node-type", nodeType), AttrEngine$1__draggable_Z1FBCCD16(EngineHelpers_Attr, true), EventEngine$1__onDragStart_Z3384A56C(EngineHelpers_Ev, (e) => {
        e.dataTransfer.setData("x/offset", Helpers_toJsonString(Helpers_clientXY(e)));
        e.dataTransfer.setData("x/type", nodeType);
    })], el);
}

export class FlowChart {
    constructor(options) {
        this.options = options;
        addGlobalStyleSheet(document, Styles_styleDefault);
    }
}

export function FlowChart$reflection() {
    return class_type("SutilOxide.Flow.FlowChart", void 0, FlowChart);
}

export function FlowChart_$ctor_Z3846D57A(options) {
    return new FlowChart(options);
}

export function FlowChart__Render_3573D436(__, graph) {
    return Views_renderGraph(graph, __.options);
}

