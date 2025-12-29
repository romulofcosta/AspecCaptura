# Changelog

Todas as alterações notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
e este projeto adere ao [Versionamento Semântico](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Integração da biblioteca UI Fluent UI Blazor para melhorar a interface do usuário.
  - Substituição de componentes HTML nativos por componentes Fluent UI (botões, inputs, selects, labels).
  - Adição de estilos Fluent UI para consistência visual e acessibilidade.
  - Data: 29/12/2025
  - Commit: 13b62c2
  - Impacto: Melhoria na UX/UI, padronização de componentes, melhor acessibilidade.

### Changed
- Atualização do layout da página Home para usar componentes Fluent UI.
  - Substituição de inputs, botões e selects por equivalentes Fluent UI.
  - Data: 29/12/2025
  - Commit: 13b62c2
  - Impacto: Interface mais profissional e consistente.

- Atualização do layout da página Login para usar componentes Fluent UI.
  - Substituição de inputs e botões por equivalentes Fluent UI.
  - Data: 29/12/2025
  - Commit: 13b62c2
  - Impacto: Melhoria na experiência de login.

### Fixed
- Erros de compilação após integração do Fluent UI.
  - Adicionado parâmetros de tipo genérico `TOption="string"` aos componentes `FluentSelect` e `FluentOption`.
  - Qualificado referências ao `ToastService` para evitar ambiguidade com o `ToastService` do Fluent UI.
  - Corrigido binding do `FluentSelect` de `@bind-Value:after` para `@onchange`.
  - Alterado `Appearance.Secondary` para `Appearance.Outline` nos botões de paginação.
  - Data: 29/12/2025
  - Commit: (próximo commit)
  - Impacto: Build funcionando corretamente, aplicação executável.