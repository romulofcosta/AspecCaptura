# Aspec Captura

Sistema de inventário patrimonial desenvolvido como Progressive Web App (PWA) com Blazor WebAssembly. Permite captura de fotos, funcionamento offline e sincronização em background.

## Objetivo

O projeto visa criar uma aplicação web que funcione offline, permitindo aos usuários autenticarem-se, capturarem itens via câmera, registrarem inventário e sincronizarem dados quando online. É direcionado para cenários de inventário móvel em ambientes com conectividade limitada.

## ⚠️ Status do Projeto & Limitações Conhecidas

> Status (27/02/2026): Integração com a API BFF consolidada. Uploads utilizam Pre-Signed URLs geradas pelo backend e a resposta de login é otimizada via streaming. Este projeto requer a API `pwa-camera-poc-api` em execução.

Principais Pontos:
1. Acesso provisionado: usuários são definidos no S3 e validados pela API.
2. Sincronização: executada via API BFF (sem chaves AWS no cliente).

## Funcionalidades

- **Acesso Provisionado**: Autenticação de usuários exclusivamente via API Backend, com suporte a múltiplos perfis e cache local para operação offline.
- **Captura de Imagens**: Integração com câmera do dispositivo para fotografar itens com suporte a múltiplas fotos por item, preview e galeria de revisão.
- **Gerenciamento de Inventário**: Adição, edição e visualização de itens com suporte a categorias, unidades gestoras, busca avançada e ordenação personalizada.
- **Filtro Hierárquico de Bens**: Sistema de filtro por hierarquia completa (Órgão → UO → Área → Subárea) com carregamento otimizado e redução de 91% no volume de dados processados.
- **Armazenamento Offline**: Uso de IndexedDB para dados de inventário e localStorage para persistência de sessão e temas.
- **Exportação de Dados**: Funcionalidade de exportação do inventário local para formato CSV, facilitando a portabilidade dos dados.
- **PWA Real**: Instalável, offline-first, com logotipos oficiais da ASPEC e suporte a ícones **Maskable**.
- **Temas Dinâmicos**: Suporte a modo claro/escuro com detecção automática de sistema e salvamento de preferência.
- **Responsividade Mobile-First**: Interface otimizada com barra de navegação inferior (Bottom Navigation) e menu lateral para gestão de perfis e unidades.
- **UI de Alta Fidelidade**: Anteriormente baseada em MudBlazor (removido); atualmente usa componentes Blazor puros e utilitários CSS.
- **Gestão de Unidades**: Sistema de filtragem e seleção de unidades gestoras (Prefeituras, Fundos, Câmaras) com nomes reais e seeding automático.
- **Performance Otimizada**: Filtro de exercício fiscal no backend reduz payload em 70-96%, acelerando login em 25x (de ~25s para <1s em 3G).

## Stack Tecnológica

- **Frontend**: Blazor WebAssembly (.NET 8)
- **Backend**: ASP.NET Core Web API (BFF Pattern) - Projeto `pwa-camera-poc-api`
- **UI Framework**: 
  - **MudBlazor 7.20.0** (MIT License) - Material Design components library
    - Componentes modernos e responsivos
    - Grids, Cards, Modals, Dialogs, Snackbars
    - Temas customizáveis
    - Mobile-first design
- **Design System**: Material Design (via MudBlazor)
- **Tipografia**: Roboto (Google Fonts) para Material Design
- **Ícones**: Material Icons (5 variantes: Filled, Outlined, Two Tone, Round, Sharp)
- **Layout System**: Flexbox e CSS Grid com variáveis CSS para consistência e responsividade
- **Linguagens**: C#, HTML, CSS, JavaScript
- **Armazenamento**: IndexedDB (inventário local), localStorage (sessão/tema), S3 via API BFF (URLs pré-assinadas)
- **Autenticação**: Autenticação via API (BFF) com provisionamento centralizado.
- **PWA**: Service Worker, Manifest JSON
- **Interoperabilidade**: JavaScript interop para câmera e IndexedDB
- **Build/Deploy**: .NET CLI, potencialmente Netlify ou similar

