# Release Notes - Versão 0.2.4

**Data de Lançamento**: 12 de Março de 2026  
**Tipo**: Patch Release (Correção de Bugs)

## 🎯 Resumo

Esta versão corrige problemas críticos relacionados ao fluxo de logout e erros de integridade do Service Worker que estavam impactando a experiência do usuário.

## 🐛 Correções de Bugs

### 1. Redirecionamento após Logout
**Problema**: Usuários não eram redirecionados para a tela de login após fazer logout, ficando presos na aplicação.

**Solução**:
- Adicionado `forceLoad: true` em todos os redirecionamentos de logout
- Corrigido URL no componente `RedirectToLogin` (de `"login"` para `"/login"`)
- Garantia de limpeza completa do estado da aplicação

**Arquivos Modificados**:
- `Components/Layout/NavMenu.razor`
- `Components/Shared/RedirectToLogin.razor`

### 2. Erros de Integridade do Service Worker
**Problema**: Console do navegador exibia múltiplos erros de SRI (Subresource Integrity) relacionados a arquivos `.pdb`, `.wasm` e do framework Blazor.

**Solução**:
- Service Worker agora ignora arquivos do framework Blazor
- Arquivos `.pdb`, `.wasm` e `_framework/` são gerenciados diretamente pelo navegador
- Eliminação de tentativas de cache desnecessárias

**Arquivos Modificados**:
- `wwwroot/service-worker.js` (v0.2.2 → v0.2.3)

## 📊 Impacto

### Antes
- ❌ Logout não redirecionava para login
- ❌ Console cheio de erros de integridade
- ❌ Experiência de usuário inconsistente
- ❌ Possível confusão sobre estado da sessão

### Depois
- ✅ Logout redireciona corretamente
- ✅ Console limpo, sem erros
- ✅ Experiência de usuário fluida
- ✅ Estado da aplicação sempre consistente

## 🔄 Instruções de Atualização

### Para Desenvolvedores

1. Faça pull das últimas mudanças:

```bash
git pull origin main
```

2. Restaure as dependências:
```bash
dotnet restore
```

3. Compile o projeto:
```bash
dotnet build
```

4. Execute localmente:
```bash
dotnet run
```

### Para Usuários Finais

1. **Limpe o cache do navegador**:
   - Chrome/Edge: `Ctrl + Shift + Delete`
   - Selecione "Imagens e arquivos em cache"
   - Clique em "Limpar dados"

2. **Recarregue a aplicação**:
   - Pressione `Ctrl + F5` (hard reload)
   - Ou `F5` duas vezes

3. **Verifique a versão**:
   - A versão 0.2.4 deve aparecer no rodapé ou nas configurações

## 🧪 Testes Realizados

### Cenários de Teste

✅ **Teste 1: Logout via Menu Lateral**
- Fazer login
- Abrir menu lateral
- Clicar em "Sair"
- Verificar redirecionamento para `/login`

✅ **Teste 2: Logout via Página de Configurações**
- Fazer login
- Navegar para Configurações
- Clicar em "Sair"
- Verificar redirecionamento para `/login`

✅ **Teste 3: Console do Navegador**
- Abrir DevTools (F12)
- Verificar ausência de erros de integridade
- Confirmar que Service Worker está ativo

✅ **Teste 4: Novo Login após Logout**
- Fazer logout
- Fazer login novamente
- Verificar que a sessão é iniciada corretamente

## 📝 Notas Técnicas

### Service Worker
- **Versão**: 0.2.4
- **Cache Name**: `aspec-captura-v0-2-4`
- **Estratégia**: Cache First para app shell, Network First para API

### Arquivos Ignorados pelo SW
- `*.pdb` - Símbolos de debug
- `*.wasm` - WebAssembly modules
- `_framework/*` - Framework Blazor
- `blazor.*` - Scripts do Blazor

### Compatibilidade
- ✅ Chrome 90+
- ✅ Edge 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Chrome Mobile
- ✅ Safari iOS

## 🔗 Links Relacionados

- [CHANGELOG.md](./CHANGELOG.md) - Histórico completo de versões
- [BUGFIX_LOGOUT_REDIRECT.md](./BUGFIX_LOGOUT_REDIRECT.md) - Análise detalhada do bug
- [README.md](../README.md) - Documentação principal

## 👥 Contribuidores

- Correções implementadas pela equipe de desenvolvimento
- Testes realizados pela equipe de QA
- Documentação atualizada

## 📅 Próximos Passos

### Versão 0.2.4 (Planejada)
- [ ] Indicador visual de tempo de sessão
- [ ] Aviso antes do logout automático por inatividade
- [ ] Melhorias na sincronização offline
- [ ] Otimização de performance do Service Worker

### Versão 0.3.0 (Planejada)
- [ ] Suporte a múltiplos idiomas
- [ ] Modo offline completo
- [ ] Sincronização em background
- [ ] Push notifications

## 🆘 Suporte

Se você encontrar problemas após a atualização:

1. Limpe completamente o cache do navegador
2. Desinstale e reinstale o PWA (se instalado)
3. Verifique o console do navegador para erros
4. Reporte issues no repositório do projeto

---

**Versão**: 0.2.4  
**Build**: 0.2.4.0  
**Data**: 2026-03-12
