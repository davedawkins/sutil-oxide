module SutilOxide.Types

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

type DockCollection = {
    Stations : Map<DockLocation,DockStation>
}
with
    static member Empty =
        {
            Stations = DockLocation.All |> List.fold (fun s e -> s.Add(e, DockStation.Empty)) Map.empty
        }
    member __.GetPanes loc = __.Stations[loc].Panes
