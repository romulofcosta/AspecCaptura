// Gesture Handler - ASPEC Capture PWA
// Provides touch gesture support for mobile devices

window.gestureHandler = {
    // Touch state
    touchStartX: 0,
    touchStartY: 0,
    touchEndX: 0,
    touchEndY: 0,
    minSwipeDistance: 50,

    /**
     * Initialize swipe gestures on an element
     * @param {string} elementId - The ID of the element to attach gestures
     * @param {object} dotNetHelper - .NET object reference for callbacks
     */
    initSwipeGestures: function (elementId, dotNetHelper) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`[Gesture] Element ${elementId} not found`);
            return;
        }

        // Touch start
        element.addEventListener('touchstart', (e) => {
            this.touchStartX = e.changedTouches[0].screenX;
            this.touchStartY = e.changedTouches[0].screenY;
        }, { passive: true });

        // Touch end
        element.addEventListener('touchend', (e) => {
            this.touchEndX = e.changedTouches[0].screenX;
            this.touchEndY = e.changedTouches[0].screenY;
            this.handleSwipe(dotNetHelper);
        }, { passive: true });

        console.log(`[Gesture] Swipe gestures initialized on ${elementId}`);
    },

    /**
     * Handle swipe gesture and invoke .NET callback
     */
    handleSwipe: function (dotNetHelper) {
        const deltaX = this.touchEndX - this.touchStartX;
        const deltaY = this.touchEndY - this.touchStartY;
        const absDeltaX = Math.abs(deltaX);
        const absDeltaY = Math.abs(deltaY);

        // Check if swipe is horizontal and meets minimum distance
        if (absDeltaX > this.minSwipeDistance && absDeltaX > absDeltaY) {
            if (deltaX > 0) {
                console.log('[Gesture] Swipe Right detected');
                dotNetHelper.invokeMethodAsync('OnSwipeRight');
                this.vibrate(50);
            } else {
                console.log('[Gesture] Swipe Left detected');
                dotNetHelper.invokeMethodAsync('OnSwipeLeft');
                this.vibrate(50);
            }
        }
        // Check if swipe is vertical
        else if (absDeltaY > this.minSwipeDistance && absDeltaY > absDeltaX) {
            if (deltaY > 0) {
                console.log('[Gesture] Swipe Down detected');
                dotNetHelper.invokeMethodAsync('OnSwipeDown');
            } else {
                console.log('[Gesture] Swipe Up detected');
                dotNetHelper.invokeMethodAsync('OnSwipeUp');
            }
        }
    },

    /**
     * Initialize pinch-to-zoom gestures
     * @param {string} elementId - The ID of the element
     * @param {object} dotNetHelper - .NET object reference
     */
    initPinchGestures: function (elementId, dotNetHelper) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`[Gesture] Element ${elementId} not found`);
            return;
        }

        let initialDistance = 0;
        let currentScale = 1;

        element.addEventListener('touchstart', (e) => {
            if (e.touches.length === 2) {
                initialDistance = this.getDistance(e.touches[0], e.touches[1]);
            }
        }, { passive: true });

        element.addEventListener('touchmove', (e) => {
            if (e.touches.length === 2) {
                const currentDistance = this.getDistance(e.touches[0], e.touches[1]);
                const scale = currentDistance / initialDistance;
                
                if (scale > 1.1) {
                    console.log('[Gesture] Pinch Out (Zoom In)');
                    dotNetHelper.invokeMethodAsync('OnPinchOut', scale);
                } else if (scale < 0.9) {
                    console.log('[Gesture] Pinch In (Zoom Out)');
                    dotNetHelper.invokeMethodAsync('OnPinchIn', scale);
                }
            }
        }, { passive: true });

        console.log(`[Gesture] Pinch gestures initialized on ${elementId}`);
    },

    /**
     * Calculate distance between two touch points
     */
    getDistance: function (touch1, touch2) {
        const dx = touch1.clientX - touch2.clientX;
        const dy = touch1.clientY - touch2.clientY;
        return Math.sqrt(dx * dx + dy * dy);
    },

    /**
     * Trigger haptic feedback
     * @param {number} duration - Vibration duration in milliseconds
     */
    vibrate: function (duration) {
        if ('vibrate' in navigator) {
            navigator.vibrate(duration);
        }
    },

    /**
     * Initialize pull-to-refresh gesture
     * @param {string} elementId - The ID of the scrollable element
     * @param {object} dotNetHelper - .NET object reference
     */
    initPullToRefresh: function (elementId, dotNetHelper) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`[Gesture] Element ${elementId} not found`);
            return;
        }

        let startY = 0;
        let isPulling = false;
        const pullThreshold = 80;

        element.addEventListener('touchstart', (e) => {
            if (element.scrollTop === 0) {
                startY = e.touches[0].clientY;
                isPulling = true;
            }
        }, { passive: true });

        element.addEventListener('touchmove', (e) => {
            if (!isPulling) return;

            const currentY = e.touches[0].clientY;
            const pullDistance = currentY - startY;

            if (pullDistance > pullThreshold) {
                console.log('[Gesture] Pull to refresh triggered');
                dotNetHelper.invokeMethodAsync('OnPullToRefresh');
                this.vibrate(100);
                isPulling = false;
            }
        }, { passive: true });

        element.addEventListener('touchend', () => {
            isPulling = false;
        }, { passive: true });

        console.log(`[Gesture] Pull-to-refresh initialized on ${elementId}`);
    },

    /**
     * Initialize long press gesture
     * @param {string} elementId - The ID of the element
     * @param {object} dotNetHelper - .NET object reference
     * @param {number} duration - Long press duration in milliseconds (default: 500ms)
     */
    initLongPress: function (elementId, dotNetHelper, duration = 500) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn(`[Gesture] Element ${elementId} not found`);
            return;
        }

        let pressTimer;

        element.addEventListener('touchstart', (e) => {
            pressTimer = setTimeout(() => {
                console.log('[Gesture] Long press detected');
                dotNetHelper.invokeMethodAsync('OnLongPress');
                this.vibrate(50);
            }, duration);
        }, { passive: true });

        element.addEventListener('touchend', () => {
            clearTimeout(pressTimer);
        }, { passive: true });

        element.addEventListener('touchmove', () => {
            clearTimeout(pressTimer);
        }, { passive: true });

        console.log(`[Gesture] Long press initialized on ${elementId}`);
    },

    /**
     * Clean up gesture listeners
     * @param {string} elementId - The ID of the element
     */
    cleanup: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            // Clone and replace to remove all event listeners
            const newElement = element.cloneNode(true);
            element.parentNode.replaceChild(newElement, element);
            console.log(`[Gesture] Cleaned up gestures on ${elementId}`);
        }
    }
};

console.log('[Gesture Handler] Loaded successfully');
