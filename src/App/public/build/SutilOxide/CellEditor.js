import { toString, Record, Union } from "../fable_modules/fable-library.3.7.20/Types.js";
import { list_type, int32_type, record_type, tuple_type, obj_type, string_type, bool_type, union_type, lambda_type, unit_type, class_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { singleton as singleton_1, append, delay, toList, empty, map, toArray } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { defaultArgWith, map as map_1, defaultArg } from "../fable_modules/fable-library.3.7.20/Option.js";
import { int32ToString, min, comparePrimitives, max, curry, uncurry } from "../fable_modules/fable-library.3.7.20/Util.js";
import { parse } from "../fable_modules/fable-library.3.7.20/Int32.js";
import { isNullOrWhiteSpace, printf, toText } from "../fable_modules/fable-library.3.7.20/String.js";
import { parse as parse_1 } from "../fable_modules/fable-library.3.7.20/Double.js";
import { parse as parse_2 } from "../fable_modules/fable-library.3.7.20/Boolean.js";
import { parse as parse_3, totalSeconds, create, minutes, seconds, hours } from "../fable_modules/fable-library.3.7.20/TimeSpan.js";
import { singleton, empty as empty_1, tryFind, length, isEmpty, ofArray, filter, mapIndexed, fold } from "../fable_modules/fable-library.3.7.20/List.js";
import { Cmd_ofMsg, Cmd_none } from "../Sutil/src/Sutil/Cmd.js";
import { Store_map, Store_getMap, Store_get, Store_modify, Store_make, StoreOperators_op_DotGreater, Store_subscribe, Store_makeElmish, Store_set } from "../Sutil/src/Sutil/Store.js";
import { distinctUntilChanged } from "../Sutil/src/Sutil/Observable.js";
import { autofocus, on, text as text_1, fragment, disposeOnUnmount } from "../Sutil/src/Sutil/CoreElements.js";
import { Bind_attr_3099C820, Bind_toggleClass_Z2A796D4F, Bind_each_1F9EC04A, Bind_el_ZF0512D0 } from "../Sutil/src/Sutil/Bind.js";
import { HtmlEngine$1__input_BB573A, HtmlEngine$1__div_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { AttrEngine$1__className_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { SutilAttrEngine__style_68BDC580, EngineHelpers_Css, EngineHelpers_Ev, EngineHelpers_Html, EngineHelpers_Attr } from "../Sutil/src/Sutil/Html.js";
import { EventEngine$1__onClick_58BC8925, EventEngine$1__onBlur_13C15648, EventEngine$1__onKeyDown_Z2153A397 } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { withStyle, rule } from "../Sutil/src/Sutil/Styling.js";
import { CssEngine$1__padding_Z445F6BAF, CssEngine$1__color_Z721C83C5, CssEngine$1__get_borderStyleNone, CssEngine$1__width_Z445F6BAF, CssEngine$1__zIndex_Z524259A4, CssEngine$1__borderWidth_18A029B5, CssEngine$1__get_borderStyleSolid, CssEngine$1__borderColor_Z721C83C5, CssEngine$1__backgroundColor_Z721C83C5, CssEngine$1__get_positionAbsolute, CssEngine$1__get_positionRelative } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { tryFind as tryFind_1 } from "../fable_modules/fable-library.3.7.20/Array.js";
import { SutilEffect_RegisterDisposable_2069CF16, SutilEffect__get_AsDomNode, SutilElement_Define_Z60F5000F } from "../Sutil/src/Sutil/Core.js";
import { subscribe } from "../fable_modules/fable-library.3.7.20/Observable.js";
import { rafu } from "../Sutil/src/Sutil/DomHelpers.js";

export const console$ = console;

export class ValuesProvider$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["NoValues", "Immediate", "Indirect"];
    }
}

export function ValuesProvider$1$reflection(gen0) {
    return union_type("SutilOxide.CellEditor.ValuesProvider`1", [gen0], ValuesProvider$1, () => [[], [["Item", class_type("System.Collections.Generic.IEnumerable`1", [gen0])]], [["Item", lambda_type(unit_type, class_type("System.Collections.Generic.IEnumerable`1", [gen0]))]]]);
}

export class Config$2 extends Record {
    constructor(IsEditable, Getter, Setter, Formatter, Parser, Styling, Values) {
        super();
        this.IsEditable = IsEditable;
        this.Getter = Getter;
        this.Setter = Setter;
        this.Formatter = Formatter;
        this.Parser = Parser;
        this.Styling = Styling;
        this.Values = Values;
    }
    get Editable() {
        const this$ = this;
        return this$.IsEditable;
    }
    GetStringValue(r) {
        const this$ = this;
        return this$.Formatter(this$.Getter(r));
    }
    SetStringValue(r, value) {
        const this$ = this;
        this$.Setter(r, this$.Parser(value));
    }
    get Style() {
        const this$ = this;
        return this$.Styling;
    }
    get AllowedValues() {
        let matchValue, values, values_1;
        const this$ = this;
        return toArray(map(this$.Formatter, (matchValue = this$.Values, (matchValue.tag === 1) ? ((values = matchValue.fields[0], values)) : ((matchValue.tag === 2) ? ((values_1 = matchValue.fields[0], values_1())) : empty()))));
    }
}

export function Config$2$reflection(gen0, gen1) {
    return record_type("SutilOxide.CellEditor.Config`2", [gen0, gen1], Config$2, () => [["IsEditable", bool_type], ["Getter", lambda_type(gen0, gen1)], ["Setter", lambda_type(gen0, lambda_type(gen1, unit_type))], ["Formatter", lambda_type(gen1, string_type)], ["Parser", lambda_type(string_type, gen1)], ["Styling", class_type("System.Collections.Generic.IEnumerable`1", [tuple_type(string_type, obj_type)])], ["Values", ValuesProvider$1$reflection(gen1)]]);
}

export function Config$2_Create_1E5B0640(getter, setter, parser, formatter, editable) {
    return new Config$2(defaultArg(editable, true), getter, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), defaultArg(formatter, (x) => {
        let copyOfStruct = x;
        return toString(copyOfStruct);
    }), defaultArg(parser, (s) => {
        throw (new Error("Not supported"));
    }), [], new ValuesProvider$1(0));
}

