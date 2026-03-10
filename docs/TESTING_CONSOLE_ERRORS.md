# Guia de Testes - Correção de Erros de Console

## Objetivo

Validar que todas as correções de console foram implementadas corretamente e que não há erros de renderização, nulidade ou inicialização.

---

## Pré-requisitos

1. Projeto compilado sem erros
2. API rodando em `http://localhost:5069`
3. Aplicação rodando em `http://localhost:5230`
4. Browser com Developer Tools aberto (F12)

---

## Teste 1: Página de Login

### Passos:

1. Abra `http://localhost:5230/login`
2. Abra Developer Tools (F12)
3. Vá para a aba **Console**
4. Verifique os logs iniciais

### Logs Esperados:

```
[info] Login page initialized
[info] Theme loaded: light
```

### Validações:

- ✅ Nenhum erro de NullReferenceException
- ✅ Nenhum erro de inicialização
- ✅ Página renderiza corretamente
- ✅ Campos de entrada visíveis
- ✅ Botão de login visível
- ✅ Ícone de visibilidade de senha funciona

### Teste de Login com Credenciais Válidas:

1. Digite um usuário válido (ex: `admin`)
2. Digite uma senha válida
3. Clique em "Entrar"
4. Verifique os logs:

```
[info] Login attempt for user: admin
[info] Login successful for user: admin
```

### Teste de Login com Credenciais Inválidas:

1. Digite um usuário inválido
2. Digite uma senha inválida
3. Clique em "Entrar"
4. Verifique os logs:

```
[warn] Login failed for user: invalid. Error: Usuário ou senha inválidos
```

### Teste de Conexão Falha:

1. Desligue a API
2. Tente fazer login
3. Verifique os logs:

```
[error] Connection error during login for user: admin
```

---

## Teste 2: Página de Settings

### Passos:

1. Faça login com sucesso
2. Navegue para `/settings`
3. Abra Developer Tools (F12)
4. Vá para a aba **Console**
5. Verifique os logs iniciais

### Logs Esperados:

```
[info] Settings page initialized
[info] App version loaded: v0.2.2
[info] Settings loaded. User: [Nome do Usuário]
```

### Validações:

- ✅ Nenhum erro de NullReferenceException
- ✅ Nenhum erro de inicialização
- ✅ Página renderiza corretamente
- ✅ Nome do usuário exibido corretamente
- ✅ Email do usuário exibido corretamente
- ✅ Versão exibida corretamente
- ✅ Todos os botões visíveis

### Teste de Logout:

1. Clique no botão "Sair"
2. Confirme o logout
3. Verifique os logs:

```
[info] Logout initiated for user: admin
[info] Logout completed successfully
```

### Teste de Mudança de Tema:

1. Clique em "Tema"
2. Verifique os logs:

```
[debug] App state changed
```

### Teste de Limpeza de Cache:

1. Clique em "Limpar Cache"
2. Confirme a ação
3. Verifique os logs:

```
[info] Clear cache requested
[info] Cache cleared successfully
```

---

## Teste 3: MinimalLayout

### Passos:

1. Navegue para qualquer página com MinimalLayout (ex: `/settings`)
2. Abra Developer Tools (F12)
3. Vá para a aba **Console**

### Logs Esperados:

```
[info] MinimalLayout initialized
[info] Theme loaded: light
```

### Validações:

- ✅ Layout renderiza corretamente
- ✅ Bottom navigation visível
- ✅ Nenhum erro de renderização

### Teste de Navegação:

1. Clique em diferentes abas do bottom navigation
2. Verifique os logs:

```
[debug] Location changed to: http://localhost:5230/settings
[debug] App state changed
```

---

## Teste 4: AuthMinimalLayout

### Passos:

1. Abra `http://localhost:5230/login`
2. Abra Developer Tools (F12)
3. Vá para a aba **Console**

### Logs Esperados:

```
[info] AuthMinimalLayout initialized
[info] App version loaded: v0.2.2
```

### Validações:

- ✅ Versão exibida corretamente
- ✅ Versão é dinâmica (não hardcoded)
- ✅ Nenhum erro de renderização

---

## Teste 5: AuthService

### Passos:

