# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [0.2.2] - 2026-03-04

### Corrigido
- **Bug de Perda de Dados na Configuração de Sessão**: Corrigido problema crítico onde os campos da tela de Configuração de Sessão ficavam vazios após logout e novo login.
  - Adicionado método `ClearSessionData()` no `AppState` para limpar dados da sessão anterior
  - `LoginAsync` agora limpa o estado anterior e define `EsferaAtual` corretamente
  - `LogoutAsync` agora limpa completamente o `AppState` e localStorage
  - `ConfiguracaoSessao.razor` agora inicializa `appState.EsferaAtual` se necessário
  - Injetado `AppState` no `AuthService` para gerenciar estado global
  - Criado documento `TROUBLESHOOTING_SESSION_CONFIG.md` com análise detalhada do bug

### Documentação
- Adicionado guia completo de troubleshooting para problemas de configuração de sessão
- Documentados testes de validação para fluxos de login/logout
- Adicionadas sugestões de melhorias futuras (persistência de configuração, validação de dados)

## [0.2.1] - 2026-02-18

### Alterado
- Atualização de modelos, serviços de autenticação e storage
- Melhorias de integração e persistência
- Suporte aprimorado ao IndexedDB

## [0.2.0] - 2026-02-18

### Adicionado
- **Refatoração Hierárquica**: Implementado novo modelo `Usuario` com suporte a `Órgão > UO > Área > Subárea`.
- **Configuração de Sessão**: Criada nova página `/configuracao-sessao` com dropdowns em cascata para seleção do contexto de trabalho após o login.
- **S3 Folder Logic**: O sistema agora organiza assets no S3 seguindo a estrutura `{Prefixo}/{IdUO}/`.
- **Captura Inteligente**: Itens capturados via câmera herdam automaticamente a hierarquia da sessão ativa.
- **IndexedDB v4**: Esquema atualizado para incluir campos hierárquicos e índice por `idUO` para performance.

### Alterado
- **Nomenclatura**: Migração de termos em inglês (`User`, `UnitId`) para português (`Usuario`, `IdUO`) em toda a base de código.
- **AppState**: Centralizada a gestão da sessão hierárquica e notificação de mudanças de estado.

### Corrigido
- **Startup Errors**: Resolvidos crashes causados por placeholders `__API_BASE_URL__` e conflitos de porta.
- **Legacy Cleanup**: Removidas todas as referências ao antigo modelo de `UnitId` (inteiro).

## [0.1.14] - 2026-02-18

#### 🔐 Remoção do Módulo de Cadastro (Client-Side) (v0.1.14)
- **Acesso Provisionado**: Removida completamente a funcionalidade de criação de novos usuários no PWA. O sistema agora opera exclusivamente sob um modelo de "Acesso Provisionado".
- **Limpeza de UI**:
  - Excluída a página `Register.razor`.
  - Removido o link "Cadastre-se" da tela de Login.
- **Refatoração de Autenticação**:
  - `AuthService` agora aponta exclusivamente para a API (`/api/auth/login`).
  - Removida lógica de validação local e usuário "admin" fixo.
  - O armazenamento local (IndexedDB/localStorage) é utilizado apenas para cache dos dados do usuário após autenticação bem-sucedida via API.
- **Dependências**:
  - Removidos modelos e métodos relacionados ao cadastro de usuários no front-end.


### Corrigido - 2026-01-26

#### 🐛 Correções de Menu e Layout (v0.1.10)
- **Menu Lateral (Drawer)**:
  - Resolvido conflito de z-index onde o rodapé bloqueava o menu em dispositivos móveis.
  - Forçado z-index do Drawer para 1300 e reduzido Footer para 100.
  - Removida regra CSS que bloqueava interações com overlays.
- **Interatividade**:
  - Corrigida ordem de eventos nos botões "Sair" e "Alternar Tema" para garantir execução antes do fechamento do menu.
  - Atualizada versão na tela de Login para `v0.1.10`.

### Adicionado - 2026-01-16

#### 🔄 Integração com API BFF (Backend for Frontend)
- **Refatoração de Upload S3**:
  - Implementada comunicação com API BFF (`pwa-camera-poc-api`) para geração de Pre-Signed URLs.
  - Substituído uso direto do AWS SDK (que causava erros em WASM) por chamadas HTTP padrão.
  - Fluxo seguro: Credenciais AWS agora residem apenas no servidor (API), não mais no cliente.
  
