window.cameraInterop = {
    startCamera: async (videoElementId, facingMode) => {
        const video = document.getElementById(videoElementId);
        if (!video) return;

        const constraints = {
            video: { 
                facingMode: facingMode,
                width: { ideal: 1280 },
                height: { ideal: 720 }
            }
        };
        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            video.srcObject = stream;
        } catch (err) {
            console.error("Error accessing camera: ", err);
            // For iOS Safari, provide more specific error messages
            let errorMessage = "Erro ao acessar a câmera.";
            if (err.name === 'NotAllowedError') {
                errorMessage = "Permissão de câmera negada. Permita o acesso à câmera nas configurações do navegador.";
            } else if (err.name === 'NotFoundError') {
                errorMessage = "Câmera não encontrada.";
            } else if (err.name === 'NotSupportedError') {
                errorMessage = "Câmera não suportada neste dispositivo.";
            } else if (err.name === 'NotReadableError') {
                errorMessage = "Câmera está sendo usada por outro aplicativo.";
            }
            throw new Error(errorMessage + " Detalhes: " + err.message);
        }
    },
    takePhoto: (videoElementId) => {
        const video = document.getElementById(videoElementId);
        if (!video) return null;

        const canvas = document.createElement('canvas');
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        canvas.getContext('2d').drawImage(video, 0, 0);
        return canvas.toDataURL('image/jpeg', 0.9);
    },
    stopCamera: (videoElementId) => {
        const video = document.getElementById(videoElementId);
        if (video && video.srcObject) {
            video.srcObject.getTracks().forEach(t => t.stop());
            video.srcObject = null;
        }
    }
};
