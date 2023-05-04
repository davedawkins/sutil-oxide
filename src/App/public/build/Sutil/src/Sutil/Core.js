import { log as log_2, isEnabled } from "./Logging.js";
import { ClassHelpers_addToClasslist, applyIfElement, descendants, isElementNode, Event_Mount, Event_Connected, CustomDispatch$1_dispatch_4FBB8B24, assertTrue, clear, DomEdit_appendChild, DomEdit_insertAfter, DomEdit_insertBefore, DomEdit_removeChild, nodeStr, cleanupDeep, getDisposables, NodeKey_Disposables, unmount, setSvId, children as children_1, svId, isTextNode, NodeKey_clear, NodeKey_getCreate, NodeKey_Groups, NodeKey_get, nodeIsConnected, domId, nodeStrShort } from "./DomHelpers.js";
import { Record, FSharpRef, Union, toString } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { record_type, tuple_type, bool_type, option_type, lambda_type, string_type, union_type, class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { printf, toText, join } from "../../../fable_modules/fable-library.3.7.20/String.js";
import { tryHead, exists, filter, fold, collect, last as last_1, head, tail, cons, iterateIndexed, length, singleton, append, isEmpty, iterate as iterate_1, empty, map } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { disposeSafe, getEnumerator, equals, int32ToString } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { toList, filter as filter_1, iterate } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { ofNullable, bind, value as value_1, some, toArray } from "../../../fable_modules/fable-library.3.7.20/Option.js";
import { makeIdGenerator, disposable } from "./Helpers.js";
import { getOption } from "./Interop.js";
import { rangeDouble } from "../../../fable_modules/fable-library.3.7.20/Range.js";
import { ElementRef__get_AsElement } from "./Types.js";

function logEnabled() {
    return isEnabled("core");
}

function log(s) {
    log_2("core", s);
}

export class SutilEffect extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["SideEffect", "DomNode", "Group"];
    }
    toString() {
        const this$ = this;
        switch (this$.tag) {
            case 1: {
                const n = this$.fields[0];
                return nodeStrShort(n);
            }
            case 2: {
                const v = this$.fields[0];
                return toString(v);
            }
            default: {
                return "SideEffect";
            }
        }
    }
    Dispose() {
        const __ = this;
        SutilEffect__Dispose(__);
    }
}

export function SutilEffect$reflection() {
    return union_type("Sutil.Core.SutilEffect", [], SutilEffect, () => [[], [["Item", class_type("Browser.Types.Node", void 0, Node)]], [["Item", SutilGroup$reflection()]]]);
}

export class SutilGroup {
    constructor(_name, _parent, _prevInit) {
        this.this = (new FSharpRef(null));
        const this$ = this.this;
        this._name = _name;
        this._parent = _parent;
        this.this.contents = this;
        this.id = int32ToString(domId());
        this._dispose = empty();
        this._children = empty();
        this._prev = _prevInit;
        this._childGroups = empty();
        this["init@300"] = 1;
        SutilEffect__Register_5DF059C1(this._parent, this.this.contents);
    }
    toString() {
        const this$ = this;
        return (((this$._name + "[") + join(",", map(toString, this$._children))) + "]#") + this$.id;
    }
}

export function SutilGroup$reflection() {
    return class_type("Sutil.Core.SutilGroup", void 0, SutilGroup);
}

function SutilGroup_$ctor_5BDBED5B(_name, _parent, _prevInit) {
    return new SutilGroup(_name, _parent, _prevInit);
}

function SutilEffect__mapDefault(this$, f, defaultValue) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return f(n);
        }
        case 2: {
            const n_1 = this$.fields[0];
            return SutilGroup__MapParent_Z6EDD0E6F(n_1, f);
        }
        default: {
            return defaultValue;
        }
    }
}

function SutilEffect__iter_42C48B28(this$, f) {
    SutilEffect__mapDefault(this$, f, void 0);
}

export function SutilEffect__IsConnected(this$) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return nodeIsConnected(n);
        }
        case 2: {
            const g = this$.fields[0];
            return SutilGroup__IsConnected(g);
        }
        default: {
            return false;
        }
    }
}

function SutilEffect_GetGroups_171AE942(node) {
    const groups = NodeKey_get(node, NodeKey_Groups);
    return groups;
}

function SutilEffect_GetCreateGroups_171AE942(node) {
    const groups = NodeKey_getCreate(node, NodeKey_Groups, empty);
    return groups;
}

function SutilEffect_CleanupGroups_171AE942(n) {
    const groups = SutilEffect_GetGroups_171AE942(n);
    iterate((list) => {
        iterate_1((g) => {
            SutilGroup__Dispose(g);
        }, list);
    }, toArray(groups));
    NodeKey_clear(n, NodeKey_Groups);
}

export function SutilEffect__Register_5DF059C1(this$, childGroup) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            const groups = SutilEffect_GetCreateGroups_171AE942(n);
            if (isEmpty(groups)) {
                SutilEffect_RegisterUnsubscribe_Z3FDC8A2C(n, () => {
                    SutilEffect_CleanupGroups_171AE942(n);
                });
            }
            n[NodeKey_Groups] = append(groups, singleton(childGroup));
            break;
        }
        case 2: {
            const g = this$.fields[0];
            SutilGroup__Register_5DF059C1(g, childGroup);
            break;
        }
        default: {
        }
    }
}

