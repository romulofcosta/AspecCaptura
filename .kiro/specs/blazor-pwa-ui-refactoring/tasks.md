# Implementation Plan: Refatoração Blazor PWA UI

## Overview

Este plano de implementação detalha as tasks para a refatoração completa do front-end da aplicação "Aspec Captura - Inventário Patrimonial", um Progressive Web App (PWA) desenvolvido em Blazor WebAssembly.

A implementação está organizada em 6 fases seguindo o roadmap do design, com duração estimada de 12 semanas. Cada task referencia os requisitos e propriedades de correção relevantes para garantir rastreabilidade completa.

**Escopo da Refatoração:**
- 4 telas principais (Login, Dashboard, Configuração de Sessão, Detalhes do Patrimônio)
- 5 componentes reutilizáveis (StatisticsCard, ItemCard, HierarchicalDropdown, ConservationStateChip, BottomNavigation)
- 7 serviços principais (Auth, Crypto, Sync, Notification, Camera, ImageCompressor, AppState)
- Sistema completo de segurança (JWT, AES-GCM, brute force protection, auto-logout)
- Performance otimizada (batching, lazy loading, compressão de imagens)
- Testes abrangentes (bUnit + FsCheck, 80% cobertura componentes, 70% páginas)

**Metas de Qualidade:**
- 80% de cobertura de testes para componentes reutilizáveis
- 70% de cobertura de testes para páginas
- 27 propriedades de correção implementadas com testes PBT
- Performance: < 3s load time, < 5s TTI em 3G
- Acessibilidade: WCAG AA mínimo

## Tasks

### FASE 1: Foundation (Semana 1-2)

- [x] 1. Configurar estrutura do projeto e dependências
  - Criar estrutura de pastas conforme design (Pages, Components, Services, Models, Tests)
  - Adicionar pacotes NuGet: bUnit, xUnit, Moq, FluentAssertions, FsCheck, Bogus
  - Configurar projeto de testes (Tests.csproj)
  - Configurar injeção de dependências no Program.cs
  - _Requirements: 18.1, 18.7, 32.1_

- [x] 2. Implementar modelos de dados core
  - [x] 2.1 Criar classes de modelo (Item, Usuario, SessionToken, SessionConfig, HierarchyNode, Notification)
    - Implementar Item com campos criptografados (EncryptedDescription, EncryptedLocation, EncryptedObservations)
    - Implementar enums (ConservationState, NotificationType, NotificationPriority, HierarchyLevel)
    - Adicionar validação de propriedades
    - _Requirements: 7.1, 7.4, 7.10, 21.1, 23.3_
  
  - [ ]* 2.2 Escrever testes unitários para modelos
    - Testar validação de campos obrigatórios
    - Testar enums e conversões
    - _Requirements: 32.4_


- [x] 3. Implementar serviços de infraestrutura base
  - [x] 3.1 Criar LocalStorageService (wrapper para localStorage)
    - Implementar métodos GetItem, SetItem, RemoveItem, Clear
    - Adicionar serialização/deserialização JSON
    - Implementar tratamento de erros (storage full, quota exceeded)
    - _Requirements: 13.2, 21.2_
  
  - [x] 3.2 Criar JavaScript Interop para IndexedDB
    - Criar wwwroot/js/storage.js com funções de acesso ao IndexedDB
    - Implementar schema do banco (items, photos, notifications, syncQueue)
    - Adicionar índices conforme design
    - _Requirements: 13.2, 14.5_
  
  - [x] 3.3 Criar AppState service (gerenciamento de estado global)
    - Implementar propriedades (CurrentUser, CurrentSession, Statistics, IsLoading, IsOffline)
    - Implementar INotifyPropertyChanged para notificações de mudança
    - Adicionar métodos UpdateStatisticsAsync, SetOfflineMode, SaveStateAsync, LoadStateAsync
    - _Requirements: 2.1, 2.2, 16.7_
  
  - [ ]* 3.4 Escrever testes unitários para AppState
    - Testar notificações de mudança de propriedade
    - Testar persistência e carregamento de estado
    - _Requirements: 32.4_


- [x] 4. Implementar CryptoService (criptografia AES-GCM)
  - [x] 4.1 Criar JavaScript Interop para Web Crypto API
    - Criar wwwroot/js/crypto.js com funções encrypt, decrypt, deriveKey
    - Implementar AES-GCM com chaves de 256 bits
    - Adicionar suporte para streaming encryption (arquivos > 500KB)
    - _Requirements: 23.1, 23.2, 23.5, 24.7_
  
  - [x] 4.2 Implementar ICryptoService e CryptoService em C#
    - Implementar EncryptAsync, DecryptAsync para strings
    - Implementar EncryptBytesAsync, DecryptBytesAsync para fotos
    - Implementar InitializeKeyAsync (derivação de chave com PBKDF2)
    - Implementar ClearKeysAsync para logout
    - Adicionar tratamento de erros (CryptoException)
    - _Requirements: 23.3, 23.7, 24.1_
  
  - [ ]* 4.3 Escrever testes unitários para CryptoService
    - Testar criptografia/descriptografia de strings
    - Testar criptografia/descriptografia de bytes
    - Testar tratamento de erros
    - _Requirements: 32.4_
  
  - [ ]* 4.4 Escrever teste PBT para round-trip de criptografia
    - **Property 18: Data Encryption Round-Trip**
    - **Validates: Requirements 23.4, 24.3**
    - Para qualquer dado, encrypt → decrypt deve retornar o original
    - _Requirements: 23.4, 24.3_


