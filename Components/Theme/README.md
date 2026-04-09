# ThemeProvider Component

## Overview

The `ThemeProvider` component manages theme state (light/dark/system) for the AspecCaptura PWA application. It provides theme context to child components via Blazor's CascadingValue and persists theme preferences to localStorage.

## Features

- **Theme Modes**: Supports `light`, `dark`, and `system` (follows OS preference)
- **System Detection**: Automatically detects system color scheme preference
- **Persistence**: Saves theme preference to localStorage
- **Smooth Transitions**: 200ms CSS transitions when theme changes
- **Event Notifications**: Raises `ThemeChanged` event when theme is updated
- **Error Handling**: Graceful fallback to system theme on errors

## Usage

### Basic Setup

Wrap your application root with the `ThemeProvider`:

```razor
<ThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</ThemeProvider>
```

### With Initial Theme

```razor
<ThemeProvider InitialTheme="dark">
    <!-- Your app content -->
</ThemeProvider>
```

### Accessing Theme in Child Components

```razor
@code {
    [CascadingParameter]
    public ThemeProvider? ThemeProvider { get; set; }

    private async Task ToggleTheme()
    {
        if (ThemeProvider != null)
        {
            await ThemeProvider.ToggleTheme();
        }
    }

    private string CurrentTheme => ThemeProvider?.CurrentTheme ?? "system";
}
```

### Listening to Theme Changes

```razor
@code {
    [CascadingParameter]
    public ThemeProvider? ThemeProvider { get; set; }

    protected override void OnInitialized()
    {
        if (ThemeProvider != null)
        {
            ThemeProvider.ThemeChanged += OnThemeChanged;
        }
    }

    private void OnThemeChanged(object? sender, string newTheme)
    {
        Console.WriteLine($"Theme changed to: {newTheme}");
        StateHasChanged();
    }

    public void Dispose()
    {
        if (ThemeProvider != null)
        {
            ThemeProvider.ThemeChanged -= OnThemeChanged;
        }
    }
}
```

## API Reference

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Child components to render |
| `InitialTheme` | `string?` | `null` | Initial theme mode (overridden by stored preference) |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentTheme` | `string` | Current theme mode: "light", "dark", or "system" |
| `IsInitialized` | `bool` | Whether the theme provider has been initialized |

### Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `SetTheme` | `string theme` | `Task` | Sets the theme mode and persists preference |
| `GetEffectiveTheme` | - | `Task<string>` | Gets the effective theme (resolves "system" to "light" or "dark") |
| `ToggleTheme` | - | `Task` | Toggles between light and dark themes |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `ThemeChanged` | `EventHandler<string>` | Raised when the theme changes |

## JavaScript Interop

The ThemeProvider uses the `theme.js` module for:

- Detecting system color scheme preference
- Applying theme to document root (`data-theme` attribute)
- Persisting theme to localStorage
- Updating meta theme-color for mobile browsers

## CSS Integration

The ThemeProvider works with the design tokens defined in `wwwroot/css/design-tokens.css`:

```css
:root {
  /* Light theme tokens */
  --color-bg: #F8FAFC;
  --color-text: #111827;
  /* ... */
}

[data-theme="dark"] {
  /* Dark theme tokens */
  --color-bg: #111827;
  --color-text: #F9FAFB;
  /* ... */
}
```

## Requirements Satisfied

- **Requirement 1.8**: Theme_Provider component that loads tokens into application context
- **Requirement 9.1**: Detect system color scheme preference (light/dark)
- **Requirement 9.2**: Allow manual theme toggle override
- **Requirement 9.3**: Persist theme preference in local storage
- **Requirement 9.4**: Update all Design_Tokens smoothly with 200ms transition
- **Requirement 9.7**: Apply theme without page reload or flicker

## Example: Theme Toggle Button

```razor
@inject ThemeProvider ThemeProvider

<button @onclick="ToggleTheme" class="theme-toggle-btn">
    <span class="material-symbols-outlined">
        @(CurrentTheme == "dark" ? "light_mode" : "dark_mode")
    </span>
</button>

@code {
    [CascadingParameter]
    public ThemeProvider? ThemeProvider { get; set; }

    private string CurrentTheme => ThemeProvider?.CurrentTheme ?? "system";

    private async Task ToggleTheme()
    {
        if (ThemeProvider != null)
        {
            await ThemeProvider.ToggleTheme();
        }
    }
}
```

## Notes

- The theme is applied before Blazor loads (via inline script in `index.html`) to prevent flash of unstyled content
- Theme transitions are disabled when `prefers-reduced-motion` is enabled
- The component gracefully handles errors and falls back to system theme
- Theme changes are logged to console for debugging
