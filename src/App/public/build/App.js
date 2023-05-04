import { Record, Union } from "./fable_modules/fable-library.3.7.20/Types.js";
import { lambda_type, unit_type, bool_type, option_type, string_type, record_type, class_type, union_type } from "./fable_modules/fable-library.3.7.20/Reflection.js";
import { equals, comparePrimitives, int32ToString, createAtom } from "./fable_modules/fable-library.3.7.20/Util.js";
import { FSharpMap__get_Values, ofSeq, FSharpMap__get_Item, FSharpMap__Add, FSharpMap__ContainsKey, empty } from "./fable_modules/fable-library.3.7.20/Map.js";
import { Store_subscribe, Store_makeElmish, Store_getMap, Store_set, Store_map, Store_make } from "./Sutil/src/Sutil/Store.js";
import { FlowChart__Render_3573D436, Types_GraphOptions, FlowChart_$ctor_Z3846D57A, Types_GraphOptions_Create, Views_makeCatalogItem, Types_Graph, Types_Edge_Create_6BD52AFB, Types_Node, Types_Node_Create_3D3F00C0, Types_Graph$reflection } from "./SutilOxide/Flow.js";
import { appendHandler, log as log_1 } from "./SutilOxide/Logging.js";
import { PromiseBuilder__While_2044D34, PromiseBuilder__Delay_62FBFDE1, PromiseBuilder__Run_212F1D4B } from "./fable_modules/Fable.Promise.3.2.0/Promise.fs.js";
import { promise } from "./fable_modules/Fable.Promise.3.2.0/PromiseImpl.fs.js";
import { fetch$ } from "./fable_modules/Fable.Fetch.2.6.0/Fetch.fs.js";
import { map, singleton, ofArray, empty as empty_1 } from "./fable_modules/fable-library.3.7.20/List.js";
import { Cmd_ofMsg, Cmd_none, Cmd_OfPromise_perform } from "./Sutil/src/Sutil/Cmd.js";
import { ModalOptions, ModalOptions_Create, modal } from "./SutilOxide/Modal.js";
import { HtmlEngine$1__span_Z721C83C5, HtmlEngine$1__textarea_BB573A, HtmlEngine$1__div_BB573A, HtmlEngine$1__div_Z721C83C5 } from "./fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { SutilHtmlEngine__span_Z686281E5, SutilHtmlEngine__divc, EngineHelpers_Attr, SutilAttrEngine__style_68BDC580, EngineHelpers_Css, EngineHelpers_Html } from "./Sutil/src/Sutil/Html.js";
import { Editor__Save, Editor__OnEditedChange_50F94480, Editor_$ctor_42540B84, Editor__get_View, Editor__Open_Z721C83C5, Editor__get_Text } from "./TextEditor.js";
import { withStyle, rule } from "./Sutil/src/Sutil/Styling.js";
import { CssEngine$1__get_cursorPointer, CssEngine$1__color_Z721C83C5, CssEngine$1__borderRadius_Z445F6BAF, CssEngine$1__border_Z6C024E7B, CssEngine$1__get_displayInlineFlex, CssEngine$1__gap_Z445F6BAF, CssEngine$1__get_alignContentFlexStart, CssEngine$1__get_alignItemsFlexStart, CssEngine$1__get_flexWrapWrap, CssEngine$1__get_flexDirectionRow, CssEngine$1__get_resizeNone, CssEngine$1__margin_Z524259A4, CssEngine$1__get_borderStyleNone, CssEngine$1__get_textDecorationLineUnderline, CssEngine$1__custom_Z384F8060, CssEngine$1__padding_Z445F6BAF, CssEngine$1__fontFamily_Z721C83C5, CssEngine$1__backgroundColor_Z721C83C5, CssEngine$1__borderTopStyle_61CE138F, CssEngine$1__borderTopWidth_Z445F6BAF, CssEngine$1__borderWidth_Z524259A4, CssEngine$1__width_Z445F6BAF, CssEngine$1__height_Z445F6BAF, CssEngine$1__get_flexDirectionColumn, CssEngine$1__get_displayFlex } from "./fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { hookParent, html, text as text_2 } from "./Sutil/src/Sutil/CoreElements.js";
import { Markdown_Parse_Z33F6198B, Markdown_ToHtml_Z6BE9E069 } from "./fable_modules/Fable.Formatting.Markdown.1.0.1/Markdown.fs.js";
import { AttrEngine$1__readOnly_Z1FBCCD16, AttrEngine$1__id_Z721C83C5, AttrEngine$1__className_Z721C83C5 } from "./fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { Bind_attr_3F2394B8, Bind_promise_Z6B94AFE8, Bind_el_ZF0512D0 } from "./Sutil/src/Sutil/Bind.js";
import { distinctUntilChanged } from "./Sutil/src/Sutil/Observable.js";
import { subscribe } from "./fable_modules/fable-library.3.7.20/Observable.js";
import { interval, rafu } from "./Sutil/src/Sutil/DomHelpers.js";
import { SutilEffect_RegisterDisposable_5FAE877D } from "./Sutil/src/Sutil/Core.js";
import { toLongTimeString, now } from "./fable_modules/fable-library.3.7.20/Date.js";
import { map as map_1, delay, toList } from "./fable_modules/fable-library.3.7.20/Seq.js";
import { LocalStorageFileSystem_$ctor_Z721C83C5, SutilOxide_FileSystem_IFileSystem__IFileSystem_GetFileName_Static_Z721C83C5 } from "./SutilOxide/FileSystem.js";
import { DockContainer__View_79A576A0, DockProperty, DockContainer__SetProperty_2F023AF1, Options_Create, DockContainer_$ctor_Z3F65FBC2, DockContainer__AddPane_1858B09, DockContainer__AddPane_71369329, DockContainer__AddPane_Z41690A9D } from "./SutilOxide/Dock.js";
import { DockLocation } from "./SutilOxide/Types.js";
import { Msg, FileExplorer__get_Dispatch, FileExplorer_$ctor_42540B84, FileExplorer__get_View } from "./SutilOxide/FileExplorer.js";
import { installStyling, LightTheme, DarkTheme } from "./SutilOxide/Css.js";
import { gap, statusbar, checkItem, menuItem, hseparator, buttonItem, ButtonProperty, dropDownItem, toolbar } from "./SutilOxide/Toolbar.js";
import { Program_mount_6E602840 } from "./Sutil/src/Sutil/Program.js";

