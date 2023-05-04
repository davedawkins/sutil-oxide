import { option_type, int32_type, record_type, union_type, bool_type, string_type, class_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { HtmlEngine$1__input_BB573A, HtmlEngine$1__button_BB573A, HtmlEngine$1__div_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { empty, singleton, append, delay, toList } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { AttrEngine$1__value_Z721C83C5, AttrEngine$1__className_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { EngineHelpers_Ev, EngineHelpers_Css, EngineHelpers_Html, EngineHelpers_Attr } from "../Sutil/src/Sutil/Html.js";
import { Record, Union } from "../fable_modules/fable-library.3.7.20/Types.js";
import { createTypeInfo } from "../fable_modules/Fable.SimpleJson.3.24.0/TypeInfo.Converter.fs.js";
import { Convert_fromJson, Convert_serialize } from "../fable_modules/Fable.SimpleJson.3.24.0/Json.Converter.fs.js";
import { SimpleJson_tryParse } from "../fable_modules/Fable.SimpleJson.3.24.0/SimpleJson.fs.js";
import { Cmd_batch, Cmd_OfFunc_either, Cmd_none, Cmd_ofMsg } from "../Sutil/src/Sutil/Cmd.js";
import { SutilOxide_FileSystem_IFileSystem__IFileSystem_Combine_Static_Z384F8060 } from "./FileSystem.js";
import { singleton as singleton_1, ofArray } from "../fable_modules/fable-library.3.7.20/List.js";
import { withStyle, rule } from "../Sutil/src/Sutil/Styling.js";
import { CssEngine$1__backgroundColor_Z721C83C5, CssEngine$1__paddingLeft_Z445F6BAF, CssEngine$1__padding_Z445F6BAF, CssEngine$1__get_cursorPointer, CssEngine$1__fontSize_Z445F6BAF, CssEngine$1__get_flexDirectionRow, CssEngine$1__gap_Z445F6BAF, CssEngine$1__get_flexDirectionColumn, CssEngine$1__get_displayFlex } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { uncurry, comparePrimitives, int32ToString } from "../fable_modules/fable-library.3.7.20/Util.js";
import { autofocus, fragment, text } from "../Sutil/src/Sutil/CoreElements.js";
import { EventEngine$1__onDblClick_58BC8925, EventEngine$1__onMouseUp_58BC8925, EventEngine$1__onMouseDown_58BC8925, EventEngine$1__onKeyDown_Z2153A397, EventEngine$1__onClick_58BC8925 } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { sortBy, map } from "../fable_modules/fable-library.3.7.20/Array.js";
import { rafu } from "../Sutil/src/Sutil/DomHelpers.js";
import { defaultArgWith, map as map_1, defaultArg } from "../fable_modules/fable-library.3.7.20/Option.js";
import { Store_makeElmish } from "../Sutil/src/Sutil/Store.js";
import { Bind_el_ZF0512D0 } from "../Sutil/src/Sutil/Bind.js";

export class UI {
    constructor() {
    }
}

export function UI$reflection() {
    return class_type("SutilOxide.FileExplorer.UI", void 0, UI);
}

export function UI_divc(cls, items) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, cls)), delay(() => items)))));
}

export class Msg extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SetSelected", "DeleteSelected", "NewFile", "SetError", "ClearError", "SetRenaming", "Created", "RenameTo", "Edit", "Refresh"];
    }
}

export function Msg$reflection() {
    return union_type("SutilOxide.FileExplorer.Msg", [], Msg, () => [[["Item", string_type]], [], [], [["Item", class_type("System.Exception")]], [], [["Item", bool_type]], [["Item", string_type]], [["Item", string_type]], [["Item", string_type]], []]);
}

export class SessionState extends Record {
    constructor(Cwd, Selected, Editing) {
        super();
        this.Cwd = Cwd;
        this.Selected = Selected;
        this.Editing = Editing;
    }
}

