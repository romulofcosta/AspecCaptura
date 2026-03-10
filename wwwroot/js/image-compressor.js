// Image Compression Interop
window.imageCompressor = {
    /**
     * Compress image to JPEG with specified quality
     * @param {Uint8Array} imageData - Image data as byte array
     * @param {number} quality - JPEG quality (0-100)
     * @param {number} maxWidth - Maximum width
     * @param {number} maxHeight - Maximum height
     * @returns {Promise<Uint8Array>} Compressed image data
     */
    async compress(imageData, quality, maxWidth, maxHeight) {
        return new Promise((resolve, reject) => {
            try {
                // Create blob from byte array
                const blob = new Blob([new Uint8Array(imageData)], { type: 'image/jpeg' });
                const url = URL.createObjectURL(blob);
                
                const img = new Image();
                img.onload = () => {
                    try {
                        // Calculate new dimensions maintaining aspect ratio
                        let width = img.width;
                        let height = img.height;
                        
                        if (width > maxWidth || height > maxHeight) {
                            const ratio = Math.min(maxWidth / width, maxHeight / height);
                            width = Math.floor(width * ratio);
                            height = Math.floor(height * ratio);
                        }
                        
                        // Create canvas and draw resized image
                        const canvas = document.createElement('canvas');
                        canvas.width = width;
                        canvas.height = height;
                        
                        const ctx = canvas.getContext('2d');
                        ctx.drawImage(img, 0, 0, width, height);
                        
                        // Convert to JPEG blob
                        canvas.toBlob(
                            async (blob) => {
                                if (!blob) {
                                    reject(new Error('Failed to create blob'));
                                    return;
                                }
                                
                                // Convert blob to byte array
                                const arrayBuffer = await blob.arrayBuffer();
                                const byteArray = new Uint8Array(arrayBuffer);
                                
                                URL.revokeObjectURL(url);
                                resolve(Array.from(byteArray));
                            },
                            'image/jpeg',
                            quality / 100
                        );
                    } catch (error) {
                        URL.revokeObjectURL(url);
                        reject(error);
                    }
                };
                
                img.onerror = () => {
                    URL.revokeObjectURL(url);
                    reject(new Error('Failed to load image'));
                };
                
                img.src = url;
            } catch (error) {
                reject(error);
            }
        });
    },

    /**
     * Resize image maintaining aspect ratio
     * @param {Uint8Array} imageData - Image data as byte array
     * @param {number} maxWidth - Maximum width
     * @param {number} maxHeight - Maximum height
     * @returns {Promise<Uint8Array>} Resized image data
     */
    async resize(imageData, maxWidth, maxHeight) {
        // Use compress with high quality for resize
        return await this.compress(imageData, 90, maxWidth, maxHeight);
    }
};
