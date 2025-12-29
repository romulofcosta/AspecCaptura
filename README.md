# PWA Camera POC - Blazor

Este é um projeto de Prova de Conceito (POC) para uma Progressive Web App (PWA) de inventário utilizando câmera, desenvolvido com Blazor WebAssembly. O objetivo é demonstrar a integração de funcionalidades de câmera, autenticação local e armazenamento offline para um sistema de inventário simples.

## Objetivo

O projeto visa criar uma aplicação web que funcione offline, permitindo aos usuários fazer login, capturar itens via câmera, registrar inventário e sincronizar dados quando online. É direcionado para cenários de inventário móvel em ambientes com conectividade limitada.

## Funcionalidades

- **Autenticação Local**: Login e registro de usuários com armazenamento em localStorage (para compatibilidade).
- **Captura de Imagens**: Integração com câmera do dispositivo para fotografar itens.
- **Gerenciamento de Inventário**: Adição, edição e visualização de itens de inventário.
- **Armazenamento Offline**: Uso de IndexedDB para dados de inventário e localStorage para autenticação.
- **PWA Features**: Instalável, funciona offline, service worker para cache.
- **Temas**: Suporte a modo claro/escuro.
- **Responsividade**: Design adaptável para dispositivos móveis e desktop.

## Stack Tecnológica

- **Frontend**: Blazor WebAssembly (.NET 8)
- **UI Framework**: Fluent UI Blazor (Microsoft) - para componentes modernos e acessíveis
- **Linguagens**: C#, HTML, CSS, JavaScript
- **Armazenamento**: IndexedDB (para inventário), localStorage (para autenticação)
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

- **Microsoft.Fast.Components.FluentUI**: Biblioteca de componentes UI da Microsoft para Blazor, fornecendo componentes modernos, acessíveis e consistentes com o design Fluent.
  - Instalação: Incluído via NuGet (`Microsoft.Fast.Components.FluentUI`)
  - Motivo: Melhorar a interface do usuário com componentes padronizados, acessíveis e responsivos.

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
| Photos | List<string> | Não | Lista de fotos em base64 |
| CoverImage | string? | Não | Propriedade auxiliar que retorna a primeira foto (somente leitura) |

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

#### Estratégia de Sincronização
- Campo `Synced` indica se o item foi sincronizado.
- Contador global `PendingSyncCount` no AppState para itens pendentes.
- TODO: Implementar sincronização real com backend (atualmente apenas flag local).

### Gerenciamento de Imagens (Estado Atual)

- **Campo de Imagem:** `Photos` (List<string> de base64 data URLs).
- **Captura:** Via `CameraService.TakePhotoAsync()`, que retorna `canvas.toDataURL('image/jpeg', 0.9)` (base64 JPEG com 90% qualidade).
- **Armazenamento:** Junto ao item no IndexedDB (store `items`).
- **Formato e Compressão:** JPEG com compressão de 90%. Resolução baseada na câmera (ideal 1280x720).
- **Limites:** Sem limite explícito por imagem, mas base64 aumenta tamanho em ~33%. Recomendado <1MB por imagem para performance.

### Fluxo de Dados Atual

1. **Navegação para Câmera:** Usuário acessa `/camera` (Camera.razor).
2. **Inicialização:** `OnInitializedAsync` obtém usuário atual via `AuthService`.
3. **Start Câmera:** `OnAfterRenderAsync` chama `CameraService.StartCameraAsync("camera-feed", useFrontCamera)`.
4. **Captura:** Botão "Capture" chama `CapturePhoto()`, que:
   - Chama `CameraService.TakePhotoAsync("camera-feed")` para obter base64.
   - Para câmera, mostra form de metadados.
5. **Form de Metadados:** Usuário preenche `InventoryItem` (Name, Code, etc.).
6. **Adicionar Fotos Extras:** Botão "Add More Photo" permite capturar mais fotos.
7. **Salvamento:** `HandleSave()`:
   - Define `itemModel.Photos = capturedPhotos`.
   - Define `UnitId` e `Timestamp`.
   - Chama `DbService.AddAsync("items", itemModel)`.
   - Atualiza `appState.PendingSyncCount`.
   - Navega para `/home`.
8. **Páginas/Componentes Envolvidos:** Camera.razor, Home.razor (para listagem).
9. **Serviços:** CameraService, IndexedDbService, AuthService, ToastService, AppState.

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