export class Theme extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Light", "Dark"];
    }
}

export function Theme$reflection() {
    return union_type("App.Theme", [], Theme, () => [[], []]);
}

export class AppContext extends Record {
    constructor(Fs) {
        super();
        this.Fs = Fs;
    }
}

export function AppContext$reflection() {
    return record_type("App.AppContext", [], AppContext, () => [["Fs", class_type("SutilOxide.FileSystem.IFileSystem")]]);
}

export let nodeStores = createAtom(empty());

export function getNodeStore(name) {
    if (!FSharpMap__ContainsKey(nodeStores(), name)) {
        nodeStores(FSharpMap__Add(nodeStores(), name, Store_make("")), true);
    }
    return FSharpMap__get_Item(nodeStores(), name);
}

export function clearNodeStores() {
    nodeStores(empty(), true);
}

export class Model extends Record {
    constructor(PreviewText, Theme, Editing, NeedsSave, Log, Graph) {
        super();
        this.PreviewText = PreviewText;
        this.Theme = Theme;
        this.Editing = Editing;
        this.NeedsSave = NeedsSave;
        this.Log = Log;
        this.Graph = Graph;
    }
}

export function Model$reflection() {
    return record_type("App.Model", [], Model, () => [["PreviewText", string_type], ["Theme", Theme$reflection()], ["Editing", option_type(string_type)], ["NeedsSave", bool_type], ["Log", string_type], ["Graph", Types_Graph$reflection()]]);
}

