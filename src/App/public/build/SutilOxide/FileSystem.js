import { Record, Union } from "../fable_modules/fable-library.3.7.20/Types.js";
import { class_type, uint8_type, record_type, array_type, tuple_type, int32_type, string_type, union_type } from "../fable_modules/fable-library.3.7.20/Reflection.js";
import { substring, printf, toText, join, split } from "../fable_modules/fable-library.3.7.20/String.js";
import { append as append_1, tryFind, choose, map as map_1, last, take } from "../fable_modules/fable-library.3.7.20/Array.js";
import { iterate as iterate_1, empty, singleton, append } from "../fable_modules/fable-library.3.7.20/List.js";
import { int32ToString, equals } from "../fable_modules/fable-library.3.7.20/Util.js";
import { bind, defaultArgWith, toArray, map, defaultArg } from "../fable_modules/fable-library.3.7.20/Option.js";
import { iterate } from "../fable_modules/fable-library.3.7.20/Seq.js";
import { log } from "./Logging.js";
import { createTypeInfo } from "../fable_modules/Fable.SimpleJson.3.24.0/TypeInfo.Converter.fs.js";
import { Convert_fromJson, Convert_serialize } from "../fable_modules/Fable.SimpleJson.3.24.0/Json.Converter.fs.js";
import { SimpleJson_tryParse } from "../fable_modules/Fable.SimpleJson.3.24.0/SimpleJson.fs.js";

export class FileEntryType extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["File", "Folder"];
    }
}

export function FileEntryType$reflection() {
    return union_type("SutilOxide.FileSystem.FileEntryType", [], FileEntryType, () => [[], []]);
}

function parsePath(path) {
    return split(path, ["/"], null, 1);
}

function buildPath(parts) {
    return "/" + join("/", parts);
}

function canonical(path) {
    return buildPath(parsePath(path));
}

export function getFolderName(path) {
    const items = parsePath(path);
    return buildPath(take(items.length - 1, items));
}

export function getFileName(path) {
    const items = parsePath(path);
    return last(items);
}

function combine(path, file) {
    return canonical(toText(printf("%s/%s"))(path)(file));
}

export function Storage_mk(rootKey, key) {
    return toText(printf("%s/%s"))(rootKey)(key);
}

export function Storage_exists(rootKey, key) {
    return window.localStorage.getItem(Storage_mk(rootKey, key)) !== null;
}

export function Storage_getContents(rootKey, key) {
    return window.localStorage.getItem(Storage_mk(rootKey, key));
}

export function Storage_setContents(rootKey, key, content) {
    window.localStorage.setItem(Storage_mk(rootKey, key), content);
}

export function Storage_remove(rootKey, key) {
    window.localStorage.removeItem(Storage_mk(rootKey, key));
}

export class FileEntry extends Record {
    constructor(Type, Name, Uid, Content, Children) {
        super();
        this.Type = Type;
        this.Name = Name;
        this.Uid = (Uid | 0);
        this.Content = Content;
        this.Children = Children;
    }
}

export function FileEntry$reflection() {
    return record_type("SutilOxide.FileSystem.FileEntry", [], FileEntry, () => [["Type", FileEntryType$reflection()], ["Name", string_type], ["Uid", int32_type], ["Content", string_type], ["Children", array_type(tuple_type(string_type, int32_type))]]);
}

export class FileContent extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Text", "Binary"];
    }
}

export function FileContent$reflection() {
    return union_type("SutilOxide.FileSystem.FileContent", [], FileContent, () => [[["Item", string_type]], [["Item", array_type(uint8_type)]]]);
}

export class Root extends Record {
    constructor(NextUid) {
        super();
        this.NextUid = (NextUid | 0);
    }
}

export function Root$reflection() {
    return record_type("SutilOxide.FileSystem.Root", [], Root, () => [["NextUid", int32_type]]);
}

export function SutilOxide_FileSystem_IFileSystem__IFileSystem_Combine_Static_Z384F8060(a, b) {
    return canonical(combine(a, b));
}

export function SutilOxide_FileSystem_IFileSystem__IFileSystem_GetFolderName_Static_Z721C83C5(path) {
    return getFolderName(path);
}

export function SutilOxide_FileSystem_IFileSystem__IFileSystem_GetFileName_Static_Z721C83C5(path) {
    return getFileName(path);
}

export function SutilOxide_FileSystem_IFileSystem__IFileSystem_GetExtension_Static_Z721C83C5(path) {
    const fileName = getFileName(path);
    const p = fileName.lastIndexOf(".") | 0;
    if (p < 0) {
        return "";
    }
    else {
        return substring(fileName, p);
    }
}

