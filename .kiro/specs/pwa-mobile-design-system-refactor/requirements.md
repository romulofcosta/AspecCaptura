# Requirements Document

## Introduction

Este documento especifica os requisitos para a refatoração completa do design system, UI/UX, estilo, layout e responsividade do AspecCaptura, uma Progressive Web Application (PWA) construída com Blazor WebAssembly (.NET 8.0). O objetivo é criar uma experiência mobile-first otimizada para dispositivos móveis, mantendo consistência visual, acessibilidade e performance.

## Glossary

- **Design_System**: Sistema de design que define tokens (cores, tipografia, espaçamentos, sombras) e componentes reutilizáveis
- **Design_Token**: Variável que armazena valores de design (cores, tamanhos, espaçamentos) para uso consistente
- **Mobile_First**: Abordagem de design que prioriza dispositivos móveis antes de desktop
- **Touch_Target**: Área interativa otimizada para toque (mínimo 44x44px conforme WCAG)
- **CSS_Isolation**: Técnica do Blazor que escopa CSS para componentes específicos
- **Responsive_Layout**: Layout que se adapta fluidamente a diferentes tamanhos de tela
- **PWA_Feature**: Funcionalidade específica de Progressive Web Apps (offline, instalação, notificações)
- **Component_Library**: Biblioteca de componentes Razor reutilizáveis
- **Viewport**: Área visível da aplicação no dispositivo
- **Safe_Area**: Área segura para conteúdo, respeitando notches e barras do sistema
- **Theme_Provider**: Componente que fornece tokens de tema para toda a aplicação
- **Accessibility_Compliance**: Conformidade com padrões WCAG 2.1 AA
- **Performance_Budget**: Limites definidos para métricas de performance (FCP, LCP, TTI)
- **CSS_Custom_Property**: Variável CSS nativa (--variable-name) para valores reutilizáveis
- **Gesture_Handler**: Sistema que processa interações touch (swipe, pinch, long-press)
- **Asset_Listing_Screen**: Tela de listagem de bens do aplicativo Aspec Captura
- **Filter_Chip**: Componente de filtro leve em formato de chip/pill para substituir selects pesados
- **Visual_Hierarchy**: Organização de elementos visuais por ordem de importância e prioridade de ação
- **One_Handed_Use**: Padrão de design otimizado para uso com uma única mão em dispositivos móveis
- **CTA**: Call-to-Action, botão ou elemento que solicita ação principal do usuário

## Requirements

### Requirement 1: Design Token System

**User Story:** Como desenvolvedor, eu quero um sistema centralizado de design tokens, para que eu possa manter consistência visual em toda a aplicação

#### Acceptance Criteria

1. THE Design_System SHALL define color tokens for primary, secondary, neutral, success, warning, error, and info palettes with light and dark variants
2. THE Design_System SHALL define typography tokens for font families, sizes (12px to 48px), weights (400, 500, 600, 700), and line heights
3. THE Design_System SHALL define spacing tokens using a scale of 4px base (4, 8, 12, 16, 24, 32, 48, 64, 96px)
4. THE Design_System SHALL define shadow tokens for elevation levels (none, sm, md, lg, xl)
5. THE Design_System SHALL define border radius tokens (none, sm: 4px, md: 8px, lg: 12px, xl: 16px, full: 9999px)
6. THE Design_System SHALL define breakpoint tokens for mobile (320px), tablet (768px), desktop (1024px), and wide (1440px)
7. THE Design_System SHALL implement all tokens as CSS Custom Properties in a global stylesheet
8. THE Design_System SHALL provide a Theme_Provider component that loads tokens into the application context

### Requirement 2: Component Library Architecture

**User Story:** Como desenvolvedor, eu quero uma biblioteca de componentes reutilizáveis e consistentes, para que eu possa construir interfaces rapidamente

#### Acceptance Criteria

