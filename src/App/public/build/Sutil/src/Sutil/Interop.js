import { some } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";

export function getOption(ob, name) {
    const matchValue = ob.hasOwnProperty(name);
    if (matchValue) {
        return some(ob[name]);
    }
    else {
        return void 0;
    }
}

export function getDefault(ob, name, defaultValue) {
    const matchValue = (() => ({})).hasOwnProperty(name);
    if (matchValue) {
        return ob[name];
    }
    else {
        return defaultValue;
    }
}

export class Window$ {
    constructor() {
    }
}

export function Window$$reflection() {
    return class_type("Sutil.Interop.Window", void 0, Window$);
}

export function Window_$ctor() {
    return new Window$();
}

export function Window_alert_1505(msg) {
    if (typeof window !== 'undefined') {
        window.alert(some(msg));
    }
}

export function Window_get_document() {
    if (typeof window !== 'undefined') {
        return window.document;
    }
    else {
        return null;
    }
}

export function Window_get_location() {
    if (typeof window !== 'undefined') {
        return window.location;
    }
    else {
        return null;
    }
}

export function Window_addEventListener_378D00DF(typ, listener) {
    if (typeof window !== 'undefined') {
        window.addEventListener(typ, listener);
    }
}

export function Window_getComputedStyle_Z5966C024(elt) {
    if (typeof window !== 'undefined') {
        return window.getComputedStyle(elt);
    }
    else {
        return null;
    }
}

export function Window_getComputedStyle_ZBDDB899(elt, pseudoElt) {
    if (typeof window !== 'undefined') {
        return window.getComputedStyle(elt, pseudoElt);
    }
    else {
        return null;
    }
}

export function Window_matchMedia_Z721C83C5(query) {
    if (typeof window !== 'undefined') {
        return window.matchMedia(query);
    }
    else {
        return null;
    }
}

export function Window_removeEventListener_378D00DF(typ, listener) {
    if (typeof window !== 'undefined') {
        window.removeEventListener(typ, listener);
    }
}

export function Window_requestAnimationFrame_1A119E11(callback) {
    if (typeof window !== 'undefined') {
        return window.requestAnimationFrame(callback);
    }
    else {
        return 0;
    }
}

