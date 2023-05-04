import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { some, map2 } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { subscribe } from "../../../fable_modules/fable-library.3.7.20/Observable.js";
import { disposable } from "./Helpers.js";
import { equals, disposeSafe } from "../../../fable_modules/fable-library.3.7.20/Util.js";

export class BasicObserver$1 {
    constructor() {
        this.stopped = false;
    }
    OnNext(value) {
        const x = this;
        if (!x.stopped) {
            x["Sutil.Observable.BasicObserver`1.Next2B595"](value);
        }
    }
    OnError(e) {
        const x = this;
        if (!x.stopped) {
            x.stopped = true;
            x["Sutil.Observable.BasicObserver`1.Error229D3F39"](e);
        }
    }
    OnCompleted() {
        const x = this;
        if (!x.stopped) {
            x.stopped = true;
            x["Sutil.Observable.BasicObserver`1.Completed"]();
        }
    }
}

export function BasicObserver$1$reflection(gen0) {
    return class_type("Sutil.Observable.BasicObserver`1", [gen0], BasicObserver$1);
}

export function BasicObserver$1_$ctor() {
    return new BasicObserver$1();
}

export function zip(a, b) {
    return {
        Subscribe(h) {
            let valueA = void 0;
            let valueB = void 0;
            const notify = () => {
                map2((a_1, b_1) => {
                    h.OnNext([a_1, b_1]);
                }, valueA, valueB);
            };
            const disposeA = subscribe((v) => {
                valueA = some(v);
                notify();
            }, a);
            const disposeB = subscribe((v_1) => {
                valueB = some(v_1);
                notify();
            }, b);
            return disposable(() => {
                disposeSafe(disposeA);
                disposeSafe(disposeB);
            });
        },
    };
}

export function distinctUntilChangedCompare(eq, source) {
    return {
        Subscribe(h) {
            let value = null;
            let init = false;
            const safeEq = (next) => {
                if (init) {
                    return eq(value, next);
                }
                else {
                    return false;
                }
            };
            const disposeA = subscribe((next_1) => {
                if (!safeEq(next_1)) {
                    h.OnNext(next_1);
                    value = next_1;
                    init = true;
                }
            }, source);
            return disposable(() => {
                disposeSafe(disposeA);
            });
        },
    };
}

export function distinctUntilChanged(source) {
    return distinctUntilChangedCompare(equals, source);
}

export function exists(predicate, source) {
    return {
        Subscribe(h) {
            const disposeA = subscribe((x) => {
                try {
                    h.OnNext(predicate(x));
                }
                catch (ex) {
                    h.OnError(ex);
                }
            }, source);
            return disposable(() => {
                disposeSafe(disposeA);
            });
        },
    };
}

export function filter(predicate, source) {
    return {
        Subscribe(h) {
            const disposeA = subscribe((x) => {
                try {
                    if (predicate(x)) {
                        h.OnNext(x);
                    }
                }
                catch (ex) {
                    h.OnError(ex);
                }
            }, source);
            return disposable(() => {
                disposeSafe(disposeA);
            });
        },
    };
}

