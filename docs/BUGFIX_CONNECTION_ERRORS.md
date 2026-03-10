# Correção de Erros de Conexão - API Backend

## Data: 2026-03-09

## Problema Identificado

A aplicação estava apresentando erros de conexão na tela de "Baixando Lotes de Tombamentos" e no login:

```
Failed to load resource: net::ERR_CONNECTION_REFUSED
Authentication error: TypeError: Failed to fetch
```

### Causa Raiz

A API backend não está rodando ou não está acessível em `http://localhost:5069`. A aplicação estava tentando se conectar mas não tinha tratamento adequado para erros de conexão, resultando em:

1. Mensagens de erro genéricas
2. Usuário preso na tela de sincronização
3. Experiência ruim quando offline ou sem API disponível

## Soluções Implementadas

### 1. ✅ Tratamento de Erro no TombamentosSync.razor

**Arquivo**: `Pages/TombamentosSync.razor`

**Melhorias**:
- Adicionado tratamento específico para `HttpRequestException` (erro de conexão)
- Adicionado tratamento para `TaskCanceledException` (timeout)
- Mensagens de erro mais claras e específicas
- Redirecionamento automático para `/configuracao-sessao` após erro
- Delay de 2 segundos para usuário ler a mensagem

```csharp
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"Erro de conexão: {ex.Message}");
    Snackbar.Add("Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.", Severity.Error);
    await Task.Delay(2000);
    Navigation.NavigateTo("/configuracao-sessao");
}
catch (TaskCanceledException ex)
{
    Console.Error.WriteLine($"Timeout na sincronização: {ex.Message}");
    Snackbar.Add("A sincronização demorou muito. Tente novamente mais tarde.", Severity.Warning);
    await Task.Delay(2000);
    Navigation.NavigateTo("/configuracao-sessao");
}
```

### 2. ✅ Tratamento de Erro no AuthService

**Arquivo**: `Services/Auth/AuthService.cs`

**Melhorias**:
- Diferenciação entre tipos de erro:
  - `HttpRequestException`: Erro de conexão (servidor offline)
  - `TaskCanceledException`: Timeout (servidor lento)
  - `Exception`: Outros erros
- Mensagens específicas para cada tipo de erro
- Melhor experiência do usuário com feedback claro

```csharp
catch (HttpRequestException ex)
{
    return new LoginResult 
    { 
        Success = false, 
        ErrorMessage = "Não foi possível conectar ao servidor. Verifique sua conexão com a internet." 
    };
}
catch (TaskCanceledException ex)
{
    return new LoginResult 
    { 
        Success = false, 
        ErrorMessage = "A conexão demorou muito. Tente novamente." 
    };
}
```

## Comportamento Atual

### Quando a API está offline:

1. **Login**:
   - Exibe: "Não foi possível conectar ao servidor. Verifique sua conexão com a internet."
   - Usuário pode tentar novamente

2. **Sincronização de Tombamentos**:
   - Exibe: "Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente."
   - Aguarda 2 segundos
   - Redireciona para `/configuracao-sessao` (permite uso offline)

### Quando a API está lenta:

1. **Login**:
   - Exibe: "A conexão demorou muito. Tente novamente."

2. **Sincronização**:
   - Exibe: "A sincronização demorou muito. Tente novamente mais tarde."
   - Redireciona para configuração de sessão

## Como Iniciar a API Backend

Para resolver o problema de conexão, você precisa iniciar a API backend:

### Opção 1: Usando o projeto pwa-camera-poc-api

```bash
cd pwa-camera-poc-api
dotnet run
```

A API deve iniciar em `http://localhost:5069`

### Opção 2: Configurar URL diferente

Se a API estiver em outra URL, edite `wwwroot/appsettings.json`:

```json
{
    "ApiBaseUrl": "https://sua-api.com"
}
```

## Modo Offline

A aplicação agora suporta melhor o modo offline:

1. Se a sincronização falhar, o usuário é redirecionado para configuração de sessão
2. Pode trabalhar com dados já sincronizados anteriormente
3. Itens capturados são salvos localmente e sincronizados quando a conexão retornar

## Próximos Passos Recomendados

1. **Iniciar a API backend** para testar o fluxo completo
2. **Implementar retry automático** com backoff exponencial
3. **Adicionar indicador de status da API** na UI
4. **Implementar sincronização em background** quando a conexão retornar
5. **Adicionar modo offline explícito** com banner informativo

## Arquivos Modificados

1. `Pages/TombamentosSync.razor` - Tratamento de erro de sincronização
2. `Services/Auth/AuthService.cs` - Tratamento de erro de autenticação

## Testes Recomendados

1. ✅ Testar login sem API rodando
2. ✅ Testar sincronização sem API rodando
3. ⏳ Testar login com API rodando
4. ⏳ Testar sincronização com API rodando
5. ⏳ Testar modo offline após sincronização bem-sucedida
