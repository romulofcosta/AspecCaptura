# 📌 Sistema de Versionamento

## 🎯 Como a Versão é Exibida no Frontend

A versão do aplicativo é exibida na página **Ajustes** (Settings) do PWA.

### Localização
- **Página:** `/settings`
- **Componente:** `Pages/Settings.razor`
- **Seção:** "SOBRE O APP"

### Visualização
```
┌─────────────────────────────┐
│  🏢 Aspec Captura          │
│     Versão 0.8.0           │
├─────────────────────────────┤
│  STATUS DA LICENÇA: ATIVA  │
│  ÚLTIMA SINCRONIZAÇÃO: ... │
└─────────────────────────────┘
```

## 🔧 Como Funciona

### 1. Definição da Versão

A versão é definida em **múltiplos locais** para garantir consistência:

#### Frontend (PWA)
```xml
<!-- pwa-camera-poc-blazor.csproj -->
<PropertyGroup>
  <Version>0.8.0</Version>
  <AssemblyVersion>0.8.0.0</AssemblyVersion>
  <FileVersion>0.8.0.0</FileVersion>
  <InformationalVersion>0.8.0</InformationalVersion>
</PropertyGroup>
```

```json
// wwwroot/manifest.json
{
  "version": "0.8.0"
}
```

```html
<!-- wwwroot/index.html -->
<meta name="version" content="0.8.0" />
```

```json
// package.json
{
  "version": "0.8.0"
}
```

#### Backend (API)
```xml
<!-- pwa-camera-poc-api.csproj -->
<PropertyGroup>
  <Version>0.8.0</Version>
  <AssemblyVersion>0.8.0.0</AssemblyVersion>
  <FileVersion>0.8.0.0</FileVersion>
  <InformationalVersion>0.8.0</InformationalVersion>
</PropertyGroup>
```

```csharp
// Program.cs - Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ASPEC Capture API", Version = "0.8.0" });
});
```

### 2. Leitura da Versão no Frontend

A classe `AppInfo` lê a versão automaticamente do assembly:

```csharp
// Services/AppInfo.cs
public class AppInfo : IAppInfo
{
    public string Version { get; private set; }
    
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
```

### 3. Injeção de Dependência

O `AppInfo` é registrado como Singleton:

```csharp
// Program.cs
builder.Services.AddSingleton<IAppInfo, AppInfo>();
```

E injetado globalmente:

```razor
<!-- _Imports.razor -->
@inject pwa_camera_poc_blazor.Services.IAppInfo AppInfo
```

### 4. Uso na Interface

```razor
<!-- Pages/Settings.razor -->
<div class="about-header">
    <img src="images/aspec_logo.png" alt="Logo" class="about-logo" />
    <div>
        <p class="about-name">@AppInfo.AppName</p>
        <p class="about-version">Versão @AppInfo.Version</p>
    </div>
</div>
```

## 🔄 Fluxo de Atualização

```
1. Atualizar versão em .csproj
   ↓
2. Build do projeto
   ↓
3. Assembly é gerado com nova versão
   ↓
4. AppInfo lê do assembly em runtime
   ↓
5. Versão é exibida na página Settings
```

## ✅ Vantagens desta Abordagem

1. **Fonte Única de Verdade**: A versão no `.csproj` é a fonte principal
2. **Automático**: Não precisa atualizar manualmente em múltiplos lugares
3. **Consistente**: Sempre reflete a versão do assembly compilado
4. **Fallback**: Se falhar, usa versão padrão (0.8.0)

## 📋 Checklist de Atualização de Versão

Ao atualizar a versão, modifique:

### Frontend
- [ ] `pwa-camera-poc-blazor.csproj` (Version, AssemblyVersion, FileVersion, InformationalVersion)
- [ ] `wwwroot/manifest.json` (version)
- [ ] `wwwroot/index.html` (meta version)
- [ ] `package.json` (version)
- [ ] `build.sh` (VERSION default)
- [ ] `CHANGELOG.md` (nova entrada)

### Backend
- [ ] `pwa-camera-poc-api.csproj` (Version, AssemblyVersion, FileVersion, InformationalVersion)
- [ ] `Program.cs` (Swagger Version)

### Documentação
- [ ] `CHANGELOG.md` (ambos os projetos)
- [ ] `VERSION_UPDATE.md` (se aplicável)

## 🧪 Como Testar

### 1. Desenvolvimento Local
```bash
dotnet run
# Acesse: http://localhost:5230/settings
# Verifique a versão exibida
```

### 2. Build de Produção
```bash
./build.sh
# Verifique o manifest.json no output:
cat bin/Release/net8.0/publish/wwwroot/manifest.json | grep version
```

### 3. Produção
```
1. Acesse: https://pwa-camera-poc-blazor.pages.dev/settings
2. Role até "SOBRE O APP"
3. Verifique: "Versão 0.8.0"
```

### 4. Verificar Assembly
```csharp
// No código C#
var assembly = Assembly.GetExecutingAssembly();
var version = assembly.GetName().Version;
Console.WriteLine($"Assembly Version: {version}");
```

## 🔍 Troubleshooting

### Versão não atualiza na página Settings

**Causa:** Cache do navegador ou service worker

**Solução:**
1. Limpe o cache do navegador (Ctrl+Shift+Delete)
2. Desregistre o service worker:
   - DevTools > Application > Service Workers > Unregister
3. Recarregue a página (Ctrl+F5)

### Versão mostra valor antigo

**Causa:** Build não foi executado ou assembly não foi atualizado

**Solução:**
1. Limpe o build: `dotnet clean`
2. Rebuild: `dotnet build -c Release`
3. Verifique o .csproj: `cat pwa-camera-poc-blazor.csproj | grep Version`

### AppInfo retorna null

**Causa:** Serviço não está registrado ou injeção falhou

**Solução:**
1. Verifique Program.cs: `builder.Services.AddSingleton<IAppInfo, AppInfo>();`
2. Verifique _Imports.razor: `@inject pwa_camera_poc_blazor.Services.IAppInfo AppInfo`
3. Rebuild o projeto

## 📚 Referências

- [Semantic Versioning](https://semver.org/)
- [Assembly Versioning](https://learn.microsoft.com/en-us/dotnet/standard/assembly/versioning)
- [PWA Manifest](https://developer.mozilla.org/en-US/docs/Web/Manifest)

## 🔗 Arquivos Relacionados

- `Services/AppInfo.cs` - Classe que lê a versão
- `Pages/Settings.razor` - Página que exibe a versão
- `Program.cs` - Registro do serviço
- `_Imports.razor` - Injeção global
- `pwa-camera-poc-blazor.csproj` - Definição da versão
- `CHANGELOG.md` - Histórico de versões

---

**Versão Atual:** 0.8.0  
**Última Atualização:** 8 de Fevereiro de 2025
