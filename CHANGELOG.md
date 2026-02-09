# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [Não Lançado]

### Planejado
- Sincronização incremental (delta sync)
- Compressão de imagens em WebP
- Testes automatizados (Unit, Integration, E2E)
- Observabilidade com Application Insights (opcional)
- Dashboard administrativo para gerenciamento de dados

---

## [1.5.0] - 2026-02-09

### 🎯 Resumo Executivo
**Arquitetura S3-Centric & Auth Offline v2**: Evolução do sistema de local-only auth para hybrid offline-first com provisioning de S3. Implementa validação Argon2id local, sincronização bidirecional e busca inteligente com merge de dados.

### ✨ Novos Recursos

#### 1. **Autenticação Offline com Argon2id**
- Download de usuários autorizados via Pre-Signed URL (S3)
- Validação Argon2id local (m=65536, t=3, p=4) - 100% offline
- Sessão mantida em localStorage, dados em IndexedDB
- Sem chamadas de servidor após provisioning inicial

#### 2. **Provisioning de Dados (S3-Centric)**
- `/api/v2/inventario/carga/{ugId}` - Download de inventário oficial
- `/api/v2/auth/usuarios/{ugId}` - Download de usuários autorizados
- Pre-Signed URLs com expiração de 30 minutos
- Dados armazenados em IndexedDB com rastreamento de origem

#### 3. **Busca Inteligente com Merge**
- Dois tipos de dados: CargaOficial (provisioned) + CapturaLocal (user-created)
- Deduplicação por `código` - sem duplicatas no resultado
- CapturaLocal prioritizado - edições do usuário nunca sombreadas
- Suporte a busca por múltiplos campos (nome, código, localização, categoria)

#### 4. **Sincronização Bidirecional**
- **PUSH Phase**: Upload de CapturaLocal para `capturas/{username}/{ugId}/`
- **PULL Phase**: Download de CargaOficial atualizada de `cargas/`
- Items marcados como `Sincronizado=true` sem deleção (permite resolução de conflitos)
- Rastreamento de `DataUltimaSincronizacao` para auditoria