1. THE Component_Library SHALL provide base components (Button, Input, Card, Modal, Toast, Badge, Avatar, Spinner)
2. THE Component_Library SHALL provide layout components (Container, Grid, Stack, Spacer, Divider)
3. THE Component_Library SHALL provide navigation components (BottomNav, TopBar, Drawer, Tabs)
4. THE Component_Library SHALL provide form components (TextField, Select, Checkbox, Radio, Switch, DatePicker)
5. THE Component_Library SHALL provide feedback components (Alert, ProgressBar, Skeleton, EmptyState)
6. WHEN a component is created, THE Component_Library SHALL use CSS_Isolation for component-specific styles
7. WHEN a component is created, THE Component_Library SHALL reference Design_Tokens via CSS_Custom_Property
8. THE Component_Library SHALL provide a consistent API pattern with Parameters for customization

### Requirement 3: Mobile-First Responsive Layout

**User Story:** Como usuário mobile, eu quero que a interface se adapte perfeitamente ao meu dispositivo, para que eu tenha uma experiência otimizada

#### Acceptance Criteria

1. THE Responsive_Layout SHALL use Mobile_First approach with min-width media queries
2. THE Responsive_Layout SHALL use CSS Grid and Flexbox for fluid layouts
3. THE Responsive_Layout SHALL ensure all Touch_Target areas are minimum 44x44px
4. THE Responsive_Layout SHALL respect Safe_Area insets on devices with notches
5. WHEN Viewport width is below 768px, THE Responsive_Layout SHALL display single-column layouts
6. WHEN Viewport width is 768px or above, THE Responsive_Layout SHALL display multi-column layouts where appropriate
7. THE Responsive_Layout SHALL use relative units (rem, em, %, vw, vh) instead of fixed pixels for sizing
8. THE Responsive_Layout SHALL implement fluid typography that scales between breakpoints

### Requirement 4: Touch Interaction and Gestures

**User Story:** Como usuário mobile, eu quero interações touch naturais e responsivas, para que eu possa navegar facilmente

#### Acceptance Criteria

1. THE Gesture_Handler SHALL support swipe gestures for navigation (swipe-left, swipe-right)
2. THE Gesture_Handler SHALL support pull-to-refresh gesture on scrollable lists
3. THE Gesture_Handler SHALL support long-press gesture for contextual actions
4. WHEN a user taps an interactive element, THE Component_Library SHALL provide visual feedback within 100ms
5. THE Component_Library SHALL implement ripple effect for button interactions
6. THE Component_Library SHALL prevent double-tap zoom on interactive elements
7. THE Component_Library SHALL support pinch-to-zoom gesture on image viewers
8. WHEN a gesture is detected, THE Gesture_Handler SHALL prevent default browser behaviors that conflict with app navigation

### Requirement 5: Navigation Optimization

**User Story:** Como usuário mobile, eu quero navegação intuitiva e acessível, para que eu possa acessar funcionalidades rapidamente

#### Acceptance Criteria

1. THE Navigation SHALL use bottom navigation bar for primary navigation on mobile devices
2. THE Navigation SHALL display maximum 5 primary navigation items in bottom bar
3. THE Navigation SHALL highlight the active navigation item with visual indicator
4. WHEN Viewport width is 768px or above, THE Navigation SHALL display sidebar navigation instead of bottom bar
5. THE Navigation SHALL provide hamburger menu for secondary navigation items
6. THE Navigation SHALL support keyboard navigation with Tab and Arrow keys
7. THE Navigation SHALL announce navigation changes to screen readers
8. WHEN a navigation item is tapped, THE Navigation SHALL provide haptic feedback where supported

### Requirement 6: Performance Optimization

**User Story:** Como usuário com conexão móvel limitada, eu quero que a aplicação carregue rapidamente, para que eu possa começar a trabalhar sem demora

#### Acceptance Criteria

