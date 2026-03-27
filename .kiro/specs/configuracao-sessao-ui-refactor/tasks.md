# Implementation Plan: Refatoração UI Configuração de Sessão

## Overview

Este plano implementa a refatoração completa da interface de usuário da tela ConfiguracaoSessao.razor, transformando-a de uma implementação com estilos inline e CSS redundante para uma implementação limpa que utiliza o design system existente (theme.css, components.css, app.css). A refatoração seguirá os padrões estabelecidos em Login.razor e Home.razor, garantindo consistência visual e de código.

A implementação será feita em fases incrementais: limpeza de CSS, padronização de componentes, estrutura e layout, estados e feedback, acessibilidade, e responsividade.

## Tasks

- [ ] 1. Fase 1: Limpeza de CSS e preparação
  - [ ] 1.1 Remover estilos inline do markup Razor
    - Remover todos os atributos `Style=""` dos componentes MudText, MudButton
    - Identificar classes CSS do design system que substituirão os estilos inline
    - _Requirements: 1.1, 1.2_

  - [ ] 1.2 Limpar bloco CSS redundante
    - Remover regras CSS que duplicam estilos do design system
    - Manter apenas estilos específicos necessários (deep selectors para MudBlazor)
    - Consolidar regras CSS similares
    - _Requirements: 8.1, 8.2, 8.3_

  - [ ] 1.3 Identificar e documentar classes do design system a usar
    - Mapear estilos atuais para classes em theme.css, components.css, app.css
    - Criar lista de classes a aplicar: auth-container, auth-card, app-title, app-subtitle, etc.
    - _Requirements: 1.2, 1.4_

- [ ] 2. Fase 2: Padronização de componentes MudBlazor
  - [ ] 2.1 Padronizar configuração dos MudSelect
    - Aplicar Variant="Variant.Outlined", Margin="Margin.Dense", FullWidth="true" a todos os 4 MudSelect
    - Remover propriedades AnchorOrigin e TransformOrigin de todos os MudSelect
    - Adicionar Class="form-select" a todos os MudSelect
    - _Requirements: 2.1, 2.2, 2.3, 11.2_

  - [ ]* 2.2 Escrever teste de propriedade para configuração MudSelect
    - **Property 1: MudSelect Configuration Consistency**
    - **Valida: Requirements 2.1, 2.3, 3.4**
    - Verificar que todos os MudSelect têm Variant.Outlined, Margin.Dense, FullWidth=true

  - [ ] 2.3 Padronizar componentes MudText
    - Aplicar Typo="Typo.h5" ao título com Class="app-title"
    - Aplicar Typo="Typo.body2" ao subtítulo com Class="app-subtitle"
    - Remover estilos inline de font-weight e color
    - _Requirements: 5.1, 5.2, 5.3_

  - [ ]* 2.4 Escrever teste de propriedade para tipografia
    - **Property 2: Typography Consistency**
    - **Valida: Requirements 5.1**
    - Verificar que MudText usa Typo consistente (h5 para títulos, body2 para subtítulos)

  - [ ] 2.5 Padronizar MudButton
    - Manter Variant="Variant.Filled", Color="Color.Primary", Size="Size.Large", FullWidth="true"
    - Adicionar Class="btn-primary-action"
    - Remover estilo inline de padding, border-radius, text-transform, font-weight
    - _Requirements: 7.1, 7.2, 7.5_

- [ ] 3. Fase 3: Estrutura e layout com design system
  - [ ] 3.1 Atualizar MudContainer com classes do design system
    - Alterar Class de "sessao-container" para "auth-container"
    - Manter MaxWidth="MaxWidth.Small"
    - _Requirements: 4.4, 6.5_

  - [ ] 3.2 Atualizar MudPaper com classe auth-card
    - Alterar Class de "sessao-card" para "auth-card"
    - Manter Elevation="0"
    - _Requirements: 12.1, 12.2, 12.3_

  - [ ] 3.3 Adicionar header section com logo
    - Adicionar MudImage com src="images/aspec_logo.png" e Class="app-logo"
    - Posicionar logo antes do título
    - Dimensões: 64x64px, border-radius 12px
    - _Requirements: 4.2_

  - [ ] 3.4 Ajustar espaçamento do MudGrid
    - Manter Spacing="3" no MudGrid
    - Verificar que todos os MudItem têm xs="12"
    - Ajustar margin-top do botão para usar classe CSS
    - _Requirements: 4.1, 4.3_

- [ ] 4. Checkpoint - Verificar estrutura e estilos básicos
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 5. Fase 4: Estados visuais e feedback
  - [ ] 5.1 Implementar estados visuais dos MudSelect
    - Verificar que CSS para estados disabled, focus, hover está aplicado
    - Ajustar CSS deep selectors se necessário para estados visuais
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [ ]* 5.2 Escrever testes unitários para estados visuais
    - Testar estado disabled com visual indicator
    - Testar estado focus com focus ring
    - Testar estado hover com efeito sutil
    - _Requirements: 3.1, 3.2, 3.3_

  - [ ] 5.3 Adicionar estado de carregamento
    - Adicionar propriedade `isLoading` no @code
    - Adicionar MudProgressCircular com Class="loading-overlay" quando isLoading=true
    - Desabilitar form fields quando isLoading=true
    - Atualizar botão para mostrar loading state durante confirmação
    - _Requirements: 10.1, 10.2, 10.3, 10.4_

  - [ ]* 5.4 Escrever testes unitários para loading state
    - Testar que MudProgressCircular é exibido quando isLoading=true
    - Testar que form fields são desabilitados quando isLoading=true
    - _Requirements: 10.1, 10.2_

  - [ ] 5.5 Implementar feedback visual do botão
    - Verificar que estado disabled do botão está claro
    - Adicionar CSS para active state do botão
    - _Requirements: 7.3, 7.4_

