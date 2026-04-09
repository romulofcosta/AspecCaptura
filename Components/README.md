# AspecCaptura Component Library

## Overview

This component library provides a comprehensive set of reusable, accessible, and mobile-first UI components for the AspecCaptura Progressive Web Application. All components follow a consistent design system based on design tokens, use CSS isolation for scoped styling, and are optimized for touch interactions.

## Architecture

The component library follows a layered architecture:

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

## Directory Structure

### `/Base`
Fundamental UI building blocks that form the foundation of the design system.

**Components:**
- `Button.razor` - Primary, secondary, outline, and ghost button variants
- `Input.razor` - Text input with validation states
- `Card.razor` - Container with elevation and interactive variants
- `Badge.razor` - Status indicators and notification badges
- `Chip.razor` - Compact elements for filters and tags
- `Select.razor` - Dropdown selection component
- `Toggle.razor` - Switch/toggle component

**Usage:**
```razor
<Button Variant="primary" Size="md" FullWidth="true" OnClick="HandleClick">
    Click Me
</Button>
```

### `/Layout`
Components for structuring page layouts and organizing content.

**Components:**
- `AppLayout.razor` - Main application layout wrapper
- `MainLayout.razor` - Standard page layout
- `TopBar.razor` - Top navigation bar
- `BottomNav.razor` - Bottom navigation for mobile
- `SafeAreaContainer.razor` - Respects device safe areas (notches)
- `Header.razor` - Page header component
- `Footer.razor` - Page footer component

**Usage:**
```razor
<SafeAreaContainer>
    <TopBar Title="Aspec Captura" />
    <main>
        @Body
    </main>
    <BottomNav />
</SafeAreaContainer>
```

### `/Navigation`
Components for app navigation and routing.

**Components:**
- `BottomNavigation.razor` - Mobile-optimized bottom navigation bar
- Navigation items with active state indicators
- Keyboard and screen reader accessible

**Usage:**
```razor
<BottomNavigation ActiveRoute="@CurrentRoute" OnNavigate="HandleNavigate" />
```

### `/Forms`
Form input components optimized for mobile touch interaction.

**Components:**
- `HierarchicalDropdown.razor` - Multi-level dropdown selection
- `ConservationStateChip.razor` - Specialized chip for conservation states
- Form validation and error handling components

**Usage:**
```razor
<HierarchicalDropdown 
    Items="@hierarchyItems" 
    @bind-SelectedValue="selectedValue"
    Placeholder="Select an option" />
```

### `/Feedback`
Components that provide user feedback for various states and actions.

**Components (Planned):**
- `Alert.razor` - Inline alert messages
- `Toast.razor` - Temporary notification toasts
- `ProgressBar.razor` - Progress indicators
- `Skeleton.razor` - Loading placeholders
- `EmptyState.razor` - Empty state illustrations
- `Modal.razor` - Dialog overlays

**Usage:**
```razor
<EmptyState 
    Icon="📦" 
    Title="Nenhum bem encontrado"
    Subtitle="Tente outro filtro ou termo de busca" />
```

### `/Cards`
Specialized card components for displaying structured content.

**Components:**
- `ItemCard.razor` - Asset/item display card
- `PatrimonioCard.razor` - Patrimony information card
- `StatisticsCard.razor` - Statistics display card

**Usage:**
```razor
<ItemCard 
    Title="@item.Name"
    Subtitle="@item.Description"
    OnClick="() => NavigateToDetail(item)" />
```

### `/Theme`
Theme management and theming utilities.

**Components:**
- `ThemeProvider.razor` - Theme context provider
- Light/dark mode switching
- System preference detection

**Documentation:**
- See `Theme/README.md` for detailed theme integration guide
- See `Theme/INTEGRATION_GUIDE.md` for implementation details

### `/Shared`
Shared utility components used across the application.

**Components:**
- `LoadingSpinner.razor` - Loading indicator
- `Toast.razor` - Toast notification system
- `ItemModal.razor` - Item detail modal
- `UpdateNotification.razor` - App update notifications

### `/Filters`
Filtering and search components.

**Components:**
- `BemFilterPanel.razor` - Asset filtering panel

**Documentation:**
- See `Filters/README.md` for filter implementation details

## Design Principles

### 1. Mobile-First
All components are designed for mobile devices first, then enhanced for larger screens.

- Touch targets minimum 44x44px
- Optimized for one-handed use
- Responsive layouts with breakpoints

### 2. CSS Isolation
Each component uses CSS isolation (`.razor.css` files) to scope styles and prevent conflicts.

```
Button.razor
Button.razor.css  ← Scoped styles for Button component only
```

### 3. Design Tokens
Components reference design tokens via CSS Custom Properties for consistency.

```css
.button {
    padding: var(--space-3) var(--space-4);
    border-radius: var(--radius-lg);
    background: var(--color-primary);
    color: var(--color-text-inverse);
}
```

### 4. Accessibility
All components follow WCAG 2.1 AA standards:

- Proper ARIA labels and roles
- Keyboard navigation support
- Screen reader announcements
- Color contrast ratios (4.5:1 for text, 3:1 for UI)
- Focus indicators

### 5. Consistent API
Components follow a consistent parameter pattern:

```csharp
[Parameter] public string? Variant { get; set; }
[Parameter] public string? Size { get; set; }
[Parameter] public bool Disabled { get; set; }
[Parameter] public EventCallback OnClick { get; set; }
[Parameter] public RenderFragment? ChildContent { get; set; }
```

