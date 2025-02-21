export class KeyedStorageIndexedDB {
    constructor(dbName /*= 'keyValueStore'*/, storeName /*= 'keyValuePairs'*/, version = 1) {
        this.dbName = dbName;
        this.storeName = storeName;
        this.version = version;
        this.db = null;
    }

    async Init() {
        if (this.db) return;

        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, this.version);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                this.db = request.result;
                resolve();
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                if (!db.objectStoreNames.contains(this.storeName)) {
                    db.createObjectStore(this.storeName);
                }
            };
        });
    }

    async Exists(key) {
        await this.Init();
        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction([this.storeName], 'readonly');
            const store = transaction.objectStore(this.storeName);
            const request = store.count(key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve(request.result > 0);
        });
    }

    async Get(key) {
        await this.Init();
        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction([this.storeName], 'readonly');
            const store = transaction.objectStore(this.storeName);
            const request = store.get(key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => {
                if (request.result === undefined) {
                    resolve(null)
//                    reject(new Error(`Key '${key}' not found`));
                } else {
                    resolve(request.result);
                }
            };
        });
    }

    async Put(key, value) {
        await this.Init();
        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction([this.storeName], 'readwrite');
            const store = transaction.objectStore(this.storeName);
            const request = store.put(value, key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    }

    async Remove(key) {
        await this.Init();
        return new Promise((resolve, reject) => {
            const transaction = this.db.transaction([this.storeName], 'readwrite');
            const store = transaction.objectStore(this.storeName);
            const request = store.delete(key);

            request.onerror = () => reject(request.error);
            request.onsuccess = () => resolve();
        });
    }
}

export function createKeyedStorageIndexedDB( rootKey ) { return new KeyedStorageIndexedDB(rootKey, "keyValues", 1); }