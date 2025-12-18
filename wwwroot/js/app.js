window.appInterop = {
    setTheme: (theme) => {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('pwa-inventory-theme', theme);
    },
    getTheme: () => {
        return localStorage.getItem('pwa-inventory-theme') || 'light';
    }
};
