# Implementation Plan: PWA Mobile Design System Refactor

## Overview

Este plano de implementação detalha as tarefas para refatorar completamente o design system, UI/UX e responsividade mobile do AspecCaptura PWA. A implementação segue uma abordagem mobile-first com foco especial na tela de listagem de bens (Bens.razor), utilizando design tokens, componentes reutilizáveis com CSS isolation, e padrões de acessibilidade WCAG 2.1 AA.

## Tasks

- [ ] 1. Setup design token system and theme provider
  - [ ] 1.1 Create design tokens CSS file with all variables
    - Create `wwwroot/css/design-tokens.css` with CSS Custom Properties
    - Define color tokens (primary, secondary, neutral, success, warning, error, info) with light/dark variants
    - Define typography tokens (font families, sizes 12px-48px, weights 400-700, line heights)
    - Define spacing tokens using 4px base scale (4, 8, 12, 16, 24, 32, 48, 64, 96px)
    - Define shadow tokens for elevation levels (none, sm, md, lg, xl)
    - Define border radius tokens (none, sm: 4px, md: 8px, lg: 12px, xl: 16px, full: 9999px)
    - Define breakpoint tokens (mobile: 320px, tablet: 768px, desktop: 1024px, wide: 1440px)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7_

  - [ ] 1.2 Implement ThemeProvider component
    - Create `Components/Theme/ThemeProvider.razor` and `ThemeProvider.razor.css`
    - Implement theme state management (light/dark/system)
    - Add system color scheme detection via JS interop
    - Implement theme persistence to localStorage
    - Add CascadingValue to provide theme context to child components
    - Implement smooth theme transitions (200ms)
    - _Requirements: 1.8, 9.1, 9.2, 9.3, 9.4, 9.7_

  - [ ] 1.3 Create theme switching JavaScript interop
    - Create `wwwroot/js/theme.js` with functions for theme detection and application
    - Implement `detectSystemTheme()` function
    - Implement `applyTheme(theme)` function to update DOM data-theme attribute
    - Implement `getStoredTheme()` and `setStoredTheme(theme)` for localStorage
    - _Requirements: 9.1, 9.3_

  - [ ] 1.4 Define dark mode color palette
    - Add `[data-theme="dark"]` selector in design-tokens.css
    - Define dark mode color overrides with appropriate contrast ratios
    - Ensure semantic color tokens adapt to theme (background, surface, text-primary, text-secondary)
    - _Requirements: 9.5, 9.8_

- [ ] 2. Create base component library structure
  - [ ] 2.1 Setup component directory structure
    - Create folder structure: `Components/Base/`, `Components/Layout/`, `Components/Navigation/`, `Components/Forms/`, `Components/Feedback/`
    - Create README.md in Components folder documenting component library architecture
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

  - [ ] 2.2 Implement Button component
    - Create `Components/Base/Button.razor` and `Button.razor.css`
    - Add Parameters: Variant (primary/secondary/outline/ghost), Size (sm/md/lg), Disabled, FullWidth, Icon, AriaLabel
    - Implement CSS classes using design tokens
    - Add ripple effect for touch interactions
    - Ensure minimum 44x44px touch target
    - _Requirements: 2.1, 2.6, 2.7, 3.3, 4.5, 8.1, 8.4_

  - [ ]* 2.3 Write unit tests for Button component
    - Test variant rendering (primary, secondary, outline, ghost)
    - Test size rendering (sm, md, lg)
    - Test disabled state
    - Test full-width rendering
    - Test icon rendering
    - Test ARIA label attribute
    - _Requirements: 2.1, 8.1_

  - [ ] 2.4 Implement Icon component
    - Create `Components/Base/Icon.razor` and `Icon.razor.css`
    - Add Parameters: Name, Size (sm: 16px, md: 20px, lg: 24px, xl: 32px), Color
    - Use Material Symbols icon library
    - Support filled and outlined variants
    - Default size 24px, inherit color from parent
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.8_

  - [ ] 2.5 Implement Card component
    - Create `Components/Base/Card.razor` and `Card.razor.css`
    - Add Parameters: Elevation (flat/raised/elevated), Interactive, Layout (horizontal/vertical)
    - Support header, body, footer sections via RenderFragments
    - Use 16px padding for content areas, 12px border radius
    - Add ripple effect for interactive variant
    - _Requirements: 2.1, 16.1, 16.2, 16.3, 16.4, 16.6, 16.7, 16.8_

  - [ ] 2.6 Implement Badge component
    - Create `Components/Base/Badge.razor` and `Badge.razor.css`
    - Add Parameters: Variant (primary/success/warning/error/neutral), Size (sm/md/lg), Dot, Position
    - Use semantic colors and full border radius for pill shape
    - Support dot variant for notification indicators
    - _Requirements: 2.1, 21.1, 21.2, 21.3, 21.4, 21.7, 21.8_