export function SessionState$reflection() {
    return record_type("SutilOxide.FileExplorer.SessionState", [], SessionState, () => [["Cwd", string_type], ["Selected", string_type], ["Editing", string_type]]);
}

export class Model extends Record {
    constructor(RefreshId, Cwd, Fs, Selected, Renaming, Error$, Editing) {
        super();
        this.RefreshId = (RefreshId | 0);
        this.Cwd = Cwd;
        this.Fs = Fs;
        this.Selected = Selected;
        this.Renaming = Renaming;
        this.Error = Error$;
        this.Editing = Editing;
    }
}

export function Model$reflection() {
    return record_type("SutilOxide.FileExplorer.Model", [], Model, () => [["RefreshId", int32_type], ["Cwd", string_type], ["Fs", class_type("SutilOxide.FileSystem.IFileSystem")], ["Selected", string_type], ["Renaming", bool_type], ["Error", option_type(class_type("System.Exception"))], ["Editing", string_type]]);
}

export function saveSessionState(m) {
    let value, typeInfo;
    window.localStorage.setItem("file-explorer-session", (value = (new SessionState(m.Cwd, m.Selected, m.Editing)), (typeInfo = createTypeInfo(SessionState$reflection()), Convert_serialize(value, typeInfo))));
}

export function loadSessionState() {
    let matchValue_1, inputJson, typeInfo;
    const matchValue = window.localStorage.getItem("file-explorer-session");
    if (matchValue === null) {
        return void 0;
    }
    else {
        const s = matchValue;
        return (matchValue_1 = SimpleJson_tryParse(s), (matchValue_1 != null) ? ((inputJson = matchValue_1, (typeInfo = createTypeInfo(SessionState$reflection()), Convert_fromJson(inputJson, typeInfo)))) : (() => {
            throw (new Error("Couldn\u0027t parse the input JSON string because it seems to be invalid"));
        })());
    }
}

export function defaultSessionState() {
    return new SessionState("/", "", "");
}

export function init(fs, s) {
    return [new Model(0, s.Cwd, fs, s.Selected, false, void 0, s.Editing), (s.Editing !== "") ? Cmd_ofMsg(new Msg(8, s.Editing)) : Cmd_none()];
}

export function update(edit, msg, model) {
    switch (msg.tag) {
        case 8: {
            const name = msg.fields[0];
            if (name !== "") {
                edit(SutilOxide_FileSystem_IFileSystem__IFileSystem_Combine_Static_Z384F8060(model.Cwd, name));
            }
            return [new Model(model.RefreshId, model.Cwd, model.Fs, model.Selected, model.Renaming, model.Error, name), Cmd_none()];
        }
        case 7: {
            const name_1 = msg.fields[0];
            const tryRename = () => {
                if (model.Selected !== "") {
                    model.Fs.RenameFile(model.Selected, name_1);
                }
            };
            return [new Model(model.RefreshId, model.Cwd, model.Fs, model.Selected, false, model.Error, model.Editing), Cmd_OfFunc_either(tryRename, void 0, () => (new Msg(0, model.Selected)), (arg) => (new Msg(3, arg)))];
        }
        case 5: {
            const z = msg.fields[0];
            return [new Model(model.RefreshId, model.Cwd, model.Fs, model.Selected, z, model.Error, model.Editing), Cmd_none()];
        }
        case 4: {
            return [new Model(model.RefreshId, model.Cwd, model.Fs, model.Selected, model.Renaming, void 0, model.Editing), Cmd_none()];
        }
        case 3: {
            const x = msg.fields[0];
            return [new Model(model.RefreshId, model.Cwd, model.Fs, model.Selected, model.Renaming, x, model.Editing), Cmd_none()];
        }
        case 0: {
            const s = msg.fields[0];
            return [new Model(model.RefreshId, model.Cwd, model.Fs, s, false, model.Error, model.Editing), Cmd_none()];
        }
        case 1: {
            const tryDelete = () => {
                if (model.Selected !== "") {
                    const path = model.Selected;
                    if (model.Fs.IsFile(path)) {
                        model.Fs.RemoveFile(path);
                    }
                    else {
                        throw (new Error("Not a file: " + path));
                    }
                }
            };
            return [model, Cmd_OfFunc_either(tryDelete, void 0, () => (new Msg(4)), (arg_1) => (new Msg(3, arg_1)))];
        }
        case 6: {
            const name_2 = msg.fields[0];
            return [model, Cmd_batch(ofArray([Cmd_ofMsg(new Msg(0, name_2)), Cmd_ofMsg(new Msg(5, true))]))];
        }
        case 2: {
            const tryCreate = () => {
                const name_3 = "NewFile.md";
                model.Fs.CreateFile(model.Cwd, name_3);
                return SutilOxide_FileSystem_IFileSystem__IFileSystem_Combine_Static_Z384F8060(model.Cwd, name_3);
            };
            return [model, Cmd_OfFunc_either(tryCreate, void 0, (arg_2) => (new Msg(6, arg_2)), (arg_3) => (new Msg(3, arg_3)))];
        }
        default: {
            return [new Model(model.RefreshId + 1, model.Cwd, model.Fs, model.Selected, model.Renaming, model.Error, model.Editing), Cmd_none()];
        }
    }
}

