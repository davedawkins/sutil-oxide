// ts2fable 0.8.0
module rec SutilOxide.AceEditor

#nowarn "3390" // disable warnings for invalid XML comments

open System
open Fable.Core
open Fable.Core.JS
open Browser.Types

//[<Literal>]
//let BasePath = "../../public/ace-builds/src-min-noconflict/ace.js"

[<Erase>] type KeyOf<'T> = Key of string
type Array<'T> = System.Collections.Generic.IList<'T>
type Function = System.Action
type RegExp = System.Text.RegularExpressions.Regex

let [<Import("Ace","module")>] ace: Ace.IExports = jsNative
let [<Import("version","module")>] version: string = jsNative
//let [<Import("config","../../node_modules/ace-builds/src-min-noconflict/ace.js")>] config: Ace.Config = jsNative
let [<Import("VirtualRenderer","module")>] VirtualRenderer: {| Create: HTMLElement -> string option -> obj |} = jsNative
let [<Import("EditSession","module")>] EditSession: {| Create: U2<string, Document> -> Ace.SyntaxMode option -> obj |} = jsNative
let [<Import("UndoManager","module")>] UndoManager: {| Create: unit -> obj |} = jsNative
let [<Import("Range","module")>] Range: {| Create: float -> float -> float -> float -> obj; fromPoints: Ace.Point -> Ace.Point -> Ace.Range; comparePoints: Ace.Point -> Ace.Point -> float |} = jsNative

type [<AllowNullLiteral>] IExports =
    abstract require: name: string -> obj option
    abstract edit: el: U2<Element, string> * ?options: obj -> Ace.Editor
    abstract createEditSession: text: U2<Ace.Document, string> * mode: Ace.SyntaxMode -> Ace.EditSession
    abstract config : Ace.Config
    