- [x] 5. Implementar AuthService e BruteForceProtection
  - [x] 5.1 Criar BruteForceProtection class
    - Implementar rastreamento de tentativas falhas por username
    - Implementar lockout de 15 minutos após 5 tentativas
    - Armazenar estado em LocalStorage
    - Implementar métodos IsLockedOutAsync, RecordFailedAttemptAsync, ResetAttemptsAsync
    - _Requirements: 22.1, 22.2, 22.3, 22.7_
  
  - [x] 5.2 Implementar IAuthService e AuthService
    - Implementar LoginAsync com validação de brute force
    - Implementar LogoutAsync com limpeza de token e chaves
    - Implementar ValidateTokenAsync (verificação de expiração)
    - Implementar RefreshTokenAsync (30 min antes da expiração)
    - Implementar rastreamento de atividade do usuário
    - Implementar auto-logout após 30 min de inatividade
    - Adicionar evento OnAuthStateChanged
    - _Requirements: 1.6, 1.7, 21.1, 21.4, 21.6, 26.1, 26.2, 26.5_
  
  - [ ]* 5.3 Escrever testes unitários para BruteForceProtection
    - Testar lockout após 5 tentativas
    - Testar reset após sucesso
    - Testar expiração de lockout
    - _Requirements: 22.2, 22.5, 22.6, 32.4_
  
  - [ ]* 5.4 Escrever testes unitários para AuthService
    - Testar login com credenciais válidas/inválidas
    - Testar validação de token
    - Testar auto-logout por inatividade
    - _Requirements: 1.6, 1.7, 21.4, 26.2, 32.4_
  
  - [ ]* 5.5 Escrever testes PBT para autenticação
    - **Property 1: Valid Login Navigation**
    - **Property 2: Invalid Login Error Display**
    - **Property 13: Session Token Generation**
    - **Property 14: Token Expiration Logout**
    - **Property 15: Brute Force Lockout**
    - **Property 16: Failed Attempt Counter Reset**
    - **Validates: Requirements 1.6, 1.7, 21.1, 21.4, 22.2, 22.5**


- [x] 6. Criar layouts base (ShellLayout e MainLayout)
  - [x] 6.1 Implementar ShellLayout.razor
    - Criar estrutura com área de conteúdo e slot para BottomNavigation
    - Adicionar estilos CSS (.shell-layout, .shell-content)
    - _Requirements: 10.1, 10.2, 10.3, 10.5_
  
  - [x] 6.2 Implementar MainLayout.razor
    - Criar estrutura simples com área de conteúdo
    - Adicionar estilos CSS (.main-layout, .main-content)
    - _Requirements: 10.6_
  
  - [ ]* 6.3 Escrever testes para layouts
    - Testar renderização de conteúdo em ShellLayout
    - Testar renderização de conteúdo em MainLayout
    - _Requirements: 32.3_

- [x] 7. Configurar arquitetura CSS e tema
  - Criar wwwroot/css/theme.css com variáveis CSS (cores, espaçamento, tipografia, sombras, transições)
  - Criar wwwroot/css/responsive.css com breakpoints (320px, 768px, 1024px, 1440px)
  - Criar wwwroot/css/components.css (estilos base para componentes)
  - Criar wwwroot/css/app.css (estilos globais e reset)
  - Configurar sistema de cores (primary, success, warning, error)
  - Implementar sistema de espaçamento (8px grid)
  - Adicionar suporte para prefers-reduced-motion
  - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 12.1, 12.2, 19.7, 29.4, 34.1_


- [x] 8. Configurar PWA (manifest e service worker)
  - [x] 8.1 Criar wwwroot/manifest.json
    - Configurar nome, ícones, cores, orientação
    - Adicionar screenshots para instalação
    - _Requirements: 13.8_
  
  - [x] 8.2 Criar wwwroot/service-worker.js
    - Implementar estratégia de cache (App Shell: cache first, API: network first)
    - Adicionar suporte para background sync
    - Implementar atualização de cache em novas versões
    - _Requirements: 13.1, 13.2, 13.3, 13.7_
  
  - [x] 8.3 Registrar service worker no index.html
    - Adicionar script de registro
    - Implementar notificação de atualização disponível
    - _Requirements: 13.7_

- [ ] 9. Checkpoint - Verificar fundação
  - Garantir que todos os serviços core estão funcionando
  - Verificar que layouts renderizam corretamente
  - Confirmar que PWA está instalável
  - Executar testes unitários da Fase 1
  - Perguntar ao usuário se há dúvidas ou ajustes necessários


### FASE 2: Components (Semana 3-4)

- [x] 10. Implementar StatisticsCard component
  - [x] 10.1 Criar Components/Cards/StatisticsCard.razor
    - Adicionar parâmetros: Value (int), Label (string), BackgroundColor (string), TextColor (string)
    - Implementar renderização com classes CSS (.statistics-card, .statistics-card__value, .statistics-card__label)
    - Adicionar animação hover (translateY)
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 18.1, 33.1_
  
  - [ ]* 10.2 Escrever testes unitários para StatisticsCard
    - Testar renderização de valor e label
    - Testar aplicação de cores customizadas
    - Testar responsividade
    - _Requirements: 3.5, 18.4, 32.2, 32.4_
  
  - [ ]* 10.3 Escrever teste PBT para StatisticsCard
    - **Property 3: Component Parameter Rendering**
    - **Validates: Requirements 3.1, 3.2**
    - Para qualquer valor e label válidos, ambos devem aparecer no output renderizado

- [x] 11. Implementar ItemCard component
  - [x] 11.1 Criar Components/Cards/ItemCard.razor
    - Adicionar parâmetros: ItemName, ItemCode, LastUpdate, IsSynchronized, IconClass, OnClick
    - Implementar renderização com ícone, conteúdo e status
    - Adicionar feedback visual no tap (scale transform)
    - Implementar indicador de status (verde para sincronizado, cinza para pendente)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 18.2, 33.2_
  
  - [ ]* 11.2 Escrever testes unitários para ItemCard
    - Testar renderização de todos os campos
    - Testar evento OnClick
    - Testar indicador de status
    - _Requirements: 4.6, 18.4, 32.2, 32.4_
  
  - [ ]* 11.3 Escrever teste PBT para ItemCard
    - **Property 4: Item Card Click Navigation**
    - **Validates: Requirements 4.6**
    - Para qualquer item, clicar no card deve disparar OnClick com ID correto


- [x] 12. Implementar HierarchicalDropdown component
  - [x] 12.1 Criar Components/Forms/HierarchicalDropdown.razor
    - Adicionar parâmetros: Label, Placeholder, Options, SelectedValue, IsEnabled, IsLoading, OnValueChanged
    - Implementar renderização de label, select e loading indicator
    - Implementar lógica de habilitação/desabilitação baseada em parent
    - Adicionar estilos touch-friendly (min-height: 44px)
    - _Requirements: 6.1, 6.2, 6.3, 6.7, 6.8, 18.3, 33.3_
  
  - [ ]* 12.2 Escrever testes unitários para HierarchicalDropdown
    - Testar renderização de opções
    - Testar evento OnValueChanged
    - Testar estado disabled
    - Testar loading indicator
    - _Requirements: 6.6, 18.4, 32.2, 32.4_
  
  - [ ]* 12.3 Escrever teste PBT para cascata hierárquica
    - **Property 5: Hierarchical Dropdown Cascading**
    - **Validates: Requirements 5.4, 5.5, 5.6, 6.4, 6.5**
    - Para qualquer mudança no parent, child deve limpar, habilitar e carregar novas opções