export class LocalStorageFileSystem {
    constructor(rootKey) {
        this.rootKey = rootKey;
        this.root = (new Root(1));
        this.onChange = empty();
        LocalStorageFileSystem__initRoot(this);
    }
    OnChange(cb) {
        const _ = this;
        _.onChange = append(_.onChange, singleton(cb));
    }
    Files(path) {
        const _ = this;
        return LocalStorageFileSystem__getEntriesWhere(_, (e) => equals(e.Type, new FileEntryType(0)), path);
    }
    Folders(path) {
        const _ = this;
        return LocalStorageFileSystem__getEntriesWhere(_, (e) => equals(e.Type, new FileEntryType(1)), path);
    }
    Exists(path) {
        const __ = this;
        return LocalStorageFileSystem__isEntry_Z721C83C5(__, path);
    }
    IsFile(path) {
        const __ = this;
        return LocalStorageFileSystem__isFile_Z721C83C5(__, path);
    }
    IsFolder(path) {
        const __ = this;
        return LocalStorageFileSystem__isFolder_Z721C83C5(__, path);
    }
    GetFileContent(path) {
        const _ = this;
        const cpath = canonical(path);
        if (!LocalStorageFileSystem__isFile_Z721C83C5(_, cpath)) {
            throw (new Error("Not a file: " + cpath));
        }
        return defaultArg(map((e) => e.Content, LocalStorageFileSystem__getEntryByPath_Z721C83C5(_, cpath)), "");
    }
    SetFileContent(path, content) {
        const __ = this;
        const cpath = canonical(path);
        if (LocalStorageFileSystem__isFolder_Z721C83C5(__, cpath)) {
            throw (new Error("Not a file: " + cpath));
        }
        if (!LocalStorageFileSystem__isFile_Z721C83C5(__, cpath)) {
            LocalStorageFileSystem__createFile(__, getFolderName(cpath), getFileName(cpath), false);
        }
        iterate((e) => {
            LocalStorageFileSystem__putEntry_Z8EC2B64(__, new FileEntry(e.Type, e.Name, e.Uid, content, e.Children));
        }, toArray(LocalStorageFileSystem__getEntryByPath_Z721C83C5(__, cpath)));
        log("SetFileContent " + path);
        LocalStorageFileSystem__notifyOnChange_Z721C83C5(__, path);
    }
    RemoveFile(path) {
        const __ = this;
        defaultArgWith(map((entry) => {
            const folderName = getFolderName(path);
            const fileName = getFileName(path);
            iterate((parentEntry) => {
                iterate((entries) => {
                    LocalStorageFileSystem__delEntry_Z8EC2B64(__, entry);
                    LocalStorageFileSystem__putEntry_Z8EC2B64(__, new FileEntry(parentEntry.Type, parentEntry.Name, parentEntry.Uid, parentEntry.Content, entries));
                }, toArray(map((array) => array.filter((tupledArg) => {
                    const name = tupledArg[0];
                    const uid_1 = tupledArg[1] | 0;
                    return name !== fileName;
                }), LocalStorageFileSystem__entryChildren_Z524259A4(__, parentEntry.Uid))));
            }, toArray(LocalStorageFileSystem__getEntryByPath_Z721C83C5(__, folderName)));
        }, LocalStorageFileSystem__getEntryByPath_Z721C83C5(__, path)), () => {
            throw (new Error(toText(printf("Cannot remove non-existent file \u0027%s\u0027"))(path)));
        });
        log("RemoveFile " + path);
        LocalStorageFileSystem__notifyOnChange_Z721C83C5(__, path);
    }
    CreateFolder(path) {
        const _ = this;
        LocalStorageFileSystem__createFolder(_, path, false);
    }
    CreateFile(path, name) {
        const __ = this;
        LocalStorageFileSystem__createFile(__, path, name, true);
    }
    RenameFile(path, newNameOrPath) {
        const __ = this;
        const cpath = canonical(path);
        let npath;
        if (newNameOrPath.indexOf("/") === 0) {
            npath = canonical(newNameOrPath);
        }
        else {
            LocalStorageFileSystem__validateFileName_Z721C83C5(__, newNameOrPath);
            npath = combine(getFolderName(cpath), newNameOrPath);
        }
        if ((cpath === "/") ? true : (npath === "/")) {
            throw (new Error("Cannot rename to/from \u0027/\u0027"));
        }
        if (!LocalStorageFileSystem__isEntry_Z721C83C5(__, cpath)) {
            throw (new Error("Cannot rename non-existent file: " + cpath));
        }
        if (LocalStorageFileSystem__isEntry_Z721C83C5(__, npath)) {
            throw (new Error("Cannot rename to existing file " + npath));
        }
        const cparent = getFolderName(cpath);
        const nparent = getFolderName(npath);
        const cname = getFileName(cpath);
        const nname = getFileName(npath);
        if (!LocalStorageFileSystem__isEntry_Z721C83C5(__, nparent)) {
            throw (new Error("Parent folder for rename target does not exist: " + nparent));
        }
        defaultArgWith(map((entry) => {
            if (cparent === nparent) {
                defaultArgWith(map((parentEntry) => {
                    LocalStorageFileSystem__putEntry_Z8EC2B64(__, new FileEntry(parentEntry.Type, parentEntry.Name, parentEntry.Uid, parentEntry.Content, map_1((tupledArg) => {
                        const name = tupledArg[0];
                        const uid = tupledArg[1] | 0;
                        if (name === cname) {
                            return [nname, uid];
                        }
                        else {
                            return [name, uid];
                        }
                    }, parentEntry.Children)));
                    LocalStorageFileSystem__putEntry_Z8EC2B64(__, new FileEntry(entry.Type, nname, entry.Uid, entry.Content, entry.Children));
                }, LocalStorageFileSystem__getEntryByPath_Z721C83C5(__, cparent)), () => {
                    throw (new Error("Cannot find entry for parent " + cparent));
                });
            }
        }, LocalStorageFileSystem__getEntryByPath_Z721C83C5(__, path)), () => {
            throw (new Error("Cannot find entry for " + path));
        });
        LocalStorageFileSystem__notifyOnChange_Z721C83C5(__, path);
    }
}

