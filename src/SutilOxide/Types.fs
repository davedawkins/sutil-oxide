module SutilOxide.Types

//
// Copyright (c) 2022 David Dawkins
//

open Sutil
open Sutil.Core
open Fable.Core

type BasicTransform2D =
    | Translate of float * float
    | Scale of float

type Transform2D = 
       Transform2D of BasicTransform2D[]
    with
        static member Empty = Transform2D [||]

        static member Translate( ox, oy ) = Transform2D [|
            Translate (ox,oy)
        |]

        static member TranslateScale( ox, oy, s ) = Transform2D [|
            Scale s
            Translate (ox,oy)
        |]

        static member Transform( t : BasicTransform2D, (x, y) : float*float ) =
            match t with
            | Translate (ox,oy) -> x + ox, y + oy 
            | Scale (s) -> x * s, y * s

        static member TransformInverse( t : BasicTransform2D, (x, y) : float*float ) =
            match t with
            | Translate (ox,oy) -> x - ox, y - oy 
            | Scale (s) -> x / s, y / s
             
        member __.Transform( x : float, y : float ) =
            let (Transform2D ts) = __
            let tx,ty = (x,y) |> Array.foldBack (fun t xy -> Transform2D.Transform(t,xy)) ts
            // Fable.Core.JS.console.log(sprintf "transform: %f,%f -> %f,%f" x y tx ty)
            tx, ty
        member __.TransformInverse( x : float, y : float ) =
            let (Transform2D ts) = __
            let tx,ty = ts |> Array.fold(fun xy t -> Transform2D.TransformInverse(t,xy)) (x,y)
            // Fable.Core.JS.console.log(sprintf "transform inverse: %f,%f -> %f,%f" x y tx ty)
            tx,ty
            
type Rect = 
    {
        X : float
        Y : float
        Width : float
        Height : float
    }
    static member Empty = { X = 0; Y = 0; Width = 0; Height = 0 }
    static member Create( x, y, w, h ) = { X = x; Y = y; Width = w; Height = h }
    static member Create( (x,y) : (float*float), ((x2,y2): float*float) ) = { X = x; Y = y; Width = x2-x; Height = y2-y }
    member __.X2 = __.X + __.Width
    member __.Y2 = __.Y + __.Height
    member __.Transform( t : Transform2D ) =
        Rect.Create(
            t.Transform( __.X, __.Y ),
            t.Transform( __.X2, __.Y2 )
        )
    member __.TransformInverse( t : Transform2D ) =
        Rect.Create(
            t.TransformInverse( __.X, __.Y ),
            t.TransformInverse( __.X2, __.Y2 )
        )

type TabHalf =
    | FirstHalf | SecondHalf

type Orientation =
    | Horizontal | Vertical
with
    member __.Opposite = if __ = Horizontal then Vertical else Horizontal

type BasicLocation =
    | Left
    | Right
    | Centre
    | Top
    | Bottom
with
    static member All = [ Left; Right; Centre; Top; Bottom ]
    member __.LowerName = __.ToString().ToLower()
    member __.Orientation =
        match __ with Left|Right -> Horizontal | _ -> Vertical
    member __.Opposite =
        match __ with
        |Centre -> Centre
        |Left -> Right
        |Right -> Left
        |Top -> Bottom
        |Bottom -> Top


type DockLocation =
    | LeftTop
    | LeftBottom
    | BottomLeft
    | CentreLeft
    | CentreRight
    | BottomRight
    | RightTop
    | RightBottom
    | TopLeft
    | TopRight
with
    static member All =
        [|
            LeftTop; LeftBottom; BottomLeft; BottomRight; RightTop; RightBottom; CentreLeft; CentreRight; TopLeft; TopRight
        |]

    static member AllNames = 
        DockLocation.All |> Array.map _.ToString()

    static member TryParse (s : string) =
        let i = DockLocation.AllNames |> Array.findIndex (fun name -> name = s)
        if i < 0 then 
            None
        else
            DockLocation.All[i] |> Some

    member __.Hand =
        match __.Primary, __.Secondary with
        | _,Left | Left,_ -> Left
        | _ -> Right

    member __.Primary =
        match __ with
        | CentreLeft | CentreRight -> Centre
        | LeftTop | LeftBottom -> Left
        | RightTop | RightBottom -> Right
        | BottomLeft | BottomRight -> Bottom
        | TopLeft | TopRight -> Top

    member __.Secondary=
        match __ with
        | LeftTop | RightTop -> Top
        | LeftBottom | RightBottom -> Bottom
        | CentreLeft | TopLeft | BottomLeft | TopLeft  -> Left
        | CentreRight | TopRight | BottomRight | TopRight -> Right

    member __.CssName =
        __.Primary.LowerName + "-" + __.Secondary.LowerName

// type DockPane = {
//     Key : string
//     Label : string
// }

