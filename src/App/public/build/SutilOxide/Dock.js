import { Union, Record } from "../fable_modules/fable-library.3.7.20/Types.js";
import { union_type, lambda_type, unit_type, bool_type, class_type, record_type, option_type, tuple_type, int32_type, string_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { DockLocation__get_Secondary, BasicLocation, DockCollection_get_Empty, DockLocation__get_Hand, BasicLocation__get_LowerName, DockLocation__get_Primary, DockLocation__get_CssName, TabHalf, DockPane, DockLocation, DockLocation_get_All, DockCollection__GetPanes_217D4758, DockCollection, DockStation, DockCollection$reflection, DockLocation$reflection } from "./Types.js";
import { tail, isEmpty, iterate, minBy, tryPick as tryPick_1, fold, tryHead, map as map_2, find, tryFind, tryFindIndex, insertAt, singleton, append, length, filter, exists } from "../fable_modules/fable-library.3.7.20/List.js";
import { empty, ofList, FSharpMap__Add, FSharpMap__get_Item, tryPick } from "../fable_modules/fable-library.3.7.20/Map.js";
import { some, defaultArg, orElseWith, bind, map as map_1 } from "../fable_modules/fable-library.3.7.20/Option.js";
import { Cmd_ofMsg, Cmd_batch, Cmd_none } from "../Sutil/src/Sutil/Cmd.js";
import { menuItem, dropDownItem, ButtonProperty, buttonItem, buttonGroup, UI_divc, MenuMonitor_monitorAll } from "./Toolbar.js";
import { iterate as iterate_1, empty as empty_1, append as append_1, singleton as singleton_1, delay, toList } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { resizeControllerEw, resizeControllerNs, resizeControllerEwFlex, resizeControllerNsFlex, targetEl, whichHalfX, containsByWidth, whichHalfY, containsByHeight, clearPreview, toEl, toListFromNodeList, getContentParentNode, getWrapperNode } from "./DomHelpers.js";
import { Store_makeElmish, Store_map } from "../Sutil/src/Sutil/Store.js";
import { equalArrays, uncurry, comparePrimitives, equals } from "../fable_modules/fable-library.3.7.20/Util.js";
import { Bind_el_ZF0512D0, Bind_visibility_40BD454A, Bind_toggleClass_Z2A796D4F } from "../Sutil/src/Sutil/Bind.js";
import { HtmlEngine$1__div_BB573A, HtmlEngine$1__span_BB573A, HtmlEngine$1__i_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { AttrEngine$1__id_Z721C83C5, AttrEngine$1__draggable_Z1FBCCD16, AttrEngine$1__className_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { EngineHelpers_Css, SutilAttrEngine__style_68BDC580, EngineHelpers_Ev, EngineHelpers_text, EngineHelpers_Html, EngineHelpers_Attr } from "../Sutil/src/Sutil/Html.js";
import { EventEngine$1__onDrop_Z3384A56C, EventEngine$1__onDragOver_Z3384A56C, EventEngine$1__onClick_58BC8925, EventEngine$1__onDragEnd_Z3384A56C, EventEngine$1__onDragStart_Z3384A56C } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { distinctUntilChanged } from "../Sutil/src/Sutil/Observable.js";
import { Program_mountAppend_Z427DD8DF } from "../Sutil/src/Sutil/Program.js";
import { EventModifier, onMount, fragment } from "../Sutil/src/Sutil/CoreElements.js";
import { CssEngine$1__get_overflowHidden } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";

export class DraggingTab extends Record {
    constructor(BeingDragged, Preview) {
        super();
        this.BeingDragged = BeingDragged;
        this.Preview = Preview;
    }
}

export function DraggingTab$reflection() {
    return record_type("SutilOxide.Dock.DraggingTab", [], DraggingTab, () => [["BeingDragged", string_type], ["Preview", option_type(tuple_type(DockLocation$reflection(), int32_type))]]);
}

export class Model extends Record {
    constructor(RefreshId, Docks, DraggingTab, SelectedPanes) {
        super();
        this.RefreshId = (RefreshId | 0);
        this.Docks = Docks;
        this.DraggingTab = DraggingTab;
        this.SelectedPanes = SelectedPanes;
    }
}

export function Model$reflection() {
    return record_type("SutilOxide.Dock.Model", [], Model, () => [["RefreshId", int32_type], ["Docks", DockCollection$reflection()], ["DraggingTab", option_type(DraggingTab$reflection())], ["SelectedPanes", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [DockLocation$reflection(), option_type(string_type)])]]);
}

export class Options extends Record {
    constructor(OnTabShow) {
        super();
        this.OnTabShow = OnTabShow;
    }
}

export function Options$reflection() {
    return record_type("SutilOxide.Dock.Options", [], Options, () => [["OnTabShow", lambda_type(tuple_type(string_type, bool_type), unit_type)]]);
}

export function Options_Create() {
    return new Options((value) => {
    });
}

export function DockHelpers_tabsContains(name, tabLabels) {
    return exists((t) => (t.Name === name), tabLabels);
}

export function DockHelpers_removeFromPanesList(panes, name) {
    return filter((t) => (t.Name !== name), panes);
}

export function DockHelpers_insertIntoPanes(panes, pane, i) {
    if (i >= length(panes)) {
        return append(panes, singleton(pane));
    }
    else {
        return insertAt(i, pane, panes);
    }
}

export function DockHelpers_findPaneLocationIndex(docks, name) {
    return tryPick((loc, station) => map_1((i) => [loc, i], tryFindIndex((t) => (t.Name === name), station.Panes)), docks.Stations);
}

export function DockHelpers_findPaneLocation(docks, name) {
    return map_1((tuple) => tuple[0], DockHelpers_findPaneLocationIndex(docks, name));
}

export function DockHelpers_getPanes(docks, loc) {
    return FSharpMap__get_Item(docks.Stations, loc).Panes;
}

export function DockHelpers_setPanes(docks, loc, value) {
    const dock = FSharpMap__get_Item(docks.Stations, loc);
    return new DockCollection(FSharpMap__Add(docks.Stations, loc, new DockStation(value)));
}

export function DockHelpers_tryGetPane(docks, name) {
    return bind((loc) => tryFind((t) => (t.Name === name), DockHelpers_getPanes(docks, loc)), DockHelpers_findPaneLocation(docks, name));
}

export function DockHelpers_getPane(docks, name) {
    const matchValue = DockHelpers_findPaneLocation(docks, name);
    if (matchValue != null) {
        const loc = matchValue;
        return find((t) => (t.Name === name), DockHelpers_getPanes(docks, loc));
    }
    else {
        throw (new Error("Not found"));
    }
}

export function DockHelpers_removeFromPanes(docks, name) {
    const matchValue = DockHelpers_findPaneLocation(docks, name);
    if (matchValue != null) {
        const cloc = matchValue;
        const tabLabels = DockHelpers_removeFromPanesList(DockHelpers_getPanes(docks, cloc), name);
        return DockHelpers_setPanes(docks, cloc, tabLabels);
    }
    else {
        throw (new Error("Not found"));
    }
}

export function DockHelpers_moveTab(model, name, loc, index) {
    const matchValue = DockHelpers_findPaneLocation(model.Docks, name);
    if (matchValue != null) {
        const currentLoc = matchValue;
        const pane = DockHelpers_getPane(model.Docks, name);
        const docks$0027 = DockHelpers_removeFromPanes(model.Docks, name);
        const panes = DockHelpers_getPanes(docks$0027, loc);
        const panes$0027 = DockHelpers_insertIntoPanes(panes, pane, index);
        return new Model(model.RefreshId, DockHelpers_setPanes(docks$0027, loc, panes$0027), model.DraggingTab, FSharpMap__Add(FSharpMap__Add(model.SelectedPanes, currentLoc, void 0), loc, name));
    }
    else {
        return model;
    }
}

export function DockHelpers_ensurePaneSelected(m) {
    return new Model(m.RefreshId, m.Docks, m.DraggingTab, ofList(map_2((loc) => {
        const selectedPaneName = orElseWith(FSharpMap__get_Item(m.SelectedPanes, loc), () => map_1((p) => p.Name, tryHead(DockCollection__GetPanes_217D4758(m.Docks, loc))));
        return [loc, selectedPaneName];
    }, DockLocation_get_All())));
}

export function DockHelpers_ensureCentreSelected(m) {
    if (FSharpMap__get_Item(m.SelectedPanes, new DockLocation(3)) == null) {
        return new Model(m.RefreshId, m.Docks, m.DraggingTab, FSharpMap__Add(m.SelectedPanes, new DockLocation(3), map_1((p) => p.Name, tryHead(DockCollection__GetPanes_217D4758(m.Docks, new DockLocation(3))))));
    }
    else {
        return m;
    }
}

export function DockHelpers_minimizePane(model, pane) {
    return defaultArg(map_1((loc) => (new Model(model.RefreshId, model.Docks, model.DraggingTab, FSharpMap__Add(model.SelectedPanes, loc, void 0))), DockHelpers_findPaneLocation(model.Docks, pane)), model);
}

export class DockProperty extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Visible", "Location"];
    }
}