- [ ] 3. Implement layout components
  - [ ] 3.1 Create Container component
    - Create `Components/Layout/Container.razor` and `Container.razor.css`
    - Add Parameters: MaxWidth, Padding, Centered
    - Use responsive padding with design tokens
    - Respect safe area insets on devices with notches
    - _Requirements: 2.2, 3.4_

  - [ ] 3.2 Create Grid component
    - Create `Components/Layout/Grid.razor` and `Grid.razor.css`
    - Add Parameters: Columns (responsive: 1 mobile, 2-4 desktop), Gap, AlignItems, JustifyContent
    - Use CSS Grid with design token gaps
    - Implement responsive column behavior with breakpoints
    - _Requirements: 2.2, 3.2, 18.2, 18.6_

  - [ ] 3.3 Create Stack component
    - Create `Components/Layout/Stack.razor` and `Stack.razor.css`
    - Add Parameters: Direction (row/column), Gap, Wrap, AlignItems, JustifyContent
    - Use Flexbox with design token spacing
    - Support responsive direction changes
    - _Requirements: 2.2, 3.2_

  - [ ] 3.4 Create Spacer and Divider components
    - Create `Components/Layout/Spacer.razor` with Size parameter
    - Create `Components/Layout/Divider.razor` and `Divider.razor.css`
    - Spacer uses design token spacing values
    - Divider uses 1px border with theme-aware color
    - _Requirements: 2.2, 18.5_

- [ ] 4. Implement form components
  - [ ] 4.1 Create TextField component
    - Create `Components/Forms/TextField.razor` and `TextField.razor.css`
    - Add Parameters: Label, Value, Type, Placeholder, Disabled, Error, HelperText, Required, Autocomplete
    - Implement floating label that moves above input when focused or filled
    - Add inline validation feedback
    - Display error message below field with error icon
    - Provide clear button (X icon) for text inputs with content
    - Use appropriate input types (tel, email, number, date) for mobile keyboards
    - _Requirements: 2.4, 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.10_

  - [ ] 4.2 Create SearchBar component
    - Create `Components/Forms/SearchBar.razor` and `SearchBar.razor.css`
    - Add Parameters: Placeholder, Value, OnSearch, DebounceMs (default 300)
    - Display search icon on left side
    - Implement debounced search input
    - Add clear button functionality
    - _Requirements: 2.4, 20.1, 20.2, 20.4_

  - [ ]* 4.3 Write unit tests for SearchBar debouncing
    - Test debounce timer functionality (300ms delay)
    - Test clear button removes value
    - Test OnSearch callback is invoked with correct value
    - _Requirements: 20.4_

  - [ ] 4.4 Create FilterChip component
    - Create `Components/Forms/FilterChip.razor` and `FilterChip.razor.css`
    - Add Parameters: Label, Active, Removable, OnClick, OnRemove
    - Use pill shape (border-radius: 999px)
    - Padding: 8px vertical, 12px horizontal
    - Background: #EEF2FF (inactive), #2F6FED (active)
    - Display remove button (X icon) when active and removable
    - Add aria-pressed attribute for accessibility
    - _Requirements: 2.4, 20.5, 20.6, 26.5, 26.16_

  - [ ] 4.5 Create Checkbox, Radio, and Switch components
    - Create `Components/Forms/Checkbox.razor` and `Checkbox.razor.css`
    - Create `Components/Forms/Radio.razor` and `Radio.razor.css`
    - Create `Components/Forms/Switch.razor` and `Switch.razor.css`
    - Ensure minimum 44x44px touch targets
    - Add proper ARIA labels and roles
    - Use design tokens for colors and spacing
    - _Requirements: 2.4, 3.3, 8.1, 8.4_

