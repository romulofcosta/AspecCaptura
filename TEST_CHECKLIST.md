# Checklist de Testes - Correções de Deploy

## 🎯 Objetivo
Validar se as correções aplicadas resolveram os problemas de CORS, timeout e configuração da API.

## 📋 Pré-requisitos

- [ ] Código commitado e pushed para o repositório
- [ ] Deploy da API concluído no Render
- [ ] Deploy do Frontend concluído no Cloudflare Pages
- [ ] URLs de produção anotadas:
  - API: `https://pwa-camera-poc-api.onrender.com` (ou sua URL)
  - Frontend: `https://pwa-camera-poc-blazor.pages.dev` (ou sua URL)

## 🧪 Testes a Executar

### 1. Teste de Health Check da API

**Objetivo**: Verificar se a API está rodando corretamente

**Como testar**:
```bash
curl https://pwa-camera-poc-api.onrender.com/health
```

**Resultado esperado**:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00.000Z"
}
```

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 2. Teste de CORS - Domínio Principal

**Objetivo**: Verificar se o domínio principal do Cloudflare Pages é aceito

**Como testar**:
```bash
curl -H "Origin: https://pwa-camera-poc-blazor.pages.dev" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     https://pwa-camera-poc-api.onrender.com/api/auth/login \
     -v
```

**Resultado esperado**:
- Status: `204 No Content` ou `200 OK`
- Headers de resposta devem incluir:
  - `Access-Control-Allow-Origin: https://pwa-camera-poc-blazor.pages.dev`
  - `Access-Control-Allow-Credentials: true`
  - `Access-Control-Allow-Methods: POST` (ou lista de métodos)

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 3. Teste de CORS - Subdomínio de Preview

**Objetivo**: Verificar se subdomínios de preview são aceitos

**Como testar**:
```bash
curl -H "Origin: https://e82ab59d.pwa-camera-poc-blazor.pages.dev" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     https://pwa-camera-poc-api.onrender.com/api/auth/login \
     -v
```

**Resultado esperado**:
- Status: `204 No Content` ou `200 OK`
- Headers de resposta devem incluir:
  - `Access-Control-Allow-Origin: https://e82ab59d.pwa-camera-poc-blazor.pages.dev`
  - `Access-Control-Allow-Credentials: true`

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 4. Teste de Login via Browser

**Objetivo**: Verificar se o login funciona no navegador

**Como testar**:
1. Abra o frontend no navegador: `https://pwa-camera-poc-blazor.pages.dev`
2. Abra DevTools (F12) → Aba "Console"
3. Abra também a aba "Network"
4. Tente fazer login com credenciais válidas (ex: `ce999.nome1.sbnome1`)
5. Observe os logs no console e as requisições na aba Network

**Resultado esperado**:
- [ ] Nenhum erro de CORS no console
- [ ] Requisição POST para `/api/auth/login` retorna status `200 OK`
- [ ] Headers de resposta incluem `Access-Control-Allow-Origin`
- [ ] Login é bem-sucedido e redireciona para a próxima tela

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

**Screenshot do erro (se houver)**:
_______________________________________________________

---

### 5. Teste de Sincronização de Dados

**Objetivo**: Verificar se a sincronização não dá timeout

**Como testar**:
1. Após fazer login com sucesso
2. Aguarde a tela de sincronização (`/tombamentos-sync`)
3. Observe o progresso da sincronização
4. Verifique se não há timeout (erro após 5 minutos)

**Resultado esperado**:
- [ ] Sincronização inicia corretamente
- [ ] Progresso é exibido (ex: "Lote 1 de 10")
- [ ] Sincronização completa sem timeout
- [ ] Mensagem de sucesso é exibida
- [ ] Redirecionamento para `/configuracao-sessao`

**Tempo de sincronização**: _______ minutos

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

**Screenshot do erro (se houver)**:
_______________________________________________________

---

### 6. Teste de Sincronização - Verificar Timeout

**Objetivo**: Confirmar que o timeout foi aumentado para 10 minutos

**Como testar**:
1. Abra DevTools (F12) → Aba "Network"
2. Inicie a sincronização
3. Observe as requisições para `/api/tombamentos/lote/*`
4. Verifique se as requisições não falham com timeout antes de 10 minutos