- [x] 13. Implementar ConservationStateChip component
  - [x] 13.1 Criar Components/Forms/ConservationStateChip.razor
    - Adicionar parâmetros: Label, IsSelected, OnClick
    - Implementar renderização com estados selected/unselected
    - Adicionar estilos (background, border, cores) baseados em IsSelected
    - Adicionar feedback visual no tap (scale transform)
    - Garantir tap target mínimo de 44x44px
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 18.4, 33.4_
  
  - [ ]* 13.2 Escrever testes unitários para ConservationStateChip
    - Testar renderização de label
    - Testar evento OnClick
    - Testar estilos selected/unselected
    - _Requirements: 8.6, 18.4, 32.2, 32.4_
  
  - [ ]* 13.3 Escrever teste PBT para estado de seleção
    - **Property 6: Conservation Chip Selection State**
    - **Validates: Requirements 8.4, 8.5**
    - Para qualquer chip, estilos visuais devem refletir corretamente IsSelected


- [x] 14. Implementar BottomNavigation component
  - [x] 14.1 Criar Components/Navigation/BottomNavigation.razor
    - Adicionar parâmetros: ActiveRoute, OnNavigate
    - Implementar 5 itens de navegação (INÍCIO, histórico, câmera, sync, configurações)
    - Adicionar ícones e labels para cada item
    - Implementar highlight do item ativo (primary color)
    - Adicionar estilos fixed bottom com shadow
    - Garantir tap targets mínimos de 44x44px
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.8, 9.9, 18.5, 33.5_
  
  - [ ]* 14.2 Escrever testes unitários para BottomNavigation
    - Testar renderização de 5 itens
    - Testar highlight do item ativo
    - Testar evento OnNavigate
    - _Requirements: 9.2, 9.4, 9.6, 18.4, 32.2, 32.4_
  
  - [ ]* 14.3 Escrever testes de acessibilidade para BottomNavigation
    - Testar ARIA labels
    - Testar navegação por teclado
    - _Requirements: 32.6_

- [x] 15. Implementar componentes auxiliares
  - [x] 15.1 Criar Components/Shared/LoadingSpinner.razor
    - Implementar spinner animado com CSS
    - Adicionar parâmetro Size (small, medium, large)
    - _Requirements: 19.3_
  
  - [x] 15.2 Criar Components/Shared/Toast.razor
    - Implementar notificação toast com tipos (success, error, warning, info)
    - Adicionar animação slideUp
    - Implementar auto-dismiss após 5 segundos
    - _Requirements: 20.6, 20.7_
  
  - [x] 15.3 Criar ToastService para gerenciar toasts
    - Implementar métodos ShowSuccess, ShowError, ShowWarning, ShowInfo
    - Adicionar fila de toasts
    - _Requirements: 20.6_


- [ ] 16. Checkpoint - Verificar componentes
  - Garantir que todos os componentes renderizam corretamente
  - Verificar que parâmetros e eventos funcionam
  - Executar todos os testes de componentes
  - Validar cobertura de testes (meta: 80% para componentes)
  - Perguntar ao usuário se há ajustes necessários

### FASE 3: Pages (Semana 5-6)

- [x] 17. Implementar página de Login
  - [x] 17.1 Criar Pages/Login.razor
    - Implementar layout com logo/ícone centralizado
    - Adicionar título "Aspec Captura" e subtítulo "Inventário Patrimonial"
    - Criar campos de input para Usuário e Senha
    - Adicionar botão "Entrar" com cor primária
    - Exibir versão da aplicação no footer
    - Implementar validação de campos obrigatórios
    - Integrar com AuthService para login
    - Exibir mensagens de erro (credenciais inválidas, conta bloqueada)
    - Implementar navegação para Dashboard ou SessionConfig após login
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 20.1, 22.4_
  
  - [ ]* 17.2 Escrever testes unitários para Login
    - Testar renderização de elementos
    - Testar validação de campos
    - Testar integração com AuthService
    - Testar exibição de erros
    - _Requirements: 1.6, 1.7, 32.3, 32.4_
  
  - [ ]* 17.3 Escrever testes PBT para Login
    - **Property 1: Valid Login Navigation**
    - **Property 2: Invalid Login Error Display**
    - **Validates: Requirements 1.6, 1.7**


- [x] 18. Implementar página de Dashboard
  - [x] 18.1 Criar Pages/Dashboard.razor
    - Implementar header com saudação "Olá, [Nome]" e avatar
    - Adicionar ícone de notificações com badge
    - Adicionar link "ALTERAR" para mudar contexto organizacional
    - Renderizar 3 StatisticsCard (TOTAL, SINCR., PEND.) com cores apropriadas
    - Criar campo de busca com placeholder "Buscar patrimônio..."
    - Adicionar botão "Iniciar Captura" com ícone de câmera
    - Implementar seção "ITENS RECENTES" com link "VER TODOS"
    - Renderizar lista de ItemCard com itens recentes
    - Implementar navegação para ItemDetails ao clicar em card
    - Usar ShellLayout com BottomNavigation
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10, 2.11, 2.12, 2.13_
  
  - [x] 18.2 Implementar funcionalidade de busca no Dashboard
    - Adicionar debounce de 300ms na busca
    - Filtrar itens por código, descrição e localização
    - Implementar busca case-insensitive
    - Exibir "Nenhum item encontrado" quando sem resultados
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5, 15.6_
  
  - [x] 18.3 Implementar lazy loading de itens no Dashboard
    - Carregar inicialmente 20 itens mais recentes
    - Detectar scroll até o final da lista
    - Carregar próximos 20 itens automaticamente
    - Exibir loading indicator durante carregamento
    - Limitar a 200 itens, depois mostrar link "Ver todos"
    - Manter posição de scroll ao voltar de ItemDetails
    - _Requirements: 28.1, 28.2, 28.3, 28.4, 28.5, 28.6, 28.8_
  
  - [ ]* 18.4 Escrever testes unitários para Dashboard
    - Testar renderização de elementos
    - Testar navegação para ItemDetails
    - Testar atualização de estatísticas
    - _Requirements: 2.12, 32.3, 32.4_
  
  - [ ]* 18.5 Escrever testes PBT para Dashboard
    - **Property 9: Search Result Filtering**
    - **Property 10: Case-Insensitive Search**
    - **Property 12: Statistics Update After Sync**
    - **Property 23: Initial Load Pagination**
    - **Property 24: Lazy Loading Trigger**
    - **Validates: Requirements 15.1, 15.6, 16.7, 28.1, 28.2**