module Ace =

    type [<AllowNullLiteral>] IExports =
        abstract Selection: {| Create: EditSession -> obj |} with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] NewLineMode =
        | Auto
        | Unix
        | Windows

    type [<AllowNullLiteral>] Anchor =
        inherit EventEmitter
        abstract getPosition: unit -> Position
        abstract getDocument: unit -> Document
        abstract setPosition: row: float * column: float * ?noClip: bool -> unit
        abstract detach: unit -> unit
        abstract attach: doc: Document -> unit

    type [<AllowNullLiteral>] Document =
        inherit EventEmitter
        abstract setValue: text: string -> unit
        abstract getValue: unit -> string
        abstract createAnchor: row: float * column: float -> Anchor
        abstract getNewLineCharacter: unit -> string
        abstract setNewLineMode: newLineMode: NewLineMode -> unit
        abstract getNewLineMode: unit -> NewLineMode
        abstract isNewLine: text: string -> bool
        abstract getLine: row: float -> string
        abstract getLines: firstRow: float * lastRow: float -> ResizeArray<string>
        abstract getAllLines: unit -> ResizeArray<string>
        abstract getLength: unit -> float
        abstract getTextRange: range: Range -> string
        abstract getLinesForRange: range: Range -> ResizeArray<string>
        abstract insert: position: Position * text: string -> Position
        abstract insert: position: {| row: float; column: float |} * text: string -> Position
        abstract insertInLine: position: Position * text: string -> Position
        abstract insertNewLine: position: Point -> Point
        abstract clippedPos: row: float * column: float -> Point
        abstract clonePos: pos: Point -> Point
        abstract pos: row: float * column: float -> Point
        abstract insertFullLines: row: float * lines: ResizeArray<string> -> unit
        abstract insertMergedLines: position: Position * lines: ResizeArray<string> -> Point
        abstract remove: range: Range -> Position
        abstract removeInLine: row: float * startColumn: float * endColumn: float -> Position
        abstract removeFullLines: firstRow: float * lastRow: float -> ResizeArray<string>
        abstract removeNewLine: row: float -> unit
        abstract replace: range: Range * text: string -> Position
        abstract applyDeltas: deltas: ResizeArray<Delta> -> unit
        abstract revertDeltas: deltas: ResizeArray<Delta> -> unit
        abstract applyDelta: delta: Delta * ?doNotValidate: bool -> unit
        abstract revertDelta: delta: Delta -> unit
        abstract indexToPosition: index: float * startRow: float -> Position
        abstract positionToIndex: pos: Position * ?startRow: float -> float

    type [<AllowNullLiteral>] FoldLine =
        abstract folds: ResizeArray<Fold> with get, set
        abstract range: Range with get, set
        abstract start: Point with get, set
        abstract ``end``: Point with get, set
        abstract shiftRow: shift: float -> unit
        abstract addFold: fold: Fold -> unit
        abstract containsRow: row: float -> bool
        abstract walk: callback: Function * ?endRow: float * ?endColumn: float -> unit
        abstract getNextFoldTo: row: float * column: float -> {| fold: Fold; kind: string |} option
        abstract addRemoveChars: row: float * column: float * len: float -> unit
        abstract split: row: float * column: float -> FoldLine
        abstract merge: foldLineNext: FoldLine -> unit
        abstract idxToPosition: idx: float -> Point

    type [<AllowNullLiteral>] Fold =
        abstract range: Range with get, set
        abstract start: Point with get, set
        abstract ``end``: Point with get, set
        abstract foldLine: FoldLine option with get, set
        abstract sameRow: bool with get, set
        abstract subFolds: ResizeArray<Fold> with get, set
        abstract setFoldLine: foldLine: FoldLine -> unit
        abstract clone: unit -> Fold
        abstract addSubFold: fold: Fold -> Fold
        abstract restoreRange: range: Range -> unit

    type [<AllowNullLiteral>] Folding =
        abstract getFoldAt: row: float * column: float * side: float -> Fold
        abstract getFoldsInRange: range: Range -> ResizeArray<Fold>
        abstract getFoldsInRangeList: ranges: ResizeArray<Range> -> ResizeArray<Fold>
        abstract getAllFolds: unit -> ResizeArray<Fold>
        abstract getFoldStringAt: row: float * column: float * ?trim: float * ?foldLine: FoldLine -> string option
        abstract getFoldLine: docRow: float * ?startFoldLine: FoldLine -> FoldLine option
        abstract getNextFoldLine: docRow: float * ?startFoldLine: FoldLine -> FoldLine option
        abstract getFoldedRowCount: first: float * last: float -> float
        abstract addFold: placeholder: U2<string, Fold> * ?range: Range -> Fold
        abstract addFolds: folds: ResizeArray<Fold> -> unit
        abstract removeFold: fold: Fold -> unit
        abstract removeFolds: folds: ResizeArray<Fold> -> unit
        abstract expandFold: fold: Fold -> unit
        abstract expandFolds: folds: ResizeArray<Fold> -> unit
        abstract unfold: location: U3<float, Point, Range> option * ?expandInner: bool -> ResizeArray<Fold> option
        abstract isRowFolded: docRow: float * ?startFoldRow: FoldLine -> bool
        abstract getFoldRowEnd: docRow: float * ?startFoldRow: FoldLine -> float
        abstract getFoldRowStart: docRow: float * ?startFoldRow: FoldLine -> float
        abstract getFoldDisplayLine: foldLine: FoldLine * endRow: float option * endColumn: float option * startRow: float option * startColumn: float option -> string
        abstract getDisplayLine: row: float * endColumn: float option * startRow: float option * startColumn: float option -> string
        abstract toggleFold: ?tryToUnfold: bool -> unit
        abstract getCommentFoldRange: row: float * column: float * dir: float -> Range option
        abstract foldAll: ?startRow: float * ?endRow: float * ?depth: float -> unit
        abstract setFoldStyle: style: string -> unit
        abstract getParentFoldRangeData: row: float * ?ignoreCurrent: bool -> {| range: Range option; firstRange: Range |}
        abstract toggleFoldWidget: ?toggleParent: bool -> unit
        abstract updateFoldWidgets: delta: Delta -> unit

    type [<AllowNullLiteral>] Range =
        abstract start: Point with get, set
        abstract ``end``: Point with get, set
        abstract isEqual: range: Range -> bool
        abstract toString: unit -> string
        abstract contains: row: float * column: float -> bool
        abstract compareRange: range: Range -> float
        abstract comparePoint: p: Point -> float
        abstract containsRange: range: Range -> bool
        abstract intersects: range: Range -> bool
        abstract isEnd: row: float * column: float -> bool
        abstract isStart: row: float * column: float -> bool
        abstract setStart: row: float * column: float -> unit
        abstract setEnd: row: float * column: float -> unit
        abstract inside: row: float * column: float -> bool
        abstract insideStart: row: float * column: float -> bool
        abstract insideEnd: row: float * column: float -> bool
        abstract compare: row: float * column: float -> float
        abstract compareStart: row: float * column: float -> float
        abstract compareEnd: row: float * column: float -> float
        abstract compareInside: row: float * column: float -> float
        abstract clipRows: firstRow: float * lastRow: float -> Range
        abstract extend: row: float * column: float -> Range
        abstract isEmpty: unit -> bool
        abstract isMultiLine: unit -> bool
        abstract clone: unit -> Range
        abstract collapseRows: unit -> Range
        abstract toScreenRange: session: EditSession -> Range
        abstract moveBy: row: float * column: float -> unit

    type [<AllowNullLiteral>] EditSessionOptions =
        abstract wrap: U3<bool, float, string> with get, set
        abstract wrapMethod: EditSessionOptionsWrapMethod with get, set
        abstract indentedSoftWrap: bool with get, set
        abstract firstLineNumber: float with get, set
        abstract useWorker: bool with get, set
        abstract useSoftTabs: bool with get, set
        abstract tabSize: float with get, set
        abstract navigateWithinSoftTabs: bool with get, set
        abstract foldStyle: EditSessionOptionsFoldStyle with get, set
        abstract overwrite: bool with get, set
        abstract newLineMode: NewLineMode with get, set
        abstract mode: string with get, set

    type [<AllowNullLiteral>] VirtualRendererOptions =
        abstract animatedScroll: bool with get, set
        abstract showInvisibles: bool with get, set
        abstract showPrintMargin: bool with get, set
        abstract printMarginColumn: float with get, set
        abstract printMargin: U2<bool, float> with get, set
        abstract showGutter: bool with get, set
        abstract fadeFoldWidgets: bool with get, set
        abstract showFoldWidgets: bool with get, set
        abstract showLineNumbers: bool with get, set
        abstract displayIndentGuides: bool with get, set
        abstract highlightGutterLine: bool with get, set
        abstract hScrollBarAlwaysVisible: bool with get, set
        abstract vScrollBarAlwaysVisible: bool with get, set
        abstract fontSize: float with get, set
        abstract fontFamily: string with get, set
        abstract maxLines: float with get, set
        abstract minLines: float with get, set
        abstract scrollPastEnd: bool with get, set
        abstract fixedWidthGutter: bool with get, set
        abstract theme: string with get, set
        abstract hasCssTransforms: bool with get, set
        abstract maxPixelHeight: float with get, set

    type [<AllowNullLiteral>] MouseHandlerOptions =
        abstract scrollSpeed: float with get, set
        abstract dragDelay: float with get, set
        abstract dragEnabled: bool with get, set
        abstract focusTimeout: float with get, set
        abstract tooltipFollowsMouse: bool with get, set

    type [<AllowNullLiteral>] EditorOptions =
        inherit EditSessionOptions
        inherit MouseHandlerOptions
        inherit VirtualRendererOptions
        abstract selectionStyle: string with get, set
        abstract highlightActiveLine: bool with get, set
        abstract highlightSelectedWord: bool with get, set
        abstract readOnly: bool with get, set
        abstract copyWithEmptySelection: bool with get, set
        abstract cursorStyle: EditorOptionsCursorStyle with get, set
        abstract mergeUndoDeltas: EditorOptionsMergeUndoDeltas with get, set
        abstract behavioursEnabled: bool with get, set
        abstract wrapBehavioursEnabled: bool with get, set
        abstract enableAutoIndent: bool with get, set
        abstract autoScrollEditorIntoView: bool with get, set
        abstract keyboardHandler: string with get, set
        abstract placeholder: string with get, set
        abstract value: string with get, set
        abstract session: EditSession with get, set

    type [<AllowNullLiteral>] SearchOptions =
        abstract needle: U2<string, RegExp> with get, set
        abstract preventScroll: bool with get, set
        abstract backwards: bool with get, set
        abstract start: Range with get, set
        abstract skipCurrent: bool with get, set
        abstract range: Range with get, set
        abstract preserveCase: bool with get, set
        abstract regExp: RegExp with get, set
        abstract wholeWord: bool with get, set
        abstract caseSensitive: bool with get, set
        abstract wrap: bool with get, set

    type [<AllowNullLiteral>] EventEmitter =
        abstract once: name: string * callback: Function -> unit
        abstract setDefaultHandler: name: string * callback: Function -> unit
        abstract removeDefaultHandler: name: string * callback: Function -> unit
        abstract on: name: string * callback: Function * ?capturing: bool -> unit
        abstract addEventListener: name: string * callback: Function * ?capturing: bool -> unit
        abstract off: name: string * callback: Function -> unit
        abstract removeListener: name: string * callback: Function -> unit
        abstract removeEventListener: name: string * callback: Function -> unit

    type [<AllowNullLiteral>] Point =
        abstract row: float with get, set
        abstract column: float with get, set

    type [<AllowNullLiteral>] Delta =
        abstract action: DeltaAction with get, set
        abstract start: Point with get, set
        abstract ``end``: Point with get, set
        abstract lines: ResizeArray<string> with get, set

    type [<AllowNullLiteral>] Annotation =
        abstract row: float option with get, set
        abstract column: float option with get, set
        abstract text: string with get, set
        abstract ``type``: string with get, set

    type [<AllowNullLiteral>] Command =
        abstract name: string option with get, set
        abstract bindKey: U2<string, {| mac: string option; win: string option |}> option with get, set
        abstract readOnly: bool option with get, set
        abstract exec: (Editor -> (obj) option -> unit) with get, set

    type CommandLike =
        U2<Command, (Editor -> unit)>

    type [<AllowNullLiteral>] KeyboardHandler =
        abstract handleKeyboard: Function with get, set

    type [<AllowNullLiteral>] MarkerLike =
        abstract range: Range with get, set
        abstract ``type``: string with get, set
        abstract renderer: MarkerRenderer option with get, set
        abstract clazz: string with get, set
        abstract inFront: bool with get, set
        abstract id: float with get, set
        abstract update: (ResizeArray<string> -> obj option -> EditSession -> obj option -> unit) option with get, set

    type [<AllowNullLiteral>] MarkerRenderer =
        [<Emit("$0($1...)")>] abstract Invoke: html: ResizeArray<string> * range: Range * left: float * top: float * config: obj option -> unit

    type [<AllowNullLiteral>] Token =
        abstract ``type``: string with get, set
        abstract value: string with get, set
        abstract index: float option with get, set
        abstract start: float option with get, set

    type [<AllowNullLiteral>] Completion =
        abstract value: string with get, set
        abstract score: float with get, set
        abstract meta: string option with get, set
        abstract name: string option with get, set
        abstract caption: string option with get, set

    type [<AllowNullLiteral>] Tokenizer =
        abstract removeCapturingGroups: src: string -> string
        abstract createSplitterRegexp: src: string * ?flag: string -> RegExp
        abstract getLineTokens: line: string * startState: U2<string, ResizeArray<string>> -> ResizeArray<Token>

    type [<AllowNullLiteral>] TokenIterator =
        abstract getCurrentToken: unit -> Token
        abstract getCurrentTokenColumn: unit -> float
        abstract getCurrentTokenRow: unit -> float
        abstract getCurrentTokenPosition: unit -> Point
        abstract getCurrentTokenRange: unit -> Range
        abstract stepBackward: unit -> Token
        abstract stepForward: unit -> Token

    type [<AllowNullLiteral>] SyntaxMode =
        abstract getTokenizer: unit -> Tokenizer
        abstract toggleCommentLines: state: obj option * session: EditSession * startRow: float * endRow: float -> unit
        abstract toggleBlockComment: state: obj option * session: EditSession * range: Range * cursor: Point -> unit
        abstract getNextLineIndent: state: obj option * line: string * tab: string -> string
        abstract checkOutdent: state: obj option * line: string * input: string -> bool
        abstract autoOutdent: state: obj option * doc: Document * row: float -> unit
        abstract createWorker: session: EditSession -> obj option
        abstract createModeDelegates: mapping: SyntaxModeCreateModeDelegatesMapping -> unit
        abstract transformAction: state: string * action: string * editor: Editor * session: EditSession * text: string -> obj option
        abstract getKeywords: ?append: bool -> Array<U2<string, RegExp>>
        abstract getCompletions: state: string * session: EditSession * pos: Point * prefix: string -> ResizeArray<Completion>

    /// <summary>
    /// Typescript interface contains an <see href="https://www.typescriptlang.org/docs/handbook/2/objects.html#index-signatures">index signature</see> (like <c>{ [key:string]: string }</c>).
    /// Unlike an indexer in F#, index signatures index over a type's members.
    ///
    /// As such an index signature cannot be implemented via regular F# Indexer (<c>Item</c> property),
    /// but instead by just specifying fields.
    ///
    /// Easiest way to declare such a type is with an Anonymous Record and force it into the function.
    /// For example:
    /// <code lang="fsharp">
    /// type I =
    ///     [&lt;EmitIndexer&gt;]
    ///     abstract Item: string -&gt; string
    /// let f (i: I) = jsNative
    ///
    /// let t = {| Value1 = "foo"; Value2 = "bar" |}
    /// f (!! t)
    /// </code>
    /// </summary>
    type [<AllowNullLiteral>] SyntaxModeCreateModeDelegatesMapping =
        [<EmitIndexer>] abstract Item: key: string -> string with get, set

    type [<AllowNullLiteral>] Config =
        abstract get: key: string -> obj option
        abstract set: key: string * value: obj option -> unit
        abstract all: unit -> ConfigAllReturn
        abstract moduleUrl: name: string * ?``component``: string -> string
        abstract setModuleUrl: name: string * subst: string -> string
        abstract loadModule: moduleName: U2<string, string * string> * ?onLoad: (obj option -> unit) -> unit
        abstract init: packaged: obj option -> obj option
        abstract defineOptions: obj: obj option * path: string * options: ConfigDefineOptionsOptions -> Config
        abstract resetOptions: obj: obj option -> unit
        abstract setDefaultValue: path: string * name: string * value: obj option -> unit
        abstract setDefaultValues: path: string * optionHash: ConfigSetDefaultValuesOptionHash -> unit

    type [<AllowNullLiteral>] ConfigAllReturn =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    /// <summary>
    /// Typescript interface contains an <see href="https://www.typescriptlang.org/docs/handbook/2/objects.html#index-signatures">index signature</see> (like <c>{ [key:string]: string }</c>).
    /// Unlike an indexer in F#, index signatures index over a type's members.
    ///
    /// As such an index signature cannot be implemented via regular F# Indexer (<c>Item</c> property),
    /// but instead by just specifying fields.
    ///
    /// Easiest way to declare such a type is with an Anonymous Record and force it into the function.
    /// For example:
    /// <code lang="fsharp">
    /// type I =
    ///     [&lt;EmitIndexer&gt;]
    ///     abstract Item: string -&gt; string
    /// let f (i: I) = jsNative
    ///
    /// let t = {| Value1 = "foo"; Value2 = "bar" |}
    /// f (!! t)
    /// </code>
    /// </summary>
    type [<AllowNullLiteral>] ConfigDefineOptionsOptions =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    /// <summary>
    /// Typescript interface contains an <see href="https://www.typescriptlang.org/docs/handbook/2/objects.html#index-signatures">index signature</see> (like <c>{ [key:string]: string }</c>).
    /// Unlike an indexer in F#, index signatures index over a type's members.
    ///
    /// As such an index signature cannot be implemented via regular F# Indexer (<c>Item</c> property),
    /// but instead by just specifying fields.
    ///
    /// Easiest way to declare such a type is with an Anonymous Record and force it into the function.
    /// For example:
    /// <code lang="fsharp">
    /// type I =
    ///     [&lt;EmitIndexer&gt;]
    ///     abstract Item: string -&gt; string
    /// let f (i: I) = jsNative
    ///
    /// let t = {| Value1 = "foo"; Value2 = "bar" |}
    /// f (!! t)
    /// </code>
    /// </summary>
    type [<AllowNullLiteral>] ConfigSetDefaultValuesOptionHash =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    type [<AllowNullLiteral>] OptionsProvider =
        abstract setOptions: optList: (*OptionsProviderSetOptionsOptList*) obj -> unit
        abstract getOptions: ?optionNames: U2<ResizeArray<string>, OptionsProviderGetOptions> -> OptionsProviderGetOptionsReturn
        abstract setOption: name: string * value: obj -> unit
        abstract getOption: name: string -> obj option

    /// <summary>
    /// Typescript interface contains an <see href="https://www.typescriptlang.org/docs/handbook/2/objects.html#index-signatures">index signature</see> (like <c>{ [key:string]: string }</c>).
    /// Unlike an indexer in F#, index signatures index over a type's members.
    ///
    /// As such an index signature cannot be implemented via regular F# Indexer (<c>Item</c> property),
    /// but instead by just specifying fields.
    ///
    /// Easiest way to declare such a type is with an Anonymous Record and force it into the function.
    /// For example:
    /// <code lang="fsharp">
    /// type I =
    ///     [&lt;EmitIndexer&gt;]
    ///     abstract Item: string -&gt; string
    /// let f (i: I) = jsNative
    ///
    /// let t = {| Value1 = "foo"; Value2 = "bar" |}
    /// f (!! t)
    /// </code>
    /// </summary>
    type [<AllowNullLiteral>] OptionsProviderSetOptionsOptList =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    type [<AllowNullLiteral>] OptionsProviderGetOptionsReturn =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    type [<AllowNullLiteral>] UndoManager =
        abstract addSession: session: EditSession -> unit
        abstract add: delta: Delta * allowMerge: bool * session: EditSession -> unit
        abstract addSelection: selection: string * ?rev: float -> unit
        abstract startNewGroup: unit -> unit
        abstract markIgnored: from: float * ?``to``: float -> unit
        abstract getSelection: rev: float * ?after: bool -> {| value: string; rev: float |}
        abstract getRevision: unit -> float
        abstract getDeltas: from: float * ?``to``: float -> ResizeArray<Delta>
        abstract undo: session: EditSession * ?dontSelect: bool -> unit
        abstract redo: session: EditSession * ?dontSelect: bool -> unit
        abstract reset: unit -> unit
        abstract canUndo: unit -> bool
        abstract canRedo: unit -> bool
        abstract bookmark: ?rev: float -> unit
        abstract isAtBookmark: unit -> bool

    type [<AllowNullLiteral>] Position =
        abstract row: float with get, set
        abstract column: float with get, set

    type [<AllowNullLiteral>] EditSession =
        inherit EventEmitter
        inherit OptionsProvider
        abstract selection: Selection with get, set
        [<Emit("$0.on('changeFold',$1)")>] abstract on_changeFold: callback: ({| data: Fold; action: string |} -> unit) -> Function
        [<Emit("$0.on('changeScrollLeft',$1)")>] abstract on_changeScrollLeft: callback: (float -> unit) -> Function
        [<Emit("$0.on('changeScrollTop',$1)")>] abstract on_changeScrollTop: callback: (float -> unit) -> Function
        [<Emit("$0.on('tokenizerUpdate',$1)")>] abstract on_tokenizerUpdate: callback: ({| data: {| first: float; last: float |} |} -> unit) -> Function
        [<Emit("$0.on('change',$1)")>] abstract on_change: callback: (unit -> unit) -> Function
        abstract setOption: name: KeyOf<EditSessionOptions> * value: obj -> unit
        abstract getOption: name: KeyOf<EditSessionOptions> -> obj
        abstract doc: Document
        abstract setDocument: doc: Document -> unit
        abstract getDocument: unit -> Document
        abstract resetCaches: unit -> unit
        abstract setValue: text: string -> unit
        abstract getValue: unit -> string
        abstract getSelection: unit -> Selection
        abstract getState: row: float -> string
        abstract getTokens: row: float -> ResizeArray<Token>
        abstract getTokenAt: row: float * column: float -> Token option
        abstract setUndoManager: undoManager: UndoManager -> unit
        abstract markUndoGroup: unit -> unit
        abstract getUndoManager: unit -> UndoManager
        abstract getTabString: unit -> string
        abstract setUseSoftTabs: ``val``: bool -> unit
        abstract getUseSoftTabs: unit -> bool
        abstract setTabSize: tabSize: float -> unit
        abstract getTabSize: unit -> float
        abstract isTabStop: position: Position -> bool
        abstract setNavigateWithinSoftTabs: navigateWithinSoftTabs: bool -> unit
        abstract getNavigateWithinSoftTabs: unit -> bool
        abstract setOverwrite: overwrite: bool -> unit
        abstract getOverwrite: unit -> bool
        abstract toggleOverwrite: unit -> unit
        abstract addGutterDecoration: row: float * className: string -> unit
        abstract removeGutterDecoration: row: float * className: string -> unit
        abstract getBreakpoints: unit -> ResizeArray<string>
        abstract setBreakpoints: rows: ResizeArray<float> -> unit
        abstract clearBreakpoints: unit -> unit
        abstract setBreakpoint: row: float * className: string -> unit
        abstract clearBreakpoint: row: float -> unit
        abstract addMarker: range: Range * clazz: string * ``type``: U2<MarkerRenderer, string> * inFront: bool -> float
        abstract addDynamicMarker: marker: MarkerLike * inFront: bool -> MarkerLike
        abstract removeMarker: markerId: float -> unit
        abstract getMarkers: ?inFront: bool -> ResizeArray<MarkerLike>
        abstract highlight: re: RegExp -> unit
        abstract highlightLines: startRow: float * endRow: float * clazz: string * ?inFront: bool -> Range
        abstract setAnnotations: annotations: ResizeArray<Annotation> -> unit
        abstract getAnnotations: unit -> ResizeArray<Annotation>
        abstract clearAnnotations: unit -> unit
        abstract getWordRange: row: float * column: float -> Range
        abstract getAWordRange: row: float * column: float -> Range
        abstract setNewLineMode: newLineMode: NewLineMode -> unit
        abstract getNewLineMode: unit -> NewLineMode
        abstract setUseWorker: useWorker: bool -> unit
        abstract getUseWorker: unit -> bool
        abstract setMode: mode: U2<string, SyntaxMode> * ?callback: (unit -> unit) -> unit
        abstract getMode: unit -> SyntaxMode
        abstract setScrollTop: scrollTop: float -> unit
        abstract getScrollTop: unit -> float
        abstract setScrollLeft: scrollLeft: float -> unit
        abstract getScrollLeft: unit -> float
        abstract getScreenWidth: unit -> float
        abstract getLineWidgetMaxWidth: unit -> float
        abstract getLine: row: float -> string
        abstract getLines: firstRow: float * lastRow: float -> ResizeArray<string>
        abstract getLength: unit -> float
        abstract getTextRange: range: Range -> string
        abstract insert: position: Position * text: string -> unit
        abstract remove: range: Range -> unit
        abstract removeFullLines: firstRow: float * lastRow: float -> unit
        abstract undoChanges: deltas: ResizeArray<Delta> * ?dontSelect: bool -> unit
        abstract redoChanges: deltas: ResizeArray<Delta> * ?dontSelect: bool -> unit
        abstract setUndoSelect: enable: bool -> unit
        abstract replace: range: Range * text: string -> unit
        abstract moveText: fromRange: Range * toPosition: Position * ?copy: bool -> unit
        abstract indentRows: startRow: float * endRow: float * indentString: string -> unit
        abstract outdentRows: range: Range -> unit
        abstract moveLinesUp: firstRow: float * lastRow: float -> unit
        abstract moveLinesDown: firstRow: float * lastRow: float -> unit
        abstract duplicateLines: firstRow: float * lastRow: float -> unit
        abstract setUseWrapMode: useWrapMode: bool -> unit
        abstract getUseWrapMode: unit -> bool
        abstract setWrapLimitRange: min: float * max: float -> unit
        abstract adjustWrapLimit: desiredLimit: float -> bool
        abstract getWrapLimit: unit -> float
        abstract setWrapLimit: limit: float -> unit
        abstract getWrapLimitRange: unit -> {| min: float; max: float |}
        abstract getRowLineCount: row: float -> float
        abstract getRowWrapIndent: screenRow: float -> float
        abstract getScreenLastRowColumn: screenRow: float -> float
        abstract getDocumentLastRowColumn: docRow: float * docColumn: float -> float
        abstract getdocumentLastRowColumnPosition: docRow: float * docColumn: float -> Position
        abstract getRowSplitData: row: float -> string option
        abstract getScreenTabSize: screenColumn: float -> float
        abstract screenToDocumentRow: screenRow: float * screenColumn: float -> float
        abstract screenToDocumentColumn: screenRow: float * screenColumn: float -> float
        abstract screenToDocumentPosition: screenRow: float * screenColumn: float * ?offsetX: float -> Position
        abstract documentToScreenPosition: docRow: float * docColumn: float -> Position
        abstract documentToScreenPosition: position: Position -> Position
        abstract documentToScreenColumn: row: float * docColumn: float -> float
        abstract documentToScreenRow: docRow: float * docColumn: float -> float
        abstract getScreenLength: unit -> float
        abstract destroy: unit -> unit

    type [<AllowNullLiteral>] KeyBinding =
        abstract setDefaultHandler: handler: KeyboardHandler -> unit
        abstract setKeyboardHandler: handler: KeyboardHandler -> unit
        abstract addKeyboardHandler: handler: KeyboardHandler * ?pos: float -> unit
        abstract removeKeyboardHandler: handler: KeyboardHandler -> bool
        abstract getKeyboardHandler: unit -> KeyboardHandler
        abstract getStatusText: unit -> string
        abstract onCommandKey: e: obj option * hashId: float * keyCode: float -> bool
        abstract onTextInput: text: string -> bool

    type [<AllowNullLiteral>] CommandMap =
        [<EmitIndexer>] abstract Item: name: string -> Command with get, set

    type [<AllowNullLiteral>] execEventHandler =
        [<Emit("$0($1...)")>] abstract Invoke: obj: {| editor: Editor; command: Command; args: ResizeArray<obj option> |} -> unit

    type [<AllowNullLiteral>] CommandManager =
        inherit EventEmitter
        abstract byName: CommandMap with get, set
        abstract commands: CommandMap with get, set
        [<Emit("$0.on('exec',$1)")>] abstract on_exec: callback: execEventHandler -> Function
        [<Emit("$0.on('afterExec',$1)")>] abstract on_afterExec: callback: execEventHandler -> Function
        abstract once: name: string * callback: Function -> unit
        abstract setDefaultHandler: name: string * callback: Function -> unit
        abstract removeDefaultHandler: name: string * callback: Function -> unit
        abstract on: name: string * callback: Function * ?capturing: bool -> unit
        abstract addEventListener: name: string * callback: Function * ?capturing: bool -> unit
        abstract off: name: string * callback: Function -> unit
        abstract removeListener: name: string * callback: Function -> unit
        abstract removeEventListener: name: string * callback: Function -> unit
        abstract exec: command: string * editor: Editor * args: obj option -> bool
        abstract toggleRecording: editor: Editor -> unit
        abstract replay: editor: Editor -> unit
        abstract addCommand: command: Command -> unit
        abstract addCommands: command: ResizeArray<Command> -> unit
        abstract removeCommand: command: U2<Command, string> * ?keepCommand: bool -> unit
        abstract removeCommands: command: ResizeArray<Command> -> unit
        abstract bindKey: key: U2<string, {| mac: string option; win: string option |}> * command: CommandLike * ?position: float -> unit
        abstract bindKeys: keys: CommandManagerBindKeysKeys -> unit
        abstract parseKeys: keyPart: string -> {| key: string; hashId: float |}
        abstract findKeyCommand: hashId: float * keyString: string -> string option
        abstract handleKeyboard: data: CommandManagerHandleKeyboardData * hashId: float * keyString: string * keyCode: U2<string, float> -> U2<unit, {| command: string |}>
        abstract getStatusText: editor: Editor * data: CommandManagerGetStatusTextData -> string

    /// <summary>
    /// Typescript interface contains an <see href="https://www.typescriptlang.org/docs/handbook/2/objects.html#index-signatures">index signature</see> (like <c>{ [key:string]: string }</c>).
    /// Unlike an indexer in F#, index signatures index over a type's members.
    ///
    /// As such an index signature cannot be implemented via regular F# Indexer (<c>Item</c> property),
    /// but instead by just specifying fields.
    ///
    /// Easiest way to declare such a type is with an Anonymous Record and force it into the function.
    /// For example:
    /// <code lang="fsharp">
    /// type I =
    ///     [&lt;EmitIndexer&gt;]
    ///     abstract Item: string -&gt; string
    /// let f (i: I) = jsNative
    ///
    /// let t = {| Value1 = "foo"; Value2 = "bar" |}
    /// f (!! t)
    /// </code>
    /// </summary>
    type [<AllowNullLiteral>] CommandManagerBindKeysKeys =
        [<EmitIndexer>] abstract Item: s: string -> Function with get, set

    type [<AllowNullLiteral>] CommandManagerHandleKeyboardData =
        interface end

    type [<AllowNullLiteral>] CommandManagerGetStatusTextData =
        interface end

    type [<AllowNullLiteral>] VirtualRenderer =
        inherit OptionsProvider
        inherit EventEmitter
        abstract container: HTMLElement
        abstract scroller: HTMLElement
        abstract content: HTMLElement
        abstract characterWidth: float
        abstract lineHeight: float
        abstract scrollLeft: float
        abstract scrollTop: float
        abstract ``$padding``: float
        abstract setOption: name: KeyOf<VirtualRendererOptions> * value: obj -> unit
        abstract getOption: name: KeyOf<VirtualRendererOptions> -> obj
        abstract setSession: session: EditSession -> unit
        abstract updateLines: firstRow: float * lastRow: float * ?force: bool -> unit
        abstract updateText: unit -> unit
        abstract updateFull: ?force: bool -> unit
        abstract updateFontSize: unit -> unit
        abstract adjustWrapLimit: unit -> bool
        abstract setAnimatedScroll: shouldAnimate: bool -> unit
        abstract getAnimatedScroll: unit -> bool
        abstract setShowInvisibles: showInvisibles: bool -> unit
        abstract getShowInvisibles: unit -> bool
        abstract setDisplayIndentGuides: display: bool -> unit
        abstract getDisplayIndentGuides: unit -> bool
        abstract setShowPrintMargin: showPrintMargin: bool -> unit
        abstract getShowPrintMargin: unit -> bool
        abstract setPrintMarginColumn: showPrintMargin: bool -> unit
        abstract getPrintMarginColumn: unit -> bool
        abstract setShowGutter: show: bool -> unit
        abstract getShowGutter: unit -> bool
        abstract setFadeFoldWidgets: show: bool -> unit
        abstract getFadeFoldWidgets: unit -> bool
        abstract setHighlightGutterLine: shouldHighlight: bool -> unit
        abstract getHighlightGutterLine: unit -> bool
        abstract getContainerElement: unit -> HTMLElement
        abstract getMouseEventTarget: unit -> HTMLElement
        abstract getTextAreaContainer: unit -> HTMLElement
        abstract getFirstVisibleRow: unit -> float
        abstract getFirstFullyVisibleRow: unit -> float
        abstract getLastFullyVisibleRow: unit -> float
        abstract getLastVisibleRow: unit -> float
        abstract setPadding: padding: float -> unit
        abstract setScrollMargin: top: float * bottom: float * left: float * right: float -> unit
        abstract setHScrollBarAlwaysVisible: alwaysVisible: bool -> unit
        abstract getHScrollBarAlwaysVisible: unit -> bool
        abstract setVScrollBarAlwaysVisible: alwaysVisible: bool -> unit
        abstract getVScrollBarAlwaysVisible: unit -> bool
        abstract freeze: unit -> unit
        abstract unfreeze: unit -> unit
        abstract updateFrontMarkers: unit -> unit
        abstract updateBackMarkers: unit -> unit
        abstract updateBreakpoints: unit -> unit
        abstract setAnnotations: annotations: ResizeArray<Annotation> -> unit
        abstract updateCursor: unit -> unit
        abstract hideCursor: unit -> unit
        abstract showCursor: unit -> unit
        abstract scrollSelectionIntoView: anchor: Position * lead: Position * ?offset: float -> unit
        abstract scrollCursorIntoView: cursor: Position * ?offset: float -> unit
        abstract getScrollTop: unit -> float
        abstract getScrollLeft: unit -> float
        abstract getScrollTopRow: unit -> float
        abstract getScrollBottomRow: unit -> float
        abstract scrollToRow: row: float -> unit
        abstract alignCursor: cursor: U2<Position, float> * alignment: float -> float
        abstract scrollToLine: line: float * center: bool * animate: bool * callback: (unit -> unit) -> unit
        abstract animateScrolling: fromValue: float * callback: (unit -> unit) -> unit
        abstract scrollToY: scrollTop: float -> unit
        abstract scrollToX: scrollLeft: float -> unit
        abstract scrollTo: x: float * y: float -> unit
        abstract scrollBy: deltaX: float * deltaY: float -> unit
        abstract isScrollableBy: deltaX: float * deltaY: float -> bool
        abstract textToScreenCoordinates: row: float * column: float -> {| pageX: float; pageY: float |}
        abstract pixelToScreenCoordinates: x: float * y: float -> {| row: float; column: float; side: VirtualRendererPixelToScreenCoordinatesSide; offsetX: float |}
        abstract visualizeFocus: unit -> unit
        abstract visualizeBlur: unit -> unit
        abstract showComposition: position: float -> unit
        abstract setCompositionText: text: string -> unit
        abstract hideComposition: unit -> unit
        abstract setTheme: theme: string * ?callback: (unit -> unit) -> unit
        abstract getTheme: unit -> string
        abstract setStyle: style: string * ?``include``: bool -> unit
        abstract unsetStyle: style: string -> unit
        abstract setCursorStyle: style: string -> unit
        abstract setMouseCursor: cursorStyle: string -> unit
        abstract attachToShadowRoot: unit -> unit
        abstract destroy: unit -> unit

    type [<AllowNullLiteral>] Selection =
        inherit EventEmitter
        abstract moveCursorWordLeft: unit -> unit
        abstract moveCursorWordRight: unit -> unit
        abstract fromOrientedRange: range: Range -> unit
        abstract setSelectionRange: ``match``: obj option -> unit
        abstract getAllRanges: unit -> ResizeArray<Range>
        abstract addRange: range: Range -> unit
        abstract isEmpty: unit -> bool
        abstract isMultiLine: unit -> bool
        abstract setCursor: row: float * column: float -> unit
        abstract setAnchor: row: float * column: float -> unit
        abstract getAnchor: unit -> Position
        abstract getCursor: unit -> Position
        abstract isBackwards: unit -> bool
        abstract getRange: unit -> Range
        abstract clearSelection: unit -> unit
        abstract selectAll: unit -> unit
        abstract setRange: range: Range * ?reverse: bool -> unit
        abstract selectTo: row: float * column: float -> unit
        abstract selectToPosition: pos: obj option -> unit
        abstract selectUp: unit -> unit
        abstract selectDown: unit -> unit
        abstract selectRight: unit -> unit
        abstract selectLeft: unit -> unit
        abstract selectLineStart: unit -> unit
        abstract selectLineEnd: unit -> unit
        abstract selectFileEnd: unit -> unit
        abstract selectFileStart: unit -> unit
        abstract selectWordRight: unit -> unit
        abstract selectWordLeft: unit -> unit
        abstract getWordRange: unit -> unit
        abstract selectWord: unit -> unit
        abstract selectAWord: unit -> unit
        abstract selectLine: unit -> unit
        abstract moveCursorUp: unit -> unit
        abstract moveCursorDown: unit -> unit
        abstract moveCursorLeft: unit -> unit
        abstract moveCursorRight: unit -> unit
        abstract moveCursorLineStart: unit -> unit
        abstract moveCursorLineEnd: unit -> unit
        abstract moveCursorFileEnd: unit -> unit
        abstract moveCursorFileStart: unit -> unit
        abstract moveCursorLongWordRight: unit -> unit
        abstract moveCursorLongWordLeft: unit -> unit
        abstract moveCursorBy: rows: float * chars: float -> unit
        abstract moveCursorToPosition: position: obj option -> unit
        abstract moveCursorTo: row: float * column: float * ?keepDesiredColumn: bool -> unit
        abstract moveCursorToScreen: row: float * column: float * keepDesiredColumn: bool -> unit
        abstract toJSON: unit -> U2<SavedSelection, ResizeArray<SavedSelection>>
        abstract fromJSON: selection: U2<SavedSelection, ResizeArray<SavedSelection>> -> unit

    type [<AllowNullLiteral>] SavedSelection =
        abstract start: Point with get, set
        abstract ``end``: Point with get, set
        abstract isBackwards: bool with get, set

    type [<AllowNullLiteral>] TextInput =
        abstract resetSelection: unit -> unit

    type [<AllowNullLiteral>] Editor =
        inherit OptionsProvider
        inherit EventEmitter
        abstract container: HTMLElement with get, set
        abstract renderer: VirtualRenderer with get, set
        abstract id: string with get, set
        abstract commands: CommandManager with get, set
        abstract keyBinding: KeyBinding with get, set
        abstract session: EditSession with get, set
        abstract selection: Selection with get, set
        abstract textInput: TextInput with get, set
        [<Emit("$0.on('blur',$1)")>] abstract on_blur: callback: (Event -> unit) -> unit
        [<Emit("$0.on('change',$1)")>] abstract on_change: callback: (Delta -> unit) -> unit
        [<Emit("$0.on('changeSelectionStyle',$1)")>] abstract on_changeSelectionStyle: callback: ({| data: string |} -> unit) -> unit
        [<Emit("$0.on('changeSession',$1)")>] abstract on_changeSession: callback: ({| session: EditSession; oldSession: EditSession |} -> unit) -> unit
        [<Emit("$0.on('copy',$1)")>] abstract on_copy: callback: ({| text: string |} -> unit) -> unit
        [<Emit("$0.on('focus',$1)")>] abstract on_focus: callback: (Event -> unit) -> unit
        [<Emit("$0.on('paste',$1)")>] abstract on_paste: callback: ({| text: string |} -> unit) -> unit
        [<Emit("$0.on('mousemove',$1)")>] abstract on_mousemove: callback: (obj option -> unit) -> unit
        [<Emit("$0.on('mouseup',$1)")>] abstract on_mouseup: callback: (obj option -> unit) -> unit
        [<Emit("$0.on('mousewheel',$1)")>] abstract on_mousewheel: callback: (obj option -> unit) -> unit
        [<Emit("$0.on('click',$1)")>] abstract on_click: callback: (obj option -> unit) -> unit
        abstract onPaste: text: string * ``event``: obj option -> unit
        //abstract setOption: name: KeyOf<EditorOptions> * value: obj -> unit
        abstract getOption: name: KeyOf<EditorOptions> -> obj
        abstract setKeyboardHandler: keyboardHandler: string * ?callback: (unit -> unit) -> unit
        abstract setKeyboardHandler: keyboardHandler: KeyboardHandler -> unit
        abstract getKeyboardHandler: unit -> string
        abstract setSession: session: EditSession -> unit
        abstract getSession: unit -> EditSession
        abstract setValue: ``val``: string * ?cursorPos: float -> string
        abstract getValue: unit -> string
        abstract getSelection: unit -> Selection
        abstract resize: ?force: bool -> unit
        abstract setTheme: theme: string * ?callback: (unit -> unit) -> unit
        abstract getTheme: unit -> string
        abstract setStyle: style: string -> unit
        abstract unsetStyle: style: string -> unit
        abstract getFontSize: unit -> string
        abstract setFontSize: size: float -> unit
        abstract focus: unit -> unit
        abstract isFocused: unit -> bool
        abstract blur: unit -> unit
        abstract getSelectedText: unit -> string
        abstract getCopyText: unit -> string
        abstract execCommand: command: U2<string, ResizeArray<string>> * ?args: obj -> bool
        abstract insert: text: string * ?pasted: bool -> unit
        abstract setOverwrite: overwrite: bool -> unit
        abstract getOverwrite: unit -> bool
        abstract toggleOverwrite: unit -> unit
        abstract setScrollSpeed: speed: float -> unit
        abstract getScrollSpeed: unit -> float
        abstract setDragDelay: dragDelay: float -> unit
        abstract getDragDelay: unit -> float
        abstract setSelectionStyle: ``val``: string -> unit
        abstract getSelectionStyle: unit -> string
        abstract setHighlightActiveLine: shouldHighlight: bool -> unit
        abstract getHighlightActiveLine: unit -> bool
        abstract setHighlightGutterLine: shouldHighlight: bool -> unit
        abstract getHighlightGutterLine: unit -> bool
        abstract setHighlightSelectedWord: shouldHighlight: bool -> unit
        abstract getHighlightSelectedWord: unit -> bool
        abstract setAnimatedScroll: shouldAnimate: bool -> unit
        abstract getAnimatedScroll: unit -> bool
        abstract setShowInvisibles: showInvisibles: bool -> unit
        abstract getShowInvisibles: unit -> bool
        abstract setDisplayIndentGuides: display: bool -> unit
        abstract getDisplayIndentGuides: unit -> bool
        abstract setShowPrintMargin: showPrintMargin: bool -> unit
        abstract getShowPrintMargin: unit -> bool
        abstract setPrintMarginColumn: showPrintMargin: float -> unit
        abstract getPrintMarginColumn: unit -> float
        abstract setReadOnly: readOnly: bool -> unit
        abstract getReadOnly: unit -> bool
        abstract setBehavioursEnabled: enabled: bool -> unit
        abstract getBehavioursEnabled: unit -> bool
        abstract setWrapBehavioursEnabled: enabled: bool -> unit
        abstract getWrapBehavioursEnabled: unit -> bool
        abstract setShowFoldWidgets: show: bool -> unit
        abstract getShowFoldWidgets: unit -> bool
        abstract setFadeFoldWidgets: fade: bool -> unit
        abstract getFadeFoldWidgets: unit -> bool
        abstract remove: ?dir: EditorRemove -> unit
        abstract removeWordRight: unit -> unit
        abstract removeWordLeft: unit -> unit
        abstract removeLineToEnd: unit -> unit
        abstract splitLine: unit -> unit
        abstract transposeLetters: unit -> unit
        abstract toLowerCase: unit -> unit
        abstract toUpperCase: unit -> unit
        abstract indent: unit -> unit
        abstract blockIndent: unit -> unit
        abstract blockOutdent: unit -> unit
        abstract sortLines: unit -> unit
        abstract toggleCommentLines: unit -> unit
        abstract toggleBlockComment: unit -> unit
        abstract modifyNumber: amount: float -> unit
        abstract removeLines: unit -> unit
        abstract duplicateSelection: unit -> unit
        abstract moveLinesDown: unit -> unit
        abstract moveLinesUp: unit -> unit
        abstract moveText: range: Range * toPosition: Point * ?copy: bool -> Range
        abstract copyLinesUp: unit -> unit
        abstract copyLinesDown: unit -> unit
        abstract getFirstVisibleRow: unit -> float
        abstract getLastVisibleRow: unit -> float
        abstract isRowVisible: row: float -> bool
        abstract isRowFullyVisible: row: float -> bool
        abstract selectPageDown: unit -> unit
        abstract selectPageUp: unit -> unit
        abstract gotoPageDown: unit -> unit
        abstract gotoPageUp: unit -> unit
        abstract scrollPageDown: unit -> unit
        abstract scrollPageUp: unit -> unit
        abstract scrollToRow: row: float -> unit
        abstract scrollToLine: line: float * center: bool * animate: bool * callback: (unit -> unit) -> unit
        abstract centerSelection: unit -> unit
        abstract getCursorPosition: unit -> Point
        abstract getCursorPositionScreen: unit -> Point
        abstract getSelectionRange: unit -> Range
        abstract selectAll: unit -> unit
        abstract clearSelection: unit -> unit
        abstract moveCursorTo: row: float * column: float -> unit
        abstract moveCursorToPosition: pos: Point -> unit
        abstract jumpToMatching: select: bool * expand: bool -> unit
        abstract gotoLine: lineNumber: float * column: float * animate: bool -> unit
        abstract navigateTo: row: float * column: float -> unit
        abstract navigateUp: unit -> unit
        abstract navigateDown: unit -> unit
        abstract navigateLeft: unit -> unit
        abstract navigateRight: unit -> unit
        abstract navigateLineStart: unit -> unit
        abstract navigateLineEnd: unit -> unit
        abstract navigateFileEnd: unit -> unit
        abstract navigateFileStart: unit -> unit
        abstract navigateWordRight: unit -> unit
        abstract navigateWordLeft: unit -> unit
        abstract replace: replacement: string * ?options: obj -> float
        abstract replaceAll: replacement: string * ?options: obj -> float
        abstract getLastSearchOptions: unit -> obj
        abstract find: needle: U2<string, RegExp> * ?options: obj * ?animate: bool -> Ace.Range option
        abstract findNext: ?options: obj * ?animate: bool -> unit
        abstract findPrevious: ?options: obj * ?animate: bool -> unit
        abstract findAll: needle: U2<string, RegExp> * ?options: obj * ?additive: bool -> float
        abstract undo: unit -> unit
        abstract redo: unit -> unit
        abstract destroy: unit -> unit
        abstract setAutoScrollEditorIntoView: enable: bool -> unit
        abstract completers: ResizeArray<Completer> with get, set

    type [<AllowNullLiteral>] CompleterCallback =
        [<Emit("$0($1...)")>] abstract Invoke: error: obj option * completions: ResizeArray<Completion> -> unit

    type [<AllowNullLiteral>] Completer =
        abstract identifierRegexps: Array<RegExp> option with get, set
        abstract getCompletions: editor: Editor * session: EditSession * position: Point * prefix: string * callback: CompleterCallback -> unit

    type [<StringEnum>] [<RequireQualifiedAccess>] EditSessionOptionsWrapMethod =
        | Code
        | Text
        | Auto

    type [<StringEnum>] [<RequireQualifiedAccess>] EditSessionOptionsFoldStyle =
        | Markbegin
        | Markbeginend
        | Manual

    type [<StringEnum>] [<RequireQualifiedAccess>] EditorOptionsCursorStyle =
        | Ace
        | Slim
        | Smooth
        | Wide

    type [<StringEnum>] [<RequireQualifiedAccess>] EditorOptionsMergeUndoDeltas =
        | Always

    type [<StringEnum>] [<RequireQualifiedAccess>] DeltaAction =
        | Insert
        | Remove

    type [<AllowNullLiteral>] OptionsProviderGetOptions =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    type [<RequireQualifiedAccess>] VirtualRendererPixelToScreenCoordinatesSide =
        | N1 = 1

    type [<StringEnum>] [<RequireQualifiedAccess>] EditorRemove =
        | Left
        | Right