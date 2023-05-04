import Timer from "../../../fable_modules/fable-library.3.7.20/Timer.js";
import { add } from "../../../fable_modules/fable-library.3.7.20/Observable.js";
import { concat, map, singleton, empty } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { singleton as singleton_1 } from "../../../fable_modules/fable-library.3.7.20/AsyncBuilder.js";
import { startImmediate, catchAsync } from "../../../fable_modules/fable-library.3.7.20/Async.js";

export function Timer_delay(interval, callback) {
    let t;
    let returnVal = new Timer(interval);
    returnVal.AutoReset = false;
    t = returnVal;
    add(callback, t.Elapsed());
    t.Enabled = true;
    t.Start();
}

export function Cmd_none() {
    return empty();
}

export function Cmd_ofEffect(effect) {
    return singleton(effect);
}

export function Cmd_map(f, cmd) {
    return map((g) => ((arg_1) => {
        g((arg) => {
            arg_1(f(arg));
        });
    }), cmd);
}

export function Cmd_ofMsg(msg) {
    return singleton((d) => {
        d(msg);
    });
}

export function Cmd_batch(cmds) {
    return concat(cmds);
}

export function Cmd_OfFunc_either(task, a, success, error) {
    return singleton((d) => {
        try {
            return d(success(task(a)));
        }
        catch (x) {
            return d(error(x));
        }
    });
}

export function Cmd_OfFunc_perform(task, a, success) {
    return singleton((d) => {
        try {
            d(success(task(a)));
        }
        catch (matchValue) {
        }
    });
}

export function Cmd_OfFunc_attempt(task, a, error) {
    return singleton((d) => {
        try {
            task(a);
        }
        catch (x) {
            d(error(x));
        }
    });
}

export function Cmd_OfFunc_exec(task, a) {
    return singleton((d) => {
        try {
            task(a);
        }
        catch (matchValue) {
        }
    });
}

export function Cmd_OfAsyncWith_either(start, task, arg, ofSuccess, ofError) {
    const bind = (dispatch) => singleton_1.Delay(() => singleton_1.Bind(catchAsync(task(arg)), (_arg) => {
        let x_1, x;
        const r = _arg;
        dispatch((r.tag === 1) ? ((x_1 = r.fields[0], ofError(x_1))) : ((x = r.fields[0], ofSuccess(x))));
        return singleton_1.Zero();
    }));
    return singleton((arg_2) => {
        start(bind(arg_2));
    });
}

export function Cmd_OfAsyncWith_perform(start, task, arg, ofSuccess) {
    const bind = (dispatch) => singleton_1.Delay(() => singleton_1.Bind(catchAsync(task(arg)), (_arg) => {
        const r = _arg;
        if (r.tag === 0) {
            const x = r.fields[0];
            dispatch(ofSuccess(x));
            return singleton_1.Zero();
        }
        else {
            return singleton_1.Zero();
        }
    }));
    return singleton((arg_2) => {
        start(bind(arg_2));
    });
}

export function Cmd_OfAsyncWith_attempt(start, task, arg, ofError) {
    const bind = (dispatch) => singleton_1.Delay(() => singleton_1.Bind(catchAsync(task(arg)), (_arg) => {
        const r = _arg;
        if (r.tag === 1) {
            const x = r.fields[0];
            dispatch(ofError(x));
            return singleton_1.Zero();
        }
        else {
            return singleton_1.Zero();
        }
    }));
    return singleton((arg_2) => {
        start(bind(arg_2));
    });
}

export function Cmd_OfAsyncWith_result(start, task) {
    const bind = (dispatch) => singleton_1.Delay(() => singleton_1.Bind(task, (_arg) => {
        const r = _arg;
        dispatch(r);
        return singleton_1.Zero();
    }));
    return singleton((arg) => {
        start(bind(arg));
    });
}

export function Cmd_OfAsync_start(x) {
    Timer_delay(0, (_arg) => {
        startImmediate(x);
    });
}

export function Cmd_OfPromise_either(task, arg, ofSuccess, ofError) {
    const bind = (dispatch) => {
        task(arg).then((arg_1) => dispatch(ofSuccess(arg_1))).catch((arg_3) => dispatch(ofError(arg_3)));
    };
    return singleton(bind);
}

export function Cmd_OfPromise_perform(task, arg, ofSuccess) {
    const bind = (dispatch) => {
        task(arg).then((arg_1) => dispatch(ofSuccess(arg_1)));
    };
    return singleton(bind);
}

export function Cmd_OfPromise_attempt(task, arg, ofError) {
    const bind = (dispatch) => {
        task(arg).catch((arg_2) => {
            dispatch(ofError(arg_2));
        });
    };
    return singleton(bind);
}

export function Cmd_OfPromise_result(task) {
    const bind = (dispatch) => {
        task.then(dispatch);
    };
    return singleton(bind);
}