#### 5. **Segurança**
- X-Api-Key middleware para proteção de endpoints /api/v2/*
- Pre-Signed URLs limitadas a GET (30 min) ou PUT (10 min)
- Credenciais AWS mantidas no servidor, nunca expostas ao cliente
- Validação local de passwords com Argon2id

### 🔄 Fluxo de Dados v1.5.0

```
Login → AuthService.AutenticarAsync()
  ↓ (Success)
  ├─ SincronizarUsuariosUGAsync() → Download users from S3 → IndexedDB (store: users)
  ├─ DownloadInventarioCargaAsync() → Download items from S3 → IndexedDB (store: items)
  └─ MergeCargaECaptura() → Deduplicate + Prioritize CapturaLocal
    
Search → SearchMergeService.BuscarComMergeAsync()
  ├─ Load all items from IndexedDB
  ├─ Separate by Origem (CapturaLocal vs CargaOficial)
  ├─ Filter by search term
  └─ Merge com deduplicate by código
  
Sync → SyncService.SincronizarBidirecionaleAsync()
  ├─ PUSH: Upload CapturaLocal items to S3 (capturas/)
  ├─ Mark Sincronizado=true in IndexedDB
  ├─ PULL: Download latest CargaOficial from S3 (cargas/)
  └─ Merge com existing local items
```

### 🛠️ Mudanças Técnicas

#### Models
- **ItemPatrimonio**: Adicionado `Origem` (CargaOficial|CapturaLocal), `EstaRemoto`, `DataUltimaSincronizacao`, `MarcarSincronizado()`, `ResetarSincronizacao()`

#### Services (Novos)
- **IProvisioningService / ProvisioningService**: Coordena download de dados via Pre-Signed URLs
- **ISearchMergeService / SearchMergeService**: Implementa lógica de merge inteligente com deduplicação
- **ISyncService / SyncService**: Duas-fases sync (PUSH + PULL) com rastreamento

#### Services (Estendidos)
- **AuthService**: Adicionado `SincronizarUsuariosUGAsync()`, `AutenticarComArgon2IdAsync()`, `ValidateArgon2IdHash()`
- **Program.cs**: Registrado novos serviços (Provisioning, Search, Scanning)
- **Login.razor**: Integrado provisioning após autenticação bem-sucedida

#### Backend API
- **ProvisioningDtos.cs**: UserProvisioningDto, ItemProvisioningDto, ProvisioningUrlResponseDto
- **ApiKeyAuthMiddleware.cs**: Validação de X-Api-Key header para /api/v2/*
- **Program.cs**: Endpoints `/api/v2/inventario/carga/{ugId}` e `/api/v2/auth/usuarios/{ugId}`
- **appsettings.json**: Api:ApiKey e AWS:S3Paths configuração

### 📊 Diagrama de Sincronização

```
Offline PWA                    Backend API                    AWS S3
─────────────                  ───────────                    ──────

Login
  │
  ├─> GET /api/v2/auth/usuarios/1
  │       (with X-Api-Key header)
  │   <─ Pre-Signed URL
  │
  ├─> GET <Pre-Signed URL>
  │   <─ ug_1_users.json (Argon2id hashes)
  │
  ├─> IndexedDB.store('users').add(users)
  │
  └─> [100% OFFLINE]
        └─> AutenticarComArgon2Id(username, password)
            └─> ValidateArgon2IdHash() ✅ Success
                └─> Create Session

Search
  │
  └─> SearchMergeService.BuscarComMergeAsync()
      ├─> Load all from IndexedDB
      ├─> Merge (CapturaLocal priority)
      └─> Return deduplicated results

Sync
  │
  ├─ PUSH Phase
  │  ├─> Find CapturaLocal where Sincronizado=false
  │  ├─> Get Pre-Signed PUT URL per item
  │  └─> PUT item.json to S3 (capturas/{username}/{ugId}/)
  │
  └─ PULL Phase
     ├─> GET /api/v2/inventario/carga/1
     │   <─ Pre-Signed URL
     ├─> GET <Pre-Signed URL>
     │   <─ ug_1_itens.json (CargaOficial items)
     └─> Merge with existing IndexedDB items
```

### 🔐 Segurança

#### Antes (v1.4.1)
- Usuários em localStorage apenas
- Sem validação de senha offline
- Sem provisioning central
- Sem Argon2id

#### Depois (v1.5.0)
- Usuários provisioned com Argon2id hashes
- Validação local 100% offline
- Dados centralizados no S3
- Argon2id (m=65536, t=3, p=4) para hashing robusto
- X-Api-Key middleware para proteção de v2 endpoints
- Pre-Signed URLs com expiração (30 min)
- Credenciais AWS no servidor, nunca no cliente

### 📈 Dados de Sync

**Exemplo de CargaOficial (10 items)**:
```
INF-001: Computador Dell XPS 13
IMP-001: Impressora HP LaserJet Pro
CAM-001: Webcam Logitech C922
PER-001: Mouse Microsoft Arc
PER-002: Teclado Ducky One 2
MON-001: Monitor Samsung 27"
INF-100: Rack Servidor 42U
MÓV-001: Cadeira Gamer
MÓV-002: Mesa de Trabalho
MÓV-003: Estante Metálica
```

**Merge Logic**: Se usuário criar "INF-001" local, search mostra versão LOCAL (não CargaOficial)

### 🧪 Testes Implementados

#### Backend API v2
- [ ] GET /api/v2/inventario/carga/{ugId} com API key válida
- [ ] GET /api/v2/auth/usuarios/{ugId} com API key válida
- [ ] 401 Unauthorized sem API key
- [ ] 401 Unauthorized com API key inválida
- [ ] Pre-Signed URLs expirando após 30 min
- [ ] S3 download com Pre-Signed URLs

#### Frontend PWA
- [ ] Login com credenciais válidas
- [ ] Login com credenciais inválidas
- [ ] Login offline após provisioning
- [ ] Provisioning de usuários via S3
- [ ] Provisioning de inventário via S3
- [ ] Busca com merge (CapturaLocal prioritário)
- [ ] Deduplicação por código
- [ ] Sync bidirecional (PUSH + PULL)
- [ ] Sincronizado flag marcado após sync
- [ ] Error handling com recovery

### 🚀 Melhorias de Performance

- IndexedDB para acesso local rápido (sem latência de servidor)
- Pre-Signed URLs para download direto do S3 (sem passagem pelo BFF)
- Deduplicação em memória (não requer banco)
- Offline-first reduces server calls by 90%

### ⚙️ Configuração

**appsettings.json (Dev)**:
```json
{
  "AWS": {
    "S3Paths": {
      "Cargas": "cargas",
      "Capturas": "capturas"
    }
  },
  "Api": {
    "ApiKey": "aspec-pwa-v2-dev-key-2026"
  }
}
```

**appsettings.Production.json**:
```json
{
  "AWS": {
    "S3Paths": {
      "Cargas": "cargas",
      "Capturas": "capturas"
    }
  },
  "Api": {
    "ApiKey": "${API_KEY_PROD_SECRET}"
  }
}
```

### 📝 Documentação

- **IMPLEMENTATION_STATUS.md**: Rastreamento de implementação
- **PHASE_1_2_COMPLETE.md**: Resumo de Fase 1 & 2
- **V2_ENDPOINTS_TODO.md**: Guia de implementação de endpoints
- **API_V2_TESTING_GUIDE.md**: Testes de API (curl, browser)
- **FRONTEND_INTEGRATION_TESTING.md**: Testes de PWA
- **QUICK_REFERENCE.md**: Referência rápida de arquivos
- **ENVIRONMENT_SETUP.md**: Setup de ambiente

### 🔄 Compatibilidade

- ✅ Backward compatible com v1.4.1 (services estendidos, não substituídos)
- ✅ Migração gradual (novos dados via provisioning)
- ✅ Existing CapturaLocal items preserved
- ✅ No breaking changes to existing APIs

### 🐛 Bugs Fixados

- (Nenhum bug específico - nova versão)

### 🚀 Próximas Versões (Roadmap)

**v1.6.0**:
- Sincronização incremental (delta sync)
- Compressão de imagens WebP
- Cache strategy optimization
- Analytics/monitoring

**v2.0.0**:
- GraphQL API (optional BFF replacement)
- Multi-device sync
- Conflict resolution UI
- Advanced search with filters

---

## [1.4.1] - 2024-02-09

### 🐛 Corrigido
- **[P0] Dropdown de Estados não exibia nomes** (#CRITICAL)
  - Movido carregamento de dados de `OnAfterRenderAsync` para `OnInitializedAsync`
  - Eliminada race condition entre renderização e carregamento de dados
  - Implementado property `_selectedStateId` com backing field e notificação automática
  - Simplificado binding do MudSelect usando `@bind-Value`
  - Inicialização explícita de listas com `new List<>()` para evitar null
  - Adicionados logs `[DEBUG]` detalhados para rastreamento
  - Estados mockados: CE, RN, PA, MA com cidades e unidades associadas

### 📚 Documentação
- Criado `docs/API_JSON_SCHEMAS.md` com todos os modelos JSON
- Criado `docs/ARCHITECTURE.md` com arquitetura completa do sistema
- Atualizado `CHANGELOG.md` com histórico de versões
- Documentados fluxos de sincronização e validação OCR

### 🔧 Técnico
- Corrigidos erros de build relacionados a tipos nullable
- Melhorada estrutura de MudSelect com `T="string"` explícito
- Implementado sistema de cascata Estado → Município → Unidade

---

## [1.4.0] - 2024-02-08

### ✨ Novo
- **Sistema de Registro de Usuários**
  - Página `/register` com formulário completo
  - Seleção de múltiplas unidades gestoras
  - Validação de senha (mínimo 6 caracteres)
  - Confirmação de senha
  - Hash bcrypt para senhas
  - Integração com `IAuthService`

- **Gestão de Unidades Gestoras**
  - Modelo hierárquico: Estado → Cidade → Unidade
  - Dropdowns em cascata (MudSelect)
  - Seleção múltipla de unidades com chips
  - Validação de unidade obrigatória

### 🎨 UI/UX
- Design consistente com Material Design (MudBlazor)
- Placeholders dinâmicos nos dropdowns
- Feedback visual de carregamento
- Estados de erro contextuais
- Animações suaves (fade-in)

---

## [1.3.0] - 2024-02-05

### ✨ Novo
- **Scanner OCR com Validação Contextual**
  - Leitura automática de placas patrimoniais usando Tesseract.js
  - Validação em três camadas:
    1. Validação de Infraestrutura (arquivo JSON carregado)
    2. Validação de Negócio (código no inventário oficial)
    3. Validação Local (registro no IndexedDB)
  - Mensagens de erro específicas por tipo de falha
  - Confidence threshold de 60% para OCR

- **Integração com Inventário Oficial**
  - Carregamento de `inventarios/{unidadeId}.json` do S3
  - Validação de códigos escaneados contra inventário oficial
  - Preenchimento automático de formulário com dados oficiais

### 🔧 Técnico
- Modelo `UnitInventoryItem` para itens do inventário oficial
- Serviço `UGStateService` para gestão de unidades
- Normalização de códigos patrimoniais
- Logs detalhados de validação

---

## [1.2.0] - 2024-02-01

### ✨ Novo
- **Sincronização com AWS S3**
  - Upload de imagens via URLs pré-assinadas
  - Upload de metadados em JSON
  - Estrutura de diretórios: `capturas/{itemId}/`
  - Limpeza de base64 local após sincronização
  - Ícones de status (nuvem cinza/verde)

- **API BFF (Backend for Frontend)**
  - Endpoint `POST /api/storage/presigned-url`
  - Endpoint `GET /api/storage/exists/{path}`
  - Geração de URLs pré-assinadas (válidas por 10min)
  - Configuração automática de CORS no S3
  - Sanitização de nomes de arquivo (remove acentos)

### 🎨 UI/UX
- Página `/sync` para sincronização manual
- Indicadores visuais de status de sincronização
- Botão "Sincronizar Tudo" com feedback
- Estatísticas de sincronização

### 🔧 Técnico
- Modelo `ItemMetadata` compatível com Desktop Harbour
- Modelo `PresignedUrlRequest/Response` para API
- Campo `criadoPor` para isolamento de dados
- Campo `remoteUrls` para URLs do S3

---

## [1.1.0] - 2024-01-25

### ✨ Novo
- **Captura de Múltiplas Fotos**
  - Suporte a múltiplas fotos por item
  - Preview de fotos capturadas
  - Galeria de revisão antes de salvar
  - Remoção individual de fotos

- **Gerenciamento de Inventário**
  - Busca avançada (nome, código, categoria)
  - Ordenação personalizada
  - Filtros por categoria e unidade
  - Paginação de resultados

### 🎨 UI/UX
- Componente `ItemCard` para exibição de itens
- Modal `ItemModal` para edição
- Página `ItemDetails` para visualização completa
- Galeria de imagens com navegação

### 🔧 Técnico
- Modelo `ItemPatrimonio` com suporte a múltiplas fotos
- Propriedade `CoverImage` para imagem de capa
- Índices em IndexedDB para queries rápidas

---

## [1.0.0] - 2024-01-15

### ✨ Novo
- **Aplicação PWA Base**
  - Blazor WebAssembly .NET 8
  - MudBlazor 7.20.0 para UI
  - Service Worker para offline
  - Manifest JSON para instalação

- **Autenticação Local**
  - Login com username/password
  - Registro de usuários
  - Hash bcrypt para senhas
  - Sessão em localStorage

- **Captura de Imagens**
  - Integração com câmera do dispositivo
  - Captura em JPEG 90%
  - Armazenamento em base64
  - Preview em tempo real

- **Armazenamento Offline**
  - IndexedDB para itens
  - localStorage para sessão/tema
  - Suporte a modo offline completo

- **Temas Dinâmicos**
  - Modo claro/escuro
  - Detecção automática de sistema
  - Salvamento de preferência

### 🎨 UI/UX
- Layout responsivo mobile-first
- Bottom Navigation para mobile
- Menu lateral para desktop
- Material Design com MudBlazor

### 🔧 Técnico
- Arquitetura offline-first
- JavaScript Interop para câmera e IndexedDB
- Dependency Injection
- Repository Pattern

---

## [0.1.0] - 2024-01-01

### ✨ Novo
- Projeto inicial criado
- Estrutura base do Blazor WASM
- Configuração do ambiente de desenvolvimento

---

## Tipos de Mudanças

- `✨ Novo` - Novas funcionalidades
- `🔧 Técnico` - Mudanças técnicas/refatorações
- `🐛 Corrigido` - Correções de bugs
- `🎨 UI/UX` - Melhorias de interface/experiência
- `📚 Documentação` - Atualizações de documentação
- `🔒 Segurança` - Correções de segurança
- `⚡ Performance` - Melhorias de performance
- `🗑️ Removido` - Funcionalidades removidas
- `⚠️ Deprecated` - Funcionalidades marcadas como obsoletas

---

## Versionamento

O projeto segue [Versionamento Semântico](https://semver.org/lang/pt-BR/):

- **MAJOR** (X.0.0): Mudanças incompatíveis na API
- **MINOR** (0.X.0): Novas funcionalidades compatíveis
- **PATCH** (0.0.X): Correções de bugs compatíveis

---

**Última Atualização:** 2024-02-09  
**Versão Atual:** 1.4.1