- [ ] 6. Fase 5: Acessibilidade
  - [ ] 6.1 Verificar associação de labels
    - Confirmar que todos os MudSelect têm propriedade Label definida
    - Adicionar aria-label onde necessário
    - _Requirements: 9.1_

  - [ ]* 6.2 Escrever teste de propriedade para labels
    - **Property 3: Form Field Label Association**
    - **Valida: Requirements 9.1**
    - Verificar que todos os form fields têm Label ou aria-label

  - [ ] 6.3 Implementar suporte completo a navegação por teclado
    - Verificar que Tab navega entre todos os campos
    - Verificar que Enter/Space ativam botões e abrem dropdowns
    - Verificar que Arrow keys navegam em dropdowns abertos
    - _Requirements: 9.2_

  - [ ]* 6.4 Escrever teste de propriedade para navegação por teclado
    - **Property 4: Keyboard Navigation Support**
    - **Valida: Requirements 9.2**
    - Verificar que todos os elementos interativos são acessíveis via teclado

  - [ ] 6.5 Adicionar ARIA labels apropriados
    - Adicionar aria-label ao logo se necessário
    - Adicionar aria-describedby aos campos com validação
    - Adicionar role apropriado ao loading overlay
    - _Requirements: 9.3_

  - [ ]* 6.6 Escrever teste de propriedade para ARIA labels
    - **Property 5: ARIA Labels Presence**
    - **Valida: Requirements 9.3**
    - Verificar que elementos ambíguos têm ARIA labels

  - [ ] 6.7 Garantir visibilidade de foco
    - Verificar que CSS de focus está aplicado a todos os elementos interativos
    - Ajustar outline width se necessário (mínimo 2px)
    - _Requirements: 9.4_

  - [ ]* 6.8 Escrever teste de propriedade para visibilidade de foco
    - **Property 6: Focus Visibility**
    - **Valida: Requirements 9.4**
    - Verificar que todos os elementos interativos mostram indicador de foco

  - [ ] 6.9 Validar contraste de cores
    - Verificar contraste de texto normal (mínimo 4.5:1)
    - Verificar contraste de texto grande (mínimo 3:1)
    - Ajustar cores se necessário para atender WCAG AA
    - _Requirements: 9.5_

  - [ ]* 6.10 Escrever teste de propriedade para contraste de cores
    - **Property 7: Color Contrast Compliance**
    - **Valida: Requirements 9.5**
    - Verificar que todos os textos têm contraste adequado (4.5:1 ou 3:1)

- [ ] 7. Checkpoint - Verificar acessibilidade
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Fase 6: Responsividade e ajustes finais
  - [ ] 8.1 Testar responsividade em múltiplos tamanhos
    - Testar em 320px (mobile pequeno)
    - Testar em 375px (mobile médio)
    - Testar em 768px (tablet)
    - Testar em 1024px (desktop pequeno)
    - Testar em 1920px (desktop grande)
    - _Requirements: 6.1_

  - [ ]* 8.2 Escrever teste de propriedade para responsividade
    - **Property 8: Responsive Layout Integrity**
    - **Valida: Requirements 6.1**
    - Verificar que layout funciona sem scroll horizontal em 320px-1920px

  - [ ] 8.3 Ajustar padding para mobile
    - Verificar que padding usa env(safe-area-inset-*) para iOS
    - Ajustar padding-bottom para acomodar botão fixo
    - _Requirements: 6.2, 6.4_

  - [ ] 8.4 Validar touch targets
    - Verificar que todos os elementos interativos têm mínimo 44x44px
    - Ajustar padding/height se necessário
    - _Requirements: 6.3_

  - [ ]* 8.5 Escrever teste de propriedade para touch targets
    - **Property 9: Touch Target Minimum Size**
    - **Valida: Requirements 6.3**
    - Verificar que todos os elementos interativos têm >= 44x44px

  - [ ] 8.6 Remover CSS customizado desnecessário do bloco style
    - Remover regras que agora são cobertas pelo design system
    - Manter apenas deep selectors específicos do MudBlazor se necessário
    - _Requirements: 8.4, 8.5_

  - [ ] 8.7 Verificar alinhamento de dropdowns
    - Testar que dropdowns abrem corretamente alinhados
    - Verificar que não há clipping em mobile
    - Confirmar que comportamento padrão do MudBlazor funciona bem
    - _Requirements: 11.1, 11.3, 11.4, 11.5_

- [ ] 9. Final checkpoint - Testes completos e validação
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marcadas com `*` são opcionais e podem ser puladas para MVP mais rápido
- Cada task referencia requirements específicos para rastreabilidade
- Checkpoints garantem validação incremental
- Property tests validam propriedades universais de corretude
- Unit tests validam exemplos específicos e edge cases
- A implementação usa C#/Blazor com MudBlazor como definido no design
- O design system já existe em theme.css, components.css, app.css
- Padrões de referência: Login.razor (auth-card) e Home.razor (layout)
