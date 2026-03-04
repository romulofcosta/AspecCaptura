window.dbInterop = {
    db: null,
    dbName: 'PwaInventoryDB',
    dbVersion: 6,

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

                // 4. Patrimonio Store (Lookup)
                if (!db.objectStoreNames.contains('patrimonio')) {
                    const patrimonioStore = db.createObjectStore('patrimonio', { keyPath: 'idPatomb' });
                    patrimonioStore.createIndex('nutomb', 'nutomb', { unique: false });
                }

                if (!db.objectStoreNames.contains('patrimonio_staging')) {
                    const staging = db.createObjectStore('patrimonio_staging', { keyPath: 'idPatomb' });
                    staging.createIndex('nutomb', 'nutomb', { unique: false });
                }

                if (!db.objectStoreNames.contains('metadata')) {
                    const metadata = db.createObjectStore('metadata', { keyPath: 'key' });
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

    bulkPut: async function (storeName, items) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const tx = this.db.transaction([storeName], 'readwrite');
            const store = tx.objectStore(storeName);
            let i = 0;
            function next() {
                if (i >= items.length) return;
                const req = store.put(items[i++]);
                req.onsuccess = next;
                req.onerror = () => reject(req.error);
            }
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
            next();
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

    clear: async function (storeName) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized.'));
                return;
            }
            const transaction = this.db.transaction([storeName], 'readwrite');
            const store = transaction.objectStore(storeName);
            const request = store.clear();
            request.onsuccess = () => resolve();
            request.onerror = () => reject(request.error);
        });
    },

    setMetadata: async function (key, value) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized.'));
                return;
            }
            const tx = this.db.transaction(['metadata'], 'readwrite');
            const store = tx.objectStore('metadata');
            const req = store.put({ key: key, value: value });
            tx.oncomplete = () => resolve();
            tx.onerror = () => reject(tx.error);
        });
    },

    getMetadata: async function (key) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized.'));
                return;
            }
            const tx = this.db.transaction(['metadata'], 'readonly');
            const store = tx.objectStore('metadata');
            const req = store.get(key);
            req.onsuccess = () => resolve(req.result ? req.result.value : null);
            req.onerror = () => reject(req.error);
        });
    },

    swapPatrimonioFromStaging: async function () {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized.'));
                return;
            }
            const tx = this.db.transaction(['patrimonio', 'patrimonio_staging'], 'readwrite');
            const destino = tx.objectStore('patrimonio');
            const staging = tx.objectStore('patrimonio_staging');
            const getAllReq = staging.getAll();
            getAllReq.onsuccess = () => {
                const items = getAllReq.result || [];
                const clearReq = destino.clear();
                clearReq.onsuccess = () => {
                    let i = 0;
                    function putNext() {
                        if (i >= items.length) {
                            const clearStaging = staging.clear();
                            clearStaging.onsuccess = () => resolve();
                            clearStaging.onerror = () => reject(clearStaging.error);
                            return;
                        }
                        const req = destino.put(items[i++]);
                        req.onsuccess = putNext;
                        req.onerror = () => reject(req.error);
                    }
                    putNext();
                };
                clearReq.onerror = () => reject(clearReq.error);
            };
            getAllReq.onerror = () => reject(getAllReq.error);
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
