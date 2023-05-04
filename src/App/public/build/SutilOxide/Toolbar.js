import { record_type, option_type, bool_type, lambda_type, unit_type, string_type, union_type, class_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { HtmlEngine$1__span_Z721C83C5, HtmlEngine$1__hr_BB573A, HtmlEngine$1__span_BB573A, HtmlEngine$1__i_BB573A, HtmlEngine$1__a_BB573A, HtmlEngine$1__div_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { empty, iterate as iterate_1, map, fold, singleton, append, delay, toList } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { AttrEngine$1__href_Z721C83C5, AttrEngine$1__className_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { EngineHelpers_text, EngineHelpers_Ev, EngineHelpers_Css, SutilAttrEngine__style_68BDC580, EngineHelpers_Html, EngineHelpers_Attr } from "../Sutil/src/Sutil/Html.js";
import { Record, Union } from "../fable_modules/fable-library.3.7.20/Types.js";
import { rangeDouble } from "../fable_modules/fable-library.3.7.20/Range.js";
import { map as map_1, defaultArg, some } from "../fable_modules/fable-library.3.7.20/Option.js";
import { ofArray, iterate } from "../fable_modules/fable-library.3.7.20/List.js";
import { printf, toText } from "../fable_modules/fable-library.3.7.20/String.js";
import { equals, createAtom } from "../fable_modules/fable-library.3.7.20/Util.js";
import { nothing, host } from "../Sutil/src/Sutil/CoreElements.js";
import { CssEngine$1__get_displayNone } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { EventEngine$1__onClick_58BC8925 } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";

export class UI {
    constructor() {
    }
}

export function UI$reflection() {
    return class_type("SutilOxide.Toolbar.UI", void 0, UI);
}

export function UI_divc(cls, items) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, cls)), delay(() => items)))));
}

export class ButtonMode extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Checkbox", "Button"];
    }
}

export function ButtonMode$reflection() {
    return union_type("SutilOxide.Toolbar.ButtonMode", [], ButtonMode, () => [[], []]);
}

export class DisplayMode extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["LabelOnly", "IconOnly", "LabelIcon"];
    }
}

export function DisplayMode$reflection() {
    return union_type("SutilOxide.Toolbar.DisplayMode", [], DisplayMode, () => [[], [], []]);
}

export class ButtonProperty extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Display", "Mode", "Label", "Icon", "OnClick", "OnCheckChanged", "IsChecked"];
    }
}

export function ButtonProperty$reflection() {
    return union_type("SutilOxide.Toolbar.ButtonProperty", [], ButtonProperty, () => [[["Item", DisplayMode$reflection()]], [["Item", ButtonMode$reflection()]], [["Item", string_type]], [["Item", string_type]], [["Item", lambda_type(class_type("Browser.Types.MouseEvent", void 0, MouseEvent), unit_type)]], [["Item", lambda_type(bool_type, unit_type)]], [["Item", bool_type]]]);
}

export class Button extends Record {
    constructor(Display, Mode, Label, Icon, OnClick, OnCheckChanged, IsChecked) {
        super();
        this.Display = Display;
        this.Mode = Mode;
        this.Label = Label;
        this.Icon = Icon;
        this.OnClick = OnClick;
        this.OnCheckChanged = OnCheckChanged;
        this.IsChecked = IsChecked;
    }
}

export function Button$reflection() {
    return record_type("SutilOxide.Toolbar.Button", [], Button, () => [["Display", DisplayMode$reflection()], ["Mode", ButtonMode$reflection()], ["Label", option_type(string_type)], ["Icon", option_type(string_type)], ["OnClick", option_type(lambda_type(class_type("Browser.Types.MouseEvent", void 0, MouseEvent), unit_type))], ["OnCheckChanged", option_type(lambda_type(bool_type, unit_type))], ["IsChecked", bool_type]]);
}

export function Button_get_Empty() {
    return new Button(new DisplayMode(2), new ButtonMode(1), void 0, void 0, void 0, void 0, false);
}