export const lorem = "Nunc dapibus tempus sapien, vitae efficitur nunc posuere non. Suspendisse in placerat turpis, at sodales nisl. Etiam in tempus nulla. Praesent sed interdum ligula. Sed non nisl est. Praesent vel metus magna. Morbi eget mi est. Nam volutpat purus ligula, ut convallis libero rhoncus ac. ";

export class Message extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["AppendToLog", "Nop", "SetTheme", "SetPreviewText", "SimpleProgressBar", "Edit", "SaveEdits", "SetEdited", "DeleteFile", "SetGraph"];
    }
}

export function Message$reflection() {
    return union_type("App.Message", [], Message, () => [[["Item", string_type]], [], [["Item", Theme$reflection()]], [["Item", string_type]], [], [["Item", string_type]], [], [["Item", bool_type]], [["Item1", bool_type], ["Item2", lambda_type(unit_type, unit_type)]], [["Item", Types_Graph$reflection()]]]);
}

export const log = (s) => {
    log_1(s);
};

export function fetchSource(url) {
    return PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (fetch$(url, empty_1()).then((_arg) => {
        const res = _arg;
        return res.text();
    }))));
}

export function uploadFile(url, targetFileName, fs) {
    return PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (fetchSource(url).then((_arg) => {
        const content = _arg;
        fs.SetFileContent(targetFileName, content);
        return Promise.resolve();
    }))));
}

export function init(app, graph) {
    return [new Model("(no markdown to preview)", new Theme(0), void 0, false, "", graph), Cmd_OfPromise_perform((fs) => uploadFile("README.md", "README.md", fs), app.Fs, () => (new Message(5, "README.md")))];
}

export function update(app, textEditor, msg, model) {
    switch (msg.tag) {
        case 0: {
            const m = msg.fields[0];
            return [new Model(model.PreviewText, model.Theme, model.Editing, model.NeedsSave, (model.Log + m) + "\n", model.Graph), Cmd_none()];
        }
        case 8: {
            const delete$ = msg.fields[1];
            const confirmed = msg.fields[0];
            const confirm = (dispatch) => {
                let inputRecord;
                modal((inputRecord = ModalOptions_Create(), new ModalOptions(inputRecord.ShowCancel, inputRecord.OnCancel, ofArray([["Cancel", (close_1) => {
                    close_1();
                }], ["Delete", (close_2) => {
                    close_2();
                    dispatch(new Message(8, true, delete$));
                }]]), (close) => HtmlEngine$1__div_Z721C83C5(EngineHelpers_Html, "Confirm delete?"))));
            };
            if (confirmed) {
                delete$();
                return [model, Cmd_none()];
            }
            else {
                return [model, singleton(confirm)];
            }
        }
        case 6: {
            return [model, Cmd_none()];
        }
        case 7: {
            const z = msg.fields[0];
            const cmd = z ? Cmd_ofMsg(new Message(3, Editor__get_Text(textEditor))) : Cmd_none();
            return [new Model(model.PreviewText, model.Theme, model.Editing, z, model.Log, model.Graph), cmd];
        }
        case 1: {
            return [model, Cmd_none()];
        }
        case 2: {
            const t = msg.fields[0];
            return [new Model(model.PreviewText, t, model.Editing, model.NeedsSave, model.Log, model.Graph), Cmd_none()];
        }
        case 3: {
            const s = msg.fields[0];
            return [new Model(s, model.Theme, model.Editing, model.NeedsSave, model.Log, model.Graph), Cmd_none()];
        }
        case 4: {
            const cmd_1 = Cmd_none();
            return [model, cmd_1];
        }
        case 5: {
            const name = msg.fields[0];
            Editor__Open_Z721C83C5(textEditor, name);
            return [new Model(model.PreviewText, model.Theme, name, false, model.Log, model.Graph), Cmd_ofMsg(new Message(3, app.Fs.GetFileContent(name)))];
        }
        default: {
            const g = msg.fields[0];
            return [new Model(model.PreviewText, model.Theme, model.Editing, model.NeedsSave, model.Log, g), Cmd_none()];
        }
    }
}