- [ ] 5. Implement feedback components
  - [ ] 5.1 Create Spinner component
    - Create `Components/Base/Spinner.razor` and `Spinner.razor.css`
    - Add Parameters: Size (sm/md/lg), Color
    - Use CSS animation with 60fps performance (transform/opacity)
    - Respect prefers-reduced-motion media query
    - _Requirements: 2.1, 10.7, 13.1_

  - [ ] 5.2 Create Skeleton component
    - Create `Components/Feedback/Skeleton.razor` and `Skeleton.razor.css`
    - Add Parameters: Shape (text/circle/rectangle), Width, Height, Count
    - Implement shimmer animation (left to right, 1.5s duration)
    - Use neutral gray color that adapts to theme
    - Fade out with 200ms transition when content loads
    - Disable animation when prefers-reduced-motion is enabled
    - _Requirements: 2.5, 10.8, 13.2, 22.1, 22.2, 22.3, 22.4, 22.6, 22.7, 22.8_

  - [ ] 5.3 Create EmptyState component
    - Create `Components/Feedback/EmptyState.razor` and `EmptyState.razor.css`
    - Add Parameters: Icon, Title, Subtitle
    - Center content with icon (64px), title (18px, font-weight 600), subtitle (14px, secondary color)
    - NO action button (per Requirement 26.3, 26.7)
    - _Requirements: 2.5, 13.7, 18.8, 26.7, 26.19_

  - [ ] 5.4 Create Toast component
    - Create `Components/Feedback/Toast.razor` and `Toast.razor.css`
    - Add Parameters: Variant (success/error/warning/info), Message, Duration, OnClose, ActionLabel, OnAction
    - Display at top-center on mobile, top-right on desktop
    - Auto-dismiss after 3s (success) or 5s (error)
    - Support manual dismiss with close button
    - Stack multiple toasts with 8px gap (max 3 visible)
    - Animate entrance from top with slide-down effect
    - Add role="alert" for screen reader announcements
    - Pause auto-dismiss on hover/focus
    - _Requirements: 2.5, 13.4, 13.5, 13.6, 23.1, 23.2, 23.3, 23.4, 23.5, 23.6, 23.7, 23.8, 23.9, 23.10_

  - [ ] 5.5 Create Alert component
    - Create `Components/Feedback/Alert.razor` and `Alert.razor.css`
    - Add Parameters: Variant (success/error/warning/info), Title, Message, Dismissible
    - Use semantic colors with proper contrast ratios
    - Add icon based on variant
    - _Requirements: 2.5, 8.2, 8.3_

  - [ ] 5.6 Create ProgressBar component
    - Create `Components/Feedback/ProgressBar.razor` and `ProgressBar.razor.css`
    - Add Parameters: Value (0-100), Indeterminate, Size, Color
    - Use smooth CSS transitions
    - Support indeterminate state for unknown duration
    - _Requirements: 2.5, 13.3_

- [ ] 6. Implement Modal and BottomSheet components
  - [ ] 6.1 Create Modal component
    - Create `Components/Feedback/Modal.razor` and `Modal.razor.css`
    - Add Parameters: IsOpen, Title, OnClose, CloseOnBackdrop, ChildContent
    - Full-screen on mobile, centered dialog on desktop (min-width: 768px)
    - Trap keyboard focus within modal when open
    - Close on Escape key press
    - Close on backdrop click (unless CloseOnBackdrop is false)
    - Prevent body scroll when open
    - Animate entrance from bottom on mobile, fade-in on desktop
    - Add role="dialog" and proper ARIA attributes
    - _Requirements: 2.5, 17.1, 17.2, 17.4, 17.5, 17.6, 17.7, 17.8, 17.9, 17.10_

  - [ ] 6.2 Add swipe-down gesture to dismiss Modal on mobile
    - Implement touch event handlers for swipe detection
    - Calculate swipe distance and velocity
    - Dismiss modal when swipe-down threshold is met
    - _Requirements: 17.3, 4.1_

  - [ ] 6.3 Create BottomSheet component
    - Create `Components/Feedback/BottomSheet.razor` and `BottomSheet.razor.css`
    - Add Parameters: IsOpen, OnClose, SnapPoints (collapsed/half/full), ChildContent
    - Slide up from bottom with drag handle
    - Support swipe-down gesture to dismiss
    - Dim background with overlay when open
    - Prevent body scroll when open
    - Animate entrance (300ms ease-out) and exit (200ms ease-in)
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6, 19.7, 19.8_