export function LocalStorageFileSystem$reflection() {
    return class_type("SutilOxide.FileSystem.LocalStorageFileSystem", void 0, LocalStorageFileSystem);
}

export function LocalStorageFileSystem_$ctor_Z721C83C5(rootKey) {
    return new LocalStorageFileSystem(rootKey);
}

function LocalStorageFileSystem__notifyOnChange_Z721C83C5(this$, path) {
    iterate_1((h) => {
        h(path);
    }, this$.onChange);
}

function LocalStorageFileSystem__uidKey_Z524259A4(this$, uid) {
    return toText(printf("uid:%d"))(uid);
}

function LocalStorageFileSystem__delEntry_Z8EC2B64(this$, e) {
    Storage_remove(this$.rootKey, LocalStorageFileSystem__uidKey_Z524259A4(this$, e.Uid));
}

function LocalStorageFileSystem__putEntry_Z8EC2B64(this$, e) {
    let typeInfo;
    Storage_setContents(this$.rootKey, LocalStorageFileSystem__uidKey_Z524259A4(this$, e.Uid), (typeInfo = createTypeInfo(FileEntry$reflection()), Convert_serialize(e, typeInfo)));
}

function LocalStorageFileSystem__getEntry_Z524259A4(this$, uid) {
    let matchValue, inputJson, typeInfo;
    try {
        return (matchValue = SimpleJson_tryParse(Storage_getContents(this$.rootKey, LocalStorageFileSystem__uidKey_Z524259A4(this$, uid))), (matchValue != null) ? ((inputJson = matchValue, (typeInfo = createTypeInfo(FileEntry$reflection()), Convert_fromJson(inputJson, typeInfo)))) : (() => {
            throw (new Error("Couldn\u0027t parse the input JSON string because it seems to be invalid"));
        })());
    }
    catch (matchValue_1) {
        return void 0;
    }
}

function LocalStorageFileSystem__entryExists_Z721C83C5(this$, uid) {
    return Storage_exists(this$.rootKey, uid);
}

function LocalStorageFileSystem__nameOf_Z8EC2B64(this$, e) {
    return e.Name;
}

function LocalStorageFileSystem__getEntries_Z524259A4(this$, uid) {
    let e;
    let result;
    const matchValue = LocalStorageFileSystem__getEntry_Z524259A4(this$, uid);
    if (matchValue != null) {
        if ((e = matchValue, !equals(e.Type, new FileEntryType(1)))) {
            const e_1 = matchValue;
            throw (new Error(toText(printf("Not a folder: %d"))(uid)));
        }
        else {
            const e_2 = matchValue;
            result = choose((x) => x, map_1((arg_1) => LocalStorageFileSystem__getEntry_Z524259A4(this$, arg_1[1]), e_2.Children));
        }
    }
    else {
        throw (new Error("Non-existent UID " + int32ToString(uid)));
    }
    return result;
}

