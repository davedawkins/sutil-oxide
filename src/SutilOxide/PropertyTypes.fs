module SutilOxide.PropertyTypes

type DataType =
    | Int32
    | Flt32
    | Boolean
    | String
    | DateTime
    static member From( t : System.Type ) =
        match t with
        | x when x = typeof<string> -> String
        | x when x = typeof<int> -> Int32
        | x when x = typeof<float> -> Flt32
        | x when x = typeof<bool> -> Boolean
        | x when x = typeof<System.DateTime> -> DateTime
        | _ -> failwith ("Cannot map type: " + t.Name)
type MutableMap<'K,'T when 'K : comparison> =
    {
        mutable Map : Map<'K,'T>
    }
    with
        member __.Set( k, v) = __.Map <- __.Map.Add( k , v )
        member __.Get(k) = __.Map[k]
        member __.Nap = __.Map
        static member Empty = { Map = (Map.empty : Map<'K,'T>) } : MutableMap<'K,'T>

type PropertyBag = MutableMap<string,obj>