1. THE PWA_Feature SHALL achieve First Contentful Paint (FCP) under 1.8 seconds on 3G connection
2. THE PWA_Feature SHALL achieve Largest Contentful Paint (LCP) under 2.5 seconds on 3G connection
3. THE PWA_Feature SHALL achieve Time to Interactive (TTI) under 3.8 seconds on 3G connection
4. THE PWA_Feature SHALL achieve Cumulative Layout Shift (CLS) under 0.1
5. THE Component_Library SHALL lazy-load components that are not immediately visible
6. THE Component_Library SHALL use CSS containment for performance isolation
7. THE PWA_Feature SHALL compress and optimize all images to WebP format with fallbacks
8. THE PWA_Feature SHALL implement critical CSS inlining for above-the-fold content
9. THE PWA_Feature SHALL minimize JavaScript bundle size to under 500KB gzipped

### Requirement 7: Offline Capability

**User Story:** Como usuário em áreas com conectividade intermitente, eu quero continuar usando funcionalidades básicas offline, para que eu possa trabalhar sem interrupção

#### Acceptance Criteria

1. THE PWA_Feature SHALL cache application shell for offline access
2. THE PWA_Feature SHALL cache static assets (CSS, JS, images, fonts) using service worker
3. WHEN network is unavailable, THE PWA_Feature SHALL serve cached content
4. WHEN network is unavailable, THE PWA_Feature SHALL display offline indicator in UI
5. THE PWA_Feature SHALL queue user actions when offline and sync when connection is restored
6. THE PWA_Feature SHALL cache recently viewed data for offline access
7. WHEN transitioning from offline to online, THE PWA_Feature SHALL sync queued actions automatically
8. THE PWA_Feature SHALL provide clear feedback about sync status to users

### Requirement 8: Accessibility Compliance

**User Story:** Como usuário com necessidades de acessibilidade, eu quero que a aplicação seja totalmente acessível, para que eu possa usar todas as funcionalidades

#### Acceptance Criteria

1. THE Component_Library SHALL provide proper ARIA labels for all interactive elements
2. THE Component_Library SHALL maintain color contrast ratio of at least 4.5:1 for normal text
3. THE Component_Library SHALL maintain color contrast ratio of at least 3:1 for large text and UI components
4. THE Component_Library SHALL support keyboard navigation for all interactive elements
5. THE Component_Library SHALL provide visible focus indicators with 2px outline
6. THE Component_Library SHALL announce dynamic content changes to screen readers using ARIA live regions
7. THE Component_Library SHALL provide text alternatives for all non-text content
8. THE Component_Library SHALL support text resize up to 200% without loss of functionality
9. THE Component_Library SHALL use semantic HTML elements (nav, main, article, section, header, footer)
10. WHEN an error occurs, THE Component_Library SHALL provide clear error messages with suggestions for resolution

### Requirement 9: Dark Mode Support

**User Story:** Como usuário que prefere interfaces escuras, eu quero suporte a dark mode, para que eu possa reduzir fadiga visual

#### Acceptance Criteria

1. THE Theme_Provider SHALL detect system color scheme preference (light/dark)
2. THE Theme_Provider SHALL allow manual theme toggle override
3. THE Theme_Provider SHALL persist theme preference in local storage
4. WHEN theme changes, THE Theme_Provider SHALL update all Design_Tokens smoothly with 200ms transition
5. THE Design_System SHALL define dark mode color palette with appropriate contrast ratios
6. THE Component_Library SHALL render correctly in both light and dark themes
7. THE Theme_Provider SHALL apply theme without page reload or flicker
8. THE Design_System SHALL use semantic color tokens (background, surface, text-primary, text-secondary) that adapt to theme

### Requirement 10: Animation and Transitions

**User Story:** Como usuário, eu quero animações suaves e significativas, para que a interface pareça responsiva e polida

#### Acceptance Criteria

1. THE Component_Library SHALL use CSS transitions for state changes (hover, focus, active)
2. THE Component_Library SHALL use animation duration between 150ms and 300ms for micro-interactions
3. THE Component_Library SHALL use animation duration between 300ms and 500ms for page transitions
4. THE Component_Library SHALL use easing functions (ease-out for entrances, ease-in for exits)
5. THE Component_Library SHALL respect prefers-reduced-motion media query for users with motion sensitivity
6. WHEN prefers-reduced-motion is enabled, THE Component_Library SHALL disable non-essential animations
7. THE Component_Library SHALL use transform and opacity for animations to ensure 60fps performance
8. THE Component_Library SHALL provide loading skeletons with shimmer animation for async content