export const appCss = ofArray([rule(".main-container", ofArray([CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "vh"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "vw")])), rule(".status-footer", ofArray([CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, (1.5).toString() + "rem"), CssEngine$1__borderWidth_Z524259A4(EngineHelpers_Css, 0), CssEngine$1__borderTopWidth_Z445F6BAF(EngineHelpers_Css, int32ToString(1) + "px"), CssEngine$1__borderTopStyle_61CE138F(EngineHelpers_Css, "solid")]))]);

export function dummy(name, colour) {
    return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, colour), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(2000) + "px")]), text_2("Example pane")]);
}

export const mdCss = ofArray([rule(".md", ofArray([CssEngine$1__fontFamily_Z721C83C5(EngineHelpers_Css, "Courier New"), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "hsl(53.2, 100%, 91.4%)"), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, (0.5).toString() + "rem"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__custom_Z384F8060(EngineHelpers_Css, "height", "auto")])), rule(".md a", singleton(CssEngine$1__get_textDecorationLineUnderline(EngineHelpers_Css)))]);

export function markdownToHtml(md) {
    try {
        return Markdown_ToHtml_Z6BE9E069(Markdown_Parse_Z33F6198B(md));
    }
    catch (x) {
        return `<pre>${x}</pre>`;
    }
}

export function bindMd(markdown) {
    return withStyle(mdCss, HtmlEngine$1__div_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "md"), Bind_el_ZF0512D0(Store_map(markdownToHtml, markdown), html)]));
}

export function bindUrl(url, view_1) {
    return Bind_promise_Z6B94AFE8(fetchSource(url), view_1);
}

export function viewMd(url) {
    return bindUrl(url, (text) => withStyle(mdCss, HtmlEngine$1__div_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "md"), html(markdownToHtml(text))])));
}

export const dummyColor = "transparent";

export const logStyle = ofArray([CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__fontFamily_Z721C83C5(EngineHelpers_Css, "\u0027Courier New\u0027, Courier, monospace"), CssEngine$1__get_borderStyleNone(EngineHelpers_Css), CssEngine$1__margin_Z524259A4(EngineHelpers_Css, 0), CssEngine$1__get_resizeNone(EngineHelpers_Css)]);

export function mainLog(model) {
    const logS = distinctUntilChanged(Store_map((m) => m.Log, model));
    return HtmlEngine$1__textarea_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, logStyle), AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, "log"), AttrEngine$1__readOnly_Z1FBCCD16(EngineHelpers_Attr, true), Bind_attr_3F2394B8("value", logS), hookParent((n) => {
        const e = n;
        const stop = subscribe((_arg) => {
            rafu(() => {
                e.setSelectionRange(99999, 99999);
            });
        }, logS);
        SutilEffect_RegisterDisposable_5FAE877D(n, stop);
    })]);
}