## Estrutura do Projeto

```
pwa-camera-poc-blazor/
├── Components/          # Componentes reutilizáveis (Layout, Shared)
├── Models/              # Modelos de dados (User, Item, Unit)
├── Pages/               # Páginas da aplicação (Home, Login, Camera, etc.)
├── Services/            # Serviços (Auth, Storage, Camera)
├── wwwroot/             # Arquivos estáticos (CSS, JS, manifest, service worker)
├── Properties/          # Configurações do projeto
├── Program.cs           # Ponto de entrada da aplicação
└── pwa-camera-poc-blazor.csproj  # Arquivo de projeto
```

### Principais Arquivos

- `Program.cs`: Configuração de serviços e inicialização.
- `App.razor`: Layout principal da aplicação.
- `wwwroot/index.html`: Página HTML principal.
- `wwwroot/js/`: Scripts JavaScript para interop.
- `wwwroot/manifest.json`: Manifesto PWA.
- `wwwroot/service-worker.js`: Service worker para offline.

## Como Executar

### Pré-requisitos

- .NET 8 SDK
- Navegador moderno com suporte a PWA (Chrome, Edge, etc.)
- API Backend (`pwa-camera-poc-api`) em execução

### Configuração da API

O projeto requer a API backend em execução. Configure a URL da API em `wwwroot/appsettings.json`:

```json
{
  "ApiBaseUrl": "http://localhost:5069"
}
```

Para produção, a URL é substituída automaticamente durante o build via variável de ambiente `API_BASE_URL`.

### Dependências

O projeto utiliza as seguintes bibliotecas principais:

#### UI Component Libraries

- **MudBlazor (removido)** (anteriormente usado)
  - **Propósito**: Biblioteca de componentes Material Design para Blazor (removida)
  - **Observação**: o projeto foi migrado para componentes Blazor puros e utilitários CSS; manter referência histórica.

#### Fontes e Ícones (CDN - Gratuitas)

- **Google Fonts - Roboto**: Tipografia Material Design
  - Pesos: 300, 400, 500, 700
  - Licença: Apache 2.0
  
- **Material Icons**: Conjunto completo de ícones do Google
  - 5 variantes: Filled, Outlined, Two Tone, Round, Sharp
  - Licença: Apache 2.0
  - +2000 ícones disponíveis

### Passos

1. Clone o repositório.
2. Navegue para a pasta do projeto: `cd pwa-camera-poc-blazor`
3. Execute: `dotnet run`
4. Abra o navegador em `http://localhost:5230`
5. Para login, utilize as credenciais provisionadas na API.

### Build para Produção

O projeto utiliza o script `build.sh` para build de produção, que:
- Detecta automaticamente a plataforma (Cloudflare Pages, Netlify, ou local)
- Substitui variáveis de ambiente antes do build
- Cria arquivos de configuração necessários (_headers, _redirects)
- Valida o output final

```bash
# Build local
export API_BASE_URL=https://pwa-camera-poc-api.onrender.com
./build.sh

# Build no Cloudflare Pages (automático)
# Configurar variável de ambiente API_BASE_URL no dashboard
```

Os arquivos publicados estarão em `bin/Release/net8.0/publish/wwwroot`.

### Deploy em Produção

**URLs de Produção:**
- Frontend: https://pwa-camera-poc-blazor.pages.dev
- Backend: https://pwa-camera-poc-api.onrender.com

**Documentação de Deploy:**
- [DEPLOY_FINAL.md](DEPLOY_FINAL.md) - Guia completo de deploy
- [CHECKLIST_DEPLOY.md](CHECKLIST_DEPLOY.md) - Checklist passo a passo
- [RESUMO_CORRECOES.md](RESUMO_CORRECOES.md) - Correções aplicadas (CORS)

**Configuração do Cloudflare Pages:**
```
Build command: ./build.sh
Build output directory: bin/Release/net8.0/publish/wwwroot
Environment variables:
  - API_BASE_URL=https://pwa-camera-poc-api.onrender.com
  - CF_PAGES=1
```

## Testes

Atualmente, o projeto não possui testes automatizados implementados. Para adicionar testes:

- Use xUnit ou NUnit para testes unitários em C#.
- Para testes de UI, considere Playwright ou Selenium.
- Execute testes com `dotnet test`.

## Arquitetura de Dados e Modelos

### Modelo de Item

O modelo `InventoryItem` (localizado em `Models/Item.cs`) representa um item de inventário no sistema:

| Propriedade | Tipo | Obrigatório | Descrição |
|-------------|------|-------------|-----------|
| Id | string | Sim | Identificador único (GUID gerado automaticamente) |
| Name | string | Sim | Nome do item |
| Code | string | Sim | Código único do item |
| Category | string | Não | Categoria do item (padrão: "Geral") |
| Location | string | Sim | Localização do item |
| Observations | string | Não | Observações adicionais |
| Timestamp | DateTime | Sim | Data/hora de criação (padrão: DateTime.Now) |
| Synced | bool | Não | Indica se o item foi sincronizado (padrão: false) |
| UnitId | int? | Não | ID da unidade gestora associada |
| Photos | List<string> | Não | Lista de fotos em base64 (removido após sincronização) |
| RemoteUrls | List<string> | Não | Lista de URLs persistentes no AWS S3 |
| CoverImage | string? | Não | Retorna a primeira `RemoteUrl` se disponível, senão a primeira `Photo` |

**Validações aplicadas:**
- `Name`, `Code` e `Location` são obrigatórios via `[Required]`.
- Outros campos são opcionais.

