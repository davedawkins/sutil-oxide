/**
 * Optimized IndexedDB implementation for the SutilOxide file system
 * Addresses performance issues with transaction management, connection handling,
 * and resource cleanup that were causing browser lockups.
 * 
 * Example usage with consistency checking:
 * 
 * const db = new KeyedStorageIndexedDB('myFileSystem');
 * await db.init();
 * 
 * // Check consistency and get detailed results
 * const result = await db.checkConsistency();
 * if (!result.isValid) {
 *     console.error('Database consistency errors:', result.errors);
 *     console.log('Summary:', result.summary);
 * } else {
 *     console.log('Database is consistent:', result.summary);
 * }
 * 
 * // Or use the convenient logging method for debugging
 * const isValid = await db.logConsistencyCheck();
 * if (!isValid) {
 *     // Handle inconsistencies...
 * }
 * 
 * // F# interface usage:
 * const storage = createKeyedStorageIndexedDB('myFileSystem');
 * const result = await storage.CheckConsistency();
 * const isValid = await storage.LogConsistencyCheck();
 */

function clog(a,b) {
    //console.log("KeyedStorageIndexedDB.js: " + a, ": ", b);
}

export class KeyedStorageIndexedDB {
    constructor(dbName = 'keyValueStore', storeName = 'keyValuePairs', version = 1) {
        this.dbName = dbName;
        this.storeName = storeName;
        this.version = version;
        this.db = null;
        this.isInitializing = false;
        this.initPromise = null;
        this.pendingTransactions = new Set();

        // Batch operation management
        this.batchMode = false;
        this.batchOperations = [];
        this.batchSize = 25; // Optimal batch size for IndexedDB operations

        // Cache management
        this.cacheEnabled = true;
        this.cache = new Map();
        this.cacheLimit = 100;

        // Setup automatic cleanup handlers
        this._setupCleanupHandlers();
    }

    /**
     * Setup handlers for cleaning up resources when page is unloaded
     * Helps prevent resource leaks and browser freezes
     * @private
     */
    _setupCleanupHandlers() {
        // Close DB when page is unloaded to prevent resource leaks
        const cleanupHandler = () => {
            this._flushBatch(true);
            this.close();
        };

        window.addEventListener('beforeunload', cleanupHandler);
        window.addEventListener('visibilitychange', () => {
            if (document.visibilityState === 'hidden') {
                this._flushCacheIfNeeded(true);
            }
        });
    }

    /**
     * Initialize the database connection with proper error handling
     * @returns {Promise<void>} A promise that resolves when the database is ready
     */
    async init() {
        // Return existing promise if initialization is in progress
        if (this.initPromise) return this.initPromise;

        // Return immediately if already initialized
        if (this.db && !this.db.closed) return Promise.resolve();

        // Set flag and create promise
        clog("init", "initPromise");
        this.isInitializing = true;
        this.initPromise = new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, this.version);

            request.onerror = () => {
                this.isInitializing = false;
                this.initPromise = null;
                reject(request.error);
            };

            request.onsuccess = () => {
                this.db = request.result;

                // Handle unexpected database closures
                this.db.onclose = () => {
                    this.db = null;
                    this.initPromise = null;
                };

                // Handle version change (another tab/window triggered upgrade)
                this.db.onversionchange = () => {
                    if (this.db) {
                        this._flushBatch(true);
                        this.db.close();
                        this.db = null;
                        this.initPromise = null;
                    }
                };

                this.isInitializing = false;
                resolve();
            };

