import { error, log as log_1, isEnabled } from "./Logging.js";
import { value as value_3, some } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { toString, Union } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { union_type, bool_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { equals, disposeSafe, getEnumerator } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { iterate, length, singleton, empty } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { makeIdGenerator } from "./Helpers.js";
import { printf, toText, split, substring, replace } from "../../../fable_modules/fable-library.3.7.20/String.js";
import { toList, map as map_1, collect, empty as empty_1, singleton as singleton_1, append, delay } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { Window_getComputedStyle_Z5966C024, Window_requestAnimationFrame_1A119E11 } from "./Interop.js";
import { parse } from "../../../fable_modules/fable-library.3.7.20/Double.js";
import { rangeDouble } from "../../../fable_modules/fable-library.3.7.20/Range.js";

function logEnabled() {
    return isEnabled("dom");
}

function log(s) {
    log_1("dom", s);
}

export const SvIdKey = "_svid";

export function asElement(target) {
    return target;
}

export const NodeKey_Disposables = "__sutil_disposables";

export const NodeKey_ResizeObserver = "__sutil_resizeObserver";

export const NodeKey_TickTask = "__sutil_tickTask";

export const NodeKey_Promise = "__sutil_promise";

export const NodeKey_NodeMap = "__sutil_nodes";

export const NodeKey_Groups = "__sutil_groups";

export const NodeKey_StyleClass = "__sutil_styleclass";

export function NodeKey_clear(node, key) {
    delete node[key];
}

export function NodeKey_get(node, key) {
    const v = node[key];
    if (v == null) {
        return void 0;
    }
    else {
        return some(v);
    }
}

export function NodeKey_setUnlessExists(node, key, value) {
    if (!(node.hasOwnProperty(key))) {
        node[key] = value;
    }
}

export function NodeKey_getCreate(node, key, cons) {
    const matchValue = NodeKey_get(node, key);
    if (matchValue == null) {
        const newVal = cons();
        node[key] = newVal;
        return newVal;
    }
    else {
        const v = value_3(matchValue);
        return v;
    }
}

export const Event_NewStore = "sutil-new-store";

export const Event_UpdateStore = "sutil-update-store";

export const Event_ElementReady = "sutil-element-ready";

export const Event_Mount = "sutil-mount";

export const Event_Unmount = "sutil-unmount";

export const Event_Show = "sutil-show";

export const Event_Hide = "sutil-hide";

export const Event_Updated = "sutil-updated";

export const Event_Connected = "sutil-connected";

export function Event_notifyEvent(doc, name, data) {
    doc.dispatchEvent(new CustomEvent(name, data));
}

export function Event_notifyUpdated(doc) {
    if (logEnabled()) {
        log("notify document");
    }
    Event_notifyEvent(doc, Event_Updated, {});
}

function dispatch(target, name, data) {
    if (!(target == null)) {
        target.dispatchEvent(new CustomEvent(name, data));
    }
}

function dispatchSimple(target, name) {
    dispatch(target, name, {});
}

function dispatchCustom(target, name, init) {
    if (!(target == null)) {
        target.dispatchEvent(new CustomEvent(name, init));
    }
}

export class CustomDispatch$1 extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Detail", "Bubbles", "Composed"];
    }
}

export function CustomDispatch$1$reflection(gen0) {
    return union_type("Sutil.DomHelpers.CustomDispatch`1", [gen0], CustomDispatch$1, () => [[["Item", gen0]], [["Item", bool_type]], [["Item", bool_type]]]);
}

export function CustomDispatch$1_toCustomEvent_Z2A5F33D1(props) {
    let data = {};
    const enumerator = getEnumerator(props);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const p = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            switch (p.tag) {
                case 1: {
                    const b = p.fields[0];
                    data["bubbles"] = b;
                    break;
                }
                case 2: {
                    const c = p.fields[0];
                    data["composed"] = c;
                    break;
                }
                default: {
                    const d = p.fields[0];
                    data["detail"] = d;
                }
            }
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    return data;
}

export function CustomDispatch$1_dispatch_4FBB8B24(target, name) {
    dispatchCustom(target, name, CustomDispatch$1_toCustomEvent_Z2A5F33D1(empty()));
}

