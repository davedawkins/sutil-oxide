import { initWith, enabled, error, log as log_1, isEnabled } from "./Logging.js";
import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { empty, iterate } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { int32ToString, createAtom } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { Event_notifyUpdated, rafu, findNodeWithSvId, Event_NewStore, CustomDispatch$1_dispatch_4FBB8B24, Event_UpdateStore, CustomDispatch$1_dispatch_472E5A31 } from "./DomHelpers.js";
import { Window_get_document } from "./Interop.js";
import { addToDict, getItemFromDict } from "../../../fable_modules/fable-library.3.7.20/MapUtil.js";
import { DevToolsControl_initialise, DevToolsControl_Options, DevToolsControl_Version } from "./Types.js";
import { iterate as iterate_1, map, toArray } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { SutilEffect, SutilEffect__PrettyPrint_Z721C83C5 } from "./Core.js";
import { toArray as toArray_1 } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { disposable } from "./Helpers.js";
import { subscribe } from "../../../fable_modules/fable-library.3.7.20/Observable.js";

function logEnabled() {
    return isEnabled("store");
}

function log(s) {
    log_1("store", s);
}

export class Helpers_CmdHandler$1 {
    constructor(handler, dispose) {
        this.handler = handler;
        this.dispose = dispose;
    }
    Dispose() {
        const this$ = this;
        Helpers_CmdHandler$1__Dispose(this$);
    }
}

export function Helpers_CmdHandler$1$reflection(gen0) {
    return class_type("Sutil.ObservableStore.Helpers.CmdHandler`1", [gen0], Helpers_CmdHandler$1);
}

export function Helpers_CmdHandler$1_$ctor_339679B6(handler, dispose) {
    return new Helpers_CmdHandler$1(handler, dispose);
}

export function Helpers_CmdHandler$1__Handle_Z40332EE1(_, cmd) {
    _.handler(cmd);
}

export function Helpers_CmdHandler$1__Dispose(_) {
    const matchValue = _.dispose;
    if (matchValue == null) {
    }
    else {
        const d = matchValue;
        d();
    }
}

export function Helpers_cmdHandler(dispatch) {
    return Helpers_CmdHandler$1_$ctor_339679B6((list) => {
        iterate((cmd) => {
            setTimeout(() => {
                cmd(dispatch);
            }, 0);
        }, list);
    });
}

export let Registry_nextId = createAtom(0);

export const Registry_idToStore = new Map([]);

export const Registry_storeToId = new Map([]);

export function Registry_notifyUpdateStore(s, v) {
    CustomDispatch$1_dispatch_472E5A31(Window_get_document(), Event_UpdateStore, {
        Value: v,
    });
}

export function Registry_notifyMakeStore(s) {
    if (Registry_storeToId.has(s)) {
        throw (new Error("Store is already registered!"));
    }
    const id = Registry_nextId() | 0;
    if (logEnabled()) {
        log(`make store #${id}`);
    }
    Registry_nextId(Registry_nextId() + 1, true);
    Registry_idToStore.set(id, s);
    Registry_storeToId.set(s, id);
    CustomDispatch$1_dispatch_4FBB8B24(Window_get_document(), Event_NewStore);
}

export function Registry_notifyDisposeStore(s) {
    const id = getItemFromDict(Registry_storeToId, s) | 0;
    if (logEnabled()) {
        log(`dispose store #${id}`);
    }
    try {
        Registry_idToStore.delete(id);
        Registry_storeToId.delete(s);
    }
    catch (x) {
        error(`disposing store ${id}: ${x.message}`);
    }
}

export function Registry_getStoreById(id) {
    return getItemFromDict(Registry_idToStore, id).Debugger;
}

export function Registry_controlBlock() {
    return new (class {
        get ControlBlockVersion() {
            return 1;
        }
        get Version() {
            return new DevToolsControl_Version(0, 1, 0);
        }
        GetOptions() {
            return DevToolsControl_Options();
        }
        SetOptions(op) {
            DevToolsControl_Options(op, true);
        }
        GetStores() {
            return toArray(Registry_storeToId.values());
        }
        GetStoreById(id) {
            return Registry_getStoreById(id);
        }
        GetLogCategories() {
            return toArray(map((k) => [k[0], k[1]], enabled));
        }
        SetLogCategories(states) {
            initWith(states);
        }
        PrettyPrint(id_1) {
            iterate_1((n) => {
                SutilEffect__PrettyPrint_Z721C83C5(new SutilEffect(1, n), "Node #" + int32ToString(id_1));
            }, toArray_1(findNodeWithSvId(Window_get_document(), id_1)));
        }
        GetMountPoints() {
            return [];
        }
    }
    )();
}

export function Registry_initialise(doc) {
    DevToolsControl_initialise(doc, Registry_controlBlock());
}

let _nextStoreId = 0;

function nextStoreId() {
    const n = _nextStoreId | 0;
    _nextStoreId = ((n + 1) | 0);
    return n | 0;
}

