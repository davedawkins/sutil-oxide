module SutilOxide.Toolbar

//
// Copyright (c) 2022 David Dawkins
//

open Browser.Dom
open Browser.CssExtensions

open Sutil.Core
open Sutil.CoreElements
open Sutil
open Fable.Core.JsInterop
open Browser.Types

type UI =
    static member divc (cls:string) (items : seq<SutilElement>) =
        Html.div [ Attr.className cls ; yield! items ]

type ButtonMode =
    | Checkbox
    | Button

type DisplayMode =
    | LabelOnly
    | IconOnly
    | LabelIcon

type ButtonProperty =
    | Display of DisplayMode
    | Mode of ButtonMode
    | Label of string
    | Icon of string
    | OnClick of (MouseEvent -> unit)
    | OnCheckChanged of (bool -> unit)
    | IsChecked of bool

type Button = {
    Display : DisplayMode
    Mode : ButtonMode
    Label : string option
    Icon : string option
    OnClick : (MouseEvent -> unit) option
    OnCheckChanged : (bool -> unit) option
    IsChecked : bool
}
with
    static member Empty = { Label = None; Icon = None; OnClick = None; OnCheckChanged = None; Mode = Button; IsChecked = false; Display = LabelIcon }
    member __.With (p : ButtonProperty) =
        match p with
        | Display s -> { __ with Display = s }
        | Mode s -> { __ with Mode = s }
        | Label s -> { __ with Label = Some s }
        | Icon s -> { __ with Icon = Some s }
        | IsChecked s -> { __ with IsChecked = s }
        | OnClick s -> { __ with OnClick = Some s }
        | OnCheckChanged s -> { __ with OnCheckChanged = Some s }
    static member From (p : ButtonProperty seq) =
        p |> Seq.fold (fun (b:Button) x -> b.With(x)) Button.Empty


module MenuMonitor =
    open Browser

    let seqOfNodeList<'T> (nodes: Browser.Types.NodeListOf<'T>) =
        seq {
            for i in [0..nodes.length-1] do
                yield nodes.[i]
        }

    // let logEntry (e : Types.IntersectionObserverEntry) =
    //     console.log("boundingClientRect=", e.boundingClientRect)
    //     console.log("intersectionRatio=", e.intersectionRatio)
    //     console.log("intersectionRect=", e.intersectionRect)
    //     console.log("isIntersecting=", e.isIntersecting)
    //     console.log("rootBounds=", e.rootBounds)
    //     console.log("target=", e.target)
    //     console.log("time=", e.time)

    let removeStyle( e : HTMLElement ) name=
        e.style.removeProperty(name) |> ignore

    let resetMenu( e : HTMLElement ) =
        [   "top"
            "left"
            "bottom"
            "right" ] |> List.iter (removeStyle e)

    let moveMenu (e : Types.HTMLElement) (bcr : Types.ClientRect) (ir : Types.ClientRect) =
        if (bcr.right > ir.right) then
            e.style.left <- "unset"
            e.style.right <- "0px"
        if (bcr.bottom > ir.bottom) then
            e.style.top <- sprintf "%fpx" ((ir.bottom - bcr.height) (*- pr.top - 12.0 *) )
            e.style.bottom <- "unset"
        else if (bcr.top < ir.top) then
            e.style.top <- "0px"
            e.style.bottom <- "unset"

    let callback (entries : Types.IntersectionObserverEntry[]) _ =
        entries |> Array.iter (fun e ->
            if (e.isIntersecting && e.intersectionRatio < 1.0) then
                moveMenu (e.target :?> Types.HTMLElement) (e.boundingClientRect) (e.intersectionRect)
        )

    let mutable _observer : IntersectionObserverType option = None

    let makeObserver() =
        let options =
            {| root = document :> Browser.Types.Node; rootMargin = ""; threshold = 0.0 |}
        IntersectionObserver.Create(callback, !! options)

    let getObserver() : IntersectionObserverType =
        match _observer with
        | None ->
            let _io = makeObserver()
            _observer <- _io |> Some
            _io
        | Some x -> x

    let monitorMenu( e : HTMLElement ) =
        resetMenu e
        getObserver().observe(e)

    let monitorQuery( query : string ) =
        getObserver().disconnect()
        document.querySelectorAll(query)
        |> seqOfNodeList
        |> Seq.iter (fun n -> monitorMenu (n :?> Types.HTMLElement) )

    let monitorAll() =
        monitorQuery( ".menu-stack" )

