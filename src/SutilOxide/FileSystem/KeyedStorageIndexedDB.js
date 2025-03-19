/**
 * Optimized IndexedDB implementation for the SutilOxide file system
 * Addresses performance issues with transaction management, connection handling,
 * and resource cleanup that were causing browser lockups.
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
        Put: (key, value) => db.put(key, value),
        Remove: (key) => db.remove(key),

        // Additional methods that could be useful for optimizing the F# code
        BeginBatch: () => db.beginBatch(),
        CommitBatch: () => db.commitBatch(),
        GetKeysWithPrefix: (prefix) => db.getKeysWithPrefix(prefix),
        Close: () => db.close()
    };
}