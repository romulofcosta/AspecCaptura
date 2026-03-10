# Guia Rápido de Logging - Aspec Captura

## Como Usar ILogger em Componentes Razor

### 1. Injetar ILogger

```csharp
@inject ILogger<NomeDoComponente> Logger
```

### 2. Usar em Métodos

```csharp
@code {
    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Componente inicializado");
            // ... código
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao inicializar componente");
        }
    }
}
```

---

## Níveis de Log

| Nível | Uso | Exemplo |
|-------|-----|---------|
| **Debug** | Informações detalhadas para depuração | `Logger.LogDebug("Valor da variável: {value}", value)` |
| **Information** | Informações gerais sobre fluxo | `Logger.LogInformation("Login bem-sucedido")` |
| **Warning** | Situações inesperadas mas recuperáveis | `Logger.LogWarning("Token expirado, renovando")` |
| **Error** | Erros que precisam de atenção | `Logger.LogError(ex, "Erro ao conectar API")` |
| **Critical** | Erros críticos que podem derrubar app | `Logger.LogCritical(ex, "Falha crítica")` |

---

## Exemplos Práticos

### Login
```csharp
Logger.LogInformation($"Login attempt for user: {username}");
Logger.LogWarning($"Login failed for user: {username}. Error: {error}");
Logger.LogError(ex, $"Connection error during login for user: {username}");
```

### Settings
```csharp
Logger.LogInformation("Settings page initialized");
Logger.LogWarning("CurrentUser is null in GetUserName");
Logger.LogError(ex, "Error initializing Settings page");
```

### AuthService
```csharp
Logger.LogInformation($"Logout initiated for user: {username}");
Logger.LogInformation("Token refreshed successfully");
Logger.LogWarning("Token refresh attempted but no token found");
Logger.LogError(ex, "Error refreshing token");
```

### Layout
```csharp
Logger.LogInformation("MinimalLayout initialized");
Logger.LogDebug($"Location changed to: {location}");
Logger.LogError(ex, "Error showing snackbar");
```

---

## Verificar Logs no Browser

### Chrome/Edge/Firefox
1. Pressione `F12` para abrir Developer Tools
2. Vá para a aba **Console**
3. Você verá logs como:
   ```
   [info] Login page initialized
   [warn] Login failed for user: admin
   [error] Connection error during login
   ```

### Filtrar Logs
- Digite no campo de busca: `Login`
- Clique em ícones de filtro para mostrar/ocultar níveis

### Limpar Logs
- Clique no ícone de lixeira ou pressione `Ctrl+L`

---

## Padrões de Logging

### ✅ BOM
```csharp
Logger.LogInformation($"Login attempt for user: {cleanUsername}");
Logger.LogError(ex, $"Connection error during login for user: {cleanUsername}");
Logger.LogWarning("Session will expire in 2 minutes due to inactivity");
```

### ❌ RUIM
```csharp
Console.WriteLine("Login");
Console.Error.WriteLine($"Error: {ex.Message}");
// Sem contexto, sem nível de log
```

---

## Estrutura de Mensagens

### Padrão Recomendado
```
[Ação] [Contexto] [Detalhes]
```

### Exemplos
```
[info] Login attempt for user: admin
[warn] Login failed for user: admin. Error: Invalid credentials
[error] Connection error during login for user: admin
[info] Settings page initialized
[info] App version loaded: v0.2.2
[debug] Location changed to: /settings
```

---

## Tratamento de Exceções com Logging

### Padrão Recomendado
```csharp
try
{
    Logger.LogInformation("Iniciando operação");
    // ... código
    Logger.LogInformation("Operação concluída com sucesso");
}
catch (HttpRequestException ex)
{
    Logger.LogError(ex, "Erro de conexão");
    // Retornar erro ao usuário
}
catch (Exception ex)
{
    Logger.LogError(ex, "Erro inesperado");
    // Retornar erro genérico ao usuário
}
```

---

## Logging de Estados Assíncronos

### Padrão Recomendado
```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        Logger.LogInformation("Componente inicializando");
        
        isLoading = true;
        StateHasChanged();
        
        var data = await LoadDataAsync();
        
        Logger.LogInformation("Dados carregados com sucesso");
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erro ao carregar dados");
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

---

## Logging de Eventos

### Padrão Recomendado
```csharp
private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
{
    Logger.LogDebug($"Location changed to: {e.Location}");
    InvokeAsync(StateHasChanged);
}

private void HandleAppStateChange()
{
    Logger.LogDebug("App state changed");
    InvokeAsync(StateHasChanged);
}
```

---

## Logging de Operações Críticas

### Autenticação
```csharp
Logger.LogInformation($"Login attempt for user: {username}");
Logger.LogInformation($"Login successful for user: {username}");
Logger.LogInformation($"Logout initiated for user: {username}");
Logger.LogInformation("Token refreshed successfully");
```

### Sincronização
```csharp
Logger.LogInformation("Sync started");
Logger.LogInformation($"Synced {count} items");
Logger.LogWarning("Sync failed, retrying");
Logger.LogError(ex, "Sync failed after retries");
```

### Cache
```csharp
Logger.LogInformation("Cache cleared");
Logger.LogWarning("Cache API not available");
Logger.LogError(ex, "Error clearing cache");
```

---

## Dicas de Performance

### ✅ Use Interpolação de String
```csharp
Logger.LogInformation($"User: {username}");
```

### ❌ Evite Concatenação
```csharp
Logger.LogInformation("User: " + username); // Menos eficiente
```

### ✅ Use Structured Logging
```csharp
Logger.LogInformation("Login attempt for user: {Username}", username);
```

---

## Troubleshooting

### Problema: Logs não aparecem

**Solução:**
1. Verifique se ILogger está injetado
2. Verifique se o nível de log é Information ou inferior
3. Recarregue a página (Ctrl+Shift+R)
4. Verifique se o logging está configurado em Program.cs

### Problema: Muitos logs

**Solução:**
1. Use LogDebug para informações detalhadas
2. Use filtro no console para buscar logs específicos
3. Aumente o nível de log em Program.cs

### Problema: Logs não estruturados

**Solução:**
1. Use padrão: `[Ação] [Contexto] [Detalhes]`
2. Inclua contexto relevante (username, ID, etc.)
3. Use níveis apropriados (Info, Warn, Error)

---

## Checklist de Logging

- [ ] ILogger injetado em componentes críticos
- [ ] OnInitializedAsync com logging
- [ ] Try/catch com logging de erros
- [ ] Eventos com logging
- [ ] Operações críticas com logging
- [ ] Mensagens estruturadas
- [ ] Níveis apropriados (Info, Warn, Error)
- [ ] Contexto claro em cada log
- [ ] Sem informações sensíveis (senhas, tokens)
- [ ] Logs testados no console do navegador

---

## Conclusão

Com este guia, você pode:

✅ Adicionar logging estruturado a novos componentes
✅ Depurar problemas rapidamente
✅ Monitorar fluxo de autenticação
✅ Rastrear erros e exceções
✅ Melhorar experiência do usuário

Para mais detalhes, consulte:
- `CONSOLE_ERRORS_FIXES_COMPLETED.md`
- `TESTING_CONSOLE_ERRORS.md`
- `CONSOLE_ERRORS_EXECUTIVE_SUMMARY.md`