- [x] 19. Implementar página de Configuração de Sessão
  - [x] 19.1 Criar Pages/SessionConfig.razor
    - Implementar header com avatar, username e status ONLINE
    - Adicionar título "Configuração" com subtítulo descritivo
    - Renderizar 4 HierarchicalDropdown (ÓRGÃO, UNIDADE, ÁREA, SUBÁREA)
    - Implementar lógica de cascata (ÓRGÃO → UNIDADE → ÁREA → SUBÁREA)
    - Adicionar texto informativo sobre detecção automática de localização
    - Implementar botão "Iniciar Sessão" (habilitado apenas quando seleções completas)
    - Adicionar ícone de seta no botão
    - Implementar salvamento de configuração e navegação para Dashboard
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9, 5.10_
  
  - [ ]* 19.2 Escrever testes unitários para SessionConfig
    - Testar renderização de dropdowns
    - Testar lógica de cascata
    - Testar habilitação do botão "Iniciar Sessão"
    - Testar salvamento de configuração
    - _Requirements: 5.4, 5.5, 5.6, 5.8, 32.3, 32.4_
  
  - [ ]* 19.3 Escrever teste PBT para cascata hierárquica
    - **Property 5: Hierarchical Dropdown Cascading**
    - **Validates: Requirements 5.4, 5.5, 5.6, 6.4, 6.5**


- [x] 20. Implementar página de Detalhes do Patrimônio
  - [x] 20.1 Criar Pages/ItemDetails.razor
    - Implementar header com cor primária, botão voltar, título "Detalhes" e avatar
    - Exibir código do patrimônio no formato "PATRIMÔNIO #XXXX-XXXX-X"
    - Criar área de foto com ícone de câmera (trigger para captura)
    - Adicionar seção "DESCRIÇÃO" com texto do item
    - Criar campos "VALOR ESTIMADO" e "LOCALIZAÇÃO" lado a lado
    - Implementar seção "ESTADO DE CONSERVAÇÃO" com 4 ConservationStateChip (Novo, Bom, Regular, Recuperável)
    - Adicionar lógica de seleção exclusiva de chips
    - Criar campo "OBSERVAÇÕES" (textarea)
    - Adicionar botão "Salvar Patrimônio" com cor primária no bottom
    - Implementar validação e salvamento de dados
    - Usar ShellLayout com BottomNavigation
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8, 7.9, 7.10, 7.11, 7.12, 7.13, 7.14, 7.15_
  
  - [x] 20.2 Implementar validação de formulário em ItemDetails
    - Validar campos obrigatórios (código, descrição, localização, estado)
    - Validar formato de valor estimado (não negativo)
    - Exibir mensagens de erro inline
    - Prevenir salvamento com dados inválidos
    - _Requirements: 20.4, 20.5_
  
  - [ ]* 20.3 Escrever testes unitários para ItemDetails
    - Testar renderização de elementos
    - Testar seleção de chips de conservação
    - Testar validação de formulário
    - Testar salvamento de dados
    - _Requirements: 7.11, 7.14, 32.3, 32.4_
  
  - [ ]* 20.4 Escrever teste PBT para seleção de chips
    - **Property 6: Conservation Chip Selection State**
    - **Validates: Requirements 8.4, 8.5**


- [x] 21. Implementar página de Notificações
  - [x] 21.1 Criar Pages/Notifications.razor
    - Implementar header com título "Notificações"
    - Renderizar lista de notificações com paginação (20 por página)
    - Exibir tipo, título, mensagem, timestamp e prioridade
    - Destacar notificações não lidas
    - Implementar ação de marcar como lida
    - Adicionar botão "Marcar todas como lidas"
    - Agrupar notificações por tipo e data
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 35.10, 35.12_
  
  - [ ]* 21.2 Escrever testes unitários para Notifications
    - Testar renderização de lista
    - Testar marcação como lida
    - Testar paginação
    - _Requirements: 32.3, 32.4_

- [ ] 22. Checkpoint - Verificar páginas
  - Garantir que todas as páginas renderizam corretamente
  - Verificar navegação entre páginas
  - Testar fluxos completos (login → dashboard → item details)
  - Executar todos os testes de páginas
  - Validar cobertura de testes (meta: 70% para páginas)
  - Perguntar ao usuário se há ajustes necessários


### FASE 4: Advanced Features (Semana 7-8)

- [x] 23. Implementar CameraService e integração com câmera
  - [x] 23.1 Criar JavaScript Interop para câmera
    - Criar wwwroot/js/camera.js com função capture
    - Implementar acesso à MediaDevices API
    - Adicionar tratamento de permissões
    - Implementar captura de foto e conversão para byte array
    - _Requirements: 14.1, 14.2_
  
  - [x] 23.2 Implementar ICameraService e CameraService
    - Implementar CapturePhotoAsync (abre câmera e captura)
    - Implementar RequestPermissionAsync (solicita permissão)
    - Implementar GetPhotoDataAsync (recupera foto criptografada)
    - Adicionar verificação de storage disponível (mínimo 10MB)
    - Integrar com ImageCompressor para compressão automática
    - Integrar com CryptoService para criptografia
    - Armazenar foto em IndexedDB
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5, 14.6, 14.8, 20.3_
  
  - [ ]* 23.3 Escrever testes unitários para CameraService
    - Testar solicitação de permissão
    - Testar captura de foto
    - Testar tratamento de erros (permissão negada, câmera não encontrada)
    - _Requirements: 14.1, 20.3, 32.4_
  
  - [ ]* 23.4 Escrever teste PBT para armazenamento offline de fotos
    - **Property 8: Offline Photo Storage**
    - **Property 19: Photo Encryption Before Storage**
    - **Validates: Requirements 14.5, 24.1**


