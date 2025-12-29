window.dbInterop = {
    db: null,
    dbName: 'PwaInventoryDB',
    dbVersion: 1,

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

                // 1. Users Store
                if (!db.objectStoreNames.contains('users')) {
                    const usersStore = db.createObjectStore('users', { keyPath: 'id', autoIncrement: true });
                    usersStore.createIndex('username', 'username', { unique: true });
                }

                // 2. Hierarchy Stores
                if (!db.objectStoreNames.contains('states')) {
                    const statesStore = db.createObjectStore('states', { keyPath: 'id' });
                    // Seeding in onsuccess/separate method, or here if we use transaction
                    statesStore.put({ id: 'CE', name: 'Ceará' });
                    statesStore.put({ id: 'PA', name: 'Pará' });
                    statesStore.put({ id: 'MA', name: 'Maranhão' });
                    statesStore.put({ id: 'RN', name: 'Rio Grande do Norte' });
                }

                if (!db.objectStoreNames.contains('cities')) {
                    const citiesStore = db.createObjectStore('cities', { keyPath: 'id', autoIncrement: true });
                    citiesStore.createIndex('stateId', 'stateId', { unique: false });
                    // Seed
                    citiesStore.put({ id: 1, stateId: 'CE', name: 'Horizonte' });
                    citiesStore.put({ id: 2, stateId: 'CE', name: 'Fortaleza' });
                    citiesStore.put({ id: 3, stateId: 'PA', name: 'Belém' });
                    citiesStore.put({ id: 4, stateId: 'MA', name: 'São Luís' });
                    citiesStore.put({ id: 5, stateId: 'RN', name: 'Natal' });
                }

                if (!db.objectStoreNames.contains('units')) {
                    const unitsStore = db.createObjectStore('units', { keyPath: 'id', autoIncrement: true });
                    unitsStore.createIndex('cityId', 'cityId', { unique: false });
                    // Seed
                    unitsStore.put({ id: 1, cityId: 1, name: 'Prefeitura Municipal de Horizonte' });
                    unitsStore.put({ id: 2, cityId: 1, name: 'Fundo Municipal de Saúde' });
                    unitsStore.put({ id: 3, cityId: 1, name: 'Câmara Municipal de Horizonte' });
                    unitsStore.put({ id: 4, cityId: 2, name: 'Prefeitura de Fortaleza' });
                    unitsStore.put({ id: 5, cityId: 3, name: 'Prefeitura de Belém' });
                    unitsStore.put({ id: 6, cityId: 4, name: 'Prefeitura de São Luís' });
                    unitsStore.put({ id: 7, cityId: 5, name: 'Prefeitura de Natal' });
                }

                // 3. Items Store (Inventory)
                if (!db.objectStoreNames.contains('items')) {
                    // keyPath 'id' to support String GUIDs from C#
                    const itemsStore = db.createObjectStore('items', { keyPath: 'id' });
                    itemsStore.createIndex('name', 'name', { unique: false });
                    itemsStore.createIndex('code', 'code', { unique: false });
                    itemsStore.createIndex('category', 'category', { unique: false });
                    itemsStore.createIndex('timestamp', 'timestamp', { unique: false });
                    itemsStore.createIndex('unitId', 'unitId', { unique: false });
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

    getItemsByUnit: async function (unitId) {
        return new Promise((resolve, reject) => {
            if (!this.db) {
                reject(new Error('Database not initialized. Call init first.'));
                return;
            }
            const transaction = this.db.transaction(['items'], 'readonly');
            const store = transaction.objectStore('items');
            const index = store.index('unitId');
            const request = index.getAll(unitId);

            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    },

    isInitialized: function () {
        return this.db !== null;
    }
};
