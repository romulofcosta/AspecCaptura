using Microsoft.JSInterop;

namespace Tests.Mocks;

/// <summary>
/// Mock setup for MediaDevices API (camera access)
/// </summary>
public static class MediaDevicesMock
{
    public static MockJSRuntime SetupCameraAccess(this MockJSRuntime mockJS, bool hasPermission = true, bool hasCamera = true)
    {
        // Mock getUserMedia
        if (hasPermission && hasCamera)
        {
            mockJS.Setup("navigator.mediaDevices.getUserMedia", new { stream = "mock-stream-id" });
        }
        else if (!hasPermission)
        {
            mockJS.SetupException("navigator.mediaDevices.getUserMedia", 
                new JSException("Permission denied"));
        }
        else
        {
            mockJS.SetupException("navigator.mediaDevices.getUserMedia", 
                new JSException("No camera found"));
        }

        // Mock enumerateDevices
        var devices = hasCamera ? new[]
        {
            new { deviceId = "camera1", kind = "videoinput", label = "Front Camera" },
            new { deviceId = "camera2", kind = "videoinput", label = "Back Camera" }
        } : Array.Empty<object>();

        mockJS.Setup("navigator.mediaDevices.enumerateDevices", devices);

        return mockJS;
    }

    public static MockJSRuntime SetupCameraConstraints(this MockJSRuntime mockJS, 
        int width = 1920, int height = 1080, string facingMode = "environment")
    {
        mockJS.Setup("getCameraConstraints", new
        {
            video = new
            {
                width = new { ideal = width },
                height = new { ideal = height },
                facingMode = facingMode
            }
        });

        return mockJS;
    }
}

/// <summary>
/// Mock setup for Web Workers
/// </summary>
public static class WebWorkerMock
{
    public static MockJSRuntime SetupWorkers(this MockJSRuntime mockJS, bool workersSupported = true)
    {
        if (workersSupported)
        {
            // Mock worker creation
            mockJS.Setup("createWorker", new { workerId = "mock-worker-1" });
            mockJS.Setup("initializeQRWorker", true);
            mockJS.Setup("initializeOCRWorker", true);
            
            // Mock worker communication
            mockJS.Setup("postMessageToWorker", true);
            mockJS.Setup("terminateWorker", true);
        }
        else
        {
            mockJS.SetupException("createWorker", new JSException("Web Workers not supported"));
        }

        return mockJS;
    }

    public static MockJSRuntime SetupWorkerResults(this MockJSRuntime mockJS, 
        string? qrResult = null, string? ocrResult = null)
    {
        if (qrResult != null)
        {
            mockJS.Setup("processQRFrame", new { 
                success = true, 
                data = qrResult,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        if (ocrResult != null)
        {
            mockJS.Setup("processOCRFrame", new { 
                success = true, 
                text = ocrResult,
                confidence = 0.95,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        return mockJS;
    }
}

/// <summary>
/// Mock setup for IndexedDB
/// </summary>
public static class IndexedDBMock
{
    public static MockJSRuntime SetupIndexedDB(this MockJSRuntime mockJS, bool supported = true)
    {
        if (supported)
        {
            mockJS.Setup("indexedDB.open", new { success = true, version = 1 });
            mockJS.Setup("indexedDB.createObjectStore", true);
            mockJS.Setup("indexedDB.createIndex", true);
        }
        else
        {
            mockJS.SetupException("indexedDB.open", new JSException("IndexedDB not supported"));
        }

        return mockJS;
    }

    public static MockJSRuntime SetupIndexedDBData(this MockJSRuntime mockJS, 
        IEnumerable<object> patrimonioData)
    {
        mockJS.Setup("indexedDB.getAll", patrimonioData.ToArray());
        mockJS.Setup("indexedDB.get", patrimonioData.FirstOrDefault());
        mockJS.Setup("indexedDB.put", true);
        mockJS.Setup("indexedDB.delete", true);

        return mockJS;
    }
}

/// <summary>
/// Mock setup for Notification API
/// </summary>
public static class NotificationMock
{
    public static MockJSRuntime SetupNotifications(this MockJSRuntime mockJS, 
        string permission = "granted")
    {
        mockJS.Setup("Notification.permission", permission);
        
        if (permission == "granted")
        {
            mockJS.Setup("showNotification", true);
        }
        else
        {
            mockJS.SetupException("showNotification", new JSException("Notification permission denied"));
        }

        return mockJS;
    }
}

/// <summary>
/// Mock setup for Canvas and Image processing
/// </summary>
public static class CanvasMock
{
    public static MockJSRuntime SetupCanvas(this MockJSRuntime mockJS)
    {
        // Mock canvas creation and context
        mockJS.Setup("document.createElement", new { tagName = "canvas" });
        mockJS.Setup("canvas.getContext", new { contextType = "2d" });
        
        // Mock image data operations
        mockJS.Setup("context.getImageData", new 
        { 
            data = new byte[1920 * 1080 * 4], // RGBA data
            width = 1920,
            height = 1080
        });
        
        mockJS.Setup("context.putImageData", true);
        mockJS.Setup("canvas.toDataURL", "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD...");

        return mockJS;
    }

    public static MockJSRuntime SetupImageCompression(this MockJSRuntime mockJS, 
        int originalSize = 1024000, int compressedSize = 512000)
    {
        mockJS.Setup("compressImage", new
        {
            originalSize,
            compressedSize,
            compressionRatio = (double)compressedSize / originalSize,
            dataUrl = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD..."
        });

        return mockJS;
    }
}

/// <summary>
/// Mock setup for Vibration API
/// </summary>
public static class VibrationMock
{
    public static MockJSRuntime SetupVibration(this MockJSRuntime mockJS, bool supported = true)
    {
        if (supported)
        {
            mockJS.Setup("navigator.vibrate", true);
        }
        else
        {
            mockJS.Setup("navigator.vibrate", false);
        }

        return mockJS;
    }
}