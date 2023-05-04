import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { Window_matchMedia_Z721C83C5 } from "./Interop.js";
import { listen } from "./DomHelpers.js";
import { StoreOperators_op_DotGreater, StoreOperators_op_LessTwiddle, Store_make } from "./Store.js";
import { disposeOnUnmount, fragment } from "./CoreElements.js";
import { disposable } from "./Helpers.js";
import { ofArray } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { Bind_el_ZF0512D0 } from "./Bind.js";
import { transition } from "./Transition.js";
import { makeMediaRule } from "./Styling.js";
import { toString } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { printf, toText } from "../../../fable_modules/fable-library.3.7.20/String.js";

export class Media {
    constructor() {
    }
}

export function Media$reflection() {
    return class_type("Sutil.Media", void 0, Media);
}

export function Media_listenMedia_1AAA471B(query, handler) {
    const mql = Window_matchMedia_Z721C83C5(query);
    handler(mql.matches);
    const clo = listen("change", mql, (e) => {
        handler(e.matches);
    });
    return () => {
        clo();
    };
}

export function Media_bindMediaQuery_7FEBD4A7(query, view) {
    const s = Store_make(false);
    const u = Media_listenMedia_1AAA471B(query, (m) => {
        StoreOperators_op_LessTwiddle(s, m);
    });
    return fragment([disposeOnUnmount(ofArray([disposable(u), s])), Bind_el_ZF0512D0(s, view)]);
}

export function Media_showIfMedia_23F7BF09(query, f, trans, view) {
    const s = Store_make(false);
    const u = Media_listenMedia_1AAA471B(query, (m) => {
        StoreOperators_op_LessTwiddle(s, m);
    });
    return fragment([disposeOnUnmount(ofArray([disposable(u), s])), transition(trans, StoreOperators_op_DotGreater(s, f), view)]);
}

export function Media_showIfMedia_788E453F(query, trans, view) {
    return Media_showIfMedia_23F7BF09(query, (x) => x, trans, view);
}

export function Media_media_Z4DAEC249(query, map, app) {
    const s = Store_make(false);
    const u = Media_listenMedia_1AAA471B(query, (m) => {
        StoreOperators_op_LessTwiddle(s, m);
    });
    return fragment([disposeOnUnmount(ofArray([disposable(u), s])), app(StoreOperators_op_DotGreater(s, map))]);
}

export class CssMedia {
    constructor() {
    }
}

export function CssMedia$reflection() {
    return class_type("Sutil.CssMedia", void 0, CssMedia);
}

export function CssMedia_custom_Z1327120(condition, rules) {
    return makeMediaRule(condition, rules);
}

export function CssMedia_minWidth_Z64D26CF6(minWidth, rules) {
    let arg;
    return makeMediaRule((arg = toString(minWidth), toText(printf("(min-width: %s)"))(arg)), rules);
}

export function CssMedia_maxWidth_Z64D26CF6(maxWidth, rules) {
    let arg;
    return makeMediaRule((arg = toString(maxWidth), toText(printf("(max-width: %s)"))(arg)), rules);
}

