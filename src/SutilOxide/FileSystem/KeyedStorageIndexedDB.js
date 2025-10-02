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
                    const entry = JSON.parse(value);
                    
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
                    parseErrors.push(`Failed to parse entry ${key}: ${parseError.message}`);
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

            if (rootEntry.Type !== "Folder" && rootEntry.Type !== 1) { // 1 is Folder enum value
                errors.push("Root entry is not a folder");
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

            // Generate summary statistics
            const folderCount = Array.from(uidToEntry.values())
                .filter(e => e.Type === "Folder" || e.Type === 1).length;
            const fileCount = Array.from(uidToEntry.values())
                .filter(e => e.Type === "File" || e.Type === 0).length;

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
     * Fix dangling child references by removing references to non-existent UIDs
     * Also repairs corrupted Type fields in root entry first
     * @returns {Promise<number>} Number of dangling references removed
     */
    async fixDanglingReferences() {
        console.log('Starting dangling reference cleanup...');
        let fixedCount = 0;
        
        try {
            // First, check and repair root entry Type field if corrupted
            const rootEntry = await this.get('uid:0');
            if (rootEntry) {
                try {
                    const entry = JSON.parse(rootEntry);
                    if (typeof entry.Type === 'number') {
                        let stringType;
                        switch (entry.Type) {
                            case 0:
                                stringType = 'File';
                                break;
                            case 1:
                                stringType = 'Folder';
                                break;
                            default:
                                console.warn(`Unknown numeric Type value for root: ${entry.Type}`);
                                stringType = 'Folder'; // Default to Folder for root
                        }
                        
                        const repairedRoot = {
                            ...entry,
                            Type: stringType,
                            Modified: new Date().toISOString()
                        };
                        
                        await this.put('uid:0', JSON.stringify(repairedRoot));
                        console.log(`Repaired root entry Type field: ${entry.Type} -> "${stringType}"`);
                    }
                } catch (parseError) {
                    console.warn(`Failed to parse root entry: ${parseError.message}`);
                }
            }
            
            // Get all entries from the database
            const allEntries = await this._getAllEntries();
            
            // Filter for file system entries (those with uid: prefix)
            const fsEntries = allEntries.filter(entry => 
                typeof entry.key === 'string' && entry.key.startsWith('uid:'));

            if (fsEntries.length === 0) {
                console.log('No file system entries found to process.');
                return 0;
            }

            // Parse entries and create lookup maps
            const uidToEntry = new Map();
            const uidToKey = new Map();

            for (const {key, value} of fsEntries) {
                try {
                    const uid = parseInt(key.substring(4)); // Remove "uid:" prefix
                    const entry = JSON.parse(value);
                    
                    // Basic validation
                    if (typeof entry === 'object' && entry !== null && 
                        typeof entry.Uid === 'number' && Array.isArray(entry.Children)) {
                        uidToEntry.set(uid, entry);
                        uidToKey.set(uid, key);
                    }
                } catch (parseError) {
                    console.warn(`Failed to parse entry ${key}: ${parseError.message}`);
                }
            }

            // Create set of valid UIDs
            const validUids = new Set(uidToEntry.keys());
            
            // Check each entry with children for dangling references
            for (const [uid, entry] of uidToEntry) {
                if (Array.isArray(entry.Children) && entry.Children.length > 0) {
                    const originalChildCount = entry.Children.length;
                    const validChildren = entry.Children.filter(child => {
                        if (Array.isArray(child) && child.length >= 2) {
                            const [name, childUid] = child;
                            if (validUids.has(childUid)) {
                                return true;
                            } else {
                                console.log(`Removing dangling reference: UID ${uid}/${name} -> UID ${childUid}`);
                                fixedCount++;
                                return false;
                            }
                        } else {
                            console.log(`Removing malformed child entry: UID ${uid} -> ${JSON.stringify(child)}`);
                            fixedCount++;
                            return false;
                        }
                    });
                    
                    // Update entry if we removed any children
                    if (validChildren.length !== originalChildCount) {
                        const updatedEntry = { ...entry, Children: validChildren };
                        const key = uidToKey.get(uid);
                        const updatedValue = JSON.stringify(updatedEntry);
                        await this.put(key, updatedValue);
                    }
                }
            }
            
            console.log(`Dangling reference cleanup completed. Fixed ${fixedCount} references.`);
            return fixedCount;
        } catch (error) {
            console.error('Error during dangling reference cleanup:', error);
            throw error;
        }
    }

    /**
     * Move orphaned entries (entries not reachable from root) to a root "orphans" folder
     * Also repairs any corrupted Type fields (string -> numeric)
     * @returns {Promise<number>} Number of orphaned entries moved
     */
    async fixOrphanedEntries() {
        console.log('Starting orphaned entries cleanup...');
        let movedCount = 0;
        
        try {
            // Get all entries from the database
            const allEntries = await this._getAllEntries();
            
            // Filter for file system entries (those with uid: prefix)
            const fsEntries = allEntries.filter(entry => 
                typeof entry.key === 'string' && entry.key.startsWith('uid:'));

            if (fsEntries.length === 0) {
                console.log('No file system entries found to process.');
                return 0;
            }

            // First pass: Repair any corrupted Type fields
            let repairedCount = 0;
            for (const {key, value} of fsEntries) {
                try {
                    const entry = JSON.parse(value);
                    
                    // Check if entry has corrupted Type field (number instead of string)
                    if (typeof entry.Type === 'number') {
                        let stringType;
                        
                        // Convert numeric Type to string enum value
                        switch (entry.Type) {
                            case 0:
                                stringType = 'File';
                                break;
                            case 1:
                                stringType = 'Folder';
                                break;
                            default:
                                console.warn(`Unknown numeric Type value: ${entry.Type} for entry ${key}`);
                                continue;
                        }
                        
                        // Update the entry with correct string Type
                        const repairedEntry = {
                            ...entry,
                            Type: stringType,
                            Modified: new Date().toISOString()
                        };
                        
                        await this.put(key, JSON.stringify(repairedEntry));
                        repairedCount++;
                        
                        console.log(`Repaired Type field for ${key}: ${entry.Type} -> "${stringType}"`);
                    }
                } catch (parseError) {
                    console.warn(`Failed to parse entry ${key} during Type repair: ${parseError.message}`);
                }
            }
            
            if (repairedCount > 0) {
                console.log(`Repaired ${repairedCount} corrupted Type fields.`);
                // Re-fetch entries after repairs
                const updatedEntries = await this._getAllEntries();
                const updatedFsEntries = updatedEntries.filter(entry => 
                    typeof entry.key === 'string' && entry.key.startsWith('uid:'));
                fsEntries.length = 0;
                fsEntries.push(...updatedFsEntries);
            }

            // Parse entries and create lookup maps
            const uidToEntry = new Map();
            const uidToKey = new Map();

            for (const {key, value} of fsEntries) {
                try {
                    const uid = parseInt(key.substring(4)); // Remove "uid:" prefix
                    const entry = JSON.parse(value);
                    
                    // Basic validation
                    if (typeof entry === 'object' && entry !== null && 
                        typeof entry.Uid === 'number' && Array.isArray(entry.Children)) {
                        uidToEntry.set(uid, entry);
                        uidToKey.set(uid, key);
                    }
                } catch (parseError) {
                    console.warn(`Failed to parse entry ${key}: ${parseError.message}`);
                }
            }

            // Check if root exists
            const rootEntry = uidToEntry.get(0);
            if (!rootEntry) {
                console.error('Root entry (UID 0) not found. Cannot fix orphaned entries.');
                return 0;
            }

            // Find all reachable entries from root
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

            // Find orphaned entries (excluding root)
            const orphanedUids = [];
            for (const uid of uidToEntry.keys()) {
                if (uid !== 0 && !reachableUids.has(uid)) {
                    orphanedUids.push(uid);
                }
            }

            if (orphanedUids.length === 0) {
                console.log('No orphaned entries found.');
                return 0;
            }

            console.log(`Found ${orphanedUids.length} orphaned entries to move.`);

            // Find or create "orphans" folder in root
            let orphansFolder = null;
            let orphansFolderUid = null;

            // Check if "orphans" folder already exists in root
            for (const child of rootEntry.Children) {
                if (Array.isArray(child) && child.length >= 2) {
                    const [name, childUid] = child;
                    if (name === 'orphans') {
                        const childEntry = uidToEntry.get(childUid);
                        if (childEntry && (childEntry.Type === 'Folder' || childEntry.Type === 1)) {
                            orphansFolder = childEntry;
                            orphansFolderUid = childUid;
                            break;
                        }
                    }
                }
            }

            // Create orphans folder if it doesn't exist
            if (!orphansFolder) {
                // Find the next available UID
                const usedUids = new Set(uidToEntry.keys());
                let nextUid = 1;
                while (usedUids.has(nextUid)) {
                    nextUid++;
                }

                orphansFolderUid = nextUid;
                orphansFolder = {
                    Uid: orphansFolderUid,
                    Name: 'orphans',
                    Type: 'Folder', // String value as expected by F#
                    Children: [],
                    Created: new Date().toISOString(),
                    Modified: new Date().toISOString()
                };

                // Add to root's children
                const updatedRoot = { 
                    ...rootEntry, 
                    Children: [...rootEntry.Children, ['orphans', orphansFolderUid]],
                    Modified: new Date().toISOString()
                };

                // Save orphans folder and updated root
                const orphansKey = `uid:${orphansFolderUid}`;
                await this.put(orphansKey, JSON.stringify(orphansFolder));
                await this.put(uidToKey.get(0), JSON.stringify(updatedRoot));
                
                uidToEntry.set(orphansFolderUid, orphansFolder);
                uidToKey.set(orphansFolderUid, orphansKey);
                
                console.log(`Created orphans folder with UID ${orphansFolderUid}`);
            }

            // Helper function to generate unique names
            const generateUniqueName = (baseName, existingNames) => {
                if (!existingNames.has(baseName)) {
                    return baseName;
                }
                
                let counter = 1;
                let uniqueName = `${baseName}_${counter}`;
                while (existingNames.has(uniqueName)) {
                    counter++;
                    uniqueName = `${baseName}_${counter}`;
                }
                return uniqueName;
            };

            // Get existing names in orphans folder
            const existingNames = new Set();
            for (const child of orphansFolder.Children) {
                if (Array.isArray(child) && child.length >= 2) {
                    existingNames.add(child[0]);
                }
            }

            // Move each orphaned entry to orphans folder
            const newOrphansChildren = [...orphansFolder.Children];

            for (const orphanUid of orphanedUids) {
                const orphanEntry = uidToEntry.get(orphanUid);
                if (orphanEntry) {
                    const baseName = orphanEntry.Name || `unnamed_${orphanUid}`;
                    const uniqueName = generateUniqueName(baseName, existingNames);
                    existingNames.add(uniqueName);
                    
                    // Update the orphan's name if it was changed for uniqueness
                    if (uniqueName !== orphanEntry.Name) {
                        const updatedOrphan = {
                            ...orphanEntry,
                            Name: uniqueName,
                            Modified: new Date().toISOString()
                        };
                        await this.put(uidToKey.get(orphanUid), JSON.stringify(updatedOrphan));
                    }
                    
                    // Add to orphans folder
                    newOrphansChildren.push([uniqueName, orphanUid]);
                    movedCount++;
                    
                    console.log(`Moved orphaned entry UID ${orphanUid} (${orphanEntry.Name}) to orphans folder as "${uniqueName}"`);
                }
            }

            // Update orphans folder with new children
            const updatedOrphansFolder = {
                ...orphansFolder,
                Children: newOrphansChildren,
                Modified: new Date().toISOString()
            };
            
            await this.put(uidToKey.get(orphansFolderUid), JSON.stringify(updatedOrphansFolder));
            
            console.log(`Orphaned entries cleanup completed. Moved ${movedCount} entries to orphans folder.`);
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