export function DockProperty$reflection() {
    return union_type("SutilOxide.Dock.DockProperty", [], DockProperty, () => [[["Item", bool_type]], [["Item", DockLocation$reflection()]]]);
}

class Message extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["RemoveTab", "AddTab", "SetDragging", "CancelDrag", "PreviewDockLocation", "CommitDrag", "SelectPane", "TogglePane", "MinimizePane", "ShowPane", "MoveTo", "DockProp"];
    }
}

function Message$reflection() {
    return union_type("SutilOxide.Dock.Message", [], Message, () => [[["Item", string_type]], [["Item", tuple_type(string_type, string_type, DockLocation$reflection(), bool_type)]], [["Item", string_type]], [], [["Item", option_type(tuple_type(DockLocation$reflection(), int32_type))]], [], [["Item1", DockLocation$reflection()], ["Item2", option_type(string_type)]], [["Item1", DockLocation$reflection()], ["Item2", string_type]], [["Item", string_type]], [["Item", string_type]], [["Item1", string_type], ["Item2", DockLocation$reflection()]], [["Item", tuple_type(string_type, DockProperty$reflection())]]]);
}

function init(docks) {
    let list;
    return [DockHelpers_ensurePaneSelected(new Model(0, docks, void 0, (list = DockLocation_get_All(), fold((s, loc) => FSharpMap__Add(s, loc, void 0), empty(), list)))), Cmd_none()];
}

