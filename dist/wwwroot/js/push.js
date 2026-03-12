// Web Push API Interop
window.pushInterop = {
    /**
     * Request permission for push notifications
     * @returns {Promise<boolean>} True if permission granted
     */
    async requestPermission() {
        try {
            if (!('Notification' in window)) {
                console.warn('Push notifications not supported');
                return false;
            }

            const permission = await Notification.requestPermission();
            return permission === 'granted';
        } catch (error) {
            console.error('Error requesting push permission:', error);
            return false;
        }
    },

    /**
     * Subscribe to push notifications
     * @param {string} vapidPublicKey - VAPID public key from server
     * @returns {Promise<object>} Push subscription object
     */
    async subscribe(vapidPublicKey) {
        try {
            if (!('serviceWorker' in navigator)) {
                throw new Error('Service Worker not supported');
            }

            const registration = await navigator.serviceWorker.ready;
            
            // Check if already subscribed
            let subscription = await registration.pushManager.getSubscription();
            
            if (!subscription) {
                // Create new subscription
                subscription = await registration.pushManager.subscribe({
                    userVisibleOnly: true,
                    applicationServerKey: this.urlBase64ToUint8Array(vapidPublicKey)
                });
            }

            return {
                endpoint: subscription.endpoint,
                keys: {
                    p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')),
                    auth: this.arrayBufferToBase64(subscription.getKey('auth'))
                }
            };
        } catch (error) {
            console.error('Error subscribing to push:', error);
            throw error;
        }
    },

    /**
     * Unsubscribe from push notifications
     * @returns {Promise<boolean>} True if unsubscribed successfully
     */
    async unsubscribe() {
        try {
            if (!('serviceWorker' in navigator)) {
                return false;
            }

            const registration = await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.getSubscription();
            
            if (subscription) {
                await subscription.unsubscribe();
                return true;
            }
            
            return false;
        } catch (error) {
            console.error('Error unsubscribing from push:', error);
            return false;
        }
    },

    /**
     * Check if push notifications are supported
     * @returns {boolean} True if supported
     */
    isSupported() {
        return 'Notification' in window && 'serviceWorker' in navigator && 'PushManager' in window;
    },

    /**
     * Get current permission status
     * @returns {string} Permission status: 'granted', 'denied', or 'default'
     */
    getPermissionStatus() {
        if (!('Notification' in window)) {
            return 'denied';
        }
        return Notification.permission;
    },

    // Helper functions
    urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding)
            .replace(/\-/g, '+')
            .replace(/_/g, '/');

        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    },

    arrayBufferToBase64(buffer) {
        const bytes = new Uint8Array(buffer);
        let binary = '';
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    }
};
