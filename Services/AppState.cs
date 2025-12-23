using System;

namespace pwa_camera_poc_blazor.Services
{
    public class AppState
    {
        private int _pendingSyncCount;
        public int PendingSyncCount
        {
            get => _pendingSyncCount;
            set
            {
                if (_pendingSyncCount != value)
                {
                    _pendingSyncCount = value;
                    NotifyStateChanged();
                }
            }
        }

        private bool _isCameraActive;
        public bool IsCameraActive
        {
            get => _isCameraActive;
            set
            {
                if (_isCameraActive != value)
                {
                    _isCameraActive = value;
                    NotifyStateChanged();
                }
            }
        }

        private int? _currentUnitId;
        public int? CurrentUnitId
        {
            get => _currentUnitId;
            set
            {
                if (_currentUnitId != value)
                {
                    _currentUnitId = value;
                    NotifyStateChanged();
                }
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