export function SutilEffect__PrettyPrint_Z721C83C5(this$, label) {
    const pr = (level, deep, node) => {
        const indent = (l) => (Array((l * 4) + 1).join(" "));
        const log_1 = (l_1, s) => {
            log(indent(l_1) + s);
        };
        const prDomNode = (l_2) => ((dn) => {
            let t;
            const groups = SutilGroup_GroupsOf_171AE942(dn);
            const l$0027 = (l_2 + length(groups)) | 0;
            iterateIndexed((i, g) => {
                log_1(l_2 + i, `<'${SutilGroup__get_Name(g)}'> #${SutilGroup__get_Id(g)}`);
            }, groups);
            if (equals(dn, null)) {
                log_1(l_2, "(null)");
            }
            else if ((t = dn, isTextNode(t))) {
                const t_1 = dn;
                log_1(l_2, `'${t_1.textContent}'`);
            }
            else {
                const e = dn;
                log_1(l$0027, (("\u003c" + e.tagName) + "\u003e #") + toString(svId(e)));
                if (deep) {
                    const source = children_1(e);
                    iterate(prDomNode(l$0027 + 1), source);
                    if (e.hasOwnProperty(NodeKey_Groups)) {
                        const groups_1 = e[NodeKey_Groups];
                        const enumerator = getEnumerator(groups_1);
                        try {
                            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                                const g_1 = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                                prVNode(l$0027 + 1)(g_1);
                            }
                        }
                        finally {
                            disposeSafe(enumerator);
                        }
                    }
                }
            }
        });
        const prVNode = (level_1) => ((v) => {
            const ch = join(",", map((c) => ("#" + SutilEffect__get_Id(c)), SutilGroup__get_Children(v)));
            log_1(level_1, ((((("group \u0027" + SutilGroup__get_Name(v)) + "\u0027 #") + SutilGroup__get_Id(v)) + " children=[") + ch) + "]");
        });
        switch (node.tag) {
            case 1: {
                const n = node.fields[0];
                prDomNode(level)(n);
                break;
            }
            case 2: {
                const v_1 = node.fields[0];
                prVNode(level)(v_1);
                break;
            }
            default: {
                log_1(level, "-");
            }
        }
    };
    console.groupCollapsed(label);
    pr(0, true, this$);
    console.groupEnd();
}

export function SutilEffect__get_Id(this$) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return svId(n);
        }
        case 2: {
            const v = this$.fields[0];
            return SutilGroup__get_Id(v);
        }
        default: {
            return "-";
        }
    }
}

export function SutilEffect__set_Id_Z721C83C5(this$, id) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            setSvId(n, id);
            break;
        }
        case 2: {
            const v = this$.fields[0];
            SutilGroup__set_Id_Z721C83C5(v, id);
            break;
        }
        default: {
        }
    }
}

export function SutilEffect__IsSameNode_2AD740C9(this$, node) {
    const matchValue = [this$, node];
    let pattern_matching_result, a, b, a_1, b_1;
    if (matchValue[0].tag === 1) {
        if (matchValue[1].tag === 1) {
            pattern_matching_result = 1;
            a = matchValue[0].fields[0];
            b = matchValue[1].fields[0];
        }
        else {
            pattern_matching_result = 3;
        }
    }
    else if (matchValue[0].tag === 2) {
        if (matchValue[1].tag === 2) {
            pattern_matching_result = 2;
            a_1 = matchValue[0].fields[0];
            b_1 = matchValue[1].fields[0];
        }
        else {
            pattern_matching_result = 3;
        }
    }
    else if (matchValue[1].tag === 0) {
        pattern_matching_result = 0;
    }
    else {
        pattern_matching_result = 3;
    }
    switch (pattern_matching_result) {
        case 0: {
            return true;
        }
        case 1: {
            return a.isSameNode(b);
        }
        case 2: {
            return SutilGroup__get_Id(a_1) === SutilGroup__get_Id(b_1);
        }
        case 3: {
            return false;
        }
    }
}

export function SutilEffect__get_Document(this$) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return n.ownerDocument;
        }
        case 2: {
            const v = this$.fields[0];
            return SutilGroup__get_Document(v);
        }
        default: {
            return window.document;
        }
    }
}

export function SutilEffect__get_IsEmpty(this$) {
    return equals(this$, new SutilEffect(0));
}

export function SutilEffect__get_PrevNode(this$) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return new SutilEffect(1, n.previousSibling);
        }
        case 2: {
            const v = this$.fields[0];
            return SutilGroup__get_PrevNode(v);
        }
        default: {
            return new SutilEffect(0);
        }
    }
}

function SutilEffect__get_NextDomNode(this$) {
    switch (this$.tag) {
        case 1: {
            const node = this$.fields[0];
            if (node == null) {
                return null;
            }
            else {
                return node.nextSibling;
            }
        }
        case 2: {
            const g = this$.fields[0];
            return SutilGroup__get_NextDomNode(g);
        }
        default: {
            return null;
        }
    }
}

export function SutilEffect__collectDomNodes(this$) {
    return SutilEffect__DomNodes(this$);
}

