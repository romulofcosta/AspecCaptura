# Requirements Document

## Introduction

Este documento define os requisitos para a refatoração completa de front-end, estilo e layout do aplicativo Blazor PWA de captura de fotos para tombamento de patrimônio. O objetivo é **remover completamente MudBlazor** e migrar para **Tailwind CSS + Blazor WebAssembly puro**, permitindo controle total sobre a estética e implementando um Design Industrial Premium de alta densidade de dados, conforme os wireframes propostos.

## Glossary

- **UI_System**: O sistema de interface do usuário do aplicativo Blazor PWA baseado em Tailwind CSS
- **Tailwind_Engine**: Motor CSS utility-first configurado via CLI ou PostCSS para estilização
- **Theme_Manager**: Componente responsável por gerenciar temas e estilos globais via Tailwind config
- **Layout_Component**: Componente de layout Blazor puro que define a estrutura visual das páginas
- **Typography_System**: Sistema de tipografia baseado em Space Grotesk, Inter e JetBrains Mono
- **Color_Palette**: Paleta de cores de Design Industrial Premium com primary (#00327d) e surface variants
- **Scanner_Screen**: Tela de captura com câmera fullscreen e controles de reconhecimento
- **Mode_Toggle**: Controles verticais para alternar entre OCR, Barcode e QR
- **Viewfinder**: Área centralizada de captura com cantos azuis animados
- **Data_Feed**: Feed de dados na parte inferior mostrando última captura e status de conexão
- **Bottom_Navigation**: Barra de navegação inferior com 4 ícones presente em todas as telas
- **Blazor_Component**: Componente Blazor puro sem dependências de bibliotecas de UI externas
- **PWA**: Progressive Web Application
- **Safe_Area**: Área segura para conteúdo em dispositivos móveis (notch, bordas arredondadas)
- **Material_Symbols**: Biblioteca de ícones do Google (SVG ou font) para iconografia consistente
- **Design_Industrial_Premium**: Estilo visual focado em alta densidade de dados, contraste e funcionalidade

## Requirements

### Requirement 1: Remoção Completa de MudBlazor

**User Story:** Como desenvolvedor, eu quero remover completamente MudBlazor do projeto, para que eu tenha controle total sobre a estética e evite conflitos com Design Industrial Premium.

#### Acceptance Criteria

1. THE UI_System SHALL remove all MudBlazor NuGet package references from the project file
2. THE UI_System SHALL remove all MudBlazor using statements from Razor components
3. THE UI_System SHALL remove all MudBlazor component usages (MudButton, MudTextField, MudCard, etc)
4. THE UI_System SHALL remove MudBlazor service registrations from Program.cs
5. THE UI_System SHALL remove MudBlazor CSS references from index.html
6. THE UI_System SHALL remove MudBlazor JavaScript references from index.html
7. WHEN the project is built, THE UI_System SHALL compile successfully without any MudBlazor dependencies

### Requirement 2: Configuração do Tailwind CSS

**User Story:** Como desenvolvedor, eu quero configurar Tailwind CSS no projeto Blazor, para que eu possa usar utility classes para estilização.

#### Acceptance Criteria

1. THE Tailwind_Engine SHALL be installed via npm or standalone CLI
2. THE Tailwind_Engine SHALL have a tailwind.config.js file with custom theme configuration
3. THE Tailwind_Engine SHALL scan Razor files (*.razor, *.html) for class names
4. THE Tailwind_Engine SHALL generate optimized CSS output file
5. THE Tailwind_Engine SHALL be integrated into the build process
6. THE Tailwind_Engine SHALL support JIT (Just-In-Time) mode for development
7. WHEN the project is built, THE Tailwind_Engine SHALL generate a minified CSS file for production

### Requirement 3: Sistema de Tipografia com Tailwind

**User Story:** Como desenvolvedor, eu quero um sistema de tipografia consistente baseado nos wireframes, para que todas as páginas usem as fontes corretas via Tailwind CSS.

#### Acceptance Criteria

1. THE Typography_System SHALL configure Space Grotesk font family in Tailwind config for headlines
2. THE Typography_System SHALL configure Inter font family in Tailwind config for body text
3. THE Typography_System SHALL configure JetBrains Mono font family in Tailwind config for monospace
4. THE Typography_System SHALL define font weights in Tailwind config (400, 500, 600, 700)
5. THE Typography_System SHALL define font sizes in Tailwind config (text-xs, text-sm, text-base, text-lg, text-xl)
6. THE Typography_System SHALL load fonts from Google Fonts via link tag in index.html
7. WHEN a page is rendered, THE Typography_System SHALL apply fonts via Tailwind utility classes

### Requirement 4: Paleta de Cores de Design Industrial Premium

**User Story:** Como desenvolvedor, eu quero uma paleta de cores de Design Industrial Premium, para que o aplicativo tenha alta densidade de dados e contraste adequado.

#### Acceptance Criteria

1. THE Color_Palette SHALL define primary color as #00327d in Tailwind config
2. THE Color_Palette SHALL define surface variants (surface-dim, surface-bright, surface-container) in Tailwind config
3. THE Color_Palette SHALL define semantic colors (success, warning, error, info) in Tailwind config
4. THE Color_Palette SHALL define on-surface text colors with contrast ratios meeting WCAG AA
5. THE Color_Palette SHALL define outline colors for borders and dividers in Tailwind config
6. THE Color_Palette SHALL avoid Material Design constraints and focus on industrial aesthetics
7. WHEN a component is rendered, THE Color_Palette SHALL apply colors via Tailwind utility classes

### Requirement 5: Sistema de Espaçamento com Tailwind

**User Story:** Como desenvolvedor, eu quero um sistema de espaçamento consistente via Tailwind, para que todos os componentes tenham margens e paddings uniformes.

#### Acceptance Criteria

1. THE UI_System SHALL use Tailwind default spacing scale (4px, 8px, 16px, 24px, 32px, 48px, 64px)
2. THE UI_System SHALL use p-4 (16px) as default padding for containers
3. THE UI_System SHALL use gap-2 (8px) as default gap between inline elements
4. THE UI_System SHALL use gap-6 (24px) as default gap between sections
5. THE UI_System SHALL define border radius values in Tailwind config (rounded-sm, rounded-md, rounded-lg, rounded-xl)
6. THE UI_System SHALL avoid inline styles and use Tailwind utility classes exclusively
7. WHEN a component is rendered, THE UI_System SHALL apply spacing via Tailwind classes

### Requirement 6: Componentes Blazor Puros Reutilizáveis

**User Story:** Como desenvolvedor, eu quero componentes Blazor puros reutilizáveis, para que eu possa construir páginas sem dependências de bibliotecas externas.

#### Acceptance Criteria

1. THE Blazor_Component SHALL provide reusable Card component styled with Tailwind classes
2. THE Blazor_Component SHALL provide reusable Button component with variants (filled, outlined, text)
3. THE Blazor_Component SHALL provide reusable Input component styled with Tailwind classes
4. THE Blazor_Component SHALL provide reusable Badge component for status indicators
5. THE Blazor_Component SHALL provide reusable Chip component for filters
6. THE Blazor_Component SHALL provide reusable Select component for dropdowns
7. THE Blazor_Component SHALL provide reusable Toggle component for boolean settings
8. WHEN a developer creates a new page, THE Blazor_Component SHALL be reusable without MudBlazor dependencies

### Requirement 7: Redesign Completo da Scanner Screen

**User Story:** Como usuário, eu quero uma interface de scanner moderna e intuitiva conforme os wireframes, para que eu possa capturar patrimônios com facilidade.

#### Acceptance Criteria

1. THE Scanner_Screen SHALL display camera feed in fullscreen mode using Tailwind h-screen and w-screen
2. THE Scanner_Screen SHALL display mode toggles (OCR, Barcode, QR) as vertical icons at the top
3. THE Scanner_Screen SHALL display viewfinder centralizado com cantos azuis animados via Tailwind
4. THE Scanner_Screen SHALL display instruction text centralizado abaixo do viewfinder
5. THE Scanner_Screen SHALL display status "PROCESSANDO" when recognition is active
6. THE Scanner_Screen SHALL remove all MudBlazor badges and replace with custom Blazor components
7. WHEN user taps a mode toggle, THE Scanner_Screen SHALL highlight the selected mode with primary color

### Requirement 8: Controles de Flash e Zoom no Scanner

**User Story:** Como usuário, eu quero controles visíveis de flash e zoom, para que eu possa ajustar a captura conforme necessário.

#### Acceptance Criteria

1. THE Scanner_Screen SHALL display flash toggle button on the right side styled with Tailwind
2. THE Scanner_Screen SHALL display zoom slider on the right side below the flash button
3. THE Scanner_Screen SHALL use Material Symbols icons for flash (flash_on, flash_off)
4. THE Scanner_Screen SHALL use Material Symbols icons for zoom (zoom_in, zoom_out)
5. WHEN user taps flash button, THE Scanner_Screen SHALL toggle camera flash on/off
6. WHEN user adjusts zoom slider, THE Scanner_Screen SHALL update camera zoom level in real-time
7. THE Scanner_Screen SHALL position controls with Tailwind classes (mr-4, safe-area-inset)

### Requirement 9: Data Feed na Scanner Screen

**User Story:** Como usuário, eu quero ver informações sobre a última captura e status de conexão, para que eu tenha feedback visual do sistema.

#### Acceptance Criteria

1. THE Scanner_Screen SHALL display data feed section at the bottom above bottom navigation
2. THE Data_Feed SHALL display last captured item with thumbnail and ID in monospace font
3. THE Data_Feed SHALL display connection status indicator (online/offline) with colored dot
4. THE Data_Feed SHALL display sync status (synced/pending) for last captured item
5. THE Data_Feed SHALL use Tailwind backdrop-blur-md for semi-transparent background
6. WHEN a new item is captured, THE Data_Feed SHALL update to show the new item immediately
7. THE Data_Feed SHALL use Tailwind classes for padding (p-2) and gap (gap-1)

### Requirement 10: Bottom Navigation Consistente

**User Story:** Como usuário, eu quero uma barra de navegação inferior consistente em todas as telas, para que eu possa navegar facilmente pelo aplicativo.

#### Acceptance Criteria

1. THE Bottom_Navigation SHALL display 4 navigation icons (Home, Lista, Scanner, Ajustes)
2. THE Bottom_Navigation SHALL be present on all main screens (Login excluded)
3. THE Bottom_Navigation SHALL use Material Symbols icons with size-6 (24px) Tailwind class
4. THE Bottom_Navigation SHALL highlight active screen with text-primary Tailwind class
5. THE Bottom_Navigation SHALL use text-on-surface-variant for inactive icons
6. THE Bottom_Navigation SHALL respect safe-area-inset-bottom via Tailwind pb-safe
7. WHEN user taps an icon, THE Bottom_Navigation SHALL navigate to corresponding screen

### Requirement 11: Lista Screen com Grid Layout

**User Story:** Como usuário, eu quero uma tela de lista organizada em grid, para que eu possa visualizar múltiplos itens simultaneamente.

#### Acceptance Criteria

1. THE UI_System SHALL display items in grid-cols-2 layout on desktop via Tailwind
2. THE UI_System SHALL display items in grid-cols-1 layout on mobile (sm: breakpoint)
3. THE UI_System SHALL display search bar at the top with p-4 Tailwind class
4. THE UI_System SHALL display horizontal filter chips using flex and gap-2 Tailwind classes
5. THE UI_System SHALL display item cards with monospace font for ID via font-mono class
6. THE UI_System SHALL use color-coded status badges (bg-green, bg-yellow, bg-red)
7. THE UI_System SHALL display pagination controls at the bottom
8. THE UI_System SHALL display FAB (Floating Action Button) styled with Tailwind

### Requirement 12: Detalhes do Bem Screen

**User Story:** Como usuário, eu quero uma tela de detalhes bem organizada, para que eu possa visualizar todas as informações do patrimônio.

#### Acceptance Criteria

1. THE UI_System SHALL display item details in grid-cols-2 layout on desktop via Tailwind
2. THE UI_System SHALL display item details in grid-cols-1 layout on mobile
3. THE UI_System SHALL display photo gallery in grid-cols-4 with gap-2 Tailwind class
4. THE UI_System SHALL display item information sections (Identificação, Localização, Estado, Valor)
5. THE UI_System SHALL display action buttons at the bottom using flex and gap-4
6. THE UI_System SHALL use font-mono Tailwind class for ID and code fields
7. THE UI_System SHALL use color-coded status badge matching Lista screen

### Requirement 13: Ajustes Screen com Seções Agrupadas

**User Story:** Como usuário, eu quero uma tela de ajustes organizada em seções, para que eu possa configurar o aplicativo facilmente.

#### Acceptance Criteria

1. THE UI_System SHALL display settings grouped in sections (Perfil, Câmera, Servidor, Sobre)
2. THE UI_System SHALL display section headers with mt-4 and mb-2 Tailwind classes
3. THE UI_System SHALL display custom toggle switches styled with Tailwind
4. THE UI_System SHALL display custom text inputs styled with Tailwind
5. THE UI_System SHALL display list items with chevron icon for navigation to sub-screens
6. THE UI_System SHALL use divide-y Tailwind class for dividers between settings
7. THE UI_System SHALL display version number and build info in Sobre section

### Requirement 14: Login Screen Centralizada

**User Story:** Como usuário, eu quero uma tela de login limpa e centralizada, para que eu possa acessar o aplicativo facilmente.

#### Acceptance Criteria

1. THE UI_System SHALL display login form using flex items-center justify-center h-screen
2. THE UI_System SHALL display application logo at the top with mb-8 Tailwind class
3. THE UI_System SHALL display username and password fields with gap-4 Tailwind class
4. THE UI_System SHALL display login button with bg-primary and w-full Tailwind classes
5. THE UI_System SHALL display link to settings at the bottom
6. THE UI_System SHALL use max-w-md Tailwind class for login container
7. THE UI_System SHALL apply backdrop-blur-lg to background if background image is present

### Requirement 15: Configuração de Sessão Screen

**User Story:** Como usuário, eu quero uma tela de configuração de sessão hierárquica, para que eu possa selecionar Órgão, UO, Área e Subárea facilmente.

#### Acceptance Criteria

1. THE UI_System SHALL display 4 select dropdowns in flex flex-col layout (Órgão, UO, Área, Subárea)
2. THE UI_System SHALL enable UO dropdown only after Órgão is selected
3. THE UI_System SHALL enable Área dropdown only after UO is selected
4. THE UI_System SHALL enable Subárea dropdown only after Área is selected
5. THE UI_System SHALL display "INICIAR SESSÃO" button at the bottom with bg-primary
6. THE UI_System SHALL display terminal status indicator at the top
7. THE UI_System SHALL use gap-4 Tailwind class between form fields

### Requirement 16: Material Symbols Icons

**User Story:** Como desenvolvedor, eu quero usar Material Symbols icons consistentemente, para que os ícones sigam o design system dos wireframes.

#### Acceptance Criteria

1. THE UI_System SHALL use Material Symbols icon font or SVG sprites
2. THE UI_System SHALL define icon sizes via Tailwind classes (size-4, size-6, size-8, size-12)
3. THE UI_System SHALL use outlined variant for navigation icons
4. THE UI_System SHALL use filled variant for active states
5. THE UI_System SHALL use consistent icon names across all screens
6. THE UI_System SHALL load Material Symbols from Google Fonts or local assets
7. WHEN an icon is rendered, THE UI_System SHALL apply correct size and color via Tailwind classes

### Requirement 17: Backdrop Blur Effects com Tailwind

**User Story:** Como usuário, eu quero efeitos visuais modernos como backdrop blur, para que a interface seja mais elegante.

#### Acceptance Criteria

1. THE UI_System SHALL apply backdrop-blur-sm to semi-transparent overlays via Tailwind
2. THE UI_System SHALL apply backdrop-blur-md to data feed section on scanner screen
3. THE UI_System SHALL apply backdrop-blur-md to bottom navigation bar
4. THE UI_System SHALL apply backdrop-blur-lg to modal dialogs
5. THE UI_System SHALL use bg-opacity-80 for semi-transparent backgrounds
6. THE UI_System SHALL provide fallback solid background for browsers without backdrop-filter support
7. WHEN a semi-transparent overlay is displayed, THE UI_System SHALL apply backdrop blur via Tailwind

### Requirement 18: Animações e Transições com Tailwind

**User Story:** Como usuário, eu quero animações suaves, para que a interface seja mais responsiva e agradável.

#### Acceptance Criteria

1. THE UI_System SHALL animate viewfinder corners with animate-pulse when scanner is active
2. THE UI_System SHALL animate scan line moving vertically within viewfinder via custom animation
3. THE UI_System SHALL animate success checkmark when code is detected
4. THE UI_System SHALL animate mode toggle selection with transition-colors duration-200
5. THE UI_System SHALL use transition-all duration-200 for interactive elements
6. THE UI_System SHALL use transition-all duration-300 for layout changes
7. WHEN user interacts with UI, THE UI_System SHALL provide visual feedback via Tailwind transitions

### Requirement 19: Responsividade Mobile-First com Tailwind

**User Story:** Como usuário mobile, eu quero que o aplicativo funcione perfeitamente no meu dispositivo, para que eu possa capturar fotos de patrimônio com facilidade.

#### Acceptance Criteria

1. THE UI_System SHALL use mobile-first responsive design via Tailwind breakpoints
2. WHEN the viewport width is less than 640px, THE UI_System SHALL display single-column layouts
3. WHEN the viewport width is between 640px and 1024px, THE UI_System SHALL use sm: and md: breakpoints
4. WHEN the viewport width is greater than 1024px, THE UI_System SHALL use lg: and xl: breakpoints
5. THE UI_System SHALL respect safe-area-inset via Tailwind safe-area plugins
6. THE UI_System SHALL ensure touch targets are at least 44x44 pixels (min-h-11 min-w-11)
7. THE UI_System SHALL use h-screen and w-screen for fullscreen scanner

### Requirement 20: Acessibilidade WCAG 2.1 AA

**User Story:** Como usuário com necessidades especiais, eu quero um aplicativo acessível, para que eu possa usar todas as funcionalidades sem barreiras.

#### Acceptance Criteria

1. THE UI_System SHALL ensure all text has contrast ratio of at least 4.5:1 for normal text
2. THE UI_System SHALL ensure all text has contrast ratio of at least 3:1 for large text (18px+)
3. THE UI_System SHALL ensure all interactive elements have appropriate ARIA labels
4. THE UI_System SHALL ensure focus states are clearly visible via focus:ring-2 Tailwind classes
5. THE UI_System SHALL ensure color is not the only means of conveying information
6. WHEN a user navigates with keyboard, THE UI_System SHALL provide logical tab order
7. THE UI_System SHALL provide text alternatives for all non-text content

### Requirement 21: Performance e Otimização com Tailwind

**User Story:** Como usuário, eu quero um aplicativo rápido e responsivo, para que eu possa trabalhar eficientemente.

#### Acceptance Criteria

1. THE UI_System SHALL use Tailwind JIT mode for minimal CSS bundle size
2. THE UI_System SHALL purge unused CSS classes in production build
3. THE UI_System SHALL avoid inline styles completely and use Tailwind utility classes
4. THE UI_System SHALL use Tailwind @layer directives for custom CSS organization
5. THE UI_System SHALL lazy load fonts to improve initial page load
6. THE UI_System SHALL use Tailwind container queries for component-level responsiveness
7. WHEN a page loads, THE UI_System SHALL render styles efficiently without layout shifts

### Requirement 22: Migração de Componentes MudBlazor para Blazor Puros

**User Story:** Como desenvolvedor, eu quero migrar todos os componentes MudBlazor para componentes Blazor puros, para que o aplicativo não tenha dependências de bibliotecas de UI externas.

#### Acceptance Criteria

1. THE UI_System SHALL replace MudButton with custom Button component styled with Tailwind
2. THE UI_System SHALL replace MudTextField with custom Input component styled with Tailwind
3. THE UI_System SHALL replace MudSelect with custom Select component styled with Tailwind
4. THE UI_System SHALL replace MudCard with custom Card component styled with Tailwind
5. THE UI_System SHALL replace MudBadge with custom Badge component styled with Tailwind
6. THE UI_System SHALL replace MudChip with custom Chip component styled with Tailwind
7. THE UI_System SHALL replace MudSwitch with custom Toggle component styled with Tailwind
8. WHEN all components are migrated, THE UI_System SHALL have zero MudBlazor dependencies

### Requirement 23: Configuração de Build com Tailwind

**User Story:** Como desenvolvedor, eu quero integrar Tailwind CSS no processo de build, para que o CSS seja gerado automaticamente.

#### Acceptance Criteria

1. THE UI_System SHALL have package.json with Tailwind CSS and dependencies
2. THE UI_System SHALL have tailwind.config.js with content paths for Razor files
3. THE UI_System SHALL have build script that runs Tailwind CLI before dotnet build
4. THE UI_System SHALL generate output.css file in wwwroot/css directory
5. THE UI_System SHALL reference output.css in index.html
6. THE UI_System SHALL use --minify flag for production builds
7. WHEN dotnet build is executed, THE UI_System SHALL generate Tailwind CSS automatically

### Requirement 24: Design Industrial Premium vs Material Design

**User Story:** Como designer, eu quero um Design Industrial Premium, para que o aplicativo tenha alta densidade de dados e não seja limitado por Material Design.

#### Acceptance Criteria

1. THE UI_System SHALL prioritize data density over whitespace
2. THE UI_System SHALL use compact layouts for information-heavy screens
3. THE UI_System SHALL use monospace fonts for technical data (IDs, códigos)
4. THE UI_System SHALL use high contrast colors for readability
5. THE UI_System SHALL avoid Material Design elevation and shadows where not needed
6. THE UI_System SHALL use sharp corners (rounded-sm) instead of heavily rounded corners
7. THE UI_System SHALL focus on functionality and efficiency over aesthetic softness
