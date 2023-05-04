import { Record } from "../fable_modules/fable-library.3.7.20/Types.js";
import { record_type, list_type, tuple_type, string_type, lambda_type, unit_type, bool_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { SutilElement$reflection } from "../Sutil/src/Sutil/Core.js";
import { isEmpty, empty } from "../fable_modules/fable-library.3.7.20/List.js";
import { HtmlEngine$1__button_BB573A, HtmlEngine$1__div_BB573A } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { EngineHelpers_Ev, EngineHelpers_text, EngineHelpers_Css, SutilAttrEngine__style_68BDC580, EngineHelpers_Attr, EngineHelpers_Html } from "../Sutil/src/Sutil/Html.js";
import { Program_mountAfter_Z427DD8DF, Program_unmount_171AE942 } from "../Sutil/src/Sutil/Program.js";
import { AttrEngine$1__id_Z721C83C5 } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { CssEngine$1__filterDropShadow_Z4C7F1E99, CssEngine$1__lineHeight_Z445F6BAF, CssEngine$1__fontSize_Z445F6BAF, CssEngine$1__get_fontWeightBold, CssEngine$1__transformRotateZ_5E38073B, CssEngine$1__get_cursorPointer, CssEngine$1__get_positionAbsolute, CssEngine$1__gap_Z445F6BAF, CssEngine$1__borderRadius_Z445F6BAF, CssEngine$1__get_positionRelative, CssEngine$1__padding_Z445F6BAF, CssEngine$1__get_flexDirectionColumn, CssEngine$1__zIndex_Z524259A4, CssEngine$1__bottom_Z445F6BAF, CssEngine$1__top_Z445F6BAF, CssEngine$1__right_Z445F6BAF, CssEngine$1__left_Z445F6BAF, CssEngine$1__backgroundColor_Z721C83C5, CssEngine$1__get_alignItemsCenter, CssEngine$1__get_justifyContentCenter, CssEngine$1__get_displayFlex, CssEngine$1__get_positionFixed } from "../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";
import { int32ToString } from "../fable_modules/fable-library.3.7.20/Util.js";
import { collect, empty as empty_1, singleton, append, delay, toList } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { EventEngine$1__onClick_58BC8925 } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";

export class ModalOptions extends Record {
    constructor(ShowCancel, OnCancel, Buttons, Content) {
        super();
        this.ShowCancel = ShowCancel;
        this.OnCancel = OnCancel;
        this.Buttons = Buttons;
        this.Content = Content;
    }
}

export function ModalOptions$reflection() {
    return record_type("SutilOxide.Modal.ModalOptions", [], ModalOptions, () => [["ShowCancel", bool_type], ["OnCancel", lambda_type(unit_type, unit_type)], ["Buttons", list_type(tuple_type(string_type, lambda_type(lambda_type(unit_type, unit_type), unit_type)))], ["Content", lambda_type(lambda_type(unit_type, unit_type), SutilElement$reflection())]]);
}

export function ModalOptions_Create() {
    return new ModalOptions(true, () => {
    }, empty(), (_arg) => HtmlEngine$1__div_BB573A(EngineHelpers_Html, []));
}

export function ModalOptions_Create_1ADEB4C0(content) {
    return new ModalOptions(true, () => {
    }, empty(), content);
}

export function modal(options) {
    let tupledArg;
    const doc = document;
    const lastBodyElement = doc.body.lastElementChild;
    const close = () => {
        Program_unmount_171AE942(doc.querySelector("#ui-modal"));
    };
    const modalBg = HtmlEngine$1__div_BB573A(EngineHelpers_Html, [AttrEngine$1__id_Z721C83C5(EngineHelpers_Attr, "ui-modal"), SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_positionFixed(EngineHelpers_Css), CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_justifyContentCenter(EngineHelpers_Css), CssEngine$1__get_alignItemsCenter(EngineHelpers_Css), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "rgba(0,0,0,0.65)"), CssEngine$1__left_Z445F6BAF(EngineHelpers_Css, int32ToString(0) + "px"), CssEngine$1__right_Z445F6BAF(EngineHelpers_Css, int32ToString(0) + "px"), CssEngine$1__top_Z445F6BAF(EngineHelpers_Css, int32ToString(0) + "px"), CssEngine$1__bottom_Z445F6BAF(EngineHelpers_Css, int32ToString(0) + "px"), CssEngine$1__zIndex_Z524259A4(EngineHelpers_Css, 10)]), HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__get_flexDirectionColumn(EngineHelpers_Css), CssEngine$1__padding_Z445F6BAF(EngineHelpers_Css, (1.5).toString() + "rem"), CssEngine$1__get_positionRelative(EngineHelpers_Css), CssEngine$1__backgroundColor_Z721C83C5(EngineHelpers_Css, "white"), CssEngine$1__borderRadius_Z445F6BAF(EngineHelpers_Css, int32ToString(4) + "px"), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, int32ToString(1) + "rem"), CssEngine$1__get_alignItemsCenter(EngineHelpers_Css)])), delay(() => append(options.ShowCancel ? singleton(HtmlEngine$1__div_BB573A(EngineHelpers_Html, [SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_positionAbsolute(EngineHelpers_Css), CssEngine$1__top_Z445F6BAF(EngineHelpers_Css, int32ToString(0) + "rem"), CssEngine$1__right_Z445F6BAF(EngineHelpers_Css, int32ToString(0) + "rem"), CssEngine$1__get_cursorPointer(EngineHelpers_Css), CssEngine$1__transformRotateZ_5E38073B(EngineHelpers_Css, 45), CssEngine$1__get_fontWeightBold(EngineHelpers_Css), CssEngine$1__fontSize_Z445F6BAF(EngineHelpers_Css, int32ToString(2) + "rem"), CssEngine$1__lineHeight_Z445F6BAF(EngineHelpers_Css, (1.5).toString() + "rem"), CssEngine$1__filterDropShadow_Z4C7F1E99(EngineHelpers_Css, 0, 0, 8, "#505050")]), EngineHelpers_text("+"), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg) => {
        close();
    })])) : empty_1(), delay(() => append(singleton(options.Content(close)), delay(() => ((!isEmpty(options.Buttons)) ? singleton(HtmlEngine$1__div_BB573A(EngineHelpers_Html, toList(delay(() => append(singleton(SutilAttrEngine__style_68BDC580(EngineHelpers_Attr, [CssEngine$1__get_displayFlex(EngineHelpers_Css), CssEngine$1__gap_Z445F6BAF(EngineHelpers_Css, int32ToString(1) + "rem")])), delay(() => collect((matchValue) => {
        const label = matchValue[0];
        const click = matchValue[1];
        return singleton(HtmlEngine$1__button_BB573A(EngineHelpers_Html, [EngineHelpers_text(label), EventEngine$1__onClick_58BC8925(EngineHelpers_Ev, (_arg_1) => {
            click(close);
        })]));
    }, options.Buttons))))))) : empty_1()))))))))))]);
    (tupledArg = [lastBodyElement, modalBg], Program_mountAfter_Z427DD8DF(tupledArg[0], tupledArg[1]));
}