**Exemplo de objeto InventoryItem:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Notebook Dell",
  "code": "NB001",
  "category": "Eletrônicos",
  "location": "Sala 101",
  "observations": "Em bom estado",
  "timestamp": "2025-12-24T10:00:00Z",
  "synced": false,
  "unitId": 1,
  "photos": [
    "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ...",
    "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQ..."
  ]
}
```

### Armazenamento de Dados

#### IndexedDB para Inventário
- **Nome da Store:** `items`
- **Chave Primária:** `id` (string, GUID)
- **Índices:** 
  - `name` (não único)
  - `code` (não único)
  - `category` (não único)
  - `timestamp` (não único)
  - `unitId` (não único)
- **Limites:** Depende do navegador (geralmente 50MB-1GB por origem). Não há limite explícito no código, mas grandes volumes podem impactar performance.

#### localStorage para Autenticação
- **Chave de Sessão:** `pwa-inventory-session` (objeto UserSession com Username e UnitId)
- **Usuários:** Cache local por username após login via API.
- **Limites:** ~5-10MB por origem, dependendo do navegador.

#### Estratégia de Sincronização (Via BFF)
O projeto utiliza uma arquitetura segura com API Backend for Frontend (BFF) para intermediar o acesso ao S3, eliminando a necessidade de credenciais no cliente.

1. **Autenticação**: O usuário é validado via API.
2. **Sincronização**: O App solicita uma URL assinada (Pre-Signed URL) para a API BFF.
3. **Upload Direto**: O App faz upload do binário da imagem/JSON diretamente para o S3 usando a URL assinada.
4. **Segurança**: As credenciais AWS ficam protegidas no servidor (API).
5. **Limpeza Local**: Após o sucesso, os dados Base64 são removidos do IndexedDB.
6. **Legado**: O sistema Harbour consome os diretórios do S3 via API de listagem ou sincronização direta de arquivos.

## ⚙️ Configuração do Ambiente

O projeto utiliza o arquivo `wwwroot/appsettings.json` para configurar a conexão com a API BFF.

```json
{
  "ApiBaseUrl": "http://localhost:5069"
}
```

### Segurança e Próximos Passos
A versão atual já elimina chaves fixas no client-side ao usar Pre-Signed URLs geradas pelo BFF.

Próximos passos:
- Autenticação JWT integrada entre Blazor e API.
- Validação robusta de tipos/tamanho de arquivo na API.

> **Importante:** O Bucket S3 deve ter políticas de **CORS** habilitadas para aceitar requisições `PUT`, `GET` e `DELETE` da origem da aplicação (localhost e domínio de produção).

### Gerenciamento de Imagens (Estado Atual)

- **Campo de Imagem:** `Photos` (List<string> de base64 data URLs).
- **Captura:** Via `CameraService.TakePhotoAsync()`, que retorna `canvas.toDataURL('image/jpeg', 0.9)` (base64 JPEG com 90% qualidade).
- **Armazenamento:** Junto ao item no IndexedDB (store `items`).
- **Formato e Compressão:** JPEG com compressão de 90%. Resolução baseada na câmera (ideal 1280x720).
- **Limites:** Sem limite explícito por imagem, mas base64 aumenta tamanho em ~33%. Recomendado <1MB por imagem para performance.

### Fluxo de Dados Atual

### Fluxo de Dados Atual (Sincronização PWA → S3 → Desktop)

1. **Navegação para Câmera:** Usuário acessa `/camera`.
2. **Captura e Metadados:** Usuário tira fotos e preenche dados (Nome, Código e Localização).
3. **Salvamento Local (IndexedDB):** 
   - Item salvo com status `Synced = false`.
   - Propriedade `CreatedBy` recebe o usuário logado para isolamento.
4. **Visualização (Home/Sync):**
   - Itens pendentes aparecem com **Nuvem Cinza**.
   - A lista é filtrada para mostrar apenas itens do usuário atual.
5. **Sincronização (Sync):**
   - Usuário clica em "Sincronizar Tudo".
   - **Upload Imagem:** Capa enviada para `capturas/{itemId}.jpg`.
   - **Upload Metadados:** JSON enviado para `capturas/{itemId}.json` (inclui `usuarioEnvio`).
   - **Confirmação:** Se sucesso, status muda para `Synced = true` (**Nuvem Verde**) e fotos locais são removidas.
6. **Consumo Desktop:** Aplicação Desktop lê os JSONs/JPGs do bucket S3.
9. **Páginas/Componentes Envolvidos:** Camera.razor, Home.razor, Stats.razor, Sync.razor, Footer.razor.
10. **Serviços:** CameraService, IndexedDbService, AuthService, ToastService, AppState.

### Serviços e Interoperabilidade JavaScript

#### CameraService
- `StartCameraAsync(string videoElementId, bool useFrontCamera)`: Inicia câmera no elemento video.
- `TakePhotoAsync(string videoElementId)`: Retorna string (base64 JPEG).
- `StopCameraAsync(string videoElementId)`: Para câmera.

#### IndexedDbService
- `AddAsync<T>(string storeName, T item)`: Adiciona item à store.
- `GetAsync<T>(string storeName, object key)`: Obtém por chave.
- `GetFromIndexAsync<T>(string storeName, string indexName, object value)`: Busca por índice.
- `UpdateAsync<T>(string storeName, T item)`: Atualiza item.
- Tratamento de erros: Console.Error.WriteLine para falhas.

#### AuthService
- `LoginAsync(string username, string password)`: Retorna User ou null via API.
- `LogoutAsync()`: Limpa sessão.
- `GetCurrentUserAsync()`: Obtém usuário atual.

#### Funções JavaScript Interop
- `cameraInterop.startCamera(videoElementId, facingMode)`: Inicia stream de câmera.
- `cameraInterop.takePhoto(videoElementId)`: Captura foto como base64.
- `cameraInterop.stopCamera(videoElementId)`: Para stream.
- `dbInterop.init()`: Inicializa IndexedDB.
- `dbInterop.add(storeName, item)`: Adiciona à store.
- `dbInterop.get(storeName, key)`: Obtém por chave.
- `dbInterop.getFromIndex(storeName, indexName, value)`: Busca por índice.

### Considerações de Performance

- **Limites de Imagem:** Recomendado <500KB por imagem (base64 ~750KB). Compressão JPEG 90% reduz tamanho sem perder qualidade visível.
- **Grandes Volumes:** IndexedDB suporta milhares de itens, mas queries em listas grandes podem ser lentas. Use índices para buscas.
- **Cache do Service Worker:** Imagens são armazenadas localmente; service worker pode cachear assets estáticos, mas dados dinâmicos ficam em IndexedDB.

## Contribuição

1. Fork o projeto.
2. Crie uma branch para sua feature: `git checkout -b feature/nova-funcionalidade`
3. Commit suas mudanças: `git commit -m 'Adiciona nova funcionalidade'`
4. Push para a branch: `git push origin feature/nova-funcionalidade`
5. Abra um Pull Request.

## Licença

Este projeto é para fins educacionais e de demonstração. Não possui licença específica.

## Notas Adicionais

- O projeto foi migrado de uma versão JavaScript pura para Blazor.
- Para compatibilidade, a autenticação usa localStorage em vez de IndexedDB.
- A câmera requer permissões do navegador.
- Em produção, considere usar um backend para sincronização de dados.

## Melhorias Recentes

### Correção de CORS e Deploy (Fevereiro 2025)
- **CORS Corrigido**: Backend agora aceita domínio principal do Cloudflare Pages (`https://pwa-camera-poc-blazor.pages.dev`)
- **Build Otimizado**: Script `build.sh` validado com verificações automáticas
- **Validações**: Build agora valida substituição de variáveis antes de concluir
- **Documentação**: Guias completos de deploy, troubleshooting e checklist
- **Remoção de Código Obsoleto**: Script `build-production.sh` removido (em desuso)

