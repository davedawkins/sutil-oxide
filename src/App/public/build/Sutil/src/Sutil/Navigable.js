import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { Window_removeEventListener_378D00DF, Window_addEventListener_378D00DF, Window_get_location } from "./Interop.js";
import { Store_set, Store_make } from "./Store.js";
import { disposeOnUnmount, fragment } from "./CoreElements.js";
import { disposable } from "./Helpers.js";
import { ofArray } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { Bind_el_ZF0512D0 } from "./Bind.js";

export class Navigable {
    constructor() {
    }
}

export function Navigable$reflection() {
    return class_type("Sutil.Navigable", void 0, Navigable);
}

export function Navigable_listenLocation_Z64CC954B(onChangeLocation) {
    let onChangeRef = (_arg) => {
        throw (new Error("`onChangeRef` has not been initialized.\nPlease make sure you used Elmish.Navigation.Program.Internal.subscribe"));
    };
    const subscribe = () => {
        let clo, clo_1;
        let lastLocation = void 0;
        const onChange = (_arg_1) => {
            let href;
            let value;
            let pattern_matching_result;
            if (lastLocation != null) {
                if ((href = lastLocation, href === Window_get_location().href)) {
                    pattern_matching_result = 0;
                }
                else {
                    pattern_matching_result = 1;
                }
            }
            else {
                pattern_matching_result = 1;
            }
            switch (pattern_matching_result) {
                case 0: {
                    value = (void 0);
                    break;
                }
                case 1: {
                    lastLocation = Window_get_location().href;
                    value = onChangeLocation(Window_get_location());
                    break;
                }
            }
            return void 0;
        };
        onChangeRef = onChange;
        Window_addEventListener_378D00DF("popstate", (clo = onChangeRef, (arg) => {
            clo(arg);
        }));
        Window_addEventListener_378D00DF("hashchange", (clo_1 = onChangeRef, (arg_1) => {
            clo_1(arg_1);
        }));
        onChange();
    };
    const unsubscribe = () => {
        let clo_2, clo_3;
        Window_removeEventListener_378D00DF("popstate", (clo_2 = onChangeRef, (arg_2) => {
            clo_2(arg_2);
        }));
        Window_removeEventListener_378D00DF("hashchange", (clo_3 = onChangeRef, (arg_3) => {
            clo_3(arg_3);
        }));
    };
    subscribe();
    return unsubscribe;
}

export function Navigable_listenLocation_5473E6E3(parser, dispatch) {
    return Navigable_listenLocation_Z64CC954B((arg) => {
        dispatch(parser(arg));
    });
}

export function Navigable_bindLocation_Z2BCB0B97(view) {
    const store = Store_make(Window_get_location());
    return fragment([disposeOnUnmount(ofArray([store, disposable(Navigable_listenLocation_5473E6E3((x) => x, (newValue) => {
        Store_set(store, newValue);
    }))])), Bind_el_ZF0512D0(store, view)]);
}