- [x] 24. Implementar ImageCompressor service
  - [x] 24.1 Criar IImageCompressor e ImageCompressor
    - Implementar CompressAsync com qualidade ajustável (Alta 90%, Média 85%, Baixa 70%)
    - Implementar ResizeAsync mantendo aspect ratio
    - Implementar GetCompressedSizeAsync para preview
    - Limitar tamanho máximo a 1MB
    - Limitar dimensões máximas a 1920x1920px
    - Ajustar qualidade baseado em storage disponível (< 100MB = 70%)
    - Usar formato JPEG
    - _Requirements: 31.1, 31.2, 31.3, 31.4, 31.5, 31.6, 31.7, 31.8_
  
  - [ ]* 24.2 Escrever testes unitários para ImageCompressor
    - Testar compressão para diferentes tamanhos
    - Testar preservação de aspect ratio
    - Testar limite de 1MB
    - _Requirements: 31.1, 31.4, 32.4_
  
  - [ ]* 24.3 Escrever testes PBT para compressão de imagens
    - **Property 7: Photo Compression Size Limit**
    - **Property 25: Aspect Ratio Preservation**
    - **Validates: Requirements 14.4, 31.1, 31.4**

- [ ] 25. Integrar captura de foto na página ItemDetails
  - Conectar área de foto ao CameraService
  - Implementar preview da foto capturada
  - Adicionar opção de substituir foto existente
  - Exibir loading durante captura e compressão
  - Tratar erros (permissão negada, storage cheio)
  - _Requirements: 7.6, 7.7, 14.7, 14.8, 20.3_


- [x] 26. Implementar SyncService e sincronização em lote
  - [x] 26.1 Criar ISyncService e SyncService
    - Implementar SyncAllAsync (sincroniza todos os itens pendentes)
    - Implementar SyncBatchAsync (sincroniza lote específico de até 50 itens)
    - Implementar QueueItemForSyncAsync (adiciona item à fila)
    - Implementar GetPendingCountAsync (conta itens pendentes)
    - Processar lotes sequencialmente (não paralelo)
    - Aguardar 2 segundos entre lotes
    - Implementar retry automático (3 tentativas por lote)
    - Descriptografar dados antes de enviar ao servidor
    - Validar HTTPS obrigatório
    - Atualizar status de sincronização após sucesso
    - Adicionar eventos OnSyncProgress e OnSyncCompleted
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6, 16.7, 25.1, 25.2, 27.1, 27.2, 27.3, 27.4, 27.5, 27.6, 27.7, 27.8_
  
  - [ ]* 26.2 Escrever testes unitários para SyncService
    - Testar criação de lotes (máximo 50 itens)
    - Testar processamento sequencial
    - Testar retry em falhas
    - Testar validação HTTPS
    - _Requirements: 16.3, 25.1, 27.1, 27.3, 27.5, 32.4_
  
  - [ ]* 26.3 Escrever testes PBT para sincronização
    - **Property 11: Sync Order Preservation**
    - **Property 20: HTTPS-Only Synchronization**
    - **Property 21: Sync Batch Size Limit**
    - **Property 22: Sequential Batch Processing**
    - **Validates: Requirements 16.3, 25.1, 27.1, 27.3**


- [x] 27. Implementar NotificationService e sistema de notificações
  - [x] 27.1 Criar JavaScript Interop para Web Push API
    - Criar wwwroot/js/push.js com funções de registro e recebimento
    - Implementar solicitação de permissão para push notifications
    - _Requirements: 35.8_
  
  - [x] 27.2 Implementar INotificationService e NotificationService
    - Implementar GetNotificationsAsync com paginação (20 por página)
    - Implementar GetUnreadCountAsync
    - Implementar MarkAsReadAsync (sincroniza com servidor em 5s)
    - Implementar MarkAllAsReadAsync
    - Implementar RegisterForPushAsync (Web Push API)
    - Implementar SyncNotificationsAsync (bidirecional)
    - Armazenar 50 notificações mais recentes localmente
    - Adicionar evento OnNotificationReceived
    - Implementar agrupamento por tipo e data
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6, 17.7, 35.1, 35.2, 35.3, 35.4, 35.5, 35.7, 35.8, 35.9, 35.10, 35.11, 35.12_
  
  - [ ]* 27.3 Escrever testes unitários para NotificationService
    - Testar paginação de notificações
    - Testar marcação como lida
    - Testar sincronização de status
    - Testar agrupamento
    - _Requirements: 17.5, 35.7, 35.10, 32.4_
  
  - [ ]* 27.4 Escrever testes PBT para notificações
    - **Property 26: Notification Broadcast**
    - **Property 27: Notification Read Status Sync**
    - **Validates: Requirements 35.3, 35.7**


- [ ] 28. Implementar funcionalidade de sincronização no Dashboard
  - Adicionar botão de sincronização no BottomNavigation
  - Implementar indicador de progresso durante sync
  - Exibir estatísticas após sync completo (X de Y sincronizados)
  - Atualizar contadores de StatisticsCard após sync
  - Desabilitar botão quando offline
  - Exibir mensagem de offline quando necessário
  - _Requirements: 16.1, 16.2, 16.7, 16.8_

- [ ] 29. Implementar testes de integração para fluxos críticos
  - [ ]* 29.1 Testar fluxo: Login → Dashboard → ItemDetails → Save
    - Validar autenticação completa
    - Validar navegação entre páginas
    - Validar salvamento de item
    - _Requirements: 32.5_
  
  - [ ]* 29.2 Testar fluxo: Offline Capture → Online Sync
    - Validar captura offline
    - Validar criptografia de dados
    - Validar sincronização quando online
    - _Requirements: 13.4, 13.5, 32.5_
  
  - [ ]* 29.3 Testar fluxo: SessionConfig → Dashboard
    - Validar seleção hierárquica
    - Validar criação de sessão
    - Validar navegação
    - _Requirements: 5.10, 32.5_
  
  - [ ]* 29.4 Testar fluxo: Photo Capture → Compression → Storage
    - Validar captura de foto
    - Validar compressão
    - Validar criptografia e armazenamento
    - _Requirements: 14.3, 14.4, 14.5, 32.5_

- [ ] 30. Checkpoint - Verificar features avançadas
  - Garantir que câmera funciona em dispositivos reais
  - Verificar que sincronização processa lotes corretamente
  - Testar notificações (local e push)
  - Executar todos os testes de integração
  - Perguntar ao usuário se há ajustes necessários


### FASE 5: Security & Performance (Semana 9-10)

