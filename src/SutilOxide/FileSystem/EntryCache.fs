/// #542: byte-budgeted LRU cache of int -> 'v, with a per-entry size ceiling. Overflow evicts the
/// least-recently-used entry one at a time; there is no code path that empties the cache wholesale.
/// Used by KeyedStorageFileSystemAsync as the Layer-1 in-memory entry cache.
module SutilOxide.EntryCache

open System.Collections.Generic
open SutilOxide.FileSystem

// #552: recency is a monotonic access-tick per entry (Dictionary<uid,tick>) rather than an
// explicit order list -- touch is then a single dictionary write, O(1) regardless of entry count.
// The tradeoff (per the issue's own suggested fix) is that finding the LRU victim on eviction is
// an O(n) scan over resident entries; that's acceptable because evictions are rare once the
// working set is warm (the entry count stays roughly stable under a byte budget), while touch is
// the hot path -- 234,544 touches measured in one compile vs a comparatively tiny eviction count.
type LruByteCache<'v>(sizeOf: 'v -> int, initialBudgetBytes: int, entryCeilingBytes: int) =
    let store = Dictionary<int, 'v>()
    let sizes = Dictionary<int, int>()
    let accessTick = Dictionary<int, int64>()
    let mutable clock = 0L
    let mutable totalBytes = 0
    let mutable budgetBytes = initialBudgetBytes

    let mutable hits = 0
    let mutable misses = 0
    let mutable evictions = 0
    let mutable admissionRefused = 0
    let mutable highWaterEntries = 0
    let mutable highWaterBytes = 0

    let touch (uid: int) =
        clock <- clock + 1L
        accessTick.[uid] <- clock

    let removeInternal (uid: int) =
        match store.TryGetValue uid with
        | true, _ ->
            totalBytes <- totalBytes - sizes.[uid]
            store.Remove uid |> ignore
            sizes.Remove uid |> ignore
            accessTick.Remove uid |> ignore
        | false, _ -> ()

    let findLruUid () =
        let mutable lruUid = -1
        let mutable lruTick = System.Int64.MaxValue
        for kv in accessTick do
            if kv.Value < lruTick then
                lruTick <- kv.Value
                lruUid <- kv.Key
        lruUid

    let evictOneLru () =
        let uid = findLruUid ()
        if uid >= 0 then
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
            touch uid
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
