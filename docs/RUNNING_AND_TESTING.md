# Guia de Execução e Testes - Aspec Captura

## 🚀 Como Executar a Aplicação

### Pré-requisitos

- .NET 8.0 SDK instalado
- Node.js (opcional, para ferramentas de build)
- Git instalado
- API rodando em `http://localhost:5069`

### Passos para Executar

#### 1. Clonar o Repositório
```bash
git clone <repository-url>
cd pwa-camera-poc-blazor
```

#### 2. Restaurar Dependências
```bash
dotnet restore
```

#### 3. Executar a Aplicação
```bash
dotnet run
```

Ou com watch mode para desenvolvimento:
```bash
dotnet watch run
```

#### 4. Acessar a Aplicação
- Abra o navegador em `http://localhost:5230`
- Você será redirecionado para `/login`

---

## 🧪 Como Testar as Correções

### Teste 1: Verificar Logs no Console

#### Passos:
1. Abra `http://localhost:5230/login`
2. Pressione `F12` para abrir Developer Tools
3. Vá para a aba **Console**
4. Você deve ver:
   ```
   [info] Login page initialized
   [info] Theme loaded: light
   ```

#### Validação:
- ✅ Logs aparecem no console
- ✅ Nenhum erro de NullReferenceException
- ✅ Nenhum erro de inicialização

---

### Teste 2: Login com Credenciais Válidas

#### Passos:
1. Na página de login, digite credenciais válidas
2. Clique em "Entrar"
3. Verifique os logs no console

#### Logs Esperados:
```
[info] Login attempt for user: admin
[info] Login successful for user: admin
```

#### Validação:
- ✅ Login bem-sucedido
- ✅ Redirecionado para `/configuracao-sessao`
- ✅ Logs aparecem corretamente

---

### Teste 3: Login com Credenciais Inválidas

#### Passos:
1. Na página de login, digite credenciais inválidas
2. Clique em "Entrar"
3. Verifique os logs no console

#### Logs Esperados:
```
[info] Login attempt for user: invalid
[warn] Login failed for user: invalid. Error: Usuário ou senha inválidos
```

#### Validação:
- ✅ Mensagem de erro exibida
- ✅ Logs aparecem corretamente
- ✅ Usuário permanece na página de login

---

### Teste 4: Página de Settings

#### Passos:
1. Faça login com sucesso
2. Navegue para `/settings`
3. Verifique os logs no console

#### Logs Esperados:
```
[info] Settings page initialized
[info] App version loaded: v0.2.2
[info] Settings loaded. User: [Nome do Usuário]
```

#### Validação:
- ✅ Página renderiza sem erros
- ✅ Nome do usuário exibido
- ✅ Versão exibida corretamente
- ✅ Logs aparecem corretamente

---

### Teste 5: Logout

#### Passos:
1. Na página de Settings, clique em "Sair"
2. Confirme o logout
3. Verifique os logs no console

#### Logs Esperados:
```
[info] Logout initiated for user: admin
[info] Logout completed successfully
```

#### Validação:
- ✅ Logout bem-sucedido
- ✅ Redirecionado para `/login`
- ✅ Logs aparecem corretamente

---

### Teste 6: Erro de Conexão

#### Passos:
1. Desligue a API
2. Tente fazer login
3. Verifique os logs no console

#### Logs Esperados:
```
[info] Login attempt for user: admin
[error] Connection error during login for user: admin
```

#### Validação:
- ✅ Mensagem de erro exibida
- ✅ Logs aparecem corretamente
- ✅ Aplicação não quebra

---

## 📋 Checklist de Testes

### Login Page
- [ ] Logs aparecem no console
- [ ] Nenhum erro de NullReferenceException
- [ ] Login com credenciais válidas funciona
- [ ] Login com credenciais inválidas mostra erro
- [ ] Erro de conexão mostra mensagem apropriada
- [ ] Ícone de visibilidade de senha funciona
- [ ] Versão exibida corretamente

### Settings Page
- [ ] Logs aparecem no console
- [ ] Nenhum erro de NullReferenceException
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

### Logging
- [ ] Logs estruturados aparecem
- [ ] Níveis de log apropriados (Info, Warn, Error)
- [ ] Contexto claro em cada log
- [ ] Sem informações sensíveis nos logs

---

## 🔍 Como Filtrar Logs no Console

### Chrome/Edge/Firefox

#### Filtrar por Tipo
1. Clique no ícone de filtro (funil)
2. Selecione os níveis desejados:
   - ✅ Verbose (Debug)
   - ✅ Info
   - ✅ Warnings
   - ✅ Errors

#### Filtrar por Texto
1. Digite no campo de busca
2. Exemplo: `Login` para ver apenas logs de login

#### Limpar Logs
1. Clique no ícone de lixeira
2. Ou pressione `Ctrl+L`

---

## 🐛 Troubleshooting

### Problema: Logs não aparecem

**Solução:**
1. Verifique se o logging está configurado em Program.cs
2. Recarregue a página (Ctrl+Shift+R)
3. Verifique se o nível de log é Information ou inferior
4. Abra o console novamente (F12)

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

## 📊 Monitorar Performance

### Chrome DevTools

#### Abrir Performance Tab
1. Pressione `F12`
2. Vá para a aba **Performance**
3. Clique em "Record"
4. Navegue entre páginas
5. Clique em "Stop"

#### Verificar Métricas
- **First Contentful Paint (FCP)**: < 1s
- **Largest Contentful Paint (LCP)**: < 2.5s
- **Cumulative Layout Shift (CLS)**: < 0.1

---

## 🔐 Segurança

### Verificações de Segurança

- [ ] Nenhuma senha em logs
- [ ] Nenhum token em logs
- [ ] Nenhuma informação sensível em logs
- [ ] HTTPS em produção
- [ ] CSP headers configurados
- [ ] CORS configurado corretamente

---

## 📝 Logs Importantes

### Logs de Autenticação
```
[info] Login attempt for user: admin
[info] Login successful for user: admin
[info] Logout initiated for user: admin
[info] Token refreshed successfully
```

### Logs de Erro
```
[error] Connection error during login for user: admin
[error] Error initializing Settings page
[error] Error showing snackbar
```

### Logs de Aviso
```
[warn] Login failed for user: admin. Error: Invalid credentials
[warn] CurrentUser is null in GetUserName
[warn] Session will expire in 2 minutes due to inactivity
```

### Logs de Debug
```
[debug] Location changed to: /settings
[debug] App state changed
[debug] Token still valid. Time until expiration: 45.5 minutes
```

---

## 🚀 Deploy em Produção

### Antes de Fazer Deploy

1. [ ] Todos os testes passaram
2. [ ] Nenhum erro de console
3. [ ] Logs estruturados funcionando
4. [ ] Performance aceitável
5. [ ] Segurança validada
6. [ ] Documentação atualizada

### Build para Produção

```bash
dotnet publish -c Release -o bin/Release/net8.0/publish
```

### Executar Build Script

```bash
./build.sh
```

---

## 📚 Documentação Relacionada

- `CONSOLE_ERRORS_FIXES_COMPLETED.md` - Detalhes técnicos
- `TESTING_CONSOLE_ERRORS.md` - Guia de testes completo
- `LOGGING_QUICK_REFERENCE.md` - Guia rápido de logging
- `CHANGES_SUMMARY.md` - Resumo de mudanças

---

## ✅ Conclusão

Após seguir este guia, você terá:

✅ Executado a aplicação com sucesso
✅ Testado todas as correções
✅ Validado os logs no console
✅ Verificado a performance
✅ Confirmado a segurança

A aplicação está pronta para produção! 🚀
