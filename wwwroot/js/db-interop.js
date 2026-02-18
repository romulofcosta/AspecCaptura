window.dbInterop = {
    db: null,
    dbName: 'PwaInventoryDB',
    dbVersion: 4,

    init: async function () {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(this.dbName, this.dbVersion);

            request.onerror = (event) => {
                console.error('DB Error:', event.target.error);
                reject(event.target.error);
            };

            request.onupgradeneeded = (event) => {
                const db = event.target.result;
                const transaction = event.target.transaction;

                // ... (existing code for users, states, cities, units) ...

                // 3. Items Store (Inventory)
                let itemsStore;
                if (!db.objectStoreNames.contains('items')) {
                    itemsStore = db.createObjectStore('items', { keyPath: 'id' });
                    itemsStore.createIndex('name', 'name', { unique: false });
                    itemsStore.createIndex('code', 'code', { unique: false });
                    itemsStore.createIndex('category', 'category', { unique: false });
                    itemsStore.createIndex('timestamp', 'timestamp', { unique: false });
                    itemsStore.createIndex('idUO', 'idUO', { unique: false });
                    itemsStore.createIndex('synced', 'synced', { unique: false });
                    itemsStore.createIndex('createdBy', 'createdBy', { unique: false });
                } else {
                    itemsStore = transaction.objectStore('items');
                    if (!itemsStore.indexNames.contains('synced')) {
                        itemsStore.createIndex('synced', 'synced', { unique: false });
                    }
                    if (!itemsStore.indexNames.contains('createdBy')) {
                        itemsStore.createIndex('createdBy', 'createdBy', { unique: false });
                    }
                    if (!itemsStore.indexNames.contains('idUO')) {
                        itemsStore.createIndex('idUO', 'idUO', { unique: false });
                    }
                }
            };

            request.onsuccess = (event) => {
                this.db = event.target.result;
                console.log('DB Initialized');
                resolve();
            };
        });
    },

    get: async function (storeName, key) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.get(key);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    getAll: async function (storeName) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.getAll();
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    getAllKeys: async function (storeName) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const request = store.getAllKeys();
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    add: async function (storeName, item) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            // .put allows updating or adding (upsert). .add enforces new key if key is provided.
            // Using .put is safer for "Add or Update" if ID exists.
            const request = store.put(item);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    update: async function (storeName, item) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            return this.add(storeName, item); // Same as put
        });
    },

    delete: async function (storeName, key) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.delete(key);
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    },

    getFromIndex: async function (storeName, indexName, value) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const index = store.index(indexName);
            const request = index.get(value);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    getAllFromIndex: async function (storeName, indexName, value) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const index = store.index(indexName);
            const request = index.getAll(value);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    getAllKeysFromIndex: async function (storeName, indexName, value) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readonly');
            const store = transaction.objectStore(storeName);
            const index = store.index(indexName);
            const request = index.getAllKeys(value);
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    getItemsByUO: async function (idUO) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction(['items'], 'readonly');
            const store = transaction.objectStore('items');
            const index = store.index('idUO');
            const request = index.getAll(idUO);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    isInitialized: function () {
        return this.db !== null;
    }
};