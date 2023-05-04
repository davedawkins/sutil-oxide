import { makeElmish, makeElmishSimple, makeStore } from "./ObservableStore.js";
import { map as map_1, subscribe } from "../../../fable_modules/fable-library.3.7.20/Observable.js";
import { zip, distinctUntilChanged, filter } from "./Observable.js";
import { disposeSafe } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { some } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { addToSet } from "../../../fable_modules/fable-library.3.7.20/MapUtil.js";
import { fold } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { iterateIndexed } from "../../../fable_modules/fable-library.3.7.20/List.js";

export function StoreHelpers_disposable(f) {
    return {
        Dispose() {
            f();
        },
    };
}

export function Store_make(modelInit) {
    const init = () => modelInit;
    const s = makeStore(init, (value) => {
    });
    return s;
}

export function Store_get(store) {
    return store.Value;
}

export function Store_set(store, newValue) {
    store.Update((_arg) => newValue);
}

export function Store_subscribe(callback, store) {
    return subscribe(callback, store);
}

export function Store_map(callback, store) {
    return map_1(callback, store);
}

export function Store_filter(predicate, store) {
    return filter(predicate, store);
}

export function Store_distinct(source) {
    return distinctUntilChanged(source);
}

export function Store_mapDistinct(callback, store) {
    return Store_distinct(Store_map(callback, store));
}

export function Store_zip(source1, source2) {
    return zip(source1, source2);
}

export function Store_current(store) {
    let value = null;
    disposeSafe(subscribe((v) => {
        value = v;
    }, store));
    return value;
}

export function Store_getMap(callback, store) {
    return callback(Store_get(store));
}

export function Store_iter(callback, source) {
    return Store_subscribe(callback, source);
}

export function Store_write(callback, store) {
    Store_iter(callback, store);
}

export function Store_modify(callback, store) {
    Store_set(store, Store_getMap(callback, store));
}

export function Store_subscribe2(source1, source2, callback) {
    let initState = 0;
    let cachea = null;
    let cacheb = null;
    const notify = () => {
        if (initState === 2) {
            callback([cachea, cacheb]);
        }
    };
    const unsuba = Store_subscribe((v) => {
        if (initState === 0) {
            initState = 1;
        }
        cachea = v;
        notify();
    }, source1);
    const unsubb = Store_subscribe((v_1) => {
        if (initState === 1) {
            initState = 2;
        }
        cacheb = v_1;
        notify();
    }, source2);
    if (initState !== 2) {
        console.log(some("Error: subscribe didn\u0027t initialize us"));
        throw (new Error("Subscribe didn\u0027t initialize us"));
    }
    return StoreHelpers_disposable(() => {
        disposeSafe(unsuba);
        disposeSafe(unsubb);
    });
}

export function Store_makeElmishSimple(init, update, dispose) {
    return makeElmishSimple(init, update, dispose);
}

export function Store_makeElmish(init, update, dispose) {
    return makeElmish(init, update, dispose);
}

export function StoreOperators_op_BarMinusGreater(s, f) {
    return Store_getMap(f, s);
}

export function StoreOperators_op_DotGreater(s, f) {
    return Store_map(f, s);
}

export function StoreOperators_op_DotGreaterGreater(s, f) {
    return Store_mapDistinct(f, s);
}

export function StoreOperators_op_LessTwiddle(s, v) {
    Store_set(s, v);
}

export function StoreOperators_op_LessTwiddleMinus(s, v) {
    Store_set(s, v);
}

export function StoreOperators_op_MinusTwiddleGreater(v, s) {
    Store_set(s, v);
}

export function StoreOperators_op_LessTwiddleEquals(store, map) {
    Store_modify(map, store);
}

export function StoreOperators_op_EqualsTwiddleGreater(map, store) {
    Store_modify(map, store);
}

export function StoreExtensions_firstOf(selectors) {
    const matches = new Set([]);
    let current = -1;
    const s = Store_make(current);
    const setMatch = (i, state) => {
        if (state) {
            addToSet(i, matches);
        }
        else {
            matches.delete(i);
        }
    };
    const scan = () => {
        const next = fold((a, i_1) => {
            if ((a < 0) ? true : (i_1 < a)) {
                return i_1 | 0;
            }
            else {
                return a | 0;
            }
        }, -1, matches) | 0;
        if (next !== current) {
            StoreOperators_op_LessTwiddle(s, next);
            current = (next | 0);
        }
    };
    iterateIndexed((i_2, pred) => {
        const u = subscribe((state_2) => {
            setMatch(i_2, state_2);
            scan();
        }, pred);
    }, selectors);
    return s;
}