- [x] 31. Implementar criptografia completa de dados offline
  - [x] 31.1 Integrar CryptoService no salvamento de itens
    - Criptografar campos sensíveis (description, location, observations) antes de salvar
    - Descriptografar ao carregar itens
    - Armazenar campos criptografados como base64
    - _Requirements: 23.3, 23.4_
  
  - [x] 31.2 Integrar CryptoService no armazenamento de fotos
    - Criptografar fotos antes de armazenar em IndexedDB
    - Descriptografar ao exibir fotos
    - Usar streaming encryption para fotos > 500KB
    - _Requirements: 24.1, 24.2, 24.3, 24.6, 24.7_
  
  - [x] 31.3 Implementar limpeza de chaves no logout
    - Limpar chaves de criptografia da memória
    - Limpar tokens de sessão
    - Limpar dados sensíveis do AppState
    - _Requirements: 23.7, 24.8_
  
  - [ ]* 31.4 Escrever teste PBT para criptografia de dados sensíveis
    - **Property 17: Sensitive Data Encryption**
    - **Validates: Requirements 23.3**


- [x] 32. Implementar proteções de segurança adicionais
  - [x] 32.1 Validar implementação de brute force protection
    - Verificar lockout após 5 tentativas
    - Verificar duração de 15 minutos
    - Verificar reset após login bem-sucedido
    - Verificar persistência em LocalStorage
    - _Requirements: 22.1, 22.2, 22.3, 22.5, 22.7, 22.8_
  
  - [x] 32.2 Validar implementação de auto-logout
    - Verificar rastreamento de atividade (taps, scrolls, keyboard)
    - Verificar logout após 30 minutos de inatividade
    - Verificar aviso 2 minutos antes do logout
    - Verificar reset do timer ao detectar atividade
    - Verificar persistência do timer entre navegações
    - _Requirements: 26.1, 26.2, 26.3, 26.4, 26.5, 26.6, 26.7, 26.8_
  
  - [x] 32.3 Validar implementação de refresh de token
    - Verificar refresh automático 30 min antes da expiração
    - Verificar que refresh ocorre apenas com usuário ativo
    - _Requirements: 21.6_
  
  - [x] 32.4 Validar segurança na sincronização
    - Verificar que sync só ocorre via HTTPS
    - Verificar validação de certificados SSL/TLS
    - Verificar inclusão de token em headers
    - Verificar abort em caso de certificado inválido
    - _Requirements: 25.1, 25.2, 25.3, 25.4, 25.5, 25.6, 25.7, 25.8_


- [x] 33. Otimizar performance da aplicação
  - [x] 33.1 Implementar detecção de performance do dispositivo
    - Detectar memória disponível (< 2GB = low memory)
    - Detectar CPU (benchmark score)
    - Ajustar animações baseado em performance
    - Respeitar preferência prefers-reduced-motion
    - _Requirements: 29.1, 29.2, 29.3, 29.4, 29.5, 29.6, 29.7, 29.8_
  
  - [x] 33.2 Otimizar carregamento inicial
    - Habilitar Blazor WebAssembly AOT compilation
    - Habilitar IL trimming
    - Habilitar compressão Brotli
    - Minificar CSS e JS
    - Limitar bundle principal a 500KB (gzipped)
    - _Requirements: 30.7_
  
  - [x] 33.3 Otimizar renderização de componentes
    - Adicionar ShouldRender override em componentes pesados
    - Usar virtualization para listas longas
    - Implementar debounce em buscas (300ms)
    - Limitar animações concorrentes a 3 elementos
    - _Requirements: 15.3, 29.8, 30.3_
  
  - [x] 33.4 Otimizar cache do service worker
    - Implementar cache first para assets estáticos
    - Implementar network first para API calls
    - Adicionar timeout de 5 segundos para requests
    - Limpar caches antigos em atualizações
    - _Requirements: 13.2, 13.3_


- [x] 34. Implementar métricas de performance
  - [x] 34.1 Adicionar logging de métricas
    - Medir e logar Initial Page Load Time
    - Medir e logar Time to Interactive (TTI)
    - Medir e logar First Contentful Paint (FCP)
    - Medir e logar tempo de resposta a interações
    - Medir e logar tempo de transições de página
    - Medir e logar frame rate durante animações
    - _Requirements: 30.1, 30.2, 30.3, 30.4, 30.5, 30.6, 30.8_
  
  - [ ]* 34.2 Escrever testes de performance
    - Testar que Dashboard carrega em < 3s em 3G
    - Testar que TTI é < 5s em 3G
    - Testar que resposta a tap é < 100ms
    - Testar que transições são < 300ms
    - Testar que FCP é < 1.5s
    - _Requirements: 30.1, 30.2, 30.3, 30.4, 30.5_

- [x] 35. Implementar responsividade para desktop
  - Adicionar breakpoint 1024px: sidebar navigation vertical
  - Adicionar breakpoint 1440px: grid 3 colunas para estatísticas
  - Adicionar breakpoint 1440px: grid 2 colunas para lista de itens
  - Centralizar conteúdo com max-width 1200px
  - Transformar BottomNavigation em sidebar em desktop
  - Ajustar header do Dashboard para layout horizontal
  - Manter tap targets mínimos de 44x44px mesmo em desktop
  - _Requirements: 34.1, 34.2, 34.3, 34.4, 34.5, 34.6, 34.7, 34.8_

- [ ] 36. Checkpoint - Verificar segurança e performance
  - Validar que todas as proteções de segurança estão ativas
  - Verificar métricas de performance (< 3s load, < 5s TTI)
  - Testar em dispositivos de baixa performance
  - Testar responsividade em diferentes tamanhos de tela
  - Executar testes de performance
  - Perguntar ao usuário se há ajustes necessários


### FASE 6: Testing & Polish (Semana 11-12)

- [ ] 37. Atingir metas de cobertura de testes
  - [ ] 37.1 Revisar cobertura de componentes (meta: 80%)
    - Executar relatório de cobertura
    - Identificar componentes com cobertura < 80%
    - Adicionar testes faltantes
    - Validar que meta foi atingida
    - _Requirements: 32.2_
  
  - [ ] 37.2 Revisar cobertura de páginas (meta: 70%)
    - Executar relatório de cobertura
    - Identificar páginas com cobertura < 70%
    - Adicionar testes faltantes
    - Validar que meta foi atingida
    - _Requirements: 32.3_
  
  - [ ] 37.3 Revisar cobertura de serviços (meta: 90%)
    - Executar relatório de cobertura
    - Identificar serviços com cobertura < 90%
    - Adicionar testes faltantes
    - Validar que meta foi atingida
    - _Requirements: 32.4_