export function SutilEffect__DomNodes(this$) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return singleton(n);
        }
        case 2: {
            const v = this$.fields[0];
            return SutilGroup__DomNodes(v);
        }
        default: {
            return empty();
        }
    }
}

export function SutilEffect__get_AsDomNode(this$) {
    return SutilEffect__mapDefault(this$, (x) => x, null);
}

export function SutilEffect__Dispose(node) {
    switch (node.tag) {
        case 2: {
            const v = node.fields[0];
            SutilGroup__Dispose(v);
            break;
        }
        case 1: {
            const n = node.fields[0];
            unmount(n);
            break;
        }
        default: {
        }
    }
}

export function SutilEffect_RegisterDisposable_5FAE877D(node, d) {
    node[NodeKey_Disposables] = cons(d, getDisposables(node));
}

export function SutilEffect_RegisterDisposable_2069CF16(node, d) {
    if (logEnabled()) {
        log(`register disposable on ${node}`);
    }
    switch (node.tag) {
        case 1: {
            const n = node.fields[0];
            SutilEffect_RegisterDisposable_5FAE877D(n, d);
            break;
        }
        case 2: {
            const v = node.fields[0];
            SutilGroup__RegisterUnsubscribe_3A5B6456(v, () => {
                disposeSafe(d);
            });
            break;
        }
        default: {
        }
    }
}

export function SutilEffect_RegisterUnsubscribe_Z3FDC8A2C(node, d) {
    SutilEffect_RegisterDisposable_5FAE877D(node, disposable(d));
}

export function SutilEffect_RegisterUnsubscribe_Z401BC241(node, d) {
    SutilEffect_RegisterDisposable_2069CF16(node, disposable(d));
}

function SutilEffect_ReplaceGroup_4E7C9F42(parent, nodes, existing) {
    let x;
    if (logEnabled()) {
        log(`ReplaceGroup: nodes ${length(nodes)} existing ${length(existing)}`);
    }
    const insertBefore = (!isEmpty(existing)) ? (isEmpty(tail(existing)) ? ((x = head(existing), x.nextSibling)) : last_1(existing).nextSibling) : null;
    const remove = (n) => {
        let copyOfStruct, copyOfStruct_1, copyOfStruct_2;
        cleanupDeep(n);
        if (((copyOfStruct = n, copyOfStruct.parentNode)) == null) {
            if (logEnabled()) {
                log(`Warning: Node ${nodeStr(n)} was unmounted unexpectedly`);
            }
        }
        else {
            if (!parent.isSameNode((copyOfStruct_1 = n, copyOfStruct_1.parentNode))) {
                if (logEnabled()) {
                    log(`Warning: Node ${nodeStr(n)} has unexpected parent`);
                }
            }
            DomEdit_removeChild((copyOfStruct_2 = n, copyOfStruct_2.parentNode), n);
        }
    };
    const insert = (n_1) => {
        DomEdit_insertBefore(parent, n_1, insertBefore);
    };
    iterate_1(remove, existing);
    iterate_1(insert, nodes);
}

export function SutilEffect__InsertBefore_Z129D0740(this$, node, refNode) {
    SutilEffect__iter_42C48B28(this$, (parent) => {
        DomEdit_insertBefore(parent, node, refNode);
    });
}

export function SutilEffect__InsertAfter_Z5097E6E0(this$, node, refNode) {
    switch (this$.tag) {
        case 1: {
            const parent = this$.fields[0];
            if (logEnabled()) {
                log(`InsertAfter (parent = ${this$}: refNode=${refNode} refNode.NextDomNode=${nodeStr(SutilEffect__get_NextDomNode(refNode))}`);
            }
            const refDomNode = SutilEffect__get_NextDomNode(refNode);
            iterate_1((child) => {
                DomEdit_insertBefore(parent, child, refDomNode);
            }, SutilEffect__collectDomNodes(node));
            break;
        }
        case 2: {
            const g = this$.fields[0];
            SutilGroup__InsertAfter_Z5097E6E0(g, node, refNode);
            break;
        }
        default: {
            console.warn(some("InsertAfter called for empty node - disposing child"));
            SutilEffect__Dispose(node);
        }
    }
}

export function SutilEffect__InsertAfter_Z129D0740(this$, node, refNode) {
    SutilEffect__iter_42C48B28(this$, (parent) => {
        DomEdit_insertAfter(parent, node, refNode);
    });
}

export function SutilEffect__ReplaceGroup_Z748E2B9E(this$, node, existing, insertBefore) {
    if (logEnabled()) {
        log(`ReplaceGroup(${node}, ${existing})`);
    }
    switch (this$.tag) {
        case 1: {
            const parent = this$.fields[0];
            SutilEffect_ReplaceGroup_4E7C9F42(parent, SutilEffect__collectDomNodes(node), SutilEffect__collectDomNodes(existing));
            break;
        }
        case 2: {
            const parent_1 = this$.fields[0];
            SutilGroup__ReplaceChild_Z748E2B9E(parent_1, node, existing, insertBefore);
            break;
        }
        default: {
        }
    }
}

