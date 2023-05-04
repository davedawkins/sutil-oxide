import * as ace from "ace-builds/src-min-noconflict/ace.js";
import { some } from "./fable_modules/fable-library.3.7.20/Option.js";
import { class_type } from "./fable_modules/fable-library.3.7.20/Reflection.js";
import { HtmlEngine$1__div_BB573A } from "./fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { EngineHelpers_Html, EngineHelpers_Ev, EngineHelpers_Attr, EngineHelpers_Css, SutilAttrEngine__style_68BDC580 } from "./Sutil/src/Sutil/Html.js";
import { CssEngine$1__height_Z445F6BAF, CssEngine$1__width_Z445F6BAF } from "./fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { int32ToString } from "./fable_modules/fable-library.3.7.20/Util.js";
import { AttrEngine$1__id_Z721C83C5 } from "./fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { EventEngine$1__onKeyDown_Z2153A397 } from "./fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { onMount } from "./Sutil/src/Sutil/CoreElements.js";
import { empty } from "./fable_modules/fable-library.3.7.20/List.js";
import { SutilOxide_FileSystem_IFileSystem__IFileSystem_GetExtension_Static_Z721C83C5 } from "./SutilOxide/FileSystem.js";
import { rafu } from "./Sutil/src/Sutil/DomHelpers.js";

export const AceSdk = ace;

export const config = AceSdk.config;

export function initAce(hostElement, onChange) {
    config.set("basePath", some("/ace-builds/src-min-noconflict"));
    const editor = AceSdk.edit(hostElement, some({
        basePath: "/ace-builds/src-min-noconflict",
    }));
    editor.setTheme("ace/theme/textmate");
    editor.session.setMode("ace/mode/text");
    editor.on('change',((_arg) => {
        onChange();
    }));
    return editor;
}

export class Editor {
    constructor(fs) {
        this.fs = fs;
        this.editor = null;
        this.editing = "";
        this.onEditedChange = ((value) => {
        });
    }
}

export function Editor$reflection() {
    return class_type("TextEditor.Editor", void 0, Editor);
}

export function Editor_$ctor_42540B84(fs) {
    return new Editor(fs);
}

export function Editor__get_View(_) {
    return Editor__createDiv(_);
}

export function Editor__get_Editor(_) {
    return _.editor;
}

export function Editor__Open_Z721C83C5(_, path) {
    Editor__load_Z721C83C5(_, path);
}

export function Editor__Save(_) {
    Editor__save(_);
}

export function Editor__OnEditedChange_50F94480(_, handler) {
    _.onEditedChange = handler;
}

export function Editor__get_Text(_) {
    return _.editor.getValue();
}

function Editor__save(this$) {
    if (this$.editing !== "") {
        this$.fs.SetFileContent(this$.editing, this$.editor.getValue());
        this$.onEditedChange(false);
    }
}

function Editor__createDiv(this$) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")]), AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, "editor"), EventEngine$1__onKeyDown_Z2153A397(EngineHelpers_Ev, (e) => {
        if ((e.ctrlKey ? true : e.metaKey) && (e.key === "s")) {
            Editor__save(this$);
            e.preventDefault();
        }
    }), onMount((e_1) => {
        this$.editor = initAce(e_1.target, () => {
            this$.onEditedChange(true);
        });
    }, empty())]);
}

function Editor__startEdit(this$, e, fs, path) {
    e.setValue(fs.GetFileContent(path), -1);
    e.session.getUndoManager().reset();
    const matchValue = SutilOxide_FileSystem_IFileSystem__IFileSystem_GetExtension_Static_Z721C83C5(path);
    switch (matchValue) {
        case ".css": {
            e.session.setMode("ace/mode/css");
            break;
        }
        case ".js": {
            e.session.setMode("ace/mode/javascript");
            break;
        }
        case ".html": {
            e.session.setMode("ace/mode/html");
            break;
        }
        case ".cfg":
        case ".json":
        case "":
        case ".proj": {
            e.session.setMode("ace/mode/json");
            break;
        }
        case ".md": {
            e.session.setMode("ace/mode/markdown");
            break;
        }
        default: {
            e.session.setMode("ace/mode/text");
        }
    }
}

function Editor__load_Z721C83C5(this$, path) {
    if (this$.editor == null) {
        rafu(() => {
            Editor__load_Z721C83C5(this$, path);
        });
    }
    else {
        this$.editing = path;
        Editor__startEdit(this$, this$.editor, this$.fs, path);
    }
}

