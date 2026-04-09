# Guia de Testes - PWA Camera PoC

## 📋 Visão Geral

Este documento explica como executar os testes para validar as correções aplicadas no deploy do Render e Cloudflare Pages.

## 🎯 Tipos de Testes

### 1. Testes Automatizados
Scripts que testam automaticamente os endpoints da API e a disponibilidade do frontend.

### 2. Testes Manuais
Checklist detalhado para testar funcionalidades no navegador.

## 🚀 Executando Testes Automatizados

### Pré-requisitos

- **Linux/Mac**: Bash instalado
- **Windows**: PowerShell 5.1 ou superior
- **Ambos**: curl instalado (para testes de API)

### Linux/Mac

```bash
# Dar permissão de execução ao script
chmod +x test-deploy.sh

# Executar com URLs padrão
./test-deploy.sh

# Executar com URLs customizadas
./test-deploy.sh \
  "https://sua-api.onrender.com" \
  "https://seu-frontend.pages.dev" \
  "https://preview.seu-frontend.pages.dev"
```

### Windows (PowerShell)

```powershell
# Executar com URLs padrão
.\test-deploy.ps1

# Executar com URLs customizadas
.\test-deploy.ps1 `
  -ApiUrl "https://sua-api.onrender.com" `
  -FrontendUrl "https://seu-frontend.pages.dev" `
  -PreviewUrl "https://preview.seu-frontend.pages.dev"
```

### Saída Esperada

```
==========================================
  Testes de Deploy - PWA Camera PoC
==========================================

API URL: https://pwa-camera-poc-api.onrender.com
Frontend URL: https://pwa-camera-poc-blazor.pages.dev
Preview URL: https://e82ab59d.pwa-camera-poc-blazor.pages.dev

==========================================
  Iniciando Testes
==========================================

[1] Health Check da API... ✓ PASSOU
[2] Testando CORS - Domínio Principal...
✓ PASSOU
  Headers CORS encontrados:
    Access-Control-Allow-Origin: https://pwa-camera-poc-blazor.pages.dev
    Access-Control-Allow-Credentials: true
[3] Testando CORS - Subdomínio de Preview...
✓ PASSOU
  Headers CORS encontrados para preview:
    Access-Control-Allow-Origin: https://e82ab59d.pwa-camera-poc-blazor.pages.dev
    Access-Control-Allow-Credentials: true
[4] Swagger UI acessível... ✓ PASSOU
[5] Endpoint de login existe... ✓ PASSOU
[6] Frontend está acessível... ✓ PASSOU
[7] Service Worker está presente... ✓ PASSOU
[8] Manifest.json está presente... ✓ PASSOU

==========================================
  Resumo dos Testes
==========================================

Total de testes: 8
Testes passados: 8
Testes falhados: 0

✓ Todos os testes passaram!

Próximos passos:
1. Teste o login manualmente no navegador
2. Teste a sincronização de dados
3. Verifique os logs da API no Render
```

## 📝 Executando Testes Manuais

Use o checklist detalhado em `TEST_CHECKLIST.md`:

```bash
# Abrir o checklist
cat TEST_CHECKLIST.md

# Ou abrir em um editor
code TEST_CHECKLIST.md
```

### Testes Manuais Principais

1. **Login no Navegador**
   - Abra `https://pwa-camera-poc-blazor.pages.dev`
   - Tente fazer login com credenciais válidas
   - Verifique se não há erros de CORS no console

2. **Sincronização de Dados**
   - Após login, aguarde a sincronização
   - Verifique se não há timeout (deve completar em menos de 10 minutos)
   - Confirme que os dados são carregados corretamente

3. **Captura de Fotos**
   - Navegue até a tela de captura
   - Tire uma foto
   - Verifique se o upload para S3 funciona

4. **Teste em Diferentes Navegadores**
   - Chrome/Edge
   - Firefox
   - Safari (se disponível)

## 🔍 Interpretando Resultados

### Todos os Testes Passaram ✅

Se todos os testes automatizados passaram:
1. As correções de CORS estão funcionando
2. A API está acessível e respondendo
3. O frontend está deployado corretamente
4. Prossiga com os testes manuais

### Alguns Testes Falharam ❌

Se algum teste falhou, verifique:

#### Teste 1 Falhou (Health Check)
- A API pode não estar rodando
- Verifique os logs no Render
- Confirme que o deploy foi concluído

