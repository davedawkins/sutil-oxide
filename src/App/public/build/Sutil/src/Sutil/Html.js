import { EventEngine$1$reflection, EventEngine$1 } from "../../../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { nothing, removeClass, addClass, toggleClass, setClass, styleAppend, style, attr, html as html_1, fragment, text, el, onUnmount, onMount, on } from "./CoreElements.js";
import { empty } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { SutilEffect, ContextHelpers_withParent, buildChildren, SutilElement_Define_Z60F5000F, SutilElement$reflection } from "./Core.js";
import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { FSharpRef } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { HtmlEngine$1__div_Z721C83C5, HtmlEngine$1__h6_Z721C83C5, HtmlEngine$1__h5_Z721C83C5, HtmlEngine$1__h4_Z721C83C5, HtmlEngine$1__h3_Z721C83C5, HtmlEngine$1__h2_Z721C83C5, HtmlEngine$1__h1_Z721C83C5, HtmlEngine$1__span_Z721C83C5, HtmlEngine$1__td_Z721C83C5, HtmlEngine$1__ol_BB573A, HtmlEngine$1__tbody_BB573A, HtmlEngine$1__hr_BB573A, HtmlEngine$1__a_BB573A, HtmlEngine$1__h6_BB573A, HtmlEngine$1__h5_BB573A, HtmlEngine$1__h4_BB573A, HtmlEngine$1__h3_BB573A, HtmlEngine$1__h2_BB573A, HtmlEngine$1__h1_BB573A, HtmlEngine$1__p_BB573A, HtmlEngine$1__dd_BB573A, HtmlEngine$1__dt_BB573A, HtmlEngine$1__dl_BB573A, HtmlEngine$1__li_BB573A, HtmlEngine$1__ul_BB573A, HtmlEngine$1__aside_BB573A, HtmlEngine$1__article_BB573A, HtmlEngine$1__footer_BB573A, HtmlEngine$1__header_BB573A, HtmlEngine$1__nav_BB573A, HtmlEngine$1__section_BB573A, HtmlEngine$1__label_BB573A, HtmlEngine$1__input_BB573A, HtmlEngine$1__td_BB573A, HtmlEngine$1__tr_BB573A, HtmlEngine$1__thead_BB573A, HtmlEngine$1__table_BB573A, HtmlEngine$1__i_BB573A, HtmlEngine$1__img_BB573A, HtmlEngine$1__span_BB573A, HtmlEngine$1__button_BB573A, HtmlEngine$1__div_BB573A, HtmlEngine$1$reflection, HtmlEngine$1 } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/HtmlEngine.fs.js";
import { Bind_style_14B41C44, Bind_el_ZF0512D0 } from "./Bind.js";
import { StoreOperators_op_DotGreater, Store_distinct } from "./Store.js";
import { int32ToString } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { singleton, append, delay, toList } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { AttrEngine$1$reflection, AttrEngine$1 } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/AttrEngine.fs.js";
import { bindAttrBoth, bindAttrIn } from "./Bindings.js";
import { CssEngine$1_$ctor_Z19E9258B } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/CssEngine.fs.js";


export class SutilEventEngine extends EventEngine$1 {
    constructor() {
        super((event, handler) => on(event.toLocaleLowerCase(), handler, empty()));
    }
}

export function SutilEventEngine$reflection() {
    return class_type("Sutil.SutilEventEngine", void 0, SutilEventEngine, EventEngine$1$reflection(SutilElement$reflection()));
}

export function SutilEventEngine_$ctor() {
    return new SutilEventEngine();
}

export function SutilEventEngine__onMount_7DDE0344(__, handler) {
    return onMount(handler, empty());
}

export function SutilEventEngine__onUnmount_7DDE0344(__, handler) {
    return onUnmount(handler, empty());
}

export class SutilHtmlEngine extends HtmlEngine$1 {
    constructor() {
        super(el, text, () => fragment([]));
        this.this = (new FSharpRef(null));
        this.this.contents = this;
        this["init@72-1"] = 1;
    }
}