export function Button__With_Z6A6902B(__, p) {
    switch (p.tag) {
        case 1: {
            const s_1 = p.fields[0];
            return new Button(__.Display, s_1, __.Label, __.Icon, __.OnClick, __.OnCheckChanged, __.IsChecked);
        }
        case 2: {
            const s_2 = p.fields[0];
            return new Button(__.Display, __.Mode, s_2, __.Icon, __.OnClick, __.OnCheckChanged, __.IsChecked);
        }
        case 3: {
            const s_3 = p.fields[0];
            return new Button(__.Display, __.Mode, __.Label, s_3, __.OnClick, __.OnCheckChanged, __.IsChecked);
        }
        case 6: {
            const s_4 = p.fields[0];
            return new Button(__.Display, __.Mode, __.Label, __.Icon, __.OnClick, __.OnCheckChanged, s_4);
        }
        case 4: {
            const s_5 = p.fields[0];
            return new Button(__.Display, __.Mode, __.Label, __.Icon, s_5, __.OnCheckChanged, __.IsChecked);
        }
        case 5: {
            const s_6 = p.fields[0];
            return new Button(__.Display, __.Mode, __.Label, __.Icon, __.OnClick, s_6, __.IsChecked);
        }
        default: {
            const s = p.fields[0];
            return new Button(s, __.Mode, __.Label, __.Icon, __.OnClick, __.OnCheckChanged, __.IsChecked);
        }
    }
}

export function Button_From_5E646C7A(p) {
    return fold(Button__With_Z6A6902B, Button_get_Empty(), p);
}

export function MenuMonitor_seqOfNodeList(nodes) {
    return delay(() => map((i) => (nodes[i]), toList(rangeDouble(0, 1, nodes.length - 1))));
}

export function MenuMonitor_logEntry(e) {
    console.log(some("boundingClientRect="), e.boundingClientRect);
    console.log(some("intersectionRatio="), e.intersectionRatio);
    console.log(some("intersectionRect="), e.intersectionRect);
    console.log(some("isIntersecting="), e.isIntersecting);
    console.log(some("rootBounds="), e.rootBounds);
    console.log(some("target="), e.target);
    console.log(some("time="), e.time);
}

export function MenuMonitor_removeStyle(e, name) {
    (e.style).removeProperty(name);
}

export function MenuMonitor_resetMenu(e) {
    iterate((name) => {
        MenuMonitor_removeStyle(e, name);
    }, ofArray(["top", "left", "bottom", "right"]));
}

export function MenuMonitor_moveMenu(e, bcr, ir) {
    let arg;
    if (bcr.right > ir.right) {
        (e.style).left = "unset";
        (e.style).right = "0px";
    }
    if (bcr.bottom > ir.bottom) {
        (e.style).top = ((arg = (ir.bottom - bcr.height), toText(printf("%fpx"))(arg)));
        (e.style).bottom = "unset";
    }
    else if (bcr.top < ir.top) {
        (e.style).top = "0px";
        (e.style).bottom = "unset";
    }
}

export function MenuMonitor_callback(entries, _arg) {
    entries.forEach((e) => {
        if (e.isIntersecting && (e.intersectionRatio < 1)) {
            MenuMonitor_moveMenu(e.target, e.boundingClientRect, e.intersectionRect);
        }
    });
}

export let MenuMonitor__observer = createAtom(void 0);

export function MenuMonitor_makeObserver() {
    const options = {
        root: document,
        rootMargin: "",
        threshold: 0,
    };
    return new IntersectionObserver(((entries, arg10$0040) => {
        MenuMonitor_callback(entries, arg10$0040);
    }), options);
}

export function MenuMonitor_getObserver() {
    if (MenuMonitor__observer() != null) {
        const x = MenuMonitor__observer();
        return x;
    }
    else {
        const _io = MenuMonitor_makeObserver();
        MenuMonitor__observer(_io, true);
        return _io;
    }
}

export function MenuMonitor_monitorMenu(e) {
    MenuMonitor_resetMenu(e);
    MenuMonitor_getObserver().observe(e);
}

export function MenuMonitor_monitorAll() {
    MenuMonitor_getObserver().disconnect();
    iterate_1((n) => {
        MenuMonitor_monitorMenu(n);
    }, MenuMonitor_seqOfNodeList(document.querySelectorAll(".menu-stack")));
}

export function buttonGroup(items) {
    return UI_divc("button-group", items);
}

export function menuStack(items) {
    return UI_divc("menu-stack", toList(delay(() => append(singleton(host((e) => {
        MenuMonitor_monitorMenu(e);
    })), delay(() => items)))));
}

