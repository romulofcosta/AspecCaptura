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
    public string Version { get; } = "0.2.2";
    public string AppName { get; } = "Aspec Captura";
    public DateTime BuildDate { get; } = new DateTime(2026, 3, 10);

    public AppInfo()
    {
        // Tenta obter a versão do assembly
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            if (version != null)
            {
                Version = $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        catch
        {
            // Usa a versão padrão se não conseguir obter do assembly
        }
    }
}
