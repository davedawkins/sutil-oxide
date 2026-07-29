/// #542: global registry for the active session's two storage-cache layers (Layer 1 = the F#
/// in-memory entry cache in KeyedStorageFileSystemAsync, Layer 2 = the JS IndexedDB key/value
/// cache in KeyedStorageIndexedDB), read by the fsimgo app's `project fs-stats` command.
///
/// Explicit registration, not "last constructed wins": a second KeyedStorageFileSystemAsync /
/// KeyedStorageIndexedDB pair (fsimgo's client/src/App/TestUI.fs) is constructed unconditionally
/// in every real session too. Relying on construction order to decide which instance is "active"
/// would silently report the wrong instance's numbers if that ordering ever changed. Instead the
/// one call site that knows it owns the real session storage (fsimgo's Server.fs) calls `setActive`
/// explicitly after construction.
module SutilOxide.EntryCacheDiagnostics

open SutilOxide.FileSystem

let mutable private activeEntryCache : IEntryCacheDiagnostics option = None
let mutable private activeKvStore : IKeyedStorageAsync option = None

let setActive (entryCache: IEntryCacheDiagnostics) (kvStore: IKeyedStorageAsync) =
    activeEntryCache <- Some entryCache
    activeKvStore <- Some kvStore

let entryCacheStats () = activeEntryCache |> Option.map (fun d -> d.GetStats())
let kvStoreStats () = activeKvStore |> Option.map (fun kv -> kv.GetStats())

let reset () =
    activeEntryCache |> Option.iter (fun d -> d.ResetStats())
    activeKvStore |> Option.iter (fun kv -> kv.ResetStats())

let setTracingEnabled (enabled: bool) =
    activeEntryCache |> Option.iter (fun d -> d.SetTracingEnabled enabled)
    activeKvStore |> Option.iter (fun kv -> kv.SetTracingEnabled enabled)

let setBudget (layer: string) (bytes: int) : Result<unit, string> =
    match layer with
    | "entry" ->
        match activeEntryCache with
        | Some d -> d.SetCacheBudget bytes; Ok ()
        | None -> Error "entry cache is not active"
    | "kv" ->
        match activeKvStore with
        | Some kv -> kv.SetCacheBudget bytes; Ok ()
        | None -> Error "kv store is not active"
    | other -> Error (sprintf "unknown layer '%s' (expected 'entry' or 'kv')" other)

let entryCacheTopReads (n: int) =
    activeEntryCache |> Option.map (fun d -> d.GetTopReads n) |> Option.defaultValue [||]

let kvStoreTopReads (n: int) =
    activeKvStore |> Option.map (fun kv -> kv.GetTopReads n) |> Option.defaultValue [||]
