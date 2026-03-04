# Troubleshooting - Configuração de Sessão

Guia de resolução de problemas relacionados à tela de Configuração de Sessão.

## 🚨 Problema: Campos Vazios na Tela de Configuração de Sessão Após Novo Login

### Sintomas
- Usuário faz logout e login novamente
- Após sincronização de tombamentos, navega para `/configuracao-sessao`
- Os dropdowns (Órgão, Unidade Orçamentária, Área, Subárea) aparecem vazios
- Dados não são exibidos mesmo após login bem-sucedido

### Causa Raiz Identificada

#### 1. **Estado da Sessão Anterior Não Era Limpo**
```csharp
// ❌ CÓDIGO PROBLEMÁTICO
public class AppState
{
    public Models.Orgao? CurrentOrgao { get; set; }
    public Models.UnidadeOrcamentaria? CurrentUO { get; set; }
    public Models.Area? CurrentArea { get; set; }
    public Models.Subarea? CurrentSubarea { get; set; }
    // Sem método para limpar estes dados
}
```

**Problema:**
- O `AppState` é um serviço `Scoped` que mantém dados entre navegações
- Quando um novo login é feito, os dados antigos permaneciam no `AppState`
- A propriedade `EsferaAtual` não era atualizada corretamente

#### 2. **LoginAsync Não Limpava Dados Anteriores**
```csharp
// ❌ CÓDIGO PROBLEMÁTICO
public async Task<Usuario?> LoginAsync(string username, string password)
{
    // ... código de autenticação ...
    
    // Salva novo usuário mas não limpa estado anterior
    await localStorage.SetItemAsync(SESSION_KEY, session);
    
    // AppState mantém dados da sessão anterior
    return user;
}
```

**Problema:**
- Dados de configuração de sessão anterior permaneciam no localStorage
- `AppState` não era resetado, mantendo referências antigas
- `EsferaAtual` não era atualizada com a esfera do novo usuário

#### 3. **ConfiguracaoSessao Não Inicializava AppState**
```csharp
// ❌ CÓDIGO PROBLEMÁTICO
protected override async Task OnInitializedAsync()
{
    _usuario = await AuthService.GetCurrentUserAsync();
    if (_usuario == null)
    {
        Navigation.NavigateTo("/login");
    }
    // Não atualiza appState.EsferaAtual
}
```

**Problema:**
- A página carregava o usuário do localStorage
- Mas não garantia que `appState.EsferaAtual` estivesse sincronizado
- Dropdowns dependem de `appState.EsferaAtual` para filtrar dados

---

## ✅ Correções Aplicadas

### Correção 1: Método ClearSessionData no AppState

**ANTES:**
```csharp
public class AppState
{
    public Models.Orgao? CurrentOrgao { get; set; }
    public Models.UnidadeOrcamentaria? CurrentUO { get; set; }
    public Models.Area? CurrentArea { get; set; }
    public Models.Subarea? CurrentSubarea { get; set; }
    // Sem método de limpeza
}
```

**DEPOIS:**
```csharp
public class AppState
{
    public Models.Orgao? CurrentOrgao { get; set; }
    public Models.UnidadeOrcamentaria? CurrentUO { get; set; }
    public Models.Area? CurrentArea { get; set; }
    public Models.Subarea? CurrentSubarea { get; set; }

    /// <summary>
    /// Limpa todos os dados da sessão atual (hierarquia contábil)
    /// Deve ser chamado ao fazer logout ou novo login
    /// </summary>
    public void ClearSessionData()
    {
        CurrentOrgao = null;
        CurrentUO = null;
        CurrentArea = null;
        CurrentSubarea = null;
        EsferaAtual = null;
        NotifyStateChanged();
    }
}
```

### Correção 2: LoginAsync Limpa Estado Anterior

**ANTES:**
```csharp
public async Task<Usuario?> LoginAsync(string username, string password)
{
    // ... autenticação ...
    
    await localStorage.SetItemAsync(SESSION_KEY, session);
    
    if (authStateProvider is CustomAuthStateProvider customProvider)
    {
        customProvider.NotifyAuthenticationStateChanged();
    }
    
    return user;
}
```

**DEPOIS:**
```csharp
public async Task<Usuario?> LoginAsync(string username, string password)
{
    // ... autenticação ...
    
    await localStorage.SetItemAsync(SESSION_KEY, session);
    
    // ✅ Limpar dados de sessão anterior no localStorage
    await localStorage.RemoveItemAsync("session-config");
    
    // ✅ Limpar estado da sessão anterior no AppState
    appState.ClearSessionData();
    appState.EsferaAtual = user.Esfera;
    
    if (authStateProvider is CustomAuthStateProvider customProvider)
    {
        customProvider.NotifyAuthenticationStateChanged();
    }
    
    return user;
}
```