- [ ] 7. Implement navigation components
  - [ ] 7.1 Create TopBar component
    - Create `Components/Navigation/TopBar.razor` and `TopBar.razor.css`
    - Add Parameters: Title, ShowBackButton, OnBackClick, Actions (RenderFragment)
    - Use design tokens for height, padding, colors
    - Support action buttons on right side
    - _Requirements: 2.3_

  - [ ] 7.2 Create BottomNav component
    - Create `Components/Navigation/BottomNav.razor` and `BottomNav.razor.css`
    - Add Parameters: Items (list of nav items), ActiveItem, OnItemClick
    - Display on mobile (below 768px), hide on desktop
    - Maximum 5 primary navigation items
    - Highlight active item with visual indicator
    - Ensure minimum 44x44px touch targets
    - Support keyboard navigation (Tab, Arrow keys)
    - Announce navigation changes to screen readers
    - Provide haptic feedback on tap (where supported)
    - _Requirements: 2.3, 5.1, 5.2, 5.3, 5.6, 5.7, 5.8, 8.4_

  - [ ] 7.3 Create Drawer component for secondary navigation
    - Create `Components/Navigation/Drawer.razor` and `Drawer.razor.css`
    - Add Parameters: IsOpen, OnClose, Position (left/right), ChildContent
    - Slide in from side with overlay
    - Close on backdrop click or Escape key
    - Trap focus within drawer when open
    - _Requirements: 2.3, 5.5_

- [ ] 8. Checkpoint - Ensure all tests pass
  - Run all unit tests for components created so far
  - Verify components render correctly in both light and dark themes
  - Test keyboard navigation and screen reader announcements
  - Ensure all touch targets meet 44x44px minimum
  - Ask the user if questions arise

