module SutilOxide.PropertyEditor

let applog s = ()

open Sutil
open Sutil.Core
open Sutil.Styling
open type Feliz.length

open SutilOxide.Toolbar

open PropertyTypes

type GridRow = {
    Label : string
    Category : string
    Editor : SutilElement
}

let makeCell (onchanged: (string*obj) -> unit) (name : string) (t : DataType) : SutilOxide.CellEditor.Cell<PropertyBag>=
    //Fable.Core.JS.console.log("makeCell", name, string t)
    let d v = onchanged (name,v)
    match t with
    | Flt32 ->
        SutilOxide.CellEditor.FloatCell.Create(
            (fun (r : PropertyBag) -> r.Get(name) :?> float ),
            (fun (r : PropertyBag) (v : float) -> r.Set(name,v); d v)
        )
        
    | Int32 ->
        SutilOxide.CellEditor.IntCell.Create(
            (fun (r : PropertyBag) -> r.Get(name) :?> int ),
            (fun (r : PropertyBag) (v : int) -> r.Set(name,v); d v)
        )
    | Boolean ->
        SutilOxide.CellEditor.BoolCell.Create(
            (fun (r : PropertyBag) -> r.Get(name) :?> bool ),
            (fun (r : PropertyBag) (v : bool) -> r.Set(name,v); d v)
        )
    | String ->
        SutilOxide.CellEditor.StrCell.Create(
            (fun (r : PropertyBag) -> r.Get(name) :?> string ),
            (fun (r : PropertyBag) (v : string) ->
                r.Set(name,v); d v)
        )
    | _ -> failwith "Unsupported cell type"

let makeGridRow name category bag cell =
    {
        GridRow.Label = name
        GridRow.Category = category
        GridRow.Editor =  SutilOxide.CellEditor.view bag false cell

    }

let isConfigType (t : System.Type) =
    (t = typeof<int> || t = typeof<float> || t = typeof<bool> || t = typeof<string>)

let makeGridRows bag category (fields : (string*DataType)[]) dispatch =
    //let fields = FSharp.Reflection.FSharpType.GetRecordFields(recordType)
    fields 
        //|> Array.filter (fun f -> isConfigType(f.PropertyType))
        |> Array.map (fun (name,dtype) ->
            let cell = makeCell dispatch name dtype
            makeGridRow name category bag cell
        )

// let makeGridRows bag (recordType : System.Type) dispatch =
//     let fields = FSharp.Reflection.FSharpType.GetRecordFields(recordType)
//     fields 
//         |> Array.filter (fun f -> isConfigType(f.PropertyType))
//         |> Array.map (fun f ->
//             let cell = makeCell dispatch f.Name f.PropertyType
//             makeGridRow f.Name bag cell
//         )

let peCss = [
    rule ".property-editor" [
        Css.width (percent 100)
        Css.height (percent 100)
        Css.overflowAuto
        Css.fontSize (percent 75)
        Css.padding (rem 0.5)
    ]
    rule ".property-editor .items" [
        Css.displayGrid
        Css.custom( "grid-template-columns", "min-content auto")
        Css.gap (rem 0.2)
        Css.alignItemsCenter
    ]

    rule ".property-editor .items>span" [
        Css.color "darkblue"
    ]

    rule ".property-editor .items>*" [
        // Css.marginLeft (px 2)
        // Css.marginRight (px 2)
    ]

    rule ".group" [
        Css.fontWeightBold
    ]
]

type Model = {
    Items : GridRow list
    Title : string
}

type ApiMessage =
    | SetValue of string * GridRow list

type Message =
    | Api of ApiMessage

let init() =
    {
        Items = []
        Title = "None"
    }, Cmd.none

let updateFromApi msg model =
    match msg with
    | SetValue (title, items) ->
        { model with Items = items; Title = title}, Cmd.none

let update msg model =
    match msg with
    | Api m -> updateFromApi m model

let create () =

    let model, dispatch = () |> Store.makeElmish init update ignore

    let addItem (g : Map<string,GridRow list>) (item : GridRow) =
        if not (g.ContainsKey (item.Category)) then
            g.Add( item.Category, [ item ])
        else
            g.Add( item.Category, g[item.Category] @ [ item ])

    let view  =
        UI.divc "property-editor" [
            Bind.el( model |> Store.map (fun m -> m.Items),                
                fun items ->
                    let grouped = items |> List.fold addItem Map.empty
                    Html.divc "groups" [
                        for group in grouped do 
                            Html.divc "group-container" [
                                Html.divc "group" [ text group.Key ]
                                UI.divc "items" [
                                    for item in group.Value do
                                        yield! [ Html.span item.Label; item.Editor ]
                                ]
                            ]
                    ]
            )
        ] |> withStyle peCss

    view, dispatch<<Api
