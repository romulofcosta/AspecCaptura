# Correções de Deploy no Render - PWA Camera PoC

## 🔍 Problemas Identificados

### 1. Erro de CORS
**Sintoma**: `Access to fetch at 'https://pwa-camera-poc-api-1.onrender.com/api/auth/login' from origin 'https://e82ab59d.pwa-camera-poc-blazor.pages.dev' has been blocked by CORS policy`

**Causa**: A configuração de CORS na API estava muito restritiva e não aceitava os subdomínios gerados pelo Cloudflare Pages (formato: `https://[hash].pwa-camera-poc-blazor.pages.dev`)

**Correção Aplicada**: Atualizada a lógica de validação de origem no `Program.cs` da API para aceitar:
- `https://pwa-camera-poc-blazor.pages.dev` (domínio principal)
- `https://[qualquer-hash].pwa-camera-poc-blazor.pages.dev` (subdomínios de preview)

```csharp
.SetIsOriginAllowed(origin =>
{
    if (builder.Environment.IsDevelopment()) return true;
    
    if (string.IsNullOrEmpty(origin)) return false;
    
    var uri = new Uri(origin);
    var host = uri.Host.ToLowerInvariant();
    
    return host == "pwa-camera-poc-blazor.pages.dev" ||
           host.EndsWith(".pwa-camera-poc-blazor.pages.dev") ||
           host.Contains("pwa-camera-poc-blazor.pages.dev");
})
```

### 2. Timeout de Requisição HTTP
**Sintoma**: `net_http_request_timedout, 300` - Requisições falhando após 5 minutos

**Causa**: O timeout padrão do HttpClient (100 segundos) é insuficiente para operações de sincronização que podem baixar grandes volumes de dados

**Correção Aplicada**: Aumentado o timeout do HttpClient para 10 minutos no `Program.cs` do Blazor:

```csharp
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
});
```

### 3. URL da API Não Configurada
**Sintoma**: O placeholder `__API_BASE_URL__` não estava sendo substituído

**Causa**: O arquivo `appsettings.json` do Blazor tinha um placeholder que deveria ser substituído durante o build/deploy

**Correção Aplicada**: Configurada a URL correta da API no `appsettings.json`:

```json
{
    "ApiBaseUrl": "https://pwa-camera-poc-api-production.up.railway.app"
}
```

## 📋 Checklist de Deploy

### Para a API (Render)

- [x] Configuração de CORS atualizada para aceitar subdomínios do Cloudflare Pages
- [ ] Verificar se as variáveis de ambiente AWS estão configuradas no Render:
  - `AWS__Region`
  - `AWS__BucketName`
  - `AWS__AccessKey`
  - `AWS__SecretKey`
- [ ] Confirmar que o health check `/health` está respondendo
- [ ] Verificar logs do Render para erros de inicialização

### Para o Frontend (Cloudflare Pages)

- [x] URL da API configurada corretamente no `appsettings.json`
- [x] Timeout do HttpClient aumentado para 10 minutos
- [ ] Verificar se o build está sendo executado com sucesso
- [ ] Confirmar que o service worker está sendo registrado corretamente
- [ ] Testar a aplicação em diferentes navegadores

## 🚀 Próximos Passos

1. **Fazer redeploy da API no Render**
   - As alterações no `Program.cs` precisam ser deployadas
   - Verificar se o build Docker está funcionando

2. **Fazer redeploy do Frontend no Cloudflare Pages**
   - As alterações no `Program.cs` e `appsettings.json` precisam ser deployadas
   - Verificar se a URL da API está correta

3. **Testar o fluxo completo**
   - Login
   - Sincronização de dados
   - Captura de fotos
   - Upload para S3

## 🔧 Configurações Adicionais Recomendadas

### Render (API)

Adicionar as seguintes variáveis de ambiente no painel do Render:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
AWS__Region=[sua-região-aws]
AWS__BucketName=[seu-bucket-s3]
AWS__AccessKey=[sua-access-key]
AWS__SecretKey=[sua-secret-key]
```

### Cloudflare Pages (Frontend)

Se necessário, adicionar variáveis de ambiente no painel do Cloudflare Pages:

```
API_BASE_URL=https://pwa-camera-poc-api-production.up.railway.app
```

## 📝 Notas Importantes

1. **CORS**: A configuração atual permite todos os subdomínios do Cloudflare Pages. Se precisar de mais segurança, considere listar explicitamente os domínios permitidos.

2. **Timeout**: O timeout de 10 minutos é adequado para sincronizações grandes, mas pode ser ajustado conforme necessário.

3. **URL da API**: Certifique-se de que a URL da API no `appsettings.json` corresponde ao domínio real do Render.

4. **Ambiente de Desenvolvimento**: A configuração de CORS permite todas as origens em desenvolvimento (`IsDevelopment()`), facilitando testes locais.

## 🐛 Troubleshooting

### Se ainda houver erros de CORS:

1. Verificar os logs do Render para confirmar que a API está recebendo as requisições
2. Usar as ferramentas de desenvolvedor do navegador para inspecionar os headers da requisição
3. Confirmar que o header `Origin` está sendo enviado corretamente

### Se houver timeout:

1. Verificar se a API está respondendo dentro do tempo esperado
2. Considerar implementar paginação ou chunking para grandes volumes de dados
3. Monitorar o uso de memória e CPU no Render

### Se a URL da API não estiver funcionando:

1. Verificar se o domínio do Render está correto
2. Testar a API diretamente usando Postman ou curl
3. Confirmar que o health check `/health` está respondendo