export function SutilEffect__AppendChild_171AE942(this$, child) {
    switch (this$.tag) {
        case 1: {
            const parent = this$.fields[0];
            DomEdit_appendChild(parent, child);
            break;
        }
        case 2: {
            const parent_1 = this$.fields[0];
            SutilGroup__AppendChild_2AD740C9(parent_1, new SutilEffect(1, child));
            break;
        }
        default: {
        }
    }
}

export function SutilEffect__get_FirstDomNodeInOrAfter(this$) {
    switch (this$.tag) {
        case 1: {
            const n = this$.fields[0];
            return n;
        }
        case 2: {
            const g = this$.fields[0];
            return SutilGroup__get_FirstDomNodeInOrAfter(g);
        }
        default: {
            return null;
        }
    }
}

export function SutilEffect__Clear(this$) {
    SutilEffect__iter_42C48B28(this$, (node) => {
        clear(node);
    });
}

export function SutilEffect_MakeGroup_5BDBED5B(name, parent, prevInit) {
    return SutilGroup_Create_5BDBED5B(name, parent, prevInit);
}

export function SutilGroup__get_Document(this$) {
    return SutilGroup__parentDomNode(this$).ownerDocument;
}

export function SutilGroup_Create_5BDBED5B(name, parent, prevInit) {
    return SutilGroup_$ctor_5BDBED5B(name, parent, prevInit);
}

export function SutilGroup__IsConnected(this$) {
    return nodeIsConnected(SutilGroup__parentDomNode(this$));
}

export function SutilGroup__AssertIsConnected(this$) {
    if (!SutilGroup__IsConnected(this$)) {
        throw (new Error(`Not connected: ${this$}`));
    }
}

export function SutilGroup__get_Parent(this$) {
    return this$._parent;
}

export function SutilGroup__Register_5DF059C1(this$, childGroup) {
    this$._childGroups = cons(childGroup, this$._childGroups);
}

export function SutilGroup__get_PrevNode(this$) {
    return this$._prev;
}

export function SutilGroup__set_PrevNode_2AD740C9(this$, v) {
    this$._prev = v;
}

export function SutilGroup__DomNodes(this$) {
    return collect(SutilEffect__DomNodes, SutilGroup__get_Children(this$));
}

export function SutilGroup__get_PrevDomNode(this$) {
    let result;
    const matchValue = SutilGroup__get_PrevNode(this$);
    switch (matchValue.tag) {
        case 2: {
            const v = matchValue.fields[0];
            const matchValue_1 = SutilGroup__get_LastDomNode(v);
            if (equals(matchValue_1, null)) {
                result = SutilGroup__get_PrevDomNode(v);
            }
            else {
                const n_1 = matchValue_1;
                result = n_1;
            }
            break;
        }
        case 0: {
            const matchValue_2 = SutilGroup__get_Parent(this$);
            if (matchValue_2.tag === 2) {
                const pv = matchValue_2.fields[0];
                result = SutilGroup__get_PrevDomNode(pv);
            }
            else {
                result = null;
            }
            break;
        }
        default: {
            const n = matchValue.fields[0];
            result = n;
        }
    }
    if (logEnabled()) {
        log(`PrevDomNode of ${this$} -> '${nodeStr(result)}' PrevNode=${SutilGroup__get_PrevNode(this$)}`);
    }
    return result;
}

export function SutilGroup__get_NextDomNode(this$) {
    let result;
    const matchValue = SutilGroup__DomNodes(this$);
    if (isEmpty(matchValue)) {
        const matchValue_1 = SutilGroup__get_PrevDomNode(this$);
        if (equals(matchValue_1, null)) {
            const matchValue_2 = SutilGroup__parentDomNode(this$);
            if (equals(matchValue_2, null)) {
                result = null;
            }
            else {
                const p = matchValue_2;
                result = p.firstChild;
            }
        }
        else {
            const prev = matchValue_1;
            result = prev.nextSibling;
        }
    }
    else {
        const ns = matchValue;
        const matchValue_3 = last_1(ns);
        if (equals(matchValue_3, null)) {
            result = null;
        }
        else {
            const last = matchValue_3;
            result = last.nextSibling;
        }
    }
    return result;
}

export function SutilGroup__get_FirstDomNode(this$) {
    const matchValue = SutilGroup__DomNodes(this$);
    if (!isEmpty(matchValue)) {
        const ns = tail(matchValue);
        const n = head(matchValue);
        return n;
    }
    else {
        return null;
    }
}

export function SutilGroup__get_LastDomNode(this$) {
    const matchValue = SutilGroup__DomNodes(this$);
    if (isEmpty(matchValue)) {
        return null;
    }
    else {
        const ns = matchValue;
        return last_1(ns);
    }
}

export function SutilGroup__get_FirstDomNodeInOrAfter(this$) {
    const matchValue = SutilGroup__get_FirstDomNode(this$);
    if (equals(matchValue, null)) {
        return SutilGroup__get_NextDomNode(this$);
    }
    else {
        const first = matchValue;
        return first;
    }
}

export function SutilGroup__MapParent_Z6EDD0E6F(this$, f) {
    return f(SutilGroup__parentDomNode(this$));
}

function SutilGroup__OwnX_171AE942(this$, n) {
    n["__sutil_snode"] = this$;
}

