using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using AspecCaptura;
using AspecCaptura.Services;
using AspecCaptura.Services.Auth;
using AspecCaptura.Services.Camera;
using AspecCaptura.Services.Recognition;
using AspecCaptura.Services.Storage;
using AspecCaptura.Services.AWS;
using AspecCaptura.Services.Crypto;
using AspecCaptura.Services.Image;
using AspecCaptura.Services.Notification;
using AspecCaptura.Services.Sync;
using AspecCaptura.Services.Configuration;
using AspecCaptura.Services.Capture;
using AspecCaptura.Models;
using LocalStorageService = AspecCaptura.Services.Storage.LocalStorageService;
using ILocalStorageService = AspecCaptura.Services.Storage.ILocalStorageService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Configuração do HttpClient para a API BFF
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("BackendApi", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(apiBaseUrl) || apiBaseUrl == "__API_BASE_URL__")
    {
        apiBaseUrl = "http://localhost:5069";
    }
    client.BaseAddress = new Uri(apiBaseUrl);
    // Aumenta o timeout para 10 minutos para operações de sincronização pesadas
    client.Timeout = TimeSpan.FromMinutes(10);
}).AddHttpMessageHandler<AuthorizationMessageHandler>();

// Storage Services
builder.Services.AddScoped<IIndexedDbService, IndexedDbService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Core Services
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddSingleton<IAppInfo, AppInfo>();
builder.Services.AddScoped<IUpdateService, UpdateService>();

// Auth Services
builder.Services.AddScoped<BruteForceProtection>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Crypto Services
builder.Services.AddScoped<ICryptoService, CryptoService>();

// Camera and Image Services
builder.Services.AddScoped<IImageCompressor, ImageCompressor>();
builder.Services.AddScoped<ICameraService, CameraService>();

// Recognition Services
builder.Services.AddScoped<IQRCodeService, QRCodeRecognitionService>();
builder.Services.AddScoped<IOCRService, OCRRecognitionService>();
builder.Services.AddScoped<IBarcodeService, BarcodeRecognitionService>();
builder.Services.AddScoped<IPatrimonioSearchService, PatrimonioSearchService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IRecognitionService, RecognitionService>();

// Configuration Services
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IConfigurationIntegrationService, ConfigurationIntegrationService>();

// Capture API Services
builder.Services.AddScoped<ICaptureApiService, CaptureApiService>();

// Sync Services
builder.Services.AddScoped<SyncService>(); // Legacy patrimonio sync
builder.Services.AddScoped<ISyncService, ItemSyncService>(); // New item sync

// Notification Services
builder.Services.AddScoped<INotificationService, NotificationService>();

// AWS Services (legacy)
builder.Services.AddScoped<AwsConfig>();
builder.Services.AddScoped<IAwsStorageService, AwsStorageService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

var host = builder.Build();

try
{
    var dbService = host.Services.GetRequiredService<IIndexedDbService>();
    await dbService.InitializeAsync();
    Console.WriteLine("IndexedDB initialized successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to initialize IndexedDB: {ex.Message}");
    // Continue running the app even if DB fails
}

try
{
    var configIntegrationService = host.Services.GetRequiredService<IConfigurationIntegrationService>();
    await configIntegrationService.InitializeAsync();
    Console.WriteLine("Configuration integration service initialized successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to initialize configuration integration service: {ex.Message}");
    // Continue running the app even if configuration integration fails
}

await host.RunAsync();