- [ ] 38. Implementar todos os testes baseados em propriedades (PBT)
  - [ ]* 38.1 Validar Property 1: Valid Login Navigation
    - Para qualquer credencial válida, deve navegar para Dashboard
    - _Requirements: 1.6_
  
  - [ ]* 38.2 Validar Property 2: Invalid Login Error Display
    - Para qualquer credencial inválida, deve exibir erro
    - _Requirements: 1.7_
  
  - [ ]* 38.3 Validar Property 3: Component Parameter Rendering
    - Para qualquer parâmetro válido, deve aparecer no output
    - _Requirements: 3.1, 3.2_
  
  - [ ]* 38.4 Validar Property 4: Item Card Click Navigation
    - Para qualquer item, clicar deve disparar OnClick correto
    - _Requirements: 4.6_
  
  - [ ]* 38.5 Validar Property 5: Hierarchical Dropdown Cascading
    - Para qualquer mudança no parent, child deve atualizar
    - _Requirements: 5.4, 5.5, 5.6, 6.4, 6.5_
  
  - [ ]* 38.6 Validar Property 6: Conservation Chip Selection State
    - Para qualquer chip, estilos devem refletir IsSelected
    - _Requirements: 8.4, 8.5_
  
  - [ ]* 38.7 Validar Property 7: Photo Compression Size Limit
    - Para qualquer foto, tamanho após compressão ≤ 1MB
    - _Requirements: 14.4, 31.1_
  
  - [ ]* 38.8 Validar Property 8: Offline Photo Storage
    - Para qualquer foto offline, deve armazenar e enfileirar
    - _Requirements: 14.5_
  
  - [ ]* 38.9 Validar Property 9: Search Result Filtering
    - Para qualquer busca, resultados devem conter termo
    - _Requirements: 15.1_
  
  - [ ]* 38.10 Validar Property 10: Case-Insensitive Search
    - Para qualquer termo, upper/lower devem retornar mesmo resultado
    - _Requirements: 15.6_


  - [ ]* 38.11 Validar Property 11: Sync Order Preservation
    - Para qualquer conjunto de itens, ordem de criação deve ser preservada
    - _Requirements: 16.3_
  
  - [ ]* 38.12 Validar Property 12: Statistics Update After Sync
    - Para qualquer sync bem-sucedido, estatísticas devem atualizar
    - _Requirements: 16.7_
  
  - [ ]* 38.13 Validar Property 13: Session Token Generation
    - Para qualquer autenticação, token deve ter expiração de 8h
    - _Requirements: 21.1_
  
  - [ ]* 38.14 Validar Property 14: Token Expiration Logout
    - Para qualquer token expirado, deve fazer logout automático
    - _Requirements: 21.4_
  
  - [ ]* 38.15 Validar Property 15: Brute Force Lockout
    - Para qualquer username, 5 falhas devem bloquear por 15 min
    - _Requirements: 22.2_
  
  - [ ]* 38.16 Validar Property 16: Failed Attempt Counter Reset
    - Para qualquer username, sucesso deve resetar contador
    - _Requirements: 22.5_
  
  - [ ]* 38.17 Validar Property 17: Sensitive Data Encryption
    - Para qualquer item, campos sensíveis devem ser criptografados
    - _Requirements: 23.3_
  
  - [ ]* 38.18 Validar Property 18: Data Encryption Round-Trip
    - Para qualquer dado, encrypt → decrypt deve retornar original
    - _Requirements: 23.4, 24.3_
  
  - [ ]* 38.19 Validar Property 19: Photo Encryption Before Storage
    - Para qualquer foto, deve ser criptografada antes de armazenar
    - _Requirements: 24.1_
  
  - [ ]* 38.20 Validar Property 20: HTTPS-Only Synchronization
    - Para qualquer sync, deve exigir HTTPS
    - _Requirements: 25.1_


  - [ ]* 38.21 Validar Property 21: Sync Batch Size Limit
    - Para qualquer número de itens, lotes devem ter ≤ 50 itens
    - _Requirements: 27.1_
  
  - [ ]* 38.22 Validar Property 22: Sequential Batch Processing
    - Para qualquer conjunto de lotes, processamento deve ser sequencial
    - _Requirements: 27.3_
  
  - [ ]* 38.23 Validar Property 23: Initial Load Pagination
    - Para qualquer carregamento, deve exibir exatamente 20 itens iniciais
    - _Requirements: 28.1_
  
  - [ ]* 38.24 Validar Property 24: Lazy Loading Trigger
    - Para qualquer scroll até o fim, deve carregar próximos 20 itens
    - _Requirements: 28.2_
  
  - [ ]* 38.25 Validar Property 25: Aspect Ratio Preservation
    - Para qualquer foto, aspect ratio deve ser preservado (±1%)
    - _Requirements: 31.4_
  
  - [ ]* 38.26 Validar Property 26: Notification Broadcast
    - Para qualquer notificação nova, todas as sessões devem receber
    - _Requirements: 35.3_
  
  - [ ]* 38.27 Validar Property 27: Notification Read Status Sync
    - Para qualquer notificação lida, status deve sincronizar em ≤ 5s
    - _Requirements: 35.7_


- [ ] 39. Realizar testes de acessibilidade
  - [ ]* 39.1 Testar hierarquia de headings
    - Verificar que cada página tem exatamente um h1
    - Verificar que headings seguem ordem lógica (h1 → h2 → h3)
    - _Requirements: 32.6_
  
  - [ ]* 39.2 Testar textos alternativos
    - Verificar que todas as imagens têm alt text
    - Verificar que ícones decorativos têm aria-hidden
    - _Requirements: 32.6_
  
  - [ ]* 39.3 Testar labels de elementos interativos
    - Verificar que todos os botões têm aria-label ou texto visível
    - Verificar que todos os inputs têm labels associados
    - Verificar que links têm texto descritivo
    - _Requirements: 32.6_
  
  - [ ]* 39.4 Testar contraste de cores
    - Verificar contraste mínimo WCAG AA (4.5:1 para texto normal)
    - Verificar contraste para texto grande (3:1)
    - Documentar áreas que requerem verificação manual
    - _Requirements: 11.7_
  
  - [ ]* 39.5 Testar navegação por teclado
    - Verificar que todos os elementos interativos são acessíveis via Tab
    - Verificar ordem lógica de foco
    - Verificar que foco é visível
    - _Requirements: 32.6_
  
  - [ ]* 39.6 Testar com leitor de tela (manual)
    - Documentar teste manual com NVDA ou JAWS
    - Verificar que conteúdo é anunciado corretamente
    - Verificar que estados (loading, error) são anunciados
    - _Requirements: 32.6_


