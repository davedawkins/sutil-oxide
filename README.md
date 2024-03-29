# Sutil Oxide

A toolkit for building IDE-style web applications, using [Sutil](https://sutil.dev).

## Demo

The demo is best viewed on desktop, though it does try and be basically responsive.

Click on the `Help` button for details on how the [demo](https://davedawkins.github.io/sutil-oxide/) works. Repository located at [https://github.com/davedawkins/sutil-oxide](https://github.com/davedawkins/sutil-oxide).

## Motivation

My own projects frequently need a UI toolkit, and my latest project needed dockable windows. Sutil Oxide is intended to be where I develop any reusable UI code and components.

## Status
Very much still in development. My focus is on the main project that consumes this toolkit, and so I add functionality, make fixes etc as and when I need to according to the needs of that driving project. In fact, mostly I'm refactoring out of the main project into this toolkit.

The main reason you're seeing it at this early stage is that I wanted to contribute to the F# Advent, and a couple of people I'd already shown it to thought it was cool in its own right.

You will find many niggles and annoyances, but nonetheless I've found it valuable for getting my main project off the ground. I can more easily focus on the business logic and then categorise issues as either being UI/presentation or application based. It's a useful separation.

## Scope

Any reusable UI component that might be generally useful for other Sutil projects. It may be possible to wrap as `web components`, such that non-Sutil projects can use this toolkit.

## Features

- 9 docking locations
- Resizable panes
- Drag and drop pane's tab to relocate panes
- Also relocate panes with "cog" drop-down menu
- Minimize pane with "-" button or click on tab
- Theming support
- Functionally reactive thanks to [Sutil](https://sutil.dev)
- Toolbar
- Statusbar
- File Explorer (and basic IFileSystem interface with LocalStorage implementation)

## Issues
- No drag *to* centre pane. Use "cog" menu to send panes to centre
- Submenu placement janky, would prefer a CSS-only solution but may have to resort to code
- No persistence of layout
- Very basic mobile support, currently best experience desktop

## Docking Layout

```
+-----------------------------+---------------------------+
| TopLeft                     |                  TopRight |
+------------+----------------+-------------+-------------+
| LeftTop    |                              |    RightTop |
|            |          CentreCentre        |             |
|            |                              |             |
+------------+                              +-------------+
| LeftBottom |                              | RightBottom |
|            |                              |             |
|            |                              |             |
+------------+----------------+-------------+-------------+
| BottomLeft                  |               BottomRight |
+-----------------------------+---------------------------+
```

## Creating a Dock Layout

See `src/App/App.fs` for a full example.

```fs
    open SutilOxide
    open SutilOxide.Dock
    open SutilOxide.Types

    let initPanes (dc : DockContainer) =
        dc.AddPane( "Test", LeftTop, Html.div "This is the Test pane" )
        dc.AddPane( "Main", CentreCentre, Html.div "This is the Main pane" )


    let mainView() =
        Html.div [
            Attr.className "main-container"
            DockContainer.Create initPanes
        ]
```

## Top-level Styling

A suggested style for `div.main-container`:

```css
    .dock-container {
        /* Occupy all of browser viewport */
        height: 100vh;
        width: 100vw;
    }
```

You can either use a regular `main.css` file for this, or apply the style within Sutil:

Using inline styles:

```fs

    open type Feliz.length

    // 1. Apply style inline

    let mainView() =
        Html.div [
            Attr.style [
                Css.width (vw 100)
                Css.height (vh 100)
            ]
            DockContainer.Create initPanes
        ]
```

or using `withStyle` to apply a private stylesheet:

```fs
    open Sutil.Style
    open type Feliz.length

    let mainStyle = [
        rule ".main-container" [
            Css.width (vw 100)
            Css.height (vh 100)
        ]
    ]

    let mainView() =
        Html.div [
            Attr.className "main-container"
            DockContainer.Create initPanes
        ] |> withStyle mainStyle
```

If you plan to add a toolbar or a statusbar then you may want to make the top-level div into a `flex column`:

```fs
    open Sutil.Style
    open type Feliz.length

    let mainStyle = [
        rule ".main-container" [
            Css.width (vw 100)
            Css.height (vh 100)
            Css.displayFlex
            Css.flexDirectionColumn
        ]
    ]

    let mainView() =
        Html.div [
            Attr.className "main-container"
            DockContainer.Create initPanes
        ] |> withStyle mainStyle
```

## Adding Panes

Method `AddPane` takes the following arguments:

```fs
    member _.AddPane (name : string, initLoc : DockLocation, content : SutilElement )
```

- Name of pane, as a `string`. This should be unique, and will also be the default label shown in the pane header
- Initial location of pane, as a `DockLocation` (see below)
- Pane content, as a `SutilElement`


Another overload of `AddPane` will allow the header to be given as a `SutilElement`:

```fs
    member __.AddPane (name : string, initLoc : DockLocation, header : SutilElement, content : SutilElement, show : bool ) =
```

Parameters are as before, but with the addition of `show : bool`:
- Initial visibility of pane as a boolean

Another overload of `AddPane` will allow the header to be given as a `SutilElement`:

```fs
    member __.AddPane (name : string, initLoc : DockLocation, header : SutilElement, content : SutilElement, show : bool ) =
```

Parameters are as before, but with the addition of `header : SutilElement`:

- Content of header, as a `SutilElement`.

Possible dock locations are defined by `DockLocation`:

```fs
type DockLocation =
    | LeftTop
    | LeftBottom
    | BottomLeft
    | CentreCentre
    | BottomRight
    | RightTop
    | RightBottom
    | TopLeft
    | TopRight
```

...more to come