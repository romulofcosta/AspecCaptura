// Performance Detection and Optimization
window.performanceInterop = {
    deviceInfo: null,

    /**
     * Detect device performance characteristics
     * @returns {Promise<object>} Device info with memory, CPU score, and recommendations
     */
    async detectDevice() {
        if (this.deviceInfo) {
            return this.deviceInfo;
        }

        const info = {
            memory: this.getMemoryInfo(),
            cpuScore: await this.getCpuScore(),
            isLowMemory: false,
            isLowPerformance: false,
            reducedMotion: this.prefersReducedMotion(),
            recommendations: {}
        };

        // Determine if low memory device (< 2GB)
        info.isLowMemory = info.memory < 2048;

        // Determine if low performance (CPU score < 50)
        info.isLowPerformance = info.cpuScore < 50;

        // Generate recommendations
        info.recommendations = {
            disableAnimations: info.isLowPerformance || info.reducedMotion,
            reducedQuality: info.isLowMemory,
            limitConcurrentAnimations: info.isLowPerformance,
            useVirtualization: info.isLowMemory
        };

        this.deviceInfo = info;
        
        // Apply recommendations automatically
        this.applyRecommendations(info.recommendations);

        return info;
    },

    /**
     * Get available memory in MB
     * @returns {number} Memory in MB
     */
    getMemoryInfo() {
        if (navigator.deviceMemory) {
            return navigator.deviceMemory * 1024; // Convert GB to MB
        }
        
        // Fallback: estimate based on user agent
        if (navigator.userAgent.includes('Mobile')) {
            return 2048; // Assume 2GB for mobile
        }
        
        return 4096; // Assume 4GB for desktop
    },

    /**
     * Benchmark CPU performance
     * @returns {Promise<number>} CPU score (0-100)
     */
    async getCpuScore() {
        const startTime = performance.now();
        
        // Simple CPU benchmark: calculate primes
        let count = 0;
        for (let i = 2; i < 10000; i++) {
            let isPrime = true;
            for (let j = 2; j <= Math.sqrt(i); j++) {
                if (i % j === 0) {
                    isPrime = false;
                    break;
                }
            }
            if (isPrime) count++;
        }
        
        const duration = performance.now() - startTime;
        
        // Score: faster = higher score (100ms = 100, 1000ms = 10)
        const score = Math.min(100, Math.max(10, 10000 / duration));
        
        return Math.round(score);
    },

    /**
     * Check if user prefers reduced motion
     * @returns {boolean} True if reduced motion preferred
     */
    prefersReducedMotion() {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    },

    /**
     * Apply performance recommendations
     * @param {object} recommendations - Recommendations object
     */
    applyRecommendations(recommendations) {
        if (recommendations.disableAnimations) {
            document.documentElement.style.setProperty('--transition-fast', '0ms');
            document.documentElement.style.setProperty('--transition-base', '0ms');
            document.documentElement.style.setProperty('--transition-slow', '0ms');
        }

        if (recommendations.limitConcurrentAnimations) {
            // Add class to body for CSS to target
            document.body.classList.add('limit-animations');
        }

        if (recommendations.reducedQuality) {
            // Store preference for image compression
            localStorage.setItem('image_quality', 'low');
        }
    },

    /**
     * Measure page load performance
     * @returns {object} Performance metrics
     */
    getPerformanceMetrics() {
        if (!window.performance || !window.performance.timing) {
            return null;
        }

        const timing = performance.timing;
        const navigation = performance.getEntriesByType('navigation')[0];

        return {
            pageLoadTime: timing.loadEventEnd - timing.navigationStart,
            domContentLoaded: timing.domContentLoadedEventEnd - timing.navigationStart,
            timeToInteractive: navigation ? navigation.domInteractive : 0,
            firstContentfulPaint: this.getFirstContentfulPaint(),
            resourceLoadTime: timing.responseEnd - timing.requestStart
        };
    },

    /**
     * Get First Contentful Paint metric
     * @returns {number} FCP time in ms
     */
    getFirstContentfulPaint() {
        const entries = performance.getEntriesByType('paint');
        const fcp = entries.find(entry => entry.name === 'first-contentful-paint');
        return fcp ? fcp.startTime : 0;
    },

    /**
     * Log performance metrics
     */
    logPerformanceMetrics() {
        const metrics = this.getPerformanceMetrics();
        if (metrics) {
            console.log('Performance Metrics:', metrics);
            
            // Send to server for monitoring (optional)
            // fetch('/api/metrics', { method: 'POST', body: JSON.stringify(metrics) });
        }
    }
};

// Auto-detect device on load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        performanceInterop.detectDevice();
    });
} else {
    performanceInterop.detectDevice();
}

// Log metrics after page load
window.addEventListener('load', () => {
    setTimeout(() => {
        performanceInterop.logPerformanceMetrics();
    }, 1000);
});