export class IntCell {
    constructor() {
    }
}

export function IntCell$reflection() {
    return class_type("SutilOxide.CellEditor.IntCell", void 0, IntCell);
}

export function IntCell_$ctor() {
    return new IntCell();
}

export function IntCell_Create_Z7611087E(value, formatString, setter) {
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), (arg) => parse(arg, 511, false, 32), toText(formatString), curry(2, setter) != null);
}

export function IntCell_Create_Z43AFC5E1(value, setter) {
    let clo;
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), (arg) => parse(arg, 511, false, 32), (clo = toText(printf("%d")), clo), curry(2, setter) != null);
}

export class FloatCell {
    constructor() {
    }
}

export function FloatCell$reflection() {
    return class_type("SutilOxide.CellEditor.FloatCell", void 0, FloatCell);
}

export function FloatCell_$ctor() {
    return new FloatCell();
}

export function FloatCell_Create_11E7D0C5(value, formatString, setter) {
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), parse_1, toText(formatString), curry(2, setter) != null);
}

export function FloatCell_Create_4547699F(value, setter) {
    let clo;
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), parse_1, (clo = toText(printf("%f")), clo), curry(2, setter) != null);
}

export class StrCell {
    constructor() {
    }
}

export function StrCell$reflection() {
    return class_type("SutilOxide.CellEditor.StrCell", void 0, StrCell);
}

export function StrCell_$ctor() {
    return new StrCell();
}

export function StrCell_Create_2F5568C5(value, formatString, setter) {
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), (x) => x, toText(formatString), curry(2, setter) != null);
}

