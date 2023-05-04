import { Record } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { class_type, record_type, int32_type, lambda_type, unit_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { iterate, filter, cons, empty } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { Window_getComputedStyle_Z5966C024 } from "./Interop.js";
import { parse } from "../../../fable_modules/fable-library.3.7.20/Int32.js";
import { NodeKey_ResizeObserver, NodeKey_getCreate, listen, documentOf } from "./DomHelpers.js";
import { printf, toText } from "../../../fable_modules/fable-library.3.7.20/String.js";
import { disposable } from "./Helpers.js";

export const isCrossOrigin = false;

class ResizeSubscriber extends Record {
    constructor(Callback, Id) {
        super();
        this.Callback = Callback;
        this.Id = (Id | 0);
    }
}

function ResizeSubscriber$reflection() {
    return record_type("Sutil.ResizeObserver.ResizeSubscriber", [], ResizeSubscriber, () => [["Callback", lambda_type(unit_type, unit_type)], ["Id", int32_type]]);
}

export class ResizeObserver {
    constructor(el) {
        let clo_2;
        this.iframe = null;
        this.subId = 0;
        this.unsubscribe = null;
        this.subscribers = empty();
        const computedStyle = Window_getComputedStyle_Z5966C024(el);
        const zIndex = ((() => {
            try {
                return parse(computedStyle.zIndex, 511, false, 32) | 0;
            }
            catch (matchValue) {
                return 0;
            }
        })() - 1) | 0;
        if ((computedStyle.position === "static") ? true : (computedStyle.position === "")) {
            (el.style).position = "relative";
        }
        this.iframe = documentOf(el).createElement("iframe");
        const style = toText(printf("%sz-index: %i;"))("display: block; position: absolute; top: 0; left: 0; width: 100%; height: 100%; overflow: hidden; border: 0; opacity: 0; pointer-events: none;")(zIndex);
        this.iframe.setAttribute("style", style);
        this.iframe.setAttribute("aria-hidden", "true");
        this.iframe.setAttribute("tabindex", "-1");
        if (isCrossOrigin) {
            this.iframe.setAttribute("src", "data:text/html,\u003cscript\u003eonresize=function(){parent.postMessage(0,\u0027*\u0027)}\u003c/script\u003e");
            this.unsubscribe = ((clo_2 = listen("message", window, (e) => {
                if ((e["source"]) === this.iframe.contentWindow) {
                    ResizeObserver__notify_1505(this, e);
                }
            }), () => {
                clo_2();
            }));
        }
        else {
            this.iframe.setAttribute("src", "about:blank");
            this.iframe.onload = ((e_1) => {
                let clo_3;
                this.unsubscribe = ((clo_3 = listen("resize", this.iframe.contentWindow, (_arg) => {
                    ResizeObserver__notify_1505(this, _arg);
                }), () => {
                    clo_3();
                }));
            });
        }
        el.appendChild(this.iframe);
    }
    Dispose() {
        const this$ = this;
        ResizeObserver__Dispose(this$);
    }
}

export function ResizeObserver$reflection() {
    return class_type("Sutil.ResizeObserver.ResizeObserver", void 0, ResizeObserver);
}

export function ResizeObserver_$ctor_4C3D2741(el) {
    return new ResizeObserver(el);
}

export function ResizeObserver__Subscribe_3A5B6456(_, callback) {
    const sub = new ResizeSubscriber(callback, _.subId);
    _.subId = ((_.subId + 1) | 0);
    _.subscribers = cons(sub, _.subscribers);
    return disposable(() => {
        _.subscribers = filter((s) => (s.Id !== sub.Id), _.subscribers);
    });
}

export function ResizeObserver__Dispose(_) {
    try {
        _.unsubscribe();
    }
    catch (matchValue) {
    }
    if (!(_.iframe == null)) {
        _.iframe.parentNode.removeChild(_.iframe);
    }
}

function ResizeObserver__notify_1505(this$, _arg) {
    iterate((sub) => {
        sub.Callback();
    }, this$.subscribers);
}

export function getResizer(el) {
    return NodeKey_getCreate(el, NodeKey_ResizeObserver, () => ResizeObserver_$ctor_4C3D2741(el));
}

