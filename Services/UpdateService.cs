using Microsoft.JSInterop;

namespace AspecCaptura.Services;

/// <summary>
/// Serviço para gerenciar atualizações do PWA e Service Worker
/// </summary>
public interface IUpdateService
{
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
    Task InitializeAsync();
    Task CheckForUpdatesAsync();
    Task ReloadAppAsync();
}

public class UpdateAvailableEventArgs : EventArgs
{
    public string CurrentVersion { get; set; } = string.Empty;
    public string NewVersion { get; set; } = string.Empty;
}

public class UpdateService : IUpdateService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IAppInfo _appInfo;
    private DotNetObjectReference<UpdateService>? _dotnetRef;

    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    public UpdateService(IJSRuntime jsRuntime, IAppInfo appInfo)
    {
        _jsRuntime = jsRuntime;
        _appInfo = appInfo;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _dotnetRef = DotNetObjectReference.Create(this);
            
            // Registra o service worker e monitora atualizações
            await _jsRuntime.InvokeVoidAsync("updateService.initialize", _dotnetRef);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro ao inicializar UpdateService: {ex.Message}");
        }
    }

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("updateService.checkForUpdates");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro ao verificar atualizações: {ex.Message}");
        }
    }

    public async Task ReloadAppAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("location.reload");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro ao recarregar app: {ex.Message}");
        }
    }

    [JSInvokable]
    public void OnUpdateAvailable(string newVersion)
    {
        UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs
        {
            CurrentVersion = _appInfo.Version,
            NewVersion = newVersion
        });
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        _dotnetRef?.Dispose();
        return ValueTask.CompletedTask;
    }
}
