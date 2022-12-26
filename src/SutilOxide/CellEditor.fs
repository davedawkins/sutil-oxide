module SutilOxide.CellEditor

//
// Copyright (c) 2022 David Dawkins
//

open System
open Sutil
open type Feliz.length
//open Fable.Core.JS
open Sutil.Styling
open Fable.Core.Util
//open UI
open Sutil.DOM
open Sutil.Attr

type Getter<'R,'T> = 'R -> 'T
type Setter<'R,'T> = 'R -> 'T -> unit
type Parser<'T> = string -> 'T
type Formatter<'T> = 'T -> string

let console = Fable.Core.JS.console

let inline ignore2 _ _ = ()

type Cell<'R> =
    abstract member Editable : bool
    abstract member AllowedValues : string array
    abstract member GetStringValue : 'R -> string
    abstract member SetStringValue : 'R -> string -> unit
    abstract member Style : (string * obj) seq with get

type ValuesProvider<'T> =
    | NoValues
    | Immediate of 'T seq
    | Indirect of (unit -> 'T seq)

type Config<'R,'T> =
    {
        IsEditable: bool
        Getter    : Getter<'R,'T>
        Setter    : Setter<'R,'T>
        Formatter : Formatter<'T>
        Parser    : Parser<'T>
        Styling   : (string * obj) seq
        Values    : ValuesProvider<'T>
    }
    interface Cell<'R> with
        member this.Editable = this.IsEditable
        member this.GetStringValue(r : 'R) = this.Getter(r) |> this.Formatter
        member this.SetStringValue(r : 'R) (value : string) = this.Setter r (value |> this.Parser)
        member this.Style = this.Styling
        member this.AllowedValues =
            match this.Values with
            | NoValues -> Seq.empty
            | Immediate values -> values
            | Indirect values -> values()
            |> Seq.map this.Formatter |> Seq.toArray

    static member Create( getter : Getter<'R,'T>, ?setter : Setter<'R,'T>, ?parser : Parser<'T>, ?formatter : Formatter<'T>, ?editable : bool  ) =
        {
            IsEditable= editable |> Option.defaultValue true
            Getter    = getter
            Setter    = setter    |> Option.defaultValue ignore2
            Formatter = formatter |> Option.defaultValue (fun x -> x.ToString())
            Parser    = parser    |> Option.defaultValue (fun s -> failwith "Not supported")
            Styling   = []
            Values    = NoValues
        }


type IntCell() =
    static member Create( value : Getter<'R,int>, formatString : Printf.StringFormat<int -> string>, ?setter : Setter<'R,int> ) =
        Config<'R,int>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf formatString,
            parser = Int32.Parse
        )
    static member Create( value : Getter<'R,int>, ?setter : Setter<'R,int> ) =
        Config<'R,int>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf "%d",
            parser = Int32.Parse
        )

type FloatCell() =
    static member Create( value : Getter<'R,float>, formatString : Printf.StringFormat<float -> string>, ?setter : Setter<'R,float> ) =
        Config<'R,float>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf formatString,
            parser = Double.Parse
        )
    static member Create( value : Getter<'R,float>, ?setter : Setter<'R,float> ) =
        Config<'R,float>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf "%f",
            parser = Double.Parse
        )

type StrCell() =
    static member Create<'R>( value : Getter<'R,string>, formatString : Printf.StringFormat<string -> string>, ?setter : Setter<'R,string> ) =
        Config<'R,string>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf formatString,
            parser = id
        )

    static member Create<'R>( value : Getter<'R,string>, ?setter : Setter<'R,string> ) =
        Config<'R,string>.Create(value,
            setter = (setter |> Option.defaultValue ignore2),
            formatter = id,
            parser = id )


type BoolCell() =
    static member Create<'R>( value : Getter<'R,bool>, formatString : Printf.StringFormat<bool -> string>, ?setter : Setter<'R,bool> ) =
        Config<'R,bool>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf formatString,
            parser = Boolean.Parse
        )
    static member Create( value : Getter<'R,bool>, ?setter : Setter<'R,bool> ) =
        Config<'R,bool>.Create(value,
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.defaultValue ignore2),
            formatter =  sprintf "%A",
            parser = Boolean.Parse
        )