### Requirement 11: Form Input Optimization

**User Story:** Como usuário mobile, eu quero formulários otimizados para entrada touch, para que eu possa preencher dados rapidamente

#### Acceptance Criteria

1. THE Component_Library SHALL use appropriate input types (tel, email, number, date) to trigger correct mobile keyboards
2. THE Component_Library SHALL provide autocomplete attributes for common fields (name, email, phone)
3. THE Component_Library SHALL display inline validation feedback as user types
4. WHEN validation fails, THE Component_Library SHALL display error message below the field with error icon
5. THE Component_Library SHALL use floating labels that move above input when focused or filled
6. THE Component_Library SHALL provide clear button (X icon) for text inputs with content
7. THE Component_Library SHALL group related fields visually with proper spacing
8. THE Component_Library SHALL auto-focus first input field when form is displayed
9. THE Component_Library SHALL prevent form submission on Enter key for multi-field forms
10. THE Component_Library SHALL provide visual indication of required fields with asterisk

### Requirement 12: Image and Media Handling

**User Story:** Como usuário que captura fotos, eu quero visualização otimizada de imagens, para que eu possa revisar capturas facilmente

#### Acceptance Criteria

1. THE Component_Library SHALL provide responsive image component with srcset for different resolutions
2. THE Component_Library SHALL lazy-load images below the fold
3. THE Component_Library SHALL display placeholder with blur-up effect while image loads
4. THE Component_Library SHALL provide image viewer with pinch-to-zoom and pan gestures
5. THE Component_Library SHALL compress uploaded images to maximum 1920px width
6. THE Component_Library SHALL convert images to WebP format with JPEG fallback
7. WHEN image fails to load, THE Component_Library SHALL display fallback placeholder with retry button
8. THE Component_Library SHALL provide thumbnail grid with aspect ratio preservation

### Requirement 13: Loading States and Feedback

**User Story:** Como usuário, eu quero feedback claro sobre o estado da aplicação, para que eu saiba quando ações estão em progresso

#### Acceptance Criteria

1. THE Component_Library SHALL display loading spinner for async operations longer than 300ms
2. THE Component_Library SHALL display skeleton screens for initial page loads
3. THE Component_Library SHALL display progress bar for operations with known duration
4. THE Component_Library SHALL display toast notifications for completed actions
5. WHEN an action succeeds, THE Component_Library SHALL display success toast for 3 seconds
6. WHEN an action fails, THE Component_Library SHALL display error toast with retry option
7. THE Component_Library SHALL display empty state with illustration and call-to-action when no data exists
8. THE Component_Library SHALL provide pull-to-refresh visual feedback with spinner

### Requirement 14: Typography System

**User Story:** Como usuário, eu quero texto legível e hierarquia visual clara, para que eu possa ler conteúdo confortavelmente

#### Acceptance Criteria

1. THE Design_System SHALL use system font stack for optimal performance and native feel
2. THE Design_System SHALL define heading styles (h1: 32px, h2: 24px, h3: 20px, h4: 18px, h5: 16px, h6: 14px)
3. THE Design_System SHALL define body text styles (body-lg: 16px, body: 14px, body-sm: 12px)
4. THE Design_System SHALL use line-height of 1.5 for body text and 1.2 for headings
5. THE Design_System SHALL use letter-spacing of -0.01em for headings
6. THE Design_System SHALL limit line length to 75 characters for optimal readability
7. THE Design_System SHALL use font-weight 600 or 700 for emphasis instead of bold
8. THE Component_Library SHALL render text with -webkit-font-smoothing: antialiased for smooth rendering

### Requirement 15: Icon System

**User Story:** Como desenvolvedor, eu quero um sistema de ícones consistente, para que eu possa adicionar ícones facilmente

#### Acceptance Criteria

