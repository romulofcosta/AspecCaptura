using System;

namespace pwa_camera_poc_blazor.Services
{
    public class ToastService
    {
        public event Action<string, string>? OnShow;

        public void Show(string message, string type = "info")
        {
            OnShow?.Invoke(message, type);
        }

        public void ShowSuccess(string message) => Show(message, "success");
        public void ShowError(string message) => Show(message, "error");
        public void ShowInfo(string message) => Show(message, "info");
    }
}
