using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace pwa_camera_poc_blazor.Services.Camera
{
    public class CameraService
    {
        private readonly IJSRuntime _jsRuntime;

        public CameraService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task StartCameraAsync(string videoElementId, bool useFrontCamera)
        {
            await _jsRuntime.InvokeVoidAsync("cameraInterop.startCamera", videoElementId, useFrontCamera ? "user" : "environment");
        }

        public async Task<string> TakePhotoAsync(string videoElementId)
        {
            return await _jsRuntime.InvokeAsync<string>("cameraInterop.takePhoto", videoElementId);
        }

        public async Task<string> CaptureFrameForOcrAsync(string videoElementId, object roi)
        {
            return await _jsRuntime.InvokeAsync<string>("cameraInterop.captureFrameForOcr", videoElementId, roi);
        }

        public async Task StopCameraAsync(string videoElementId)
        {
            await _jsRuntime.InvokeVoidAsync("cameraInterop.stopCamera", videoElementId);
        }
    }
}