export function StrCell_Create_Z3903A61(value, setter) {
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), (x) => x, (x_1) => x_1);
}

export class BoolCell {
    constructor() {
    }
}

export function BoolCell$reflection() {
    return class_type("SutilOxide.CellEditor.BoolCell", void 0, BoolCell);
}

export function BoolCell_$ctor() {
    return new BoolCell();
}

export function BoolCell_Create_Z1C3F898C(value, formatString, setter) {
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), parse_2, toText(formatString), curry(2, setter) != null);
}

export function BoolCell_Create_Z33FC8AA1(value, setter) {
    let clo;
    return Config$2_Create_1E5B0640(value, uncurry(2, defaultArg(curry(2, setter), (arg00$0040) => ((arg10$0040) => {
    }))), parse_2, (clo = toText(printf("%A")), clo), curry(2, setter) != null);
}

export function formatDuration(v) {
    if (hours(v) > 0) {
        const arg_2 = seconds(v) | 0;
        const arg_1 = minutes(v) | 0;
        const arg = hours(v) | 0;
        return toText(printf("%d:%02d:%02d"))(arg)(arg_1)(arg_2);
    }
    else {
        const arg_4 = seconds(v) | 0;
        const arg_3 = minutes(v) | 0;
        return toText(printf("%d:%02d"))(arg_3)(arg_4);
    }
}

export class DurationCell {
    constructor() {
    }
}

export function DurationCell$reflection() {
    return class_type("SutilOxide.CellEditor.DurationCell", void 0, DurationCell);
}

export function DurationCell_$ctor() {
    return new DurationCell();
}

export function DurationCell_Create_Z6B5A2738(value) {
    return Config$2_Create_1E5B0640((r) => create(0, 0, ~(~value(r))), uncurry(2, void 0), void 0, formatDuration, false);
}

export function DurationCell_Create_4547699F(value, setter) {
    return Config$2_Create_1E5B0640((r) => create(0, 0, ~(~value(r))), uncurry(2, defaultArg(map_1((setFn) => ((r_1) => ((ts) => {
        setFn(r_1)(totalSeconds(ts));
    })), curry(2, setter)), (arg00$0040) => ((arg10$0040) => {
    }))), (s) => parse_3("00:" + s), formatDuration, curry(2, setter) != null);
}

export class Options$2 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["IsEditable", "Formatter", "Parser", "Getter", "Setter", "Styling", "Values"];
    }
}

export function Options$2$reflection(gen0, gen1) {
    return union_type("SutilOxide.CellEditor.Options`2", [gen0, gen1], Options$2, () => [[["Item", bool_type]], [["Item", lambda_type(gen1, string_type)]], [["Item", lambda_type(string_type, gen1)]], [["Item", lambda_type(gen0, gen1)]], [["Item", lambda_type(gen0, lambda_type(gen1, unit_type))]], [["Item", class_type("System.Collections.Generic.IEnumerable`1", [tuple_type(string_type, obj_type)])]], [["Item", ValuesProvider$1$reflection(gen1)]]]);
}

function update(cfg, option) {
    switch (option.tag) {
        case 1: {
            const f_1 = option.fields[0];
            return new Config$2(cfg.IsEditable, cfg.Getter, cfg.Setter, f_1, cfg.Parser, cfg.Styling, cfg.Values);
        }
        case 2: {
            const f_2 = option.fields[0];
            return new Config$2(cfg.IsEditable, cfg.Getter, cfg.Setter, cfg.Formatter, f_2, cfg.Styling, cfg.Values);
        }
        case 3: {
            const f_3 = option.fields[0];
            return new Config$2(cfg.IsEditable, f_3, cfg.Setter, cfg.Formatter, cfg.Parser, cfg.Styling, cfg.Values);
        }
        case 4: {
            const f_4 = option.fields[0];
            return new Config$2(cfg.IsEditable, cfg.Getter, f_4, cfg.Formatter, cfg.Parser, cfg.Styling, cfg.Values);
        }
        case 5: {
            const f_5 = option.fields[0];
            return new Config$2(cfg.IsEditable, cfg.Getter, cfg.Setter, cfg.Formatter, cfg.Parser, f_5, cfg.Values);
        }
        case 6: {
            const f_6 = option.fields[0];
            return new Config$2(cfg.IsEditable, cfg.Getter, cfg.Setter, cfg.Formatter, cfg.Parser, cfg.Styling, f_6);
        }
        default: {
            const f = option.fields[0];
            return new Config$2(f, cfg.Getter, cfg.Setter, cfg.Formatter, cfg.Parser, cfg.Styling, cfg.Values);
        }
    }
}

