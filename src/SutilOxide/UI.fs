module SutilOxide.UI

open Sutil

module Helpers =
    open Fable.Core

    [<Emit("requestAnimationFrame($0)")>]
    let rafu (f : unit -> unit ) = jsNative

    let cursorEnd (input: Browser.Types.HTMLInputElement) =
        rafu (fun _ -> 
            let v = input.value //store the value of the element
            input.setSelectionRange(v.Length, v.Length))

module Common = 

    type Orientation = Horizontal | Vertical

    [<RequireQualifiedAccess>]
    type Value<'T> =   
        | Const of 'T
        | Getter of (unit -> 'T)
        | Observable of (System.IObservable<'T> * 'T)
//        | Store of Sutil.IStore<'T>

    type Direction = Vertical | Horizontal

    type Icon = 
        | FaIcon of Value<string>
        | UrlIcon of string

    type Text =
        | PlainText of Value<string>

    type Shortcut = 
        Shortcut of string

    type DockLocation =
        | TopLeft | TopRight
        | BottomLeft | BottomRight
        | LeftTop | LeftBottom
        | RightTop | RightBottom
        | CenterLeft | CenterRight


    /// Pass the component's label as-is, it will be converted to a standard format. For example, "Sign Out" will
    /// produce the attribute "data-cmd='sign-out'"
    let attrDataCmd (cmd : string ) = Attr.custom( "data-cmd", (cmd.ToLower().Replace(" ", "-").Replace(".", "")) )

module Icon =
    let private _makeFa (prefix : string) (name : string) =
        if name.StartsWith "fa " || name.StartsWith "fa-sharp " || name.StartsWith "fa-brands " || name.StartsWith "icon-" then 
            name
        else 
            let faName = if name.StartsWith "fa-" then name else "fa-" + name
            prefix + " " + faName
            
    let makeFaBrand (name : string) = 
        _makeFa "fa-brands" name

    let makeFaClassic (name : string) = 
        _makeFa "fa" name

    let makeFaSharp (name : string) = 
        _makeFa "fa-sharp" name

    let makeFaSharpLight (name : string) = 
//        makeFaSharp name
        _makeFa "fa-sharp fa-light" name

    // let makeFaSharpSolid (name : string) = 
    //     _makeFa "fa-sharp fa-solid" name

    let makeFa (name : string) =
        makeFaSharpLight name
//        _makeFa "fa" name


module Control =
    open Common
    open Sutil
    open Sutil.CoreElements
    type Popup = interface end

    type OnClick = (unit -> unit)
    type OnCheck = (bool -> unit)
    type OnSelect = (obj -> unit)

    let private idGenerator( prefix : string ) =
        let mutable n = 0
        fun () ->
            n <- n + 1
            sprintf "%s%d" prefix n

    let private keyGen = idGenerator( "key" )

    type ControlType =
        | ControlLabel
        | ControlButton of OnClick
        | ControlCheck of OnCheck * Value<bool>
        | ControlMenu of (unit -> Control list)
        | ControlSelect of (OnSelect * Value<obj> * (unit -> (string * obj) list))
        | ControlCustom of Core.SutilElement

    and Control = {
        Key : string
        Text : Text option
        Icon : Icon option
        Shortcut : Shortcut option
        Type : ControlType
    }
    with 
        static member Create() = 
            {
                Key = keyGen()
                Text = None
                Icon = None
                Shortcut = None
                Type = ControlLabel
            }

    type ControlOption =
        | Key of string
        | Text of Text
        | Icon of Icon
        | Shortcut of Shortcut
        //| Type of RibbonControlType

    let custom( e : Core.SutilElement ) : Control = { Control.Create() with Type = ControlCustom e }

    let makeControl (options : ControlOption list) =
        let withOption (c : Control) (option : ControlOption) : Control =
            match option with
            | Key l -> { c with Key = l }
            | Text l -> { c with Text = Some l }
            | Icon l -> { c with Icon = Some l }
            | Shortcut l -> { c with Shortcut = Some l }
            //| Type l -> { c with Type = l }
        options |> List.fold withOption (Control.Create())

    let makeLabel options = options |> makeControl

    let makeSeparator() =
        //makeLabel [ ControlOption.Text (PlainText (Value.Const "|")) ]
        makeLabel [ 
            ControlOption.Icon (FaIcon (Value.Const "pipe"))
        ]

    let makeButton cb options = 
        { makeControl options with Type = ControlButton cb }
        
    let makeCheck onCheck value options = 
        { makeControl options with Type = ControlCheck (onCheck,value) }
        
    let makeMenu items options = 
        { makeControl options with Type = ControlMenu items }

    let makeSelect<'t> (onSelect : 't -> unit) (value : Value<'t>) (items : unit -> (string * 't) list) options =
        { makeControl options with Type = ControlSelect (unbox onSelect, unbox value, unbox items) }

    let internal renderSeparator() = Html.divc "ui-vsep" []

    let rec internal renderControl (item : Control) =
        match item.Type with
        | ControlCustom el -> el
        | _ -> renderBuiltIn item
    
    and renderBuiltIn(item : Control) =
        Html.divc "ui-control" [
            Attr.tabIndex 0

            yield!
                match item.Type with
                | ControlCheck (onCheck, value) ->
                    let checkedS = 
                        match value with
                        | Value.Const z -> Store.make z
                        | Value.Getter g -> Store.make (g())
                        | Value.Observable (o,init) ->
                            let s = Store.make init
                            let d = o.Subscribe( Store.set s )
                            s
                        // | Store s -> s 
                    [
                        Attr.roleCheckbox
                        Bind.attr( "aria-checked", checkedS .>> string )
                        disposeOnUnmount [ checkedS ]
                        Bind.el( checkedS,
                            fun ch -> Html.i [ Attr.className ("left " + (Icon.makeFa("check")) + " " + (if ch then "checked" else "")) ]
                        )
                        Ev.onClick (fun e ->
                            e.preventDefault()
                            checkedS |> Store.modify (not)
                            onCheck (checkedS.Value)
                        )
                    ]
                | _ -> []


            match item.Icon with
            | Some (FaIcon (Value.Const fa)) -> Html.ic ("left " + Icon.makeFa fa) []
            | Some (FaIcon (Value.Getter fa)) -> Html.ic ("left " + Icon.makeFa (fa())) []
            | Some (FaIcon (Value.Observable (fas,init))) -> 
                Html.i [
                    Bind.className( fas .> Icon.makeFa .> ((+)"left ") )
                ]
            | _ -> Sutil.CoreElements.nothing

            match item.Text with
            | Some (PlainText textValue) -> 
                match textValue with
                | Value.Const s -> text s
                | Value.Getter g -> text (g())
                | Value.Observable (o,_) ->
                    Bind.el( o, text )
            | _ -> Sutil.CoreElements.nothing

            yield!
                match item.Type with
                | ControlCustom _ -> [ ]

                | ControlButton onClick -> 
                    [
                        Attr.roleButton
                        attrDataCmd (item.Key)
                        Ev.onClick (fun _ -> onClick()) 
                    ]

                | ControlMenu items ->
                    let itemStore : IStore<Control list> = Store.make []
                    [ 
                        Attr.roleMenu
                        attrDataCmd (item.Key)
                        disposeOnUnmount [ itemStore ]
                        Html.ic ("right " + (Icon.makeFa("angle-down"))) []
                        Ev.onClick (fun _ -> items() |> Store.set itemStore)
                        Bind.el( itemStore, fun items ->
                            Html.divc "ui-menu-stack scroll-shadows" (items |> List.map renderControl)
                        )
                    ]

                | ControlSelect (onSelect, value, items) ->
                    
                    let _eq = JsHelpers.fastEquals

                    let itemMap : IStore<(string*obj) list> = Store.make (items())

                    let findLabel (v : obj) =
                        itemMap.Value 
                        |> List.tryFind (snd>>_eq v) 
                        |> Option.map fst
                        |> Option.defaultValue ""

                    let current = 
                        match value with
                        | Value.Const z -> Store.make z
                        | Value.Getter g -> Store.make (g())
                        | Value.Observable (o,init) ->
                            let s = Store.make init
                            let d = o.Subscribe( Store.set s )
                            s

                    let makeSelectItem (label, value) =

                        let handler () =
                            Store.set current value
                            onSelect value

                        //let checkValue = Value.Observable (current .> _eq value, _eq value current.Value )

                        [
                            Text (PlainText (Value.Const label))
                        ] |> makeButton handler

                    let itemStore : IStore<Control list> = Store.make []

                    [ 
                        Attr.roleListBox
                        Html.divc "ui-select" [
                            Attr.tabIndex 0
                            attrDataCmd (item.Key)
                            disposeOnUnmount [ itemStore ]
                            
                            Html.divc "ui-select-value" [
                                Html.divc "option-list" [
                                    Bind.each( 
                                        itemMap, 
                                        (fun (label, value) -> 
                                            Html.divc "option" [ 
                                                Bind.toggleClass( current .> _eq value, "selected" )
                                                text label 
                                            ]), 
                                        fst )
                                ]
                                // Bind.el( 
                                //     current .> findLabel,
                                //     fun label -> Html.span [ text label ]
                                // )
                                Html.ic ("right " + (Icon.makeFa("angle-down"))) []
                            ]
                            Ev.onClick (fun _ -> 
                                let _items = items() 
                                _items |> Store.set itemMap
                                _items |> List.map makeSelectItem |> Store.set itemStore
                            )
                            Bind.el( itemStore, fun items ->
                                Html.divc "ui-menu-stack scroll-shadows" (items |> List.map renderControl)
                            )
                        ]
                    ]

                | _ -> []
        ]

