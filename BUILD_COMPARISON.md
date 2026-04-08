# Comparação dos Scripts de Build

## Scripts Disponíveis

Existem **2 scripts de build** no projeto:

### 1. `build.sh` (Script Universal)
- **Propósito**: Build universal para Netlify e Cloudflare Pages
- **Detecção automática**: Detecta a plataforma via variáveis de ambiente
- **Características**:
  - ✅ Substitui `__API_BASE_URL__` ANTES do build (no arquivo fonte)
  - ✅ Substitui `__APP_VERSION__` no index.html
  - ✅ Instala wasm-tools automaticamente
  - ✅ Cria arquivos `_headers` e `_redirects` para Cloudflare
  - ✅ Detecta versão do Git automaticamente
  - ⚠️ Output: `bin/Release/net8.0/publish/wwwroot`

### 2. `build-production.sh` (Script Específico)
- **Propósito**: Build específico para produção no Cloudflare Pages
- **Características**:
  - ✅ Substitui `__API_BASE_URL__` DEPOIS do build (no dist)
  - ✅ Build sem AOT (mais rápido)
  - ✅ Validações extensivas do output
  - ✅ Logs detalhados
  - ⚠️ Output: `dist/wwwroot`
  - ❌ Não cria `_headers` e `_redirects`
  - ❌ Não instala wasm-tools

## Qual está sendo usado no Cloudflare Pages?

Para descobrir, você precisa verificar as configurações do projeto no Cloudflare Pages:

### Método 1: Via Dashboard do Cloudflare Pages

1. Acesse: https://dash.cloudflare.com
2. Vá em **Workers & Pages** > **pwa-camera-poc-blazor**
3. Clique em **Settings** > **Builds & deployments**
4. Verifique:
   - **Build command**: Qual comando está configurado?
     - `./build.sh` → Usando script universal
     - `./build-production.sh` → Usando script específico
   - **Build output directory**: Qual diretório está configurado?
     - `bin/Release/net8.0/publish/wwwroot` → build.sh
     - `dist/wwwroot` → build-production.sh

### Método 2: Via Logs de Deploy

1. Acesse o último deploy no Cloudflare Pages
2. Veja os logs de build
3. Procure por:
   - `"🚀 Building Blazor PWA for Production (Cloudflare Pages)"` → build-production.sh
   - `"=== Plataforma detectada: cloudflare ==="` → build.sh

### Método 3: Verificar o Site em Produção

1. Acesse: https://pwa-camera-poc-blazor.pages.dev
2. Abra DevTools (F12) > Network
3. Recarregue a página
4. Procure por `appsettings.json`
5. Verifique o conteúdo:
   - Se `ApiBaseUrl` contém a URL real → Build funcionou
   - Se contém `__API_BASE_URL__` → Build não substituiu a variável

## Recomendação

**Use `build.sh`** porque:

1. ✅ Substitui variáveis ANTES do build (mais confiável)
2. ✅ Cria arquivos necessários (`_headers`, `_redirects`)
3. ✅ Detecta plataforma automaticamente
4. ✅ Instala dependências necessárias (wasm-tools)
5. ✅ Já está testado e funcionando

## Como Configurar no Cloudflare Pages

### Configuração Recomendada:

```
Build command: ./build.sh
Build output directory: bin/Release/net8.0/publish/wwwroot
Root directory: (deixe vazio ou /)
```

### Variáveis de Ambiente:

```
API_BASE_URL=https://pwa-camera-poc-api.onrender.com
CF_PAGES=1
```

## Diferenças Críticas

| Aspecto | build.sh | build-production.sh |
|---------|----------|---------------------|
| Substitui API_BASE_URL | ✅ Antes (fonte) | ⚠️ Depois (dist) |
| Cria _headers | ✅ Sim | ❌ Não |
| Cria _redirects | ✅ Sim | ❌ Não |
| Instala wasm-tools | ✅ Sim | ❌ Não |
| Output directory | publish/wwwroot | dist/wwwroot |
| Validações | Básicas | Extensivas |
| Logs | Moderados | Detalhados |

## Ação Recomendada

1. **Verifique qual script está sendo usado** (Método 1 acima)
2. **Se estiver usando `build-production.sh`**:
   - Mude para `build.sh`
   - Atualize o output directory
   - Configure as variáveis de ambiente
3. **Se estiver usando `build.sh`**:
   - ✅ Está correto!
   - Apenas certifique-se que `API_BASE_URL` está configurada

## Teste Local

Para testar localmente qual script funciona melhor:

```bash
# Teste build.sh
export API_BASE_URL=https://pwa-camera-poc-api.onrender.com
./build.sh

# Verifique o output
cat bin/Release/net8.0/publish/wwwroot/appsettings.json

# Teste build-production.sh
export API_BASE_URL=https://pwa-camera-poc-api.onrender.com
./build-production.sh

# Verifique o output
cat dist/wwwroot/appsettings.json
```

Ambos devem mostrar a URL real da API, não `__API_BASE_URL__`.