export function mkButton(b) {
    return HtmlEngine$1__a_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "xd-item-button" + (equals(b.Mode, new ButtonMode(0)) ? " checkbox" : ""))), delay(() => append(singleton(AttrEngine$1__href_Z721C83C5(EngineHelpers_Attr, "-")), delay(() => {
        let matchValue, icon;
        return append((matchValue = [b.Mode, b.Icon, b.Display], (matchValue[0].tag === 0) ? singleton(HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa fa-check " + (b.IsChecked ? "checked" : ""))])) : ((matchValue[1] != null) ? ((matchValue[2].tag === 2) ? ((icon = matchValue[1], singleton(HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa " + icon)])))) : ((matchValue[2].tag === 1) ? ((icon = matchValue[1], singleton(HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa " + icon)])))) : singleton(HtmlEngine$1__i_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_displayNone(EngineHelpers_Css)])])))) : singleton(HtmlEngine$1__i_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_displayNone(EngineHelpers_Css)])])))), delay(() => {
            let matchValue_1, label;
            return append((matchValue_1 = [b.Label, b.Display], (matchValue_1[0] != null) ? ((matchValue_1[1].tag === 0) ? ((label = matchValue_1[0], singleton(HtmlEngine$1__span_BB573A(EngineHelpers_Html, [EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e) => {
                console.log(some("click span"));
            }), EngineHelpers_text(label)])))) : ((matchValue_1[1].tag === 2) ? ((label = matchValue_1[0], singleton(HtmlEngine$1__span_BB573A(EngineHelpers_Html, [EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e) => {
                console.log(some("click span"));
            }), EngineHelpers_text(label)])))) : singleton(HtmlEngine$1__span_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_displayNone(EngineHelpers_Css)])])))) : singleton(HtmlEngine$1__span_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_displayNone(EngineHelpers_Css)])]))), delay(() => append(singleton(defaultArg(map_1((cb) => EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e_1) => {
                console.log(some("click"));
                e_1.preventDefault();
                cb(e_1);
            }), b.OnClick), nothing)), delay(() => singleton(defaultArg(map_1((cb_1) => EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e_2) => {
                console.log(some("click2"));
                e_2.preventDefault();
                cb_1(!b.IsChecked);
            }), b.OnCheckChanged), nothing))))));
        }));
    })))))));
}

export const vseparator = HtmlEngine$1__span_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "xd-vseparator"), EngineHelpers_text("|")]);

export const hseparator = HtmlEngine$1__hr_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "xd-hseparator")]);

export const gap = HtmlEngine$1__span_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "xd-gap")]);

export function buttonItem(props) {
    let inputRecord;
    return mkButton((inputRecord = Button_From_5E646C7A(props), new Button(inputRecord.Display, new ButtonMode(1), inputRecord.Label, inputRecord.Icon, inputRecord.OnClick, inputRecord.OnCheckChanged, inputRecord.IsChecked)));
}

export function button(name, icon, cb) {
    return buttonItem(toList(delay(() => append((icon !== "") ? singleton(new ButtonProperty(3, "fa-" + icon)) : empty(), delay(() => append(singleton(new ButtonProperty(2, name)), delay(() => append(singleton(new ButtonProperty(4, cb)), delay(() => ((icon !== "") ? singleton(new ButtonProperty(0, new DisplayMode(1))) : empty()))))))))));
}

export function toolbar(props, items) {
    return UI_divc("xd-toolbar", items);
}

export function statusbar(props, items) {
    return UI_divc("xd-toolbar xd-statusbar theme-control-bg theme-border", items);
}

export function checkItem(props) {
    let inputRecord;
    return mkButton((inputRecord = Button_From_5E646C7A(props), new Button(inputRecord.Display, new ButtonMode(0), inputRecord.Label, void 0, void 0, inputRecord.OnCheckChanged, inputRecord.IsChecked)));
}

export function menuItem(props, items) {
    const b = Button_From_5E646C7A(props);
    return HtmlEngine$1__a_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "xd-item-button item-menu"), defaultArg(map_1((icon) => HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa " + icon)]), b.Icon), HtmlEngine$1__i_BB573A(EngineHelpers_Html, [])), defaultArg(map_1((label) => HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, label), b.Label), HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, "")), defaultArg(map_1((cb) => EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e) => {
        console.log(some("click"));
        e.preventDefault();
        cb(e);
    }), b.OnClick), nothing), HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa fa-angle-right")]), AttrEngine$1__href_Z721C83C5(EngineHelpers_Attr, "-"), menuStack(items)]);
}

export function dropDownItem(props, items) {
    return HtmlEngine$1__a_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "xd-item-button xd-dropdown")), delay(() => append(singleton(AttrEngine$1__href_Z721C83C5(EngineHelpers_Attr, "-")), delay(() => append(map((p) => {
        switch (p.tag) {
            case 2: {
                const label = p.fields[0];
                return HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, label);
            }
            case 3: {
                const icon = p.fields[0];
                return HtmlEngine$1__i_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "fa " + icon)]);
            }
            default: {
                return nothing;
            }
        }
    }, props), delay(() => append(singleton(EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (e) => {
        iterate_1((p_1) => {
            if (p_1.tag === 4) {
                const cb = p_1.fields[0];
                cb(e);
            }
        }, props);
        if (!e.defaultPrevented) {
            e.preventDefault();
        }
    })), delay(() => singleton(menuStack(items)))))))))))));
}

export function right(items) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "item-group-right")), delay(() => items)))));
}