**Resultado esperado**:
- [ ] Requisições não falham com `net_http_request_timedout` antes de 10 minutos
- [ ] Todas as requisições completam com sucesso

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 7. Teste de Configuração da URL da API

**Objetivo**: Verificar se a URL da API está configurada corretamente

**Como testar**:
1. Abra DevTools (F12) → Aba "Network"
2. Faça qualquer requisição (ex: login)
3. Verifique a URL da requisição

**Resultado esperado**:
- [ ] URL da requisição aponta para a API correta (ex: `https://pwa-camera-poc-api.onrender.com`)
- [ ] Não há erros de "Failed to fetch" ou "Network error"

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 8. Teste em Diferentes Navegadores

**Objetivo**: Verificar compatibilidade cross-browser

**Como testar**:
Repita os testes 4, 5 e 6 nos seguintes navegadores:

**Chrome/Edge**:
- [ ] Login funciona
- [ ] Sincronização funciona
- [ ] Sem erros de CORS

**Firefox**:
- [ ] Login funciona
- [ ] Sincronização funciona
- [ ] Sem erros de CORS

**Safari** (se disponível):
- [ ] Login funciona
- [ ] Sincronização funciona
- [ ] Sem erros de CORS

**Status Geral**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 9. Teste de Deploy de Preview (Cloudflare Pages)

**Objetivo**: Verificar se deploys de preview funcionam

**Como testar**:
1. Crie uma branch de teste
2. Faça push para o repositório
3. Aguarde o Cloudflare Pages criar um deploy de preview
4. Acesse a URL do preview (ex: `https://[hash].pwa-camera-poc-blazor.pages.dev`)
5. Tente fazer login

**Resultado esperado**:
- [ ] Deploy de preview é criado com sucesso
- [ ] Login funciona no deploy de preview
- [ ] Sem erros de CORS

**URL do preview**: _______________________________________________________

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

### 10. Teste de Logs da API

**Objetivo**: Verificar se há erros nos logs da API

**Como testar**:
1. Acesse o painel do Render
2. Vá para o seu web service
3. Clique na aba "Logs"
4. Procure por erros relacionados a CORS, AWS, ou exceções não tratadas

**Resultado esperado**:
- [ ] Nenhum erro crítico nos logs
- [ ] Mensagens de log indicam funcionamento normal
- [ ] Configuração de S3 CORS bem-sucedida (se aplicável)

**Status**: [ ] ✅ Passou | [ ] ❌ Falhou

**Notas**:
_______________________________________________________

---

## 📊 Resumo dos Testes

| Teste | Status | Observações |
|-------|--------|-------------|
| 1. Health Check | [ ] | |
| 2. CORS - Domínio Principal | [ ] | |
| 3. CORS - Subdomínio Preview | [ ] | |
| 4. Login via Browser | [ ] | |
| 5. Sincronização de Dados | [ ] | |
| 6. Timeout da Sincronização | [ ] | |
| 7. URL da API | [ ] | |
| 8. Cross-Browser | [ ] | |
| 9. Deploy de Preview | [ ] | |
| 10. Logs da API | [ ] | |

**Total de testes passados**: _____ / 10

---

## 🐛 Problemas Encontrados

### Problema 1
**Descrição**:
_______________________________________________________

**Teste relacionado**: _______________________________________________________

**Solução proposta**:
_______________________________________________________

---

### Problema 2
**Descrição**:
_______________________________________________________

**Teste relacionado**: _______________________________________________________

**Solução proposta**:
_______________________________________________________

---

## ✅ Conclusão

**As correções resolveram os problemas?**: [ ] Sim | [ ] Não | [ ] Parcialmente

**Observações finais**:
_______________________________________________________
_______________________________________________________
_______________________________________________________

**Data do teste**: _______________________________________________________

**Testado por**: _______________________________________________________

---

## 📝 Próximos Passos

Se todos os testes passaram:
- [ ] Marcar a issue como resolvida
- [ ] Documentar as correções no CHANGELOG
- [ ] Notificar a equipe

Se algum teste falhou:
- [ ] Revisar os logs de erro
- [ ] Consultar o arquivo `RENDER_DEPLOY_FIX.md`
- [ ] Aplicar correções adicionais
- [ ] Executar os testes novamente
