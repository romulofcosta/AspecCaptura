# Configuração de URL da API - ASPEC Captura

## Como Funciona

O projeto usa um sistema de configuração em camadas para a URL da API:

### Arquivos de Configuração

1. **`wwwroot/appsettings.json`** (Produção)
   - Contém o placeholder `__API_BASE_URL__`
   - É substituído pelo `build.sh` durante o deploy
   - Commitado no Git

2. **`wwwroot/appsettings.Development.json`** (Desenvolvimento Local)
   - Contém `http://localhost:5069`
   - Sobrescreve o appsettings.json apenas em desenvolvimento
   - **NÃO é commitado no Git** (está no .gitignore)

### Fluxo de Desenvolvimento Local

Quando você roda o projeto localmente:

```bash
dotnet run
```

O Blazor WebAssembly carrega automaticamente:
1. `appsettings.json` (com placeholder)
2. `appsettings.Development.json` (sobrescreve com localhost)

Resultado: A aplicação chama `http://localhost:5069`

### Fluxo de Build/Deploy (build.sh)

Quando o `build.sh` é executado:

```bash
./build.sh
```

O script:
1. Lê a variável de ambiente `API_BASE_URL`
2. Substitui `__API_BASE_URL__` no `wwwroot/appsettings.json`
3. Faz o build/publish
4. O `appsettings.Development.json` **NÃO é incluído** no build de produção

Resultado: A aplicação publicada usa a URL configurada em `API_BASE_URL`

### Código Responsável (Program.cs)

```csharp
builder.Services.AddHttpClient("BackendApi", client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(apiBaseUrl) || apiBaseUrl == "__API_BASE_URL__")
    {
        apiBaseUrl = "http://localhost:5069";
    }
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromMinutes(10);
});
```

### Configuração por Ambiente

| Ambiente | Arquivo Usado | URL da API |
|----------|---------------|------------|
| Desenvolvimento Local | `appsettings.Development.json` | `http://localhost:5069` |
| Build Local (sem API_BASE_URL) | `appsettings.json` (fallback no código) | `http://localhost:5069` |
| Netlify/Cloudflare | `appsettings.json` (substituído) | Valor de `$API_BASE_URL` |
| Railway | `appsettings.json` (substituído) | Valor de `$API_BASE_URL` |

### Como Alterar a URL Local

Se precisar usar uma porta diferente localmente:

1. Edite `wwwroot/appsettings.Development.json`:
   ```json
   {
       "ApiBaseUrl": "http://localhost:NOVA_PORTA"
   }
   ```

2. Ou crie um `.env` e configure no código (futuro)

### Troubleshooting

**Problema**: App local está chamando API de produção

**Solução**: 
1. Verifique se existe `wwwroot/appsettings.Development.json`
2. Se não existir, crie com:
   ```json
   {
       "ApiBaseUrl": "http://localhost:5069"
   }
   ```

**Problema**: Build de produção está usando localhost

**Solução**:
1. Verifique se `wwwroot/appsettings.json` tem `__API_BASE_URL__`
2. Verifique se a variável `API_BASE_URL` está definida no ambiente de build
3. Verifique os logs do `build.sh` para ver se a substituição funcionou

### Segurança

- ✅ `appsettings.Development.json` está no `.gitignore`
- ✅ `appsettings.json` usa placeholder, não URL real
- ✅ URL de produção é injetada via variável de ambiente no build
- ✅ Nenhuma URL sensível é commitada no Git

### Referências

- Script de build: `build.sh` (linhas 35-50)
- Configuração do HttpClient: `Program.cs` (linhas 27-36)
- Gitignore: `.gitignore` (seção "Configuração de desenvolvimento")
