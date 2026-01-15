# PWA Camera POC - Blazor

Este é um projeto de Prova de Conceito (POC) para uma Progressive Web App (PWA) de inventário utilizando câmera, desenvolvido com Blazor WebAssembly. O objetivo é demonstrar a integração de funcionalidades de câmera, autenticação local e armazenamento offline para um sistema de inventário simples.

## Objetivo

O projeto visa criar uma aplicação web que funcione offline, permitindo aos usuários fazer login, capturar itens via câmera, registrar inventário e sincronizar dados quando online. É direcionado para cenários de inventário móvel em ambientes com conectividade limitada.

## ⚠️ Status do Projeto & Limitações Conhecidas

> **Nota Crítica (09/01/2026):** Uma análise técnica detalhada identificou bloqueios na validação de login e incompatibilidade do SDK da AWS com o ambiente WebAssembly puro para uploads S3.

Para detalhes completos e soluções propostas, consulte o [Relatório de Análise Técnica](docs/ANALYSIS_REPORT.md).

**Principais Pontos de Atenção:**
1.  **Login**: Validação de formato de e-mail impede uso de usuários de teste simples (ex: `admin`).
2.  **Sincronização**: O upload direto via AWS SDK gera erros de plataforma (`PlatformNotSupportedException`). A solução recomendada é a implementação de **Pre-Signed URLs**.

## Funcionalidades

- **Autenticação Local**: Login e registro de usuários com armazenamento em localStorage e suporte a múltiplos perfis.
- **Captura de Imagens**: Integração com câmera do dispositivo para fotografar itens com suporte a múltiplas fotos por item, preview e galeria de revisão.
- **Gerenciamento de Inventário**: Adição, edição e visualização de itens com suporte a categorias, unidades gestoras, busca avançada e ordenação personalizada.
- **Armazenamento Offline**: Uso de IndexedDB para dados de inventário e localStorage para persistência de sessão e temas.
- **Exportação de Dados**: Funcionalidade de exportação do inventário local para formato CSV, facilitando a portabilidade dos dados.
- **PWA Real**: Instalável, offline-first, com logotipos oficiais da ASPEC e suporte a ícones **Maskable**.
- **Temas Dinâmicos**: Suporte a modo claro/escuro com detecção automática de sistema e salvamento de preferência.
- **Responsividade Mobile-First**: Interface otimizada com barra de navegação inferior (Bottom Navigation) e menu lateral para gestão de perfis e unidades.
- **UI de Alta Fidelidade**: Baseada em MudBlazor, oferecendo uma experiência Material Design refinada e profissional.
- **Gestão de Unidades**: Sistema de filtragem e seleção de unidades gestoras (Prefeituras, Fundos, Câmaras) com nomes reais e seeding automático.

## Stack Tecnológica

- **Frontend**: Blazor WebAssembly (.NET 8)
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
- **Armazenamento**: IndexedDB (inventário local), localStorage (sessão/tema), armazenamento remoto via API de integração (ex.: S3 usando URLs pré-assinadas)
- **Autenticação**: Autenticação local baseada em armazenamento no navegador
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

### Dependências

O projeto utiliza as seguintes bibliotecas principais:

#### UI Component Libraries

- **MudBlazor 7.20.0** (Gratuita, MIT License)
  - **Propósito**: Biblioteca de componentes Material Design para Blazor
  - **Instalação**: `dotnet add package MudBlazor --version 7.20.0`
  - **Recursos**:
    - +60 componentes prontos para uso
    - Sistema de temas customizável
    - Responsividade mobile-first
    - Grids, Cards, Dialogs, Snackbars, DataTables
    - Documentação completa: https://mudblazor.com
  - **Motivo**: Proporcionar UI moderna, profissional e responsiva seguindo Material Design guidelines

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
5. Para login, use as credenciais padrão: usuário `admin`, senha `admin`

### Build para Produção

```bash
dotnet publish -c Release
```

Os arquivos publicados estarão em `bin/Release/net8.0/publish/wwwroot`.

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
- **Usuários:** Armazenados por username (ex: chave "admin" para User object)
- **Limites:** ~5-10MB por origem, dependendo do navegador.

#### Estratégia de Sincronização Serverless (Modo Atual)
1. **Autenticação**: O usuário é validado localmente (LocalStorage).
2. **Sincronização**: O cliente consome uma **API de integração** configurada em `appsettings.json` (`IntegracaoApiUrl`), que expõe endpoints para geração de **URLs pré-assinadas** e operações de armazenamento.
3. **Upload Mídia**: Imagens em Base64 são convertidas para bytes e enviadas via `HttpClient` com um `PUT` direto para a URL pré-assinada.
4. **Upload Metadata**: Um JSON de metadados é serializado e enviado via `HttpClient` para a URL pré-assinada correspondente.
5. **Limpeza Local**: Após o sucesso, os dados Base64 são removidos do IndexedDB e substituídos por URLs remotas persistentes.
6. **Legado**: O sistema Harbour (ou outro consumidor) lê os arquivos gerados no storage em nuvem através da própria API de integração ou de serviços de backend.

## ⚙️ Configuração do Ambiente

O projeto utiliza o arquivo `wwwroot/appsettings.json` para definir a URL da **API de integração** responsável por gerar URLs pré-assinadas e conversar com o storage em nuvem.

```json
{
  "Aws": {
    "IntegracaoApiUrl": "https://sua-api-de-integracao.example.com"
  }
}
```

### Roadmap de Segurança
Para a versão de produção, recomenda-se:
- Manter todas as credenciais de storage apenas na API/Backend.
- Utilizar URLs pré-assinadas com tempo de expiração curto.
- Implementar controle de escopo por usuário no backend (por exemplo, via claims de usuário na API).

### Gerenciamento de Imagens (Estado Atual)

- **Campo de Imagem:** `Photos` (List<string> de base64 data URLs).
- **Captura:** Via `CameraService.TakePhotoAsync()`, que retorna `canvas.toDataURL('image/jpeg', 0.9)` (base64 JPEG com 90% qualidade).
- **Armazenamento:** Junto ao item no IndexedDB (store `items`).
- **Formato e Compressão:** JPEG com compressão de 90%. Resolução baseada na câmera (ideal 1280x720).
- **Limites:** Sem limite explícito por imagem, mas base64 aumenta tamanho em ~33%. Recomendado <1MB por imagem para performance.

### Fluxo de Dados Atual

### Fluxo de Dados Atual (Sincronização PWA → API de Integração → Storage/Desktop)

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
   - A aplicação solicita URLs pré-assinadas à API de integração.
   - **Upload Imagem:** Capa enviada via `HttpClient` para a URL pré-assinada (ex.: `capturas/{itemId}.jpg`).
   - **Upload Metadados:** JSON enviado via `HttpClient` para a URL pré-assinada (ex.: `capturas/{itemId}.json`, inclui `usuarioEnvio`).
   - **Confirmação:** Se sucesso, status muda para `Synced = true` (**Nuvem Verde**) e fotos locais são removidas.
6. **Consumo Desktop:** Aplicação Desktop ou serviços de backend leem os JSONs/JPGs do storage em nuvem.
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
- `LoginAsync(string username, string password)`: Retorna User ou null.
- `RegisterAsync(string username, string password, List<int> unitIds)`: Cria usuário.
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