module Toolbar =

    type ToolbarOption =
        | ToolbarOrientation of Common.Orientation

    let render (options : ToolbarOption seq) (controls : Control.Control seq) =
        Html.divc "ui-toolbar" (controls |> Seq.map Control.renderControl)

module Ribbon =
    open Common
    open Control
    open Sutil.Core
    open Sutil
    open Sutil.CoreElements
        
    // type Stack = 
    //     Stack of Control list

    type Group = {
        Key : string
        Label : string
        Controls : Control list
    }

    type Ribbon = {
        Key : string
        Label : string
        Groups : Group list
    }

    let private renderStack ( chunk : Control list ) =
        Html.divc "ui-stack" 
            (chunk |> List.map Control.renderControl)

    let private renderGroup (group : Group) =
        Html.divc "ui-group" [
            Html.divc "ui-stacks"
                (group.Controls |> List.chunkBySize 3 |> List.map renderStack)
            Html.divc "ui-group-label" [ text group.Label ]
        ]

    let mutable cssInstalled = false

    let private installCss() =
        if not cssInstalled then
            cssInstalled <- true
            Sutil.Styling.addGlobalStyleSheet (Browser.Dom.document) UICss.style |> ignore
        
    let render (ribbon : Ribbon) = 
        installCss()

        Html.divc "ui-ribbon" 
            (ribbon.Groups 
                    |> List.collect (fun g ->
                        [
                            renderGroup g
                            renderSeparator()
                        ])
            )