#### Testes 2 ou 3 Falharam (CORS)
- A configuração de CORS pode não estar correta
- Verifique se o código foi deployado
- Confirme que `Program.cs` da API tem as alterações

#### Teste 4 Falhou (Swagger)
- Swagger pode estar desabilitado em produção
- Isso é normal se `ASPNETCORE_ENVIRONMENT=Production`

#### Testes 5 Falhou (Endpoint de Login)
- O endpoint pode não existir
- Verifique se a rota está correta
- Confirme que a API foi deployada

#### Testes 6, 7 ou 8 Falharam (Frontend)
- O frontend pode não estar deployado
- Verifique o build no Cloudflare Pages
- Confirme que os arquivos estão no lugar certo

## 🐛 Troubleshooting

### Erro: "curl: command not found"

**Linux/Mac**:
```bash
# Ubuntu/Debian
sudo apt-get install curl

# macOS
brew install curl
```

**Windows**:
- curl já vem instalado no Windows 10+
- Se não estiver disponível, use o script PowerShell

### Erro: "Permission denied"

**Linux/Mac**:
```bash
chmod +x test-deploy.sh
```

### Erro: "Execution of scripts is disabled"

**Windows PowerShell**:
```powershell
# Executar como Administrador
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Erro: "Connection refused" ou "Network error"

Possíveis causas:
1. A API não está rodando
2. A URL está incorreta
3. Firewall bloqueando a conexão
4. O serviço está em cold start (Render free tier)

**Solução**:
- Aguarde alguns segundos e tente novamente
- Verifique a URL no painel do Render
- Teste acessando a URL diretamente no navegador

## 📊 Testes de Performance

### Teste de Carga (Opcional)

Se quiser testar a performance da API:

```bash
# Instalar Apache Bench (se não tiver)
# Ubuntu/Debian
sudo apt-get install apache2-utils

# macOS
brew install httpd

# Executar teste de carga
ab -n 100 -c 10 https://pwa-camera-poc-api.onrender.com/health
```

**Interpretação**:
- `-n 100`: 100 requisições totais
- `-c 10`: 10 requisições concorrentes
- Tempo de resposta médio deve ser < 500ms
- Taxa de erro deve ser 0%

## 📈 Monitoramento Contínuo

### Ferramentas Recomendadas

1. **Render Dashboard**
   - Monitore CPU, memória e logs
   - Configure alertas para erros

2. **Cloudflare Analytics**
   - Monitore tráfego e performance
   - Verifique taxa de erro

3. **Browser DevTools**
   - Console: Erros JavaScript
   - Network: Requisições HTTP
   - Application: Service Worker e Cache

4. **Uptime Monitoring**
   - Use serviços como UptimeRobot ou Pingdom
   - Configure checks para `/health` endpoint

## 🔄 Testes de Regressão

Após cada deploy, execute:

1. **Testes Automatizados**
   ```bash
   ./test-deploy.sh
   ```

2. **Smoke Tests Manuais**
   - Login
   - Sincronização
   - Captura de foto

3. **Verificação de Logs**
   - Render: Logs da API
   - Browser: Console do frontend

## 📚 Recursos Adicionais

- [TEST_CHECKLIST.md](./TEST_CHECKLIST.md) - Checklist detalhado de testes manuais
- [RENDER_DEPLOY_FIX.md](./RENDER_DEPLOY_FIX.md) - Problemas conhecidos e correções
- [DEPLOY_GUIDE.md](./DEPLOY_GUIDE.md) - Guia completo de deploy
- [CORS-CONFIG.md](../pwa-camera-poc-api/CORS-CONFIG.md) - Documentação de CORS

## 🆘 Suporte

Se os testes continuarem falhando:

1. Revise os logs da API no Render
2. Verifique o console do navegador
3. Consulte o arquivo `RENDER_DEPLOY_FIX.md`
4. Abra uma issue no repositório com:
   - Output dos testes automatizados
   - Screenshots dos erros
   - Logs da API
   - Informações do navegador

## ✅ Checklist Rápido

Antes de considerar o deploy bem-sucedido:

- [ ] Todos os testes automatizados passaram
- [ ] Login funciona no navegador
- [ ] Sincronização completa sem timeout
- [ ] Captura de fotos funciona
- [ ] Upload para S3 funciona
- [ ] Sem erros de CORS no console
- [ ] Testado em pelo menos 2 navegadores
- [ ] Logs da API sem erros críticos
- [ ] Deploy de preview funciona (se aplicável)