### Correção 3: LogoutAsync Limpa AppState

**ANTES:**
```csharp
public async Task LogoutAsync()
{
    await localStorage.RemoveItemAsync(SESSION_KEY);
    if (authStateProvider is CustomAuthStateProvider customProvider)
    {
        customProvider.NotifyAuthenticationStateChanged();
    }
}
```

**DEPOIS:**
```csharp
public async Task LogoutAsync()
{
    await localStorage.RemoveItemAsync(SESSION_KEY);
    await localStorage.RemoveItemAsync("session-config");
    
    // ✅ Limpar estado da sessão ao fazer logout
    appState.ClearSessionData();
    
    if (authStateProvider is CustomAuthStateProvider customProvider)
    {
        customProvider.NotifyAuthenticationStateChanged();
    }
}
```

### Correção 4: ConfiguracaoSessao Inicializa AppState

**ANTES:**
```csharp
protected override async Task OnInitializedAsync()
{
    _usuario = await AuthService.GetCurrentUserAsync();
    if (_usuario == null)
    {
        Navigation.NavigateTo("/login");
    }
}
```

**DEPOIS:**
```csharp
protected override async Task OnInitializedAsync()
{
    _usuario = await AuthService.GetCurrentUserAsync();
    if (_usuario == null)
    {
        Navigation.NavigateTo("/login");
        return;
    }

    // ✅ Garantir que o AppState tenha a esfera atualizada
    if (string.IsNullOrEmpty(appState.EsferaAtual))
    {
        appState.EsferaAtual = _usuario.Esfera;
    }

    // ✅ Forçar atualização da UI após carregar os dados
    StateHasChanged();
}
```

### Correção 5: Injeção de AppState no AuthService

**ANTES:**
```csharp
public class AuthService(
    ILocalStorageService localStorage, 
    AuthenticationStateProvider authStateProvider, 
    IHttpClientFactory httpClientFactory, 
    IIndexedDbService dbService) : IAuthService
```

**DEPOIS:**
```csharp
public class AuthService(
    ILocalStorageService localStorage, 
    AuthenticationStateProvider authStateProvider, 
    IHttpClientFactory httpClientFactory, 
    IIndexedDbService dbService, 
    AppState appState) : IAuthService
```

---

## 🔍 Como Diagnosticar

### 1. Verificar Estado do AppState

```csharp
// No console do navegador (F12)
// Adicione breakpoint em ConfiguracaoSessao.razor OnInitializedAsync
// Verifique:
Console.WriteLine($"Usuario: {_usuario?.UsuarioNome}");
Console.WriteLine($"Esfera Usuario: {_usuario?.Esfera}");
Console.WriteLine($"Esfera AppState: {appState.EsferaAtual}");
Console.WriteLine($"Orgaos Count: {_usuario?.Orgaos.Count}");
Console.WriteLine($"Orgaos Filtrados Count: {Orgaos.Count}");
```

### 2. Verificar LocalStorage

```javascript
// No console do navegador (F12)
// Verificar se há dados de sessão anterior
localStorage.getItem('session-config');
localStorage.getItem('pwa-inventory-session');
localStorage.getItem('user-esfera');
```

### 3. Verificar Fluxo de Navegação

```
Login → TombamentosSync → ConfiguracaoSessao
  ↓           ↓                    ↓
LoginAsync  SyncAsync         OnInitializedAsync
  ↓           ↓                    ↓
ClearState  (mantém)         Verifica EsferaAtual
```

---

## 🧪 Testes de Validação

### Teste 1: Login Inicial

```csharp
// Cenário: Primeiro login do usuário
1. Fazer login com credenciais válidas
2. Aguardar sincronização de tombamentos
3. Navegar para /configuracao-sessao
4. Verificar se dropdowns estão populados
5. Verificar se appState.EsferaAtual == usuario.Esfera
```

### Teste 2: Logout e Novo Login

```csharp
// Cenário: Logout e login com mesmo usuário
1. Fazer login inicial
2. Configurar sessão (selecionar Órgão, UO, Área, Subárea)
3. Fazer logout
4. Fazer login novamente com mesmo usuário
5. Aguardar sincronização
6. Navegar para /configuracao-sessao
7. Verificar se dropdowns estão vazios (resetados)
8. Verificar se appState.CurrentOrgao == null
```

