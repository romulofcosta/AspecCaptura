# ✅ Checklist de Deploy - PWA Camera POC

Use este checklist para garantir que o deploy foi feito corretamente.

## 📋 Pré-Deploy

### Backend (API)
- [x] Arquivo `Program.cs` atualizado com CORS corrigido
- [ ] Commit feito no repositório
- [ ] Push para o repositório remoto
- [ ] Render iniciou o redeploy automaticamente
- [ ] Deploy do Render concluído com sucesso

### Frontend (PWA)
- [x] Arquivo `build.sh` validado e otimizado
- [x] Arquivo `build-production.sh` removido
- [x] Documentação criada
- [ ] Commit feito no repositório
- [ ] Push para o repositório remoto

## ⚙️ Configuração do Cloudflare Pages

- [ ] Acessei o dashboard: https://dash.cloudflare.com
- [ ] Naveguei para: Workers & Pages > pwa-camera-poc-blazor
- [ ] Fui em: Settings > Builds & deployments
- [ ] Configurei Build command: `./build.sh`
- [ ] Configurei Output directory: `bin/Release/net8.0/publish/wwwroot`
- [ ] Fui em: Settings > Environment Variables
- [ ] Adicionei variável: `API_BASE_URL` = `https://pwa-camera-poc-api.onrender.com`
- [ ] Adicionei variável: `CF_PAGES` = `1`
- [ ] Salvei as configurações

## 🚀 Deploy

- [ ] Cloudflare Pages iniciou o build automaticamente
- [ ] Build concluído sem erros
- [ ] Deploy concluído com sucesso
- [ ] Recebi notificação de deploy bem-sucedido

## ✅ Verificações Pós-Deploy

### 1. Verificar Backend
- [ ] Acessei: https://pwa-camera-poc-api.onrender.com/health
- [ ] Recebi resposta: `{"status":"healthy","timestamp":"..."}`
- [ ] Status code: 200 OK

### 2. Verificar Configuração do Frontend
- [ ] Acessei: https://pwa-camera-poc-blazor.pages.dev/appsettings.json
- [ ] Vi: `{"ApiBaseUrl":"https://pwa-camera-poc-api.onrender.com"}`
- [ ] NÃO vi: `__API_BASE_URL__` (placeholder)

### 3. Verificar CORS
- [ ] Acessei: https://pwa-camera-poc-blazor.pages.dev
- [ ] Abri DevTools (F12)
- [ ] Fui na aba Console
- [ ] NÃO há erros de CORS
- [ ] NÃO há mensagens: "Access-Control-Allow-Origin"

### 4. Testar Funcionalidade
- [ ] Página inicial carregou corretamente
- [ ] Formulário de login está visível
- [ ] Inseri credenciais válidas
- [ ] Cliquei em "Entrar"
- [ ] Login foi bem-sucedido
- [ ] Dados foram carregados
- [ ] Posso navegar pelo sistema

### 5. Testar em Diferentes Navegadores
- [ ] Chrome/Edge: Funcionando
- [ ] Firefox: Funcionando
- [ ] Safari (se disponível): Funcionando

### 6. Testar em Dispositivo Móvel
- [ ] Acessei pelo celular
- [ ] PWA funciona corretamente
- [ ] Sem erros de CORS

## 🐛 Troubleshooting

Se algum item falhou, consulte:

### ❌ Backend não responde
→ Veja logs no Render Dashboard
→ Verifique se o deploy foi concluído
→ Aguarde alguns minutos (cold start)

### ❌ appsettings.json com placeholder
→ Verifique se `API_BASE_URL` está configurada no Cloudflare
→ Faça um novo deploy (trigger manual)
→ Limpe o cache do Cloudflare

### ❌ Erro de CORS
→ Verifique se o backend foi redeployado
→ Limpe o cache do navegador (Ctrl+Shift+Delete)
→ Tente em modo anônimo/privado
→ Verifique se a URL da API está correta

### ❌ Login não funciona
→ Verifique credenciais
→ Veja console do navegador (F12)
→ Verifique Network tab para ver requisições
→ Confirme que a API está respondendo

### ❌ Build falha no Cloudflare
→ Veja logs de build no dashboard
→ Verifique se o comando é `./build.sh` (com ./)
→ Verifique se as variáveis de ambiente estão configuradas
→ Tente fazer deploy manual

## 📊 Status Final

Marque quando tudo estiver funcionando:

- [ ] ✅ Backend online e respondendo
- [ ] ✅ Frontend deployado com sucesso
- [ ] ✅ Configuração correta (sem placeholders)
- [ ] ✅ Sem erros de CORS
- [ ] ✅ Login funcionando
- [ ] ✅ Dados carregando
- [ ] ✅ Testado em múltiplos navegadores
- [ ] ✅ Testado em dispositivo móvel

## 🎉 Deploy Concluído!

Se todos os itens acima estão marcados, o deploy foi bem-sucedido!

---

**Data do Deploy:** _______________

**Responsável:** _______________

**Observações:**
_______________________________________________
_______________________________________________
_______________________________________________