1. THE Design_System SHALL use Material Symbols icon library
2. THE Component_Library SHALL provide Icon component with size variants (sm: 16px, md: 20px, lg: 24px, xl: 32px)
3. THE Component_Library SHALL support icon color customization via CSS_Custom_Property
4. THE Component_Library SHALL provide icon-only buttons with proper ARIA labels
5. THE Component_Library SHALL support icon with text combinations with proper spacing
6. THE Component_Library SHALL load icons as font or SVG sprite for performance
7. THE Component_Library SHALL provide filled and outlined icon variants
8. THE Icon component SHALL have default size of 24px and inherit color from parent

### Requirement 16: Card Component System

**User Story:** Como usuário, eu quero cards visuais consistentes para exibir informações, para que eu possa escanear conteúdo rapidamente

#### Acceptance Criteria

1. THE Component_Library SHALL provide Card component with header, body, and footer sections
2. THE Card component SHALL support elevation levels (flat, raised, elevated)
3. THE Card component SHALL support interactive variant with hover and active states
4. THE Card component SHALL support horizontal and vertical layouts
5. THE Card component SHALL support image with overlay text
6. THE Card component SHALL use 16px padding for content areas
7. THE Card component SHALL use 12px border radius
8. WHEN Card is interactive, THE Card component SHALL display ripple effect on tap

### Requirement 17: Modal and Dialog System

**User Story:** Como usuário, eu quero modais e diálogos que funcionem bem em mobile, para que eu possa completar ações focadas

#### Acceptance Criteria

1. THE Component_Library SHALL provide Modal component that covers full viewport on mobile
2. THE Modal component SHALL display as centered dialog on desktop (min-width: 768px)
3. THE Modal component SHALL support swipe-down gesture to dismiss on mobile
4. THE Modal component SHALL trap keyboard focus within modal when open
5. THE Modal component SHALL close on Escape key press
6. THE Modal component SHALL close on backdrop click unless closeOnBackdrop is false
7. THE Modal component SHALL prevent body scroll when open
8. THE Modal component SHALL animate entrance from bottom on mobile and fade-in on desktop
9. THE Modal component SHALL provide header with title and close button
10. THE Modal component SHALL announce modal opening to screen readers with role="dialog"

### Requirement 18: List and Grid Layouts

**User Story:** Como usuário, eu quero visualizar listas e grids de forma eficiente, para que eu possa navegar grandes conjuntos de dados

#### Acceptance Criteria

1. THE Component_Library SHALL provide List component with virtual scrolling for lists over 100 items
2. THE Component_Library SHALL provide Grid component with responsive columns (1 col mobile, 2-4 cols desktop)
3. THE List component SHALL support pull-to-refresh gesture
4. THE List component SHALL support infinite scroll with loading indicator
5. THE List component SHALL display item dividers with 1px border
6. THE Grid component SHALL maintain consistent gaps using spacing tokens
7. THE List component SHALL support swipe actions (swipe-left for delete, swipe-right for archive)
8. THE Component_Library SHALL provide EmptyState component for lists with no items

### Requirement 19: Bottom Sheet Component

**User Story:** Como usuário mobile, eu quero bottom sheets para ações contextuais, para que eu possa acessar opções facilmente

#### Acceptance Criteria

1. THE Component_Library SHALL provide BottomSheet component that slides up from bottom
2. THE BottomSheet component SHALL support drag handle for visual affordance
3. THE BottomSheet component SHALL support swipe-down gesture to dismiss
4. THE BottomSheet component SHALL support snap points (collapsed, half, full)
5. THE BottomSheet component SHALL dim background with overlay when open
6. THE BottomSheet component SHALL prevent body scroll when open
7. THE BottomSheet component SHALL animate entrance with 300ms ease-out transition
8. WHEN BottomSheet is dismissed, THE BottomSheet component SHALL animate exit with 200ms ease-in transition

### Requirement 20: Search and Filter UI

**User Story:** Como usuário, eu quero buscar e filtrar conteúdo facilmente, para que eu possa encontrar informações rapidamente

#### Acceptance Criteria