            request.onblocked = (event) => {
                console.warn('Database upgrade was blocked. Please close other tabs with this app open.');
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;

                // Create object store if it doesn't exist
                if (!db.objectStoreNames.contains(this.storeName)) {
                    // Add an index for faster lookups on common patterns
                    const store = db.createObjectStore(this.storeName);

                    // Add indexes for common access patterns observed in the F# code
                    store.createIndex('prefix', '', { multiEntry: false });
                }
            };
        });

        return this.initPromise;
    }

    /**
     * Close the database connection and clean up resources
     */
    close() {
        // Wait for any pending transactions to complete
        if (this.pendingTransactions.size > 0) {
            console.warn(`Closing database with ${this.pendingTransactions.size} pending transactions`);
        }

        if (this.db && !this.db.closed) {
            // Flush any pending batch operations
            this._flushBatch(true);
            this.db.close();
        }

        this.db = null;
        this.initPromise = null;
        this.cache.clear();
    }

    /**
     * Begin a batch of operations for better performance
     * Operations will be queued and executed together
     */
    beginBatch() {
        if (!this.batchMode) {
            clog("beginBatch", "batchMode");
            this.batchMode = true;
            this.batchOperations = [];
        }
    }

    /**
     * Commit all pending batch operations
     * @returns {Promise<void>}
     */
    async commitBatch() {
        clog("commitBatch", this.batchMode);
        if (!this.batchMode) {
            return;
        }
        await this._flushBatch();
        this.batchMode = false;
    }

    /**
     * Process a batch of pending operations
     * @param {boolean} immediate - Whether to process immediately regardless of batch size
     * @returns {Promise<void>}
     * @private
     */
    async _flushBatch(immediate = true) {
        if (!this.batchOperations.length) {
            clog("_flushBatch", "no operations");
            return;
        }

        // Only flush if we've hit batch size or immediate is true
        if (!immediate && this.batchOperations.length < this.batchSize) {
            clog("_flushBatch", [ immediate, this.batchOperations.length, this.batchSize ]);
            return;
        }

        await this.ensureConnection();

        const operations = [...this.batchOperations];
        this.batchOperations = [];

        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction([this.storeName], 'readwrite');
            const store = transaction.objectStore(this.storeName);

            // Add this transaction to pending set
            const transactionId = Date.now() + Math.random();
            this.pendingTransactions.add(transactionId);

            transaction.oncomplete = () => {
                this.pendingTransactions.delete(transactionId);
                resolve();
            };

            transaction.onerror = () => {
                this.pendingTransactions.delete(transactionId);
                reject(transaction.error);
            };

            transaction.onabort = () => {
                this.pendingTransactions.delete(transactionId);
                reject(new Error('Transaction was aborted'));
            };

            clog("Flushing batcn ", operations.length)
            // Process all operations in the batch
            operations.forEach(op => {
                clog("- ", op);
                if (op.type === 'put') {
                    store.put(op.value, op.key);
                    // Update cache
                    if (this.cacheEnabled) {
                        this.cache.set(op.key, op.value);
                    }
                } else if (op.type === 'delete') {
                    store.delete(op.key);
                    // Remove from cache
                    if (this.cacheEnabled) {
                        this.cache.delete(op.key);
                    }
                }
            });
            clog("(batch flushed) ", operations.length)            

        });
    }

    /**
     * Flush cache if it's getting too large
     * @param {boolean} force - Whether to force flush regardless of size
     * @private
     */
    _flushCacheIfNeeded(force = false) {
        if (!this.cacheEnabled) return;

        if (force || this.cache.size > this.cacheLimit) {
            this.cache.clear();
        }
    }

    /**
     * Check if the database connection is valid
     * @returns {boolean} True if the connection is valid
     */
    isConnectionValid() {
        return this.db !== null && !this.db.closed;
    }

    /**
     * Ensure the database connection is ready
     * @returns {Promise<void>}
     */
    async ensureConnection() {
        if (!this.isConnectionValid()) {
            await this.init();
        }
    }

    /**
     * Create a transaction with proper error handling
     * @param {string} mode - Transaction mode ('readonly' or 'readwrite')
     * @returns {Object} Object containing transaction, store and promise
     */
    createTransaction(mode) {
        const transaction = this.db.transaction([this.storeName], mode);
        const store = transaction.objectStore(this.storeName);

        // Create a unique ID for this transaction
        const transactionId = Date.now() + Math.random();
        this.pendingTransactions.add(transactionId);

        // Create a promise that resolves when the transaction completes
        const transactionPromise = new Promise((resolve, reject) => {
            transaction.oncomplete = () => {
                this.pendingTransactions.delete(transactionId);
                resolve();
            };

            transaction.onerror = () => {
                this.pendingTransactions.delete(transactionId);
                reject(transaction.error || new Error('Transaction failed'));
            };

            transaction.onabort = () => {
                this.pendingTransactions.delete(transactionId);
                reject(new Error('Transaction was aborted'));
            };
        });

        return { transaction, store, transactionPromise };
    }

    /**
     * Check if a key exists in the database
     * @param {string} key - The key to check
     * @returns {Promise<boolean>} Whether the key exists
     */
    async exists(key) {
        clog("exists", key);

        // Check cache first if enabled
        if (this.cacheEnabled && this.cache.has(key)) {
            return true;
        }

        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction('readonly');

        return new Promise((resolve, reject) => {
            const request = store.count(key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result > 0);

            // Also wait for transaction to complete
            transactionPromise.catch(reject);
        });
    }

    /**
     * Get a value by key with caching
     * @param {string} key - The key to retrieve
     * @returns {Promise<any>} The value or null if not found
     */
    async get(key) {
        clog("get", key);

        // Check cache first if enabled
        if (this.cacheEnabled && this.cache.has(key)) {
            return this.cache.get(key);
        }

        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction('readonly');

        return new Promise((resolve, reject) => {
            const request = store.get(key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                const result = request.result === undefined ? null : request.result;

                // Update cache
                if (this.cacheEnabled && result !== null) {
                    this.cache.set(key, result);
                    this._flushCacheIfNeeded();
                }

                resolve(result);
            };

            // Also wait for transaction to complete
            transactionPromise.catch(reject);
        });
    }

    /**
     * Store a value with a key with batching support
     * @param {string} key - The key to store
     * @param {any} value - The value to store
     * @returns {Promise<void>}
     */
    async put(key, value) {
        clog("put", [key, value]);

        // If in batch mode, add to batch operations
        if (this.batchMode) {
            this.batchOperations.push({ type: 'put', key, value });

            // Flush if we've hit batch size
            if (this.batchOperations.length >= this.batchSize) {
                await this._flushBatch();
            }

            return;
        }

        // Update cache immediately for faster subsequent reads
        if (this.cacheEnabled) {
            this.cache.set(key, value);
            this._flushCacheIfNeeded();
        }

        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction('readwrite');

        return new Promise((resolve, reject) => {
            const request = store.put(value, key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();

            // Also wait for transaction to complete
            transactionPromise.catch(reject);
        });
    }

    /**
     * Remove a key-value pair with batching support
     * @param {string} key - The key to remove
     * @returns {Promise<void>}
     */
    async remove(key) {
        clog("remove", key);
        
        // Remove from cache immediately
        if (this.cacheEnabled) {
            this.cache.delete(key);
        }

        // If in batch mode, add to batch operations
        if (this.batchMode) {
            this.batchOperations.push({ type: 'delete', key });

            // Flush if we've hit batch size
            if (this.batchOperations.length >= this.batchSize) {
                await this._flushBatch();
            }

            return;
        }

        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction('readwrite');

        return new Promise((resolve, reject) => {
            const request = store.delete(key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();

            // Also wait for transaction to complete
            transactionPromise.catch(reject);
        });
    }

    /**
     * Get all keys with a specific prefix
     * Useful for the F# file system implementation
     * @param {string} prefix - The prefix to search for
     * @returns {Promise<string[]>} Array of matching keys
     */
    async getKeysWithPrefix(prefix) {
        clog("getKeysWithPrefix", prefix);

        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction('readonly');

        return new Promise((resolve, reject) => {
            const result = [];
            const request = store.openCursor();

            request.onerror = () => reject(request.error);
            request.onsuccess = (event) => {
                const cursor = event.target.result;

                if (cursor) {
                    const key = cursor.key.toString();
                    if (key.startsWith(prefix)) {
                        result.push(key);
                    }
                    cursor.continue();
                } else {
                    resolve(result);
                }
            };

            // Also wait for transaction to complete
            transactionPromise.catch(reject);
        });
    }

    /**
     * Perform multiple operations in a single transaction
     * @param {string} mode - Transaction mode ('readonly' or 'readwrite')
     * @param {Function} operations - Function that receives the store and performs operations
     * @returns {Promise<any>} Result of the operations
     */
    async withTransaction(mode, operations) {
        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction(mode);

        try {
            const result = await operations(store);
            await transactionPromise;
            return result;
        } catch (error) {
            // The transaction will be automatically aborted on error
            throw error;
        }
    }

    /**
     * Get all entries in the database
     * @returns {Promise<Object[]>} Array of all key-value pairs
     * @private
     */
    async _getAllEntries() {
        await this.ensureConnection();

        const { store, transactionPromise } = this.createTransaction('readonly');

        return new Promise((resolve, reject) => {
            const result = [];
            const request = store.openCursor();

            request.onerror = () => reject(request.error);
            request.onsuccess = (event) => {
                const cursor = event.target.result;

                if (cursor) {
                    result.push({
                        key: cursor.key,
                        value: cursor.value
                    });
                    cursor.continue();
                } else {
                    resolve(result);
                }
            };

            // Also wait for transaction to complete
            transactionPromise.catch(reject);
        });
    }


    /**
     * Decode a raw value read from the IndexedDB store into the JSON header
     * text that downstream code can pass to JSON.parse.
     *
     * The current write path (F# side: ByteArray.textEncode) stores values
     * as Uint8Array. For File entries, the value is `<json-header>\0<blob>`,
     * matching the F# read path which slices at the first NUL byte
     * (see KeyedStorageFileSystemAsync.fs:getDecodedEntry).
     *
     * Legacy DBs may still contain string-typed values; pass those through.
     *
     * @param {*} value - The raw value as returned by the cursor (Uint8Array,
     *   string, or other).
     * @returns {string|null} The decoded JSON header text, or null if the
     *   value's shape is unrecognised.
     * @private
     */
    _decodeEntryBytes(value) {
        if (typeof value === 'string') return value;
        if (value instanceof Uint8Array) {
            const nul = value.indexOf(0);
            const header = nul >= 0 ? value.subarray(0, nul) : value;
            return new TextDecoder().decode(header);
        }
        return null;
    }

    /**
     * Check database consistency for file system entries
     * Validates that:
     * 1. All entries are descendants of the root folder (no orphaned entries)
     * 2. All children UIDs exist as entries
     * 3. Root entry exists and is valid
     * @returns {Promise<Object>} Consistency check results
     */
    async checkConsistency() {
        clog("checkConsistency", "starting consistency check");

        const errors = [];
        const warnings = [];
        let totalEntries = 0;
        let validatedEntries = 0;
        
        try {
            // Get all entries from the database
            const allEntries = await this._getAllEntries();
            totalEntries = allEntries.length;

            // Filter for file system entries (those with uid: prefix)
            const fsEntries = allEntries.filter(entry => 
                typeof entry.key === 'string' && entry.key.startsWith('uid:'));

            if (fsEntries.length === 0) {
                warnings.push("No file system entries found in database");
                return {
                    isValid: true,
                    errors,
                    warnings,
                    totalEntries,
                    validatedEntries: 0,
                    summary: "No file system data to validate"
                };
            }

            // Parse entries and create lookup maps
            const uidToEntry = new Map();
            const uidToKey = new Map();
            const parseErrors = [];
            const uidMismatchErrors = [];

            for (const {key, value} of fsEntries) {
                try {
                    const uid = parseInt(key.substring(4)); // Remove "uid:" prefix
                    const decoded = this._decodeEntryBytes(value);
                    if (decoded === null) {
                        parseErrors.push(`Entry ${key} has unrecognised value shape (expected Uint8Array or string)`);
                        continue;
                    }
                    let entry;
                    try {
                        entry = JSON.parse(decoded);
                    } catch (e) {
                        // Include the first ~120 chars of decoded text so the
                        // error is actionable. Pre-fix code surfaced the raw
                        // byte-array stringification, which began "123,34,..."
                        // and tripped on the comma at column 4.
                        const snippet = decoded.length > 120
                            ? decoded.substring(0, 120) + '...'
                            : decoded;
                        parseErrors.push(`Failed to parse entry ${key}: ${e.message} | header: ${snippet}`);
                        continue;
                    }
                    
                    // Basic validation of entry structure
                    if (typeof entry !== 'object' || entry === null) {
                        parseErrors.push(`Entry ${key} is not a valid object`);
                        continue;
                    }
                    
                    if (typeof entry.Uid !== 'number') {
                        parseErrors.push(`Entry ${key} has invalid Uid: ${entry.Uid}`);
                        continue;
                    }
                    
                    if (entry.Uid !== uid) {
                        uidMismatchErrors.push({key, expectedUid: uid, actualUid: entry.Uid, entry});
                    }

                    if (!Array.isArray(entry.Children)) {
                        parseErrors.push(`Entry ${key} has invalid Children array`);
                        continue;
                    }

                    uidToEntry.set(uid, entry);
                    uidToKey.set(uid, key);
                    validatedEntries++;
                } catch (parseError) {
                    // Structural / validation errors (Uid type, Children type).
                    // Decode/parse errors are surfaced above with a snippet.
                    parseErrors.push(`Entry ${key} failed validation: ${parseError.message}`);
                }
            }

            // Report parse errors as errors
            errors.push(...parseErrors);

            // Check 1: Root entry must exist (UID 0)
            const rootEntry = uidToEntry.get(0);
            if (!rootEntry) {
                errors.push("Root entry (UID 0) not found");
                return {
                    isValid: false,
                    errors,
                    warnings,
                    totalEntries,
                    validatedEntries,
                    summary: "Critical error: No root entry found"
                };
            }

            if (rootEntry.Type !== "Folder") {
                errors.push(`Root entry is not a folder (Type=${JSON.stringify(rootEntry.Type)})`);
            }

            // Helper function to build full paths for entries
            const buildPath = (uid, visited = new Set()) => {
                if (visited.has(uid)) {
                    return `[CIRCULAR:${uid}]`; // Prevent infinite loops
                }
                visited.add(uid);

                if (uid === 0) {
                    return "/";
                }

                const entry = uidToEntry.get(uid);
                if (!entry) {
                    return `[MISSING:${uid}]`;
                }

                // Find parent by looking for an entry that has this uid as a child
                for (const [parentUid, parentEntry] of uidToEntry) {
                    if (Array.isArray(parentEntry.Children)) {
                        for (const child of parentEntry.Children) {
                            if (Array.isArray(child) && child.length >= 2 && child[1] === uid) {
                                const parentPath = buildPath(parentUid, new Set(visited));
                                const entryName = entry.Name || 'unnamed';
                                return parentPath === "/" ? `/${entryName}` : `${parentPath}/${entryName}`;
                            }
                        }
                    }
                }
                
                // No parent found - this is an orphan
                return `[ORPHAN]/${entry.Name || 'unnamed'}`;
            };

            // Create a cache for paths to avoid recalculating
            const pathCache = new Map();
            const getPath = (uid) => {
                if (!pathCache.has(uid)) {
                    pathCache.set(uid, buildPath(uid));
                }
                return pathCache.get(uid);
            };

            // Report UID mismatch errors with path information
            for (const {key, expectedUid, actualUid, entry} of uidMismatchErrors) {
                const entryPath = getPath(expectedUid);
                errors.push(`Entry at path "${entryPath}" has mismatched UID: key ${key} suggests ${expectedUid}, but entry has ${actualUid}`);
            }

            // Check 2: All referenced children UIDs must exist
            const referencedUids = new Set();
            const childrenErrors = [];

            for (const [uid, entry] of uidToEntry) {
                if (Array.isArray(entry.Children)) {
                    for (const child of entry.Children) {
                        if (Array.isArray(child) && child.length >= 2) {
                            const [name, childUid] = child;
                            referencedUids.add(childUid);
                            
                            if (!uidToEntry.has(childUid)) {
                                const parentPath = getPath(uid);
                                const childPath = parentPath === "/" ? `/${name}` : `${parentPath}/${name}`;
                                childrenErrors.push(`Entry at path "${parentPath}" (UID ${uid}) references non-existent child UID ${childUid} for "${childPath}"`);
                            }
                        } else {
                            const parentPath = getPath(uid);
                            childrenErrors.push(`Entry at path "${parentPath}" (UID ${uid}) has malformed child entry: ${JSON.stringify(child)}`);
                        }
                    }
                }
            }

            errors.push(...childrenErrors);

            // Check 3: All entries (except root) must be reachable from root (no orphans)
            const reachableUids = new Set();
            const toVisit = [0]; // Start with root UID

            while (toVisit.length > 0) {
                const currentUid = toVisit.pop();
                
                if (reachableUids.has(currentUid)) {
                    continue; // Already visited
                }
                
                reachableUids.add(currentUid);
                const entry = uidToEntry.get(currentUid);
                
                if (entry && Array.isArray(entry.Children)) {
                    for (const child of entry.Children) {
                        if (Array.isArray(child) && child.length >= 2) {
                            const childUid = child[1];
                            if (!reachableUids.has(childUid)) {
                                toVisit.push(childUid);
                            }
                        }
                    }
                }
            }

            // Find orphaned entries
            const orphanedUids = [];
            for (const uid of uidToEntry.keys()) {
                if (uid !== 0 && !reachableUids.has(uid)) {
                    const entry = uidToEntry.get(uid);
                    const entryPath = getPath(uid);
                    orphanedUids.push(`Orphaned entry at path "${entryPath}" (UID ${uid}, name: "${entry.Name || 'unnamed'}") is not reachable from root`);
                }
            }

            errors.push(...orphanedUids);

            // Multi-parent detection: every non-root UID must appear in
            // exactly one parent's Children list. Multiple in-edges mean
            // path lookups can fail when the entry's own Name disagrees
            // with a parent's Children-row label.
            const inEdges = new Map();
            for (const [parentUid, entry] of uidToEntry) {
                for (const row of entry.Children) {
                    if (Array.isArray(row) && row.length >= 2 && typeof row[1] === 'number') {
                        const childUid = row[1];
                        if (!inEdges.has(childUid)) inEdges.set(childUid, []);
                        inEdges.get(childUid).push({ parentUid, name: row[0] });
                    }
                }
            }
            for (const [childUid, refs] of inEdges) {
                if (refs.length < 2) continue;
                const child = uidToEntry.get(childUid);
                const childName = child ? child.Name : '?';
                const refSummary = refs.map(r => `uid:${r.parentUid}/"${r.name}"`).join(', ');
                errors.push(
                    `Entry uid:${childUid} ("${childName}") has ${refs.length} parent references ` +
                    `(should be 1): ${refSummary}`);
            }

            // (root) bookkeeping sanity: NextUid must exceed every existing
            // uid, otherwise the next allocation will collide. Warn-only;
            // the FS continues to function with a stale NextUid (just risks
            // a collision on the next createEntry call).
            const rootBookkeeping = allEntries.find(e => e.key === '(root)');
            // Find max uid via a loop — spread (Math.max(...iter)) blows the
            // call stack at ~10k entries on V8/SpiderMonkey, and real DBs
            // already exceed that (see issue #292).
            let maxUid = -1;
            for (const k of uidToEntry.keys()) {
                if (k > maxUid) maxUid = k;
            }
            if (!rootBookkeeping) {
                warnings.push("(root) bookkeeping key is absent; next-uid allocator will start from 1 and may collide with existing entries");
            } else {
                const decodedRoot = this._decodeEntryBytes(rootBookkeeping.value);
                if (decodedRoot === null) {
                    warnings.push(`(root) bookkeeping value has unrecognised shape (expected Uint8Array or string)`);
                } else {
                    try {
                        const r = JSON.parse(decodedRoot);
                        if (typeof r.NextUid !== 'number') {
                            warnings.push(`(root) bookkeeping has invalid NextUid: ${JSON.stringify(r.NextUid)}`);
                        } else if (r.NextUid <= maxUid) {
                            warnings.push(`(root) NextUid (${r.NextUid}) is not greater than max(uid:N) (${maxUid}); next allocation may collide`);
                        }
                    } catch (e) {
                        warnings.push(`(root) bookkeeping failed to parse: ${e.message}`);
                    }
                }
            }

            // Generate summary statistics
            const folderCount = Array.from(uidToEntry.values())
                .filter(e => e.Type === "Folder").length;
            const fileCount = Array.from(uidToEntry.values())
                .filter(e => e.Type === "File").length;

            const isValid = errors.length === 0;
            const summary = isValid 
                ? `Database is consistent: ${validatedEntries} entries (${folderCount} folders, ${fileCount} files), ${referencedUids.size} child references`
                : `Found ${errors.length} consistency errors in ${validatedEntries} entries`;

            return {
                isValid,
                errors,
                warnings,
                totalEntries,
                validatedEntries,
                reachableEntries: reachableUids.size,
                folderCount,
                fileCount,
                childReferences: referencedUids.size,
                summary
            };

        } catch (error) {
            errors.push(`Consistency check failed: ${error.message}`);
            return {
                isValid: false,
                errors,
                warnings,
                totalEntries,
                validatedEntries,
                summary: `Consistency check failed due to error: ${error.message}`
            };
        }
    }

    /**
     * Run consistency check and log results to console
     * Useful for debugging and development
     * @returns {Promise<boolean>} True if database is consistent
     */
    async logConsistencyCheck() {
        const result = await this.checkConsistency();
        
        console.group('Database Consistency Check');
        console.log(`Status: ${result.isValid ? 'Valid' : 'Invalid'}`);
        console.log(`Summary: ${result.summary}`);
        
        if (result.errors && result.errors.length > 0) {
            console.group('Errors:');
            result.errors.forEach(error => console.error(error));
            console.groupEnd();
        }
        
        if (result.warnings && result.warnings.length > 0) {
            console.group('Warnings:');
            result.warnings.forEach(warning => console.warn(warning));
            console.groupEnd();
        }
        
        if (result.isValid) {
            console.log(`Statistics:`);
            console.log(`  - Total entries: ${result.totalEntries}`);
            console.log(`  - Validated entries: ${result.validatedEntries}`);
            console.log(`  - Reachable entries: ${result.reachableEntries}`);
            console.log(`  - Folders: ${result.folderCount}`);
            console.log(`  - Files: ${result.fileCount}`);
            console.log(`  - Child references: ${result.childReferences}`);
        }
        
        console.groupEnd();
        
        return result.isValid;
    }

    /**
     * Encode an entry into the byte format the F# read path expects.
     *
     * Header bytes are JSON encoded via TextEncoder. If `originalValue` is
     * a Uint8Array carrying a `\0`+blob tail (File entry), the original
     * tail is preserved verbatim — `<new-header><\0><original-tail>`.
     * Folders and legacy string-stored values produce header-only output.
     *
     * Blob preservation matters: the consistency-check tests on a real DB
     * showed File entries with arbitrary binary content (e.g. typed-array
     * payloads). Re-encoding by decoding+re-encoding would corrupt
     * non-UTF-8 bytes.
     *
     * @param {object} entry - Decoded entry; JSON-stringified for the header.
     * @param {Uint8Array|string|null} originalValue - Pre-edit stored value;
     *   used to preserve any blob tail.
     * @returns {Uint8Array}
     * @private
     */
    _encodeEntryBytes(entry, originalValue) {
        const headerBytes = new TextEncoder().encode(JSON.stringify(entry));
        if (originalValue instanceof Uint8Array) {
            const nul = originalValue.indexOf(0);
            if (nul >= 0) {
                const tail = originalValue.subarray(nul + 1);
                const out = new Uint8Array(headerBytes.length + 1 + tail.length);
                out.set(headerBytes, 0);
                out[headerBytes.length] = 0;
                out.set(tail, headerBytes.length + 1);
                return out;
            }
        }
        return headerBytes;
    }

    /**
     * Update `entry.Meta`'s ModifiedAt row in place. Creates the Meta
     * array (with CreatedAt + ModifiedAt) if absent, and appends a
     * ModifiedAt row if the array exists but doesn't carry one. Never
     * writes a top-level Modified field — that was a legacy schema slot.
     *
     * @param {object} entry
     * @private
     */
    _setMetaModified(entry) {
        const now = new Date().toISOString();
        if (!Array.isArray(entry.Meta)) {
            entry.Meta = [['CreatedAt', now], ['ModifiedAt', now]];
            return;
        }
        let found = false;
        for (let i = 0; i < entry.Meta.length; i++) {
            const row = entry.Meta[i];
            if (Array.isArray(row) && row.length >= 2 && row[0] === 'ModifiedAt') {
                entry.Meta[i] = ['ModifiedAt', now];
                found = true;
                break;
            }
        }
        if (!found) {
            entry.Meta.push(['ModifiedAt', now]);
        }
    }

    /**
     * Decode + validate uid:N entries from the store. Skips entries that
     * fail decoding/parsing/validation — `checkConsistency` owns reporting
     * those. Returns three parallel maps so callers can both read the
     * parsed entry and round-trip the original bytes for blob preservation.
     *
     * @returns {Promise<{uidToEntry: Map, uidToKey: Map, uidToOrig: Map}>}
     * @private
     */
    async _loadEntryMaps() {
        const allEntries = await this._getAllEntries();
        const fsEntries = allEntries.filter(e =>
            typeof e.key === 'string' && e.key.startsWith('uid:'));

        const uidToEntry = new Map();
        const uidToKey = new Map();
        const uidToOrig = new Map();
        for (const { key, value } of fsEntries) {
            try {
                const uid = parseInt(key.substring(4));
                if (!Number.isFinite(uid)) continue;
                const decoded = this._decodeEntryBytes(value);
                if (decoded === null) continue;
                const entry = JSON.parse(decoded);
                if (typeof entry !== 'object' || entry === null) continue;
                if (typeof entry.Uid !== 'number') continue;
                if (!Array.isArray(entry.Children)) continue;
                uidToEntry.set(uid, entry);
                uidToKey.set(uid, key);
                uidToOrig.set(uid, value);
            } catch (_) {
                // Skip — checkConsistency owns parse-error reporting.
            }
        }
        return { uidToEntry, uidToKey, uidToOrig, allEntries };
    }

    /**
     * Fix dangling child references by removing pointers to non-existent
     * UIDs and dropping malformed child rows. Idempotent: re-running on a
     * clean store returns 0 and writes nothing.
     *
     * Writes Uint8Array values, preserves File blob tails, updates
     * Meta.ModifiedAt (never top-level Modified).
     *
     * @returns {Promise<number>} Number of bad child rows removed
     */
    async fixDanglingReferences() {
        console.log('Starting dangling reference cleanup...');
        let fixedCount = 0;

        try {
            const { uidToEntry, uidToKey, uidToOrig } = await this._loadEntryMaps();
            if (uidToEntry.size === 0) {
                console.log('No file system entries found to process.');
                return 0;
            }

            const validUids = new Set(uidToEntry.keys());

            // Multi-parent detection: build inverse map and pick a canonical
            // keeper for each UID with >1 in-edges. Heuristic: keep the row
            // whose name matches the child's own Name field (the canonical
            // reference). If ambiguous (none match, or multiple match),
            // pick the lowest-UID parent deterministically — the entry
            // stays reachable either way; the duplicate rows are removed
            // by the same per-parent filter that handles dangling refs.
            const parentsOf = new Map();
            for (const [parentUid, entry] of uidToEntry) {
                for (const row of entry.Children) {
                    if (Array.isArray(row) && row.length >= 2 && typeof row[1] === 'number') {
                        const childUid = row[1];
                        if (!parentsOf.has(childUid)) parentsOf.set(childUid, []);
                        parentsOf.get(childUid).push({ parentUid, name: row[0] });
                    }
                }
            }
            // Reachability from root via the (current, pre-repair) Children
            // edges. The keeper-pick prefers reachable parents over
            // orphan-subtree parents — a keeper inside an orphan subtree
            // would silently orphan the child, fresh corruption.
            const reachable = new Set();
            (() => {
                const stack = [0];
                while (stack.length > 0) {
                    const u = stack.pop();
                    if (reachable.has(u)) continue;
                    reachable.add(u);
                    const e = uidToEntry.get(u);
                    if (e && Array.isArray(e.Children)) {
                        for (const row of e.Children) {
                            if (Array.isArray(row) && row.length >= 2 && typeof row[1] === 'number') {
                                if (uidToEntry.has(row[1])) stack.push(row[1]);
                            }
                        }
                    }
                }
            })();

            // Keeper-pick priority:
            //  1. Reachable parent whose row name matches child.Name (canonical + reachable).
            //  2. Reachable parent (any name; lowest-UID deterministic).
            //  3. Any parent whose row name matches child.Name (canonical, even if orphaned).
            //  4. Any parent (lowest-UID deterministic).
            // If the keeper falls into class 3 or 4, the child remains in an
            // orphan subtree post-repair — fixOrphanedEntries will pick it up.
            const keeperOf = new Map();   // childUid -> {parentUid, name} (only when refs > 1)
            for (const [childUid, refs] of parentsOf) {
                if (refs.length < 2) continue;
                const child = uidToEntry.get(childUid);
                if (!child) continue;
                const pools = [
                    refs.filter(r => reachable.has(r.parentUid) && r.name === child.Name),
                    refs.filter(r => reachable.has(r.parentUid)),
                    refs.filter(r => r.name === child.Name),
                    refs,
                ];
                const pool = pools.find(p => p.length >= 1) || refs;
                keeperOf.set(childUid, pool.slice().sort((a, b) => a.parentUid - b.parentUid)[0]);
            }

            // Per-parent filter: drops malformed rows, dangling refs, and
            // duplicate-parent rows (keeper retained; others removed).
            for (const [uid, entry] of uidToEntry) {
                if (!entry.Children.length) continue;
                const before = entry.Children;
                const after = before.filter(row => {
                    if (!Array.isArray(row) || row.length < 2) {
                        console.log(`Removing malformed child row from uid:${uid}: ${JSON.stringify(row)}`);
                        return false;
                    }
                    const [name, childUid] = row;
                    if (!validUids.has(childUid)) {
                        console.log(`Removing dangling reference: uid:${uid}/${name} -> uid:${childUid}`);
                        return false;
                    }
                    const keeper = keeperOf.get(childUid);
                    if (keeper && !(keeper.parentUid === uid && keeper.name === name)) {
                        console.log(
                            `Removing duplicate parent ref: uid:${uid}/"${name}" -> uid:${childUid} ` +
                            `(keeper: uid:${keeper.parentUid}/"${keeper.name}")`);
                        return false;
                    }
                    return true;
                });
                const removed = before.length - after.length;
                if (removed > 0) {
                    fixedCount += removed;
                    const updated = { ...entry, Children: after };
                    this._setMetaModified(updated);
                    const bytes = this._encodeEntryBytes(updated, uidToOrig.get(uid));
                    await this.put(uidToKey.get(uid), bytes);
                    uidToEntry.set(uid, updated);
                    uidToOrig.set(uid, bytes);
                }
            }

            console.log(`Dangling reference cleanup completed. Removed ${fixedCount} child rows.`);
            return fixedCount;
        } catch (error) {
            console.error('Error during dangling reference cleanup:', error);
            throw error;
        }
    }

    /**
     * Move entries not reachable from root into a `/orphans` folder.
     * Idempotent: re-running on a clean store returns 0, makes no writes,
     * and does NOT create a duplicate /orphans folder.
     *
     * Ordering note: if a DB has both dangling references AND orphans,
     * run `fixDanglingReferences` first. The legacy implementation
     * accidentally marked dangling UIDs as "reachable" (its BFS pushed
     * UIDs without verifying they existed); the rewrite's BFS only walks
     * real entries, so an orphaned entry that was previously hidden
     * behind a dangling reference is now correctly identified.
     *
     * Coordinates UID allocation with the F# `Root.NextUid` counter in the
     * `(root)` bookkeeping key. Allocates the /orphans folder UID above
     * `max(NextUid, max(uid:N))`, then writes back `(root).NextUid` so the
     * F# layer's next allocation skips ours. Refuses to run if `(root)`
     * is absent or unparseable — without it the JS-side allocator can't
     * safely coordinate with future F# writes.
     *
     * @returns {Promise<number>} Number of orphans moved
     */
    async fixOrphanedEntries() {
        console.log('Starting orphaned entries cleanup...');
        let movedCount = 0;

        try {
            const { uidToEntry, uidToKey, uidToOrig, allEntries } = await this._loadEntryMaps();
            if (uidToEntry.size === 0) {
                console.log('No file system entries found to process.');
                return 0;
            }

            // Coordinate with the F# Root.NextUid counter. Without it we
            // cannot safely allocate a UID for /orphans — a future F#
            // createEntry call could overwrite our folder.
            const rootBookkeeping = allEntries.find(e => e.key === '(root)');
            if (!rootBookkeeping) {
                console.error('(root) bookkeeping key is absent. Cannot allocate /orphans UID safely. Refusing to run fixOrphanedEntries.');
                return 0;
            }
            let nextUid;
            try {
                const decoded = this._decodeEntryBytes(rootBookkeeping.value);
                if (decoded === null) throw new Error('unrecognised value shape');
                const parsed = JSON.parse(decoded);
                if (typeof parsed.NextUid !== 'number') throw new Error('invalid NextUid');
                nextUid = parsed.NextUid;
            } catch (e) {
                console.error(`(root) bookkeeping failed to parse: ${e.message}. Refusing to run fixOrphanedEntries.`);
                return 0;
            }

            const rootEntry = uidToEntry.get(0);
            if (!rootEntry) {
                console.error('Root entry (uid:0) not found. Cannot move orphans.');
                return 0;
            }

            // BFS reachability from root via Children edges.
            const reachable = new Set();
            const stack = [0];
            while (stack.length > 0) {
                const u = stack.pop();
                if (reachable.has(u)) continue;
                reachable.add(u);
                const e = uidToEntry.get(u);
                if (e && Array.isArray(e.Children)) {
                    for (const c of e.Children) {
                        if (Array.isArray(c) && c.length >= 2 && uidToEntry.has(c[1])) {
                            stack.push(c[1]);
                        }
                    }
                }
            }

            const orphanUids = [];
            for (const u of uidToEntry.keys()) {
                if (u !== 0 && !reachable.has(u)) orphanUids.push(u);
            }
            if (orphanUids.length === 0) {
                console.log('No orphaned entries found.');
                return 0;
            }
            console.log(`Found ${orphanUids.length} orphan(s) to move.`);

            // Find existing /orphans folder under root, if any.
            let orphansUid = null;
            let orphansEntry = null;
            for (const c of rootEntry.Children) {
                if (Array.isArray(c) && c.length >= 2 && c[0] === 'orphans') {
                    const cand = uidToEntry.get(c[1]);
                    if (cand && cand.Type === 'Folder') {
                        orphansUid = c[1];
                        orphansEntry = cand;
                        break;
                    }
                }
            }

            // Compute max(uid:N) via loop (no spread — see #292 for why).
            let maxUid = -1;
            for (const u of uidToEntry.keys()) {
                if (u > maxUid) maxUid = u;
            }

            if (!orphansEntry) {
                // Allocate /orphans UID above max(NextUid, max(uid)+1) so
                // the F# layer's next allocation skips it.
                orphansUid = Math.max(nextUid, maxUid + 1);
                const now = new Date().toISOString();
                orphansEntry = {
                    Type: 'Folder',
                    Name: 'orphans',
                    Uid: orphansUid,
                    Children: [],
                    Meta: [['CreatedAt', now], ['ModifiedAt', now]],
                };
                // Order matters for crash-safety: write /orphans entry
                // first, then root.Children update. A crash between the
                // two leaves an unreferenced /orphans entry (which a
                // re-run will turn into an orphan and re-attach), rather
                // than a root.Children pointing at a non-existent UID
                // (which would be a fresh dangling reference).
                await this.put(`uid:${orphansUid}`,
                    this._encodeEntryBytes(orphansEntry, null));
                const updatedRoot = {
                    ...rootEntry,
                    Children: [...rootEntry.Children, ['orphans', orphansUid]],
                };
                this._setMetaModified(updatedRoot);
                await this.put(uidToKey.get(0),
                    this._encodeEntryBytes(updatedRoot, uidToOrig.get(0)));
                // Bump (root).NextUid so F# skips orphansUid. Preserve any
                // other fields the F# Root record may carry (or gain) by
                // round-tripping the existing object — write only the
                // NextUid override.
                let existingRoot = {};
                try {
                    const decoded = this._decodeEntryBytes(rootBookkeeping.value);
                    if (decoded !== null) existingRoot = JSON.parse(decoded) || {};
                } catch (_) {
                    // Already validated above; this can't fail. Defensive only.
                    existingRoot = {};
                }
                const newBookkeeping = {
                    ...existingRoot,
                    NextUid: Math.max(nextUid, orphansUid + 1),
                };
                await this.put('(root)',
                    this._encodeEntryBytes(newBookkeeping, rootBookkeeping.value));

                uidToEntry.set(orphansUid, orphansEntry);
                uidToKey.set(orphansUid, `uid:${orphansUid}`);
                uidToOrig.set(orphansUid, null);
                console.log(`Created /orphans folder at uid:${orphansUid}.`);
            }

            // Move each orphan, renaming to avoid name collisions in /orphans.
            const existingNames = new Set();
            for (const c of orphansEntry.Children) {
                if (Array.isArray(c) && c.length >= 2) existingNames.add(c[0]);
            }
            const generateUniqueName = (base) => {
                if (!existingNames.has(base)) return base;
                let i = 1;
                while (existingNames.has(`${base}_${i}`)) i++;
                return `${base}_${i}`;
            };

            const newChildren = [...orphansEntry.Children];
            for (const orphanUid of orphanUids) {
                const orphan = uidToEntry.get(orphanUid);
                if (!orphan) continue;
                const base = orphan.Name || `unnamed_${orphanUid}`;
                const unique = generateUniqueName(base);
                existingNames.add(unique);

                if (unique !== orphan.Name) {
                    const renamed = { ...orphan, Name: unique };
                    this._setMetaModified(renamed);
                    await this.put(uidToKey.get(orphanUid),
                        this._encodeEntryBytes(renamed, uidToOrig.get(orphanUid)));
                }
                newChildren.push([unique, orphanUid]);
                movedCount++;
                console.log(`Moved orphan uid:${orphanUid} ("${orphan.Name}") to /orphans as "${unique}".`);
            }

            // Update /orphans Children in one put.
            const updatedOrphans = { ...orphansEntry, Children: newChildren };
            this._setMetaModified(updatedOrphans);
            await this.put(uidToKey.get(orphansUid),
                this._encodeEntryBytes(updatedOrphans, uidToOrig.get(orphansUid)));

            console.log(`Orphaned entries cleanup completed. Moved ${movedCount} entries.`);
            return movedCount;
        } catch (error) {
            console.error('Error during orphaned entries cleanup:', error);
            throw error;
        }
    }
}

/**
 * Create a new KeyedStorageIndexedDB instance
 * @param {string} rootKey - The database name
 * @returns {Object} A new instance with F# compatible interface
 */
export function createKeyedStorageIndexedDB(rootKey) {
    const db = new KeyedStorageIndexedDB(rootKey, "keyValues", 1);

    // Return an object that matches the F# IKeyedStorageAsync interface
    return {
        Exists: (key) => db.exists(key),
        Get: (key) => db.get(key),
        GetAll: () => db._getAllEntries(),
        Put: (key, value) => db.put(key, value),
        Remove: (key) => db.remove(key),

        // Additional methods that could be useful for optimizing the F# code
        BeginBatch: () => db.beginBatch(),
        CommitBatch: () => db.commitBatch(),
        GetKeysWithPrefix: (prefix) => db.getKeysWithPrefix(prefix),
        Close: () => db.close(),

        // Consistency checking for file system integrity
        CheckConsistency: () => db.checkConsistency(),
        FixDanglingReferences: () => db.fixDanglingReferences(),
        FixOrphanedEntries: () => db.fixOrphanedEntries(),
        LogConsistencyCheck: () => db.logConsistencyCheck()
    };
}