using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using pwa_camera_poc_blazor;
using pwa_camera_poc_blazor.Services;
using pwa_camera_poc_blazor.Services.Auth;
using pwa_camera_poc_blazor.Services.Camera;
using pwa_camera_poc_blazor.Services.Storage;
using MudBlazor.Services;
using MudBlazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IIndexedDbService, IndexedDbService>();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CameraService>();
builder.Services.AddScoped<pwa_camera_poc_blazor.Services.ToastService>();
builder.Services.AddScoped<AppState>();

// UI Component Library
// MudBlazor: Material Design component library (free & open-source)
// Provides modern, responsive components with excellent mobile support
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});

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

await host.RunAsync();