let buttonGroup items =
    UI.divc "button-group" items

let menuStack items =
    UI.divc "menu-stack" [
        host MenuMonitor.monitorMenu
        yield! items
    ]

let mkButton b =
    let checkedS = Store.make b.IsChecked
    Html.a [
        disposeOnUnmount [ checkedS ]
        Attr.className ("xd-item-button" + (if b.Mode = Checkbox then " checkbox" else ""))
        Attr.href "-"
        
        if b.Mode = Checkbox then
            Attr.roleCheckbox
        else
            Attr.roleButton

        match b.Label with
        | Some label -> Attr.custom("data-cmd", label.ToLower().Replace(" ", "-"))
        | _ -> ()
        
        match b.Mode, b.Icon, b.Display with

        | Checkbox, _, _ ->

            Bind.el( checkedS,
                fun ch -> Html.i [ Attr.className ("fa fa-check " + (if ch then "checked" else "")) ]
            )

        | _, Some icon, LabelIcon | _, Some icon, IconOnly ->
            Html.i [ Attr.className ("fa " + icon) ]

        | _ ->
            Html.i [ Attr.style [ Css.displayNone ] ]

        match b.Label, b.Display with

        | Some label, LabelOnly | Some label, LabelIcon ->
            Html.span [
                //Ev.onClick (fun e -> console.log("click span"))
                text label
            ]

        | _ ->
            Html.span [ Attr.style [ Css.displayNone ] ]

        b.OnClick
            |> Option.map (fun cb ->  Ev.onClick (fun e -> 
                e.preventDefault(); cb e

                let mutable e  = (e.target :?> Browser.Types.HTMLElement)
                while e <> null do
                    e.blur()
                    e <- e.parentElement
                )
            )
            |> Option.defaultValue nothing

        b.OnCheckChanged
            |> Option.map (fun cb ->  Ev.onClick (fun e -> 
                e.preventDefault()
                checkedS |> Store.modify (not)
                cb (checkedS.Value)
            ))
            |> Option.defaultValue nothing

    ]

let vseparator =
    Html.span [ Attr.className "xd-vseparator"; text "|"]

let hseparator =
    Html.hr [ Attr.className "xd-hseparator" ]

let gap = Html.span [ Attr.className "xd-gap" ]

let buttonItem props =
    { (Button.From props) with Mode = Button } |> mkButton

let button name icon cb =
    buttonItem [
        if icon <> "" then
            Icon ("fa-" + icon)
        Label name
        OnClick cb
        if icon <> "" then
            Display IconOnly
    ]

let toolbar props items =
    UI.divc "xd-toolbar" items

let statusbar props items =
    UI.divc "xd-toolbar xd-statusbar theme-control-bg theme-border" items

let checkItem props =
    { (Button.From props) with Mode = Checkbox; Icon = None; OnClick = None } |> mkButton

let rec findMenuLevel (e : HTMLElement ) =
    if e = null then 
        -1
    else
        let levelAttr = e.getAttribute("data-menu-level")
        if levelAttr = null then
            e.parentElement |> findMenuLevel
        else
            levelAttr |> System.Int32.TryParse |> fun (a,b) -> if a then b else -1

let menuItem props items =
    let b = Button.From props

    Html.a [
        Attr.className "xd-item-button item-menu"

        CoreElements.hookElement (fun e ->
            e.setAttribute("data-menu-level", (e.parentElement) |> findMenuLevel |> string )
        )

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

        Attr.href "-"

        menuStack items
    ]

let dropDownItem props items =
    Html.a [
        Attr.className "xd-item-button xd-dropdown"
        Attr.href "-"
        Attr.custom("data-menu-level", "0")

        yield! props |> Seq.map (fun p ->

            match p with

            | Label label ->
                Html.span label

            | Icon icon ->
                Html.i [ Attr.className ("fa " + icon) ]

            | _ ->
                nothing

        )

        //Html.i [
        //    Attr.className "xd-dropdown-caret fa fa-caret-down"
        //]

        Ev.onClick (fun e ->
            props |> Seq.iter (fun p -> match p with OnClick cb -> cb e | _ -> ())
            if not e.defaultPrevented then
                e.preventDefault()
                //let el = (DomHelpers.DomHelpers.currentEl e)
                //console.log(el)
                //el.classList.toggle("active") |> ignore
        )
        menuStack items
    ]

let right items =
    Html.div [
        Attr.className "item-group-right"
        yield! items
    ]