function SutilGroup__OwnX_2AD740C9(this$, child) {
    if (child.tag === 1) {
        const n = child.fields[0];
        SutilGroup__OwnX_171AE942(this$, n);
    }
}

export function SutilGroup_GroupOf_171AE942(n) {
    return getOption(n, "__sutil_snode");
}

export function SutilGroup_GroupsOf_171AE942(n) {
    const parentsOf = (r_mut) => {
        parentsOf:
        while (true) {
            const r = r_mut;
            if (!isEmpty(r)) {
                const xs = tail(r);
                const x = head(r);
                const matchValue = SutilGroup__get_Parent(x);
                if (matchValue.tag === 2) {
                    const g = matchValue.fields[0];
                    r_mut = cons(g, r);
                    continue parentsOf;
                }
                else {
                    return r;
                }
            }
            else {
                return r;
            }
            break;
        }
    };
    const init = (n_1) => {
        const matchValue_1 = getOption(n_1, "__sutil_snode");
        if (matchValue_1 != null) {
            const g_1 = value_1(matchValue_1);
            return singleton(g_1);
        }
        else {
            return empty();
        }
    };
    return parentsOf(init(n));
}

export function SutilGroup__Clear(this$) {
    this$._children = empty();
}

export function SutilGroup__AddChild_2AD740C9(this$, child) {
    SutilGroup__OwnX_2AD740C9(this$, child);
    this$._children = append(this$._children, singleton(child));
    SutilGroup__updateChildrenPrev(this$);
}

export function SutilGroup__AppendChild_2AD740C9(this$, child) {
    if (SutilGroup__get_Parent(this$).tag === 0) {
    }
    else {
        const cn = map(nodeStrShort, SutilGroup__DomNodes(this$));
        const pn = map(nodeStrShort, SutilEffect__DomNodes(SutilGroup__get_PrevNode(this$)));
        const parent = SutilGroup__parentDomNode(this$);
        const before = SutilGroup__get_NextDomNode(this$);
        iterate_1((ch) => {
            DomEdit_insertBefore(parent, ch, before);
        }, SutilEffect__collectDomNodes(child));
    }
    SutilGroup__OwnX_2AD740C9(this$, child);
    this$._children = append(this$._children, singleton(child));
    SutilGroup__updateChildrenPrev(this$);
}

function SutilGroup__get_FirstChild(this$) {
    const matchValue = this$._children;
    if (!isEmpty(matchValue)) {
        const xs = tail(matchValue);
        const x = head(matchValue);
        return x;
    }
    else {
        return new SutilEffect(0);
    }
}

function SutilGroup__ChildAfter_2AD740C9(this$, prev) {
    if (prev.tag === 0) {
        return SutilGroup__get_FirstChild(this$);
    }
    else {
        const find = (list_mut) => {
            let y, x, x_1;
            find:
            while (true) {
                const list = list_mut;
                let pattern_matching_result, x_3, y_1, x_4, xs;
                if (!isEmpty(list)) {
                    if (!isEmpty(tail(list))) {
                        if ((y = head(tail(list)), (x = head(list), SutilEffect__IsSameNode_2AD740C9(x, prev)))) {
                            pattern_matching_result = 2;
                            x_3 = head(list);
                            y_1 = head(tail(list));
                        }
                        else {
                            pattern_matching_result = 3;
                            x_4 = head(list);
                            xs = tail(list);
                        }
                    }
                    else if ((x_1 = head(list), SutilEffect__IsSameNode_2AD740C9(x_1, prev))) {
                        pattern_matching_result = 1;
                    }
                    else {
                        pattern_matching_result = 3;
                        x_4 = head(list);
                        xs = tail(list);
                    }
                }
                else {
                    pattern_matching_result = 0;
                }
                switch (pattern_matching_result) {
                    case 0: {
                        return new SutilEffect(0);
                    }
                    case 1: {
                        return new SutilEffect(0);
                    }
                    case 2: {
                        return y_1;
                    }
                    case 3: {
                        list_mut = xs;
                        continue find;
                    }
                }
                break;
            }
        };
        return find(this$._children);
    }
}

export function SutilGroup__InsertAfter_Z5097E6E0(this$, child, prev) {
    SutilGroup__InsertBefore_Z5097E6E0(this$, child, SutilGroup__ChildAfter_2AD740C9(this$, prev));
}

function SutilGroup__InsertBefore_Z5097E6E0(this$, child, refNode) {
    const refDomNode = (refNode.tag === 0) ? SutilGroup__get_NextDomNode(this$) : SutilEffect__get_FirstDomNodeInOrAfter(refNode);
    if (logEnabled()) {
        log(`InsertBefore: child='${child}' before '${refNode}' refDomNode='${nodeStrShort(refDomNode)}' child.PrevNode='${SutilEffect__get_PrevNode(child)}'`);
    }
    const parent = SutilGroup__parentDomNode(this$);
    const len = length(this$._children) | 0;
    const enumerator = getEnumerator(SutilEffect__collectDomNodes(child));
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const dnode = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            DomEdit_insertBefore(parent, dnode, refDomNode);
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    if (equals(refNode, new SutilEffect(0))) {
        SutilGroup__AddChild_2AD740C9(this$, child);
    }
    else {
        this$._children = fold((list, ch) => {
            let n;
            if ((n = ch, SutilEffect__IsSameNode_2AD740C9(n, refNode))) {
                const n_1 = ch;
                return append(list, append(singleton(child), singleton(n_1)));
            }
            else {
                return append(list, singleton(ch));
            }
        }, empty(), this$._children);
        SutilGroup__OwnX_2AD740C9(this$, child);
    }
    SutilGroup__updateChildrenPrev(this$);
    if (length(this$._children) === len) {
        if (logEnabled()) {
            log("Error: Child was not added");
        }
    }
}