export function SutilHtmlEngine$reflection() {
    return class_type("Sutil.SutilHtmlEngine", void 0, SutilHtmlEngine, HtmlEngine$1$reflection(SutilElement$reflection()));
}

export function SutilHtmlEngine_$ctor() {
    return new SutilHtmlEngine();
}

export function SutilHtmlEngine__app_Z5F036EB1(_, xs) {
    return fragment(xs);
}

export function SutilHtmlEngine__body_Z5F036EB1(_, xs) {
    return SutilElement_Define_Z60F5000F("Html.body", (ctx) => {
        buildChildren(xs, ContextHelpers_withParent(new SutilEffect(1, ctx.Document.body), ctx));
    });
}

export function SutilHtmlEngine__parse_Z721C83C5(_, html) {
    return html_1(html);
}

export function SutilHtmlEngine__parent(_, selector, xs) {
    return SutilElement_Define_Z60F5000F("Html.parent", (ctx) => {
        buildChildren(xs, ContextHelpers_withParent(new SutilEffect(1, ctx.Document.querySelector(selector)), ctx));
    });
}

export function SutilHtmlEngine__divc(__, cls, children) {
    return HtmlEngine$1__div_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__buttonc(__, cls, children) {
    return HtmlEngine$1__button_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__spanc(__, cls, children) {
    return HtmlEngine$1__span_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__imgc(__, cls, children) {
    return HtmlEngine$1__img_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__ic(__, cls, children) {
    return HtmlEngine$1__i_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__tablec(__, cls, children) {
    return HtmlEngine$1__table_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__theadc(__, cls, children) {
    return HtmlEngine$1__thead_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__trc(__, cls, children) {
    return HtmlEngine$1__tr_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__tdc(__, cls, children) {
    return HtmlEngine$1__td_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__inputc(__, cls, children) {
    return HtmlEngine$1__input_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__labelc(__, cls, children) {
    return HtmlEngine$1__label_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__sectionc(__, cls, children) {
    return HtmlEngine$1__section_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__navc(__, cls, children) {
    return HtmlEngine$1__nav_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__headerc(__, cls, children) {
    return HtmlEngine$1__header_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__footerc(__, cls, children) {
    return HtmlEngine$1__footer_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__articlec(__, cls, children) {
    return HtmlEngine$1__article_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__asidec(__, cls, children) {
    return HtmlEngine$1__aside_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__ulc(__, cls, children) {
    return HtmlEngine$1__ul_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__lic(__, cls, children) {
    return HtmlEngine$1__li_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__dlc(__, cls, children) {
    return HtmlEngine$1__dl_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__dtc(__, cls, children) {
    return HtmlEngine$1__dt_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__ddc(__, cls, children) {
    return HtmlEngine$1__dd_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__pc(__, cls, children) {
    return HtmlEngine$1__p_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__h1c(__, cls, children) {
    return HtmlEngine$1__h1_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__h2c(__, cls, children) {
    return HtmlEngine$1__h2_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__h3c(__, cls, children) {
    return HtmlEngine$1__h3_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__h4c(__, cls, children) {
    return HtmlEngine$1__h4_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__h5c(__, cls, children) {
    return HtmlEngine$1__h5_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__h6c(__, cls, children) {
    return HtmlEngine$1__h6_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__ac(__, cls, children) {
    return HtmlEngine$1__a_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__hrc(__, cls, children) {
    return HtmlEngine$1__hr_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__tbodyc(__, cls, children) {
    return HtmlEngine$1__tbody_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__olc(__, cls, children) {
    return HtmlEngine$1__ol_BB573A(__, SutilHtmlEngine___clsch(__, cls, children));
}

export function SutilHtmlEngine__text_Z686281E5(_, v) {
    return Bind_el_ZF0512D0(Store_distinct(v), text);
}

export function SutilHtmlEngine__text_76B5483C(_, v) {
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), text);
}

export function SutilHtmlEngine__text_Z198ADE65(_, v) {
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), text);
}

export function SutilHtmlEngine__td_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__td_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__td_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__td_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__td_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__td_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__span_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__span_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__span_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__span_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__span_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__span_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h1_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h1_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h1_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h1_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h1_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h1_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h2_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h2_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h2_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h2_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h2_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h2_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h3_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h3_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h3_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h3_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h3_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h3_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h4_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h4_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h4_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h4_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h4_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h4_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h5_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h5_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h5_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h5_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h5_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h5_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h6_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h6_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h6_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h6_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__h6_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__h6_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__div_Z686281E5(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(v), (objectArg = _.this.contents, (arg) => HtmlEngine$1__div_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__div_76B5483C(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, int32ToString)), (objectArg = _.this.contents, (arg) => HtmlEngine$1__div_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__div_Z198ADE65(_, v) {
    let objectArg;
    return Bind_el_ZF0512D0(Store_distinct(StoreOperators_op_DotGreater(v, (value) => value.toString())), (objectArg = _.this.contents, (arg) => HtmlEngine$1__div_Z721C83C5(objectArg, arg)));
}

export function SutilHtmlEngine__fragment_788D4CC0(_, v) {
    return Bind_el_ZF0512D0(v, (x) => x);
}

function SutilHtmlEngine___clsch(this$, cls, ch) {
    return toList(delay(() => append(singleton(attr("class", cls)), delay(() => ch))));
}

export class SutilAttrEngine extends AttrEngine$1 {
    constructor() {
        super(attr, attr);
    }
}

export function SutilAttrEngine$reflection() {
    return class_type("Sutil.SutilAttrEngine", void 0, SutilAttrEngine, AttrEngine$1$reflection(SutilElement$reflection()));
}

export function SutilAttrEngine_$ctor() {
    return new SutilAttrEngine();
}

export function SutilAttrEngine__disabled_75709723(_, value) {
    return bindAttrIn("disabled", value);
}

export function SutilAttrEngine__value_4E60E31B(_, value) {
    return attr("value", value);
}

export function SutilAttrEngine__value_Z524259A4(_, value) {
    return attr("value", value);
}

export function SutilAttrEngine__value_5E38073B(_, value) {
    return attr("value", value);
}

export function SutilAttrEngine__value_Z1FBCCD16(_, value) {
    return attr("value", value);
}

export function SutilAttrEngine__value_75709723(_, value) {
    return bindAttrIn("value", value);
}

export function SutilAttrEngine__value_Z5EDE14D4(_, value, dispatch) {
    return bindAttrBoth("value", value, dispatch);
}

export function SutilAttrEngine__style_68BDC580(_, cssAttrs) {
    return style(cssAttrs);
}

export function SutilAttrEngine__styleAppend_68BDC580(_, cssAttrs) {
    return styleAppend(cssAttrs);
}

export function SutilAttrEngine__style_14B41C44(_, cssAttrs) {
    return Bind_style_14B41C44(cssAttrs);
}

export function SutilAttrEngine__setClass_Z721C83C5(_, className) {
    return setClass(className);
}

export function SutilAttrEngine__toggleClass_Z721C83C5(_, className) {
    return toggleClass(className);
}

export function SutilAttrEngine__addClass_Z721C83C5(_, className) {
    return addClass(className);
}

export function SutilAttrEngine__removeClass_Z721C83C5(_, className) {
    return removeClass(className);
}

export function SutilAttrEngine__get_none(_) {
    return nothing;
}

export function SutilAttrEngine__text_Z721C83C5(_, s) {
    return text(s);
}

export const EngineHelpers_Html = SutilHtmlEngine_$ctor();

export const EngineHelpers_Attr = SutilAttrEngine_$ctor();

export const EngineHelpers_Ev = SutilEventEngine_$ctor();

export const EngineHelpers_Css = CssEngine$1_$ctor_Z19E9258B((k, v) => [k, v]);

export function EngineHelpers_text(s) {
    return text(s);
}

export const EngineHelpers_prop = EngineHelpers_Attr;

function PseudoCss_cssAttr() {
    return (x) => x;
}

export function PseudoCss_addClass(n) {
    return PseudoCss_cssAttr()(["sutil-add-class", n]);
}

export const PseudoCss_useGlobal = PseudoCss_cssAttr()(["sutil-use-global", ""]);