export function withOptions(options, cfg) {
    return fold(update, cfg, options);
}

class Autocomplete_FilteredItem extends Record {
    constructor(Index, Value) {
        super();
        this.Index = (Index | 0);
        this.Value = Value;
    }
}

function Autocomplete_FilteredItem$reflection() {
    return record_type("SutilOxide.CellEditor.Autocomplete.FilteredItem", [], Autocomplete_FilteredItem, () => [["Index", int32_type], ["Value", string_type]]);
}

class Autocomplete_Model extends Record {
    constructor(SelectedIndex, Values, FilteredValues, Showing, Value) {
        super();
        this.SelectedIndex = (SelectedIndex | 0);
        this.Values = Values;
        this.FilteredValues = FilteredValues;
        this.Showing = Showing;
        this.Value = Value;
    }
}

function Autocomplete_Model$reflection() {
    return record_type("SutilOxide.CellEditor.Autocomplete.Model", [], Autocomplete_Model, () => [["SelectedIndex", int32_type], ["Values", list_type(string_type)], ["FilteredValues", list_type(Autocomplete_FilteredItem$reflection())], ["Showing", bool_type], ["Value", string_type]]);
}

function Autocomplete_mValues(m) {
    return m.Values;
}

function Autocomplete_mFilteredValues(m) {
    return m.FilteredValues;
}

function Autocomplete_mSelectedIndex(m) {
    return m.SelectedIndex;
}

function Autocomplete_mShowing(m) {
    return m.Showing;
}

class Autocomplete_Message extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SetSelectedIndex", "IncSelectedIndex", "SetShowing", "SetValue", "Commit"];
    }
}

function Autocomplete_Message$reflection() {
    return union_type("SutilOxide.CellEditor.Autocomplete.Message", [], Autocomplete_Message, () => [[["Item", int32_type]], [["Item", int32_type]], [["Item", bool_type]], [["Item", string_type]], []]);
}

function Autocomplete_filtered(value, values) {
    return mapIndexed((i, v_1) => (new Autocomplete_FilteredItem(i, v_1)), (value === "") ? values : filter((v) => (v.toLocaleLowerCase().indexOf(value.toLocaleLowerCase()) >= 0), values));
}

function Autocomplete_init(values) {
    return [new Autocomplete_Model(-1, ofArray(values), Autocomplete_filtered("", ofArray(values)), false, ""), Cmd_none()];
}