function LocalStorageFileSystem__entryName_Z524259A4(this$, uid) {
    return map((e) => e.Name, LocalStorageFileSystem__getEntry_Z524259A4(this$, uid));
}

function LocalStorageFileSystem__entryNameWithDefault(this$, defaultName, uid) {
    return defaultArg(map((e) => e.Name, LocalStorageFileSystem__getEntry_Z524259A4(this$, uid)), defaultName);
}

function LocalStorageFileSystem__entryChildren_Z524259A4(this$, uid) {
    return map((e) => e.Children, LocalStorageFileSystem__getEntry_Z524259A4(this$, uid));
}

function LocalStorageFileSystem__entryChildNames_Z524259A4(this$, uid) {
    return map((array) => map_1((tuple) => tuple[0], array), LocalStorageFileSystem__entryChildren_Z524259A4(this$, uid));
}

function LocalStorageFileSystem__entryChildUids_Z524259A4(this$, uid) {
    return map((array) => map_1((tuple) => tuple[1], array, Int32Array), LocalStorageFileSystem__entryChildren_Z524259A4(this$, uid));
}

function LocalStorageFileSystem__uidOf_Z721C83C5(this$, path) {
    const parts = parsePath(path);
    const findUid = (parent_mut, parts_1_mut, i_mut) => {
        let n;
        findUid:
        while (true) {
            const parent = parent_mut, parts_1 = parts_1_mut, i = i_mut;
            if ((n = (i | 0), n >= parts_1.length)) {
                const n_1 = i | 0;
                return parent;
            }
            else {
                const matchValue = LocalStorageFileSystem__getEntry_Z524259A4(this$, parent);
                if (matchValue != null) {
                    const e = matchValue;
                    const matchValue_1 = tryFind((tupledArg) => {
                        const name = tupledArg[0];
                        return name === parts_1[i];
                    }, e.Children);
                    if (matchValue_1 != null) {
                        const uid = matchValue_1[1] | 0;
                        parent_mut = uid;
                        parts_1_mut = parts_1;
                        i_mut = (i + 1);
                        continue findUid;
                    }
                    else {
                        return void 0;
                    }
                }
                else {
                    throw (new Error("No entry found for part of path " + path));
                }
            }
            break;
        }
    };
    const result = findUid(0, parts, 0);
    return result;
}

function LocalStorageFileSystem__getEntryByPath_Z721C83C5(this$, path) {
    return bind((uid) => LocalStorageFileSystem__getEntry_Z524259A4(this$, uid), LocalStorageFileSystem__uidOf_Z721C83C5(this$, path));
}

function LocalStorageFileSystem__isEntry_Z721C83C5(this$, path) {
    if (LocalStorageFileSystem__getEntryByPath_Z721C83C5(this$, path) != null) {
        return true;
    }
    else {
        return false;
    }
}

function LocalStorageFileSystem__isFile_Z721C83C5(this$, path) {
    let e;
    const matchValue = LocalStorageFileSystem__getEntryByPath_Z721C83C5(this$, path);
    let pattern_matching_result;
    if (matchValue != null) {
        if ((e = matchValue, equals(e.Type, new FileEntryType(0)))) {
            pattern_matching_result = 0;
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
            return true;
        }
        case 1: {
            return false;
        }
    }
}

function LocalStorageFileSystem__isFolder_Z721C83C5(this$, path) {
    let e;
    const matchValue = LocalStorageFileSystem__getEntryByPath_Z721C83C5(this$, path);
    let pattern_matching_result;
    if (matchValue != null) {
        if ((e = matchValue, equals(e.Type, new FileEntryType(1)))) {
            pattern_matching_result = 0;
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
            return true;
        }
        case 1: {
            return false;
        }
    }
}

function LocalStorageFileSystem__makeKey_Z721C83C5(this$, path) {
    return "fs:" + path;
}

function LocalStorageFileSystem__validateFileName_Z721C83C5(this$, file) {
    if (((file.indexOf("..") >= 0) ? true : (file.indexOf("/") >= 0)) ? true : (file.indexOf("\\") >= 0)) {
        throw (new Error("Invalid file name: " + file));
    }
}

function LocalStorageFileSystem__getEntriesWhere(this$, filter, path) {
    const value = new Array(0);
    return defaultArg(map((id) => {
        let array;
        return map_1((f) => f.Name, (array = LocalStorageFileSystem__getEntries_Z524259A4(this$, id), array.filter(filter)));
    }, LocalStorageFileSystem__uidOf_Z721C83C5(this$, canonical(path))), value);
}

