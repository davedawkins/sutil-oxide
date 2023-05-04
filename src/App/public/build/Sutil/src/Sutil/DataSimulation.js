import { take, fold as fold_1, toList, head, sortBy } from "../../../fable_modules/fable-library.3.7.20/Seq.js";
import { equals, round, comparePrimitives } from "../../../fable_modules/fable-library.3.7.20/Util.js";
import { Record, Union } from "../../../fable_modules/fable-library.3.7.20/Types.js";
import { float64_type, record_type, bool_type, string_type, class_type, union_type } from "../../../fable_modules/fable-library.3.7.20/Reflection.js";
import { makeStore } from "./ObservableStore.js";
import { interval } from "./DomHelpers.js";
import { Store_modify } from "./Store.js";
import { take as take_1, ofArray, map, fold, mapIndexed, cons, filter, isEmpty, length, append, singleton, empty } from "../../../fable_modules/fable-library.3.7.20/List.js";
import { rangeChar, rangeDouble } from "../../../fable_modules/fable-library.3.7.20/Range.js";
import { printf, toText } from "../../../fable_modules/fable-library.3.7.20/String.js";

export function Random_shuffleR(xs) {
    return sortBy((_arg) => (~(~((Math.random()) * 100))), xs, {
        Compare: comparePrimitives,
    });
}

export function Random_pickOne(xs) {
    return head(Random_shuffleR(xs));
}

export function Random_randomInt(min, max) {
    return min + (~(~round((Math.random()) * (max - min))));
}

export function Random_randomSign(n) {
    return n * ((Random_randomInt(0, 1) === 0) ? 1 : -1);
}

class Edit extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Nop", "Create", "Update", "Delete"];
    }
}

function Edit$reflection() {
    return union_type("Sutil.Edit", [], Edit, () => [[], [], [], []]);
}

export class DataSimulation {
    constructor() {
    }
}

export function DataSimulation$reflection() {
    return class_type("Sutil.DataSimulation", void 0, DataSimulation);
}

export function DataSimulation_Stream(init, f, delay) {
    let dispose = null;
    let tick = 0;
    const store = makeStore(init, (_arg) => {
        dispose();
    });
    dispose = interval(() => {
        tick = ((tick + 1) | 0);
        Store_modify((v) => f(tick, v), store);
    }, delay);
    return store;
}

export function DataSimulation_CountList_4F7761DC(min, max, delay) {
    const n = ((max - min) + 1) | 0;
    return DataSimulation_Stream(empty, (t, current) => {
        const i = (t % n) | 0;
        return (i === 0) ? singleton(min) : append(current, singleton(min + i));
    }, delay);
}

export function DataSimulation_Records_Z36979C25(data, update, maxEditsPerTick, allowCreateDelete, delay) {
    const dataLen = length(data) | 0;
    let removed = empty();
    const chooseEdit = (current) => {
        if (allowCreateDelete) {
            if (isEmpty(current) && (!isEmpty(removed))) {
                return new Edit(1);
            }
            else if (isEmpty(removed)) {
                return Random_pickOne([new Edit(2), new Edit(3)]);
            }
            else {
                return Random_pickOne([new Edit(1), new Edit(2), new Edit(3)]);
            }
        }
        else {
            return new Edit(2);
        }
    };
    const editOne = (current_1) => {
        const matchValue = chooseEdit(current_1);
        switch (matchValue.tag) {
            case 1: {
                const item = Random_pickOne(removed);
                removed = filter((r) => (!equals(r, item)), removed);
                return append(current_1, singleton(item));
            }
            case 3: {
                const item_1 = Random_pickOne(current_1);
                removed = cons(item_1, removed);
                return filter((r_1) => (!equals(r_1, item_1)), current_1);
            }
            case 2: {
                const item_2 = Random_randomInt(0, dataLen - 1) | 0;
                return mapIndexed((i, r_2) => {
                    if (i === item_2) {
                        return update(r_2);
                    }
                    else {
                        return r_2;
                    }
                }, current_1);
            }
            default: {
                return current_1;
            }
        }
    };
    const edit = (_arg, current_2) => {
        const n = (1 + (~(~((Math.random()) * maxEditsPerTick)))) | 0;
        return fold((current$0027, _arg_1) => editOne(current$0027), current_2, toList(rangeDouble(1, 1, n)));
    };
    return DataSimulation_Stream(() => data, edit, delay);
}

export function DataSimulation_Records_Z6F411353(data, update, delay) {
    return DataSimulation_Records_Z36979C25(data, update, 1, true, delay);
}

export function DataSimulation_Count_4F7761DC(min, max, delay) {
    return DataSimulation_Stream(() => min, (_arg_1, n) => ((n === max) ? min : (n + 1)), delay);
}

