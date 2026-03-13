// Service Worker for Aspec Captura PWA
// Version: 0.2.7

const APP_VERSION = '0.2.7';
const CACHE_NAME = `aspec-captura-v${APP_VERSION.replace(/\./g, '-')}`;
const API_CACHE_NAME = `aspec-captura-api-v${APP_VERSION.replace(/\./g, '-')}`;

// App Shell - Cache First Strategy
const APP_SHELL_URLS = [
    '/',
    '/index.html',
    '/css/theme.css',
    '/css/components.css',
    '/css/responsive.css',
    '/css/app-global.css',
    '/css/app.css',
    '/js/app.js',
    '/js/crypto.js',
    '/js/camera-interop.js',
    '/js/db-interop.js',
    '/manifest.json',
    '/icon-192.png',
    '/icon-512.png'
];

// Install event - cache app shell
self.addEventListener('install', event => {
    console.log('[ServiceWorker] Installing...');
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('[ServiceWorker] Caching app shell');
                return cache.addAll(APP_SHELL_URLS);
            })
            .then(() => self.skipWaiting())
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    console.log('[ServiceWorker] Activating...');
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(cacheName => cacheName !== CACHE_NAME && cacheName !== API_CACHE_NAME)
                        .map(cacheName => {
                            console.log('[ServiceWorker] Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        })
                );
            })
            .then(() => self.clients.claim())
    );
});

// Fetch event - implement caching strategies
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip non-GET requests
    if (request.method !== 'GET') {
        return;
    }

    // Skip .pdb, .wasm files and framework files (let browser handle them)
    if (url.pathname.endsWith('.pdb') || 
        url.pathname.endsWith('.wasm') || 
        url.pathname.includes('_framework/') ||
        url.pathname.includes('blazor.')) {
        return;
    }

    // API requests - Network First with timeout
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            networkFirstWithTimeout(request, 5000)
        );
        return;
    }

    // App Shell - Cache First
    if (APP_SHELL_URLS.some(shellUrl => url.pathname.endsWith(shellUrl))) {
        event.respondWith(
            cacheFirst(request)
        );
        return;
    }

    // Other resources - Cache First with network fallback
    event.respondWith(
        cacheFirst(request)
    );
});

// Cache First Strategy
async function cacheFirst(request) {
    const cache = await caches.open(CACHE_NAME);
    const cached = await cache.match(request);
    
    if (cached) {
        return cached;
    }
    
    try {
        const response = await fetch(request);
        if (response.ok) {
            cache.put(request, response.clone());
        }
        return response;
    } catch (error) {
        console.error('[ServiceWorker] Fetch failed:', error);
        // Return offline page if available
        return new Response('Offline', { status: 503, statusText: 'Service Unavailable' });
    }
}

// Network First with Timeout Strategy
async function networkFirstWithTimeout(request, timeout) {
    const cache = await caches.open(API_CACHE_NAME);
    
    try {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), timeout);
        
        const response = await fetch(request, { signal: controller.signal });
        clearTimeout(timeoutId);
        
        if (response.ok) {
            cache.put(request, response.clone());
        }
        return response;
    } catch (error) {
        console.log('[ServiceWorker] Network request failed, trying cache:', error);
        const cached = await cache.match(request);
        
        if (cached) {
            return cached;
        }
        
        throw error;
    }
}

// Background Sync for offline operations
self.addEventListener('sync', event => {
    console.log('[ServiceWorker] Background sync:', event.tag);
    
    if (event.tag === 'sync-items') {
        event.waitUntil(syncItems());
    }
});

async function syncItems() {
    try {
        // This will be triggered when connection is restored
        // The actual sync logic is handled by SyncService in the app
        console.log('[ServiceWorker] Sync items triggered');
        
        // Notify all clients that sync is available
        const clients = await self.clients.matchAll();
        clients.forEach(client => {
            client.postMessage({
                type: 'SYNC_AVAILABLE'
            });
        });
    } catch (error) {
        console.error('[ServiceWorker] Sync failed:', error);
        throw error;
    }
}

// Push notification support
self.addEventListener('push', event => {
    console.log('[ServiceWorker] Push received');
    
    const data = event.data ? event.data.json() : {};
    const title = data.title || 'Aspec Captura';
    const options = {
        body: data.message || 'Nova notificação',
        icon: '/icon-192.png',
        badge: '/icon-192.png',
        data: data
    };
    
    event.waitUntil(
        self.registration.showNotification(title, options)
    );
});

// Notification click handler
self.addEventListener('notificationclick', event => {
    console.log('[ServiceWorker] Notification clicked');
    event.notification.close();
    
    event.waitUntil(
        clients.openWindow('/')
    );
});

