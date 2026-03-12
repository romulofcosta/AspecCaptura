using System.Reflection;

namespace pwa_camera_poc_blazor.Services;

/// <summary>
/// Serviço para gerenciar informações da aplicação, incluindo versionamento
/// </summary>
public interface IAppInfo
{
    string Version { get; }
    string AppName { get; }
    DateTime BuildDate { get; }
}

public class AppInfo : IAppInfo
{
    public string Version { get; private set; } = "0.2.4";
    public string AppName { get; } = "Aspec Captura";
    public DateTime BuildDate { get; } = new DateTime(2026, 3, 12);

    public AppInfo()
    {
        // Versão fixa definida acima - não obtém do assembly
    }
}
