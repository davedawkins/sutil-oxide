import { parse } from "../fable_modules/fable-library.3.7.20/Double.js";
import { some } from "../fable_modules/fable-library.3.7.20/Option.js";
import { EventEngine$1__onMouseDown_58BC8925 } from "../fable_modules/Feliz.Engine.Event.1.0.0-beta-004/EventEngine.fs.js";
import { MenuMonitor_monitorAll } from "./Toolbar.js";
import { EngineHelpers_Ev } from "../Sutil/src/Sutil/Html.js";
import { uncurry } from "../fable_modules/fable-library.3.7.20/Util.js";
import { iterate, map } from "../fable_modules/fable-library.3.7.20/List.js";
import { toList } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { rangeDouble } from "../fable_modules/fable-library.3.7.20/Range.js";
import { TabHalf } from "./Types.js";

export function toEl(et) {
    return et;
}

export function targetEl(e) {
    return toEl(e.target);
}

export function currentEl(e) {
    return toEl(e.currentTarget);
}

export function getPaneFlexGrow(el) {
    const cs = window.getComputedStyle(el);
    try {
        return (~(~parse(cs.flexGrow))) | 0;
    }
    catch (matchValue) {
        return 0;
    }
}

export function setPaneFlexGrow(el, w) {
    (el.style).flexGrow = (`${w}`);
}

export function getPaneWidth(el) {
    return ~(~parse((window.getComputedStyle(el)).width.slice(void 0, -3 + 1)));
}

export function setPaneWidth(el, w) {
    (el.style).width = (`${w}px`);
}

export function getPaneHeight(el) {
    console.log(some((window.getComputedStyle(el)).height));
    return (~(~parse((window.getComputedStyle(el)).height.slice(void 0, -3 + 1)))) | 0;
}

export function setPaneHeight(el, h) {
    (el.style).height = (`${h}px`);
}

export function setPaneSizeUsingFlexGrow(getSize, el, size) {
    const parentSz = getSize(el.parentElement) | 0;
    const pct = size / parentSz;
    setPaneFlexGrow(el, ~(~(pct * 10000)));
    setPaneFlexGrow(toEl(el.previousElementSibling), ~(~((1 - pct) * 10000)));
}

export const setPaneWidthUsingFlexGrow = (el_1) => ((size) => {
    setPaneSizeUsingFlexGrow(getPaneWidth, el_1, size);
});

export const setPaneHeightUsingFlexGrow = (el_1) => ((size) => {
    setPaneSizeUsingFlexGrow(getPaneHeight, el_1, size);
});

export function getContentParentNode(location) {
    const contentId = (location.tag === 1) ? "#dock-left-bottom" : ((location.tag === 5) ? "#dock-right-top" : ((location.tag === 6) ? "#dock-right-bottom" : ((location.tag === 2) ? "#dock-bottom-left" : ((location.tag === 4) ? "#dock-bottom-right" : ((location.tag === 7) ? "#dock-top-left" : ((location.tag === 8) ? "#dock-top-right" : ((location.tag === 3) ? "#dock-centre-centre" : "#dock-left-top")))))));
    return document.querySelector(contentId);
}

export function getWrapperNode(name) {
    return document.querySelector("#pane-" + name.toLocaleLowerCase());
}

export function resizeController(pos, getSize, setSize, commit, direction) {
    return EventEngine$1__onMouseDown_58BC8925(EngineHelpers_Ev, (e) => {
        e.preventDefault();
        const pane = targetEl(e).parentElement;
        const posOffset = pos(e);
        const startSize = getSize(pane);
        const mouseDragHandler = (e_1) => {
            e_1.preventDefault();
            const primaryButtonPressed = e_1.buttons === 1;
            if (!primaryButtonPressed) {
                const arg_2 = (~(~(((posOffset - pos(e_1)) * direction) + startSize))) | 0;
                commit(pane, arg_2);
                document.body.removeEventListener("pointermove", mouseDragHandler);
                MenuMonitor_monitorAll();
            }
            else {
                const arg_4 = (~(~(((posOffset - pos(e_1)) * direction) + startSize))) | 0;
                setSize(pane, arg_4);
            }
        };
        document.body.addEventListener("pointermove", mouseDragHandler);
    });
}

export function resizeControllerEw(direction) {
    return resizeController((e) => e.pageX, getPaneWidth, (el_1, w) => {
        setPaneWidth(el_1, w);
    }, (el_2, w_1) => {
        setPaneWidth(el_2, w_1);
    }, direction);
}

export function resizeControllerNs(direction) {
    return resizeController((e) => e.pageY, getPaneHeight, (el_1, h) => {
        setPaneHeight(el_1, h);
    }, (el_2, h_1) => {
        setPaneHeight(el_2, h_1);
    }, direction);
}

export function resizeControllerNsFlex(direction) {
    return resizeController((e) => e.pageY, getPaneHeight, uncurry(2, setPaneHeightUsingFlexGrow), uncurry(2, setPaneHeightUsingFlexGrow), direction);
}

export function resizeControllerEwFlex(direction) {
    return resizeController((e) => e.pageX, getPaneWidth, uncurry(2, setPaneWidthUsingFlexGrow), uncurry(2, setPaneWidthUsingFlexGrow), direction);
}

export function toListFromNodeList(l) {
    return map((i) => [l.item(i), i], toList(rangeDouble(0, 1, l.length - 1)));
}

export function attributesAsList(e) {
    const l = e.attributes;
    return map((i) => [l.item(i), i], toList(rangeDouble(0, 1, l.length - 1)));
}

export function containsByWidth(clientX, el) {
    const r = el.getBoundingClientRect();
    if (clientX >= r.left) {
        return clientX <= r.right;
    }
    else {
        return false;
    }
}

export function whichHalfX(clientX, el) {
    const r = el.getBoundingClientRect();
    if (clientX < (r.left + (r.width / 2))) {
        return new TabHalf(0);
    }
    else {
        return new TabHalf(1);
    }
}

export function whichHalfY(clientY, el) {
    const r = el.getBoundingClientRect();
    if (clientY < (r.top + (r.height / 2))) {
        return new TabHalf(0);
    }
    else {
        return new TabHalf(1);
    }
}

export function containsByHeight(clientY, el) {
    const r = el.getBoundingClientRect();
    if (clientY >= r.top) {
        return clientY <= r.bottom;
    }
    else {
        return false;
    }
}

export function clearPreview() {
    iterate((tupledArg) => {
        const el = tupledArg[0];
        el.classList.remove("preview-insert-before");
        el.classList.remove("preview-insert-after");
    }, toListFromNodeList(document.querySelectorAll(".tab-label")));
}