type LabelElement =
    | LabelString of string
    | LabelElement of SutilElement

type PaneOptions =
    | Group of string
    | Label of string
    | LabelEl of SutilElement
    | CanClose of bool
    | Location of DockLocation
    | Header of SutilElement
    | Content of SutilElement
    | IsOpen of bool
    | Icon of string
    | OnClose of (unit -> unit)
    | OnShow of (bool -> unit)
    | Size of float

module StringHelpers =
    open System
    open System.Text.RegularExpressions

    let toClass( s : string ) = s.ToLower().Replace(" ", "-").Replace("_","-")

    let toCapWords (input: string) =
        let spaced =
            // Insert space before capitals in PascalCase / camelCase
            Regex.Replace(input, "([a-z])([A-Z])", "$1 $2")
        let cleaned =
            spaced.Replace("-", " ")
                .Replace("_", " ")
                .Trim()
                .ToLowerInvariant()
        // Capitalize each word
        cleaned.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)
        |> Array.map (fun w -> w.[0].ToString().ToUpper() + w.Substring(1))
        |> String.concat " "

type DockPaneKey = Key of string
    with 
        static member From(s : string) =
            if StringHelpers.toClass s <> s || s = "" then
                Fable.Core.JS.console.error( "DockPaneKey: ",  sprintf "Invalid key value: %s: lowercase and '-' only" s ) 
                failwithf "Invalid key value: %s: lowercase and '-' only" s
            Key s

        override __.ToString (): string = let (Key s) = __ in s
        member __.AsLabel = __.ToString() |> StringHelpers.toCapWords


type DockPane = 
    {
        Label : LabelElement
        CanClose : bool
        StrictKey : DockPaneKey
        Icon: string
        Location : DockLocation
        Group : string
        Header : SutilElement
        Content : SutilElement
        IsOpen : bool
        OnClose : unit -> unit
        OnShow : bool -> unit
        Size : float
    }
    member __.Key = __.StrictKey.ToString()
    member __.KeyAsClass = __.StrictKey.ToString()
    member __.KeyAsLabel = __.StrictKey.AsLabel
    static member Equals( p1 : DockPane, p2 : DockPane) =
        p1.Label = p2.Label &&
        p1.CanClose = p2.CanClose &&
        p1.Key = p2.Key &&
        p1.Location = p2.Location &&
        p1.IsOpen = p2.IsOpen 
        
//    static member MakeKey(s:string) = s.ToLower().Replace(".", "_")
    static member Default( key : string ) =
        {
            StrictKey = DockPaneKey.From(key)
            Label = LabelString key
            CanClose = false
            Location = CentreLeft
            Header = text key
            Content = Html.div key
            IsOpen = true
            OnClose = ignore
            Icon = "fa-folder"
            OnShow = ignore
            Size = -1
            Group = ""
        }
        
    static member Create( key : string, options : PaneOptions list ) : DockPane =
        let withOpt cfg opt : DockPane =
            match opt with 
            | LabelEl e -> { cfg with Label = LabelElement e }
            | Label s -> { cfg with Label = LabelString s }
            | Icon s -> { cfg with Icon = s }
            | Group s -> { cfg with Group = s }
            | CanClose s -> { cfg with CanClose = s }
            | Location s -> { cfg with Location = s }
            | Content s -> { cfg with Content = s }
            | Header s -> { cfg with Header = s }
            | IsOpen s -> { cfg with IsOpen = s }
            | OnClose s -> { cfg with OnClose = s }
            | OnShow s -> { cfg with OnShow = s }
            | Size s -> { cfg with Size = s }

        let init = (DockPane.Default(key))
        options |> List.fold withOpt init


type DockStation = {
    Panes : DockPane list
}
with
    static member Empty = { Panes = [] }

type  DockCollection = {
    Stations : Map<DockLocation,DockStation>
}
with
    static member Empty =
        {
            Stations = DockLocation.All |> Array.fold (fun s e -> s.Add(e, DockStation.Empty)) Map.empty
        }
    member __.GetPanes loc = __.Stations[loc].Panes


[<Erase>]
type SpacedList = SpacedList of string
    with    static member Of s = SpacedList s
            member __.Text = (let (SpacedList s) = __ in s)
            member __.AsArray() = __.Text.Split( [| ' ' |], System.StringSplitOptions.RemoveEmptyEntries )
            member __.FromArray( values : string[] ) = SpacedList.Of( values |> String.concat " " )

[<Erase>]
type MultiLineText = private MultiLineText of string
    with    static member Of s = MultiLineText s
            member __.Text = (let (MultiLineText s) = __ in s)
            member __.AsLines() = __.ToString().Replace("\r\n", "\n").Split( [| '\n'; '\r' |] )
            member __.FromLines( lines : string[] ) = SpacedList.Of( lines |> String.concat "\n" )
