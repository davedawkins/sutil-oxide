module SutilOxide.Modal

//
// Copyright (c) 2022 David Dawkins
//

open Sutil
open Sutil.Core
open Sutil.DomHelpers
open type Feliz.length
open Fable.Core.JsInterop
open Browser.Types

type Close = unit -> unit

type ModalOptions = {
        ShowCancel : bool
        OnCancel : unit -> unit
        Buttons : (string * (Close -> unit)) list
        Content : Close -> SutilElement
    } with
    static member Create() =
        { ShowCancel = true; OnCancel = ignore; Buttons = []; Content = fun _ -> Html.div [] }
    static member Create( content : (unit->unit) -> SutilElement) =
        { ShowCancel = true; OnCancel = ignore; Buttons = []; Content = content }


let modal (options : ModalOptions) =
    let doc = Browser.Dom.document
    let lastBodyElement : Browser.Types.HTMLElement = doc.body?lastElementChild
    let close() = Program.unmount (doc.querySelector("#ui-modal") :?> HTMLElement)
    let modalBg =
        Html.div [
            Attr.id "ui-modal"
            Attr.style [
                Css.positionFixed
                Css.displayFlex
                Css.justifyContentCenter
                Css.alignItemsCenter
                Css.backgroundColor "rgba(0,0,0,0.65)"
                Css.left (px 0)
                Css.right (px 0)
                Css.top (px 0)
                Css.bottom (px 0)
                Css.zIndex 10
            ]
            Html.div [
                Attr.style [
                    Css.displayFlex
                    Css.flexDirectionColumn
                    Css.padding (rem 1.5)
                    Css.positionRelative
                    Css.backgroundColor "white"
                    Css.borderRadius (px 4)
                    Css.gap (rem 1)
                    Css.alignItemsCenter
                ]
                if (options.ShowCancel) then
                    Html.div [
                        Attr.style [
                            Css.positionAbsolute
                            Css.top (rem 0)
                            Css.right (rem 0)
                            Css.cursorPointer
                            Css.transformRotateZ(45.0)
                            Css.fontWeightBold
                            Css.fontSize (rem 2)
                            Css.lineHeight (rem 1.5)
                            Css.filterDropShadow(0,0,8,"#505050")
                        ]
                        text "+"
                        Ev.onClick (fun _ -> close())
                    ]

                (options.Content) close

                if not options.Buttons.IsEmpty then
                    Html.div [
                        Attr.style [
                            Css.displayFlex
                            Css.gap (rem 1)
                        ]
                        for (label,click) in options.Buttons do
                            Html.button [
                                text label
                                Ev.onClick (fun _ -> click(close))
                            ]
                    ]
            ]
        ]
    (lastBodyElement,modalBg) |> Sutil.Program.mountAfter |> ignore
    ()