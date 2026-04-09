# Design Document: PWA Mobile Design System Refactor

## Overview

This design document specifies the technical architecture for refactoring the AspecCaptura PWA's design system, UI/UX, and mobile responsiveness. The refactor transforms the application from its current state into a mobile-first, professionally designed Progressive Web Application with a comprehensive design token system, reusable component library, and optimized user experience.

### Goals

- Establish a scalable design token system for consistent visual language
- Create a comprehensive component library with CSS isolation
- Implement mobile-first responsive layouts optimized for touch interaction
- Refactor the Asset Listing Screen (Bens page) with minimalist, professional design
- Ensure WCAG 2.1 AA accessibility compliance
- Optimize performance for mobile networks (3G+)
- Support offline-first PWA capabilities
- Provide dark mode support with smooth theme transitions

### Non-Goals

- Backend API changes or data model modifications
- Authentication or authorization system changes
- Camera/scanner functionality modifications
- AWS integration or cloud infrastructure changes
- Migration to different UI framework (staying with Blazor WebAssembly)

### Success Criteria

- Design token system implemented with CSS Custom Properties
- Component library with 20+ reusable components using CSS isolation
- Asset Listing Screen redesigned per Requirement 26 specifications
- Lighthouse PWA score ≥ 90
- Lighthouse Performance score ≥ 85 on mobile
- Lighthouse Accessibility score ≥ 95
- First Contentful Paint (FCP) < 1.8s on 3G
- All touch targets ≥ 44x44px
- Color contrast ratios meet WCAG 2.1 AA standards

## Architecture

### System Architecture

The refactored design system follows a layered architecture:

```
┌─────────────────────────────────────────────────┐
│           Application Layer                      │
│  (Pages: Bens, Camera, Settings, etc.)          │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│         Component Library Layer                  │
│  (Base, Layout, Navigation, Forms, Feedback)    │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│          Design Token Layer                      │
│  (Colors, Typography, Spacing, Shadows, etc.)   │
└─────────────────────────────────────────────────┘
                      ↓
┌─────────────────────────────────────────────────┐
│            Theme Provider                        │
│  (Light/Dark mode, System preference detection) │
└─────────────────────────────────────────────────┘
```

### Technology Stack

- **Frontend Framework**: Blazor WebAssembly (.NET 8.0)
- **Component Model**: Razor Components with CSS Isolation
- **Styling**: CSS Custom Properties + Tailwind CSS (utility classes)
- **Icons**: Material Symbols (already in use)
- **PWA**: Service Worker + Web App Manifest
- **Build**: .NET SDK 8.0 + npm for CSS processing

### Design Token System Architecture

Design tokens are implemented as CSS Custom Properties in a global stylesheet (`wwwroot/css/design-tokens.css`):

```css
:root {
  /* Color Tokens */
  --color-primary: #2F6FED;
  --color-primary-dark: #1E4FBD;
  --color-primary-light: #EEF2FF;
  
  /* Spacing Tokens */
  --space-1: 4px;
  --space-2: 8px;
  --space-3: 12px;
  --space-4: 16px;
  --space-5: 20px;
  --space-6: 24px;
  --space-8: 32px;
  --space-12: 48px;
  --space-16: 64px;
  --space-24: 96px;
  
  /* Typography Tokens */
  --font-family-base: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, ...;
  --font-size-xs: 12px;
  --font-size-sm: 13px;
  --font-size-base: 14px;
  --font-size-lg: 16px;
  --font-size-xl: 18px;
  --font-size-2xl: 20px;
  --font-size-3xl: 24px;
  --font-size-4xl: 32px;
  
  /* ... additional tokens */
}

[data-theme="dark"] {
  /* Dark mode overrides */
  --color-bg: #111827;
  --color-surface: #1F2937;
  --color-text: #F9FAFB;
  /* ... */
}
```

### Component Library Structure