function Autocomplete_update(value, msg, model) {
    switch (msg.tag) {
        case 3: {
            const v_1 = msg.fields[0];
            const fv = Autocomplete_filtered(v_1, model.Values);
            return [new Autocomplete_Model(0, model.Values, fv, model.Showing && (!isEmpty(fv)), v_1), Cmd_none()];
        }
        case 2: {
            const f = msg.fields[0];
            return [new Autocomplete_Model(0, model.Values, model.FilteredValues, f && (!isEmpty(model.FilteredValues)), model.Value), Cmd_none()];
        }
        case 0: {
            const i = msg.fields[0] | 0;
            return [new Autocomplete_Model(i, model.Values, model.FilteredValues, model.Showing, model.Value), Cmd_none()];
        }
        case 1: {
            const i_1 = msg.fields[0] | 0;
            if (model.Showing) {
                const newIndex = max(comparePrimitives, 0, min(comparePrimitives, model.SelectedIndex + i_1, length(model.FilteredValues) - 1)) | 0;
                return [new Autocomplete_Model(newIndex, model.Values, model.FilteredValues, model.Showing, model.Value), Cmd_none()];
            }
            else {
                return [model, Cmd_ofMsg(new Autocomplete_Message(2, true))];
            }
        }
        default: {
            if (model.Showing) {
                const patternInput = defaultArg(map_1((fi) => [fi.Value, fi.Index], tryFind((v) => (v.Index === model.SelectedIndex), model.FilteredValues)), [model.Value, -1]);
                const selectedValue = patternInput[0];
                const selectedIndex = patternInput[1] | 0;
                Store_set(value, selectedValue);
                return [new Autocomplete_Model(selectedIndex, model.Values, model.FilteredValues, model.Showing, selectedValue), Cmd_ofMsg(new Autocomplete_Message(2, false))];
            }
            else {
                return [model, Cmd_none()];
            }
        }
    }
}

function Autocomplete_view(value, values) {
    const patternInput = Store_makeElmish(Autocomplete_init, (msg, model) => Autocomplete_update(value, msg, model), (value_1) => {
    })(values);
    const model_1 = patternInput[0];
    const dispatch = patternInput[1];
    const show = (f) => {
        dispatch(new Autocomplete_Message(2, f));
    };
    const incIndex = (i) => {
        dispatch(new Autocomplete_Message(1, i));
    };
    const watchValue = Store_subscribe((arg_3) => {
        dispatch(new Autocomplete_Message(3, arg_3));
    }, distinctUntilChanged(value));
    return ofArray([disposeOnUnmount(ofArray([model_1, watchValue])), Bind_el_ZF0512D0(distinctUntilChanged(StoreOperators_op_DotGreater(model_1, Autocomplete_mShowing)), (show_1) => ((!show_1) ? fragment([]) : HtmlEngine$1__div_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "autocomplete"), Bind_each_1F9EC04A(StoreOperators_op_DotGreater(model_1, Autocomplete_mFilteredValues), (item) => HtmlEngine$1__div_BB573A(EngineHelpers_Html, [Bind_toggleClass_Z2A796D4F(StoreOperators_op_DotGreater(StoreOperators_op_DotGreater(model_1, Autocomplete_mSelectedIndex), (y) => (item.Index === y)), "selected"), text_1(item.Value)]), (item_1) => item_1.Value)]))), on("focusout", (e) => {
        show(false);
    }, empty_1()), EventEngine$1__onKeyDown_Z2153A397(EngineHelpers_Ev, (e_1) => {
        const matchValue = e_1.key;
        switch (matchValue) {
            case "Escape": {
                show(false);
                e_1.preventDefault();
                break;
            }
            case "ArrowDown": {
                incIndex(1);
                e_1.preventDefault();
                break;
            }
            case "ArrowUp": {
                incIndex(-1);
                e_1.preventDefault();
                break;
            }
            case "Return":
            case "Enter": {
                dispatch(new Autocomplete_Message(4));
                break;
            }
            default: {
            }
        }
    })]);
}

export class Elmish_Model$1 extends Record {
    constructor(Record, Editing, Selected) {
        super();
        this.Record = Record;
        this.Editing = Editing;
        this.Selected = Selected;
    }
}

export function Elmish_Model$1$reflection(gen0) {
    return record_type("SutilOxide.CellEditor.Elmish.Model`1", [gen0], Elmish_Model$1, () => [["Record", gen0], ["Editing", bool_type], ["Selected", bool_type]]);
}

export class Elmish_Message$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["StartEdit", "FinishEdit", "ToggleSelect", "SetSelected"];
    }
}

