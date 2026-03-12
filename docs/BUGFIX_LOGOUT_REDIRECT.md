# Correção de Bug - Redirecionamento após Logout

## Data: 2026-03-12

## Problema Identificado

Após fazer logout, o usuário não era redirecionado para a tela de login, ficando preso na aplicação.

### Sintomas
- Clicar em "Sair" no menu lateral não redirecionava para login
- Console do navegador mostrava erros de integridade do Service Worker
- Arquivos `.pdb` e `.wasm` causavam falhas de SRI (Subresource Integrity)

### Causa Raiz

1. **NavMenu.razor**: Faltava `forceLoad: true` no `NavigateTo` após logout
2. **RedirectToLogin.razor**: URL sem barra inicial (`"login"` ao invés de `"/login"`)
3. **Service Worker**: Tentava cachear arquivos do framework Blazor causando conflitos

## Soluções Implementadas

### ✅ 1. Correção do NavMenu.razor

**Arquivo**: `Components/Layout/NavMenu.razor`

**Antes**:
```csharp
private async Task HandleLogout()
{
    await AuthService.LogoutAsync();
    await OnMenuItemClick.InvokeAsync();
    Navigation.NavigateTo("/login");
}
```

**Depois**:
```csharp
private async Task HandleLogout()
{
    await AuthService.LogoutAsync();
    await OnMenuItemClick.InvokeAsync();
    Navigation.NavigateTo("/login", forceLoad: true);
}
```


### ✅ 2. Correção do RedirectToLogin.razor

**Arquivo**: `Components/Shared/RedirectToLogin.razor`

**Antes**:
```csharp
Navigation.NavigateTo("login");
```

**Depois**:
```csharp
Navigation.NavigateTo("/login", forceLoad: true);
```

### ✅ 3. Correção do Service Worker

**Arquivo**: `wwwroot/service-worker.js`

**Mudança**: Adicionado filtro para ignorar arquivos do framework Blazor

```javascript
// Skip .pdb, .wasm files and framework files (let browser handle them)
if (url.pathname.endsWith('.pdb') || 
    url.pathname.endsWith('.wasm') || 
    url.pathname.includes('_framework/') ||
    url.pathname.includes('blazor.')) {
    return;
}
```

**Versão atualizada**: 0.2.2 → 0.2.4

## Resultado

✅ Logout redireciona corretamente para `/login`  
✅ Página recarrega completamente (`forceLoad: true`)  
✅ Service Worker não interfere com arquivos do framework  
✅ Erros de integridade eliminados  
✅ Estado da aplicação limpo após logout  

## Como Testar

1. Faça login na aplicação
2. Clique no menu lateral (ícone de hambúrguer)
3. Clique em "Sair"
4. Confirme o logout
5. Verifique se foi redirecionado para `/login`
6. Verifique o console do navegador (não deve haver erros de integridade)

## Arquivos Modificados

1. `Components/Layout/NavMenu.razor` - Adicionado `forceLoad: true`
2. `Components/Shared/RedirectToLogin.razor` - Corrigido URL e adicionado `forceLoad`
3. `wwwroot/service-worker.js` - Filtro para arquivos do framework

## Próximos Passos

- Testar logout em diferentes navegadores
- Verificar se o cache é limpo corretamente
- Considerar adicionar animação de transição no logout
