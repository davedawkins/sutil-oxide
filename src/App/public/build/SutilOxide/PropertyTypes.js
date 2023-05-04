import { Record, Union } from "../fable_modules/fable-library.3.7.20/Types.js";
import { record_type, name, class_type, bool_type, float64_type, int32_type, string_type, equals, union_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { empty, FSharpMap__get_Item, FSharpMap__Add } from "../fable_modules/fable-library.3.7.20/Map.js";

export class DataType extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Int32", "Flt32", "Boolean", "String", "DateTime"];
    }
}

export function DataType$reflection() {
    return union_type("SutilOxide.PropertyTypes.DataType", [], DataType, () => [[], [], [], [], []]);
}

export function DataType_From_24524716(t) {
    let x, x_1, x_2, x_3, x_4;
    if ((x = t, equals(x, string_type))) {
        const x_5 = t;
        return new DataType(3);
    }
    else if ((x_1 = t, equals(x_1, int32_type))) {
        const x_6 = t;
        return new DataType(0);
    }
    else if ((x_2 = t, equals(x_2, float64_type))) {
        const x_7 = t;
        return new DataType(1);
    }
    else if ((x_3 = t, equals(x_3, bool_type))) {
        const x_8 = t;
        return new DataType(2);
    }
    else if ((x_4 = t, equals(x_4, class_type("System.DateTime")))) {
        const x_9 = t;
        return new DataType(4);
    }
    else {
        throw (new Error("Cannot map type: " + name(t)));
    }
}

export class MutableMap$2 extends Record {
    constructor(Map$) {
        super();
        this.Map = Map$;
    }
}

export function MutableMap$2$reflection(gen0, gen1) {
    return record_type("SutilOxide.PropertyTypes.MutableMap`2", [gen0, gen1], MutableMap$2, () => [["Map", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [gen0, gen1])]]);
}

export function MutableMap$2__Set_5BDDA1(__, k, v) {
    __.Map = FSharpMap__Add(__.Map, k, v);
}

export function MutableMap$2__Get_2B595(__, k) {
    return FSharpMap__get_Item(__.Map, k);
}

export function MutableMap$2__get_Nap(__) {
    return __.Map;
}

export function MutableMap$2_get_Empty() {
    return new MutableMap$2(empty());
}

