// Web Crypto API Interop for AES-GCM encryption
window.cryptoInterop = {
    // Store the encryption key in memory
    encryptionKey: null,

    /**
     * Derive encryption key from user credentials using PBKDF2
     * @param {string} credentials - User credentials (username + password)
     * @param {string} salt - Salt for key derivation (base64)
     * @returns {Promise<string>} Base64 encoded key
     */
    async deriveKey(credentials, salt) {
        try {
            const encoder = new TextEncoder();
            const credentialsBuffer = encoder.encode(credentials);
            const saltBuffer = salt ? this.base64ToBuffer(salt) : crypto.getRandomValues(new Uint8Array(16));

            // Import credentials as key material
            const keyMaterial = await crypto.subtle.importKey(
                'raw',
                credentialsBuffer,
                'PBKDF2',
                false,
                ['deriveBits', 'deriveKey']
            );

            // Derive AES-GCM key
            const key = await crypto.subtle.deriveKey(
                {
                    name: 'PBKDF2',
                    salt: saltBuffer,
                    iterations: 100000,
                    hash: 'SHA-256'
                },
                keyMaterial,
                { name: 'AES-GCM', length: 256 },
                true,
                ['encrypt', 'decrypt']
            );

            // Store key in memory
            this.encryptionKey = key;

            // Export and return key as base64
            const exportedKey = await crypto.subtle.exportKey('raw', key);
            return this.bufferToBase64(new Uint8Array(exportedKey));
        } catch (error) {
            console.error('Error deriving key:', error);
            throw new Error('Failed to derive encryption key: ' + error.message);
        }
    },

    /**
     * Encrypt string data using AES-GCM
     * @param {string} plainText - Text to encrypt
     * @returns {Promise<string>} Base64 encoded encrypted data (iv + ciphertext)
     */
    async encrypt(plainText) {
        try {
            if (!this.encryptionKey) {
                throw new Error('Encryption key not initialized');
            }

            const encoder = new TextEncoder();
            const data = encoder.encode(plainText);

            // Generate random IV (12 bytes for GCM)
            const iv = crypto.getRandomValues(new Uint8Array(12));

            // Encrypt data
            const encryptedData = await crypto.subtle.encrypt(
                { name: 'AES-GCM', iv: iv },
                this.encryptionKey,
                data
            );

            // Combine IV + encrypted data
            const combined = new Uint8Array(iv.length + encryptedData.byteLength);
            combined.set(iv, 0);
            combined.set(new Uint8Array(encryptedData), iv.length);

            return this.bufferToBase64(combined);
        } catch (error) {
            console.error('Error encrypting:', error);
            throw new Error('Failed to encrypt data: ' + error.message);
        }
    },

    /**
     * Decrypt string data using AES-GCM
     * @param {string} cipherText - Base64 encoded encrypted data
     * @returns {Promise<string>} Decrypted text
     */
    async decrypt(cipherText) {
        try {
            if (!this.encryptionKey) {
                throw new Error('Encryption key not initialized');
            }

            const combined = this.base64ToBuffer(cipherText);

            // Extract IV (first 12 bytes) and encrypted data
            const iv = combined.slice(0, 12);
            const encryptedData = combined.slice(12);

            // Decrypt data
            const decryptedData = await crypto.subtle.decrypt(
                { name: 'AES-GCM', iv: iv },
                this.encryptionKey,
                encryptedData
            );

            const decoder = new TextDecoder();
            return decoder.decode(decryptedData);
        } catch (error) {
            console.error('Error decrypting:', error);
            throw new Error('Failed to decrypt data: ' + error.message);
        }
    },

    /**
     * Encrypt byte array (for photos) using AES-GCM
     * @param {Uint8Array} data - Data to encrypt
     * @returns {Promise<Uint8Array>} Encrypted data (iv + ciphertext)
     */
    async encryptBytes(data) {
        try {
            if (!this.encryptionKey) {
                throw new Error('Encryption key not initialized');
            }

            // Generate random IV
            const iv = crypto.getRandomValues(new Uint8Array(12));

            // Encrypt data
            const encryptedData = await crypto.subtle.encrypt(
                { name: 'AES-GCM', iv: iv },
                this.encryptionKey,
                data
            );

            // Combine IV + encrypted data
            const combined = new Uint8Array(iv.length + encryptedData.byteLength);
            combined.set(iv, 0);
            combined.set(new Uint8Array(encryptedData), iv.length);

            return Array.from(combined);
        } catch (error) {
            console.error('Error encrypting bytes:', error);
            throw new Error('Failed to encrypt bytes: ' + error.message);
        }
    },

    /**
     * Decrypt byte array using AES-GCM
     * @param {Uint8Array} encryptedData - Encrypted data
     * @returns {Promise<Uint8Array>} Decrypted data
     */
    async decryptBytes(encryptedData) {
        try {
            if (!this.encryptionKey) {
                throw new Error('Encryption key not initialized');
            }

            const data = new Uint8Array(encryptedData);

            // Extract IV and encrypted data
            const iv = data.slice(0, 12);
            const ciphertext = data.slice(12);

            // Decrypt data
            const decryptedData = await crypto.subtle.decrypt(
                { name: 'AES-GCM', iv: iv },
                this.encryptionKey,
                ciphertext
            );

            return Array.from(new Uint8Array(decryptedData));
        } catch (error) {
            console.error('Error decrypting bytes:', error);
            throw new Error('Failed to decrypt bytes: ' + error.message);
        }
    },

    /**
     * Encrypt large file using streaming (for files > 500KB)
     * @param {Uint8Array} data - Data to encrypt
     * @returns {Promise<Uint8Array>} Encrypted data
     */
    async encryptStream(data) {
        // For files > 500KB, we could implement chunked encryption
        // For now, use the same method as encryptBytes
        return await this.encryptBytes(data);
    },

    /**
     * Clear encryption keys from memory
     */
    clearKeys() {
        this.encryptionKey = null;
    },

    // Helper functions
    bufferToBase64(buffer) {
        let binary = '';
        const bytes = new Uint8Array(buffer);
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary);
    },

    base64ToBuffer(base64) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes;
    }
};