```
Components/
├── Base/
│   ├── Button.razor + Button.razor.css
│   ├── Input.razor + Input.razor.css
│   ├── Card.razor + Card.razor.css
│   ├── Badge.razor + Badge.razor.css
│   ├── Avatar.razor + Avatar.razor.css
│   ├── Spinner.razor + Spinner.razor.css
│   └── Icon.razor + Icon.razor.css
├── Layout/
│   ├── Container.razor + Container.razor.css
│   ├── Grid.razor + Grid.razor.css
│   ├── Stack.razor + Stack.razor.css
│   ├── Spacer.razor + Spacer.razor.css
│   └── Divider.razor + Divider.razor.css
├── Navigation/
│   ├── BottomNav.razor + BottomNav.razor.css
│   ├── TopBar.razor + TopBar.razor.css
│   ├── Drawer.razor + Drawer.razor.css
│   └── Tabs.razor + Tabs.razor.css
├── Forms/
│   ├── TextField.razor + TextField.razor.css
│   ├── Select.razor + Select.razor.css
│   ├── Checkbox.razor + Checkbox.razor.css
│   ├── Radio.razor + Radio.razor.css
│   ├── Switch.razor + Switch.razor.css
│   ├── DatePicker.razor + DatePicker.razor.css
│   ├── SearchBar.razor + SearchBar.razor.css
│   └── FilterChip.razor + FilterChip.razor.css
├── Feedback/
│   ├── Alert.razor + Alert.razor.css
│   ├── Toast.razor + Toast.razor.css
│   ├── ProgressBar.razor + ProgressBar.razor.css
│   ├── Skeleton.razor + Skeleton.razor.css
│   ├── EmptyState.razor + EmptyState.razor.css
│   └── Modal.razor + Modal.razor.css
└── Theme/
    └── ThemeProvider.razor + ThemeProvider.razor.css
```

Each component uses CSS Isolation (`.razor.css` files) to scope styles and references design tokens via CSS Custom Properties.

## Components and Interfaces

### Core Components

#### 1. ThemeProvider Component

**Purpose**: Manages theme state (light/dark) and provides theme context to the application.

**Interface**:
```csharp
@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? InitialTheme { get; set; } // "light", "dark", "system"
    
    private string currentTheme = "system";
    
    protected override async Task OnInitializedAsync()
    {
        // Load theme preference from localStorage
        // Detect system preference via JS interop
        // Apply theme to document root
    }
    
    public async Task SetTheme(string theme)
    {
        // Update theme
        // Persist to localStorage
        // Apply to DOM
    }
}
```

**Rendering**:
```html
<CascadingValue Value="this">
    @ChildContent
</CascadingValue>
```

#### 2. Button Component

**Purpose**: Reusable button with variants, sizes, and states.

**Interface**:
```csharp
@code {
    [Parameter] public string? Variant { get; set; } = "primary"; // primary, secondary, outline, ghost
    [Parameter] public string? Size { get; set; } = "md"; // sm, md, lg
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool FullWidth { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
}
```

**CSS Classes** (in Button.razor.css):
```css
.button {
    /* Base styles using design tokens */
    font-family: var(--font-family-base);
    border-radius: var(--radius-lg);
    transition: all 150ms ease-out;
    /* ... */
}

.button--primary {
    background: var(--color-primary);
    color: white;
}

.button--size-md {
    padding: var(--space-3) var(--space-4);
    font-size: var(--font-size-base);
}

/* ... additional variants */
```

#### 3. FilterChip Component

**Purpose**: Lightweight filter chip for the Asset Listing Screen.

**Interface**:
```csharp
@code {
    [Parameter] public string? Label { get; set; }
    [Parameter] public bool Active { get; set; }
    [Parameter] public bool Removable { get; set; } = true;
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }
}
```

**Rendering**:
```html
<button class="filter-chip @(Active ? "filter-chip--active" : "")" 
        @onclick="OnClick"
        aria-pressed="@Active">
    <span class="filter-chip__label">@Label</span>
    @if (Removable && Active)
    {
        <button class="filter-chip__remove" @onclick="OnRemove" @onclick:stopPropagation="true">
            <span class="material-symbols-outlined">close</span>
        </button>
    }
</button>
```

#### 4. SearchBar Component

**Purpose**: Search input with debouncing and clear functionality.

**Interface**:
```csharp
@code {
    [Parameter] public string? Placeholder { get; set; } = "Search...";
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> OnSearch { get; set; }
    [Parameter] public int DebounceMs { get; set; } = 300;
    
    private Timer? debounceTimer;
    
    private void HandleInput(ChangeEventArgs e)
    {
        var value = e.Value?.ToString() ?? "";
        debounceTimer?.Dispose();
        debounceTimer = new Timer(async _ => 
        {
            await OnSearch.InvokeAsync(value);
        }, null, DebounceMs, Timeout.Infinite);
    }
}
```