- **Limpeza de Código**:
  - Removidos pacotes NuGet do AWS SDK (`AWSSDK.S3`, `AWSSDK.CognitoIdentity`, etc.) do projeto Blazor.
  - Removida lógica de autenticação Cognito legada/comentada do `AuthService`.
  - Simplificado modelo `AwsConfig` para conter apenas configurações públicas.

### Análise Técnica & Roadmap de Correção - 2026-01-09

> Consulte `docs/ANALYSIS_REPORT.md` para o relatório completo.

#### 🔴 Crítico (Bloqueios)
- **Correção de Login**: Ajustar `Login.razor` e `LoginModel` para aceitar username simples (`admin`) OU alterar usuário padrão para formato de email (`admin@aspec.com`).
- **Fix Sincronização (S3)**: O `AmazonS3Client` falha em WASM.
  - **Ação Necessária**: Pivotar arquitetura de upload para uso de **Pre-Signed URLs** ou Proxy API.
  - **Meta**: Remover dependência direta do AWS SDK para transferência de dados no cliente.

### Adicionado - 2026-01-08

#### 🔄 Sincronização S3 com Padrão Desktop

- **Fluxo de Sincronização Unidirecional (PWA → S3)**:
  - Implementado upload de itens do IndexedDB para AWS S3 seguindo padrão de nomenclatura compatível com módulo Desktop
  - Estrutura de arquivos: `capturas/{itemId}.jpg` e `capturas/{itemId}.json`
  - Upload apenas da primeira foto (capa) de cada item
  - Metadados em formato JSON compatível com Desktop, incluindo campo obrigatório `usuarioEnvio`

- **Modelo de Metadados (`ItemMetadata.cs`)**:
  - Criado modelo específico para sincronização com campos em português
  - Campo obrigatório `UsuarioEnvio`: username do fiscal que realizou o envio
  - Campo `DataEnvio`: timestamp do momento da sincronização
  - Isolamento de dados por usuário via atributo `UsuarioEnvio` no JSON

- **Filtro de Visualização por Usuário**:
  - Adicionada propriedade `CreatedBy` ao modelo `InventoryItem`
  - Home e Sync exibem apenas itens criados pelo usuário logado
  - Garantia de privacidade: cada fiscal visualiza apenas seus próprios registros

- **Indicadores Visuais de Status**:
  - Ícone de nuvem cinza: Item pendente de sincronização (apenas local)
  - Ícone de nuvem verde: Item confirmado no S3
  - Verificação rápida de existência no S3 via método `ItemExistsInS3Async`

- **Segurança e Configuração**:
  - Removidas credenciais AWS do código-fonte
  - Placeholders no `appsettings.json` para injeção via variáveis de ambiente
  - Preparado para deploy no Cloudflare Pages com secrets gerenciados

#### 📝 Documentação de Integração

- **Padrão de Integração Desktop**:
  - Documentado que o isolamento de dados no bucket é feito via `UsuarioEnvio` no JSON
  - Fluxo de dados: Captura Local → IndexedDB → Sync S3 (JSON+JPG) → Consumo Desktop
  - Sistema não permite edição de itens já sincronizados (upload-only)

### Adicionado - 2026-01-07

#### 📊 Gestão de Dados e Exportação
- **Exportação para CSV**: Adicionado recurso de exportação completa do inventário local para arquivo CSV via menu inferior.
- **Sincronização Progressiva**: Melhoria na tela de sincronização com indicadores visuais de progresso e feedback em tempo real.
- **Dados de Exemplo**: Implementada carga automática de item de exemplo (`exemplo-item.json`) quando o inventário está vazio para auxiliar novos usuários.

- **Sincronização S3 (Modo PoC)**: Implementada integração direta com S3 via credenciais estáticas para validação de fluxo técnica.
- **Autenticação Híbrida**: Postergada integração com AWS Cognito devido a restrições de acesso administrativo; o sistema utiliza validação local com bypass para serviços AWS.
- **Gestão de Armazenamento**: Fluxo automático que substitui dados Base64 locais por URLs da AWS após sincronização bem-sucedida, otimizando o armazenamento local.
- **Gestão Remota**: Implementada exclusão de fotos diretamente no S3 através da tela de detalhes do item.
- **Limpeza de Cache**: Otimização automática do banco IndexedDB removendo mídias já sincronizadas.