export function CustomDispatch$1_dispatch_Z31D27F2B(e, name) {
    dispatchCustom(e.target, name, CustomDispatch$1_toCustomEvent_Z2A5F33D1(empty()));
}

export function CustomDispatch$1_dispatch_Z6D73DC75(target, name, props) {
    dispatchCustom(target, name, CustomDispatch$1_toCustomEvent_Z2A5F33D1(props));
}

export function CustomDispatch$1_dispatch_467D575A(e, name, props) {
    dispatchCustom(e.target, name, CustomDispatch$1_toCustomEvent_Z2A5F33D1(props));
}

export function CustomDispatch$1_dispatch_472E5A31(target, name, data) {
    dispatchCustom(target, name, CustomDispatch$1_toCustomEvent_Z2A5F33D1(singleton(new CustomDispatch$1(0, data))));
}

export const domId = makeIdGenerator();

export function isTextNode(n) {
    if (!equals(n, null)) {
        return n.nodeType === 3;
    }
    else {
        return false;
    }
}

export function isElementNode(n) {
    if (!equals(n, null)) {
        return n.nodeType === 1;
    }
    else {
        return false;
    }
}

export function documentOf(n) {
    return n.ownerDocument;
}

export function Browser_Types_Node__Node_get_asTextNode(__) {
    if (isTextNode(__)) {
        return __;
    }
    else {
        return void 0;
    }
}

export function Browser_Types_Node__Node_get_asHtmlElement(__) {
    if (isElementNode(__)) {
        return __;
    }
    else {
        return void 0;
    }
}

export function applyIfElement(f, n) {
    if (isElementNode(n)) {
        f(n);
    }
}

export function applyIfText(f, n) {
    if (isTextNode(n)) {
        f(n);
    }
}

export function getNodeMap(doc) {
    return NodeKey_getCreate(doc.body, NodeKey_NodeMap, () => ({}));
}

export function setSvId(n, id) {
    const map = getNodeMap(n.ownerDocument);
    map[toString(id)] = n;
    n[SvIdKey] = id;
    if (isElementNode(n)) {
        n.setAttribute(SvIdKey, toString(id));
    }
}

export function svId(n) {
    return n[SvIdKey];
}

export function hasSvId(n) {
    return n.hasOwnProperty(SvIdKey);
}

export function findNodeWithSvId(doc, id) {
    const map = getNodeMap(doc);
    const key = toString(id);
    if (map.hasOwnProperty(key)) {
        return map[key];
    }
    else {
        return void 0;
    }
}

export function rectStr(r) {
    return `${r.left},${r.top} -> ${r.right},${r.bottom}`;
}

export function nodeStr(node) {
    if (node == null) {
        return "null";
    }
    else {
        let tc = replace(replace(node.textContent, "\n", "\\n"), "\r", "");
        if (tc.length > 80) {
            tc = substring(tc, 0, 80);
        }
        const matchValue = node.nodeType;
        switch (matchValue) {
            case 1: {
                const e = node;
                return `<${e.tagName.toLocaleLowerCase()}>#${svId(node)} "${tc}"`;
            }
            case 3: {
                return `"${tc}"#${svId(node)}`;
            }
            default: {
                return `?'${tc}'#${svId(node)}`;
            }
        }
    }
}

export function nodeStrShort(node) {
    if (node == null) {
        return "null";
    }
    else {
        let tc = node.textContent;
        if (tc.length > 16) {
            tc = (substring(tc, 0, 16) + "...");
        }
        const matchValue = node.nodeType;
        switch (matchValue) {
            case 1: {
                const e = node;
                return `<${e.tagName.toLocaleLowerCase()}> #${svId(node)}`;
            }
            case 3: {
                return `text:"${tc}" #${svId(node)}`;
            }
            default: {
                return `?'${tc}'#${svId(node)}`;
            }
        }
    }
}

export function children(node) {
    const visit = (child) => delay(() => ((!(child == null)) ? append(singleton_1(child), delay(() => visit(child.nextSibling))) : empty_1()));
    return visit(node.firstChild);
}

export function descendants(node) {
    return delay(() => collect((child) => append(singleton_1(child), delay(() => descendants(child))), children(node)));
}

export function descendantsDepthFirst(node) {
    return delay(() => collect((child) => append(descendants(child), delay(() => singleton_1(child))), children(node)));
}