1. THE Component_Library SHALL provide SearchBar component with clear button
2. THE SearchBar component SHALL display search icon on left side
3. THE SearchBar component SHALL provide autocomplete suggestions below input
4. THE SearchBar component SHALL debounce search input by 300ms
5. THE Component_Library SHALL provide FilterChip component for active filters
6. THE FilterChip component SHALL display remove button (X icon) on right side
7. THE Component_Library SHALL provide FilterPanel component for filter options
8. WHEN filters are applied, THE Component_Library SHALL display filter count badge on filter button

### Requirement 21: Status and Badge System

**User Story:** Como usuário, eu quero indicadores visuais claros de status, para que eu possa entender o estado de itens rapidamente

#### Acceptance Criteria

1. THE Component_Library SHALL provide Badge component with color variants (primary, success, warning, error, neutral)
2. THE Badge component SHALL support sizes (sm: 16px height, md: 20px height, lg: 24px height)
3. THE Badge component SHALL support dot variant for notification indicators
4. THE Badge component SHALL support positioning (top-right, top-left, bottom-right, bottom-left) when overlaying elements
5. THE Component_Library SHALL provide StatusIndicator component with online/offline/busy states
6. THE StatusIndicator component SHALL use semantic colors (green: online, red: offline, yellow: busy)
7. THE Badge component SHALL use 12px font size and 600 font weight
8. THE Badge component SHALL use full border radius for pill shape

### Requirement 22: Skeleton Loading System

**User Story:** Como usuário, eu quero ver placeholders durante carregamento, para que eu entenda que conteúdo está chegando

#### Acceptance Criteria

1. THE Component_Library SHALL provide Skeleton component with shimmer animation
2. THE Skeleton component SHALL support shape variants (text, circle, rectangle)
3. THE Skeleton component SHALL support width and height customization
4. THE Skeleton component SHALL use 200ms fade-out transition when content loads
5. THE Component_Library SHALL provide pre-built skeleton layouts for common patterns (card, list, profile)
6. THE Skeleton component SHALL use neutral gray color that adapts to theme
7. THE Skeleton component SHALL animate shimmer from left to right with 1.5s duration
8. WHEN prefers-reduced-motion is enabled, THE Skeleton component SHALL display static placeholder without animation

### Requirement 23: Toast Notification System

**User Story:** Como usuário, eu quero notificações não-intrusivas, para que eu receba feedback sem interromper meu trabalho

#### Acceptance Criteria

1. THE Component_Library SHALL provide Toast component with variants (success, error, warning, info)
2. THE Toast component SHALL display at top-center on mobile and top-right on desktop
3. THE Toast component SHALL auto-dismiss after 3 seconds for success and 5 seconds for errors
4. THE Toast component SHALL support manual dismiss with close button
5. THE Toast component SHALL stack multiple toasts with 8px gap
6. THE Toast component SHALL animate entrance from top with slide-down effect
7. THE Toast component SHALL support action button for undo operations
8. THE Toast component SHALL announce message to screen readers with role="alert"
9. THE Toast component SHALL pause auto-dismiss timer on hover or focus
10. THE Toast component SHALL limit maximum visible toasts to 3, queuing additional toasts

### Requirement 24: Progressive Enhancement

**User Story:** Como usuário com JavaScript desabilitado ou falhas de carregamento, eu quero funcionalidade básica, para que eu possa usar a aplicação mesmo em condições adversas

#### Acceptance Criteria

1. THE PWA_Feature SHALL display meaningful content before JavaScript loads
2. THE PWA_Feature SHALL provide fallback styles for critical UI when CSS fails to load
3. THE Component_Library SHALL use semantic HTML that works without JavaScript
4. THE PWA_Feature SHALL display error boundary with recovery options when JavaScript errors occur
5. THE PWA_Feature SHALL provide noscript message with instructions when JavaScript is disabled
6. THE Component_Library SHALL use native HTML form validation as fallback
7. THE PWA_Feature SHALL ensure critical navigation works with standard HTML links
8. THE PWA_Feature SHALL provide print stylesheet for document printing

