window.updateService = {
    dotnetRef: null,
    currentVersion: '0.2.2',
    updateCheckInterval: 60000, // 1 minuto
    updateCheckTimer: null,

    initialize: async function (dotnetRef) {
        this.dotnetRef = dotnetRef;
        console.log('[UpdateService] Inicializando...');

        if ('serviceWorker' in navigator) {
            try {
                // Registra o service worker
                const registration = await navigator.serviceWorker.register('/service-worker.js', {
                    scope: '/'
                });

                console.log('[UpdateService] Service Worker registrado:', registration);

                // Monitora atualizações
                registration.addEventListener('updatefound', () => {
                    console.log('[UpdateService] Atualização encontrada');
                    const newWorker = registration.installing;

                    newWorker.addEventListener('statechange', () => {
                        if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                            console.log('[UpdateService] Nova versão disponível');
                            this.notifyUpdateAvailable();
                        }
                    });
                });

                // Verifica atualizações periodicamente
                this.startUpdateCheck();

                // Monitora mensagens do service worker
                navigator.serviceWorker.addEventListener('message', (event) => {
                    if (event.data.type === 'UPDATE_AVAILABLE') {
                        console.log('[UpdateService] Atualização disponível via mensagem');
                        this.notifyUpdateAvailable();
                    }
                });
            } catch (error) {
                console.error('[UpdateService] Erro ao registrar Service Worker:', error);
            }
        }
    },

    checkForUpdates: async function () {
        if ('serviceWorker' in navigator) {
            try {
                const registration = await navigator.serviceWorker.getRegistration();
                if (registration) {
                    console.log('[UpdateService] Verificando atualizações...');
                    await registration.update();
                }
            } catch (error) {
                console.error('[UpdateService] Erro ao verificar atualizações:', error);
            }
        }
    },

    startUpdateCheck: function () {
        // Verifica atualizações a cada intervalo
        this.updateCheckTimer = setInterval(() => {
            this.checkForUpdates();
        }, this.updateCheckInterval);

        console.log('[UpdateService] Verificação de atualizações iniciada');
    },

    stopUpdateCheck: function () {
        if (this.updateCheckTimer) {
            clearInterval(this.updateCheckTimer);
            this.updateCheckTimer = null;
            console.log('[UpdateService] Verificação de atualizações parada');
        }
    },

    notifyUpdateAvailable: function () {
        if (this.dotnetRef) {
            try {
                // Obtém a nova versão do manifest
                const newVersion = this.getNewVersion();
                this.dotnetRef.invokeMethodAsync('OnUpdateAvailable', newVersion);
                console.log('[UpdateService] Notificação de atualização enviada');
            } catch (error) {
                console.error('[UpdateService] Erro ao notificar atualização:', error);
            }
        }
    },

    getNewVersion: function () {
        // Tenta obter a versão do manifest.json
        try {
            const versionMeta = document.querySelector('meta[name="app-version"]');
            if (versionMeta) {
                return versionMeta.getAttribute('content') || '0.2.2';
            }
        } catch (error) {
            console.error('[UpdateService] Erro ao obter versão:', error);
        }
        return '0.2.2';
    },

    skipWaiting: async function () {
        if ('serviceWorker' in navigator) {
            try {
                const registration = await navigator.serviceWorker.getRegistration();
                if (registration && registration.waiting) {
                    registration.waiting.postMessage({ type: 'SKIP_WAITING' });
                    console.log('[UpdateService] Skip waiting enviado');
                }
            } catch (error) {
                console.error('[UpdateService] Erro ao enviar skip waiting:', error);
            }
        }
    }
};
