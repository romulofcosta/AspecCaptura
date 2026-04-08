# Guia de Deploy - PWA Camera POC

## Problema Identificado

O erro de CORS ocorria porque:

1. A configuração de CORS no backend não permitia o domínio principal `https://pwa-camera-poc-blazor.pages.dev`
2. A URL da API não estava sendo configurada corretamente no build de produção

## Correções Aplicadas

### 1. Backend (API no Render)

Atualizada a configuração de CORS em `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", corsBuilder =>
    {
        corsBuilder
            .SetIsOriginAllowed(origin =>
            {
                if (builder.Environment.IsDevelopment()) return true;
                
                // Permite o domínio principal e subdomínios
                return origin == "https://pwa-camera-poc-blazor.pages.dev" ||
                       origin.EndsWith(".pwa-camera-poc-blazor.pages.dev");
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

### 2. Frontend (PWA no Cloudflare Pages)

Atualizado o script `build-production.sh` para configurar a URL da API automaticamente.

## Instruções de Deploy

### Deploy do Backend (Render)

1. Faça commit das alterações no repositório
2. O Render detectará automaticamente as mudanças e fará o redeploy
3. Aguarde o deploy completar (geralmente 2-5 minutos)

### Deploy do Frontend (Cloudflare Pages)

#### Opção 1: Deploy Automático (Recomendado)

1. Configure a variável de ambiente no Cloudflare Pages:
   - Vá em Settings > Environment Variables
   - Adicione: `API_BASE_URL` = `https://pwa-camera-poc-api.onrender.com`

2. Faça commit das alterações no repositório
3. O Cloudflare Pages fará o deploy automaticamente

#### Opção 2: Deploy Manual

```bash
# Execute o script de build
./build-production.sh

# O script usará a URL padrão: https://pwa-camera-poc-api.onrender.com
# Ou você pode especificar uma URL customizada:
API_BASE_URL=https://sua-api.com ./build-production.sh
```

## Verificação

Após o deploy, verifique:

1. Acesse `https://pwa-camera-poc-blazor.pages.dev`
2. Abra o DevTools (F12) > Console
3. Não deve haver erros de CORS
4. Tente fazer login para confirmar que a API está respondendo

## Troubleshooting

### Ainda vejo erros de CORS

1. Verifique se o backend foi redeployado com as novas configurações
2. Limpe o cache do navegador (Ctrl+Shift+Delete)
3. Verifique se a URL da API está correta no `appsettings.json` do build

### API não responde

1. Verifique se o serviço no Render está ativo
2. Acesse diretamente `https://pwa-camera-poc-api.onrender.com/health`
3. Deve retornar: `{"status":"healthy","timestamp":"..."}`

### Build falha no Cloudflare Pages

1. Verifique se o comando de build está correto: `./build-production.sh`
2. Verifique se o diretório de output está correto: `dist/wwwroot`
3. Verifique os logs de build no Cloudflare Pages

## URLs de Produção

- Frontend: https://pwa-camera-poc-blazor.pages.dev
- Backend: https://pwa-camera-poc-api.onrender.com
- API Health Check: https://pwa-camera-poc-api.onrender.com/health
- API Swagger: https://pwa-camera-poc-api.onrender.com/swagger
