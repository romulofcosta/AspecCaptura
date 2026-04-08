namespace pwa_camera_poc_blazor.Services;

using System.Reflection;

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
    public string Version { get; private set; }
    public string AppName { get; } = "Aspec Captura";
    public DateTime BuildDate { get; } = DateTime.UtcNow;

    public AppInfo()
    {
        // Obtém a versão do assembly automaticamente
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly.GetName().Version?.ToString()
                   ?? "0.8.0";
        
        Version = version;
    }
}
