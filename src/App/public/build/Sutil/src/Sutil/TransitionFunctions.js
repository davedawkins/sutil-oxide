import { log as log_1, isEnabled } from "./Logging.js";
import { join, printf, toText, replace } from "../../../fable_modules/fable-library.3.7.20/String.js";
import { tryParse } from "../../../fable_modules/fable-library.3.7.20/Double.js";
import { FSharpRef } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { Transition, Transition_get_Default, applyProps } from "./Transition.js";
import { cubicInOut, cubicOut, linear } from "./Easing.js";
import { computedStyleOpacity } from "./DomHelpers.js";
import { Window_getComputedStyle_Z5966C024 } from "./Interop.js";
import { ofArray, map } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { curry, comparePrimitives, min } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { getItemFromDict } from "../../../fable_modules/fable-library.3.7.20/MapUtil.js";

function logEnabled() {
    return isEnabled("trfn");
}

const log = (message) => {
    log_1("trfn", message);
};

export function parseFloat$(s, name) {
    if (s == null) {
        return 0;
    }
    else {
        const s$0027 = replace(s, "px", "");
        let patternInput;
        let outArg = 0;
        patternInput = [tryParse(s$0027, new FSharpRef(() => outArg, (v) => {
            outArg = v;
        })), outArg];
        const success = patternInput[0];
        const num = patternInput[1];
        if (success) {
            return num;
        }
        else {
            return 0;
        }
    }
}

export function fade(initProps, node, unitVar) {
    let inputRecord;
    const tr = applyProps(initProps, (inputRecord = Transition_get_Default(), new Transition(inputRecord.Key, inputRecord.X, inputRecord.Y, inputRecord.Opacity, 0, 400, inputRecord.DurationFn, inputRecord.Speed, linear(), inputRecord.CssGen, inputRecord.Tick, inputRecord.Fallback)));
    return new Transition(tr.Key, tr.X, tr.Y, tr.Opacity, tr.Delay, tr.Duration, tr.DurationFn, tr.Speed, tr.Ease, (t, _arg) => {
        const arg = t * computedStyleOpacity(node);
        return toText(printf("opacity: %f"))(arg);
    }, tr.Tick, tr.Fallback);
}

export function slide(props, node) {
    let inputRecord;
    const tr = applyProps(props, (inputRecord = Transition_get_Default(), new Transition(inputRecord.Key, inputRecord.X, inputRecord.Y, inputRecord.Opacity, 0, 400, inputRecord.DurationFn, inputRecord.Speed, cubicOut, inputRecord.CssGen, inputRecord.Tick, inputRecord.Fallback)));
    const style = Window_getComputedStyle_Z5966C024(node);
    const opacity = parseFloat$(style.opacity, "opacity");
    const height = parseFloat$(style.height, "height");
    const padding_top = parseFloat$(style.paddingTop, "paddingTop");
    const padding_bottom = parseFloat$(style.paddingBottom, "paddingBottom");
    const margin_top = parseFloat$(style.marginTop, "marginTop");
    const margin_bottom = parseFloat$(style.marginBottom, "marginBottom");
    const border_top_width = parseFloat$(style.borderTopWidth, "borderTopWidth");
    const border_bottom_width = parseFloat$(style.borderBottomWidth, "borderBottomWidth");
    const set$ = (tupledArg) => {
        const name = tupledArg[0];
        const value = tupledArg[1];
        const units = tupledArg[2];
        return toText(printf("%s: %s%s;"))(name)(value)(units);
    };
    return () => (new Transition(tr.Key, tr.X, tr.Y, tr.Opacity, tr.Delay, tr.Duration, tr.DurationFn, tr.Speed, tr.Ease, (t_1, _arg) => {
        let value_1, value_2, value_3, value_4, value_5, value_6, value_7, value_8;
        const result = join("", map(set$, ofArray([["overflow", "hidden", ""], ["opacity", (value_1 = (min(comparePrimitives, t_1 * 20, 1) * opacity), value_1.toString()), ""], ["height", (value_2 = (t_1 * height), value_2.toString()), "px"], ["padding-top", (value_3 = (t_1 * padding_top), value_3.toString()), "px"], ["padding-bottom", (value_4 = (t_1 * padding_bottom), value_4.toString()), "px"], ["margin-top", (value_5 = (t_1 * margin_top), value_5.toString()), "px"], ["margin-bottom", (value_6 = (t_1 * margin_bottom), value_6.toString()), "px"], ["border-top-width", (value_7 = (t_1 * border_top_width), value_7.toString()), "px"], ["border-bottom-width", (value_8 = (t_1 * border_bottom_width), value_8.toString()), "px"]])));
        return result;
    }, tr.Tick, tr.Fallback));
}