export function isSameNode(a, b) {
    if (a == null) {
        return b == null;
    }
    else {
        return a.isSameNode(b);
    }
}

function hasDisposables(node) {
    return node.hasOwnProperty(NodeKey_Disposables);
}

export function getDisposables(node) {
    if (hasDisposables(node)) {
        return node[NodeKey_Disposables];
    }
    else {
        return empty();
    }
}

function clearDisposables(node) {
    delete node[NodeKey_Disposables];
}

function cleanup(node) {
    const safeDispose = (d) => {
        try {
            disposeSafe(d);
        }
        catch (x) {
            error(`Disposing ${d}: ${x} from ${nodeStr(node)}`);
        }
    };
    const d_1 = getDisposables(node);
    if (logEnabled()) {
        log(`cleanup ${nodeStr(node)} - ${length(d_1)} disposable(s)`);
    }
    iterate(safeDispose, d_1);
    clearDisposables(node);
    dispatchSimple(node, Event_Unmount);
}

export function assertTrue(condition, message) {
    if (!condition) {
        throw (new Error(message));
    }
}

export function cleanupDeep(node) {
    const array = Array.from(descendantsDepthFirst(node));
    array.forEach((node_1) => {
        cleanup(node_1);
    });
    cleanup(node);
}

export function DomEdit_log(s) {
    if (window.hasOwnProperty("domeditlog")) {
        window.domeditlog(s);
    }
    else {
        log_1("dom", s);
    }
}

export function DomEdit_appendChild(parent, child) {
    DomEdit_log(`appendChild parent='${nodeStrShort(parent)}' child='${nodeStrShort(child)}'`);
    parent.appendChild(child);
    DomEdit_log(`after: appendChild parent='${nodeStrShort(parent)}' child='${nodeStrShort(child)}'`);
}

export function DomEdit_removeChild(parent, child) {
    DomEdit_log(`removeChild parent='${nodeStrShort(parent)}' child='${nodeStrShort(child)}'`);
    cleanupDeep(child);
    parent.removeChild(child);
    DomEdit_log(`after: removeChild parent='${nodeStrShort(parent)}' child='${nodeStrShort(child)}'`);
}

export function DomEdit_insertBefore(parent, child, refNode) {
    DomEdit_log(`insertBefore parent='${nodeStrShort(parent)}' child='${nodeStrShort(child)}' refNode='${nodeStrShort(refNode)}'`);
    parent.insertBefore(child, refNode);
    DomEdit_log(`after: insertBefore parent='${nodeStrShort(parent)}' child='${nodeStrShort(child)}' refNode='${nodeStrShort(refNode)}'`);
}

export function DomEdit_insertAfter(parent, newChild, refChild) {
    const beforeChild = (refChild == null) ? parent.firstChild : refChild.nextSibling;
    DomEdit_insertBefore(parent, newChild, beforeChild);
}

export function unmount(node) {
    cleanupDeep(node);
    if (!(node.parentNode == null)) {
        DomEdit_removeChild(node.parentNode, node);
    }
}

export function clear(node) {
    const array = Array.from(children(node));
    array.forEach((node_1) => {
        unmount(node_1);
    });
}

export function listen(event, e, fn) {
    e.addEventListener(event, fn);
    return () => {
        const value = e.removeEventListener(event, fn);
    };
}

export function raf(f) {
    return Window_requestAnimationFrame_1A119E11((t) => {
        try {
            f(t);
        }
        catch (x) {
            error(`raf: ${x.message}`);
        }
    });
}

export function rafu(f) {
    Window_requestAnimationFrame_1A119E11((_arg) => {
        try {
            f();
        }
        catch (x) {
            error(`rafu: ${x.message}`);
        }
    });
}

export function anyof(events, target, fn) {
    const inner = (e) => {
        iterate((e_1) => {
            target.removeEventListener(e_1, inner);
        }, events);
        fn(e);
    };
    iterate((e_2) => {
        listen(e_2, target, inner);
    }, events);
}

export function once(event, target, fn) {
    const inner = (e) => {
        target.removeEventListener(event, inner);
        fn(e);
    };
    listen(event, target, inner);
}