#### 🚀 Deploy e Infraestrutura
- **Resolução de Impedimento**: Migração para **Cloudflare Pages**, resolvendo limitações de deploy anteriores e garantindo suporte a SPA routing.

#### 📸 Câmera e Inventário
- **Suporte Multi-Foto**: Agora é possível capturar e associar múltiplas fotografias a um único item de inventário.
- **Gestão Pós-Captura**: Adicionada funcionalidade de adicionar novas fotos ou remover existentes diretamente da tela de detalhes do item.
- **Visualização Full-Screen**: Implementada sobreposição (overlay) para visualização de fotos em tamanho real com zoom e fechar.
- **Galeria de Visualização**: Implementada navegação entre fotos capturadas antes do salvamento com opção de remoção individual.
- **Resiliência da Câmera**: Tratamento de erros aprimorado para dispositivos iOS/Safari e mensagens de erro amigáveis para permissões negadas ou dispositivos ocupados.

#### ⚡ UI/UX e Navegação
- **Menu de Navegação Inferior (Bottom Bar)**: Implementada barra de navegação principal para acesso rápido a Home, Estatísticas, Câmera e Sincronização, otimizada para uso com uma mão.
- **Paginação e Ordenação**: Tela inicial agora conta com paginação robusta e diversas opções de ordenação (por data, nome e código).
- **Badge de Sincronização**: Adicionado contador visual no menu inferior indicando a quantidade de itens pendentes de sincronização.

### Corrigido - 2026-01-07
- **Filtros de Categoria**: Corrigida a lógica de filtragem que ocasionalmente falhava ao alternar rapidamente entre categorias.
- **Estado Global**: Sincronização do modo escuro persistida corretamente no primeiro carregamento via script inline no `index.html`.

## [0.2.0] - 2025-12-30

### Adicionado - 2025-12-30

