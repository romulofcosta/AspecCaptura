window.dbInterop = {
    db: null,
    dbName: 'aspec-captura-db',
    dbVersion: 7,

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

                // 1. Items Store (Patrimônios)
                let itemsStore;
                if (!db.objectStoreNames.contains('items')) {
                    itemsStore = db.createObjectStore('items', { keyPath: 'id' });
                    itemsStore.createIndex('code', 'code', { unique: true });
                    itemsStore.createIndex('isSynchronized', 'isSynchronized', { unique: false });
                    itemsStore.createIndex('createdAt', 'createdAt', { unique: false });
                    itemsStore.createIndex('userId', 'userId', { unique: false });
                    itemsStore.createIndex('name', 'name', { unique: false });
                    itemsStore.createIndex('category', 'category', { unique: false });
                    itemsStore.createIndex('timestamp', 'timestamp', { unique: false });
                    itemsStore.createIndex('idUO', 'idUO', { unique: false });
                    itemsStore.createIndex('synced', 'synced', { unique: false });
                    itemsStore.createIndex('createdBy', 'createdBy', { unique: false });
                } else {
                    itemsStore = transaction.objectStore('items');
                    // Add new indexes if they don't exist
                    if (!itemsStore.indexNames.contains('isSynchronized')) {
                        itemsStore.createIndex('isSynchronized', 'isSynchronized', { unique: false });
                    }
                    if (!itemsStore.indexNames.contains('userId')) {
                        itemsStore.createIndex('userId', 'userId', { unique: false });
                    }
                }

                // 2. Photos Store (Fotos criptografadas)
                if (!db.objectStoreNames.contains('photos')) {
                    const photosStore = db.createObjectStore('photos', { keyPath: 'id' });
                    photosStore.createIndex('itemId', 'itemId', { unique: false });
                    photosStore.createIndex('createdAt', 'createdAt', { unique: false });
                }

                // 3. Notifications Store (Notificações)
                if (!db.objectStoreNames.contains('notifications')) {
                    const notificationsStore = db.createObjectStore('notifications', { keyPath: 'id' });
                    notificationsStore.createIndex('userId', 'userId', { unique: false });
                    notificationsStore.createIndex('isRead', 'isRead', { unique: false });
                    notificationsStore.createIndex('timestamp', 'timestamp', { unique: false });
                    notificationsStore.createIndex('priority', 'priority', { unique: false });
                }

                // 4. Sync Queue Store (Fila de sincronização)
                if (!db.objectStoreNames.contains('syncQueue')) {
                    const syncQueueStore = db.createObjectStore('syncQueue', { keyPath: 'id' });
                    syncQueueStore.createIndex('itemId', 'itemId', { unique: false });
                    syncQueueStore.createIndex('createdAt', 'createdAt', { unique: false });
                    syncQueueStore.createIndex('retryCount', 'retryCount', { unique: false });
                }

                // 5. Patrimonio Store (Lookup) - mantido para compatibilidade
                if (!db.objectStoreNames.contains('patrimonio')) {
                    const patrimonioStore = db.createObjectStore('patrimonio', { keyPath: 'idPatomb' });
                    patrimonioStore.createIndex('nutomb', 'nutomb', { unique: false });
                }

                if (!db.objectStoreNames.contains('patrimonio_staging')) {
                    const staging = db.createObjectStore('patrimonio_staging', { keyPath: 'idPatomb' });
                    staging.createIndex('nutomb', 'nutomb', { unique: false });
                }

                // 6. Metadata Store
                if (!db.objectStoreNames.contains('metadata')) {
                    const metadata = db.createObjectStore('metadata', { keyPath: 'key' });
                }
            };

            request.onsuccess = (event) => {
                this.db = event.target.result;
                console.log('IndexedDB initialized successfully');
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
