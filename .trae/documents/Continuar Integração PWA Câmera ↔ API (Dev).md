## Estado Atual
- Front-end Blazor WASM com PWA configurado [Program.cs](file:///c:/source/pwa-camera-poc-blazor/Program.cs), [index.html](file:///c:/source/pwa-camera-poc-blazor/wwwroot/index.html), [service-worker.published.js](file:///c:/source/pwa-camera-poc-blazor/wwwroot/service-worker.published.js).
- Captura de câmera via JS interop [camera-interop.js](file:///c:/source/pwa-camera-poc-blazor/wwwroot/js/camera-interop.js) usada em [Camera.razor](file:///c:/source/pwa-camera-poc-blazor/Pages/Camera.razor).
- Armazenamento offline com IndexedDB [IndexedDbService.cs](file:///c:/source/pwa-camera-poc-blazor/Services/Storage/IndexedDbService.cs).
- Integração com API para URL pré‑assinada e verificação [AwsStorageService.cs](file:///c:/source/pwa-camera-poc-blazor/Services/AWS/AwsStorageService.cs).
- API ASP.NET Core minimal com CORS e S3 [Program.cs](file:///c:/source/pwa-camera-poc-api/Program.cs), configs em [appsettings.Development.json](file:///c:/source/pwa-camera-poc-api/appsettings.Development.json).

## Objetivo
- Concluir o fluxo end‑to‑end em Desenvolvimento: capturar Foto → salvar offline → sincronizar para S3 via API, com CORS correto, configuração de ambiente alinhada e comportamento offline validado.

## Ajustes de Configuração
- API: revisar [appsettings.Development.json](file:///c:/source/pwa-camera-poc-api/appsettings.Development.json) (Region, BucketName). Credenciais via variáveis de ambiente; não versionar segredos.
- CORS: confirmar origens de Dev na política "AllowSpecificOrigins" [Program.cs](file:///c:/source/pwa-camera-poc-api/Program.cs#L24-L53), incluindo a URL do Blazor.
- Front-end: alinhar ApiBaseUrl em [appsettings.Development.json](file:///c:/source/pwa-camera-poc-blazor/wwwroot/appsettings.Development.json) com a URL da API de Dev.

## Integração HTTP
- Validar registro do HttpClient nomeado "BackendApi" [Program.cs](file:///c:/source/pwa-camera-poc-blazor/Program.cs#L21-L26) lendo ApiBaseUrl.
- Opcional