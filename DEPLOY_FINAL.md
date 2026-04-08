# 🚀 Guia de Deploy Final - PWA Camera POC

## ✅ Correções Aplicadas

### 1. Backend (API no Render)
- ✅ CORS configurado para aceitar `https://pwa-camera-poc-blazor.pages.dev`
- ✅ CORS configurado para aceitar subdomínios `*.pwa-camera-poc-blazor.pages.dev`
- ✅ Permite credenciais (cookies/auth)

### 2. Frontend (PWA no Cloudflare Pages)
- ✅ Script `build.sh` validado e otimizado
- ✅ Substituição de variáveis ANTES do build (mais confiável)
- ✅ Validações automáticas no build
- ✅ Criação de `_headers` e `_redirects` para Cloudflare
- ❌ Removido `build-production.sh` (em desuso)

## 📋 Configuração do Cloudflare Pages

### Build Settings

Acesse: https://dash.cloudflare.com → Workers & Pages → pwa-camera-poc-blazor → Settings → Builds & deployments

```
Build command: ./build.sh
Build output directory: bin/Release/net8.0/publish/wwwroot
Root directory: (deixe vazio)
```

### Environment Variables

**Production:**
```
API_BASE_URL=https://pwa-camera-poc-api.onrender.com
CF_PAGES=1
```

**Preview (opcional):**
```
API_BASE_URL=https://pwa-camera-poc-api.onrender.com
CF_PAGES=1
```

## 🔍 Validação do Build

O script `build.sh` agora inclui validações automáticas:

1. ✅ Verifica se `API_BASE_URL` está configurada
2. ✅ Substitui `__API_BASE_URL__` no arquivo fonte
3. ✅ Valida se a substituição funcionou ANTES do build
4. ✅ Verifica se `appsettings.json` foi copiado para o output
5. ✅ Valida se não há placeholders no output final
6. ✅ Confirma criação de `_headers` e `_redirects`

## 🧪 Teste Local

Para testar o build localmente:

```bash
# Configurar variável de ambiente
export API_BASE_URL=https://pwa-camera-poc-api.onrender.com
export CF_PAGES=1

# Executar build
./build.sh

# Verificar output
cat bin/Release/net8.0/publish/wwwroot/appsettings.json
# Deve mostrar: {"ApiBaseUrl":"https://pwa-camera-poc-api.onrender.com"}

# Verificar arquivos de configuração
ls -la bin/Release/net8.0/publish/wwwroot/_*
# Deve listar: _headers e _redirects
```

## 🚀 Deploy

### Opção 1: Deploy Automático (Recomendado)

1. Faça commit das alterações:
```bash
git add .
git commit -m "fix: corrigir CORS e validar build.sh"
git push
```

2. O Cloudflare Pages detectará automaticamente e fará o deploy
3. Aguarde 2-5 minutos

### Opção 2: Deploy Manual

1. No Cloudflare Pages Dashboard
2. Vá em Deployments
3. Clique em "Retry deployment" ou "Create deployment"

## ✅ Verificação Pós-Deploy

### 1. Verificar appsettings.json

Acesse: https://pwa-camera-poc-blazor.pages.dev/appsettings.json

**Esperado:**
```json
{"ApiBaseUrl":"https://pwa-camera-poc-api.onrender.com"}
```

**Se aparecer `__API_BASE_URL__`:**
- ❌ A variável de ambiente não está configurada
- Vá em Settings → Environment Variables
- Adicione `API_BASE_URL`

### 2. Verificar CORS

1. Acesse: https://pwa-camera-poc-blazor.pages.dev
2. Abra DevTools (F12) → Console
3. Tente fazer login

**Esperado:**
- ✅ Sem erros de CORS
- ✅ Requisições para API funcionando

**Se houver erro de CORS:**
- Verifique se o backend foi redeployado com as novas configurações
- Limpe o cache do navegador (Ctrl+Shift+Delete)

### 3. Testar API Health

Acesse: https://pwa-camera-poc-api.onrender.com/health

**Esperado:**
```json
{"status":"healthy","timestamp":"2024-..."}
```

### 4. Testar Login

1. Acesse o PWA
2. Tente fazer login com credenciais válidas
3. Verifique se carrega os dados

## 📊 Checklist de Deploy

- [ ] Backend redeployado com CORS corrigido
- [ ] Variável `API_BASE_URL` configurada no Cloudflare Pages
- [ ] Build command: `./build.sh`
- [ ] Output directory: `bin/Release/net8.0/publish/wwwroot`
- [ ] Deploy executado com sucesso
- [ ] appsettings.json sem placeholders
- [ ] Sem erros de CORS no console
- [ ] Login funcionando
- [ ] Dados carregando corretamente

## 🐛 Troubleshooting

### Erro: "API_BASE_URL não definida"
**Solução:** Configure a variável de ambiente no Cloudflare Pages

### Erro: "CORS policy: No 'Access-Control-Allow-Origin'"
**Solução:** 
1. Verifique se o backend foi redeployado
2. Limpe o cache do navegador
3. Verifique se a URL da API está correta

### Erro: "Failed to fetch"
**Solução:**
1. Verifique se a API está online: https://pwa-camera-poc-api.onrender.com/health
2. Verifique se a URL no appsettings.json está correta
3. Verifique se não há typos na URL

### Build falha no Cloudflare Pages
**Solução:**
1. Verifique os logs de build
2. Certifique-se que o comando é `./build.sh` (com ./)
3. Verifique se o script tem permissões de execução no Git

## 📝 Arquivos Importantes

- `build.sh` - Script de build (ÚNICO em uso)
- `wwwroot/appsettings.json` - Configuração da API (com placeholder)
- `Program.cs` (backend) - Configuração de CORS
- `_headers` - Headers HTTP (gerado no build)
- `_redirects` - Redirecionamentos SPA (gerado no build)

## 🎯 URLs de Produção

- **Frontend:** https://pwa-camera-poc-blazor.pages.dev
- **Backend:** https://pwa-camera-poc-api.onrender.com
- **API Health:** https://pwa-camera-poc-api.onrender.com/health
- **API Swagger:** https://pwa-camera-poc-api.onrender.com/swagger
- **Config Check:** https://pwa-camera-poc-blazor.pages.dev/appsettings.json

## 📞 Suporte

Se após seguir todos os passos ainda houver problemas:

1. Verifique os logs de build no Cloudflare Pages
2. Verifique os logs do backend no Render
3. Verifique o console do navegador (F12)
4. Execute `./check-build-config.sh` para diagnóstico automático
