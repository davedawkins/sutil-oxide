import { fragment, text, attr, elns } from "./CoreElements.js";
import { SvgEngine$1$reflection, SvgEngine$1 } from "../../../fable_modules/Feliz.Engine.1.0.0-beta-004/SvgEngine.fs.js";
import { SutilElement$reflection } from "./Core.js";
import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";

export function SvgInternal_svgel(tag, xs) {
    return elns("http://www.w3.org/2000/svg", tag, xs);
}

export function SvgInternal_svg(xs) {
    return SvgInternal_svgel("svg", xs);
}

export function SvgInternal_g(xs) {
    return SvgInternal_svgel("g", xs);
}

export function SvgInternal_rect(xs) {
    return SvgInternal_svgel("rect", xs);
}

export function SvgInternal_circle(xs) {
    return SvgInternal_svgel("circle", xs);
}

export function SvgInternal_pattern(xs) {
    return SvgInternal_svgel("pattern", xs);
}

export function SvgInternal_text(xs) {
    return SvgInternal_svgel("text", xs);
}

export function SvgInternal_line(xs) {
    return SvgInternal_svgel("line", xs);
}

export function SvgInternal_x(obj) {
    return attr("x", obj);
}

export function SvgInternal_y(obj) {
    return attr("y", obj);
}

export function SvgInternal_cx(obj) {
    return attr("cx", obj);
}

export function SvgInternal_cy(obj) {
    return attr("cy", obj);
}

export function SvgInternal_rx(obj) {
    return attr("rx", obj);
}

export function SvgInternal_ry(obj) {
    return attr("ry", obj);
}

export function SvgInternal_r(obj) {
    return attr("r", obj);
}

export function SvgInternal_x1(obj) {
    return attr("x1", obj);
}

export function SvgInternal_y1(obj) {
    return attr("y1", obj);
}

export function SvgInternal_x2(obj) {
    return attr("x2", obj);
}

export function SvgInternal_y2(obj) {
    return attr("y2", obj);
}

export function SvgInternal_width(obj) {
    return attr("width", obj);
}

export function SvgInternal_height(obj) {
    return attr("height", obj);
}

export function SvgInternal_transform(obj) {
    return attr("transform", obj);
}

export class SutilSvgEngine extends SvgEngine$1 {
    constructor() {
        super(SvgInternal_svgel, text, () => fragment([]));
    }
}

export function SutilSvgEngine$reflection() {
    return class_type("Sutil.SutilSvgEngine", void 0, SutilSvgEngine, SvgEngine$1$reflection(SutilElement$reflection()));
}

export function SutilSvgEngine_$ctor() {
    return new SutilSvgEngine();
}

export function SutilSvgEngine__get_x(_) {
    return SvgInternal_x;
}

export function SutilSvgEngine__get_x1(_) {
    return SvgInternal_x1;
}

export function SutilSvgEngine__get_x2(_) {
    return SvgInternal_x2;
}

export function SutilSvgEngine__get_y(_) {
    return SvgInternal_y;
}

export function SutilSvgEngine__get_y1(_) {
    return SvgInternal_y1;
}

export function SutilSvgEngine__get_y2(_) {
    return SvgInternal_y2;
}

export function SutilSvgEngine__get_width(_) {
    return SvgInternal_width;
}

export function SutilSvgEngine__get_height(_) {
    return SvgInternal_height;
}

export function SutilSvgEngine__get_transform(_) {
    return SvgInternal_transform;
}

export function SutilSvgEngine__text_Z5F036EB1(_, children) {
    return SvgInternal_text(children);
}

export const SvgEngineHelpers_Svg = SutilSvgEngine_$ctor();