export function updateWithSaveSession(edit, msg, model) {
    const result = update(edit, msg, model);
    saveSessionState(result[0]);
    return result;
}

export const css = ofArray([rule(".file-explorer", ofArray([CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, (0.5).toString() + "rem")])), rule(".file-explorer-buttons", ofArray([CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionRow(EngineHelpers_Css), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, (0.15).toString() + "rem")])), rule(".file-explorer-entries", ofArray([CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, (0.2).toString() + "rem"), CssEngine$1__fontSize_Z445F6BAF(EngineHelpers_Css, int32ToString(12) + "px")])), rule(".fx-folder", ofArray([CssEngine$1__get_cursorPointer(EngineHelpers_Css), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, int32ToString(2) + "px"), CssEngine$1__paddingLeft_Z445F6BAF(EngineHelpers_Css, (0.5).toString() + "rem")])), rule(".fx-file", ofArray([CssEngine$1__get_cursorPointer(EngineHelpers_Css), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, int32ToString(2) + "px"), CssEngine$1__paddingLeft_Z445F6BAF(EngineHelpers_Css, (0.5).toString() + "rem")])), rule(".file-explorer-entries .selected", singleton_1(CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "#DDDDDD")))]);

export function buttons(m, dispatch) {
    return UI_divc("file-explorer-buttons", [HtmlEngine$1__button_BB573A(EngineHelpers_Html, [text("Up"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg) => {
    })]), HtmlEngine$1__button_BB573A(EngineHelpers_Html, [text("New File"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_1) => {
        dispatch(new Msg(2));
    })]), HtmlEngine$1__button_BB573A(EngineHelpers_Html, [text("New Folder"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_2) => {
    })]), HtmlEngine$1__button_BB573A(EngineHelpers_Html, [text("Rename"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_3) => {
        dispatch(new Msg(5, true));
    })]), HtmlEngine$1__button_BB573A(EngineHelpers_Html, [text("Edit"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_4) => {
        dispatch(new Msg(8, m.Selected));
    })]), HtmlEngine$1__button_BB573A(EngineHelpers_Html, [text("Delete"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_5) => {
        dispatch(new Msg(1));
    })])]);
}