## Design Tokens

Design tokens are defined in `wwwroot/css/design-tokens.css` and include:

### Colors
```css
--color-primary: #2F6FED;
--color-bg: #F8FAFC;
--color-surface: #FFFFFF;
--color-border: #E5E7EB;
--color-text: #111827;
--color-text-secondary: #6B7280;
```

### Spacing
```css
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
```

### Typography
```css
--font-family-base: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
--font-size-xs: 12px;
--font-size-sm: 13px;
--font-size-base: 14px;
--font-size-lg: 16px;
--font-size-xl: 18px;
--font-size-2xl: 20px;
--font-size-3xl: 24px;
--font-size-4xl: 32px;
```

### Border Radius
```css
--radius-sm: 4px;
--radius-md: 12px;
--radius-lg: 16px;
--radius-full: 9999px;
```

### Breakpoints
```css
--breakpoint-mobile: 320px;
--breakpoint-tablet: 768px;
--breakpoint-desktop: 1024px;
--breakpoint-wide: 1440px;
```

## Usage Guidelines

### Importing Components

Components can be imported in pages or other components:

```razor
@using AspecCaptura.Components.Base
@using AspecCaptura.Components.Layout
@using AspecCaptura.Components.Forms

<Button Variant="primary" OnClick="HandleSubmit">Submit</Button>
```

### Component Composition

Components are designed to be composed together:

```razor
<Card Elevation="raised">
    <Header>
        <h3>Asset Details</h3>
    </Header>
    <Body>
        <p>Asset information goes here</p>
    </Body>
    <Footer>
        <Button Variant="primary">View Details</Button>
    </Footer>
</Card>
```

### Responsive Behavior

Components automatically adapt to screen size using CSS media queries:

```css
/* Mobile-first approach */
.component {
    padding: var(--space-3);
}

/* Tablet and above */
@media (min-width: 768px) {
    .component {
        padding: var(--space-6);
    }
}
```

### Theme Support

Components automatically adapt to light/dark theme:

```css
/* Light theme (default) */
.component {
    background: var(--color-surface);
    color: var(--color-text);
}

/* Dark theme */
[data-theme="dark"] .component {
    background: var(--color-surface-dark);
    color: var(--color-text-dark);
}
```

## Best Practices

### 1. Use Semantic HTML
```razor
<!-- Good -->
<button class="button" @onclick="HandleClick">Click Me</button>

<!-- Avoid -->
<div class="button" @onclick="HandleClick">Click Me</div>
```

### 2. Provide ARIA Labels
```razor
<Button Icon="search" AriaLabel="Search assets">
    <span class="sr-only">Search</span>
</Button>
```

### 3. Handle Loading States
```razor
@if (isLoading)
{
    <Skeleton Type="card" Count="3" />
}
else
{
    @foreach (var item in items)
    {
        <ItemCard Item="@item" />
    }
}
```

### 4. Provide Empty States
```razor
@if (!items.Any())
{
    <EmptyState 
        Icon="📦"
        Title="No items found"
        Subtitle="Try adjusting your filters" />
}
```

### 5. Use Design Tokens
```css
/* Good - uses design tokens */
.custom-component {
    padding: var(--space-4);
    color: var(--color-text);
}

/* Avoid - hardcoded values */
.custom-component {
    padding: 16px;
    color: #111827;
}
```

## Performance Considerations

### CSS Isolation
- Scoped styles prevent global CSS bloat
- Only component-specific styles are loaded

### Lazy Loading
- Components can be lazy-loaded for better performance
- Use `@attribute [Lazy]` for components not immediately visible

### Minimal Dependencies
- Components have minimal external dependencies
- Use native browser features when possible

## Testing

### Unit Tests
Components should have unit tests covering:
- Rendering with different parameters
- Event handling
- Accessibility attributes
- Responsive behavior

### Visual Regression Tests
- Snapshot tests for visual consistency
- Test in both light and dark themes

### Accessibility Tests
- Automated tests with axe-core
- Manual screen reader testing
- Keyboard navigation testing

## Migration Guide

When migrating existing components to the new design system:

1. **Identify the component category** (Base, Layout, Forms, etc.)
2. **Review design token usage** - replace hardcoded values
3. **Implement CSS isolation** - create `.razor.css` file
4. **Add accessibility attributes** - ARIA labels, roles, keyboard support
5. **Test responsive behavior** - verify on mobile and desktop
6. **Test theme support** - verify in light and dark modes
7. **Write unit tests** - ensure component works as expected

## Contributing

When adding new components:

1. Place in the appropriate category directory
2. Follow the consistent parameter pattern
3. Use CSS isolation for styling
4. Reference design tokens
5. Ensure accessibility compliance
6. Add documentation and examples
7. Write unit tests
8. Test on real mobile devices

## Resources

- [Design Document](../.kiro/specs/pwa-mobile-design-system-refactor/design.md)
- [Requirements Document](../.kiro/specs/pwa-mobile-design-system-refactor/requirements.md)
- [Theme Integration Guide](Theme/INTEGRATION_GUIDE.md)
- [Filter Components Guide](Filters/README.md)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Material Design 3](https://m3.material.io/)

## Support

For questions or issues with components, please refer to:
- Component-specific README files in each directory
- Design system documentation in `.kiro/specs/`
- Project documentation in `/docs/`

---

**Version:** 1.0  
**Last Updated:** 2024  
**Status:** Active Development