#### 5. EmptyState Component

**Purpose**: Display empty state with icon, title, and subtitle (no action button per Requirement 26).

**Interface**:
```csharp
@code {
    [Parameter] public string? Icon { get; set; } = "📦";
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Subtitle { get; set; }
}
```

**Rendering**:
```html
<div class="empty-state">
    <div class="empty-state__icon">@Icon</div>
    <h3 class="empty-state__title">@Title</h3>
    @if (!string.IsNullOrEmpty(Subtitle))
    {
        <p class="empty-state__subtitle">@Subtitle</p>
    }
</div>
```

#### 6. Modal Component

**Purpose**: Full-screen modal on mobile, centered dialog on desktop.

**Interface**:
```csharp
@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public bool CloseOnBackdrop { get; set; } = true;
}
```

### Asset Listing Screen Components

The refactored Bens.razor page will use the following component composition:

```
<ThemeProvider>
  <TopBar title="Aspec Captura" />
  
  <SearchBar 
    placeholder="Buscar bem ou descrição..."
    @bind-Value="searchQuery"
    OnSearch="HandleSearch" />
  
  <Button 
    variant="primary" 
    fullWidth="true"
    icon="qr_code_scanner"
    @onclick="NavigateToScanner">
    ESCANEAR BEM
  </Button>
  
  <Stack direction="row" gap="2" wrap="true">
    @foreach (var filter in activeFilters)
    {
      <FilterChip 
        label="@filter.Label"
        active="true"
        @onclick="() => ToggleFilter(filter)" />
    }
  </Stack>
  
  <div class="summary-bar">
    <span>@filteredCount BENS</span>
    <span>|</span>
    <span>@currentUO</span>
  </div>
  
  @if (isLoading)
  {
    <Skeleton type="list" count="5" />
  }
  else if (!items.Any())
  {
    <EmptyState 
      icon="📦"
      title="Nenhum bem encontrado"
      subtitle="Tente outro filtro ou termo de busca" />
  }
  else
  {
    @foreach (var item in items)
    {
      <Card item="@item" @onclick="() => NavigateToDetail(item)" />
    }
  }
</ThemeProvider>
```

## Data Models

### Theme Configuration

```csharp
public class ThemeConfig
{
    public string Mode { get; set; } = "system"; // "light", "dark", "system"
    public bool FollowSystem { get; set; } = true;
}
```

### Design Token Model (for programmatic access)

```csharp
public class DesignTokens
{
    // Colors
    public string ColorPrimary { get; set; } = "#2F6FED";
    public string ColorBg { get; set; } = "#F8FAFC";
    public string ColorSurface { get; set; } = "#FFFFFF";
    public string ColorBorder { get; set; } = "#E5E7EB";
    public string ColorText { get; set; } = "#111827";
    public string ColorTextSecondary { get; set; } = "#6B7280";
    
    // Spacing (in px)
    public int Space1 { get; set; } = 4;
    public int Space2 { get; set; } = 8;
    public int Space3 { get; set; } = 12;
    public int Space4 { get; set; } = 16;
    public int Space5 { get; set; } = 20;
    public int Space6 { get; set; } = 24;
    public int Space8 { get; set; } = 32;
    public int Space12 { get; set; } = 48;
    public int Space16 { get; set; } = 64;
    public int Space24 { get; set; } = 96;
    
    // Border Radius (in px)
    public int RadiusSm { get; set; } = 4;
    public int RadiusMd { get; set; } = 12;
    public int RadiusLg { get; set; } = 16;
    public int RadiusFull { get; set; } = 9999;
    
    // Breakpoints (in px)
    public int BreakpointMobile { get; set; } = 320;
    public int BreakpointTablet { get; set; } = 768;
    public int BreakpointDesktop { get; set; } = 1024;
    public int BreakpointWide { get; set; } = 1440;
}
```

### Component Props Models

```csharp
public class ButtonProps
{
    public string Variant { get; set; } = "primary";
    public string Size { get; set; } = "md";
    public bool Disabled { get; set; }
    public bool FullWidth { get; set; }
    public string? Icon { get; set; }
    public string? AriaLabel { get; set; }
}

public class FilterChipProps
{
    public string Label { get; set; } = "";
    public bool Active { get; set; }
    public bool Removable { get; set; } = true;
}

public class EmptyStateProps
{
    public string Icon { get; set; } = "📦";
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
}
```