### Teste 3: Troca de Usuário

```csharp
// Cenário: Logout e login com usuário diferente
1. Fazer login com usuário A (esfera E)
2. Configurar sessão
3. Fazer logout
4. Fazer login com usuário B (esfera L)
5. Aguardar sincronização
6. Navegar para /configuracao-sessao
7. Verificar se dropdowns mostram dados do usuário B
8. Verificar se appState.EsferaAtual == "L"
```

### Teste 4: Navegação Direta

```csharp
// Cenário: Navegação direta para /configuracao-sessao
1. Fazer login
2. Navegar diretamente para /configuracao-sessao (sem passar por sync)
3. Verificar se dropdowns estão populados
4. Verificar se appState.EsferaAtual está definido
```

---

## 📋 Checklist de Verificação

Antes de considerar o problema resolvido, verifique:

- [ ] `AppState.ClearSessionData()` é chamado no `LoginAsync`
- [ ] `AppState.ClearSessionData()` é chamado no `LogoutAsync`
- [ ] `appState.EsferaAtual` é definido no `LoginAsync`
- [ ] `appState.EsferaAtual` é verificado no `OnInitializedAsync` de `ConfiguracaoSessao`
- [ ] `localStorage.RemoveItemAsync("session-config")` é chamado no `LoginAsync`
- [ ] `localStorage.RemoveItemAsync("session-config")` é chamado no `LogoutAsync`
- [ ] Dropdowns aparecem vazios após novo login (comportamento esperado)
- [ ] Dropdowns são populados corretamente após carregar usuário
- [ ] Filtro por esfera funciona corretamente

---

## 🚀 Melhorias Futuras

### 1. Persistência de Configuração de Sessão

```csharp
// Salvar última configuração de sessão para restaurar após login
public async Task SaveSessionConfigAsync(SessionConfig config)
{
    await localStorage.SetItemAsync($"session-config-{_usuario.UsuarioNome}", config);
}

public async Task<SessionConfig?> GetLastSessionConfigAsync()
{
    return await localStorage.GetItemAsync<SessionConfig>($"session-config-{_usuario.UsuarioNome}");
}
```

### 2. Validação de Dados

```csharp
// Validar se os dados de hierarquia ainda existem para o usuário
public bool ValidateSessionConfig(SessionConfig config, Usuario usuario)
{
    var orgao = usuario.Orgaos.FirstOrDefault(o => o.IdOrgao == config.OrgaoId);
    if (orgao == null) return false;
    
    var uo = orgao.UnidadesOrcamentarias.FirstOrDefault(u => u.IdUO == config.UOId);
    if (uo == null) return false;
    
    // ... validar área e subárea
    
    return true;
}
```

### 3. Logging de Estado

```csharp
// Adicionar logs para debug
public void LogStateChange(string action)
{
    Console.WriteLine($"[AppState] {action}");
    Console.WriteLine($"  EsferaAtual: {EsferaAtual}");
    Console.WriteLine($"  CurrentOrgao: {CurrentOrgao?.NomeOrgao ?? "null"}");
    Console.WriteLine($"  CurrentUO: {CurrentUO?.NomeUO ?? "null"}");
    Console.WriteLine($"  CurrentArea: {CurrentArea?.NomeArea ?? "null"}");
    Console.WriteLine($"  CurrentSubarea: {CurrentSubarea?.NomeSubarea ?? "null"}");
}
```

---

## 📞 Suporte

**Se o problema persistir:**

1. Limpar cache do navegador (Ctrl+Shift+Delete)
2. Verificar console do navegador (F12) para erros JavaScript
3. Verificar se `AppState` está registrado como `Scoped` no `Program.cs`
4. Verificar se `AuthService` está recebendo `AppState` via DI
5. Adicionar breakpoints em `LoginAsync`, `LogoutAsync` e `OnInitializedAsync`

**Arquivos Relacionados:**
- `pwa-camera-poc-blazor/Services/Auth/AuthService.cs` - Lógica de autenticação
- `pwa-camera-poc-blazor/Services/AppState.cs` - Estado global da aplicação
- `pwa-camera-poc-blazor/Pages/ConfiguracaoSessao.razor` - Tela de configuração
- `pwa-camera-poc-blazor/Pages/Login.razor` - Tela de login
- `pwa-camera-poc-blazor/Pages/TombamentosSync.razor` - Sincronização

---

**Última Atualização:** 2026-03-04  
**Versão:** 0.2.2  
**Autor:** Equipe ASPEC