### Requirement 25: Installation and App-like Experience

**User Story:** Como usuário, eu quero instalar a aplicação no meu dispositivo, para que eu possa acessá-la como um app nativo

#### Acceptance Criteria

1. THE PWA_Feature SHALL provide web app manifest with name, icons, theme color, and display mode
2. THE PWA_Feature SHALL register service worker for offline capability
3. THE PWA_Feature SHALL display install prompt when PWA criteria are met
4. THE PWA_Feature SHALL use standalone display mode to hide browser UI
5. THE PWA_Feature SHALL provide splash screen with app icon and brand color
6. THE PWA_Feature SHALL use theme-color meta tag that adapts to current theme
7. THE PWA_Feature SHALL provide icons in sizes 192x192 and 512x512
8. THE PWA_Feature SHALL handle app shortcuts for quick access to key features
9. THE PWA_Feature SHALL support share target API for receiving shared content
10. THE PWA_Feature SHALL provide maskable icon for adaptive icon support on Android

### Requirement 26: Asset Listing Screen UI/UX

**User Story:** Como usuário da tela de listagem de bens, eu quero uma interface minimalista e profissional focada na ação principal de escanear, para que eu possa trabalhar de forma eficiente com uma mão

#### Acceptance Criteria

1. THE Asset_Listing_Screen SHALL implement a clear visual hierarchy with primary action (Escanear Bem) positioned at the top
2. THE Asset_Listing_Screen SHALL NOT use floating action button (FAB) pattern
3. THE Asset_Listing_Screen SHALL NOT duplicate CTA buttons in empty state or other sections
4. THE Asset_Listing_Screen SHALL organize elements in the following order: Header, Search Field, Primary CTA Button, Filters, Results Summary, Content/Empty State
5. THE Asset_Listing_Screen SHALL use lightweight chip-based filters instead of heavy select dropdowns
6. THE Asset_Listing_Screen SHALL display results summary inline without heavy container blocks
7. THE Asset_Listing_Screen SHALL implement empty state with icon, title, and subtitle WITHOUT action button
8. THE Asset_Listing_Screen SHALL use spacing (not borders) to separate sections visually
9. THE Asset_Listing_Screen SHALL optimize layout for one-handed mobile use
10. THE Asset_Listing_Screen SHALL reduce visual weight by minimizing background colors and unnecessary containers
11. THE Asset_Listing_Screen SHALL use the defined Design_System tokens: --color-primary (#2F6FED), --color-bg (#F8FAFC), --color-surface (#FFFFFF), --color-border (#E5E7EB), --color-text (#111827), --color-text-secondary (#6B7280)
12. THE Asset_Listing_Screen SHALL use spacing tokens: --space-2 (8px), --space-3 (12px), --space-4 (16px), --space-5 (20px)
13. THE Asset_Listing_Screen SHALL use border radius tokens: --radius-md (12px), --radius-lg (16px)
14. THE Asset_Listing_Screen SHALL implement primary button with full width, 14px padding, rounded corners (--radius-lg), and subtle shadow
15. THE Asset_Listing_Screen SHALL implement search input with 12px padding, rounded corners (--radius-md), and minimal border
16. THE Asset_Listing_Screen SHALL implement filter chips with 8px vertical and 12px horizontal padding, pill shape (border-radius: 999px), and light background (#EEF2FF)
17. THE Asset_Listing_Screen SHALL display active filter chips with visual distinction
18. THE Asset_Listing_Screen SHALL use typography scale: title (18px, font-weight 600), subtitle (14px, color: --color-text-secondary)
19. THE Asset_Listing_Screen SHALL implement empty state with centered icon (📦 emoji or equivalent), title, and descriptive subtitle
20. WHEN user interacts with primary button, THE Asset_Listing_Screen SHALL provide immediate visual feedback (ripple effect)
21. WHEN filters are applied, THE Asset_Listing_Screen SHALL update results summary inline
22. WHEN search is performed, THE Asset_Listing_Screen SHALL debounce input by 300ms
23. THE Asset_Listing_Screen SHALL maintain consistent 16px horizontal padding throughout
24. THE Asset_Listing_Screen SHALL avoid generic template appearance by using custom spacing and visual rhythm
25. THE Asset_Listing_Screen SHALL prioritize scannable content with clear information hierarchy

## Notes

### Implementation Priority

1. **Phase 1 - Foundation**: Design tokens, theme provider, base component architecture
2. **Phase 2 - Core Components**: Button, Input, Card, Modal, Navigation, Filter Chips
3. **Phase 3 - Layout & Responsiveness**: Grid, responsive layouts, mobile-first styles
4. **Phase 4 - Asset Listing Screen**: Implement complete refactored listing screen with new design system
5. **Phase 5 - Interactions**: Touch gestures, animations, transitions
6. **Phase 6 - Advanced Features**: Offline support, performance optimization, PWA enhancements
7. **Phase 7 - Polish**: Accessibility audit, dark mode refinement, animation polish

### Asset Listing Screen Design Specifications

**Color Palette:**
- Primary: #2F6FED
- Background: #F8FAFC
- Surface: #FFFFFF
- Border: #E5E7EB
- Text: #111827
- Text Secondary: #6B7280
- Filter Chip Background: #EEF2FF

**Spacing Scale:**
- space-2: 8px
- space-3: 12px
- space-4: 16px
- space-5: 20px

**Border Radius:**
- radius-md: 12px (inputs, cards)
- radius-lg: 16px (primary buttons)
- radius-full: 999px (filter chips)

**Typography:**
- Title: 18px, font-weight 600
- Subtitle: 14px, color: text-secondary
- Body: 14px
- Small: 13px (filter chips)

**Component Specifications:**

1. **Primary Button (Escanear Bem)**
   - Width: 100%
   - Padding: 14px
   - Border-radius: 16px
   - Background: #2F6FED
   - Color: white
   - Font-weight: 500
   - Box-shadow: 0 1px 2px rgba(0,0,0,0.05)

2. **Search Input**
   - Width: 100%
   - Padding: 12px
   - Border-radius: 12px
   - Border: 1px solid #E5E7EB
   - Background: #FFFFFF

3. **Filter Chips**
   - Padding: 8px 12px
   - Border-radius: 999px
   - Background: #EEF2FF (inactive), #2F6FED (active)
   - Font-size: 13px
   - Color: #2F6FED (inactive), white (active)

4. **Empty State**
   - Icon: 📦 (64px)
   - Title: 18px, font-weight 600
   - Subtitle: 14px, color: #6B7280
   - No action button

**Layout Structure:**
```
┌─────────────────────────────┐
│ Header (simple, lightweight)│
├─────────────────────────────┤
│ Search Field                │
├─────────────────────────────┤
│ [Escanear Bem] (CTA)       │
├─────────────────────────────┤
│ [Chip] [Chip] (Filters)    │
├─────────────────────────────┤
│ 0 bens • Secretaria...      │
├─────────────────────────────┤
│ Content / Empty State       │
└─────────────────────────────┘
```

**UX Principles:**
- One-handed operation priority
- Primary action always visible at top
- No FAB (floating action button)
- No duplicate CTAs
- Lightweight visual design
- Spacing over borders for separation
- Scannable content hierarchy
- Professional, non-generic appearance

### Testing Strategy

- Visual regression testing for component consistency
- Accessibility testing with automated tools (axe, Lighthouse) and manual screen reader testing
- Performance testing on real mobile devices with throttled connections
- Cross-browser testing (Chrome, Safari, Firefox, Edge)
- Touch interaction testing on iOS and Android devices
- Offline functionality testing with service worker

### Design References

- Material Design 3 guidelines for mobile patterns
- iOS Human Interface Guidelines for touch targets and gestures
- WCAG 2.1 AA standards for accessibility
- Web.dev PWA best practices

### Migration Strategy

- Implement new design system alongside existing styles
- Migrate components incrementally, starting with most-used components
- Use feature flags to toggle between old and new UI
- Maintain backward compatibility during transition period
- Document migration guide for developers
