// Library Loader with fallback and caching
window.libraryLoader = {
    loadedLibraries: new Set(),
    
    // Load external library with fallback URLs
    async loadLibrary(name, urls, timeout = 15000) {
        if (this.loadedLibraries.has(name)) {
            return true;
        }
        
        const urlArray = Array.isArray(urls) ? urls : [urls];
        
        for (const url of urlArray) {
            try {
                await this.loadScript(url, timeout);
                this.loadedLibraries.add(name);
                console.log(`Successfully loaded ${name} from ${url}`);
                return true;
            } catch (error) {
                console.warn(`Failed to load ${name} from ${url}:`, error.message);
                continue;
            }
        }
        
        throw new Error(`Failed to load ${name} from all URLs: ${urlArray.join(', ')}`);
    },
    
    // Load single script with timeout
    loadScript(url, timeout = 15000) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            const timeoutId = setTimeout(() => {
                script.remove();
                reject(new Error(`Script loading timeout: ${url}`));
            }, timeout);
            
            script.onload = () => {
                clearTimeout(timeoutId);
                resolve();
            };
            
            script.onerror = () => {
                clearTimeout(timeoutId);
                script.remove();
                reject(new Error(`Script loading error: ${url}`));
            };
            
            script.src = url;
            document.head.appendChild(script);
        });
    },
    
    // Check if library is available
    isLibraryLoaded(name) {
        return this.loadedLibraries.has(name);
    }
};