1. Faça login
2. Abra Developer Tools (F12)
3. Vá para a aba **Console**

### Logs Esperados Durante Login:

```
[info] Login attempt for user: admin
[info] Login successful for user: admin
```

### Logs Esperados Durante Logout:

```
[info] Logout initiated for user: admin
[info] Logout completed successfully
```

### Logs Esperados Durante Refresh de Token:

```
[info] Attempting to refresh token
[info] Token refreshed successfully
```

### Validações:

- ✅ Todos os logs aparecem no console
- ✅ Nenhum erro de exceção
- ✅ Mensagens de erro são claras

---

## Teste 6: Verificação de Nulidade

### Passos:

1. Abra Developer Tools (F12)
2. Vá para a aba **Console**
3. Navegue para `/settings` sem estar autenticado (se possível)

### Validações:

- ✅ Nenhum erro de NullReferenceException
- ✅ Nenhum erro de "Cannot read property of undefined"
- ✅ Página renderiza com valores padrão

---

## Teste 7: Tratamento de Erros

### Passos:

1. Abra Developer Tools (F12)
2. Vá para a aba **Console**
3. Simule erros (ex: desligar API, desligar internet)

### Validações:

- ✅ Erros são capturados e logados
- ✅ Mensagens de erro são claras
- ✅ Aplicação não quebra
- ✅ Usuário recebe feedback

---

## Teste 8: Performance

### Passos:

1. Abra Developer Tools (F12)
2. Vá para a aba **Performance**
3. Navegue entre páginas
4. Verifique o tempo de renderização

### Validações:

- ✅ Tempo de renderização < 1s
- ✅ Nenhum jank ou lag
- ✅ Transições suaves

---

## Checklist de Validação

### Login Page
- [ ] Sem erros de console
- [ ] Logs aparecem corretamente
- [ ] Login com credenciais válidas funciona
- [ ] Login com credenciais inválidas mostra erro
- [ ] Conexão falha mostra erro apropriado
- [ ] Ícone de visibilidade de senha funciona
- [ ] Versão exibida corretamente

### Settings Page
- [ ] Sem erros de console
- [ ] Logs aparecem corretamente
- [ ] Nome do usuário exibido
- [ ] Email do usuário exibido
- [ ] Versão exibida corretamente
- [ ] Logout funciona
- [ ] Mudança de tema funciona
- [ ] Limpeza de cache funciona

### Layouts
- [ ] MinimalLayout renderiza sem erros
- [ ] AuthMinimalLayout renderiza sem erros
- [ ] Bottom navigation funciona
- [ ] Navegação entre páginas funciona

### AuthService
- [ ] Login com logging correto
- [ ] Logout com logging correto
- [ ] Refresh de token com logging correto
- [ ] Erros são capturados e logados

### Geral
- [ ] Nenhum NullReferenceException
- [ ] Nenhum erro de inicialização
- [ ] Nenhum erro de renderização
- [ ] Logs claros para depuração
- [ ] Experiência do usuário estável

---

## Troubleshooting

### Problema: Logs não aparecem no console

**Solução:**
1. Verifique se o logging está configurado em Program.cs
2. Verifique se o nível de log é Information ou inferior
3. Recarregue a página (Ctrl+Shift+R)

### Problema: Erros de NullReferenceException

**Solução:**
1. Verifique se os operadores seguros (?) estão sendo usados
2. Verifique se OnInitializedAsync está carregando dados
3. Verifique se as verificações de nulidade estão em lugar

### Problema: Versão não aparece

**Solução:**
1. Verifique se o meta tag está em index.html
2. Verifique se o build.sh está substituindo a versão
3. Verifique se a versão está no git commit message

### Problema: Usuário não carrega em Settings

**Solução:**
1. Verifique se AuthService.GetCurrentUserAsync() retorna dados
2. Verifique se appState.CurrentUser está sendo atualizado
3. Verifique se o localStorage tem dados do usuário

---

## Conclusão

Após completar todos os testes acima, você terá validado que:

✅ Todas as correções foram implementadas corretamente
✅ Não há erros de console
✅ Logging está funcionando
✅ Tratamento de erros está robusto
✅ Experiência do usuário é estável

Se todos os testes passarem, o projeto está pronto para produção!