export function Elmish_Message$1$reflection(gen0) {
    return union_type("SutilOxide.CellEditor.Elmish.Message`1", [gen0], Elmish_Message$1, () => [[["Item", gen0]], [["Item", gen0]], [["Item", gen0]], [["Item1", gen0], ["Item2", bool_type]]]);
}

export function Elmish_init(record) {
    return [new Elmish_Model$1(record, false, false), Cmd_none()];
}

export function Elmish_update(msg, model) {
    let tupledArg;
    switch (msg.tag) {
        case 0: {
            const r_1 = msg.fields[0];
            return [new Elmish_Model$1(model.Record, true, model.Selected), Cmd_none()];
        }
        case 2: {
            const r_2 = msg.fields[0];
            return [model, Cmd_ofMsg((tupledArg = [r_2, !model.Selected], new Elmish_Message$1(3, tupledArg[0], tupledArg[1])))];
        }
        case 3: {
            const r_3 = msg.fields[0];
            const b = msg.fields[1];
            return [new Elmish_Model$1(model.Record, model.Editing, b), Cmd_none()];
        }
        default: {
            const r = msg.fields[0];
            return [new Elmish_Model$1(model.Record, false, model.Selected), Cmd_none()];
        }
    }
}

export const cellStyle = ofArray([rule(".cell", singleton(CssEngine$1__get_positionRelative(EngineHelpers_Css))), rule(".autocomplete", ofArray([CssEngine$1__get_positionAbsolute(EngineHelpers_Css), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "white"), CssEngine$1__borderColor_Z721C83C5(EngineHelpers_Css, "gray"), CssEngine$1__get_borderStyleSolid(EngineHelpers_Css), CssEngine$1__borderWidth_18A029B5(EngineHelpers_Css, int32ToString(1) + "px"), CssEngine$1__zIndex_Z524259A4(EngineHelpers_Css, 10), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")])), rule(".cell input", ofArray([CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__get_borderStyleNone(EngineHelpers_Css), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "inherit")])), rule(".autocomplete .selected", ofArray([CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "black"), CssEngine$1__color_Z721C83C5(EngineHelpers_Css, "white")]))]);

export function findBest(allowed, value) {
    return defaultArgWith(tryFind_1((s) => (s.toLocaleLowerCase() === value.toLocaleLowerCase()), allowed), () => defaultArg(tryFind_1((s_1) => {
        if (!isNullOrWhiteSpace(value)) {
            return s_1.toLocaleLowerCase().indexOf(value.toLocaleLowerCase()) >= 0;
        }
        else {
            return false;
        }
    }, allowed), value));
}

export function bindFocus(isFocused) {
    return SutilElement_Define_Z60F5000F("bindFocus", (ctx) => {
        const inputEl = SutilEffect__get_AsDomNode(ctx.Parent);
        const un = subscribe((f) => {
            if (f) {
                rafu(() => {
                    inputEl.focus();
                    inputEl.setSelectionRange(99999, 99999);
                });
            }
        }, isFocused);
        SutilEffect_RegisterDisposable_2069CF16(ctx.Parent, un);
    });
}

export function view(record, wantsFocus, cell) {
    return withStyle(cellStyle, HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton_1(SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, cell.Style)), delay(() => append(singleton_1(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "cell")), delay(() => {
        if (cell.Editable) {
            const valueStore = Store_make(cell.GetStringValue(record));
            return append(singleton_1(disposeOnUnmount(singleton(valueStore))), delay(() => append(singleton_1(HtmlEngine$1__input_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton_1(Bind_attr_3099C820("value", valueStore)), delay(() => append(wantsFocus ? singleton_1(autofocus) : empty(), delay(() => singleton_1(EventEngine$1__onBlur_13C15648(EngineHelpers_Ev, (e) => {
                let allowed;
                Store_modify((allowed = cell.AllowedValues, (value) => findBest(allowed, value)), valueStore);
                const text = Store_get(valueStore);
                if (text !== cell.GetStringValue(record)) {
                    cell.SetStringValue(record, text);
                }
            })))))))))), delay(() => {
                const allowedValues = cell.AllowedValues;
                return (allowedValues.length > 0) ? Autocomplete_view(valueStore, allowedValues) : empty();
            }))));
        }
        else {
            return singleton_1(text_1(cell.GetStringValue(record)));
        }
    }))))))));
}

