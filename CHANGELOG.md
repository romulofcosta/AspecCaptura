# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Não Lançado]

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