#### 🎨 Refinamento de UI/UX e Identidade Visual
- **Identidade Visual ASPEC**: 
  - Logotipo oficial (`aspec_logo.png`) implementado como Favicon e ícone PWA.
  - Configuração de ícones **Maskable** para suporte a ícones adaptativos no Android.
  - Cores corporativas sincronizadas em toda a aplicação (Azul ASPEC #003366).
- **Melhorias de Usabilidade**:
  - Aumento da altura dos campos de entrada (`MudTextField`) em Login, Cadastro e Perfil para melhores alvos de toque em dispositivos móveis.
  - Navegação fluida: O menu lateral agora fecha automaticamente ao navegar para o perfil via clique no avatar.
  - Exibição de nomes de unidades reais em vez de IDs (ex: "Prefeitura de São Luís" em vez de "6").
- **Melhorias no Tema Escuro**:
  - Ajuste de contraste para textos primários e secundários.
  - Correção visual nos campos "Outlined" para que o fundo do label (notch) acompanhe a cor da superfície do tema.
  - Sincronização de ícones de alternância de tema entre o menu lateral e a tela de login.

#### 🔧 Melhorias Técnicas e Estabilidade
- **AuthService**:
  - Implementado `UpdateUserAsync` para atualização segura de perfis de usuários.
  - Adicionada deduplicação automática de IDs de unidades (`Distinct()`).
- **Resiliência de Dados**:
  - Implementação de seeding idempotente para Estados, Cidades e Unidades, prevenindo duplicação de dados ao recarregar a aplicação.
  - Sincronização robusta de estado entre layouts e páginas via `AppState`.
- **Performance e Layout**:
  - Implementado detector automático de overflow em páginas críticas (`Stats`, `Home`, `Sync`, `Profile`).
  - Tratamento de erros e segurança em chamadas de Interop JavaScript.
  - Resolução de todos os conflitos de merge pendentes no repositório.

### Corrigido - 2025-12-30
- **Build**: Resolvido aviso `MUD0002` (atributo `Hover` ilegal em `MudCard`).
- **Lógica de Unidades**: Corrigido problema onde IDs apareciam na lista de unidades antes do carregamento completo dos nomes.

### Adicionado - 2025-12-29

### Adicionado - 2025-12-29

#### 🎨 Melhorias de UI/UX e Responsividade

- **MudBlazor 7.20.0**: Biblioteca de componentes Material Design gratuita e open-source
  - Componentes modernos e responsivos
  - Suporte completo para mobile-first design
  - Ícones Material Design integrados
  - Temas customizáveis

- **Refinamento de Tema (ASPEC Identity)**:
  - Configuração centralizada em `MainLayout.razor` via `MudThemeProvider`.
  - Paleta de cores corporativa: Azul (#0066CC) e Azul Escuro (#003366).
  - Tipografia ajustada: Roboto (400, 600, 700).
  - **Bordas**: DefaultBorderRadius ajustado para **6px** (suave e moderno).
  - **Placeholder**: Imagem padrão (`placeholder.svg`) implementada para itens sem fotos.
  - Removidos estilos globais manuais (`app.css` limpado) em favor de estilos nativos do MudBlazor.

- **Componentes Refatorados**:
  - `Home.razor`: 
    - Grid/Lista responsivo funcional (Alternância verificada).
    - Botões de filtro e busca com ações conectadas.
  - `ItemDetails.razor`: Layout modernizado com `MudContainer` e `MudCard`.
  - `Login.razor` / `Register.razor`: Formulários convertidos inteiramente para componentes MudBlazor com `Variant.Outlined`.
  - `ItemCard.razor`: Lógica de fallback de imagem aprimorada.

### Corrigido - 2025-12-29

#### 🐛 Correções de Acessibilidade e Layout

- **Acessibilidade**:
  - Adicionados IDs explícitos e Labels em Inputs (`Home.razor`) para resolver avisos de auditoria.
  - Melhorado contraste e semântica dos botões.

- **Cleanup de Código**:
  - Removido `app.css` legado (~1600 linhas) para garantir consistência e leveza.
  - Eliminadas classes CSS órfãs (`.auth-card`, `.gallery-grid`).

### Alterado - 2025-12-29

#### 🔄 Atualizações de Configuração

- **Program.cs**:
  - Adicionado `using MudBlazor.Services`
  - Configurado `builder.Services.AddMudServices()`
- **UI Architecture**: Migração completa para **MudBlazor**.
  - `MainLayout` agora gerencia o tema globalmente.
  - `app.css` contém apenas overrides essenciais (video feed, scrollbars).
  - Sistema de validação integrado aos componentes MudBlazor.

## Bibliotecas e Dependências

### UI Component Libraries

| Biblioteca | Versão | Licença | Propósito |
|-----------|--------|---------|-----------|
| **MudBlazor** | 7.20.0 | MIT | Material Design components, grids, cards, modals |
| **Microsoft Fluent UI Blazor** | 3.8.0 | MIT | Microsoft design system components |

### Fontes e Ícones

| Recurso | Fonte | Licença |
|---------|-------|---------|
| **Roboto** | Google Fonts | Apache 2.0 |
| **Material Icons** | Google Fonts | Apache 2.0 |

### Próximos Passos

- [ ] Migrar componentes existentes para MudBlazor
- [ ] Implementar MudThemeProvider para temas dinâmicos
- [ ] Adicionar MudSnackbar para notificações
- [ ] Criar componentes reutilizáveis com MudCard, MudPaper
- [ ] Implementar MudDataGrid para listagens
- [ ] Adicionar MudDialog para modais
- [ ] Configurar breakpoints responsivos customizados

---

## Notas de Versão

### Por que MudBlazor?

1. **Gratuito e Open-Source**: Licença MIT, sem custos
2. **Material Design**: Seguindo guidelines do Google
3. **Mobile-First**: Responsividade nativa
4. **Bem Documentado**: Documentação extensa e exemplos
5. **Comunidade Ativa**: +3.5k stars no GitHub
6. **Compatível**: Funciona junto com FluentUI

### Compatibilidade

- ✅ .NET 8.0
- ✅ Blazor WebAssembly
- ✅ PWA (Progressive Web App)
- ✅ Todos os navegadores modernos
- ✅ iOS Safari, Chrome Mobile, Edge Mobile