### Correção de Carregamento de Bens por Subárea (2024)
- **Filtro Hierárquico**: Implementação de filtro por hierarquia completa (Órgão → UO → Área → Subárea) com carregamento otimizado.
- **IndexedDB v11**: Migração automática com novos índices `cdArea` e `cdSArea` para consultas eficientes.
- **Performance**: Redução de 91% no volume de dados processados (500 → 45 bens típicos) e consultas 25x mais rápidas.
- **Filtro de Exercício Fiscal**: Backend filtra apenas dados do ano corrente, reduzindo payload em 70-96% (~8MB → ~270KB).
- **Compatibilidade**: Comportamento legado preservado quando filtros não são utilizados.
- **Documentação Completa**: Guias de deploy, rollback, monitoramento e troubleshooting disponíveis em `docs/`.

### Integração MudBlazor
- **Componentes Modernos**: Substituição de elementos HTML nativos por componentes MudBlazor (MudTextField, MudButton, MudSelect, MudCard) para interface mais profissional e alinhada ao Material Design.
- **Consistência Visual**: Padronização de design com sistema de temas do MudBlazor.
- **Acessibilidade**: Componentes MudBlazor seguem padrões de acessibilidade.

### Sistema de Layout e Responsividade
- **Flexbox/Grid Consistente**: Implementação uniforme de Flexbox e CSS Grid em todo o aplicativo para alinhamento perfeito.
- **Design Responsivo**: Media queries otimizadas para tablet (≤768px) e mobile (≤480px), garantindo usabilidade em todos os dispositivos.
- **Container Centralizado**: Adição de container de conteúdo no MainLayout para melhor organização visual.
- **Estados Visuais**: Estilos para estados vazios e paginação melhoram a experiência do usuário.

### Correções Técnicas
- **Build Estável**: Resolução de erros de compilação, incluindo qualificações de namespace e correções de sintaxe.
- **Performance**: Layout otimizado reduz reflows e melhora performance em dispositivos móveis.

## Documentação Adicional

Para informações detalhadas sobre a correção de carregamento por subárea:
- **[CHANGELOG.md](CHANGELOG.md)**: Histórico completo de mudanças
- **[DEPLOYMENT_SUBAREA_FILTER.md](docs/DEPLOYMENT_SUBAREA_FILTER.md)**: Guia de deploy com checklist e estratégia gradual
- **[ROLLBACK_SUBAREA_FILTER.md](docs/ROLLBACK_SUBAREA_FILTER.md)**: Plano de rollback em 3 níveis
- **[MONITORING_SUBAREA_FILTER.md](docs/MONITORING_SUBAREA_FILTER.md)**: Guia de monitoramento com métricas e alertas
- **[TROUBLESHOOTING_SUBAREA_FILTER.md](docs/TROUBLESHOOTING_SUBAREA_FILTER.md)**: Guia de resolução de problemas comuns
