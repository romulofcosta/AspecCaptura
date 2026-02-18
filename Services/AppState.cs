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


        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    NotifyStateChanged();
                }
            }
        }

        // Hierarquia Contábil da Sessão
        public Models.Orgao? CurrentOrgao { get; set; }
        public Models.UnidadeOrcamentaria? CurrentUO { get; set; }
        public Models.Area? CurrentArea { get; set; }
        public Models.Subarea? CurrentSubarea { get; set; }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
