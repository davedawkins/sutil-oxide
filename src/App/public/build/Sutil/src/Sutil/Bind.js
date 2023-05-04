import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { transition } from "./Transition.js";
import { exactlyOne, singleton, empty } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { arrayWrapO, eachio, eachi, eachiko, eachk, listWrapO, each, bindElement2, bindElementKO, bindElementK, bindElement, bindClassNames, bindClassName, bindClassToggle, bindWidthHeight, bindLeftTop, bindStyle, cssAttrsToString, bindAttrOut, bindAttrIn, bindAttrStoreBoth, bindAttrBoth, attrNotify, bindSelected, bindSelectSingle, bindSelectOptional, bindSelectMultiple, bindGroup, bindRadioGroup } from "./Bindings.js";
import { Store_map, StoreOperators_op_DotGreater } from "./Store.js";
import { Fable_Core_JS_Promise$1__Promise$1_ToObservable } from "./Promise.js";
import { text, class$0027, el } from "./CoreElements.js";

export class Bind {
    constructor() {
    }
}

export function Bind$reflection() {
    return class_type("Sutil.Bind", void 0, Bind);
}

export function Bind_visibility_40BD454A(isVisible) {
    return (element) => transition(empty(), isVisible, element);
}

export function Bind_visibility_4431D48E(isVisible, trans) {
    return (element) => transition(trans, isVisible, element);
}

export function Bind_radioValue_7ACACBBB(store) {
    return bindRadioGroup(store);
}

export function Bind_checkboxGroup_ZF7FFE17(store) {
    return bindGroup(store);
}

export function Bind_selectMultiple_Z25BDACE1(store) {
    return bindSelectMultiple(store);
}

export function Bind_selectOptional_6D568EE0(store) {
    return bindSelectOptional(store);
}

export function Bind_selectSingle_Z685FE8AB(store) {
    return bindSelectSingle(store);
}

export function Bind_selectSingle_3891390A(value, dispatch) {
    return bindSelected(StoreOperators_op_DotGreater(value, singleton), (arg) => {
        dispatch(exactlyOne(arg));
    });
}

export function Bind_selectMultiple_Z3971AD96(value, dispatch) {
    return bindSelected(value, dispatch);
}

export function Bind_attrInit_16177AF1(attrName, initValue, dispatch) {
    return attrNotify(attrName, initValue, dispatch);
}

export function Bind_attr_Z5ECB44E9(name, value, dispatch) {
    return bindAttrBoth(name, value, dispatch);
}

export function Bind_attr_3099C820(name, value) {
    return bindAttrStoreBoth(name, value);
}

export function Bind_attr_3F2394B8(name, value) {
    return bindAttrIn(name, value);
}

export function Bind_attr_Z370E6CCC(name, dispatch) {
    return bindAttrOut(name, dispatch);
}

export function Bind_style_14B41C44(attrs) {
    return Bind_attr_3F2394B8("style", Store_map(cssAttrsToString, attrs));
}

export function Bind_style_2C84B00C(values, updater) {
    return bindStyle(values, updater);
}

export function Bind_leftTop_44BD5D2F(xy) {
    return bindLeftTop(xy);
}

export function Bind_widthHeight_44BD5D2F(xy) {
    return bindWidthHeight(xy);
}

export function Bind_toggleClass_BBB94EA(toggle, activeClass, inactiveClass) {
    return bindClassToggle(toggle, activeClass, inactiveClass);
}

export function Bind_toggleClass_Z2A796D4F(toggle, activeClass) {
    return bindClassToggle(toggle, activeClass, "");
}

export function Bind_className_Z686281E5(name) {
    return bindClassName(name);
}

export function Bind_classNames_Z7D5B9F70(name) {
    return bindClassNames(name);
}

export function Bind_el_ZF0512D0(value, element) {
    return bindElement(value, element);
}

export function Bind_el_4DBB430E(value, key, element) {
    return bindElementK(value, element, key);
}

export function Bind_el_Z7444598(value, key, element) {
    return bindElementKO(value, element, key);
}

export function Bind_fragment(value, element) {
    return bindElement(value, element);
}

export function Bind_el2(valueA, valueB, element) {
    return bindElement2(valueA, valueB, element);
}

export function Bind_fragment2(valueA, valueB, element) {
    return bindElement2(valueA, valueB, element);
}