export const exampleGraph = new Types_Graph(ofSeq([["n1", Types_Node_Create_3D3F00C0("n1", "Hello", 250, 50)], ["n2", Types_Node_Create_3D3F00C0("n2", "World", 250, 150)], ["n3", (() => {
    const inputRecord = Types_Node_Create_3D3F00C0("n3", "No Select", 50, 50);
    return new Types_Node(inputRecord.Id, inputRecord.Type, inputRecord.X, inputRecord.Y, inputRecord.Width, inputRecord.Height, inputRecord.ZIndex, inputRecord.ClassName, inputRecord.SourceLocation, inputRecord.TargetLocation, false, inputRecord.CanMove);
})()], ["n4", (() => {
    const inputRecord_1 = Types_Node_Create_3D3F00C0("n4", "No Move", 50, 150);
    return new Types_Node(inputRecord_1.Id, "", inputRecord_1.X, inputRecord_1.Y, inputRecord_1.Width, inputRecord_1.Height, inputRecord_1.ZIndex, inputRecord_1.ClassName, inputRecord_1.SourceLocation, inputRecord_1.TargetLocation, inputRecord_1.CanSelect, false);
})()]], {
    Compare: comparePrimitives,
}), ofSeq([["e1", Types_Edge_Create_6BD52AFB("e1", "n1", "out", "n2", "in")]], {
    Compare: comparePrimitives,
}));

export const catalogStyle = ofArray([rule(".flow-catalog", ofArray([CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionRow(EngineHelpers_Css), CssEngine$1__get_flexWrapWrap(EngineHelpers_Css), CssEngine$1__get_alignItemsFlexStart(EngineHelpers_Css), CssEngine$1__get_alignContentFlexStart(EngineHelpers_Css), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, int32ToString(1) + "rem"), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, int32ToString(1) + "rem"), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "white"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%"), CssEngine$1__width_Z445F6BAF(EngineHelpers_Css, int32ToString(100) + "%")])), rule(".flow-catalog-item", ofArray([CssEngine$1__get_displayInlineFlex(EngineHelpers_Css), CssEngine$1__border_Z6C024E7B(EngineHelpers_Css, int32ToString(1) + "px", "solid", "#888888"), CssEngine$1__borderRadius_Z445F6BAF(EngineHelpers_Css, int32ToString(4) + "px"), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "white"), CssEngine$1__color_Z721C83C5(EngineHelpers_Css, "black"), CssEngine$1__height_Z445F6BAF(EngineHelpers_Css, "auto"), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, (0.35).toString() + "rem"), CssEngine$1__get_cursorPointer(EngineHelpers_Css)]))]);

export function catalogItem(name) {
    return SutilHtmlEngine__divc(EngineHelpers_Html, "flow-catalog-item", [text_2(name)]);
}

export const catalogTypes = ofArray(["Start", "Pause", "Rewind", "Forward", "Reset", "Clock", "Stop"]);

export const clockS = Store_make(now());

interval(() => {
    Store_set(clockS, now());
}, 500);

export function flowCatalog() {
    return withStyle(catalogStyle, SutilHtmlEngine__divc(EngineHelpers_Html, "flow-catalog", toList(delay(() => map((t) => Views_makeCatalogItem(t, catalogItem(t)), catalogTypes)))));
}

export function flowGraph(graph, dispatch) {
    const options$0027 = Types_GraphOptions_Create();
    const nodeFactory = (tupledArg) => {
        const name = tupledArg[0];
        const typ = tupledArg[1];
        return options$0027.NodeFactory([name, typ]);
    };
    const viewNode = (node) => {
        if (node.Id.indexOf("Clock") === 0) {
            return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [Bind_el_ZF0512D0(clockS, (c) => HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, toLongTimeString(c)))]);
        }
        else if (node.Id.indexOf("Start") === 0) {
            return HtmlEngine$1__div_BB573A(EngineHelpers_Html, [Bind_el_ZF0512D0(getNodeStore(node.Id), (arg) => HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, arg))]);
        }
        else {
            return options$0027.ViewNode(node);
        }
    };
    const fc = FlowChart_$ctor_Z3846D57A(new Types_GraphOptions(options$0027.SnapToGrid, options$0027.SnapToGridSize, options$0027.Css, options$0027.NodePorts, nodeFactory, options$0027.EdgeFactory, viewNode, (arg_2) => {
        dispatch(new Message(9, arg_2));
    }, options$0027.OnSelectionChange));
    return FlowChart__Render_3573D436(fc, graph);
}