const cmdMonitorAll = singleton((d) => {
    MenuMonitor_monitorAll();
});

function update(options, msg, model) {
    let name_3;
    switch (msg.tag) {
        case 0: {
            const name_1 = msg.fields[0];
            const docks = DockHelpers_removeFromPanes(DockHelpers_minimizePane(model, name_1).Docks, name_1);
            return [new Model(model.RefreshId, docks, model.DraggingTab, model.SelectedPanes), cmdMonitorAll];
        }
        case 1: {
            const show = msg.fields[0][3];
            const name_2 = msg.fields[0][0];
            const location = msg.fields[0][2];
            const icon = msg.fields[0][1];
            const station = FSharpMap__get_Item(model.Docks.Stations, location);
            const panes = append(station.Panes, singleton(new DockPane(name_2)));
            const station$0027 = new DockStation(panes);
            return [new Model(model.RefreshId, new DockCollection(FSharpMap__Add(model.Docks.Stations, location, station$0027)), model.DraggingTab, model.SelectedPanes), Cmd_batch(toList(delay(() => (show ? singleton_1(Cmd_ofMsg(new Message(9, name_2))) : append_1(singleton_1(Cmd_none()), delay(() => singleton_1(cmdMonitorAll)))))))];
        }
        case 6: {
            const pane = msg.fields[1];
            const loc = msg.fields[0];
            return [new Model(model.RefreshId, model.Docks, model.DraggingTab, FSharpMap__Add(model.SelectedPanes, loc, pane)), Cmd_none()];
        }
        case 7: {
            const pane_1 = msg.fields[1];
            const loc_1 = msg.fields[0];
            let patternInput;
            const matchValue = FSharpMap__get_Item(model.SelectedPanes, loc_1);
            let pattern_matching_result;
            if (matchValue != null) {
                if ((name_3 = matchValue, name_3 === pane_1)) {
                    pattern_matching_result = 0;
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
                    patternInput = [void 0, false];
                    break;
                }
                case 1: {
                    patternInput = [pane_1, true];
                    break;
                }
            }
            const show_1 = patternInput[1];
            const selected = patternInput[0];
            options.OnTabShow([pane_1, show_1]);
            return [DockHelpers_ensureCentreSelected(new Model(model.RefreshId, model.Docks, model.DraggingTab, FSharpMap__Add(model.SelectedPanes, loc_1, selected))), cmdMonitorAll];
        }
        case 9: {
            const pane_2 = msg.fields[0];
            const m_1 = defaultArg(map_1((loc_2) => (new Model(model.RefreshId, model.Docks, model.DraggingTab, FSharpMap__Add(model.SelectedPanes, loc_2, pane_2))), DockHelpers_findPaneLocation(model.Docks, pane_2)), model);
            options.OnTabShow([pane_2, true]);
            return [m_1, cmdMonitorAll];
        }
        case 8: {
            const pane_3 = msg.fields[0];
            return [DockHelpers_minimizePane(model, pane_3), cmdMonitorAll];
        }
        case 2: {
            const d = msg.fields[0];
            return [new Model(model.RefreshId, model.Docks, new DraggingTab(d, void 0), model.SelectedPanes), Cmd_none()];
        }
        case 10: {
            const pane_4 = msg.fields[0];
            const loc_3 = msg.fields[1];
            const m_2 = DockHelpers_moveTab(model, pane_4, loc_3, 999);
            const wrapper = getWrapperNode(pane_4);
            const parent = getContentParentNode(loc_3);
            parent.appendChild(wrapper);
            return [DockHelpers_ensureCentreSelected(new Model(m_2.RefreshId, m_2.Docks, void 0, FSharpMap__Add(m_2.SelectedPanes, loc_3, pane_4))), cmdMonitorAll];
        }
        case 5: {
            const m_5 = bind((dt) => map_1((tupledArg) => {
                const loc_4 = tupledArg[0];
                const i = tupledArg[1] | 0;
                const m_4 = DockHelpers_moveTab(model, dt.BeingDragged, loc_4, i);
                const wrapper_1 = getWrapperNode(dt.BeingDragged);
                const parent_1 = getContentParentNode(loc_4);
                parent_1.appendChild(wrapper_1);
                return new Model(m_4.RefreshId, m_4.Docks, void 0, FSharpMap__Add(m_4.SelectedPanes, loc_4, dt.BeingDragged));
            }, dt.Preview), model.DraggingTab);
            return [DockHelpers_ensureCentreSelected(defaultArg(m_5, model)), cmdMonitorAll];
        }
        case 3: {
            return [new Model(model.RefreshId, model.Docks, void 0, model.SelectedPanes), Cmd_none()];
        }
        case 4: {
            const dockLoc = msg.fields[0];
            let m_7;
            const matchValue_1 = model.DraggingTab;
            if (matchValue_1 != null) {
                const d_1 = matchValue_1;
                m_7 = (new Model(model.RefreshId, model.Docks, new DraggingTab(d_1.BeingDragged, dockLoc), model.SelectedPanes));
            }
            else {
                m_7 = model;
            }
            return [m_7, Cmd_none()];
        }
        default: {
            const p = msg.fields[0][1];
            const name = msg.fields[0][0];
            if (p.tag === 1) {
                const l = p.fields[0];
                return [model, Cmd_ofMsg(new Message(10, name, l))];
            }
            else {
                const z = p.fields[0];
                return [model, Cmd_ofMsg(z ? (new Message(9, name)) : (new Message(8, name)))];
            }
        }
    }
}

