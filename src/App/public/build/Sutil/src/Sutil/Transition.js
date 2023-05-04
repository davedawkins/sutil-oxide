import { log as log_2, isEnabled } from "./Logging.js";
import { Union, Record } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { option_type, union_type, list_type, string_type, class_type, record_type, unit_type, lambda_type, bool_type, float64_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { HashSet } from "../../../fable_modules/fable-library.3.7.20/MutableSet.js";
import { curry, disposeSafe, getEnumerator, partialApply, uncurry, safeHash, equals } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { wait, CustomDispatch$1_dispatch_4FBB8B24, NodeKey_clear, NodeKey_TickTask, NodeKey_get, nodeStr, documentOf, raf } from "./DomHelpers.js";
import { addToSet } from "../../../fable_modules/fable-library.3.7.20/MapUtil.js";
import { quintOut, linear } from "./Easing.js";
import { iterate, map as map_2, skip, head, singleton, cons, isEmpty, length, empty, fold, append } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { DevToolsControl_Options } from "./Types.js";
import { defaultArg, map } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { FSharpMap__get_Count, FSharpMap__Add, empty as empty_1 } from "../../../fable_modules/fable-library.3.7.20/Map.js";
import { Window_getComputedStyle_Z5966C024, Window_requestAnimationFrame_1A119E11 } from "./Interop.js";
import { makeIdGenerator } from "./Helpers.js";
import { join, printf, toText, isNullOrEmpty } from "../../../fable_modules/fable-library.3.7.20/String.js";
import { toList } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { rangeDouble } from "../../../fable_modules/fable-library.3.7.20/Range.js";
import { map as map_1 } from "../../../fable_modules/fable-library.3.7.20/Array.js";
import { showEl } from "./Styling.js";
import { ContextHelpers_withParent, BuildContext__AddChild_2AD740C9, SutilEffect_MakeGroup_5BDBED5B, SutilElement_Define_7B1F8004, build, SutilEffect__get_IsEmpty, SutilElement_Define_Z60F5000F, SutilEffect__collectDomNodes, SutilEffect, SutilEffect$reflection, SutilElement$reflection } from "./Core.js";
import { Store_map, Store_subscribe } from "./Store.js";

function logEnabled() {
    return isEnabled("trans");
}

function log(s) {
    log_2("trans", s);
}

class LoopTasks_Task extends Record {
    constructor(C, F) {
        super();
        this.C = C;
        this.F = F;
    }
}

function LoopTasks_Task$reflection() {
    return record_type("Sutil.Transition.LoopTasks.Task", [], LoopTasks_Task, () => [["C", lambda_type(float64_type, bool_type)], ["F", lambda_type(unit_type, unit_type)]]);
}

class LoopTasks_LoopTask extends Record {
    constructor(Promise$, Abort) {
        super();
        this.Promise = Promise$;
        this.Abort = Abort;
    }
}

function LoopTasks_LoopTask$reflection() {
    return record_type("Sutil.Transition.LoopTasks.LoopTask", [], LoopTasks_LoopTask, () => [["Promise", class_type("Fable.Core.JS.Promise`1", [unit_type])], ["Abort", lambda_type(unit_type, unit_type)]]);
}

let LoopTasks_tasks = new HashSet([], {
    Equals: equals,
    GetHashCode: safeHash,
});

function LoopTasks_runTasks(now) {
    const array = Array.from(LoopTasks_tasks);
    array.forEach((task) => {
        if (!task.C(now)) {
            LoopTasks_tasks.delete(task);
            task.F();
        }
    });
    if (LoopTasks_tasks.size !== 0) {
        raf((now_1) => {
            LoopTasks_runTasks(now_1);
        });
    }
}

const LoopTasks_clearLoops = LoopTasks_tasks.clear();

function LoopTasks_loop(callback) {
    let task = null;
    if (LoopTasks_tasks.size === 0) {
        raf((now) => {
            LoopTasks_runTasks(now);
        });
    }
    return new LoopTasks_LoopTask(new Promise((fulfill, _arg) => {
        task = (new LoopTasks_Task(callback, fulfill));
        addToSet(task, LoopTasks_tasks);
    }), () => {
        LoopTasks_tasks.delete(task);
    });
}

export class TransitionProp extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Key", "X", "Y", "Opacity", "Delay", "Duration", "DurationFn", "Ease", "CssGen", "Tick", "Speed", "Fallback"];
    }
}