- [x] 9. Refactor Asset Listing Screen (Bens.razor)
  - [x] 9.1 Create new Bens.razor layout structure
    - Remove existing layout and styles
    - Implement new layout order: Header, SearchBar, Primary CTA, Filters, Summary, Content/EmptyState
    - Use Container component with consistent 16px horizontal padding
    - Remove floating action button (FAB) pattern
    - Remove duplicate CTA buttons from empty state
    - _Requirements: 26.1, 26.2, 26.3, 26.4, 26.23_

  - [x] 9.2 Implement header section
    - Use TopBar component with title "Aspec Captura"
    - Keep header simple and lightweight
    - _Requirements: 26.4_

  - [x] 9.3 Implement search field
    - Use SearchBar component with placeholder "Buscar bem ou descrição..."
    - Bind to searchQuery state variable
    - Implement HandleSearch method with 300ms debounce
    - Use 12px padding, rounded corners (--radius-md), minimal border
    - _Requirements: 26.4, 26.15, 26.22_

  - [x] 9.4 Implement primary CTA button
    - Use Button component with variant="primary", fullWidth="true"
    - Text: "ESCANEAR BEM"
    - Icon: "qr_code_scanner"
    - Position at top after search field (NOT floating)
    - Use 14px padding, rounded corners (--radius-lg), subtle shadow
    - Implement ripple effect on interaction
    - Navigate to scanner on click
    - _Requirements: 26.1, 26.2, 26.4, 26.14, 26.20_

  - [x] 9.5 Implement filter chips section
    - Use Stack component with direction="row", gap="2", wrap="true"
    - Render FilterChip components for each active filter
    - Implement ToggleFilter method
    - Use design tokens: padding 8px/12px, pill shape, light background (#EEF2FF)
    - Display active chips with visual distinction
    - _Requirements: 26.4, 26.5, 26.16, 26.17_

  - [x] 9.6 Implement results summary bar
    - Display inline without heavy container blocks
    - Format: "{count} BENS | {currentUO}"
    - Use typography scale: 14px, secondary color
    - Update dynamically when filters change
    - _Requirements: 26.6, 26.21_

  - [x] 9.7 Implement content area with EmptyState
    - Use EmptyState component when no items found
    - Icon: "📦", Title: "Nenhum bem encontrado", Subtitle: "Tente outro filtro ou termo de busca"
    - NO action button in empty state
    - Use Skeleton component while loading
    - Render Card components for each item when data exists
    - _Requirements: 26.7, 26.19_

  - [x] 9.8 Apply design tokens and visual styling
    - Use defined color palette: primary (#2F6FED), bg (#F8FAFC), surface (#FFFFFF), border (#E5E7EB), text (#111827), text-secondary (#6B7280)
    - Use spacing tokens: --space-2 (8px), --space-3 (12px), --space-4 (16px), --space-5 (20px)
    - Use border radius tokens: --radius-md (12px), --radius-lg (16px)
    - Use spacing (not borders) to separate sections visually
    - Reduce visual weight by minimizing background colors and unnecessary containers
    - _Requirements: 26.8, 26.10, 26.11, 26.12, 26.13_

  - [x] 9.9 Optimize for one-handed mobile use
    - Ensure primary action is easily reachable at top
    - Maintain clear visual hierarchy
    - Prioritize scannable content
    - Avoid generic template appearance with custom spacing
    - _Requirements: 26.9, 26.24, 26.25_

  - [ ]* 9.10 Write integration tests for Bens.razor
    - Test empty state rendering
    - Test search functionality with debouncing
    - Test filter chip interactions
    - Test results summary updates
    - Test navigation to scanner
    - _Requirements: 26.1-26.25_

- [x] 10. Implement responsive layout system
  - [x] 10.1 Create responsive utility classes
    - Create `wwwroot/css/responsive.css` with mobile-first media queries
    - Define utility classes for responsive display, spacing, typography
    - Use min-width media queries (mobile-first approach)
    - _Requirements: 3.1_

  - [x] 10.2 Implement fluid typography
    - Add CSS clamp() functions for font sizes that scale between breakpoints
    - Update typography tokens to use relative units (rem)
    - _Requirements: 3.8_

  - [x] 10.3 Update BottomNav to hide on desktop
    - Add media query to hide BottomNav when viewport width >= 768px
    - Show sidebar navigation on desktop (if applicable)
    - _Requirements: 5.4_

  - [x] 10.4 Test responsive layouts on multiple devices
    - Test on mobile (320px-767px)
    - Test on tablet (768px-1023px)
    - Test on desktop (1024px+)
    - Verify single-column on mobile, multi-column on desktop
    - _Requirements: 3.5, 3.6_

- [x] 11. Implement touch interactions and gestures
  - [x] 11.1 Create gesture handler utility
    - Create `wwwroot/js/gestures.js` with touch event handlers
    - Implement swipe detection (left, right, up, down)
    - Implement long-press detection
    - Implement pinch-to-zoom detection
    - Calculate velocity and distance for gestures
    - _Requirements: 4.1, 4.3, 4.7_

  - [x] 11.2 Add ripple effect to interactive components
    - Create `wwwroot/css/ripple.css` with ripple animation
    - Add ripple effect to Button component
    - Add ripple effect to Card interactive variant
    - Ensure visual feedback within 100ms
    - _Requirements: 4.4, 4.5_

  - [x] 11.3 Implement pull-to-refresh gesture
    - Add pull-to-refresh to scrollable lists
    - Display spinner during refresh
    - Trigger data reload on pull
    - _Requirements: 4.2, 13.8_

  - [x] 11.4 Prevent double-tap zoom on interactive elements
    - Add touch-action: manipulation CSS to interactive elements
    - Prevent default browser behaviors that conflict with app navigation
    - _Requirements: 4.6, 4.8_

- [x] 12. Implement animations and transitions
  - [x] 12.1 Create animation utility classes
    - Create `wwwroot/css/animations.css` with reusable animations
    - Define entrance animations (fade-in, slide-in)
    - Define exit animations (fade-out, slide-out)
    - Use duration 150-300ms for micro-interactions, 300-500ms for page transitions
    - Use easing functions (ease-out for entrances, ease-in for exits)
    - _Requirements: 10.1, 10.2, 10.3, 10.4_

  - [x] 12.2 Add prefers-reduced-motion support
    - Add media query to disable non-essential animations
    - Update Skeleton, Spinner, and other animated components
    - Ensure critical functionality works without animations
    - _Requirements: 10.5, 10.6_

  - [x] 12.3 Optimize animations for 60fps
    - Use transform and opacity for animations (GPU acceleration)
    - Avoid animating layout properties (width, height, margin, padding)
    - Test animation performance on low-end devices
    - _Requirements: 10.7_

- [x] 13. Implement PWA features and offline support
  - [x] 13.1 Update service worker for caching
    - Update `wwwroot/service-worker.js` to cache application shell
    - Cache static assets (CSS, JS, images, fonts)
    - Implement cache-first strategy for static assets
    - Implement network-first strategy for API calls with cache fallback
    - _Requirements: 7.1, 7.2, 7.3_

  - [x] 13.2 Add offline indicator UI
    - Create offline indicator component
    - Display when network is unavailable
    - Update indicator when connection is restored
    - _Requirements: 7.4_

  - [x] 13.3 Implement offline action queue
    - Create queue for user actions when offline
    - Store queued actions in IndexedDB
    - Sync queued actions when connection is restored
    - Provide feedback about sync status
    - _Requirements: 7.5, 7.7, 7.8_

  - [x] 13.4 Cache recently viewed data
    - Implement IndexedDB storage for recently viewed items
    - Cache data after successful API calls
    - Serve cached data when offline
    - _Requirements: 7.6_

  - [x] 13.5 Update web app manifest
    - Update `wwwroot/manifest.json` with app name, icons, theme color
    - Set display mode to "standalone"
    - Add icons in sizes 192x192 and 512x512
    - Add maskable icon for adaptive icon support
    - Define splash screen with app icon and brand color
    - Add app shortcuts for quick access to key features
    - _Requirements: 25.1, 25.4, 25.5, 25.7, 25.10_

  - [x] 13.6 Implement install prompt
    - Detect when PWA criteria are met
    - Display install prompt to user
    - Handle beforeinstallprompt event
    - _Requirements: 25.3_

  - [x] 13.7 Add theme-color meta tag
    - Update theme-color meta tag to adapt to current theme
    - Use primary color for light theme, dark surface color for dark theme
    - _Requirements: 25.6_

- [x] 14. Optimize performance
  - [x] 14.1 Implement lazy loading for components
    - Use Blazor's lazy loading for components not immediately visible
    - Lazy load images below the fold
    - _Requirements: 6.5, 12.2_

  - [x] 14.2 Optimize images
    - Convert images to WebP format with JPEG fallback
    - Compress images to maximum 1920px width
    - Implement responsive images with srcset
    - Add blur-up placeholder effect
    - _Requirements: 6.7, 12.5, 12.6_

  - [x] 14.3 Implement critical CSS inlining
    - Extract critical above-the-fold CSS
    - Inline critical CSS in index.html
    - Defer non-critical CSS loading
    - _Requirements: 6.8_

  - [x] 14.4 Minimize JavaScript bundle size
    - Analyze bundle size with webpack-bundle-analyzer or equivalent
    - Remove unused dependencies
    - Enable tree-shaking and minification
    - Target bundle size under 500KB gzipped
    - _Requirements: 6.9_

  - [x] 14.5 Add CSS containment for performance isolation
    - Add contain: layout style to components
    - Use content-visibility: auto for off-screen content
    - _Requirements: 6.6_

  - [ ]* 14.6 Run Lighthouse performance audit
    - Test on 3G throttled connection
    - Verify FCP < 1.8s, LCP < 2.5s, TTI < 3.8s, CLS < 0.1
    - Verify Performance score >= 85
    - _Requirements: 6.1, 6.2, 6.3, 6.4_

- [ ] 15. Accessibility audit and improvements
  - [ ] 15.1 Run automated accessibility tests
    - Use axe-core or Lighthouse accessibility audit
    - Fix all critical and serious issues
    - Target Accessibility score >= 95
    - _Requirements: 8.1-8.10_

  - [ ] 15.2 Verify color contrast ratios
    - Test all text against backgrounds (4.5:1 for normal, 3:1 for large/UI)
    - Test in both light and dark themes
    - Use contrast checker tools
    - _Requirements: 8.2, 8.3_

  - [ ] 15.3 Test keyboard navigation
    - Verify all interactive elements are keyboard accessible
    - Test Tab order is logical
    - Verify focus indicators are visible (2px outline)
    - Test Escape key closes modals/drawers
    - _Requirements: 8.4, 8.5_

  - [ ] 15.4 Test with screen readers
    - Test with NVDA (Windows), JAWS (Windows), VoiceOver (macOS/iOS)
    - Verify ARIA labels are present and correct
    - Verify dynamic content changes are announced
    - Verify semantic HTML is used correctly
    - _Requirements: 8.1, 8.6, 8.9_

  - [ ] 15.5 Test text resize up to 200%
    - Verify functionality is maintained at 200% zoom
    - Verify no content is cut off or overlapping
    - _Requirements: 8.8_

  - [ ] 15.6 Verify error messages are clear
    - Test all error states provide clear messages
    - Verify error messages include suggestions for resolution
    - _Requirements: 8.10_

- [ ] 16. Progressive enhancement and fallbacks
  - [ ] 16.1 Add noscript fallback
    - Create noscript message with instructions
    - Provide fallback styles for critical UI
    - _Requirements: 24.5_

  - [ ] 16.2 Implement error boundary
    - Create error boundary component
    - Display recovery options when JavaScript errors occur
    - Log errors to console or monitoring service
    - _Requirements: 24.4_

  - [ ] 16.3 Add print stylesheet
    - Create `wwwroot/css/print.css` for document printing
    - Hide navigation and interactive elements
    - Optimize layout for print
    - _Requirements: 24.8_

  - [ ] 16.4 Ensure semantic HTML
    - Verify use of nav, main, article, section, header, footer elements
    - Ensure critical navigation works with standard HTML links
    - _Requirements: 8.9, 24.7_

- [ ] 17. Final polish and testing
  - [ ] 17.1 Cross-browser testing
    - Test on Chrome (desktop + mobile)
    - Test on Safari (desktop + iOS)
    - Test on Firefox (desktop + mobile)
    - Test on Edge (desktop)
    - Fix any browser-specific issues
    - _Requirements: All_

  - [ ] 17.2 Dark mode refinement
    - Review all components in dark mode
    - Adjust colors for better contrast if needed
    - Verify smooth theme transitions
    - _Requirements: 9.1-9.8_

  - [ ] 17.3 Animation polish
    - Review all animations for smoothness
    - Adjust timing and easing if needed
    - Verify 60fps performance
    - _Requirements: 10.1-10.7_

  - [ ] 17.4 Touch interaction testing on real devices
    - Test on iOS devices (iPhone)
    - Test on Android devices (various manufacturers)
    - Verify touch targets are comfortable
    - Verify gestures work smoothly
    - _Requirements: 3.3, 4.1-4.8_

  - [ ]* 17.5 Run final Lighthouse audit
    - Verify PWA score >= 90
    - Verify Performance score >= 85
    - Verify Accessibility score >= 95
    - Document any remaining issues
    - _Requirements: All_

- [ ] 18. Final checkpoint - Ensure all tests pass
  - Run all unit tests and integration tests
  - Verify all acceptance criteria are met
  - Review code for consistency and best practices
  - Update documentation if needed
  - Ask the user if questions arise

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Focus on mobile-first approach throughout implementation
- Prioritize accessibility and performance at every step
- Test on real devices frequently, not just browser DevTools
- The Asset Listing Screen (Bens.razor) is the primary deliverable and showcase of the new design system