export function Bind_selected_Z3971AD96(value, dispatch) {
    return bindSelected(value, dispatch);
}

export function Bind_selected_Z25BDACE1(store) {
    return bindSelectMultiple(store);
}

export function Bind_selected_6D568EE0(store) {
    return bindSelectOptional(store);
}

export function Bind_selected_Z685FE8AB(store) {
    return bindSelectSingle(store);
}

export function Bind_each_Z85A3A68(items, view, trans) {
    return each(listWrapO(items), view, trans);
}

export function Bind_each_Z85D83E4(items, view) {
    return each(listWrapO(items), view, empty());
}

export function Bind_each_F22F38E(items, view, key, trans) {
    return eachk(listWrapO(items), view, key, trans);
}

export function Bind_each_1F9EC04A(items, view, key) {
    return eachk(listWrapO(items), view, key, empty());
}

export function Bind_each_51B11E46(items, view, key, trans) {
    return eachiko()(listWrapO(items))((arg) => view(arg[1]))((arg_1) => key(arg_1[1]))(trans);
}

export function Bind_each_Z2C2F5BFE(items, view, key) {
    return eachiko()(listWrapO(items))((arg) => view(arg[1]))((arg_1) => key(arg_1[1]))(empty());
}

export function Bind_eachi_Z47A5CC55(items, view, trans) {
    return eachi(listWrapO(items), view, trans);
}

export function Bind_eachi_528C0FCF(items, view) {
    return eachi(listWrapO(items), view, empty());
}

export function Bind_eachi_Z1010AD75(items, view, trans) {
    return eachio(listWrapO(items), view, trans);
}

export function Bind_eachi_Z1F66ED51(items, view) {
    return eachio(listWrapO(items), view, empty());
}

export function Bind_eachi_757B3BE6(items, view, key, trans) {
    return eachiko()(listWrapO(items))(view)(key)(trans);
}

export function Bind_eachi_58854DA2(items, view, key) {
    return eachiko()(listWrapO(items))(view)(key)(empty());
}

export function Bind_promises_B7F7BB7(items, view, waiting, error) {
    return Bind_el_ZF0512D0(items, (p) => Bind_promise_ZD6A6129(p, view, waiting, error));
}

export function Bind_promise_ZD6A6129(p, view, waiting, error) {
    return Bind_el_ZF0512D0(Fable_Core_JS_Promise$1__Promise$1_ToObservable(p), (state) => {
        switch (state.tag) {
            case 2: {
                const x = state.fields[0];
                return error(x);
            }
            case 1: {
                const r = state.fields[0];
                return view(r);
            }
            default: {
                return waiting;
            }
        }
    });
}

export function Bind_promise_Z6B94AFE8(p, view) {
    const w = el("div", [class$0027("promise-waiting"), text("waiting...")]);
    const e = (x) => el("div", [class$0027("promise-error"), text(x.message)]);
    return Bind_promise_ZD6A6129(p, view, w, e);
}

export class BindArray {
    constructor() {
    }
}

export function BindArray$reflection() {
    return class_type("Sutil.BindArray", void 0, BindArray);
}

export function BindArray_each_Z5C92114C(items, view, trans) {
    return each(arrayWrapO(items), view, trans);
}

export function BindArray_each_Z40060150(items, view) {
    return each(arrayWrapO(items), view, empty());
}

export function BindArray_each_Z559F9DDE(items, view, key, trans) {
    return eachk(arrayWrapO(items), view, key, trans);
}

export function BindArray_each_4B56EB66(items, view, key) {
    return eachk(arrayWrapO(items), view, key, empty());
}

export function BindArray_each_3F71B88C(items, view, key, trans) {
    return eachiko()(arrayWrapO(items))((arg) => view(arg[1]))((arg_1) => key(arg_1[1]))(trans);
}

export function BindArray_each_Z5444AE78(items, view, key) {
    return eachiko()(arrayWrapO(items))((arg) => view(arg[1]))((arg_1) => key(arg_1[1]))(empty());
}

export function BindArray_eachi_699D03C7(items, view, trans) {
    return eachi(arrayWrapO(items), view, trans);
}

export function BindArray_eachi_1AD78D63(items, view) {
    return eachi(arrayWrapO(items), view, empty());
}

export function BindOperators_op_GreaterDivide(a, b) {
    return Bind_el_ZF0512D0(a, b);
}