export function TransitionProp$reflection() {
    return union_type("Sutil.Transition.TransitionProp", [], TransitionProp, () => [[["Item", string_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", float64_type]], [["Item", lambda_type(float64_type, float64_type)]], [["Item", lambda_type(float64_type, float64_type)]], [["Item", lambda_type(float64_type, lambda_type(float64_type, string_type))]], [["Item", lambda_type(float64_type, lambda_type(float64_type, unit_type))]], [["Item", float64_type]], [["Item", lambda_type(list_type(TransitionProp$reflection()), lambda_type(class_type("Browser.Types.HTMLElement", void 0, HTMLElement), lambda_type(unit_type, Transition$reflection())))]]]);
}

export class Transition extends Record {
    constructor(Key, X, Y, Opacity, Delay, Duration, DurationFn, Speed, Ease, CssGen, Tick, Fallback) {
        super();
        this.Key = Key;
        this.X = X;
        this.Y = Y;
        this.Opacity = Opacity;
        this.Delay = Delay;
        this.Duration = Duration;
        this.DurationFn = DurationFn;
        this.Speed = Speed;
        this.Ease = Ease;
        this.CssGen = CssGen;
        this.Tick = Tick;
        this.Fallback = Fallback;
    }
}

export function Transition$reflection() {
    return record_type("Sutil.Transition.Transition", [], Transition, () => [["Key", string_type], ["X", float64_type], ["Y", float64_type], ["Opacity", float64_type], ["Delay", float64_type], ["Duration", float64_type], ["DurationFn", option_type(lambda_type(float64_type, float64_type))], ["Speed", float64_type], ["Ease", lambda_type(float64_type, float64_type)], ["CssGen", option_type(lambda_type(float64_type, lambda_type(float64_type, string_type)))], ["Tick", option_type(lambda_type(float64_type, lambda_type(float64_type, unit_type)))], ["Fallback", option_type(lambda_type(list_type(TransitionProp$reflection()), lambda_type(class_type("Browser.Types.HTMLElement", void 0, HTMLElement), lambda_type(unit_type, Transition$reflection()))))]]);
}

export function Transition_get_Default() {
    return new Transition("", 0, 0, 0, 0, 0, void 0, 0, linear(), uncurry(2, void 0), uncurry(2, void 0), uncurry(3, void 0));
}

export class Animation extends Record {
    constructor(From, To) {
        super();
        this.From = From;
        this.To = To;
    }
}

export function Animation$reflection() {
    return record_type("Sutil.Transition.Animation", [], Animation, () => [["From", class_type("Browser.Types.ClientRect", void 0, ClientRect)], ["To", class_type("Browser.Types.ClientRect", void 0, ClientRect)]]);
}

export function mergeProps(newerProps, existingProps) {
    return append(existingProps, newerProps);
}

export function withProps(userProps, f, initProps) {
    return partialApply(2, f, [mergeProps(userProps, initProps)]);
}

export class TransitionAttribute extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["InOut", "In", "Out"];
    }
}

export function TransitionAttribute$reflection() {
    return union_type("Sutil.Transition.TransitionAttribute", [], TransitionAttribute, () => [[["Item", lambda_type(list_type(TransitionProp$reflection()), lambda_type(class_type("Browser.Types.HTMLElement", void 0, HTMLElement), lambda_type(unit_type, Transition$reflection())))]], [["Item", lambda_type(list_type(TransitionProp$reflection()), lambda_type(class_type("Browser.Types.HTMLElement", void 0, HTMLElement), lambda_type(unit_type, Transition$reflection())))]], [["Item", lambda_type(list_type(TransitionProp$reflection()), lambda_type(class_type("Browser.Types.HTMLElement", void 0, HTMLElement), lambda_type(unit_type, Transition$reflection())))]]]);
}

function overrideDuration(d) {
    if (DevToolsControl_Options().SlowAnimations) {
        return 10 * d;
    }
    else {
        return d;
    }
}

function overrideDurationFn(fo) {
    if (DevToolsControl_Options().SlowAnimations) {
        return map((f) => ((arg) => (10 * f(arg))), fo);
    }
    else {
        return fo;
    }
}

function applyProp(r, prop) {
    switch (prop.tag) {
        case 5: {
            const d_1 = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, d_1, void 0, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 6: {
            const fo = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, 0, fo, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 7: {
            const f = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, f, r.CssGen, r.Tick, r.Fallback);
        }
        case 8: {
            const f_1 = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, f_1, r.Tick, r.Fallback);
        }
        case 9: {
            const f_2 = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, f_2, r.Fallback);
        }
        case 10: {
            const s = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, s, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 1: {
            const n = prop.fields[0];
            return new Transition(r.Key, n, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 2: {
            const n_1 = prop.fields[0];
            return new Transition(r.Key, r.X, n_1, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 3: {
            const n_2 = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, n_2, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 0: {
            const f_3 = prop.fields[0];
            return new Transition(f_3, r.X, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
        case 11: {
            const f_4 = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, r.Delay, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, r.Tick, f_4);
        }
        default: {
            const d = prop.fields[0];
            return new Transition(r.Key, r.X, r.Y, r.Opacity, d, r.Duration, r.DurationFn, r.Speed, r.Ease, r.CssGen, r.Tick, r.Fallback);
        }
    }
}

export function applyProps(props, tr) {
    return fold(applyProp, tr, props);
}

export function makeTransition(props) {
    return applyProps(props, Transition_get_Default());
}

export function mapTrans(f, t) {
    return applyProps(f(t), t);
}

export function element(doc, tag) {
    return doc.createElement(tag);
}

let numActiveAnimations = 0;

let tasks = empty();

let activeDocs = empty_1();

function registerDoc(doc) {
    activeDocs = FSharpMap__Add(activeDocs, safeHash(doc), doc);
    if (logEnabled()) {
        log(`Active docs: ${FSharpMap__get_Count(activeDocs)}`);
    }
}

function runTasks() {
    const copy = tasks;
    tasks = empty();
    if (length(copy) > 0) {
        if (logEnabled()) {
            log(`- - - Tasks: running ${length(copy)} tasks - - - - - - - - - - - - - -`);
        }
    }
    const enumerator = getEnumerator(copy);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const f = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            f();
        }
    }
    finally {
        disposeSafe(enumerator);
    }
}

function waitAnimationFrame(f) {
    const init = isEmpty(tasks);
    tasks = cons(f, tasks);
    if (init) {
        Window_requestAnimationFrame_1A119E11((_arg) => {
            runTasks();
        });
    }
}

function getSutilStyleElement(doc) {
    let e = doc.querySelector("head style#__sutil_keyframes");
    if (e == null) {
        e = element(doc, "style");
        e.setAttribute("id", "__sutil_keyframes");
        doc.head.appendChild(e);
    }
    return e;
}

function dotSheet(styleElem) {
    return styleElem["sheet"];
}

function getSutilStylesheet(doc) {
    return dotSheet(getSutilStyleElement(doc));
}

const nextRuleId = makeIdGenerator();

function toEmptyStr(s) {
    if (isNullOrEmpty(s)) {
        return "";
    }
    else {
        return s;
    }
}

export function createRule(node, a, b, tr, uid) {
    let arg_1, arg, arg_2, arg_6, list;
    registerDoc(documentOf(node));
    let css;
    const matchValue = tr.CssGen;
    if (curry(2, matchValue) == null) {
        throw (new Error("No CSS function supplied"));
    }
    else {
        const f = curry(2, matchValue);
        css = f;
    }
    if (tr.DurationFn != null) {
        throw (new Error("Duration function not permitted in createRule"));
    }
    const durn = overrideDuration(tr.Duration);
    const step = 16.666 / durn;
    let keyframes = singleton("{\n");
    const enumerator = getEnumerator(toList(rangeDouble(0, step, 1)));
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const p = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            const t = a + ((b - a) * tr.Ease(p));
            keyframes = append(keyframes, singleton((arg_1 = css(t)(1 - t), (arg = (p * 100), toText(printf("%f%%{%s}\n"))(arg)(arg_1)))));
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    const rule = join("", append(keyframes, singleton((arg_2 = css(b)(1 - b), toText(printf("100%% {%s}\n"))(arg_2)))));
    let name;
    const arg_3 = ((uid === 0) ? nextRuleId() : uid) | 0;
    name = toText(printf("__sutil_%d"))(arg_3);
    const keyframeText = toText(printf("@keyframes %s %s"))(name)(rule);
    if (logEnabled()) {
        log((arg_6 = head((list = keyframes, skip(~(~(length(keyframes) / 2)), list))), toText(printf("keyframe: %s"))(arg_6)));
    }
    if (logEnabled()) {
        log(`createRule ${name} ${durn}ms for ${nodeStr(node)}`);
    }
    const stylesheet = getSutilStylesheet(documentOf(node));
    stylesheet.insertRule(keyframeText, stylesheet.cssRules.length);
    const animations = append(isNullOrEmpty((node.style).animation) ? empty() : singleton((node.style).animation), singleton(toText(printf("%s %fms linear %fms 1 both"))(name)(durn)(tr.Delay)));
    (node.style).animation = join(", ", animations);
    numActiveAnimations = ((numActiveAnimations + 1) | 0);
    return name;
}

export function clearAnimations(node) {
    (node.style).animation = "";
}

function clearRules() {
    Window_requestAnimationFrame_1A119E11((_arg) => {
        let arg;
        if (numActiveAnimations === 0) {
            const enumerator = getEnumerator(activeDocs);
            try {
                while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                    const kv = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                    const doc = kv[1];
                    const stylesheet = getSutilStylesheet(doc);
                    if (logEnabled()) {
                        log((arg = ((~(~stylesheet.cssRules.length)) | 0), toText(printf("clearing %d rules"))(arg)));
                    }
                    const enumerator_1 = getEnumerator(toList(rangeDouble((~(~stylesheet.cssRules.length)) - 1, -1, 0)));
                    try {
                        while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                            const i = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
                            stylesheet.deleteRule(i);
                        }
                    }
                    finally {
                        disposeSafe(enumerator_1);
                    }
                }
            }
            finally {
                disposeSafe(enumerator);
            }
        }
        activeDocs = empty_1();
    });
}

function deleteRule(node, name) {
    const previous = toEmptyStr((node.style).animation).split(",");
    let next;
    const predicate = isNullOrEmpty(name) ? ((anim) => (anim.indexOf(name) < 0)) : ((anim_1) => (anim_1.indexOf("__sutil") < 0));
    next = previous.filter(predicate);
    const deleted = (previous.length - next.length) | 0;
    if (deleted > 0) {
        (node.style).animation = join(", ", map_1((s) => s.trim(), next));
        numActiveAnimations = ((numActiveAnimations - deleted) | 0);
        if (numActiveAnimations === 0) {
            clearRules();
        }
    }
}

function rectToStr(c) {
    const arg_3 = c.bottom;
    const arg_2 = c.right;
    const arg_1 = c.top;
    const arg = c.left;
    return toText(printf("[%f,%f -\u003e %f,%f]"))(arg)(arg_1)(arg_2)(arg_3);
}

export function flip(node, animation, props) {
    let inputRecord, arg_4, arg_3, matchValue, f;
    const tr = applyProps(props, (inputRecord = Transition_get_Default(), new Transition(inputRecord.Key, inputRecord.X, inputRecord.Y, inputRecord.Opacity, 0, inputRecord.Duration, (d) => (Math.sqrt(d) * 60), inputRecord.Speed, quintOut, inputRecord.CssGen, inputRecord.Tick, inputRecord.Fallback)));
    const style = Window_getComputedStyle_Z5966C024(node);
    const transform = (style.transform === "none") ? "" : style.transform;
    const scaleX = animation.From.width / node.clientWidth;
    const scaleY = animation.From.height / node.clientHeight;
    const dx = (animation.From.left - animation.To.left) / scaleX;
    const dy = (animation.From.top - animation.To.top) / scaleY;
    const d_1 = Math.sqrt((dx * dx) + (dy * dy));
    if (true) {
        log((arg_4 = rectToStr(animation.To), (arg_3 = rectToStr(animation.From), toText(printf("flip: %A,%A %A %A -\u003e %A"))(dx)(dy)(transform)(arg_3)(arg_4))));
    }
    return new Transition(tr.Key, tr.X, tr.Y, tr.Opacity, tr.Delay, (matchValue = tr.DurationFn, (matchValue != null) ? ((f = matchValue, f(d_1))) : tr.Duration), void 0, tr.Speed, tr.Ease, (t_1, u) => {
        const arg_7 = u * dy;
        const arg_6 = u * dx;
        return toText(printf("transform: %s translate(%fpx, %fpx);`"))(transform)(arg_6)(arg_7);
    }, tr.Tick, tr.Fallback);
}

export function createAnimation(node, from, animateFn, props) {
    const tgt = node.getBoundingClientRect();
    const shouldCreate = (((!(from == null)) && (!(from.width === 0))) && (!(from.height === 0))) && (!((((from.left === tgt.left) && (from.right === tgt.right)) && (from.top === tgt.top)) && (from.bottom === tgt.bottom)));
    if (shouldCreate) {
        let a;
        const arg_1 = new Animation(from, tgt);
        a = animateFn(node, arg_1, props);
        const r = new Transition(a.Key, a.X, a.Y, a.Opacity, a.Delay, ((a.Duration === 0) && (a.DurationFn == null)) ? 300 : a.Duration, a.DurationFn, a.Speed, a.Ease, a.CssGen, a.Tick, a.Fallback);
        return createRule(node, 0, 1, r, 0);
    }
    else {
        return "";
    }
}

function waitAnimationEnd(el, f) {
    const cb = (_arg) => {
        el.removeEventListener("animationend", cb);
        f();
    };
    el.addEventListener("animationend", cb);
}

export function animateNode(node, from) {
    waitAnimationFrame(() => {
        const name = createAnimation(node, from, flip, empty());
        waitAnimationEnd(node, () => {
            deleteRule(node, name);
        });
    });
}

const tickGen = makeIdGenerator();

function findTransition(intro, trans) {
    let matchValue, x_2, x_3, x_1;
    let result = void 0;
    const enumerator = getEnumerator(trans);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const x = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            result = ((matchValue = [result, x, intro], (matchValue[0] == null) ? ((matchValue[1].tag === 2) ? (matchValue[2] ? (void 0) : ((x_2 = curry(3, matchValue[1].fields[0]), x_2))) : ((matchValue[1].tag === 0) ? ((x_3 = curry(3, matchValue[1].fields[0]), x_3)) : (matchValue[2] ? ((x_1 = curry(3, matchValue[1].fields[0]), x_1)) : (void 0)))) : result));
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    return result;
}

export function transitionNode(el, trans, initProps, isVisible, start, complete) {
    let ruleName = "";
    const cancelTick = () => {
        const matchValue = NodeKey_get(el, NodeKey_TickTask);
        if (matchValue == null) {
        }
        else {
            const f = matchValue;
            NodeKey_clear(el, NodeKey_TickTask);
            f();
        }
    };
    const runTick = (tr, b, durn) => {
        const logEnabled_1 = isEnabled("tick");
        const log_1 = (message) => {
            log_2("tick", message);
        };
        const a = (b === 0) ? 1 : 0;
        const d = b - a;
        const tickId = tickGen() | 0;
        let tick;
        const matchValue_1 = tr.Tick;
        if (curry(2, matchValue_1) == null) {
            throw (new Error("No tick function supplied"));
        }
        else {
            const f_1 = curry(2, matchValue_1);
            tick = f_1;
        }
        const ease = tr.Ease;
        const delay = tr.Delay;
        let t = a;
        let start_1 = 0;
        let finish = 0;
        let started = false;
        let finished = false;
        el[NodeKey_TickTask] = (() => {
            if (logEnabled_1) {
                log_1(`#${tickId}: cancel`);
            }
            finished = true;
        });
        if (logEnabled_1) {
            log_1(`#${tickId}: run b=${b} durn=${durn}`);
        }
        if (b > 0) {
            tick(0)(1);
        }
        return LoopTasks_loop((now) => {
            if (!started) {
                start_1 = (now + delay);
                finish = (start_1 + durn);
                if (logEnabled_1) {
                    log_1(`#${tickId}: start: start=${start_1} finish=${finish}`);
                }
                started = true;
            }
            if (finished ? true : (now >= finish)) {
                if (logEnabled_1) {
                    log_1(`#${tickId}: finish: t=${t}`);
                }
                t = b;
                tick(t)(1 - t);
                finished = true;
            }
            else if (now >= start_1) {
                const e = now - start_1;
                const t0 = e / durn;
                t = (a + (d * ease(t0)));
                if (logEnabled_1) {
                    log_1(`#${tickId}: tick: t=${t} t0=${t0} e=${e}`);
                }
                tick(t)(1 - t);
            }
            return !finished;
        });
    };
    const hide = () => {
        if (logEnabled()) {
            log(`hide ${nodeStr(el)}`);
        }
        showEl(el, false);
        complete(el);
        if (ruleName !== "") {
            deleteRule(el, ruleName);
        }
        CustomDispatch$1_dispatch_4FBB8B24(el, "outroend");
    };
    const show = () => {
        if (logEnabled()) {
            log(`show ${nodeStr(el)}`);
        }
        showEl(el, true);
        complete(el);
        if (ruleName !== "") {
            deleteRule(el, ruleName);
        }
        CustomDispatch$1_dispatch_4FBB8B24(el, "introend");
    };
    const tr_1 = findTransition(isVisible, trans);
    const startTransition = (createTrans) => {
        const event = isVisible ? "introstart" : "outrostart";
        const patternInput = isVisible ? [0, 1] : [1, 0];
        const b_1 = patternInput[1];
        const a_1 = patternInput[0];
        const onEnd = isVisible ? show : hide;
        cancelTick();
        waitAnimationFrame(() => {
            CustomDispatch$1_dispatch_4FBB8B24(el, event);
            start(el);
            waitAnimationEnd(el, onEnd);
            if (isVisible) {
                showEl(el, true);
            }
            const tr_2 = createTrans();
            if (tr_2.DurationFn != null) {
                throw (new Error("Duration function not permitted"));
            }
            const d_1 = tr_2.Duration;
            if (curry(2, tr_2.CssGen) != null) {
                ruleName = createRule(el, a_1, b_1, tr_2, 0);
            }
            if (curry(2, tr_2.Tick) != null) {
                wait(el, () => {
                    const t_1 = runTick(tr_2, b_1, d_1);
                    return t_1.Promise;
                });
            }
        });
    };
    const _display = (el.style).display;
    if (((!(_display == null)) && (_display !== "")) && (_display !== "none")) {
        el["_display"] = _display;
    }
    if (tr_1 != null) {
        const init = tr_1;
        deleteRule(el, "");
        const createTrans_1 = init(initProps)(el);
        startTransition(createTrans_1);
    }
    else {
        showEl(el, isVisible);
        complete(el);
    }
}

export class Hideable extends Record {
    constructor(predicate, element, transOpt) {
        super();
        this.predicate = predicate;
        this.element = element;
        this.transOpt = transOpt;
    }
}

export function Hideable$reflection() {
    return record_type("Sutil.Transition.Hideable", [], Hideable, () => [["predicate", class_type("System.IObservable`1", [bool_type])], ["element", SutilElement$reflection()], ["transOpt", list_type(TransitionAttribute$reflection())]]);
}

export class HideableRuntime extends Record {
    constructor(hideable, target, cache, unsubscribe) {
        super();
        this.hideable = hideable;
        this.target = target;
        this.cache = cache;
        this.unsubscribe = unsubscribe;
    }
}

export function HideableRuntime$reflection() {
    return record_type("Sutil.Transition.HideableRuntime", [], HideableRuntime, () => [["hideable", Hideable$reflection()], ["target", SutilEffect$reflection()], ["cache", bool_type], ["unsubscribe", class_type("System.IDisposable")]]);
}

export function createHideableRuntime(h) {
    return new HideableRuntime(h, new SutilEffect(0), false, null);
}

export function collectNodes(sn) {
    return defaultArg(map(SutilEffect__collectDomNodes, sn), empty());
}

export function transitionList(list) {
    return SutilElement_Define_Z60F5000F("transitionList", (ctx) => {
        const runtimes = map_2(createHideableRuntime, list);
        const enumerator = getEnumerator(runtimes);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const rt = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                rt.unsubscribe = Store_subscribe((show) => {
                    if (SutilEffect__get_IsEmpty(rt.target)) {
                        rt.target = build(rt.hideable.element, ctx);
                        rt.cache = (!show);
                    }
                    if (rt.cache !== show) {
                        rt.cache = show;
                        iterate((node) => {
                            transitionNode(node, rt.hideable.transOpt, empty(), show, (value) => {
                            }, (value_1) => {
                            });
                        }, SutilEffect__collectDomNodes(rt.target));
                    }
                }, rt.hideable.predicate);
            }
        }
        finally {
            disposeSafe(enumerator);
        }
    });
}

export function makeHideable(guard, element_1, transOpt) {
    return new Hideable(guard, element_1, transOpt);
}

export function transitionMatch(store, options) {
    return transitionList(map_2((tupledArg) => {
        const p = tupledArg[0];
        const e = tupledArg[1];
        const t = tupledArg[2];
        return makeHideable(Store_map(p, store), e, t);
    }, options));
}

export function transitionOpt(trans, store, element_1, elseElement) {
    return SutilElement_Define_7B1F8004("transitionOpt", (ctx) => {
        const transResult = new SutilEffect(2, SutilEffect_MakeGroup_5BDBED5B("transition", ctx.Parent, ctx.Previous));
        BuildContext__AddChild_2AD740C9(ctx, transResult);
        const transCtx = ContextHelpers_withParent(transResult, ctx);
        let target = new SutilEffect(0);
        let cache = false;
        let targetElse = new SutilEffect(0);
        const unsub = Store_subscribe((isVisible) => {
            const wantTransition = !SutilEffect__get_IsEmpty(target);
            if (SutilEffect__get_IsEmpty(target)) {
                target = build(element_1, transCtx);
                cache = (!isVisible);
                if (elseElement == null) {
                }
                else {
                    const e = elseElement;
                    targetElse = build(e, transCtx);
                }
            }
            if (cache !== isVisible) {
                cache = isVisible;
                const trans$0027 = wantTransition ? trans : empty();
                iterate((node) => {
                    transitionNode(node, trans$0027, empty(), isVisible, (value) => {
                    }, (value_1) => {
                    });
                }, SutilEffect__collectDomNodes(target));
                iterate((node_1) => {
                    transitionNode(node_1, trans$0027, empty(), !isVisible, (value_2) => {
                    }, (value_3) => {
                    });
                }, SutilEffect__collectDomNodes(targetElse));
            }
        }, store);
        return transResult;
    });
}

export function transition(options, visibility, element_1) {
    return transitionOpt(options, visibility, element_1, void 0);
}

export function transitionElse(options, visibility, element_1, otherElement) {
    return transitionOpt(options, visibility, element_1, otherElement);
}

export function showIf(visibility, element_1) {
    return transitionOpt(empty(), visibility, element_1, void 0);
}

export function showIfElse(visibility, element_1, otherElement) {
    return transitionOpt(empty(), visibility, element_1, otherElement);
}