export function fileExplorer(dispatch, m) {
    const cwd = m.Cwd;
    const fs = m.Fs;
    return withStyle(css, UI_divc("file-explorer", [UI_divc("file-explorer-entries", [fragment(map((name) => {
        const path = SutilOxide_FileSystem_IFileSystem__IFileSystem_Combine_Static_Z384F8060(cwd, name);
        return UI_divc("fx-folder", toList(delay(() => append(singleton(text(name)), delay(() => append(singleton(EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg) => {
            dispatch(new Msg(0, path));
        })), delay(() => ((m.Selected === path) ? singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "selected")) : empty()))))))));
    }, fs.Folders(cwd))), fragment(map((name_1) => {
        const path_1 = SutilOxide_FileSystem_IFileSystem__IFileSystem_Combine_Static_Z384F8060(cwd, name_1);
        if ((m.Selected === path_1) && m.Renaming) {
            return HtmlEngine$1__input_BB573A(EngineHelpers_Html, [autofocus, AttrEngine$1__value_Z721C83C5(EngineHelpers_Attr, name_1), EventEngine$1__onKeyDown_Z2153A397(EngineHelpers_Ev, (e) => {
                const value = e.target.value;
                const matchValue = e.key;
                switch (matchValue) {
                    case "Enter": {
                        if ((value !== "") && (value !== name_1)) {
                            dispatch(new Msg(7, value));
                        }
                        else {
                            dispatch(new Msg(5, false));
                        }
                        break;
                    }
                    case "Escape": {
                        dispatch(new Msg(5, false));
                        break;
                    }
                    default: {
                    }
                }
            })]);
        }
        else {
            return UI_divc("fx-file", toList(delay(() => append(singleton(text(name_1)), delay(() => append(singleton(EventEngine$1__onMouseDown_58BC8925(EngineHelpers_Ev, (e_1) => {
                e_1.stopPropagation();
                e_1.preventDefault();
            })), delay(() => append(singleton(EventEngine$1__onMouseUp_58BC8925(EngineHelpers_Ev, (e_2) => {
                rafu(() => {
                    dispatch(new Msg(0, path_1));
                });
            })), delay(() => append(singleton(EventEngine$1__onDblClick_58BC8925(EngineHelpers_Ev, (e_3) => {
                dispatch(new Msg(8, m.Selected));
            })), delay(() => ((m.Selected === path_1) ? singleton(AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "selected")) : empty()))))))))))));
        }
    }, sortBy((s) => s.toLocaleLowerCase(), fs.Files(cwd), {
        Compare: comparePrimitives,
    })))]), UI_divc("file-explorer-error", [text(defaultArg(map_1((e_4) => e_4.message, m.Error), ""))])]));
}

export class FileExplorer {
    constructor(fs) {
        this.onEdit = ((value) => {
        });
        const patternInput = FileExplorer__create_42540B84(this, fs);
        this.model = patternInput[0];
        this.dispatch = patternInput[1];
        fs.OnChange((_arg) => {
            this.dispatch(new Msg(9));
        });
    }
}

export function FileExplorer$reflection() {
    return class_type("SutilOxide.FileExplorer.FileExplorer", void 0, FileExplorer);
}

export function FileExplorer_$ctor_42540B84(fs) {
    return new FileExplorer(fs);
}

export function FileExplorer__get_View(_) {
    return FileExplorer__view(_);
}

export function FileExplorer__OnEdit_41EFD311(_, h) {
    _.onEdit = h;
}

export function FileExplorer__get_Dispatch(_) {
    return _.dispatch;
}

function FileExplorer__create_42540B84(this$, fs) {
    let edit;
    const sessionState = defaultArgWith(loadSessionState(), defaultSessionState);
    const patternInput = Store_makeElmish((tupledArg) => init(tupledArg[0], tupledArg[1]), uncurry(2, (edit = this$.onEdit, (msg) => ((model) => updateWithSaveSession(edit, msg, model)))), (value) => {
    })([fs, sessionState]);
    const model_1 = patternInput[0];
    const dispatch = patternInput[1];
    return [model_1, dispatch];
}

function FileExplorer__view(this$) {
    return Bind_el_ZF0512D0(this$.model, (m) => fileExplorer(this$.dispatch, m));
}