export function draw(props, node) {
    let inputRecord;
    const tr = applyProps(props, (inputRecord = Transition_get_Default(), new Transition(inputRecord.Key, inputRecord.X, inputRecord.Y, inputRecord.Opacity, 0, 800, inputRecord.DurationFn, inputRecord.Speed, cubicInOut, inputRecord.CssGen, inputRecord.Tick, inputRecord.Fallback)));
    const len = node.getTotalLength();
    let duration;
    const matchValue = tr.Duration;
    if (matchValue === 0) {
        duration = ((tr.Speed === 0) ? 800 : (len / tr.Speed));
    }
    else {
        const d = matchValue;
        duration = d;
    }
    return () => (new Transition(tr.Key, tr.X, tr.Y, tr.Opacity, tr.Delay, duration, tr.DurationFn, tr.Speed, tr.Ease, (t_1, u) => {
        const arg_2 = u * len;
        const arg_1 = t_1 * len;
        return toText(printf("stroke-dasharray: %f %f"))(arg_1)(arg_2);
    }, tr.Tick, tr.Fallback));
}

export function fly(props, node) {
    let inputRecord;
    const tr = applyProps(props, (inputRecord = Transition_get_Default(), new Transition(inputRecord.Key, 0, 0, inputRecord.Opacity, 0, 400, inputRecord.DurationFn, inputRecord.Speed, cubicOut, inputRecord.CssGen, inputRecord.Tick, inputRecord.Fallback)));
    const style = Window_getComputedStyle_Z5966C024(node);
    const targetOpacity = computedStyleOpacity(node);
    const transform = (style.transform === "none") ? "" : style.transform;
    const od = targetOpacity * (1 - tr.Opacity);
    return () => (new Transition(tr.Key, tr.X, tr.Y, tr.Opacity, tr.Delay, tr.Duration, tr.DurationFn, tr.Speed, tr.Ease, (t_1, u) => {
        const arg_3 = targetOpacity - (od * u);
        const arg_2 = (1 - t_1) * tr.Y;
        const arg_1 = (1 - t_1) * tr.X;
        return toText(printf("transform: %s translate(%fpx, %fpx); opacity: %f;"))(transform)(arg_1)(arg_2)(arg_3);
    }, tr.Tick, tr.Fallback));
}

