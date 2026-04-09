# Guia de Deploy - PWA Camera PoC

## 📦 Visão Geral

Este projeto consiste em duas partes:
1. **API Backend** - Minimal API ASP.NET Core (hospedada no Render)
2. **Frontend PWA** - Blazor WebAssembly (hospedado no Cloudflare Pages)

## 🔧 Pré-requisitos

- Conta no [Render](https://render.com)
- Conta no [Cloudflare Pages](https://pages.cloudflare.com)
- Conta AWS com bucket S3 configurado
- Repositório Git com o código

## 🚀 Deploy da API (Render)

### 1. Criar Web Service no Render

1. Acesse o [Dashboard do Render](https://dashboard.render.com)
2. Clique em "New +" → "Web Service"
3. Conecte seu repositório Git
4. Configure:
   - **Name**: `pwa-camera-poc-api`
   - **Region**: Oregon (ou sua preferência)
   - **Branch**: `desenvolvimento_v3` (ou sua branch principal)
   - **Runtime**: Docker
   - **Dockerfile Path**: `./Dockerfile`
   - **Docker Context**: `.`
   - **Plan**: Free (ou conforme necessário)

### 2. Configurar Variáveis de Ambiente

No painel do Render, adicione as seguintes variáveis de ambiente:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
AWS__Region=us-east-1
AWS__BucketName=seu-bucket-s3
AWS__AccessKey=sua-access-key-aws
AWS__SecretKey=sua-secret-key-aws
```

**Importante**: Substitua os valores acima pelos seus dados reais da AWS.

### 3. Configurar Health Check

- **Health Check Path**: `/health`
- **Health Check Interval**: 30 segundos

### 4. Deploy

1. Clique em "Create Web Service"
2. Aguarde o build e deploy (pode levar alguns minutos)
3. Anote a URL gerada (ex: `https://pwa-camera-poc-api.onrender.com`)

### 5. Verificar Deploy

Teste o endpoint de health check:
```bash
curl https://pwa-camera-poc-api.onrender.com/health
```

Resposta esperada:
```json
{
  "status": "healthy",
  "timestamp": "2024-01-01T00:00:00.000Z"
}
```

## 🌐 Deploy do Frontend (Cloudflare Pages)

### 1. Atualizar Configuração da API

Antes de fazer o deploy, certifique-se de que o arquivo `wwwroot/appsettings.json` contém a URL correta da API:

```json
{
    "ApiBaseUrl": "https://pwa-camera-poc-api.onrender.com"
}
```

**Substitua pela URL real gerada pelo Render no passo anterior.**

### 2. Criar Projeto no Cloudflare Pages

1. Acesse o [Dashboard do Cloudflare Pages](https://dash.cloudflare.com)
2. Clique em "Create a project"
3. Conecte seu repositório Git
4. Configure:
   - **Project name**: `pwa-camera-poc-blazor`
   - **Production branch**: `desenvolvimento_v3` (ou sua branch principal)
   - **Framework preset**: None
   - **Build command**: `dotnet publish -c Release -o output`
   - **Build output directory**: `output/wwwroot`

### 3. Configurar Variáveis de Ambiente (Opcional)

Se preferir usar variáveis de ambiente em vez de hardcoded:

```
API_BASE_URL=https://pwa-camera-poc-api.onrender.com
```

### 4. Deploy

1. Clique em "Save and Deploy"
2. Aguarde o build e deploy (pode levar alguns minutos)
3. Anote a URL gerada (ex: `https://pwa-camera-poc-blazor.pages.dev`)

### 5. Atualizar CORS na API

Após obter a URL do Cloudflare Pages, verifique se a configuração de CORS na API está correta. O código já está preparado para aceitar subdomínios do Cloudflare Pages.

## ✅ Verificação Pós-Deploy

### 1. Testar a Aplicação

1. Acesse a URL do Cloudflare Pages
2. Tente fazer login com credenciais válidas
3. Verifique se a sincronização de dados funciona
4. Teste a captura de fotos
5. Confirme o upload para S3

### 2. Verificar Logs

**Render (API)**:
- Acesse o painel do Render
- Clique no seu web service
- Vá para a aba "Logs"
- Verifique se há erros

**Cloudflare Pages (Frontend)**:
- Abra as ferramentas de desenvolvedor do navegador (F12)
- Vá para a aba "Console"
- Verifique se há erros JavaScript

### 3. Testar CORS

Use as ferramentas de desenvolvedor do navegador:
1. Abra a aba "Network"
2. Faça uma requisição à API (ex: login)
3. Verifique os headers da resposta:
   - `Access-Control-Allow-Origin` deve conter a origem do frontend
   - `Access-Control-Allow-Credentials` deve ser `true`

## 🔄 Atualizações e Redeploy

### API (Render)

O Render faz redeploy automático quando você faz push para a branch configurada.

Para forçar um redeploy manual:
1. Acesse o painel do Render
2. Clique no seu web service
3. Clique em "Manual Deploy" → "Deploy latest commit"

### Frontend (Cloudflare Pages)

O Cloudflare Pages faz redeploy automático quando você faz push para a branch configurada.

Para forçar um redeploy manual:
1. Acesse o painel do Cloudflare Pages
2. Clique no seu projeto
3. Vá para "Deployments"
4. Clique em "Retry deployment" no último deploy

## 🐛 Troubleshooting

### Erro de CORS

**Sintoma**: `Access to fetch has been blocked by CORS policy`

**Solução**:
1. Verifique se a URL da API no `appsettings.json` está correta
2. Confirme que a configuração de CORS na API aceita a origem do frontend
3. Verifique os logs da API para ver se as requisições estão chegando

### Timeout de Requisição

**Sintoma**: `net_http_request_timedout`

**Solução**:
1. O timeout já foi aumentado para 10 minutos no código
2. Se ainda houver problemas, considere:
   - Otimizar as queries da API
   - Implementar paginação
   - Reduzir o tamanho dos dados retornados

### Erro 500 na API

**Sintoma**: Requisições retornam status 500

**Solução**:
1. Verifique os logs da API no Render
2. Confirme que as variáveis de ambiente AWS estão configuradas corretamente
3. Teste a conexão com o S3 usando as credenciais fornecidas

### Build Falha no Cloudflare Pages

**Sintoma**: Build do Blazor falha

**Solução**:
1. Verifique se o comando de build está correto
2. Confirme que o diretório de output está correto
3. Verifique se todas as dependências estão no repositório

## 📊 Monitoramento

### Métricas Importantes

**API (Render)**:
- Tempo de resposta
- Taxa de erro (5xx)
- Uso de memória
- Uso de CPU

**Frontend (Cloudflare Pages)**:
- Tempo de carregamento
- Taxa de erro JavaScript
- Uso de IndexedDB
- Service Worker status

### Ferramentas Recomendadas

- **Render**: Logs integrados e métricas básicas
- **Cloudflare Analytics**: Métricas de tráfego e performance
- **Browser DevTools**: Console, Network, Application tabs
- **Lighthouse**: Auditoria de performance e PWA

## 🔒 Segurança

### Checklist de Segurança

- [ ] Variáveis de ambiente AWS não estão no código
- [ ] CORS configurado para aceitar apenas origens específicas
- [ ] HTTPS habilitado em produção
- [ ] Headers de segurança configurados (`_headers` do Cloudflare)
- [ ] Service Worker com cache adequado
- [ ] Tokens de autenticação com expiração

### Boas Práticas

1. **Nunca commite credenciais**: Use variáveis de ambiente
2. **Rotacione credenciais regularmente**: Especialmente as chaves AWS
3. **Monitore logs**: Fique atento a tentativas de acesso não autorizado
4. **Mantenha dependências atualizadas**: Use `dotnet outdated` regularmente

## 📚 Recursos Adicionais

- [Documentação do Render](https://render.com/docs)
- [Documentação do Cloudflare Pages](https://developers.cloudflare.com/pages)
- [Blazor WebAssembly Hosting](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

## 🆘 Suporte

Se encontrar problemas não cobertos neste guia:

1. Verifique os logs da API e do frontend
2. Consulte a documentação oficial das plataformas
3. Revise o arquivo `RENDER_DEPLOY_FIX.md` para problemas conhecidos
4. Abra uma issue no repositório com detalhes do erro