export function interval(handler, delayMs) {
    const id = setInterval(handler, delayMs) | 0;
    return () => {
        clearInterval(id);
    };
}

export function timeout(handler, delayMs) {
    const id = setTimeout(handler, delayMs) | 0;
    return () => {
        clearTimeout(id);
    };
}

export function nodeIsConnected(node) {
    return node.isConnected;
}

export function ClassHelpers_splitBySpace(s) {
    return split(s, [" "], null, 1);
}

export function ClassHelpers_setClass(className, e) {
    e.className = className;
}

export function ClassHelpers_toggleClass(className, e) {
    e.classList.toggle(className);
}

export function ClassHelpers_addToClasslist(classes, e) {
    e.classList.add(...ClassHelpers_splitBySpace(classes));
}

export function ClassHelpers_removeFromClasslist(classes, e) {
    e.classList.remove(...ClassHelpers_splitBySpace(classes));
}

export function nullToEmpty(s) {
    if (s == null) {
        return "";
    }
    else {
        return s;
    }
}

export function setAttribute(el, name, value) {
    const isBooleanAttribute = (name_1) => {
        if ((((name_1 === "hidden") ? true : (name_1 === "disabled")) ? true : (name_1 === "readonly")) ? true : (name_1 === "required")) {
            return true;
        }
        else {
            return name_1 === "checked";
        }
    };
    const svalue = toString(value);
    if (name === "sutil-toggle-class") {
        ClassHelpers_toggleClass(svalue, el);
    }
    if (name === "class") {
        ClassHelpers_addToClasslist(svalue, el);
    }
    else if (name === "class-") {
        ClassHelpers_removeFromClasslist(svalue, el);
    }
    else if (isBooleanAttribute(name)) {
        const bValue = ((typeof value) === "boolean") ? value : (svalue !== "false");
        if (bValue) {
            el.setAttribute(name, "");
        }
        else {
            el.removeAttribute(name);
        }
    }
    else if (name === "value") {
        el["__value"] = value;
        el["value"] = svalue;
    }
    else if (name === "style+") {
        el.setAttribute("style", nullToEmpty(el.getAttribute("style")) + svalue);
    }
    else {
        el.setAttribute(name, svalue);
    }
}

const idSelector = (() => {
    const clo = toText(printf("#%s"));
    return clo;
})();

const classSelector = (() => {
    const clo = toText(printf(".%s"));
    return clo;
})();

function findElement(doc, selector) {
    return doc.querySelector(selector);
}

function visitChildren(parent, f) {
    let child = parent.firstChild;
    while (!(child == null)) {
        if (f(child)) {
            visitChildren(child, f);
            child = child.nextSibling;
        }
        else {
            child = null;
        }
    }
}

function findNode(parent, f) {
    let x;
    let child = parent.firstChild;
    let result = void 0;
    while (!(child == null)) {
        result = f(child);
        if (result == null) {
            result = findNode(child, f);
        }
        child = ((result != null) ? ((x = value_3(result), null)) : child.nextSibling);
    }
    return result;
}

function prevSibling(node) {
    if (equals(node, null)) {
        return null;
    }
    else {
        return node.previousSibling;
    }
}

function lastSibling(node_mut) {
    lastSibling:
    while (true) {
        const node = node_mut;
        if ((node == null) ? true : (node.nextSibling == null)) {
            return node;
        }
        else {
            node_mut = node.nextSibling;
            continue lastSibling;
        }
        break;
    }
}

function lastChild(node) {
    return lastSibling(node.firstChild);
}

function firstSiblingWhere(node_mut, condition_mut) {
    firstSiblingWhere:
    while (true) {
        const node = node_mut, condition = condition_mut;
        if (node == null) {
            return null;
        }
        else if (condition(node)) {
            return node;
        }
        else {
            node_mut = node.nextSibling;
            condition_mut = condition;
            continue firstSiblingWhere;
        }
        break;
    }
}

function firstChildWhere(node, condition) {
    return firstSiblingWhere(node.firstChild, condition);
}

function lastSiblingWhere(node_mut, condition_mut) {
    lastSiblingWhere:
    while (true) {
        const node = node_mut, condition = condition_mut;
        if (node == null) {
            return null;
        }
        else if (condition(node) && ((node.nextSibling == null) ? true : (!condition(node.nextSibling)))) {
            return node;
        }
        else {
            node_mut = node.nextSibling;
            condition_mut = condition;
            continue lastSiblingWhere;
        }
        break;
    }
}