export function ModelHelpers_beingDragged(m) {
    return map_1((p) => p.BeingDragged, m.DraggingTab);
}

export function ModelHelpers_childTabIsDragging(model, tabs) {
    return Store_map((m) => {
        const matchValue = ModelHelpers_beingDragged(m);
        if (matchValue != null) {
            const name = matchValue;
            return exists((t) => (t.Name === name), tabs(m));
        }
        else {
            return false;
        }
    }, model);
}

export function ModelHelpers_showOverlay(model, target) {
    return Store_map((m) => {
        const matchValue = bind((p) => p.Preview, m.DraggingTab);
        if (matchValue != null) {
            const loc = matchValue[0];
            return equals(loc, target);
        }
        else {
            return false;
        }
    }, model);
}

function EventHandlers_previewOver(dragEl, query, clientXY, contains, whichHalf) {
    const tabs = toListFromNodeList(document.querySelectorAll(query));
    const over = tryPick_1((tupledArg) => {
        const el = tupledArg[0];
        const i = tupledArg[1] | 0;
        if (contains(clientXY, el)) {
            return [el, i];
        }
        else {
            return void 0;
        }
    }, tabs);
    return defaultArg(map_1((tupledArg_1) => {
        const el_1 = tupledArg_1[0];
        const i_1 = tupledArg_1[1] | 0;
        const matchValue = whichHalf(clientXY, toEl(el_1));
        if (matchValue.tag === 1) {
            el_1.parentElement.insertBefore(dragEl, el_1.nextSibling);
            return i_1 | 0;
        }
        else {
            el_1.parentElement.insertBefore(dragEl, el_1);
            return i_1 | 0;
        }
    }, over), -1) | 0;
}

