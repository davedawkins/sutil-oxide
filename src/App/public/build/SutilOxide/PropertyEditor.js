import { Union, toString, Record } from "../fable_modules/fable-library.3.7.20/Types.js";
import { union_type, list_type, bool_type, float64_type, int32_type, equals, record_type, string_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { SutilElement$reflection } from "../Sutil/src/Sutil/Core.js";
import { some } from "../fable_modules/fable-library.3.7.20/Option.js";
import { view as view_1, StrCell_Create_Z3903A61, BoolCell_Create_Z33FC8AA1, IntCell_Create_Z43AFC5E1, FloatCell_Create_4547699F } from "./CellEditor.js";
import { MutableMap$2__Set_5BDDA1, MutableMap$2__Get_2B595 } from "./PropertyTypes.js";
import { map } from "../fable_modules/fable-library.3.7.20/Array.js";
import { withStyle, rule } from "../Sutil/src/Sutil/Styling.js";
import { CssEngine$1__color_Z721C83C5, CssEngine$1__get_alignItemsCenter, CssEngine$1__gap_Z445F6BAF, CssEngine$1__custom_Z384F8060, CssEngine$1__get_displayGrid, CssEngine$1__padding_Z445F6BAF, CssEngine$1__fontSize_Z445F6BAF, CssEngine$1__get_overflowAuto, CssEngine$1__height_Z445F6BAF, CssEngine$1__width_Z445F6BAF } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { int32ToString } from "../fable_modules/fable-library.3.7.20/Util.js";
import { EngineHelpers_Html, EngineHelpers_Css } from "../Sutil/src/Sutil/Html.js";
import { empty, singleton, ofArray } from "../fable_modules/fable-library.3.7.20/List.js";
import { Cmd_none } from "../Sutil/src/Sutil/Cmd.js";
import { Store_map, Store_makeElmish } from "../Sutil/src/Sutil/Store.js";
import { UI_divc } from "./Toolbar.js";
import { Bind_el_ZF0512D0 } from "../Sutil/src/Sutil/Bind.js";
import { collect, delay, toList } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { HtmlEngine$1__span_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";

export function applog(s) {
}

export class GridRow extends Record {
    constructor(Label, Category, Editor) {
        super();
        this.Label = Label;
        this.Category = Category;
        this.Editor = Editor;
    }
}

export function GridRow$reflection() {
    return record_type("SutilOxide.PropertyEditor.GridRow", [], GridRow, () => [["Label", string_type], ["Category", string_type], ["Editor", SutilElement$reflection()]]);
}

export function makeCell(onchanged, name, t) {
    console.log(some("makeCell"), name, toString(t));
    const d = (v) => {
        const tupledArg = [name, v];
        onchanged([tupledArg[0], tupledArg[1]]);
    };
    switch (t.tag) {
        case 1: {
            return FloatCell_Create_4547699F((r) => MutableMap$2__Get_2B595(r, name), (r_1, v_1) => {
                MutableMap$2__Set_5BDDA1(r_1, name, v_1);
                d(v_1);
            });
        }
        case 0: {
            return IntCell_Create_Z43AFC5E1((r_2) => MutableMap$2__Get_2B595(r_2, name), (r_3, v_2) => {
                MutableMap$2__Set_5BDDA1(r_3, name, v_2);
                d(v_2);
            });
        }
        case 2: {
            return BoolCell_Create_Z33FC8AA1((r_4) => MutableMap$2__Get_2B595(r_4, name), (r_5, v_3) => {
                MutableMap$2__Set_5BDDA1(r_5, name, v_3);
                d(v_3);
            });
        }
        case 3: {
            return StrCell_Create_Z3903A61((r_6) => MutableMap$2__Get_2B595(r_6, name), (r_7, v_4) => {
                MutableMap$2__Set_5BDDA1(r_7, name, v_4);
                d(v_4);
            });
        }
        default: {
            throw (new Error("Unsupported cell type"));
        }
    }
}

export function makeGridRow(name, bag, cell) {
    return new GridRow(name, "General", view_1(bag, false, cell));
}

export function isConfigType(t) {
    if ((equals(t, int32_type) ? true : equals(t, float64_type)) ? true : equals(t, bool_type)) {
        return true;
    }
    else {
        return equals(t, string_type);
    }
}

export function makeGridRows(bag, fields, dispatch) {
    return map((tupledArg) => {
        const name = tupledArg[0];
        const dtype = tupledArg[1];
        const cell = makeCell(dispatch, name, dtype);
        return makeGridRow(name, bag, cell);
    }, fields);
}

export const peCss = ofArray([rule(".property-editor", ofArray([CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__get_overflowAuto(EngineHelpers_Css), CssEngine$1__fontSize_Z445F6BAF(EngineHelpers_Css, int32ToString(75) + "%"), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, (0.5).toString() + "rem")])), rule(".property-editor .items", ofArray([CssEngine$1__get_displayGrid(EngineHelpers_Css), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "grid-template-columns", "min-content auto"), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, (0.2).toString() + "rem"), CssEngine$1__get_alignItemsCenter(EngineHelpers_Css)])), rule(".property-editor .items\u003espan", singleton(CssEngine$1__color_Z721C83C5(EngineHelpers_Css, "darkblue"))), rule(".property-editor .items\u003e*", empty())]);

export class Model extends Record {
    constructor(Items, Title) {
        super();
        this.Items = Items;
        this.Title = Title;
    }
}

export function Model$reflection() {
    return record_type("SutilOxide.PropertyEditor.Model", [], Model, () => [["Items", list_type(GridRow$reflection())], ["Title", string_type]]);
}

export class ApiMessage extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SetValue"];
    }
}

export function ApiMessage$reflection() {
    return union_type("SutilOxide.PropertyEditor.ApiMessage", [], ApiMessage, () => [[["Item1", string_type], ["Item2", list_type(GridRow$reflection())]]]);
}

export class Message extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Api"];
    }
}

export function Message$reflection() {
    return union_type("SutilOxide.PropertyEditor.Message", [], Message, () => [[["Item", ApiMessage$reflection()]]]);
}

export function init() {
    return [new Model(empty(), "None"), Cmd_none()];
}

export function updateFromApi(msg, model) {
    const title = msg.fields[0];
    const items = msg.fields[1];
    return [new Model(items, title), Cmd_none()];
}

export function update(msg, model) {
    const m = msg.fields[0];
    return updateFromApi(m, model);
}

export function create() {
    const patternInput = Store_makeElmish(init, update, (value) => {
    })();
    const model_1 = patternInput[0];
    const dispatch = patternInput[1];
    const view = withStyle(peCss, UI_divc("property-editor", [Bind_el_ZF0512D0(Store_map((m) => m.Items, model_1), (items) => UI_divc("items", toList(delay(() => collect((item) => [HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, item.Label), item.Editor], items)))))]));
    return [view, (arg_5) => {
        dispatch(new Message(0, arg_5));
    }];
}