function lastChildWhere(node, condition) {
    return lastSiblingWhere(node.firstChild, condition);
}

export function visitElementChildren(parent, f) {
    visitChildren(parent, (child) => {
        if (isElementNode(child)) {
            f(child);
        }
        return true;
    });
}

export function addTransform(node, a) {
    let arg_2, arg_1;
    const b = node.getBoundingClientRect();
    if ((a.left !== b.left) ? true : (a.top !== b.top)) {
        const s = Window_getComputedStyle_Z5966C024(node);
        const transform = (s.transform === "none") ? "" : s.transform;
        (node.style).transform = ((arg_2 = (a.top - b.top), (arg_1 = (a.left - b.left), toText(printf("%s translate(%fpx, %fpx)"))(transform)(arg_1)(arg_2))));
        if (logEnabled()) {
            log((node.style).transform);
        }
    }
}

export function fixPosition(node) {
    const s = Window_getComputedStyle_Z5966C024(node);
    if ((s.position !== "absolute") && (s.position !== "fixed")) {
        if (logEnabled()) {
            log(`fixPosition ${nodeStr(node)}`);
        }
        const width = s.width;
        const height = s.height;
        const a = node.getBoundingClientRect();
        (node.style).position = "absolute";
        (node.style).width = width;
        (node.style).height = height;
        addTransform(node, a);
    }
}

export function computedStyleOpacity(e) {
    let arg;
    try {
        return parse(Window_getComputedStyle_Z5966C024(e).opacity);
    }
    catch (matchValue) {
        if (logEnabled()) {
            log((arg = Window_getComputedStyle_Z5966C024(e).opacity, toText(printf("parse error: \u0027%A\u0027"))(arg)));
        }
        return 1;
    }
}

export function computedStyleTransform(node) {
    const style = Window_getComputedStyle_Z5966C024(node);
    if (style.transform === "none") {
        return "";
    }
    else {
        return style.transform;
    }
}

export function wait(el, andThen) {
    const key = NodeKey_Promise;
    const run = () => {
        const value = andThen();
        el[key] = value;
    };
    if (el.hasOwnProperty(key)) {
        const p = el[key];
        delete el[key];
        p.then(run);
    }
    else {
        run();
    }
}

export function textNode(doc, value) {
    const id = domId() | 0;
    if (logEnabled()) {
        log(`create "${value}" #${id}`);
    }
    const n = doc.createTextNode(value);
    setSvId(n, id);
    return n;
}

export function viewportWidth() {
    return Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
}

export function viewportHeight() {
    return Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);
}

export function Browser_Types_NodeListOf$1__NodeListOf$1_toSeq(nodes) {
    return delay(() => map_1((i) => (nodes[i]), toList(rangeDouble(0, 1, nodes.length - 1))));
}

export function Browser_Types_NodeList__NodeList_toSeq(nodes) {
    return delay(() => map_1((i) => (nodes[i]), toList(rangeDouble(0, 1, nodes.length - 1))));
}

export function setHeadStylesheet(doc, url) {
    const head = findElement(doc, "head");
    const styleEl = doc.createElement("link");
    head.appendChild(styleEl);
    styleEl.setAttribute("rel", "stylesheet");
    const value_1 = styleEl.setAttribute("href", url);
}

export function setHeadScript(doc, url) {
    const head = findElement(doc, "head");
    const el = doc.createElement("script");
    head.appendChild(el);
    const value_1 = el.setAttribute("src", url);
}

export function setHeadEmbedScript(doc, source) {
    const head = findElement(doc, "head");
    const el = doc.createElement("script");
    head.appendChild(el);
    el.appendChild(doc.createTextNode(source));
}

export function setHeadTitle(doc, title) {
    const head = findElement(doc, "head");
    const existingTitle = findElement(doc, "head\u003etitle");
    if (!(existingTitle == null)) {
        head.removeChild(existingTitle);
    }
    const titleEl = doc.createElement("title");
    titleEl.appendChild(doc.createTextNode(title));
    head.appendChild(titleEl);
}