export class Store$1 {
    constructor(init, dispose) {
        this.init = init;
        this.dispose = dispose;
        this.uid = 0;
        this.storeId = (nextStoreId() | 0);
        this.name = ("store-" + int32ToString(this.storeId));
        this._modelInitialized = false;
        this._model = null;
        this.subscribers = (new Map([]));
    }
    toString() {
        const _ = this;
        return `#${_.storeId}=${_._model}`;
    }
    Subscribe(observer) {
        const this$ = this;
        return Store$1__Subscribe_32482748(this$, observer);
    }
    Update(f) {
        const this$ = this;
        Store$1__Update_Z1FC644CA(this$, f);
    }
    get Value() {
        const this$ = this;
        return Store$1__get_Value(this$);
    }
    get Name() {
        const this$ = this;
        return Store$1__get_Name(this$);
    }
    set Name(v) {
        const this$ = this;
        Store$1__set_Name_Z721C83C5(this$, v);
    }
    get Debugger() {
        const this$ = this;
        return new (class {
            get Value() {
                return Store$1__get_Value(this$);
            }
            get NumSubscribers() {
                return this$.subscribers.size;
            }
        }
        )();
    }
    Dispose() {
        const this$ = this;
        Store$1__Dispose(this$);
    }
}

export function Store$1$reflection(gen0) {
    return class_type("Sutil.ObservableStore.Store`1", [gen0], Store$1);
}

export function Store$1_$ctor_130652E0(init, dispose) {
    return new Store$1(init, dispose);
}

export function Store$1__get_Value(_) {
    return Store$1__model(_);
}

export function Store$1__Update_Z1FC644CA(this$, f) {
    const newModel = f(Store$1__model(this$));
    if (!(this$._model === newModel)) {
        this$._model = newModel;
        if (this$.subscribers.size > 0) {
            iterate_1((s) => {
                s.OnNext(this$._model);
            }, this$.subscribers.values());
        }
    }
}

export function Store$1__Subscribe_32482748(this$, observer) {
    const id = this$.uid | 0;
    this$.uid = ((this$.uid + 1) | 0);
    if (logEnabled()) {
        log(`subscribe ${id}`);
    }
    addToDict(this$.subscribers, id, observer);
    observer.OnNext(Store$1__model(this$));
    return disposable(() => {
        if (logEnabled()) {
            log(`unsubscribe ${id}`);
        }
        this$.subscribers.delete(id);
    });
}

export function Store$1__get_Name(this$) {
    return this$.name;
}

export function Store$1__set_Name_Z721C83C5(this$, v) {
    this$.name = v;
}

export function Store$1__Dispose(this$) {
    iterate_1((x) => {
        x.OnCompleted();
    }, this$.subscribers.values());
    this$.subscribers.clear();
    this$.dispose(Store$1__model(this$));
    this$._model = null;
    Registry_notifyDisposeStore(this$);
}

function Store$1__model(this$) {
    if (!this$._modelInitialized) {
        this$._model = this$.init();
        this$._modelInitialized = true;
    }
    return this$._model;
}

export function makeElmishWithCons(init, update, dispose, cons) {
    let _storeDispatch = void 0;
    let _cmdHandler = null;
    return (props) => {
        if (_storeDispatch == null) {
            const patternInput_1 = cons(() => {
                const patternInput = init(props);
                const m = patternInput[0];
                const cmd = patternInput[1];
                Helpers_CmdHandler$1__Handle_Z40332EE1(_cmdHandler, cmd);
                return m;
            }, (m_1) => {
                Helpers_CmdHandler$1__Dispose(_cmdHandler);
                dispose(m_1);
            });
            const storeUpdate = patternInput_1[1];
            const store = patternInput_1[0];
            const dispatch = (msg) => {
                let _cmds = empty();
                storeUpdate((model) => {
                    const patternInput_2 = update(msg, model);
                    const model_1 = patternInput_2[0];
                    const cmds = patternInput_2[1];
                    _cmds = cmds;
                    return model_1;
                });
                Helpers_CmdHandler$1__Handle_Z40332EE1(_cmdHandler, _cmds);
            };
            _cmdHandler = Helpers_cmdHandler(dispatch);
            _storeDispatch = [store, dispatch];
            return [store, dispatch];
        }
        else {
            const storeDispatch = _storeDispatch;
            return storeDispatch;
        }
    };
}

export function makeStore(init, dispose) {
    const s = Store$1_$ctor_130652E0(init, dispose);
    Registry_notifyMakeStore(s);
    rafu(() => {
        subscribe((v) => {
            Registry_notifyUpdateStore(s, v);
        }, s);
    });
    return s;
}

export function makeElmishWithDocument(doc, init, update, dispose) {
    return makeElmishWithCons(init, update, dispose, (i, d) => {
        const s = makeStore(i, d);
        const u = (f) => {
            Store$1__Update_Z1FC644CA(s, f);
            Event_notifyUpdated(doc);
        };
        return [s, u];
    });
}

export function makeElmishSimpleWithDocument(doc, init, update, dispose) {
    const init_1 = (p) => [init(p), empty()];
    const update_1 = (msg, model) => [update(msg, model), empty()];
    return makeElmishWithCons(init_1, update_1, dispose, (i, d) => {
        const s = makeStore(i, d);
        const u = (f) => {
            Store$1__Update_Z1FC644CA(s, f);
            Event_notifyUpdated(doc);
        };
        return [s, u];
    });
}

export function makeElmishSimple(i, u, d) {
    return makeElmishSimpleWithDocument(document, i, u, d);
}

export function makeElmish(i, u, d) {
    return makeElmishWithDocument(document, i, u, d);
}