let formatDuration (v : TimeSpan) =
    if v.Hours > 0 then
        sprintf "%d:%02d:%02d"  v.Hours v.Minutes v.Seconds
    else
        sprintf "%d:%02d"  v.Minutes v.Seconds

type DurationCell() =

    static member Create( value : Getter<'R,float> ) =
        Config<'R,TimeSpan>.Create(
            (fun r -> TimeSpan( 0, 0, int(value r) ) ),
            editable = false,
            formatter = formatDuration
        )
    static member Create( value : Getter<'R,float>, ?setter : Setter<'R,float> ) =
        Config<'R,TimeSpan>.Create(
            (fun r -> TimeSpan( 0, 0, int(value r) ) ),
            editable = (setter |> Option.isSome),
            setter = (setter |> Option.map (fun setFn ->(fun r (ts : TimeSpan) -> setFn r ts.TotalSeconds)) |> Option.defaultValue ignore2),
            formatter = formatDuration,
            parser = (fun s -> TimeSpan.Parse("00:" + s))
        )

type Options<'R,'T> =
    | IsEditable of bool
    | Formatter of ('T -> string)
    | Parser    of (string -> 'T)
    | Getter    of ('R -> 'T)
    | Setter    of ('R -> 'T -> unit)
    | Styling   of (string * obj) seq
    | Values    of ValuesProvider<'T>

let private update (cfg:Config<'R,'T>) (option : Options<'R,'T>) =
    match option with
    | IsEditable f -> { cfg with IsEditable = f }
    | Formatter  f -> { cfg with Formatter = f }
    | Parser     f -> { cfg with Parser = f }
    | Getter     f -> { cfg with Getter = f }
    | Setter     f -> { cfg with Setter = f }
    | Styling    f -> { cfg with Styling = f }
    | Values     f -> { cfg with Values = f }

let withOptions (options : Options<'R,'T> list) (cfg : Config<'R,'T>) =
    options |> List.fold update cfg


module private Autocomplete =
    type FilteredItem =
        {
            Index : int
            Value : string
        }

    type Model =
        {
            SelectedIndex : int
            Values : string list
            FilteredValues : FilteredItem list
            Showing : bool
            Value : string
        }

    let mValues m = m.Values
    let mFilteredValues m = m.FilteredValues
    let mSelectedIndex m = m.SelectedIndex
    let mShowing m = m.Showing

    type Message =
        |   SetSelectedIndex of int
        |   IncSelectedIndex of int
        |   SetShowing of bool
        |   SetValue of string
        |   Commit

    let filtered (value : string) (values : string list) : FilteredItem list=
        if value = "" then
            values
        else
            values |> List.filter (fun v -> v.ToLower().Contains(value.ToLower()))
        |> List.mapi (fun i v -> { Index = i; Value = v })

    let init (values:string array) : Model * Cmd<Message> =
        {
            SelectedIndex = -1
            Value = ""
            Values = values |> List.ofArray
            FilteredValues = values |> List.ofArray |> filtered ""
            Showing = false
        }, Cmd.none

    let update (value : IStore<string>) msg (model : Model) =
        //console.log($"ac: update: {msg}")
        match msg with

        | Commit ->
            if model.Showing then
                let (selectedValue, selectedIndex) =
                    model.FilteredValues
                    |> List.tryFind (fun v -> v.Index = model.SelectedIndex)
                    |> Option.map (fun fi -> fi.Value, fi.Index)
                    |> Option.defaultValue(model.Value,-1)

                // Need to make this effect before the triggering event (Return) bubbles upwards and results
                // in an OnBlur occurs
                Store.set value selectedValue

                { model with Value = selectedValue; SelectedIndex = selectedIndex}, Cmd.ofMsg (SetShowing false)
            else
                model, Cmd.none

        | SetValue v ->
            let fv = filtered v (model.Values)
            { model with Value = v; FilteredValues = fv; SelectedIndex = 0; Showing = model.Showing && not fv.IsEmpty }, Cmd.none

        | SetShowing f ->
            { model with Showing = f && not model.FilteredValues.IsEmpty; SelectedIndex = 0 }, Cmd.none

        | SetSelectedIndex i ->
            { model with SelectedIndex = i }, Cmd.none

        | IncSelectedIndex i ->
            if model.Showing then
                let newIndex = Math.Max(0, Math.Min(model.SelectedIndex + i, model.FilteredValues.Length-1))
                { model with SelectedIndex = newIndex }, Cmd.none
            else
                model, Cmd.ofMsg (SetShowing true)

    let public view (value : IStore<string>) (values : string array) =
        let model, dispatch = values |> Store.makeElmish init (update value) ignore

        //let dispatch m = console.log($"dispatch {m}"); dispatch' m
        let show f = f |> SetShowing |> dispatch
        let incIndex i =
            i |> IncSelectedIndex |> dispatch
        let watchValue = value |> Observable.distinctUntilChanged |> Store.subscribe (dispatch << SetValue)

        [
            //unsubscribeOnUnmount [ (fun _ -> console.log("ummount")) ]
            disposeOnUnmount [ model; watchValue ]

            Bind.el( model .> mShowing |> Observable.distinctUntilChanged, fun show ->
                if not show then
                    fragment []
                else
                    Html.div [
                        Attr.className "autocomplete"

                        Bind.each(
                            model .> mFilteredValues,
                            (fun item ->
                                Html.div [
                                    Bind.toggleClass( (model .> mSelectedIndex .> ((=) item.Index)), "selected")
                                    text item.Value
                                ]),
                            (fun item -> item.Value)
                        )
                    ]
            )

            Sutil.Attr.on "focusout" (fun e -> show false) []

            Ev.onKeyDown (fun e ->
                match (e.key) with
                | "Escape" ->
                    show false
                    e.preventDefault()
                | "ArrowDown" ->
                    incIndex +1
                    e.preventDefault()
                | "ArrowUp" ->
                    incIndex -1
                    e.preventDefault()
                | "Return" | "Enter" ->
                    dispatch Commit
                    //e.preventDefault()
                | _ ->
                    ()
            )
        ]

module Elmish =
    type Model<'R> = {
        Record : 'R;
        Editing : bool
        Selected : bool
    }

    type Message<'R> =
        | StartEdit of 'R
        | FinishEdit of 'R
        | ToggleSelect of 'R
        | SetSelected of 'R * bool

    let init (record : 'R) =
        { Record = record; Editing = false; Selected = false }, Cmd.none

    let update msg model : (Model<'R> * Cmd<Message<'R>>)=
        //console.log($"edit:update: {msg}")
        match msg with
        | FinishEdit r -> { model with Editing = false }, Cmd.none
        | StartEdit r -> { model with Editing = true }, Cmd.none
        | ToggleSelect r -> model, (r, not model.Selected) |> SetSelected |> Cmd.ofMsg
        | SetSelected (r,b) -> { model with Selected = b }, Cmd.none

let cellStyle = [
    rule ".cell" [
        Css.positionRelative
    ]

    rule ".autocomplete" [
        Css.positionAbsolute
        Css.backgroundColor "white"
        Css.borderColor "gray"
        Css.borderStyleSolid
        Css.borderWidth (px 1)
        Css.zIndex 10
        Css.width (percent 100)
    ]

    rule ".cell input" [
        Css.width (percent 100)
        Css.borderStyleNone
        Css.backgroundColor "inherit"
    ]

    rule ".autocomplete .selected" [
        Css.backgroundColor "black"
        Css.color "white"
    ]
]

let findBest (allowed : string array) (value : string) =
    allowed
    |> Array.tryFind (fun s -> s.ToLower() = value.ToLower())
    |> Option.defaultWith (fun () ->
        allowed
        |> Array.tryFind (fun s -> not (String.IsNullOrWhiteSpace(value)) &&  s.ToLower().Contains(value.ToLower()))
        |> Option.defaultValue value
    )

let bindFocus (isFocused : IObservable<bool>) : SutilElement =
    nodeFactory <| fun ctx ->
        let inputEl = ctx.Parent.AsDomNode :?> Browser.Types.HTMLInputElement

        let un = isFocused.Subscribe( fun f ->
            if f then
                DOM.rafu (fun _ ->
                    inputEl.focus()
                    inputEl.setSelectionRange(99999,99999)
                    )
        )

        SutilNode.RegisterDisposable(ctx.Parent,un)
        unitResult(ctx, "autofocus")

open Fable.Core.JsInterop

let view<'R> (record : 'R) (wantsFocus : bool) (cell : Cell<'R>) =
    Html.div [
        Attr.style cell.Style
        Attr.className "cell"

        if cell.Editable then

            // Observable "text" node
            let valueStore = Store.make (cell.GetStringValue(record))
            disposeOnUnmount [ valueStore ]

            Html.input [
                Bind.attr( "value", valueStore )
                if wantsFocus then autofocus
                Ev.onBlur (fun e ->
                    valueStore |> Store.modify (findBest (cell.AllowedValues))

                    let text = valueStore |> Store.get
                    // Commit current value if changed
                    if (text <> cell.GetStringValue(record)) then
                        cell.SetStringValue record text
                )
            ]

            // Autocomplete for allowed values
            let allowedValues = cell.AllowedValues
            if allowedValues.Length > 0 then
               yield! Autocomplete.view valueStore allowedValues

        else
           cell.GetStringValue(record) |> text

    ] |> withStyle cellStyle

let editStyle = [
    rule ".editing" [
        Css.backgroundColor "#fae3cd"
        // Css.borderColor "orange"
        // Css.borderStyleSolid
        // Css.borderWidth (Feliz.length.px 1)
        // Css.borderRadius (Feliz.length.px 4)
    ]
    rule ".editing>div:focus" [
        Css.backgroundColor "white"
    ]
    rule ".edit-row" [
        Css.padding (Feliz.length.rem 0.2)
    ]

    rule ".selected .row-select" [
        Css.backgroundColor "gray"
    ]
]

open Fable.Core
open Fable.Core.JsInterop
open Microsoft.FSharp.Core
[<Emit("$1 === $2")>]
let inline jsEq a b : bool = jsNative;

let containsNode (parent : Browser.Types.Node) (child : Browser.Types.Node option) =
    if isNullOrUndefined parent then
        false
    else
        match child with
        | None -> false
        | Some x -> parent.contains(x)

[<Emit("$0.relatedTarget")>]
let relatedTargetOf( e : Browser.Types.Event ) : Browser.Types.Node option = jsNative


let onFocusEnter f =
    Sutil.Attr.on "focusin" (fun e -> f()) []

let onFocusLeave f =
    Sutil.Attr.on "focusout" (fun e ->
        let currentTarget = e.currentTarget :?> Browser.Types.Node

        // if relatedtarget is a child then we are stepping between fields
        // in the same record, so maintain edit status
        if not (containsNode currentTarget (relatedTargetOf e)) then
            f()
    ) []


type EditController<'R,'K>( record : 'R, key : 'R -> 'K, cells : Cell<'R> seq ) =
    let mutable model : IStore<Elmish.Model<'R>> = Unchecked.defaultof<_>
    let mutable dispatch : Dispatch<Elmish.Message<'R>> = Unchecked.defaultof<_>

    let view parentDispatch =
        let model', dispatch' =
            record
            |> Store.makeElmish
                Elmish.init
                (fun msg model ->
                    let result = Elmish.update msg model
                    parentDispatch msg
                    result)
                ignore

        model  <- model'
        dispatch <- dispatch'

        Html.div [
            Attr.className "edit-row"
            DOM.disposeOnUnmount [ model ]

            onFocusEnter (fun _ -> dispatch (Elmish.StartEdit record))
            onFocusLeave (fun _ -> dispatch (Elmish.FinishEdit record))

            Bind.toggleClass( model |> Store.map (fun m -> m.Editing), "editing")
            Bind.toggleClass( model |> Store.map (fun m -> m.Selected), "selected")

            Html.div [
                Attr.className "row-select"
                Ev.onClick (fun _ -> dispatch (Elmish.ToggleSelect record))
            ]
            yield! (cells |> Seq.map (view record false))
        ] |> withStyle editStyle

    member this.Selected
        with get() =
            model |> Store.getMap (fun m -> m.Selected)
        and set(value:bool) =
            if this.Selected then
                dispatch (Elmish.SetSelected (record, value))
    member _.View(dispatch)= view dispatch
    member _.Record = record
    member _.Key = record |> key

let edit<'R,'K> (cells : Cell<'R> seq) (key : 'R -> 'K) (record:'R) =
    EditController( record, key, cells )
