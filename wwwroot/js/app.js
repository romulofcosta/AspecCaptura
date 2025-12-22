window.appInterop = {
    setTheme: (theme) => {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('pwa-inventory-theme', theme);
    },
    getTheme: () => {
        return localStorage.getItem('pwa-inventory-theme') || 'light';
    },
    downloadFile: (fileName, base64Content) => {
        const link = document.createElement('a');
        link.href = 'data:text/csv;base64,' + base64Content;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};