export function DataSimulation_Random_76A3BBFC(min, max, delay) {
    const next = () => (min + ((max - min) * (Math.random())));
    return DataSimulation_Stream(next, (_arg, _arg_1) => next(), delay);
}

export function DataSimulation_Random_4F7761DC(min, max, delay) {
    const next = () => (min + (~(~round((max - min) * (Math.random())))));
    return DataSimulation_Stream(next, (_arg, _arg_1) => next(), delay);
}

export function DataSimulation_Random_Z524259A4(max) {
    return DataSimulation_Random_4F7761DC(1, max, 1000);
}

export function DataSimulation_Random() {
    return DataSimulation_Stream(() => (Math.random()), (_arg, _arg_1) => (Math.random()), 1000);
}

export class SampleData_Todo extends Record {
    constructor(Description, Completed) {
        super();
        this.Description = Description;
        this.Completed = Completed;
    }
}

export function SampleData_Todo$reflection() {
    return record_type("Sutil.SampleData.Todo", [], SampleData_Todo, () => [["Description", string_type], ["Completed", bool_type]]);
}

export class SampleData_Name extends Record {
    constructor(Name, Surname, Email, Zip) {
        super();
        this.Name = Name;
        this.Surname = Surname;
        this.Email = Email;
        this.Zip = Zip;
    }
}

export function SampleData_Name$reflection() {
    return record_type("Sutil.SampleData.Name", [], SampleData_Name, () => [["Name", string_type], ["Surname", string_type], ["Email", string_type], ["Zip", string_type]]);
}

export class SampleData_Stock extends Record {
    constructor(Symbol$, Price) {
        super();
        this.Symbol = Symbol$;
        this.Price = Price;
    }
}

export function SampleData_Stock$reflection() {
    return record_type("Sutil.SampleData.Stock", [], SampleData_Stock, () => [["Symbol", string_type], ["Price", float64_type]]);
}

export function SampleData_updateStock(r) {
    return new SampleData_Stock(r.Symbol, r.Price * (1 + Random_randomSign(0.01)));
}

export function SampleData_sampleStocks(number) {
    const nextSymbol = () => fold_1((s, c) => toText(printf("%s%c"))(s)(c), "", take(4, Random_shuffleR(toList(rangeChar("A", "Z")))));
    return map((_arg) => (new SampleData_Stock(nextSymbol(), round((Math.random()) * 100, 2))), toList(rangeDouble(1, 1, number)));
}

export function SampleData_stockFeed(numStocks, delay) {
    return DataSimulation_Records_Z36979C25(SampleData_sampleStocks(numStocks), SampleData_updateStock, 5, false, delay);
}

export function SampleData_sampleTodos() {
    return ofArray([new SampleData_Todo("Write documentation", false), new SampleData_Todo("Create website", true), new SampleData_Todo("Check spellling", false), new SampleData_Todo("Write tests", false), new SampleData_Todo("Create package", true), new SampleData_Todo("DevTools for Firefox", true), new SampleData_Todo("DevTools for Chrome", false), new SampleData_Todo("Implement benchmarks", false), new SampleData_Todo("Create templates", false), new SampleData_Todo("WebGL version", false)]);
}

export function SampleData_updateTodo(r) {
    return new SampleData_Todo(r.Description, !r.Completed);
}

export function SampleData_todosFeed(numTodos, delay) {
    return DataSimulation_Records_Z36979C25(take_1(numTodos, SampleData_sampleTodos()), SampleData_updateTodo, 1, true, delay);
}

export const SampleData_sampleNames = ofArray([new SampleData_Name("Alvin", "Melendez", "tristique@ligulaNullam.net", "50619"), new SampleData_Name("Brooke", "Cline", "ullamcorper.nisl.arcu@ligulaAenean.co.uk", "61195"), new SampleData_Name("Jameson", "Douglas", "Donec.est@mollisneccursus.ca", "494841"), new SampleData_Name("Uta", "Wade", "dapibus.rutrum.justo@auctornonfeugiat.net", "Z4008"), new SampleData_Name("Burke", "Guerra", "tristique.senectus.et@Quisque.com", "68-482"), new SampleData_Name("Joseph", "Abbott", "risus@mollis.ca", "432199"), new SampleData_Name("Alma", "Dominguez", "augue@nibhPhasellus.ca", "CX8 3LX"), new SampleData_Name("Buckminster", "Bauer", "enim.Nunc.ut@Integer.co.uk", "R3X 6T2"), new SampleData_Name("Belle", "Wilcox", "mollis.non.cursus@amet.edu", "60090"), new SampleData_Name("Caleb", "Burnett", "quam@ornare.com", "56446")]);