function LocalStorageFileSystem__putRoot(this$) {
    let value, typeInfo;
    Storage_setContents(this$.rootKey, "(root)", (value = this$.root, (typeInfo = createTypeInfo(Root$reflection()), Convert_serialize(value, typeInfo))));
}

function LocalStorageFileSystem__initRoot(this$) {
    let s, matchValue, inputJson, typeInfo;
    if (!Storage_exists(this$.rootKey, LocalStorageFileSystem__uidKey_Z524259A4(this$, 0))) {
        LocalStorageFileSystem__putEntry_Z8EC2B64(this$, new FileEntry(new FileEntryType(1), "/", 0, "", new Array(0)));
    }
    const _arg = Storage_getContents(this$.rootKey, "(root)");
    if ((s = _arg, s !== null)) {
        const s_1 = _arg;
        this$.root = ((matchValue = SimpleJson_tryParse(s_1), (matchValue != null) ? ((inputJson = matchValue, (typeInfo = createTypeInfo(Root$reflection()), Convert_fromJson(inputJson, typeInfo)))) : (() => {
            throw (new Error("Couldn\u0027t parse the input JSON string because it seems to be invalid"));
        })()));
    }
    LocalStorageFileSystem__putRoot(this$);
}

function LocalStorageFileSystem__newUid(this$) {
    let inputRecord;
    const uid = this$.root.NextUid | 0;
    this$.root = ((inputRecord = this$.root, new Root(uid + 1)));
    LocalStorageFileSystem__putRoot(this$);
    return uid | 0;
}

function LocalStorageFileSystem__createFile(this$, path, name, notify) {
    LocalStorageFileSystem__validateFileName_Z721C83C5(this$, name);
    const cpath = canonical(path);
    const fname = combine(cpath, name);
    if (LocalStorageFileSystem__isEntry_Z721C83C5(this$, fname)) {
        throw (new Error("File already exists: " + fname));
    }
    if (!LocalStorageFileSystem__isFolder_Z721C83C5(this$, cpath)) {
        throw (new Error("Not a folder: " + cpath));
    }
    defaultArgWith(map((entry) => {
        const uid = LocalStorageFileSystem__newUid(this$) | 0;
        LocalStorageFileSystem__putEntry_Z8EC2B64(this$, new FileEntry(entry.Type, entry.Name, entry.Uid, entry.Content, append_1([[name, uid]], entry.Children)));
        LocalStorageFileSystem__putEntry_Z8EC2B64(this$, new FileEntry(new FileEntryType(0), name, uid, "", new Array(0)));
    }, LocalStorageFileSystem__getEntryByPath_Z721C83C5(this$, cpath)), () => {
        throw (new Error("Parent folder does not exist"));
    });
    if (notify) {
        log((("CreateFile " + path) + " ") + name);
        LocalStorageFileSystem__notifyOnChange_Z721C83C5(this$, fname);
    }
}

function LocalStorageFileSystem__createFolder(this$, folderPath, notify) {
    const name = getFileName(folderPath);
    const parent = getFolderName(folderPath);
    LocalStorageFileSystem__validateFileName_Z721C83C5(this$, name);
    const cpath = canonical(parent);
    const fname = combine(cpath, name);
    if (LocalStorageFileSystem__isEntry_Z721C83C5(this$, fname)) {
        throw (new Error("File already exists: " + fname));
    }
    if (!LocalStorageFileSystem__isFolder_Z721C83C5(this$, cpath)) {
        throw (new Error("Not a folder: " + cpath));
    }
    defaultArgWith(map((entry) => {
        const uid = LocalStorageFileSystem__newUid(this$) | 0;
        LocalStorageFileSystem__putEntry_Z8EC2B64(this$, new FileEntry(entry.Type, entry.Name, entry.Uid, entry.Content, append_1([[name, uid]], entry.Children)));
        LocalStorageFileSystem__putEntry_Z8EC2B64(this$, new FileEntry(new FileEntryType(1), name, uid, "", new Array(0)));
    }, LocalStorageFileSystem__getEntryByPath_Z721C83C5(this$, cpath)), () => {
        throw (new Error("Parent folder does not exist"));
    });
    if (notify) {
        log((("CreateFolder " + fname) + " ") + name);
        LocalStorageFileSystem__notifyOnChange_Z721C83C5(this$, fname);
    }
}

