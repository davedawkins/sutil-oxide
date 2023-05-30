module SutilOxide.Types

//
// Copyright (c) 2022 David Dawkins
//



type BasicTransform2D =
    | Translate of float * float
    | Scale of float

type Transform2D = 
       Transform2D of BasicTransform2D[]
    with
        static member Empty = Transform2D [||]
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
            Fable.Core.JS.console.log(sprintf "transform: %f,%f -> %f,%f" x y tx ty)
            tx, ty
        member __.TransformInverse( x : float, y : float ) =
            let (Transform2D ts) = __
            let tx,ty = ts |> Array.fold(fun xy t -> Transform2D.TransformInverse(t,xy)) (x,y)
            Fable.Core.JS.console.log(sprintf "transform inverse: %f,%f -> %f,%f" x y tx ty)
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
    | CentreCentre
    | BottomRight
    | RightTop
    | RightBottom
    | TopLeft
    | TopRight
with
    static member All =
        [
            LeftTop; LeftBottom; BottomLeft; BottomRight; RightTop; RightBottom; CentreCentre; TopLeft; TopRight
        ]

    member __.Hand =
        match __.Primary, __.Secondary with
        | _,Left | Left,_ -> Left
        | _ -> Right


    member __.Primary =
        match __ with
        | CentreCentre -> Centre
        | LeftTop | LeftBottom -> Left
        | RightTop | RightBottom -> Right
        | BottomLeft | BottomRight -> Bottom
        | TopLeft | TopRight -> Top

    member __.Secondary=
        match __ with
        | CentreCentre -> Centre
        | LeftTop | RightTop -> Top
        | LeftBottom | RightBottom -> Bottom
        | BottomLeft | TopLeft  -> Left
        | BottomRight | TopRight -> Right

    member __.CssName =
        __.Primary.LowerName + "-" + __.Secondary.LowerName

type DockPane = {
    Name : string
}

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
            Stations = DockLocation.All |> List.fold (fun s e -> s.Add(e, DockStation.Empty)) Map.empty
        }
    member __.GetPanes loc = __.Stations[loc].Panes