export function SutilGroup__RemoveChild_2AD740C9(_, child) {
    const rc = (p, c) => {
        switch (c.tag) {
            case 1: {
                const n = c.fields[0];
                unmount(n);
                break;
            }
            case 2: {
                const g = c.fields[0];
                iterate_1((gc) => {
                    SutilGroup__RemoveChild_2AD740C9(g, gc);
                }, SutilGroup__get_Children(g));
                SutilGroup__Dispose(g);
                break;
            }
            default: {
            }
        }
    };
    const newChildren = filter((n_1) => (!equals(n_1, child)), _._children);
    rc(_.this.contents, child);
    _._children = newChildren;
    SutilGroup__updateChildrenPrev(_);
}

export function SutilGroup__ReplaceChild_Z748E2B9E(this$, child, oldChild, insertBefore) {
    const deleteOldNodes = () => {
        const oldNodes = SutilEffect__collectDomNodes(oldChild);
        iterate_1((c) => {
            if (c.parentNode == null) {
                if (logEnabled()) {
                    log(`Node has no parent: ${nodeStrShort(c)}`);
                }
            }
            else {
                DomEdit_removeChild(c.parentNode, c);
            }
        }, oldNodes);
    };
    const nodes = SutilEffect__collectDomNodes(child);
    assertTrue(!equals(child, new SutilEffect(0)), "Empty child for replace child");
    if (!equals(oldChild, new SutilEffect(0))) {
        assertTrue(exists((c_1) => (SutilEffect__get_Id(c_1) === SutilEffect__get_Id(oldChild)), this$._children), "Child not found");
        SutilEffect__set_Id_Z721C83C5(child, SutilEffect__get_Id(oldChild));
    }
    const parent = SutilGroup__parentDomNode(this$);
    iterate_1((n) => {
        DomEdit_insertBefore(parent, n, insertBefore);
    }, nodes);
    deleteOldNodes();
    if (oldChild.tag === 2) {
        const g = oldChild.fields[0];
        SutilGroup__Dispose(g);
    }
    if ((insertBefore == null) ? true : equals(oldChild, new SutilEffect(0))) {
        SutilGroup__AddChild_2AD740C9(this$, child);
    }
    else {
        SutilGroup__OwnX_2AD740C9(this$, child);
        this$._children = map((n_1) => {
            if (SutilEffect__get_Id(n_1) === SutilEffect__get_Id(oldChild)) {
                return child;
            }
            else {
                return n_1;
            }
        }, this$._children);
    }
    SutilGroup__updateChildrenPrev(this$);
}

export function SutilGroup__get_Name(_) {
    return _._name;
}

export function SutilGroup__get_Id(_) {
    return _.id;
}

export function SutilGroup__set_Id_Z721C83C5(_, id$0027) {
    _.id = id$0027;
}

export function SutilGroup__get_Children(_) {
    return _._children;
}

export function SutilGroup__RegisterUnsubscribe_3A5B6456(_, d) {
    _._dispose = append(_._dispose, singleton(d));
}

export function SutilGroup__Dispose(_) {
    iterate_1((c) => {
        SutilGroup__Dispose(c);
    }, _._childGroups);
    iterate_1((d) => {
        d();
    }, _._dispose);
    _._dispose = empty();
}

function SutilGroup__updateChildrenPrev(this$) {
    let p = new SutilEffect(0);
    const enumerator = getEnumerator(this$._children);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const c = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            if (c.tag === 2) {
                const v = c.fields[0];
                SutilGroup__set_PrevNode_2AD740C9(v, p);
            }
            p = c;
        }
    }
    finally {
        disposeSafe(enumerator);
    }
}

function SutilGroup__parentDomNode(this$) {
    const findParent = (p_mut) => {
        findParent:
        while (true) {
            const p = p_mut;
            switch (p.tag) {
                case 1: {
                    const n = p.fields[0];
                    return n;
                }
                case 2: {
                    const v = p.fields[0];
                    p_mut = SutilGroup__get_Parent(v);
                    continue findParent;
                }
                default: {
                    return null;
                }
            }
            break;
        }
    };
    return findParent(this$._parent);
}

function notifySutilEvents(parent, node) {
    if (SutilEffect__IsConnected(parent)) {
        iterate_1((n) => {
            CustomDispatch$1_dispatch_4FBB8B24(n, Event_Connected);
            CustomDispatch$1_dispatch_4FBB8B24(n, Event_Mount);
            iterate((n_2) => {
                CustomDispatch$1_dispatch_4FBB8B24(n_2, Event_Mount);
            }, filter_1(isElementNode, descendants(n)));
        }, SutilEffect__collectDomNodes(node));
    }
}