- [x] 40. Refinar UX e corrigir bugs
  - [x] 40.1 Revisar transições e animações
    - Verificar que todas as transições são suaves (≤ 400ms)
    - Verificar que animações respeitam prefers-reduced-motion
    - Ajustar timing e easing conforme necessário
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6, 19.7_
  
  - [x] 40.2 Revisar tratamento de erros
    - Verificar que todas as mensagens de erro são claras e acionáveis
    - Verificar que erros críticos exigem acknowledgment
    - Verificar que erros não críticos auto-dismiss em 5s
    - Verificar que ações "Tentar Novamente" funcionam
    - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.5, 20.6, 20.7, 20.8_
  
  - [x] 40.3 Revisar feedback visual
    - Verificar que todos os botões têm feedback no tap
    - Verificar que cards têm ripple ou highlight
    - Verificar que loading indicators aparecem quando apropriado
    - _Requirements: 4.7, 8.6, 19.2_
  
  - [x] 40.4 Testar em dispositivos reais
    - Testar em Android (diferentes versões)
    - Testar em iOS (diferentes versões)
    - Testar em tablets
    - Testar em desktop (Chrome, Firefox, Safari, Edge)
    - Documentar bugs encontrados
  
  - [x] 40.5 Corrigir bugs identificados
    - Priorizar bugs críticos (bloqueiam funcionalidade)
    - Corrigir bugs de alta prioridade (UX ruim)
    - Documentar bugs de baixa prioridade para backlog


- [x] 41. Preparar para deployment
  - [x] 41.1 Configurar build de produção
    - Habilitar AOT compilation
    - Habilitar IL trimming
    - Habilitar compressão Brotli
    - Configurar minificação de CSS e JS
    - Validar que bundle principal ≤ 500KB (gzipped)
    - _Requirements: 30.7_
  
  - [x] 41.2 Configurar HTTPS e segurança
    - Configurar redirect HTTP → HTTPS
    - Adicionar HSTS headers
    - Configurar validação de certificados SSL/TLS
    - _Requirements: 25.1, 25.2, 25.3_
  
  - [x] 41.3 Configurar atualização de service worker
    - Implementar notificação de nova versão disponível
    - Implementar limpeza de caches antigos
    - Implementar prompt para reload
    - _Requirements: 13.7_
  
  - [x] 41.4 Configurar monitoramento e analytics
    - Configurar logging de erros client-side para servidor
    - Configurar envio de métricas de performance
    - Configurar tracking de ações do usuário
    - _Requirements: 30.8_
  
  - [x] 41.5 Criar documentação de deployment
    - Documentar processo de build
    - Documentar requisitos de infraestrutura (HTTPS obrigatório)
    - Documentar processo de atualização
    - Documentar monitoramento e métricas


- [ ] 42. Validação final e entrega
  - [ ] 42.1 Executar suite completa de testes
    - Executar todos os testes unitários
    - Executar todos os testes de integração
    - Executar todos os testes PBT (100 iterações cada)
    - Validar que todos os testes passam
    - _Requirements: 32.7_
  
  - [ ] 42.2 Gerar relatórios de cobertura
    - Gerar relatório HTML para revisão
    - Gerar relatório XML para CI/CD
    - Validar cobertura: 80% componentes, 70% páginas, 90% serviços
    - _Requirements: 32.2, 32.3, 32.8_
  
  - [ ] 42.3 Validar métricas de performance
    - Validar Initial Load Time < 3s em 3G
    - Validar TTI < 5s em 3G
    - Validar resposta a interações < 100ms
    - Validar transições < 300ms
    - Validar FCP < 1.5s
    - Validar 60fps durante animações
    - _Requirements: 30.1, 30.2, 30.3, 30.4, 30.5, 30.6_
  
  - [ ] 42.4 Validar PWA installability
    - Verificar que manifest está correto
    - Verificar que service worker está registrado
    - Verificar que app é instalável em Android
    - Verificar que app é instalável em iOS
    - _Requirements: 13.8_
  
  - [ ] 42.5 Criar documentação final
    - Documentar arquitetura implementada
    - Documentar componentes e suas APIs
    - Documentar serviços e suas interfaces
    - Documentar fluxos de usuário
    - Documentar decisões técnicas importantes
    - _Requirements: 18.9, 33.1, 33.2, 33.3, 33.4, 33.5, 33.6, 33.7, 33.8_

- [ ] 43. Checkpoint final - Entrega
  - Revisar todos os requisitos (35 requisitos implementados)
  - Revisar todas as propriedades (27 propriedades validadas)
  - Confirmar que todas as metas de qualidade foram atingidas
  - Apresentar aplicação ao usuário para aprovação final
  - Coletar feedback e documentar melhorias futuras


## Notes

- Tasks marcadas com `*` são opcionais (testes) e podem ser puladas para MVP mais rápido
- Cada task referencia requisitos específicos para rastreabilidade completa
- Checkpoints garantem validação incremental e oportunidade para feedback
- Testes PBT validam propriedades universais com 100 iterações mínimas
- Metas de cobertura: 80% componentes, 70% páginas, 90% serviços
- Metas de performance: < 3s load time, < 5s TTI, < 100ms resposta, < 300ms transições

## Summary

**Total de Tasks:** 43 tasks principais
**Total de Sub-tasks:** 127 sub-tasks
**Tasks Opcionais (testes):** 67 sub-tasks marcadas com `*`
**Duração Estimada:** 12 semanas (6 fases de 2 semanas cada)

**Distribuição por Fase:**
- Fase 1 (Foundation): 9 tasks
- Fase 2 (Components): 7 tasks
- Fase 3 (Pages): 6 tasks
- Fase 4 (Advanced Features): 7 tasks
- Fase 5 (Security & Performance): 6 tasks
- Fase 6 (Testing & Polish): 8 tasks

**Cobertura de Requisitos:**
- 35 requisitos cobertos
- 27 propriedades de correção implementadas
- 100% de rastreabilidade entre tasks e requisitos

**Próximos Passos:**
1. Revisar este plano com a equipe
2. Ajustar estimativas se necessário
3. Iniciar Fase 1: Foundation
4. Executar checkpoints ao final de cada fase
