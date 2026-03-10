# Arquitetura Técnica - Aspec Captura

Esta documentação descreve a estrutura técnica e o fluxo de dados da aplicação Blazor WebAssembly.

## 🏗️ Estrutura de Serviços (Dependency Injection)

A aplicação utiliza Injeção de Dependência (DI) escopada (`Scoped`) para gerenciar os serviços principais:

| Serviço | Interface | Responsabilidade |
|---------|-----------|------------------|
| **AuthService** | `IAuthService` | Gestão de login via API, logout e atualização de perfil. |
| **IndexedDbService** | `IIndexedDbService` | Interoperação com o banco de dados local do navegador. |
| **LocalStorageService** | `ILocalStorageService` | Persistência simples para tokens de sessão e preferências. |
| **CameraService** | - | Controle de hardware da câmera via JS Interop. |
| **AppState** | - | Gestão de estado global compartilhado (ex: contador de sincronização). |
| **ToastService** | - | Notificações rápidas para o usuário. |

## 🔐 Sistema de Autenticação (Acesso Provisionado)

A autenticação é realizada via API Backend para centralização e controle:
1. **Provisionamento**: Usuários são gerenciados externamente e não podem se cadastrar via PWA.
2. **Login**: O `AuthService` envia as credenciais para a API (`/api/auth/login`).
3. **Cache & Sessão**: Após autenticação, o objeto de usuário é cacheado localmente e a sessão é salva no `localStorage`.
4. **Provider**: O `CustomAuthStateProvider` gerencia o estado de autenticação no Blazor.
5. **Multi-Tenant**: O usuário pode estar associado a múltiplas Unidades Gestoras através de uma lista de IDs retornada pela API.

## 💿 Persistência de Dados (IndexedDB)

Utilizamos o IndexedDB para armazenar dados volumosos (imagens em Base64):
- **Store `users`**: Cache de dados de perfil após login (operação offline).
- **Store `units`**: Unidades gestoras seeded automaticamente.
- **Store `states` / `cities`**: Dados de hierarquia geográfica.
- **Store `items`**: Inventários capturados.

## ⚛️ Estados Reativos

O componente `AppState` é injetado em todas as páginas e layouts para sincronizar dados em tempo real sem a necessidade de recarregar a página:
- `IsDarkMode`: Alternância global de tema.
- `PendingSyncCount`: Contador de itens não sincronizados.
- `CurrentOrgao`, `CurrentUO`, `CurrentArea`, `CurrentSubarea`: Hierarquia da sessão atual.

## 🚀 Ciclo de Vida da Aplicação

1. **Inicialização (`Program.cs`)**:
   - Registro de serviços.
   - Chamada assíncrona para `dbService.InitializeAsync()` antes do `RunAsync`.
2. **Início do App**:
   - `App.razor` define as rotas.
   - `MainLayout.razor` inicializa o tema baseado nas preferências do usuário.
3. **Uso Offline**:
   - O Service Worker intercepta requisições de rede.
   - O App renderiza a partir do cache local.