function EventHandlers_closestDock(cx, cy, r) {
    const patternInput = [cx - r.left, cy - r.top];
    const y = patternInput[1];
    const x = patternInput[0];
    const distanceTo = (loc) => {
        let pattern_matching_result;
        if (loc.tag === 0) {
            if (y < (r.height / 2)) {
                pattern_matching_result = 0;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 1) {
            if (y >= (r.height / 2)) {
                pattern_matching_result = 1;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 5) {
            if (y < (r.height / 2)) {
                pattern_matching_result = 2;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 6) {
            if (y >= (r.height / 2)) {
                pattern_matching_result = 3;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 2) {
            if (x < (r.width / 2)) {
                pattern_matching_result = 4;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 4) {
            if (x > (r.width / 2)) {
                pattern_matching_result = 5;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 7) {
            if (x < (r.width / 2)) {
                pattern_matching_result = 6;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else if (loc.tag === 8) {
            if (x > (r.width / 2)) {
                pattern_matching_result = 7;
            }
            else {
                pattern_matching_result = 8;
            }
        }
        else {
            pattern_matching_result = 8;
        }
        switch (pattern_matching_result) {
            case 0: {
                return x;
            }
            case 1: {
                return x;
            }
            case 2: {
                return r.width - x;
            }
            case 3: {
                return r.width - x;
            }
            case 4: {
                return r.height - y;
            }
            case 5: {
                return r.height - y;
            }
            case 6: {
                return y;
            }
            case 7: {
                return y;
            }
            case 8: {
                return 1.7976931348623157E+308;
            }
        }
    };
    const patternInput_1 = minBy((tuple) => tuple[1], map_2((loc_1) => [loc_1, distanceTo(loc_1)], DockLocation_get_All()), {
        Compare: comparePrimitives,
    });
    const loc_2 = patternInput_1[0];
    const dist = patternInput_1[1];
    if (Math.abs(dist) < 200) {
        return loc_2;
    }
    else {
        return void 0;
    }
}

function EventHandlers_dragOver(dispatch, e) {
    try {
        e.preventDefault();
        const el = toEl(e.currentTarget);
        const r = el.getBoundingClientRect();
        clearPreview();
        const dragEl = toEl(document.querySelector(".dragging"));
        const invert = (_arg) => {
            if (_arg.tag === 0) {
                return new TabHalf(1);
            }
            else {
                return new TabHalf(0);
            }
        };
        const previewOverLoc = (loc) => {
            const query = `.tabs-${DockLocation__get_CssName(loc)} > div`;
            return (clientXY) => ((contains) => ((whichHalf) => EventHandlers_previewOver(dragEl, query, clientXY, uncurry(2, contains), uncurry(2, whichHalf))));
        };
        const matchValue = EventHandlers_closestDock(e.clientX, e.clientY, r);
        if (matchValue != null) {
            const loc_1 = matchValue;
            let i;
            const matchValue_1 = DockLocation__get_Primary(loc_1);
            switch (matchValue_1.tag) {
                case 1: {
                    i = previewOverLoc(loc_1)(e.clientY)((clientY_1) => ((el_2) => containsByHeight(clientY_1, el_2)))((clientY_2) => ((el_3) => whichHalfY(clientY_2, el_3)));
                    break;
                }
                case 4:
                case 3: {
                    i = previewOverLoc(loc_1)(e.clientX)((clientX) => ((el_4) => containsByWidth(clientX, el_4)))((clientX_1) => ((el_5) => whichHalfX(clientX_1, el_5)));
                    break;
                }
                case 2: {
                    i = -1;
                    break;
                }
                default: {
                    i = previewOverLoc(loc_1)(e.clientY)((clientY) => ((el_1) => containsByHeight(clientY, el_1)))((a) => ((b) => invert(whichHalfY(a, b))));
                }
            }
            if (i !== -1) {
                dispatch(new Message(4, [loc_1, i]));
            }
        }
        else {
            dispatch(new Message(4, void 0));
        }
    }
    catch (x) {
        console.log(some(x.message));
    }
}

function EventHandlers_drop(dispatch, e) {
    return dispatch(new Message(5));
}

function EventHandlers_dragStart(tabLabel, dispatch, e) {
    e.dataTransfer.setData("text/plain", tabLabel.Name);
    dispatch(new Message(2, tabLabel.Name));
    const el = targetEl(e);
    const img = toEl(el.cloneNode(true));
    img.classList.add("dragimage");
    document.body.appendChild(img);
    el.classList.add("dragging");
    return e.dataTransfer.setDragImage(img, 0, 0);
}

function EventHandlers_dragEnd(dispatch, e) {
    clearPreview();
    iterate((tupledArg) => {
        const el = tupledArg[0];
        el.remove();
    }, toListFromNodeList(document.querySelectorAll(".dragimage")));
    dispatch(new Message(3));
    dispatch(new Message(4, void 0));
    const el_1 = targetEl(e);
    el_1.classList.remove("dragging");
}

function EventHandlers_tabClick(dockLocation, tabLabel, dispatch, e) {
    let tupledArg, tupledArg_1;
    if (dockLocation.tag === 3) {
        return dispatch((tupledArg = [dockLocation, tabLabel.Name], new Message(6, tupledArg[0], tupledArg[1])));
    }
    else {
        return dispatch((tupledArg_1 = [dockLocation, tabLabel.Name], new Message(7, tupledArg_1[0], tupledArg_1[1])));
    }
}

function viewTabLabel(model, dispatch, dockLocation, tabLabel) {
    return UI_divc("tab-label", [Bind_toggleClass_Z2A796D4F(Store_map((m) => (defaultArg(ModelHelpers_beingDragged(m), "") === tabLabel.Name), model), "preview"), Bind_toggleClass_Z2A796D4F(Store_map((m_1) => (defaultArg(FSharpMap__get_Item(m_1.SelectedPanes, dockLocation), "") === tabLabel.Name), model), "selected"), HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa fa-folder")]), HtmlEngine$1__span_BB573A(EngineHelpers_Html, [EngineHelpers_text(tabLabel.Name)]), AttrEngine$1__draggable_Z1FBCCD16(EngineHelpers_Attr, true), EventEngine$1__onDragStart_Z3384A56C(EngineHelpers_Ev, (e) => {
        EventHandlers_dragStart(tabLabel, dispatch, e);
    }), EventEngine$1__onDragEnd_Z3384A56C(EngineHelpers_Ev, (e_1) => {
        EventHandlers_dragEnd(dispatch, e_1);
    }), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e_2) => {
        EventHandlers_tabClick(dockLocation, tabLabel, dispatch, e_2);
    })]);
}

export function dragOverlay(model, loc) {
    const arg_1 = singleton(Bind_toggleClass_Z2A796D4F(ModelHelpers_showOverlay(model, loc), "visible"));
    return UI_divc(`drag-overlay ${BasicLocation__get_LowerName(DockLocation__get_Primary(loc))} ${DockLocation__get_CssName(loc)}`, arg_1);
}

export function dockContainer(model, loc) {
    const arg_1 = toList(delay(() => append_1(singleton_1(Bind_toggleClass_Z2A796D4F(Store_map((m) => (FSharpMap__get_Item(m.SelectedPanes, loc) == null), model), "hidden")), delay(() => append_1(singleton_1(AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, `dock-${DockLocation__get_CssName(loc)}`)), delay(() => {
        const matchValue = loc;
        switch (matchValue.tag) {
            case 1:
            case 6: {
                return singleton_1(UI_divc("dock-resize-handle top vertical", [resizeControllerNsFlex(1)]));
            }
            case 8:
            case 4: {
                return singleton_1(UI_divc("dock-resize-handle left horizontal", [resizeControllerEwFlex(1)]));
            }
            default: {
                return empty_1();
            }
        }
    }))))));
    return UI_divc(`dock-${DockLocation__get_CssName(loc)}-container dock-${BasicLocation__get_LowerName(DockLocation__get_Hand(loc))}-hand`, arg_1);
}

export class DockContainer {
    constructor(options) {
        const patternInput = Store_makeElmish(init, (msg, model) => update(options, msg, model), (value) => {
        })(DockCollection_get_Empty());
        this.model = patternInput[0];
        this.dispatch = patternInput[1];
    }
}

export function DockContainer$reflection() {
    return class_type("SutilOxide.Dock.DockContainer", void 0, DockContainer);
}

export function DockContainer_$ctor_Z3F65FBC2(options) {
    return new DockContainer(options);
}

export function DockContainer_Create_6ECE4E9E(init_1, options) {
    const dc = DockContainer_$ctor_Z3F65FBC2(options);
    return DockContainer__View_79A576A0(dc, init_1);
}

export function DockContainer__View_79A576A0(__, init_1) {
    return DockContainer__view(__, init_1, __);
}

export function DockContainer__SetProperty_2F023AF1(__, name, p) {
    __.dispatch(new Message(11, [name, p]));
}

export function DockContainer__SetProperties_Z79B44B82(__, name, props) {
    iterate_1((p) => {
        DockContainer__SetProperty_2F023AF1(__, name, p);
    }, props);
}

export function DockContainer__RemovePane_Z721C83C5(__, name) {
    __.dispatch(new Message(0, name));
}

export function DockContainer__AddPane_Z41690A9D(__, name, initLoc, content) {
    DockContainer__AddPane_71369329(__, name, initLoc, content, true);
}

export function DockContainer__AddPane_71369329(__, name, initLoc, content, show) {
    DockContainer__AddPane_1858B09(__, name, initLoc, EngineHelpers_text(name), content, show);
}

export function DockContainer__ContainsPane_Z721C83C5(__, name) {
    return DockHelpers_tryGetPane(__.model.Value.Docks, name) != null;
}

export function DockContainer__ShowPane_Z721C83C5(__, name) {
    __.dispatch(new Message(9, name));
}

export function DockContainer__AddPane_1858B09(__, name, initLoc, header, content, show) {
    let tupledArg;
    const lname = name.toLocaleLowerCase();
    const loc = distinctUntilChanged(Store_map((m) => DockHelpers_findPaneLocation(m.Docks, name), __.model));
    const toolbar = buttonGroup([buttonItem([new ButtonProperty(3, "fa-window-minimize"), new ButtonProperty(2, ""), new ButtonProperty(4, (_arg) => {
        __.dispatch(new Message(8, name));
    })]), dropDownItem([new ButtonProperty(3, "fa-cog"), new ButtonProperty(2, "")], [menuItem([new ButtonProperty(2, "Move To")], [buttonItem([new ButtonProperty(3, "fa-caret-square-left"), new ButtonProperty(2, "Left Top"), new ButtonProperty(4, (_arg_1) => {
        __.dispatch(new Message(10, name, new DockLocation(0)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-left"), new ButtonProperty(2, "Left Bottom"), new ButtonProperty(4, (_arg_2) => {
        __.dispatch(new Message(10, name, new DockLocation(1)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-right"), new ButtonProperty(2, "Right Top"), new ButtonProperty(4, (_arg_3) => {
        __.dispatch(new Message(10, name, new DockLocation(5)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-right"), new ButtonProperty(2, "Right Bottom"), new ButtonProperty(4, (_arg_4) => {
        __.dispatch(new Message(10, name, new DockLocation(6)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-down"), new ButtonProperty(2, "Bottom Left"), new ButtonProperty(4, (_arg_5) => {
        __.dispatch(new Message(10, name, new DockLocation(2)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-down"), new ButtonProperty(2, "Bottom Right"), new ButtonProperty(4, (_arg_6) => {
        __.dispatch(new Message(10, name, new DockLocation(4)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-up"), new ButtonProperty(2, "Top Left"), new ButtonProperty(4, (_arg_7) => {
        __.dispatch(new Message(10, name, new DockLocation(7)));
    })]), buttonItem([new ButtonProperty(3, "fa-caret-square-up"), new ButtonProperty(2, "Top Right"), new ButtonProperty(4, (_arg_8) => {
        __.dispatch(new Message(10, name, new DockLocation(8)));
    })]), buttonItem([new ButtonProperty(3, "fa-square"), new ButtonProperty(2, "Centre"), new ButtonProperty(4, (_arg_9) => {
        __.dispatch(new Message(10, name, new DockLocation(3)));
    })])])])]);
    const wrapper = UI_divc("dock-pane-wrapper", [AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, "pane-" + lname), Bind_toggleClass_Z2A796D4F(Store_map((m_1) => (defaultArg(bind((l) => FSharpMap__get_Item(m_1.SelectedPanes, l), DockHelpers_findPaneLocation(m_1.Docks, name)), "") === name), __.model), "selected"), UI_divc("pane-header", [HtmlEngine$1__div_BB573A(EngineHelpers_Html, [header, Bind_visibility_40BD454A(Store_map((optLoc) => {
        if (!equals(initLoc, new DockLocation(3))) {
            return true;
        }
        else {
            return !equals(optLoc, new DockLocation(3));
        }
    }, loc))(toolbar)])]), UI_divc("pane-content", [content])]);
    (tupledArg = [getContentParentNode(initLoc), wrapper], Program_mountAppend_Z427DD8DF(tupledArg[0], tupledArg[1]));
    __.dispatch(new Message(1, [name, "", initLoc, show]));
}

function DockContainer__dockContainer(this$) {
    return UI_divc("dock-container", [EventEngine$1__onDragOver_Z3384A56C(EngineHelpers_Ev, (e) => {
        EventHandlers_dragOver(this$.dispatch, e);
    }), EventEngine$1__onDrop_Z3384A56C(EngineHelpers_Ev, (e_1) => {
        EventHandlers_drop(this$.dispatch, e_1);
    }), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m) => DockCollection__GetPanes_217D4758(m.Docks, new DockLocation(7)), this$.model)), (tabs) => UI_divc("dock-tabs tabs-top tabs-top-left border border-bottom", toList(delay(() => map_2((tabLabel) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(7), tabLabel), tabs))))), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_1) => DockCollection__GetPanes_217D4758(m_1.Docks, new DockLocation(8)), this$.model)), (tabs_1) => UI_divc("dock-tabs tabs-top tabs-top-right border border-bottom", toList(delay(() => map_2((tabLabel_1) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(8), tabLabel_1), tabs_1))))), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_2) => DockCollection__GetPanes_217D4758(m_2.Docks, new DockLocation(0)), this$.model)), (tabs_2) => UI_divc("dock-tabs tabs-left tabs-left-top border border-right", toList(delay(() => map_2((tabLabel_2) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(0), tabLabel_2), tabs_2))))), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_3) => DockCollection__GetPanes_217D4758(m_3.Docks, new DockLocation(1)), this$.model)), (tabs_3) => UI_divc("dock-tabs tabs-left tabs-left-bottom border border-right", toList(delay(() => map_2((tabLabel_3) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(1), tabLabel_3), tabs_3))))), UI_divc("dock-main-grid", [UI_divc("dock-top-container", [Bind_toggleClass_Z2A796D4F(Store_map((m_4) => equalArrays([FSharpMap__get_Item(m_4.SelectedPanes, new DockLocation(7)), FSharpMap__get_Item(m_4.SelectedPanes, new DockLocation(8))], [void 0, void 0]), this$.model), "hidden"), dockContainer(this$.model, new DockLocation(7)), dockContainer(this$.model, new DockLocation(8)), UI_divc("dock-resize-handle bottom vertical", [resizeControllerNs(-1)])]), UI_divc("dock-centre-container", [UI_divc("dock-left-container", [Bind_toggleClass_Z2A796D4F(Store_map((m_5) => equalArrays([FSharpMap__get_Item(m_5.SelectedPanes, new DockLocation(0)), FSharpMap__get_Item(m_5.SelectedPanes, new DockLocation(1))], [void 0, void 0]), this$.model), "hidden"), dockContainer(this$.model, new DockLocation(0)), dockContainer(this$.model, new DockLocation(1)), UI_divc("dock-resize-handle right horizontal", [resizeControllerEw(-1)])]), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_6) => DockCollection__GetPanes_217D4758(m_6.Docks, new DockLocation(3)), this$.model)), (tabs_4) => UI_divc("dock-tabs tabs-centre border border-bottom", toList(delay(() => ((!isEmpty(tabs_4)) ? (isEmpty(tail(tabs_4)) ? [] : map_2((tabLabel_4) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(3), tabLabel_4), tabs_4)) : []))))), UI_divc("dock-main", [dockContainer(this$.model, new DockLocation(3))]), UI_divc("dock-right-container", [Bind_toggleClass_Z2A796D4F(Store_map((m_7) => equalArrays([FSharpMap__get_Item(m_7.SelectedPanes, new DockLocation(5)), FSharpMap__get_Item(m_7.SelectedPanes, new DockLocation(6))], [void 0, void 0]), this$.model), "hidden"), dockContainer(this$.model, new DockLocation(5)), dockContainer(this$.model, new DockLocation(6)), UI_divc("dock-resize-handle left horizontal", [resizeControllerEw(1)])])]), UI_divc("dock-bottom-container", [Bind_toggleClass_Z2A796D4F(Store_map((m_8) => equalArrays([FSharpMap__get_Item(m_8.SelectedPanes, new DockLocation(2)), FSharpMap__get_Item(m_8.SelectedPanes, new DockLocation(4))], [void 0, void 0]), this$.model), "hidden"), dockContainer(this$.model, new DockLocation(2)), dockContainer(this$.model, new DockLocation(4)), UI_divc("dock-resize-handle top vertical", [resizeControllerNs(1)])])]), UI_divc("overlays", [UI_divc("overlays-left", toList(delay(() => map_2((l_1) => dragOverlay(this$.model, l_1), filter((l) => {
        if (equals(DockLocation__get_Primary(l), new BasicLocation(0))) {
            return true;
        }
        else {
            return equals(DockLocation__get_Secondary(l), new BasicLocation(0));
        }
    }, DockLocation_get_All()))))), UI_divc("overlays-right", toList(delay(() => map_2((l_3) => dragOverlay(this$.model, l_3), filter((l_2) => {
        if (equals(DockLocation__get_Primary(l_2), new BasicLocation(1))) {
            return true;
        }
        else {
            return equals(DockLocation__get_Secondary(l_2), new BasicLocation(1));
        }
    }, DockLocation_get_All())))))]), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_9) => DockCollection__GetPanes_217D4758(m_9.Docks, new DockLocation(5)), this$.model)), (tabs_5) => UI_divc("dock-tabs tabs-right tabs-right-top border border-left", toList(delay(() => map_2((tabLabel_5) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(5), tabLabel_5), tabs_5))))), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_10) => DockCollection__GetPanes_217D4758(m_10.Docks, new DockLocation(6)), this$.model)), (tabs_6) => UI_divc("dock-tabs tabs-right tabs-right-bottom border border-left", toList(delay(() => map_2((tabLabel_6) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(6), tabLabel_6), tabs_6))))), UI_divc("dock-tabs box-left border border-top", []), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_11) => DockCollection__GetPanes_217D4758(m_11.Docks, new DockLocation(2)), this$.model)), (tabs_7) => UI_divc("dock-tabs tabs-bottom tabs-bottom-left border border-top", toList(delay(() => map_2((tabLabel_7) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(2), tabLabel_7), tabs_7))))), Bind_el_ZF0512D0(distinctUntilChanged(Store_map((m_12) => DockCollection__GetPanes_217D4758(m_12.Docks, new DockLocation(4)), this$.model)), (tabs_8) => UI_divc("dock-tabs tabs-bottom tabs-bottom-right border border-top", toList(delay(() => map_2((tabLabel_8) => viewTabLabel(this$.model, this$.dispatch, new DockLocation(4), tabLabel_8), tabs_8))))), UI_divc("dock-tabs box-right border border-top", [])]);
}

function DockContainer__view(this$, init_1, self) {
    return fragment([SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_overflowHidden(EngineHelpers_Css)]), onMount((e) => {
        init_1(self);
    }, singleton(new EventModifier(0))), DockContainer__dockContainer(this$)]);
}

