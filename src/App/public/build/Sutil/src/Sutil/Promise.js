import { Union } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { union_type, class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { StoreOperators_op_LessTwiddle, Store_make } from "./Store.js";

export class PromiseState$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Waiting", "Result", "Error"];
    }
}

export function PromiseState$1$reflection(gen0) {
    return union_type("Sutil.ObservablePromise.PromiseState`1", [gen0], PromiseState$1, () => [[], [["Item", gen0]], [["Item", class_type("System.Exception")]]]);
}

export class ObservablePromise$1 {
    constructor(p) {
        this.p = p;
        this.store = Store_make(new PromiseState$1(0));
        ObservablePromise$1__run(this);
    }
    Subscribe(observer) {
        const this$ = this;
        return this$.store.Subscribe(observer);
    }
}

export function ObservablePromise$1$reflection(gen0) {
    return class_type("Sutil.ObservablePromise.ObservablePromise`1", [gen0], ObservablePromise$1);
}

export function ObservablePromise$1_$ctor_56E03C9D(p) {
    return new ObservablePromise$1(p);
}

function ObservablePromise$1__run(this$) {
    let pr_1;
    StoreOperators_op_LessTwiddle(this$.store, new PromiseState$1(0));
    (pr_1 = (this$.p.then((v) => {
        StoreOperators_op_LessTwiddle(this$.store, new PromiseState$1(1, v));
    })), pr_1.catch((x) => {
        StoreOperators_op_LessTwiddle(this$.store, new PromiseState$1(2, x));
    }));
}

export function Fable_Core_JS_Promise$1__Promise$1_ToObservable(self) {
    return ObservablePromise$1_$ctor_56E03C9D(self);
}