export function runGraph(g) {
    const threads = map_1((n) => {
        if (n.Id.indexOf("Start") === 0) {
            return PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => {
                let i = 0;
                return PromiseBuilder__While_2044D34(promise, () => true, PromiseBuilder__Delay_62FBFDE1(promise, () => ((new Promise(resolve => setTimeout(resolve, 1000))).then(() => {
                    i = ((i + 1) | 0);
                    const newValue = int32ToString(i);
                    Store_set(getNodeStore(n.Id), newValue);
                    return Promise.resolve();
                }))));
            }));
        }
        else {
            return PromiseBuilder__Run_212F1D4B(promise, PromiseBuilder__Delay_62FBFDE1(promise, () => (Promise.resolve(undefined))));
        }
    }, FSharpMap__get_Values(g.Nodes));
    const pr_1 = Promise.all(threads);
    void pr_1;
}

export function initPanes(fileExplorer, textEditor, model, dispatch, dc) {
    const editorTitle = (model_1) => SutilHtmlEngine__span_Z686281E5(EngineHelpers_Html, Store_map((m) => {
        const matchValue = m.Editing;
        if (matchValue != null) {
            const fileName = matchValue;
            return SutilOxide_FileSystem_IFileSystem__IFileSystem_GetFileName_Static_Z721C83C5(fileName) + (m.NeedsSave ? " (edited)" : "");
        }
        else {
            return "Editor";
        }
    }, model_1));
    DockContainer__AddPane_Z41690A9D(dc, "Explorer", new DockLocation(0), FileExplorer__get_View(fileExplorer));
    DockContainer__AddPane_71369329(dc, "Database", new DockLocation(0), dummy("Database", "hsl(43, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Solution", new DockLocation(0), dummy("Solution", "hsl(43, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Insights", new DockLocation(1), dummy("Insights", "hsl(80, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Translation", new DockLocation(1), dummy("Translation", "hsl(43, 100%, 95%)"), false);
    DockContainer__AddPane_Z41690A9D(dc, "Instructions", new DockLocation(5), dummy("Instructions", "hsl(43, 100%, 95%)"));
    DockContainer__AddPane_71369329(dc, "Links", new DockLocation(6), dummy("Links", "hsl(240, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Objects", new DockLocation(6), dummy("Objects", "hsl(43, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Console", new DockLocation(2), dummy("Console", "hsl(160, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Messages", new DockLocation(2), mainLog(model), true);
    DockContainer__AddPane_71369329(dc, "Knowledgebase", new DockLocation(4), dummy("Knowledgebase", "hsl(43, 100%, 95%)"), false);
    DockContainer__AddPane_71369329(dc, "Help", new DockLocation(4), viewMd("HELP.md"), false);
    DockContainer__AddPane_71369329(dc, "Preview", new DockLocation(5), bindMd(Store_map((m_1) => m_1.PreviewText, model)), true);
    DockContainer__AddPane_1858B09(dc, "Ace", new DockLocation(3), editorTitle(model), Editor__get_View(textEditor), true);
    DockContainer__AddPane_71369329(dc, "Catalog", new DockLocation(0), flowCatalog(), true);
    DockContainer__AddPane_71369329(dc, "Graph", new DockLocation(3), flowGraph(Store_getMap((m_2) => m_2.Graph, model), dispatch), true);
}

export function view() {
    const dc = DockContainer_$ctor_Z3F65FBC2(Options_Create());
    const app = new AppContext(LocalStorageFileSystem_$ctor_Z721C83C5("oxide-demo"));
    const graph = exampleGraph;
    const textEditor = Editor_$ctor_42540B84(app.Fs);
    const fileExplorer = FileExplorer_$ctor_42540B84(app.Fs);
    const patternInput = Store_makeElmish((tupledArg) => init(tupledArg[0], tupledArg[1]), (msg, model) => update(app, textEditor, msg, model), (value) => {
    })([app, graph]);
    const model_1 = patternInput[0];
    const dispatch = patternInput[1];
    Editor__OnEditedChange_50F94480(textEditor, (arg_1) => {
        dispatch(new Message(7, arg_1));
    });
    appendHandler((arg_3) => {
        dispatch(new Message(0, arg_3));
    });
    const timeS = Store_make("");
    const stopClock = interval(() => {
        let copyOfStruct;
        Store_set(timeS, (copyOfStruct = now(), toLongTimeString(copyOfStruct)));
    }, 1000);
    let styleCleanup = () => {
    };
    Store_subscribe((t_1) => {
        styleCleanup();
        const theme = (t_1.tag === 1) ? DarkTheme : LightTheme;
        styleCleanup = installStyling(theme);
    }, distinctUntilChanged(Store_map((t) => t.Theme, model_1)));
    log("SutilOxide Demo started");
    return withStyle(appCss, HtmlEngine$1__div_BB573A(EngineHelpers_Html, [AttrEngine$1__className_Z721C83C5(EngineHelpers_Attr, "main-container"), toolbar(empty_1(), [dropDownItem([new ButtonProperty(2, "File")], [buttonItem([new ButtonProperty(2, "New File"), new ButtonProperty(3, "fa-file-o"), new ButtonProperty(4, (e) => {
        FileExplorer__get_Dispatch(fileExplorer)(new Msg(2));
    })]), buttonItem([new ButtonProperty(2, "Save"), new ButtonProperty(3, "fa-save"), new ButtonProperty(4, (e_1) => {
        Editor__Save(textEditor);
    })]), buttonItem([new ButtonProperty(2, "Rename"), new ButtonProperty(3, "fa-i-cursor"), new ButtonProperty(4, (e_2) => {
        FileExplorer__get_Dispatch(fileExplorer)(new Msg(5, true));
    })]), hseparator, buttonItem([new ButtonProperty(2, "Delete"), new ButtonProperty(3, "fa-trash-o"), new ButtonProperty(4, (e_3) => {
        const deleteSelected = () => {
            FileExplorer__get_Dispatch(fileExplorer)(new Msg(1));
        };
        dispatch(new Message(8, false, deleteSelected));
    })])]), dropDownItem([new ButtonProperty(2, "View")], [Bind_el_ZF0512D0(Store_map((m) => m.Theme, model_1), (t_2) => menuItem([new ButtonProperty(2, "Theme")], [checkItem([new ButtonProperty(2, "Light"), new ButtonProperty(6, equals(t_2, new Theme(0))), new ButtonProperty(5, (b) => {
        if (b) {
            dispatch(new Message(2, new Theme(0)));
        }
    })]), checkItem([new ButtonProperty(2, "Dark"), new ButtonProperty(6, equals(t_2, new Theme(1))), new ButtonProperty(5, (b_1) => {
        if (b_1) {
            dispatch(new Message(2, new Theme(1)));
        }
    })])]))]), buttonItem([new ButtonProperty(2, "Run"), new ButtonProperty(3, "fa-play"), new ButtonProperty(4, (_arg_1) => {
        runGraph(Store_getMap((m_1) => m_1.Graph, model_1));
    })]), buttonItem([new ButtonProperty(2, "Help"), new ButtonProperty(3, "fa-life-ring"), new ButtonProperty(4, (_arg_2) => {
        DockContainer__SetProperty_2F023AF1(dc, "Help", new DockProperty(0, true));
    })])]), DockContainer__View_79A576A0(dc, (dc_1) => {
        initPanes(fileExplorer, textEditor, model_1, dispatch, dc_1);
    }), statusbar(empty_1(), [text_2("Time:"), gap, Bind_el_ZF0512D0(timeS, (arg_4) => HtmlEngine$1__span_Z721C83C5(EngineHelpers_Html, arg_4))])]));
}

Program_mount_6E602840(view());

