// Service Worker for Aspec Captura PWA
// Version: 0.4.0
// Requirements: 7.1, 7.2, 7.3

const APP_VERSION = '0.11.2';
const CACHE_NAME = `aspec-captura-v${APP_VERSION.replace(/\./g, '-')}`;
const API_CACHE_NAME = `aspec-captura-api-v${APP_VERSION.replace(/\./g, '-')}`;
const STATIC_CACHE_NAME = `aspec-captura-static-v${APP_VERSION.replace(/\./g, '-')}`;

// App Shell - Cache First Strategy
const APP_SHELL_URLS = [
    '/',
    '/index.html',
    '/manifest.json',
    '/icon-192.png',
    '/icon-512.png'
];

// Static Assets - Cache First Strategy
const STATIC_ASSETS = [
    '/css/design-tokens.css',
    '/css/theme.css',
    '/css/components.css',
    '/css/responsive.css',
    '/css/fluid-typography.css',
    '/css/ripple.css',
    '/css/pull-to-refresh.css',
    '/css/touch-action.css',
    '/css/animations.css',
    '/css/app-global.css',
    '/css/app.css',
    '/js/app.js',
    '/js/theme.js',
    '/js/crypto.js',
    '/js/camera-interop.js',
    '/js/db-interop.js',
    '/js/gestures.js',
    '/js/ripple.js',
    '/js/pull-to-refresh.js'
];

// Install event - cache app shell and static assets
self.addEventListener('install', event => {
    console.log('[ServiceWorker] Installing version', APP_VERSION);
    event.waitUntil(
        Promise.all([
            // Cache app shell
            caches.open(CACHE_NAME)
                .then(cache => {
                    console.log('[ServiceWorker] Caching app shell');
                    return cache.addAll(APP_SHELL_URLS);
                }),
            // Cache static assets
            caches.open(STATIC_CACHE_NAME)
                .then(cache => {
                    console.log('[ServiceWorker] Caching static assets');
                    return cache.addAll(STATIC_ASSETS);
                })
        ])
        .then(() => {
            console.log('[ServiceWorker] Installation complete');
            return self.skipWaiting();
        })
        .catch(error => {
            console.error('[ServiceWorker] Installation failed:', error);
        })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    console.log('[ServiceWorker] Activating version', APP_VERSION);
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(cacheName => 
                            cacheName !== CACHE_NAME && 
                            cacheName !== API_CACHE_NAME && 
                            cacheName !== STATIC_CACHE_NAME
                        )
                        .map(cacheName => {
                            console.log('[ServiceWorker] Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        })
                );
            })
            .then(() => {
                console.log('[ServiceWorker] Activation complete');
                return self.clients.claim();
            })
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

    // API requests — sem cache, sempre vai para a rede
    // Timeout alto para suportar operações lentas (sync de tombamentos, login com arquivo grande)
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            networkFirstWithTimeout(request, 120000) // 2 minutos
        );
        return;
    }

    // Static assets (CSS, JS, images, fonts) - Cache First
    if (url.pathname.match(/\.(css|js|png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)$/)) {
        event.respondWith(
            cacheFirst(request, STATIC_CACHE_NAME)
        );
        return;
    }

    // App Shell - Cache First
    if (APP_SHELL_URLS.some(shellUrl => url.pathname.endsWith(shellUrl))) {
        event.respondWith(
            cacheFirst(request, CACHE_NAME)
        );
        return;
    }

    // Other resources - Network First with cache fallback
    event.respondWith(
        networkFirst(request)
    );
});

// Cache First Strategy
async function cacheFirst(request, cacheName = CACHE_NAME) {
    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    
    if (cached) {
        // Return cached response and update cache in background
        fetch(request)
            .then(response => {
                if (response.ok) {
                    cache.put(request, response.clone());
                }
            })
            .catch(() => {}); // Ignore network errors
        
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

// Network First Strategy
async function networkFirst(request) {
    const cache = await caches.open(CACHE_NAME);
    
    try {
        const response = await fetch(request);
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

