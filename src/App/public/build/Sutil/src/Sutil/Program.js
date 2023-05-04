import { Registry_initialise } from "./ObservableStore.js";
import { ElementRef, MountOp, ElementRef__get_AsElement } from "./Types.js";
import { mount } from "./Core.js";
import { exclusive } from "./CoreElements.js";
import { class_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { unmount } from "./DomHelpers.js";

export function ProgramModule__mount(mp_, mp__1, view) {
    const mp = [mp_, mp__1];
    const mp_1 = mp;
    const eref = mp_1[1];
    Registry_initialise(ElementRef__get_AsElement(eref).ownerDocument);
    return mount(view, mp_1[0], mp_1[1]);
}

export function ProgramModule_mountElementOnDocumentElement(host, app) {
    return ProgramModule__mount(new MountOp(0), new ElementRef(2, host), app);
}

export function ProgramModule_mountElementOnDocument(doc, id, app) {
    const host = doc.querySelector(`#${id}`);
    return ProgramModule__mount(new MountOp(0), new ElementRef(2, host), app);
}

export function ProgramModule_mountDomElement(host, app) {
    return ProgramModule__mount(new MountOp(0), new ElementRef(2, host), app);
}

export function ProgramModule_mountElement(id, app) {
    return ProgramModule__mount(new MountOp(0), new ElementRef(0, id), exclusive(app));
}

export function ProgramModule_mountElementAfter(prev, app) {
    return ProgramModule__mount(new MountOp(1), new ElementRef(2, prev), app);
}

export class Program {
    constructor() {
    }
}

export function Program$reflection() {
    return class_type("Sutil.Program", void 0, Program);
}

export function Program_$ctor() {
    return new Program();
}

export function Program_mount_24332BDB(id, app) {
    return ProgramModule__mount(new MountOp(0), new ElementRef(0, id), exclusive(app));
}

export function Program_mount_Z427DD8DF(host, app) {
    return ProgramModule__mount(new MountOp(0), new ElementRef(2, host), exclusive(app));
}

export function Program_mount_6E602840(app) {
    Program_mount_24332BDB("sutil-app", app);
}

export function Program_mount_37443D56(doc, id, app) {
    const host = doc.querySelector(`#${id}`);
    return Program_mount_Z427DD8DF(host, app);
}

export function Program_mountAfter_Z427DD8DF(prev, app) {
    return ProgramModule__mount(new MountOp(1), new ElementRef(2, prev), app);
}

export function Program_mountAppend_Z427DD8DF(prev, app) {
    return ProgramModule__mount(new MountOp(0), new ElementRef(2, prev), app);
}

export function Program_unmount_171AE942(node) {
    unmount(node);
}