module RibbonMenu = 
    open Sutil
    open Sutil.Core

    open Ribbon

    type RibbonMenu = {
        Ribbons : Ribbon list
    }
        with static member Create() = { Ribbons = [] }


    let private renderMenuLabel (selected : IStore<string>) (p : Ribbon) =
        Html.divc "ui-rm-label" [ 
            Bind.toggleClass( selected .> (fun s -> s = p.Key), "selected" )
            text p.Label 
            Ev.onClick (fun e -> p.Key |> Store.set selected)
        ]

    let mutable cssInstalled = false

    let private installCss() =
        if not cssInstalled then
            cssInstalled <- true
            Sutil.Styling.addGlobalStyleSheet (Browser.Dom.document) UICss.styleRibbonMenu |> ignore
        
    let render( ps : RibbonMenu ) =
        installCss()

        let currentRibbon = Store.make (ps.Ribbons |> List.tryHead |> Option.map _.Key |> Option.defaultValue "")

        Html.divc "ui-ribbonmenu" [
            Html.divc "ui-rm-menu" [
                yield! ps.Ribbons |> List.map (renderMenuLabel currentRibbon)
            ]
            Html.divc "ui-rm-ribboncontainer" [
                yield! ps.Ribbons |> List.map (fun ps -> Ribbon.render ps |> Bind.visibility (currentRibbon |> Store.map (fun cp -> cp = ps.Key)))
            ]
        ]       

