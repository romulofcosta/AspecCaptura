window.cameraInterop = {
    startCamera: async (videoElementId, facingMode) => {
        const video = document.getElementById(videoElementId);
        if (!video) return;

        const constraints = {
            video: { facingMode: facingMode, width: { ideal: 1920 } }
        };
        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            video.srcObject = stream;
        } catch (err) {
            console.error("Error accessing camera: ", err);
            throw err;
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