export function crossfade(userProps) {
    let tupledArg_3, clo_11, tupledArg_4, clo_14;
    const fallback = applyProps(userProps, Transition_get_Default()).Fallback;
    const toReceive = new Map([]);
    const toSend = new Map([]);
    const dump = () => {
        const ks = (d) => join(", ", d.keys());
        if (logEnabled()) {
            log(`toReceive = ${ks(toReceive)}`);
        }
        if (logEnabled()) {
            log(`toSend    = ${ks(toSend)}`);
        }
    };
    const crossfadeInner = (tupledArg) => {
        let inputRecord, arg_3, arg_2, arg_1, arg;
        const from = tupledArg[0];
        const node = tupledArg[1];
        const props = tupledArg[2];
        const intro = tupledArg[3];
        const tr_2 = applyProps(userProps, applyProps(props, (inputRecord = Transition_get_Default(), new Transition(inputRecord.Key, inputRecord.X, inputRecord.Y, inputRecord.Opacity, inputRecord.Delay, inputRecord.Duration, (d_1) => (Math.sqrt(d_1) * 30), inputRecord.Speed, cubicOut, inputRecord.CssGen, inputRecord.Tick, inputRecord.Fallback))));
        const tgt = node.getBoundingClientRect();
        const dx = from.left - tgt.left;
        const dy = from.top - tgt.top;
        const dw = from.width / tgt.width;
        const dh = from.height / tgt.height;
        if (logEnabled()) {
            log((arg_3 = tgt.top, (arg_2 = tgt.left, (arg_1 = from.top, (arg = from.left, toText(printf("crossfade from %f,%f -\u003e %f,%f"))(arg)(arg_1)(arg_2)(arg_3))))));
        }
        const d_2 = Math.sqrt((dx * dx) + (dy * dy));
        const style = Window_getComputedStyle_Z5966C024(node);
        const transform = (style.transform === "none") ? "" : style.transform;
        const opacity = computedStyleOpacity(node);
        let duration;
        const matchValue = tr_2.DurationFn;
        if (matchValue == null) {
            duration = tr_2.Duration;
        }
        else {
            const f = matchValue;
            duration = f(d_2);
        }
        return new Transition(tr_2.Key, tr_2.X, tr_2.Y, tr_2.Opacity, tr_2.Delay, duration, void 0, tr_2.Speed, tr_2.Ease, (t_1, u) => {
            const arg_10 = t_1 + ((1 - t_1) * dh);
            const arg_9 = t_1 + ((1 - t_1) * dw);
            const arg_8 = u * dy;
            const arg_7 = u * dx;
            const arg_5 = t_1 * opacity;
            return toText(printf("\n                      opacity: %f;\n                      transform-origin: top left;\n                      transform: %s translate(%fpx,%fpx) scale(%f, %f);"))(arg_5)(transform)(arg_7)(arg_8)(arg_9)(arg_10);
        }, tr_2.Tick, tr_2.Fallback);
    };
    const transition = (tupledArg_1) => {
        const items = tupledArg_1[0];
        const counterparts = tupledArg_1[1];
        const intro_1 = tupledArg_1[2];
        return (props_1) => ((node_1) => {
            const initProps = applyProps(props_1, Transition_get_Default());
            const key = initProps.Key;
            const r = node_1.getBoundingClientRect();
            const action = intro_1 ? "receiving" : "sending";
            if (logEnabled()) {
                log(`${action} ${key} (adding)`);
            }
            items.set(key, r);
            const trfac = () => {
                const finalProps = props_1;
                if (counterparts.has(key)) {
                    const rect = getItemFromDict(counterparts, key);
                    if (logEnabled()) {
                        log(`${action} ${key} (removing from counterparts)`);
                    }
                    counterparts.delete(key);
                    const tupledArg_2 = [rect, node_1, finalProps, intro_1];
                    return crossfadeInner([tupledArg_2[0], tupledArg_2[1], tupledArg_2[2], tupledArg_2[3]]);
                }
                else {
                    items.delete(key);
                    if (logEnabled()) {
                        log(`${action} falling back for ${key}`);
                    }
                    if (curry(3, fallback) == null) {
                        return fade(finalProps, node_1, void 0);
                    }
                    else {
                        const f_1 = curry(3, fallback);
                        return f_1(finalProps)(node_1)(void 0);
                    }
                }
            };
            return trfac;
        });
    };
    return [(tupledArg_3 = [toSend, toReceive, false], (clo_11 = transition([tupledArg_3[0], tupledArg_3[1], tupledArg_3[2]]), (arg_18) => {
        const clo_12 = clo_11(arg_18);
        return (arg_19) => {
            const clo_13 = clo_12(arg_19);
            return clo_13;
        };
    })), (tupledArg_4 = [toReceive, toSend, true], (clo_14 = transition([tupledArg_4[0], tupledArg_4[1], tupledArg_4[2]]), (arg_24) => {
        const clo_15 = clo_14(arg_24);
        return (arg_25) => {
            const clo_16 = clo_15(arg_25);
            return clo_16;
        };
    }))];
}