export class DomAction extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Append", "Replace", "Nothing"];
    }
}

export function DomAction$reflection() {
    return union_type("Sutil.Core.DomAction", [], DomAction, () => [[], [["Item1", SutilEffect$reflection()], ["Item2", class_type("Browser.Types.Node", void 0, Node)]], []]);
}

export class BuildContext extends Record {
    constructor(Document$, Parent, Previous, Action, MakeName, Class, Debug, Pipeline) {
        super();
        this.Document = Document$;
        this.Parent = Parent;
        this.Previous = Previous;
        this.Action = Action;
        this.MakeName = MakeName;
        this.Class = Class;
        this.Debug = Debug;
        this.Pipeline = Pipeline;
    }
}

export function BuildContext$reflection() {
    return record_type("Sutil.Core.BuildContext", [], BuildContext, () => [["Document", class_type("Browser.Types.Document", void 0, Document)], ["Parent", SutilEffect$reflection()], ["Previous", SutilEffect$reflection()], ["Action", DomAction$reflection()], ["MakeName", lambda_type(string_type, string_type)], ["Class", option_type(string_type)], ["Debug", bool_type], ["Pipeline", lambda_type(tuple_type(BuildContext$reflection(), SutilEffect$reflection()), tuple_type(BuildContext$reflection(), SutilEffect$reflection()))]]);
}

export function BuildContext__get_ParentElement(this$) {
    return SutilEffect__get_AsDomNode(this$.Parent);
}

export function BuildContext__get_ParentNode(this$) {
    return SutilEffect__get_AsDomNode(this$.Parent);
}

export function BuildContext__AddChild_2AD740C9(ctx, node) {
    const matchValue = ctx.Action;
    switch (matchValue.tag) {
        case 0: {
            if (logEnabled()) {
                log(`ctx.Append '${node}' to '${ctx.Parent}' after ${ctx.Previous}`);
            }
            SutilEffect__InsertAfter_Z5097E6E0(ctx.Parent, node, ctx.Previous);
            notifySutilEvents(ctx.Parent, node);
            break;
        }
        case 1: {
            const insertBefore = matchValue.fields[1];
            const existing = matchValue.fields[0];
            if (logEnabled()) {
                log(`ctx.Replace '${existing}' with '${node}' before '${nodeStrShort(insertBefore)}'`);
            }
            SutilEffect__ReplaceGroup_Z748E2B9E(ctx.Parent, node, existing, insertBefore);
            notifySutilEvents(ctx.Parent, node);
            break;
        }
        default: {
        }
    }
}

export function domResult(node) {
    return new SutilEffect(1, node);
}

export function sutilResult(node) {
    return node;
}

export function sideEffect(ctx, name) {
    const text = () => {
        const tn = ctx.Document.createTextNode(name);
        const d = ctx.Document.createElement("div");
        DomEdit_appendChild(d, tn);
        BuildContext__AddChild_2AD740C9(ctx, new SutilEffect(1, d));
        return d;
    };
    if (ctx.Debug) {
        return new SutilEffect(1, text());
    }
    else {
        return new SutilEffect(0);
    }
}

export class SutilElement {
    constructor(name, children, builder) {
        this.builder = builder;
    }
}

export function SutilElement$reflection() {
    return class_type("Sutil.Core.SutilElement", void 0, SutilElement);
}

function SutilElement_$ctor_71ED6ECB(name, children, builder) {
    return new SutilElement(name, children, builder);
}

export function SutilElement_Define_314C839F(builder) {
    return SutilElement_$ctor_71ED6ECB("", [], builder);
}

export function SutilElement_Define_7B1F8004(name, builder) {
    return SutilElement_$ctor_71ED6ECB(name, [], builder);
}

export function SutilElement_Define_Z60F5000F(name, builder) {
    return SutilElement_$ctor_71ED6ECB(name, [], (ctx) => {
        builder(ctx);
        return sideEffect(ctx, name);
    });
}

export function SutilElement_Define_1C1F44C0(name, children, builder) {
    return SutilElement_$ctor_71ED6ECB(name, children, (ctx) => (new SutilEffect(1, builder(ctx))));
}

export function SutilElement__get_Builder(__) {
    return __.builder;
}

function defaultContext(parent) {
    const gen = makeIdGenerator();
    return new BuildContext(parent.ownerDocument, new SutilEffect(1, parent), new SutilEffect(0), new DomAction(0), (baseName) => {
        const arg_1 = gen() | 0;
        return toText(printf("%s-%d"))(baseName)(arg_1);
    }, void 0, false, (x) => x);
}

function makeContext(parent) {
    const getSutilClasses = (e) => {
        const classes = filter((cls) => (cls.indexOf("sutil") === 0), map((i) => (e.classList[i]), toList(rangeDouble(0, 1, e.classList.length - 1))));
        return classes;
    };
    const inputRecord = defaultContext(parent);
    return new BuildContext(inputRecord.Document, inputRecord.Parent, inputRecord.Previous, inputRecord.Action, inputRecord.MakeName, bind((e_1) => tryHead(getSutilClasses(e_1)), ofNullable(parent)), inputRecord.Debug, inputRecord.Pipeline);
}

