import { empty, toList, map, delay } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { rangeDouble } from "../../../fable_modules/fable-library.3.7.20/Range.js";
import { disposeSafe } from "../../../fable_modules/fable-library.3.7.20/Util.js";

export function fastNotEquals(x, y) {
    return !(x === y);
}

export function fileListToSeq(files) {
    if (!(files == null)) {
        return delay(() => map((i) => (files[i]), toList(rangeDouble(0, 1, files.length - 1))));
    }
    else {
        return empty();
    }
}

export function disposable(f) {
    return {
        Dispose() {
            f();
        },
    };
}

export function unsubify(d, unitVar) {
    disposeSafe(d);
}

export function makeIdGeneratorFrom(start) {
    let id = start;
    return () => {
        const r = id | 0;
        id = ((id + 1) | 0);
        return r | 0;
    };
}

export function makeIdGenerator() {
    return makeIdGeneratorFrom(0);
}

