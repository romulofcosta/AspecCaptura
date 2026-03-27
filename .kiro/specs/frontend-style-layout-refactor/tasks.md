# Implementation Plan: Frontend Style & Layout Refactor

## Overview

This implementation plan converts the frontend from MudBlazor to Tailwind CSS + Blazor WebAssembly pure components. The approach follows a phased strategy: (1) Remove MudBlazor dependencies, (2) Setup Tailwind CSS infrastructure, (3) Create reusable base components, (4) Redesign all 6 pages, (5) Implement property-based tests for correctness validation.

The implementation uses C# with Blazor WebAssembly and Tailwind CSS 3.x for styling. All components will be pure Blazor components without external UI library dependencies.

## Tasks

- [x] 1. Remove MudBlazor dependencies from project
  - Remove MudBlazor NuGet package from .csproj file
  - Remove MudBlazor service registrations from Program.cs
  - Remove MudBlazor CSS and JS references from wwwroot/index.html
  - _Requirements: 1.1, 1.4, 1.5, 1.6_

- [ ] 2. Setup Tailwind CSS infrastructure
  - [x] 2.1 Initialize npm and install Tailwind CSS
    - Create package.json with Tailwind CSS dependencies
    - Add build scripts for CSS generation (build:css, watch:css, build:css:prod)
    - Install @tailwindcss/forms plugin
    - _Requirements: 2.1, 2.5, 23.1_
  
  - [x] 2.2 Create Tailwind configuration file
    - Create tailwind.config.js with custom theme (colors, fonts, spacing, animations)
    - Configure content paths to scan all Razor files
    - Define Design Industrial Premium color palette (#00327d primary, surface variants)
    - Configure safe-area-inset spacing utilities
    - _Requirements: 2.2, 2.3, 4.1, 4.2, 4.3, 4.4, 4.5, 23.2_
  
  - [x] 2.3 Create Tailwind input CSS file
    - Create Styles/input.css with @tailwind directives
    - Add custom @layer base styles for typography
    - Add custom @layer components for reusable patterns
    - Add custom @layer utilities for safe-area classes
    - _Requirements: 21.4, 23.3_
  
  - [x] 2.4 Generate Tailwind output CSS and integrate into build
    - Run Tailwind CLI to generate wwwroot/css/output.css
    - Update wwwroot/index.html to reference output.css
    - Load Google Fonts (Space Grotesk, Inter, JetBrains Mono) in index.html
    - Verify project builds successfully with Tailwind CSS
    - _Requirements: 2.4, 2.7, 3.6, 23.4, 23.5, 23.6_

- [x] 3. Checkpoint - Verify Tailwind setup
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 4. Create reusable base components
  - [x] 4.1 Create Button component
    - Create Components/Base/Button.razor with variants (Filled, Outlined, Text)
    - Implement props: Text, Icon, Variant, Color, Size, Disabled, FullWidth, OnClick
    - Style with Tailwind classes (bg-primary, border-2, hover states, focus:ring-2)
    - Ensure minimum touch target size (min-h-11 min-w-11)
    - _Requirements: 6.2, 19.6, 20.4_
  
  - [x] 4.2 Create Input component
    - Create Components/Base/Input.razor with label and error message support
    - Implement props: Label, Placeholder, Value, ValueChanged, Type, Required, Disabled, ErrorMessage, Icon
    - Style with Tailwind classes (border-outline, focus:ring-primary, text-on-surface)
    - _Requirements: 6.3_
  
  - [x] 4.3 Create Select component
    - Create Components/Base/Select.razor with dropdown functionality
    - Implement props: Label, Value, ValueChanged, Options, Disabled, Placeholder
    - Style with Tailwind classes (appearance-none, custom dropdown icon)
    - _Requirements: 6.6_
  
  - [x] 4.4 Create Card component
    - Create Components/Base/Card.razor with Header, Content, Actions slots
    - Implement props: Header, ChildContent, Actions, Elevation, Clickable, OnClick
    - Style with Tailwind classes (bg-surface-container, shadow variants, rounded-lg)
    - _Requirements: 6.1_
  
  - [x] 4.5 Create Badge component
    - Create Components/Base/Badge.razor for status indicators
    - Implement props: Text, Color, Size, Dot
    - Style with Tailwind classes (rounded-full, color-coded backgrounds)
    - _Requirements: 6.4_
  
  - [x] 4.6 Create Chip component
    - Create Components/Base/Chip.razor for filter tags
    - Implement props: Text, Selected, Clickable, OnClick, Icon
    - Style with Tailwind classes (rounded-full, border-outline, selected state)
    - _Requirements: 6.5_
  
  - [x] 4.7 Create Toggle component
    - Create Components/Base/Toggle.razor for boolean settings
    - Implement props: Value, ValueChanged, Label, Disabled
    - Style with Tailwind classes (track, thumb, transition-transform)
    - Add aria-pressed attribute for accessibility
    - _Requirements: 6.7, 20.3_

- [ ]* 4.8 Write property test for base components
  - **Property 2: Tailwind Class Usage Consistency**
  - **Validates: Requirements 5.6, 21.3**

- [ ] 5. Create layout components
  - [x] 5.1 Create SafeAreaContainer component
    - Create Components/Layout/SafeAreaContainer.razor
    - Implement props: ChildContent, IncludeBottomNav
    - Apply safe-area-inset classes (pt-safe, pb-safe)
    - _Requirements: 19.5_
  
  - [x] 5.2 Create BottomNavigation component
    - Create Components/Layout/BottomNavigation.razor with 4 navigation items
    - Implement navigation items: Home, Lista, Scanner, Ajustes
    - Use Material Symbols icons (home, list, photo_camera, settings)
    - Style with Tailwind classes (backdrop-blur-md, text-primary for active)
    - Apply pb-safe for safe area respect
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 16.3, 16.4_
  
  - [x] 5.3 Create TopBar component
    - Create Components/Layout/TopBar.razor for page headers
    - Implement props: Title, ShowBackButton, Actions
    - Style with Tailwind classes (border-b border-outline, pt-safe)
    - _Requirements: 19.5_

- [ ]* 5.4 Write property test for safe area respect
  - **Property 4: Safe Area Respect**
  - **Validates: Requirements 19.5**

- [ ] 6. Create scanner-specific components
  - [x] 6.1 Create Viewfinder component
    - Create Components/Scanner/Viewfinder.razor with animated corners
    - Implement props: Active, Success
    - Style with Tailwind classes (border-2, animate-pulse, custom scan animation)
    - Add custom keyframes for scan line animation in tailwind.config.js
    - _Requirements: 7.3, 18.1, 18.2_
  
  - [x] 6.2 Create ModeToggle component
    - Create Components/Scanner/ModeToggle.razor for OCR/Barcode/QR selection
    - Implement props: SelectedMode, OnModeChanged
    - Style with Tailwind classes (rounded-full, bg-primary for active, backdrop-blur-sm)
    - _Requirements: 7.2, 18.4_
  
  - [x] 6.3 Create DataFeed component
    - Create Components/Scanner/DataFeed.razor for last capture info
    - Implement props: LastCapturedId, LastCapturedThumbnail, IsOnline, IsSynced
    - Style with Tailwind classes (backdrop-blur-md, font-mono for ID)
    - Display connection status with colored dot (bg-green-500/bg-red-500)
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

- [x] 7. Checkpoint - Verify all components created
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 8. Redesign Login page
  - [x] 8.1 Migrate Login.razor to Tailwind CSS
    - Remove all MudBlazor components (MudTextField, MudButton, MudCard)
    - Replace with custom Input and Button components
    - Apply Tailwind layout classes (flex items-center justify-center h-screen)
    - Use max-w-md for login container
    - _Requirements: 1.3, 14.1, 14.2, 14.3, 14.4, 14.5, 14.6, 22.1, 22.2_
  
  - [ ]* 8.2 Write unit tests for Login page
    - Test login form validation
    - Test button disabled state when fields are empty
    - _Requirements: 14.1, 14.4_

- [ ] 9. Redesign ConfiguracaoSessao page
  - [x] 9.1 Migrate ConfiguracaoSessao.razor to Tailwind CSS
    - Remove all MudBlazor components (MudSelect, MudButton)
    - Replace with custom Select and Button components
    - Implement hierarchical dropdown enabling (Órgão → UO → Área → Subárea)
    - Apply Tailwind layout classes (flex flex-col gap-4)
    - Add TopBar and BottomNavigation components
    - _Requirements: 1.3, 15.1, 15.2, 15.3, 15.4, 15.5, 15.6, 15.7, 22.3_
  
  - [ ]* 9.2 Write unit tests for ConfiguracaoSessao page
    - Test dropdown enabling logic
    - Test button enabled only when all fields selected
    - _Requirements: 15.2, 15.3, 15.4, 15.5_

- [ ] 10. Redesign Camera page (Scanner screen)
  - [x] 10.1 Migrate Camera.razor to Tailwind CSS
    - Remove all MudBlazor components (MudBadge, MudIconButton)
    - Replace with Viewfinder, ModeToggle, and DataFeed components
    - Apply fullscreen layout (h-screen w-screen)
    - Position controls with Tailwind absolute positioning
    - Add flash toggle and zoom slider on right side
    - Style with backdrop-blur-md for overlays
    - _Requirements: 1.3, 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 17.2, 19.7, 22.1_
  
  - [ ]* 10.2 Write unit tests for Camera page
    - Test mode toggle selection
    - Test flash toggle functionality
    - Test zoom slider updates
    - _Requirements: 7.7, 8.5, 8.6_

- [ ] 11. Redesign Items page (Lista screen)
  - [x] 11.1 Migrate Items.razor to Tailwind CSS
    - Remove all MudBlazor components (MudCard, MudChip, MudTextField)
    - Replace with custom Card, Chip, and Input components
    - Implement responsive grid layout (grid-cols-1 sm:grid-cols-2 lg:grid-cols-3)
    - Add search bar and filter chips
    - Style item cards with monospace font for IDs (font-mono)
    - Add color-coded status badges
    - _Requirements: 1.3, 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7, 11.8, 19.2, 19.3, 19.4, 22.2, 22.4, 22.5_
  
  - [ ]* 11.2 Write property test for responsive grid
    - **Property 3: Mobile-First Responsive Grid**
    - **Validates: Requirements 19.2, 19.3, 19.4**

- [ ] 12. Redesign ItemDetails page (Detalhes screen)
  - [x] 12.1 Migrate ItemDetails.razor to Tailwind CSS
    - Remove all MudBlazor components (MudCard, MudButton, MudBadge)
    - Replace with custom Card, Button, and Badge components
    - Implement photo gallery with grid-cols-4 layout
    - Organize information in sections (Identificação, Localização, Estado, Valor)
    - Use font-mono for ID and code fields
    - Apply responsive grid (grid-cols-1 sm:grid-cols-2)
    - _Requirements: 1.3, 12.1, 12.2, 12.3, 12.4, 12.5, 12.6, 12.7, 22.1, 22.4_
  
  - [ ]* 12.2 Write unit tests for ItemDetails page
    - Test photo gallery navigation
    - Test action buttons (Editar, Excluir)
    - _Requirements: 12.3, 12.5_

- [ ] 13. Redesign Settings page (Ajustes screen)
  - [x] 13.1 Migrate Settings.razor to Tailwind CSS
    - Remove all MudBlazor components (MudSwitch, MudTextField, MudList)
    - Replace with custom Toggle and Input components
    - Organize settings in sections (Perfil, Câmera, Servidor, Sobre)
    - Style with Tailwind classes (divide-y, border-outline)
    - Add chevron icons for navigation items
    - _Requirements: 1.3, 13.1, 13.2, 13.3, 13.4, 13.5, 13.6, 13.7, 22.3, 22.7_
  
  - [ ]* 13.2 Write unit tests for Settings page
    - Test toggle switches
    - Test navigation to sub-screens
    - _Requirements: 13.3, 13.5_

- [x] 14. Checkpoint - Verify all pages redesigned
  - Ensure all tests pass, ask the user if questions arise.

- [ ]* 15. Implement property-based tests for correctness properties
  - [ ]* 15.1 Write property test for MudBlazor removal
    - **Property 1: Complete MudBlazor Removal**
    - **Validates: Requirements 1.2, 1.3, 6.8**
  
  - [ ]* 15.2 Write property test for touch target size
    - **Property 5: Touch Target Minimum Size**
    - **Validates: Requirements 19.6**
  
  - [ ]* 15.3 Write property test for color contrast
    - **Property 6: Color Contrast Compliance**
    - **Validates: Requirements 20.1, 20.2**
  
  - [ ]* 15.4 Write property test for interactive element accessibility
    - **Property 7: Interactive Element Accessibility**
    - **Validates: Requirements 20.3**
  
  - [ ]* 15.5 Write property test for focus state visibility
    - **Property 8: Focus State Visibility**
    - **Validates: Requirements 20.4**
  
  - [ ]* 15.6 Write property test for text alternatives
    - **Property 9: Text Alternatives for Non-Text Content**
    - **Validates: Requirements 20.7**
  
  - [ ]* 15.7 Write property test for logical tab order
    - **Property 10: Logical Tab Order**
    - **Validates: Requirements 20.6**

- [ ] 16. Final integration and verification
  - [x] 16.1 Verify build process integration
    - Ensure npm run build:css generates output.css correctly
    - Ensure dotnet build compiles without MudBlazor errors
    - Verify production build with minified CSS (build:css:prod)
    - _Requirements: 1.7, 2.7, 23.7_
  
  - [x] 16.2 Update MainLayout and routing
    - Update Layouts/MainLayout.razor to use SafeAreaContainer and BottomNavigation
    - Ensure all pages use correct layout (MainLayout vs AuthMinimalLayout)
    - Verify routing works correctly with new components
    - _Requirements: 10.2_
  
  - [x] 16.3 Verify Material Symbols icons integration
    - Ensure Material Symbols font is loaded in index.html
    - Verify all icons render correctly across all pages
    - Check icon sizes are consistent (size-4, size-6, size-8)
    - _Requirements: 16.1, 16.2, 16.6, 16.7_
  
  - [x] 16.4 Final accessibility verification
    - Run browser DevTools accessibility checker on all pages
    - Verify keyboard navigation works on all interactive elements
    - Test with screen reader (manual testing required)
    - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.6, 20.7_

- [x] 17. Final checkpoint - Complete verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation at key milestones
- Property tests validate universal correctness properties across all components
- Unit tests validate specific examples and edge cases
- The implementation maintains all existing services (ICameraService, IRecognitionService, IAuthService, IIndexedDbService, AppState) without modification
- Focus is exclusively on UI layer refactoring - no business logic changes