module Input =
    open Common
    open Browser.Types

    type Option<'T> =
    | Key of string
    | Class of string
    | Orientation of Orientation
    | Placeholder of string
    | Caption of string
    | OnChange of ('T -> unit)
    | OnEnter of (unit -> unit)
    | Value of Value<'T>
    | Attrs of Core.SutilElement list
    | MinValue of float
    | MaxValue of float
    | StepValue of float

    type Options<'T> = {
        Key : string
        Class : string
        Orientation : Orientation
        Caption : string
        Placeholder : string
        OnChange : 'T -> unit
        OnEnter : unit -> unit
        Value : Value<'T>
        Attrs : Core.SutilElement list
        MinValue : float
        MaxValue : float
        StepValue : float
    }
    with
        static member Create() : Options<'T> =
            {
                Key = ""
                Class = ""
                Orientation = Common.Vertical
                Placeholder = ""
                Caption = ""
                OnChange = ignore
                Value = Value.Const (Unchecked.defaultof<_>)
                OnEnter = ignore
                Attrs = []
                MinValue = 0.0
                MaxValue = 1000.0
                StepValue = 1.0                
            }
        member __.With (p : Option<'T>) =
            match p with
            | Key s -> { __ with Key = s }
            | Class s -> { __ with Class = s }
            | Orientation s -> { __ with Orientation = s }
            | Caption s -> { __ with Caption = s }
            | Placeholder s -> { __ with Placeholder = s }
            | OnChange s -> { __ with OnChange = s }
            | OnEnter s -> { __ with OnEnter = s }
            | Value s -> { __ with Value = s }
            | Attrs s -> { __ with Attrs = s }
            | MinValue s -> { __ with MinValue = s }
            | MaxValue s -> { __ with MaxValue = s }
            | StepValue s -> { __ with StepValue = s }

        static member From (p : Option<'T> seq) : Options<'T> =
            p |> Seq.fold (fun b x -> b.With(x)) (Options<'T>.Create())

    let mutable private cssInstalled = false

    let private installCss() =
        if not cssInstalled then
            cssInstalled <- true
            Sutil.Styling.addGlobalStyleSheet (Browser.Dom.document) UICss.styleInput |> ignore
        
    let private inputCommon<'T> attrs (options : Option<'T> list) (parse: string -> 'T) (format : 'T -> string) =
        installCss()
        
        let cfg = Options<'T>.From options
        Html.divc (sprintf "ui-input %s %s" (cfg.Class) (cfg.Orientation.ToString().ToLower())) [
            Html.label [ text cfg.Caption; Attr.for' (cfg.Key) ]

            Html.input [
                yield! attrs

                Attr.name (cfg.Key)
                Attr.placeholder (cfg.Placeholder)

                yield!
                    match cfg.Value with

                    | Value.Const s ->
                        [ 
                            Attr.value s 
                        ]

                    | Value.Observable (o,i) ->
                        [ Bind.attr( "value", o, ignore) ]

                    | _ -> 
                        [ ]

                Ev.onKeyUp (fun e ->
                    if e.key = "Return" || e.key = "Enter" then cfg.OnEnter()                    
                )

                Ev.onBlur (fun e -> 
                    let input = (e.target :?> Browser.Types.HTMLInputElement)
                    let text = input.value
                    cfg.OnChange (parse text)
                )

                yield! cfg.Attrs
            ]
        ]    

    let inputString (options : Option<string> list) =
        inputCommon<string> [] options id id

    let inputInt32 (options : Option<int> list) =
        let cfg = Options<int>.From options
        inputCommon<int>
            [
                Attr.typeNumber
                Attr.min (int (cfg.MinValue))
                Attr.max (int (cfg.MaxValue))
                Attr.step (int (cfg.StepValue))
            ] options (fun s -> s |> System.Int32.TryParse |> (fun (succ,b) -> if succ then b else 0)) (sprintf "%d")

    let inputFloat32 (options : Option<float> list) =
        let cfg = Options<float>.From options
        inputCommon<float>
            [
                Attr.typeNumber
                Attr.min (cfg.MinValue)
                Attr.max (cfg.MaxValue)
                Attr.step (cfg.StepValue)
            ] options (fun s -> s |> System.Single.TryParse |> (fun (succ,b) -> if succ then float b else 0.0)) (sprintf "%f")

    let inputMultiString (options : Option<string> list) =
        installCss()
        
        let cfg = Options<string>.From options
        Html.divc (sprintf "ui-input %s %s" (cfg.Class) (cfg.Orientation.ToString().ToLower())) [
            Html.label [ text cfg.Caption; Attr.for' (cfg.Key) ]
            Html.textarea [
                Attr.name (cfg.Key)

                yield!
                    match cfg.Value with

                    | Value.Const s ->
                        [ 
                            Attr.value s 
                        ]

                    | Value.Observable (o,i) ->
                        [ Bind.attr( "value", o, ignore) ]

                    | _ -> 
                        [ ]

                Ev.onBlur (fun e -> 
                    let input = (e.target :?> Browser.Types.HTMLTextAreaElement)
                    let text = input.value
                    cfg.OnChange text
                )

                Ev.onKeyUp (fun e ->
                    if e.key = "Return" || e.key = "Enter" then cfg.OnEnter()                    
                )

                yield! cfg.Attrs
            ]
        ]

module Controller =
    open Sutil.Core

    type IRibbonController =
        abstract CreateRibbon: ribbonKey:string * label:string * groups:List<string * string * Control.Control list> -> unit
        abstract CreateGroup: ribbonKey:string * key:string * label:string * controls : Control.Control list -> unit

    type PaneOption =
        | Location of Common.DockLocation
        | TabText of Common.Text 
        | TabIcon of Common.Icon
        | TabElement of SutilElement
        | HeaderText of Common.Text
        | HeaderElement of SutilElement
        | ControlElement of SutilElement
        | IsShowing of bool

    type IDockController =
        abstract CreatePane: paneKey:string * PaneOption list -> unit
        abstract ShowPane: paneKey:string -> unit

module Forms =

    type FieldElement = FieldElement of (unit -> Core.SutilElement)
        with 
            static member Of( f : unit -> Core.SutilElement ) = FieldElement f
            member __.Render() =
                let (FieldElement f) = __
                f()

    let shortName (tname : string) =
        let p = tname.LastIndexOf('.')
        if p < 0 then tname else tname.Substring(p+1)

    let isEnum( t : System.Type ) =
        try
            Reflection.FSharpType.IsUnion(t) && (Reflection.FSharpType.GetUnionCases(t) |> Array.exists (fun cs -> cs.GetFields().Length > 0) |> not)
        with x -> Log.log("Error: " + x.Message + ": " + t.FullName); false

    let parseDouble ( s : string ) : Result<double, string> =
        try
            System.Double.Parse s |> Ok
        with
        | x -> Error (x.Message) 

    let parseInt ( s : string ) : Result<int, string> =
        try
            System.Int32.Parse s |> Ok
        with
        | x -> Error (x.Message)
        
    type Parser<'T> = string -> Result<'T,string>

    [<RequireQualifiedAccess>]
    type BuiltInEditor = 
        | Checkbox
        | Text
        | Number
        | MultiLineText
        | Select
        | Input

    [<RequireQualifiedAccess>]
    type FieldEditor<'T> = 
        | BuiltIn of BuiltInEditor
        | Ctor of ((Field<'T> * IStore<string>) -> Core.SutilElement)

    and Field<'T> = 
        {
            Editor : FieldEditor<'T>
            Label: string
            Parse: Parser<'T>
            Format: 'T -> string
            Set: ('T -> unit) option
            Get: unit -> 'T
            AllowedValues: (unit -> string[]) option
            Step : float
            SystemTypeName : string
        }

        static member Empty<'T>() = 
                {   Label = ""
                    Editor = FieldEditor.BuiltIn BuiltInEditor.Text
                    SystemTypeName = typedefof<System.String>.FullName
                    Parse = fun value -> Result<'T,string>.Error("No parser for '" + value + "'")
                    Format = fun (v : 'T) -> sprintf "%A" v
                    AllowedValues = None
                    Set = None
                    Step = 1.0
                    Get = fun () -> Unchecked.defaultof<'T> } : Field<'T>

        /// Map system types to input type
        static member Init<'T>( t : System.Type, sysTypeName : string ) : Field<'T> = 

            let empty = { Field.Empty<'T>() with  SystemTypeName = sysTypeName }

            match (shortName(empty.SystemTypeName)) with
            | "String" -> 
                    empty.WithParse( fun s -> Ok ( (s :> obj :?> 'T)  ))

            | "Double" | "Float64" | "Float32" | "Float" -> 
                empty
                    .WithBuiltIn(BuiltInEditor.Number)
                    .WithParse( parseDouble :> obj :?> Parser<'T> )

            | "Int32" -> 
                empty
                    .WithBuiltIn(BuiltInEditor.Number)
                    .WithParse( parseInt :> obj :?> Parser<'T> )

            | "Boolean" -> empty.WithBuiltIn(BuiltInEditor.Checkbox)

            | _ ->
                if (isEnum(t)) then
                    let cases = Reflection.FSharpType.GetUnionCases(t) |> Array.filter (fun c -> c.GetFields().Length = 0)
                    { empty with
                        Editor = FieldEditor.BuiltIn BuiltInEditor.Select
                        AllowedValues = (fun () -> cases |> Array.map (fun c -> c.Name) |> Array.sort) |> Some
                        Parse =
                            fun s -> 
                                match cases |> Array.tryFind (fun c -> c.Name = s) with
                                | Some c -> Reflection.FSharpValue.MakeUnion(c, Array.empty) :?> 'T |> Ok
                                | None -> Error ("Case not found for type: "  + s + " is not in " + t.FullName)
                    }
                else
                    { empty with Format = sprintf "%A" }
                    //failwith ("Unsupported input for type " + t.FullName )

        static member inline Create() : Field<'T> = 
            let t = typedefof<'T> // For erased union , this will be underlying primitive type, like string
            let fullname = typedefof<'T>.FullName // This will be the name of the erased union
            Field<'T>.Init(t, fullname)

        // static member FromJs ( js : obj ) =
        //     let mapJs f

        member private __.WithEditor( e : FieldEditor<'T> ) : Field<'T> = { __ with Editor = e }
        member private __.WithBuiltIn( t : BuiltInEditor ) : Field<'T> = { __ with Editor = FieldEditor.BuiltIn t }
        member __.WithLabel( s : string ) : Field<'T> = { __ with Label = s }
        member __.WithParse( p : Parser<'T> ) : Field<'T> = { __ with Parse = p }
        member __.WithFormat( f : 'T -> string ) : Field<'T> = { __ with Format = f }
        member __.WithSet( s : 'T -> unit ) : Field<'T> = { __ with Set = Some s }
        member __.WithGet( g : unit -> 'T ) : Field<'T> = { __ with Get = g }

        member __.WithAllowedValues( vals : unit -> string[] ) : Field<'T> = 
            { __ with AllowedValues = Some vals}

        member __.WithAllowedValues( vals : string seq ) : Field<'T> = 
            let _vals = vals |> Seq.toArray
            { __ with AllowedValues = Some (fun () -> _vals)}

    open Sutil.Styling
    open type Feliz.length

    let private style = [
        rule ".ui-field" [
            Css.displayFlex
            Css.flexDirectionRow
            Css.flexBasis (percent 100)
            Css.paddingRight (px 10)
        ]

        rule ".ui-field>div" [
            Css.displayFlex
            Css.flexDirectionColumn
            Css.width (percent 100)
        ]

        rule ".ui-field label" [
            Css.flexBasis (px 115)
            Css.fontWeightBold
        ]

        rule ".ui-field input" [
            Css.borderStyleNone
            Css.borderBottom (px 1, Feliz.borderStyle.solid, "transparent")
            Css.width (percent 100)
        ]

        rule ".ui-field input[type='checkbox']" [
            Css.custom("width", "fit-content")
        ]

        rule ".ui-field input:focus" [
            Css.outlineStyleNone
            Css.borderColor "#eeeeee"
        ]

        rule ".ui-field select" [
            Css.borderStyleNone
            Css.borderBottom (px 1, Feliz.borderStyle.solid, "transparent")
            Css.width (percent 100)
            Css.backgroundColor "white"
        ]

        rule ".ui-field select *" [
            Css.backgroundColor "white"
        ]

        rule ".ui-field select:focus" [
            Css.outlineStyleNone
//            Css.borderColor "#eeeeee"
        ]

        rule ".ui-field .error" [
            Css.fontSize (percent 80)
            Css.color "red"
        ]

        // Maybe try this height transition at some point
        // https://codepen.io/chriscoyier/pen/qBXoEMV

        rule ".ui-field .error:empty" [
            Css.displayNone
        ]
    ]

    let mutable private cssInstalled = false

    let internal installCss() =
        if not cssInstalled then
            cssInstalled <- true
            Sutil.Styling.addGlobalStyleSheet (Browser.Dom.document) style |> ignore

    let viewFields (fields : FieldElement seq) =
        installCss()
        fields |> Seq.map _.Render() |> Html.div


    let internal withLabelError (label: string) (editor: IStore<string> -> Core.SutilElement) =
        let error = Store.make ""

        Html.divc "ui-field" [
            CoreElements.disposeOnUnmount [ error ]
            Html.label [
                text label
            ]
            Html.div [
                editor error
                Bind.el( error, fun e -> Html.spanc "error" [ text e ] )
            ]
        ]    

    let internal editFieldSelect (f : Field<'t>) (error : IStore<string>) =
        let (getter: unit -> 't) = f.Get
        let (setter: ('t -> unit) option) = f.Set
        let (format: 't -> string) = f.Format
        let (parse: string -> Result<'t,string>) = f.Parse
        let allowed : string[] option = f.AllowedValues |> Option.map (fun g -> g())

        let formatted() = getter() |> format

        let validateSelect (e : Browser.Types.Event) =
            let input = (e.target :?> Browser.Types.HTMLSelectElement)

            let result = input.value |> parse 

            match result with Error s -> s | _ -> "" 
                |> Store.set error

            result

        Html.select [
            Attr.value (formatted())
            Ev.onMount (fun e ->
                let selE = (e.target :?> Browser.Types.HTMLSelectElement)
                selE.selectedIndex <- allowed |> Option.bind ( Array.tryFindIndex ((=) (formatted()))) |> Option.defaultValue -1
            )
            yield! 
                allowed 
                |> Option.map (Array.map (fun v -> Html.option [ Attr.value v; text v ]))
                |> Option.defaultValue (Array.empty)

            match setter with
            | Some f ->
                Ev.onChange( fun e -> validateSelect e |> Result.iter f )
            | None ->
                Attr.readOnly true
        ]


    let internal editFieldCheckbox (field : Field<bool>) (error : IStore<string>) =

        Html.input [    
            Attr.typeCheckbox
            Attr.isChecked (field.Get())   

            match field.Set with 
            | Some f ->
                Ev.onCheckedChange f
            | None ->
                Attr.readOnly true
        ]

    open Fable.Core
    [<Emit("document.activeElement === $0")>]
    let hasFocus( el : Browser.Types.EventTarget ) : bool = jsNative

    let internal editFieldInput (builtIn : BuiltInEditor) (f : Field<'t>) (error : IStore<string>) =
        let (typ: string) = builtIn |> string |> _.ToLower()
        let (getter: unit -> 't) = f.Get
        let (setter: ('t -> unit) option) = f.Set
        let (format: 't -> string) = f.Format
        let (parse: string -> Result<'t,string>) = f.Parse

        let timeout = SutilOxide.JsHelpers.createTimeout()

        let formatted() = getter() |> format

        let validate (e : Browser.Types.Event) =
            let input = (e.target :?> Browser.Types.HTMLInputElement)
            let result = input.value |> parse 

            match result with Error s -> s | _ -> "" 
                |> Store.set error

            result
        
        let commit set (e : Browser.Types.Event)  =
            let input = (e.target :?> Browser.Types.HTMLInputElement)
            if input.value <> formatted() then
                validate e |> Result.iter set

        Html.input [    
            Attr.custom ("type" ,typ)

            Attr.value (formatted())

            Ev.onFocus(fun e ->
                let input = (e.target :?> Browser.Types.HTMLInputElement)

                input.value <- formatted()

                "" |> Store.set error 
                validate e |> ignore            
            )

            Ev.onMount( fun e -> validate e |> ignore )

            match setter with 
            | Some f ->
                Ev.onBlur (commit f)
                Ev.onChange (fun (e : Browser.Types.Event) -> 
                    if not (hasFocus e.target) then
                        timeout 500 (fun _ -> commit f e) 
                )
                Ev.onInput (fun (e : Browser.Types.Event) -> 
                    if not (hasFocus e.target) then
                        timeout 500 (fun _ -> commit f e) 
                )
            | None ->
                Attr.readOnly true
                Ev.onInput( fun e -> validate e |> ignore)
        ]

//    open FrameworkTypes

    let editMultiLineText( field : Field<Types.MultiLineText> ) (_ : IStore<string>) =
        Html.textarea [
            Attr.value (field.Get().Text)
            match field.Set with
            | Some f ->
                Ev.onBlur (fun e -> 
                    let input = (e.target :?> Browser.Types.HTMLTextAreaElement)
                    let text = input.value
                    f(Types.MultiLineText.Of(text))
                )
            | None -> Attr.readOnly true
        ]
            
    [<AutoOpen>]
    module FormExt =

        type Field<'T> with
            member __.BuildMap( f : Core.SutilElement -> Core.SutilElement ) =
                let editor : (IStore<string> -> Core.SutilElement) =
                    match shortName(__.SystemTypeName) with

                    | "MultiLineText" ->
                        editMultiLineText (__ :> obj :?> Field<Types.MultiLineText>)

                    | _ ->
                        match __.Editor with

                        | FieldEditor.BuiltIn (BuiltInEditor.Select) ->
                            editFieldSelect __  

                        | FieldEditor.BuiltIn (BuiltInEditor.Text) when __.AllowedValues.IsSome ->
                            editFieldSelect __  

                        | FieldEditor.BuiltIn (BuiltInEditor.Checkbox) ->
                            editFieldCheckbox (__ :> obj :?> Field<bool>)

                        | FieldEditor.BuiltIn builtIn ->
                            editFieldInput builtIn __ 

                        | FieldEditor.Ctor ctor ->
                            fun errors -> ctor(__,errors)

                FieldElement.Of( fun () -> withLabelError __.Label editor |> f )

            member __.Build() =
                __.BuildMap id