function makeShadowContext(customElement) {
    const inputRecord = defaultContext(customElement);
    return new BuildContext(inputRecord.Document, inputRecord.Parent, inputRecord.Previous, new DomAction(2), inputRecord.MakeName, inputRecord.Class, inputRecord.Debug, inputRecord.Pipeline);
}

export function ContextHelpers_withStyleSheet(sheet, ctx) {
    return ctx;
}

export function ContextHelpers_withDebug(ctx) {
    return new BuildContext(ctx.Document, ctx.Parent, ctx.Previous, ctx.Action, ctx.MakeName, ctx.Class, true, ctx.Pipeline);
}

export function ContextHelpers_withPreProcess(f, ctx) {
    return new BuildContext(ctx.Document, ctx.Parent, ctx.Previous, ctx.Action, ctx.MakeName, ctx.Class, ctx.Debug, (arg) => ctx.Pipeline(f(arg)));
}

export function ContextHelpers_withPostProcess(f, ctx) {
    return new BuildContext(ctx.Document, ctx.Parent, ctx.Previous, ctx.Action, ctx.MakeName, ctx.Class, ctx.Debug, (arg) => f(ctx.Pipeline(arg)));
}

export function ContextHelpers_withParent(parent, ctx) {
    return new BuildContext(ctx.Document, parent, ctx.Previous, new DomAction(0), ctx.MakeName, ctx.Class, ctx.Debug, ctx.Pipeline);
}

export function ContextHelpers_withPrevious(prev, ctx) {
    return new BuildContext(ctx.Document, ctx.Parent, prev, ctx.Action, ctx.MakeName, ctx.Class, ctx.Debug, ctx.Pipeline);
}

export function ContextHelpers_withParentNode(parent, ctx) {
    return ContextHelpers_withParent(new SutilEffect(1, parent), ctx);
}

export function ContextHelpers_withReplace(toReplace, before, ctx) {
    return new BuildContext(ctx.Document, ctx.Parent, ctx.Previous, new DomAction(1, toReplace, before), ctx.MakeName, ctx.Class, ctx.Debug, ctx.Pipeline);
}

export function errorNode(parent, message) {
    const doc = SutilEffect__get_Document(parent);
    const d = doc.createElement("div");
    DomEdit_appendChild(d, doc.createTextNode(`sutil-error: ${message}`));
    SutilEffect__AppendChild_171AE942(parent, d);
    d.setAttribute("style", "color: red; padding: 4px; font-size: 10px;");
    return d;
}

export function build(f, ctx) {
    return ctx.Pipeline([ctx, SutilElement__get_Builder(f)(ctx)])[1];
}

export function buildOnly(f, ctx) {
    return SutilElement__get_Builder(f)(ctx);
}

function pipelineDispatchMount(ctx, result) {
    if (result.tag === 1) {
        const n = result.fields[0];
        CustomDispatch$1_dispatch_4FBB8B24(n, Event_Mount);
    }
    return [ctx, result];
}

function pipelineAddClass(ctx, result) {
    const matchValue = [ctx.Class, result];
    let pattern_matching_result, cls;
    if (matchValue[0] != null) {
        if (matchValue[1].tag === 1) {
            pattern_matching_result = 0;
            cls = matchValue[0];
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
            applyIfElement((e) => {
                ClassHelpers_addToClasslist(cls, e);
            }, SutilEffect__get_AsDomNode(result));
            break;
        }
    }
    return [ctx, result];
}

export function buildChildren(xs, ctx) {
    const e = ctx.Parent;
    let prev = new SutilEffect(0);
    const enumerator = getEnumerator(xs);
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const x = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
            const matchValue = build(x, ContextHelpers_withPrevious(prev, ctx));
            if (matchValue.tag === 0) {
            }
            else {
                const r = matchValue;
                prev = r;
            }
        }
    }
    finally {
        disposeSafe(enumerator);
    }
}

export function mountOnShadowRoot(app, host) {
    const el = build(app, makeShadowContext(host));
    switch (el.tag) {
        case 2: {
            const group = el.fields[0];
            const shadowRoot_1 = host.shadowRoot;
            const enumerator = getEnumerator(SutilGroup__DomNodes(group));
            try {
                while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                    const node_1 = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                    shadowRoot_1.appendChild(node_1);
                }
            }
            finally {
                disposeSafe(enumerator);
            }
            break;
        }
        case 0: {
            throw (new Error("Custom components must return at least one node"));
            break;
        }
        default: {
            const node = el.fields[0];
            const shadowRoot = host.shadowRoot;
            shadowRoot.appendChild(node);
        }
    }
    const dispose = () => {
        SutilEffect__Dispose(el);
    };
    return dispose;
}

export function mount(app, _arg1_, _arg1__1) {
    let inputRecord;
    const _arg = [_arg1_, _arg1__1];
    const op = _arg[0];
    const eref = _arg[1];
    const node = ElementRef__get_AsElement(eref);
    if (op.tag === 1) {
        return build(app, (inputRecord = makeContext(node.parentElement), new BuildContext(inputRecord.Document, inputRecord.Parent, new SutilEffect(1, node), inputRecord.Action, inputRecord.MakeName, inputRecord.Class, inputRecord.Debug, inputRecord.Pipeline)));
    }
    else {
        return build(app, makeContext(node));
    }
}

