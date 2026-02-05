// ASPEC Capture PWA - Service Worker
// Cache Strategy: Network First with Cache Fallback for API, Cache First for Static Assets

const CACHE_NAME = 'aspec-capture-v1.4.1';
const STATIC_CACHE = 'aspec-static-v1.4.1';
const API_CACHE = 'aspec-api-v1.4.1';
const IMAGE_CACHE = 'aspec-images-v1.4.1';

// Static assets to cache on install
const STATIC_ASSETS = [
    '/',
    '/index.html',
    '/css/app.css',
    '/css/design-system.css',
    '/images/aspec_logo.png',
    '/manifest.json',
    '/_framework/blazor.webassembly.js',
    '/_content/MudBlazor/MudBlazor.min.css',
    '/_content/MudBlazor/MudBlazor.min.js'
];

// Install event - cache static assets
self.addEventListener('install', (event) => {
    console.log('[Service Worker] Installing...');
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => {
                console.log('[Service Worker] Caching static assets');
                return cache.addAll(STATIC_ASSETS);
            })
            .then(() => self.skipWaiting())
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    console.log('[Service Worker] Activating...');
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== STATIC_CACHE && 
                        cacheName !== API_CACHE && 
                        cacheName !== IMAGE_CACHE) {
                        console.log('[Service Worker] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

// Fetch event - implement cache strategies
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip cross-origin requests
    if (url.origin !== location.origin) {
        return;
    }

    // Strategy 1: Cache First for images
    if (request.destination === 'image') {
        event.respondWith(cacheFirstStrategy(request, IMAGE_CACHE));
        return;
    }

    // Strategy 2: Cache First for static assets
    if (isStaticAsset(url.pathname)) {
        event.respondWith(cacheFirstStrategy(request, STATIC_CACHE));
        return;
    }

    // Strategy 3: Network First for API calls
    if (url.pathname.includes('/api/') || url.pathname.includes('.json')) {
        event.respondWith(networkFirstStrategy(request, API_CACHE));
        return;
    }

    // Strategy 4: Network First with cache fallback for everything else
    event.respondWith(networkFirstStrategy(request, CACHE_NAME));
});

// Cache First Strategy - for static assets and images
async function cacheFirstStrategy(request, cacheName) {
    try {
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match(request);
        
        if (cachedResponse) {
            console.log('[Service Worker] Cache hit:', request.url);
            // Update cache in background
            fetch(request).then((response) => {
                if (response && response.status === 200) {
                    cache.put(request, response.clone());
                }
            }).catch(() => {});
            return cachedResponse;
        }

        console.log('[Service Worker] Cache miss, fetching:', request.url);
        const response = await fetch(request);
        
        if (response && response.status === 200) {
            cache.put(request, response.clone());
        }
        
        return response;
    } catch (error) {
        console.error('[Service Worker] Cache First error:', error);
        // Return offline fallback if available
        return caches.match('/offline.html') || new Response('Offline', { status: 503 });
    }
}

// Network First Strategy - for API calls and dynamic content
async function networkFirstStrategy(request, cacheName) {
    try {
        const response = await fetch(request);
        
        if (response && response.status === 200) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        
        return response;
    } catch (error) {
        console.log('[Service Worker] Network failed, trying cache:', request.url);
        const cachedResponse = await caches.match(request);
        
        if (cachedResponse) {
            return cachedResponse;
        }
        
        // Return offline fallback
        return new Response(JSON.stringify({ 
            error: 'Offline', 
            message: 'Você está offline. Algumas funcionalidades podem não estar disponíveis.' 
        }), {
            status: 503,
            headers: { 'Content-Type': 'application/json' }
        });
    }
}

// Helper: Check if URL is a static asset
function isStaticAsset(pathname) {
    const staticExtensions = ['.css', '.js', '.woff', '.woff2', '.ttf', '.eot', '.svg'];
    return staticExtensions.some(ext => pathname.endsWith(ext)) ||
           pathname.includes('/_framework/') ||
           pathname.includes('/_content/');
}

// Background sync for offline actions (future enhancement)
self.addEventListener('sync', (event) => {
    if (event.tag === 'sync-items') {
        console.log('[Service Worker] Background sync triggered');
        event.waitUntil(syncPendingItems());
    }
});

async function syncPendingItems() {
    // This will be implemented when IndexedDB sync is ready
    console.log('[Service Worker] Syncing pending items...');
    // TODO: Implement sync logic with IndexedDB
}

// Push notifications (future enhancement)
self.addEventListener('push', (event) => {
    const data = event.data ? event.data.json() : {};
    const title = data.title || 'ASPEC Capture';
    const options = {
        body: data.body || 'Nova notificação',
        icon: '/images/aspec_logo.png',
        badge: '/images/aspec_logo.png',
        vibrate: [200, 100, 200],
        data: data
    };

    event.waitUntil(
        self.registration.showNotification(title, options)
    );
});

// Notification click handler
self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
        clients.openWindow(event.notification.data.url || '/')
    );
});

console.log('[Service Worker] Loaded successfully');