## Error Handling

### Component Error Boundaries

Each major component should handle errors gracefully:

```csharp
@code {
    private string? errorMessage;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Component initialization
        }
        catch (Exception ex)
        {
            errorMessage = "Failed to load component";
            Console.Error.WriteLine($"Component error: {ex.Message}");
        }
    }
}

@if (!string.IsNullOrEmpty(errorMessage))
{
    <Alert variant="error">@errorMessage</Alert>
}
```

### Theme Loading Errors

```csharp
public class ThemeProvider
{
    private async Task LoadThemePreference()
    {
        try
        {
            var stored = await localStorage.GetItemAsync<string>("theme");
            currentTheme = stored ?? "system";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to load theme: {ex.Message}");
            currentTheme = "system"; // Fallback to system
        }
    }
}
```

### CSS Loading Failures

Provide fallback inline styles for critical UI elements:

```html
<noscript>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background: #F8FAFC;
            color: #111827;
        }
    </style>
</noscript>
```

### Service Worker Errors

```javascript
// service-worker.js
self.addEventListener('error', (event) => {
    console.error('Service Worker error:', event.error);
    // Log to monitoring service if available
});

self.addEventListener('unhandledrejection', (event) => {
    console.error('Service Worker unhandled rejection:', event.reason);
});
```

## Testing Strategy

This refactor focuses on UI/UX, design system infrastructure, and component library development. The nature of this work involves:

- **Declarative configuration** (design tokens, CSS variables)
- **UI rendering and layout** (component visual appearance)
- **Infrastructure setup** (theme provider, CSS isolation)
- **Visual design consistency** (spacing, colors, typography)

**Property-based testing is NOT appropriate for this feature** because:

1. **Design tokens are declarative configuration**, not functions with input/output behavior
2. **UI rendering and layout** are best tested with snapshot tests and visual regression tests
3. **Component styling** doesn't have universal properties that hold across inputs
4. **Theme switching** is a state management concern, not a pure function
5. **CSS isolation** is a build-time feature, not runtime logic

### Recommended Testing Approach

#### 1. Visual Regression Testing

Use snapshot testing for component visual consistency:

```csharp
[Fact]
public void Button_Primary_RendersCorrectly()
{
    // Arrange
    var cut = RenderComponent<Button>(parameters => parameters
        .Add(p => p.Variant, "primary")
        .Add(p => p.ChildContent, "Click Me"));
    
    // Assert
    cut.MarkupMatches(@"
        <button class=""button button--primary button--size-md"">
            Click Me
        </button>
    ");
}
```

#### 2. Accessibility Testing

Automated accessibility checks using bUnit and manual screen reader testing:

```csharp
[Fact]
public void Button_HasProperAriaLabel()
{
    var cut = RenderComponent<Button>(parameters => parameters
        .Add(p => p.Icon, "search")
        .Add(p => p.AriaLabel, "Search items"));
    
    var button = cut.Find("button");
    Assert.Equal("Search items", button.GetAttribute("aria-label"));
}

[Fact]
public void FilterChip_HasProperAriaPressed()
{
    var cut = RenderComponent<FilterChip>(parameters => parameters
        .Add(p => p.Label, "Active")
        .Add(p => p.Active, true));
    
    var chip = cut.Find("button");
    Assert.Equal("true", chip.GetAttribute("aria-pressed"));
}
```

#### 3. Component Behavior Testing

Test component interactions and state management:

```csharp
[Fact]
public async Task SearchBar_DebounceWorks()
{
    var searchCalled = false;
    var searchValue = "";
    
    var cut = RenderComponent<SearchBar>(parameters => parameters
        .Add(p => p.DebounceMs, 300)
        .Add(p => p.OnSearch, EventCallback.Factory.Create<string>(this, value => {
            searchCalled = true;
            searchValue = value;
        })));
    
    var input = cut.Find("input");
    input.Change("test query");
    
    // Should not call immediately
    Assert.False(searchCalled);
    
    // Wait for debounce
    await Task.Delay(350);
    
    Assert.True(searchCalled);
    Assert.Equal("test query", searchValue);
}
```

#### 4. Theme Provider Testing

Test theme switching logic:

```csharp
[Fact]
public async Task ThemeProvider_SwitchesToDarkMode()
{
    var cut = RenderComponent<ThemeProvider>();
    var provider = cut.Instance;
    
    await provider.SetTheme("dark");
    
    Assert.Equal("dark", provider.CurrentTheme);
    // Verify localStorage was called
    // Verify DOM attribute was set
}
```

#### 5. Integration Testing

Test complete page rendering with all components:

```csharp
[Fact]
public void BensPage_RendersWithEmptyState()
{
    var cut = RenderComponent<Bens>();
    
    // Verify empty state is displayed
    var emptyState = cut.Find(".empty-state");
    Assert.NotNull(emptyState);
    Assert.Contains("Nenhum bem encontrado", emptyState.TextContent);
}
```

#### 6. Performance Testing

Manual testing on real devices:

- Test on actual mobile devices (iOS Safari, Android Chrome)
- Use Chrome DevTools throttling (3G, 4G)
- Measure Lighthouse scores (Performance, Accessibility, PWA)
- Test with React DevTools Profiler equivalent for Blazor
- Monitor bundle sizes and load times

#### 7. Cross-Browser Testing

- Chrome (desktop + mobile)
- Safari (desktop + iOS)
- Firefox (desktop + mobile)
- Edge (desktop)

#### 8. Manual Accessibility Testing

- Screen reader testing (NVDA, JAWS, VoiceOver)
- Keyboard navigation testing
- Color contrast verification with tools
- Touch target size verification on real devices

### Test Coverage Goals

- **Unit tests**: 80%+ coverage for component logic
- **Snapshot tests**: All components have visual regression tests
- **Accessibility tests**: 100% of interactive components
- **Integration tests**: All major user flows (search, filter, navigation)
- **Manual testing**: Complete checklist for each release

### Testing Tools

- **bUnit**: Blazor component testing framework
- **xUnit**: Test runner
- **Playwright**: End-to-end testing (optional)
- **axe-core**: Automated accessibility testing
- **Lighthouse CI**: Performance and PWA testing
- **Percy/Chromatic**: Visual regression testing (optional)

## Implementation Notes

### Phase 1: Foundation (Week 1)

1. Create design token CSS file with all variables
2. Implement ThemeProvider component
3. Set up CSS isolation structure
4. Create base utility classes

### Phase 2: Core Components (Week 2-3)

1. Implement base components (Button, Input, Card, Badge, Icon)
2. Implement layout components (Container, Grid, Stack)
3. Implement form components (TextField, SearchBar, FilterChip)
4. Write unit tests for each component

### Phase 3: Asset Listing Screen Refactor (Week 4)

1. Refactor Bens.razor using new components
2. Implement new layout structure per Requirement 26
3. Remove FAB, implement top CTA
4. Style with design tokens
5. Test on mobile devices

### Phase 4: Navigation & Feedback (Week 5)

1. Implement navigation components (BottomNav, TopBar, Drawer)
2. Implement feedback components (Toast, Modal, Skeleton, EmptyState)
3. Integrate into existing pages

### Phase 5: Interactions & Animations (Week 6)

1. Implement touch gesture handlers
2. Add ripple effects and transitions
3. Implement pull-to-refresh
4. Add loading animations

### Phase 6: PWA & Performance (Week 7)

1. Optimize service worker caching
2. Implement lazy loading for components
3. Compress and optimize images
4. Run Lighthouse audits and optimize

### Phase 7: Polish & Accessibility (Week 8)

1. Accessibility audit with automated tools
2. Manual screen reader testing
3. Dark mode refinement
4. Animation polish
5. Cross-browser testing
6. Final performance optimization

### Migration Strategy

- Implement new design system alongside existing styles
- Use feature flags to toggle between old and new UI
- Migrate pages incrementally (start with Bens.razor)
- Maintain backward compatibility during transition
- Document migration guide for developers

### Performance Considerations

- Use CSS containment for component isolation
- Lazy load components below the fold
- Implement virtual scrolling for long lists
- Optimize images to WebP with fallbacks
- Minimize JavaScript bundle size
- Use CSS transforms for animations (GPU acceleration)
- Implement critical CSS inlining

### Accessibility Considerations

- All interactive elements have proper ARIA labels
- Color contrast ratios meet WCAG 2.1 AA
- Touch targets are minimum 44x44px
- Keyboard navigation works for all interactions
- Screen reader announcements for dynamic content
- Focus indicators are clearly visible
- Text can resize up to 200% without loss of functionality

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Status**: Ready for Implementation

