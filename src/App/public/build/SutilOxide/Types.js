import { Record, toString, Union } from "../fable_modules/fable-library.3.7.20/Types.js";
import { class_type, list_type, record_type, string_type, union_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { equals } from "../fable_modules/fable-library.3.7.20/Util.js";
import { fold, empty, ofArray } from "../fable_modules/fable-library.3.7.20/List.js";
import { FSharpMap__get_Item, empty as empty_1, FSharpMap__Add } from "../fable_modules/fable-library.3.7.20/Map.js";

export class TabHalf extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["FirstHalf", "SecondHalf"];
    }
}

export function TabHalf$reflection() {
    return union_type("SutilOxide.Types.TabHalf", [], TabHalf, () => [[], []]);
}

export class Orientation extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Horizontal", "Vertical"];
    }
}

export function Orientation$reflection() {
    return union_type("SutilOxide.Types.Orientation", [], Orientation, () => [[], []]);
}

export function Orientation__get_Opposite(__) {
    if (equals(__, new Orientation(0))) {
        return new Orientation(1);
    }
    else {
        return new Orientation(0);
    }
}

export class BasicLocation extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Left", "Right", "Centre", "Top", "Bottom"];
    }
}

export function BasicLocation$reflection() {
    return union_type("SutilOxide.Types.BasicLocation", [], BasicLocation, () => [[], [], [], [], []]);
}

export function BasicLocation__get_LowerName(__) {
    return toString(__).toLocaleLowerCase();
}

export function BasicLocation__get_Orientation(__) {
    switch (__.tag) {
        case 0:
        case 1: {
            return new Orientation(0);
        }
        default: {
            return new Orientation(1);
        }
    }
}

export function BasicLocation__get_Opposite(__) {
    switch (__.tag) {
        case 0: {
            return new BasicLocation(1);
        }
        case 1: {
            return new BasicLocation(0);
        }
        case 3: {
            return new BasicLocation(4);
        }
        case 4: {
            return new BasicLocation(3);
        }
        default: {
            return new BasicLocation(2);
        }
    }
}

export class DockLocation extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["LeftTop", "LeftBottom", "BottomLeft", "CentreCentre", "BottomRight", "RightTop", "RightBottom", "TopLeft", "TopRight"];
    }
}

export function DockLocation$reflection() {
    return union_type("SutilOxide.Types.DockLocation", [], DockLocation, () => [[], [], [], [], [], [], [], [], []]);
}

export function DockLocation_get_All() {
    return ofArray([new DockLocation(0), new DockLocation(1), new DockLocation(2), new DockLocation(4), new DockLocation(5), new DockLocation(6), new DockLocation(3), new DockLocation(7), new DockLocation(8)]);
}

export function DockLocation__get_Hand(__) {
    const matchValue = [DockLocation__get_Primary(__), DockLocation__get_Secondary(__)];
    let pattern_matching_result;
    if (matchValue[1].tag === 0) {
        pattern_matching_result = 0;
    }
    else if (matchValue[0].tag === 0) {
        pattern_matching_result = 0;
    }
    else {
        pattern_matching_result = 1;
    }
    switch (pattern_matching_result) {
        case 0: {
            return new BasicLocation(0);
        }
        case 1: {
            return new BasicLocation(1);
        }
    }
}

export function DockLocation__get_Primary(__) {
    switch (__.tag) {
        case 0:
        case 1: {
            return new BasicLocation(0);
        }
        case 5:
        case 6: {
            return new BasicLocation(1);
        }
        case 2:
        case 4: {
            return new BasicLocation(4);
        }
        case 7:
        case 8: {
            return new BasicLocation(3);
        }
        default: {
            return new BasicLocation(2);
        }
    }
}

export function DockLocation__get_Secondary(__) {
    switch (__.tag) {
        case 0:
        case 5: {
            return new BasicLocation(3);
        }
        case 1:
        case 6: {
            return new BasicLocation(4);
        }
        case 2:
        case 7: {
            return new BasicLocation(0);
        }
        case 4:
        case 8: {
            return new BasicLocation(1);
        }
        default: {
            return new BasicLocation(2);
        }
    }
}

export function DockLocation__get_CssName(__) {
    return (BasicLocation__get_LowerName(DockLocation__get_Primary(__)) + "-") + BasicLocation__get_LowerName(DockLocation__get_Secondary(__));
}

export class DockPane extends Record {
    constructor(Name) {
        super();
        this.Name = Name;
    }
}

export function DockPane$reflection() {
    return record_type("SutilOxide.Types.DockPane", [], DockPane, () => [["Name", string_type]]);
}

export class DockStation extends Record {
    constructor(Panes) {
        super();
        this.Panes = Panes;
    }
}

export function DockStation$reflection() {
    return record_type("SutilOxide.Types.DockStation", [], DockStation, () => [["Panes", list_type(DockPane$reflection())]]);
}

export function DockStation_get_Empty() {
    return new DockStation(empty());
}

export class DockCollection extends Record {
    constructor(Stations) {
        super();
        this.Stations = Stations;
    }
}

export function DockCollection$reflection() {
    return record_type("SutilOxide.Types.DockCollection", [], DockCollection, () => [["Stations", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [DockLocation$reflection(), DockStation$reflection()])]]);
}

export function DockCollection_get_Empty() {
    let list;
    return new DockCollection((list = DockLocation_get_All(), fold((s, e) => FSharpMap__Add(s, e, DockStation_get_Empty()), empty_1(), list)));
}

export function DockCollection__GetPanes_217D4758(__, loc) {
    return FSharpMap__get_Item(__.Stations, loc).Panes;
}

