/// #542: byte-budgeted LRU cache of int -> 'v, with a per-entry size ceiling. Overflow evicts the
/// least-recently-used entry one at a time; there is no code path that empties the cache wholesale.
/// Used by KeyedStorageFileSystemAsync as the Layer-1 in-memory entry cache.
module SutilOxide.FileSystem.EntryCache

open System.Collections.Generic

// Recency order lives in a plain ResizeArray (index 0 = least-recently-used, last = most-recently-
// used) rather than a System.Collections.Generic.LinkedList<'T> -- Fable does not support LinkedList.
// Touch/remove are O(n) (IndexOf + RemoveAt), acceptable at the entry counts a byte budget bounds
// this to in practice (thousands, not millions).
type LruByteCache<'v>(sizeOf: 'v -> int, initialBudgetBytes: int, entryCeilingBytes: int) =
    let store = Dictionary<int, 'v>()
    let sizes = Dictionary<int, int>()
    let order = ResizeArray<int>()
    let mutable totalBytes = 0
    let mutable budgetBytes = initialBudgetBytes

    let mutable hits = 0
    let mutable misses = 0
    let mutable evictions = 0
    let mutable admissionRefused = 0
    let mutable highWaterEntries = 0
    let mutable highWaterBytes = 0

    let touch (uid: int) =
        let idx = order.IndexOf uid
        if idx >= 0 then
            order.RemoveAt idx
            order.Add uid

    let removeInternal (uid: int) =
        match store.TryGetValue uid with
        | true, _ ->
            totalBytes <- totalBytes - sizes.[uid]
            store.Remove uid |> ignore
            sizes.Remove uid |> ignore
            let idx = order.IndexOf uid
            if idx >= 0 then order.RemoveAt idx
        | false, _ -> ()

    let evictOneLru () =
        if order.Count > 0 then
            let uid = order.[0]
            removeInternal uid
            evictions <- evictions + 1

    let bumpHighWater () =
        highWaterEntries <- max highWaterEntries store.Count
        highWaterBytes <- max highWaterBytes totalBytes

    let evictWhileOverBudget () =
        // The `store.Count > 1` guard is what makes "never empties itself wholesale" structural:
        // eviction always leaves at least the most-recently-inserted/touched entry resident.
        while totalBytes > budgetBytes && store.Count > 1 do
            evictOneLru ()

    member _.Count = store.Count
    member _.Bytes = totalBytes
    member _.Budget = budgetBytes
    member _.EntryCeiling = entryCeilingBytes

    member _.ContainsKey(uid: int) : bool = store.ContainsKey uid

    member _.TryGet(uid: int) : 'v option =
        match store.TryGetValue uid with
        | true, v ->
            hits <- hits + 1
            touch uid
            Some v
        | false, _ ->
            misses <- misses + 1
            None

    /// Insert/replace. An entry above the per-entry ceiling is never admitted (refused, not
    /// evicted-immediately-after) -- this is what keeps one oversized entry (e.g. project.json)
    /// from displacing the rest of the working set.
    member this.Put(uid: int, v: 'v) =
        let size = sizeOf v
        if size > entryCeilingBytes then
            admissionRefused <- admissionRefused + 1
            removeInternal uid
        else
            removeInternal uid
            store.[uid] <- v
            sizes.[uid] <- size
            totalBytes <- totalBytes + size
            order.Add uid
            evictWhileOverBudget ()
        bumpHighWater ()

    /// Unconditional -- never gated by the eviction guard in Put. Remove is a caller-directed
    /// delete (the entry is actually gone from the backing store), not an eviction-under-pressure
    /// decision, so it must succeed even when this is the last cached entry (#542 red-team finding).
    member _.Remove(uid: int) = removeInternal uid

    member _.SetBudget(bytes: int) =
        budgetBytes <- bytes
        evictWhileOverBudget ()

    member _.Stats() : LayerStats =
        { Hits = hits; Misses = misses; Evictions = evictions; Flushes = 0
          AdmissionRefused = admissionRefused; Entries = store.Count; Bytes = totalBytes
          HighWaterEntries = highWaterEntries; HighWaterBytes = highWaterBytes
          BudgetBytes = budgetBytes; EntryCeilingBytes = entryCeilingBytes; Enabled = true
          TransactionsOpened = 0; ReadsIssued = misses }

    /// Zero the counters. Never touches cached entries -- see `project fs-stats reset`.
    member _.ResetCounters() =
        hits <- 0; misses <- 0; evictions <- 0; admissionRefused <- 0
        highWaterEntries <- store.Count; highWaterBytes <- totalBytes