export const editStyle = ofArray([rule(".editing", singleton(CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "#fae3cd"))), rule(".editing\u003ediv:focus", singleton(CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "white"))), rule(".edit-row", singleton(CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, (0.2).toString() + "rem"))), rule(".selected .row-select", singleton(CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "gray")))]);

export function containsNode(parent, child) {
    if (parent == null) {
        return false;
    }
    else if (child != null) {
        const x = child;
        return parent.contains(x);
    }
    else {
        return false;
    }
}

export function onFocusEnter(f) {
    return on("focusin", (e) => {
        f();
    }, empty_1());
}

export function onFocusLeave(f) {
    return on("focusout", (e) => {
        const currentTarget = e.currentTarget;
        if (!containsNode(currentTarget, e.relatedTarget)) {
            f();
        }
    }, empty_1());
}

export class EditController$2 {
    constructor(record, key, cells) {
        this.record = record;
        this.key = key;
        this.cells = cells;
        this.model = null;
        this.dispatch = null;
    }
}

export function EditController$2$reflection(gen0, gen1) {
    return class_type("SutilOxide.CellEditor.EditController`2", [gen0, gen1], EditController$2);
}

export function EditController$2_$ctor_Z5A9CA05A(record, key, cells) {
    return new EditController$2(record, key, cells);
}

export function EditController$2__get_Selected(this$) {
    return Store_getMap((m) => m.Selected, this$.model);
}

export function EditController$2__set_Selected_Z1FBCCD16(this$, value) {
    if (EditController$2__get_Selected(this$)) {
        this$.dispatch(new Elmish_Message$1(3, this$.record, value));
    }
}

export function EditController$2__View_6CD04694(_, dispatch) {
    return EditController$2__view_6CD04694(_, dispatch);
}

export function EditController$2__get_Record(_) {
    return _.record;
}

export function EditController$2__get_Key(_) {
    return _.key(_.record);
}

function EditController$2__view_6CD04694(this$, parentDispatch) {
    const patternInput = Store_makeElmish(Elmish_init, (msg, model) => {
        const result = Elmish_update(msg, model);
        parentDispatch(msg);
        return result;
    }, (value) => {
    })(this$.record);
    const model$0027 = patternInput[0];
    const dispatch$0027 = patternInput[1];
    this$.model = model$0027;
    this$.dispatch = dispatch$0027;
    return withStyle(editStyle, HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton_1(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "edit-row")), delay(() => append(singleton_1(disposeOnUnmount(singleton(this$.model))), delay(() => append(singleton_1(onFocusEnter(() => {
        this$.dispatch(new Elmish_Message$1(0, this$.record));
    })), delay(() => append(singleton_1(onFocusLeave(() => {
        this$.dispatch(new Elmish_Message$1(1, this$.record));
    })), delay(() => append(singleton_1(Bind_toggleClass_Z2A796D4F(Store_map((m) => m.Editing, this$.model), "editing")), delay(() => append(singleton_1(Bind_toggleClass_Z2A796D4F(Store_map((m_1) => m_1.Selected, this$.model), "selected")), delay(() => append(singleton_1(HtmlEngine$1__div_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "row-select"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_2) => {
        this$.dispatch(new Elmish_Message$1(2, this$.record));
    })])), delay(() => map((cell) => view(this$.record, false, cell), this$.cells)))))))))))))))))));
}

export function edit(cells, key, record) {
    return EditController$2_$ctor_Z5A9CA05A(record, key, cells);
}

