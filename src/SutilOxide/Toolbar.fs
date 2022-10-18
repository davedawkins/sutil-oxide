module SutilOxide.Toolbar

open Browser.Dom
open Browser.CssExtensions

open Sutil.DOM
open Sutil
open Sutil.Styling
open type Feliz.length
open Fable.Core
open Fable.Core.JsInterop
open Browser.Types
open SutilOxide.Css

type UI =
    static member divc (cls:string) (items : seq<SutilElement>) =
        Html.div [ Attr.className cls ; yield! items ]


type ButtonMode =
    | Checkbox
    | Button

type ButtonProperty =
    | Mode of ButtonMode
    | Label of string
    | Icon of string
    | OnClick of (MouseEvent -> unit)
    | OnCheckChanged of (bool -> unit)
    | IsChecked of bool

type Button = {
    Mode : ButtonMode
    Label : string option
    Icon : string option
    OnClick : (MouseEvent -> unit) option
    OnCheckChanged : (bool -> unit) option
    IsChecked : bool
}
with
    static member Empty = { Label = None; Icon = None; OnClick = None; OnCheckChanged = None; Mode = Button; IsChecked = false }
    member __.With (p : ButtonProperty) =
        match p with
        | Mode s -> { __ with Mode = s }
        | Label s -> { __ with Label = Some s }
        | Icon s -> { __ with Icon = Some s }
        | IsChecked s -> { __ with IsChecked = s }
        | OnClick s -> { __ with OnClick = Some s }
        | OnCheckChanged s -> { __ with OnCheckChanged = Some s }
    static member From (p : ButtonProperty seq) =
        p |> Seq.fold (fun (b:Button) x -> b.With(x)) Button.Empty

let buttonGroup items =
    UI.divc "button-group" items

let menuStack items =
    UI.divc "menu-stack" items

let mkButton b =
    Html.a [
        Attr.className ("item-button" + (if b.Mode = Checkbox then " checkbox" else ""))

        if (b.Mode = Checkbox) then
            Html.i [ Attr.className ("fa fa-check " + (if b.IsChecked then "checked" else "")) ]
        else
            b.Icon
                |> Option.map (fun icon ->  Html.i [ Attr.className ("fa " + icon) ])
                |> Option.defaultValue (Html.i [])

        b.Label
            |> Option.map (fun label ->  Html.span label)
            |> Option.defaultValue (Html.span "")

        b.OnClick
            |> Option.map (fun cb ->  Ev.onClick (fun e -> e.preventDefault(); cb e))
            |> Option.defaultValue nothing

        b.OnCheckChanged
            |> Option.map (fun cb ->  Ev.onClick (fun e -> e.preventDefault(); cb (not (b.IsChecked))))
            |> Option.defaultValue nothing

        Attr.href "#"
    ]

let buttonItem props =
    { (Button.From props) with Mode = Button } |> mkButton

let toolbar props items =
    UI.divc "xd-toolbar" items

let checkItem props =
    { (Button.From props) with Mode = Checkbox; Icon = None; OnClick = None } |> mkButton

let menuItem props items =
    let b = Button.From props

    Html.a [
        Attr.className "item-button item-menu"

        b.Icon
            |> Option.map (fun icon ->  Html.i [ Attr.className ("fa " + icon) ])
            |> Option.defaultValue (Html.i [])

        b.Label
            |> Option.map (fun label ->  Html.span label)
            |> Option.defaultValue (Html.span "")

        b.OnClick
            |> Option.map (fun cb ->  Ev.onClick (fun e -> e.preventDefault(); cb e))
            |> Option.defaultValue nothing

        Html.i [ Attr.className "fa fa-angle-right"]

        Attr.href "#"

        menuStack items
    ]

let dropDownItem props items =
    Html.a [
        Attr.className "item-button"

        yield! props |> Seq.map (fun p ->

            match p with

            | Label label ->
                Html.span label

            | Icon icon ->
                Html.i [ Attr.className ("fa " + icon) ]

            | _ ->
                nothing

        )

        Attr.href "#"

        Ev.onClick (fun e ->
            props |> Seq.iter (fun p -> match p with OnClick cb -> cb e | _ -> ())
            if not e.defaultPrevented then
                e.preventDefault()
                let el = (DomHelpers.DomHelpers.currentEl e)
                console.log(el)
                //el.classList.toggle("active") |> ignore
        )
        menuStack items

    ]