# Arquitetura Técnica - ASPEC Capture

Esta documentação descreve a estrutura técnica e o fluxo de dados da aplicação Blazor WebAssembly.

## 🏗️ Estrutura de Serviços (Dependency Injection)

A aplicação utiliza Injeção de Dependência (DI) escopada (`Scoped`) para gerenciar os serviços principais:

| Serviço | Interface | Responsabilidade |
|---------|-----------|------------------|
| **AuthService** | `IAuthService` | Gestão de login, registro, logout e atualização de perfil. |
| **IndexedDbService** | `IIndexedDbService` | Interoperação com o banco de dados local do navegador. |
| **LocalStorageService** | `ILocalStorageService` | Persistência simples para tokens de sessão e preferências. |
| **CameraService** | - | Controle de hardware da câmera via JS Interop. |
| **AppState** | - | Gestão de estado global compartilhado (ex: contador de sincronização). |
| **ToastService** | - | Notificações rápidas para o usuário. |

## 🔐 Sistema de Autenticação

A autenticação é realizada localmente para suportar cenários 100% offline:
1. **Cadastro**: O usuário é salvo no IndexedDB (`users`).
2. **Login**: Verifica as credenciais e salva uma sessão no `localStorage`.
3. **Provider**: O `CustomAuthStateProvider` lê o `localStorage` para notificar o Blazor sobre o estado do usuário.
4. **Multi-Tenant**: O usuário pode estar associado a múltiplas Unidades Gestoras através de uma lista de IDs.

## 💿 Persistência de Dados (IndexedDB)

Utilizamos o IndexedDB para armazenar dados volumosos (imagens em Base64):
- **Store `users`**: Dados de perfil e senhas hasheadas.
- **Store `units`**: Unidades gestoras seeded automaticamente.
- **Store `states` / `cities`**: Dados de hierarquia geográfica.
- **Store `items`**: Inventários capturados.

## ⚛️ Estados Reativos

O componente `AppState` é injetado em todas as páginas e layouts para sincronizar dados em tempo real sem a necessidade de recarregar a página:
- `IsDarkMode`: Alternância global de tema.
- `PendingSyncCount`: Contador de itens não sincronizados.
- `CurrentUnitId`: Unidade selecionada no menu lateral.

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
