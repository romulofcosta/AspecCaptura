# ThemeProvider Integration Guide

## Step-by-Step Integration

### 1. Verify Design Tokens CSS is Loaded

Ensure `design-tokens.css` is loaded in `wwwroot/index.html` **before** other stylesheets:

```html
<!-- Design Tokens (must load first) -->
<link rel="stylesheet" href="css/design-tokens.css" />

<!-- Other stylesheets -->
<link rel="stylesheet" href="css/output.css" />
<link rel="stylesheet" href="css/theme.css" />
```

✅ **Already done** - design-tokens.css is now loaded first in index.html

### 2. Verify Theme Initialization Script

The theme initialization script in `index.html` prevents flash of unstyled content:

```html
<script>
    (function () {
        const savedTheme = localStorage.getItem('aspec-captura-theme') || 'light';
        document.documentElement.setAttribute('data-theme', savedTheme);
    })();
</script>
```

✅ **Already exists** - This script is already in index.html

### 3. Wrap Application with ThemeProvider

Update `App.razor` to wrap the application with ThemeProvider:

**Before:**
```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- content -->
    </Router>
</CascadingAuthenticationState>
```

**After:**
```razor
@using AspecCaptura.Components.Theme

<ThemeProvider>
    <CascadingAuthenticationState>
        <Router AppAssembly="@typeof(App).Assembly">
            <!-- content -->
        </Router>
    </CascadingAuthenticationState>
</ThemeProvider>
```

### 4. Create Theme Toggle Component (Optional)

Create a reusable theme toggle button component:

**File:** `Components/Navigation/ThemeToggle.razor`

```razor
@using AspecCaptura.Components.Theme

<button @onclick="ToggleTheme" 
        class="theme-toggle" 
        aria-label="Toggle theme"
        title="@GetTooltip()">
    <span class="material-symbols-outlined">
        @GetIcon()
    </span>
</button>

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

    private string GetIcon()
    {
        if (ThemeProvider == null) return "light_mode";
        
        return ThemeProvider.CurrentTheme switch
        {
            "dark" => "light_mode",
            "light" => "dark_mode",
            "system" => "brightness_auto",
            _ => "light_mode"
        };
    }

    private string GetTooltip()
    {
        if (ThemeProvider == null) return "Toggle theme";
        
        return ThemeProvider.CurrentTheme switch
        {
            "dark" => "Switch to light mode",
            "light" => "Switch to dark mode",
            "system" => "Using system theme",
            _ => "Toggle theme"
        };
    }
}
```

**File:** `Components/Navigation/ThemeToggle.razor.css`

```css
.theme-toggle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: var(--touch-target-min);
    height: var(--touch-target-min);
    padding: var(--space-2);
    background: transparent;
    border: none;
    border-radius: var(--radius-full);
    color: var(--color-text-secondary);
    cursor: pointer;
    transition: all var(--transition-fast) var(--easing-ease-out);
}

.theme-toggle:hover {
    background: var(--color-neutral-100);
    color: var(--color-text-primary);
}

.theme-toggle:active {
    transform: scale(0.95);
}

.theme-toggle .material-symbols-outlined {
    font-size: 24px;
}

/* Dark mode styles */
[data-theme="dark"] .theme-toggle:hover {
    background: var(--color-neutral-800);
}
```

### 5. Add Theme Toggle to Navigation

Add the ThemeToggle component to your navigation bar (e.g., TopBar or Settings page):

```razor
@using AspecCaptura.Components.Navigation

<nav class="top-bar">
    <h1>Aspec Captura</h1>
    <div class="actions">
        <ThemeToggle />
        <!-- Other action buttons -->
    </div>
</nav>
```

### 6. Access Theme in Components

Any component can access the current theme:

```razor
@code {
    [CascadingParameter]
    public ThemeProvider? ThemeProvider { get; set; }

    protected override void OnInitialized()
    {
        if (ThemeProvider != null)
        {
            // Subscribe to theme changes
            ThemeProvider.ThemeChanged += OnThemeChanged;
            
            // Get current theme
            var currentTheme = ThemeProvider.CurrentTheme;
            Console.WriteLine($"Current theme: {currentTheme}");
        }
    }

    private void OnThemeChanged(object? sender, string newTheme)
    {
        Console.WriteLine($"Theme changed to: {newTheme}");
        // Update component state if needed
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

### 7. Testing

1. **Build the application:**
   ```bash
   dotnet build
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Test theme switching:**
   - Open the application in a browser
   - Click the theme toggle button
   - Verify the theme changes smoothly
   - Refresh the page - theme should persist
   - Check browser DevTools console for theme logs

4. **Test system theme detection:**
   - Set theme to "system" mode
   - Change your OS theme preference
   - Verify the app theme updates automatically

### 8. Verify Requirements

✅ **Requirement 1.8**: Theme_Provider component loads tokens into application context  
✅ **Requirement 9.1**: Detects system color scheme preference  
✅ **Requirement 9.2**: Allows manual theme toggle override  
✅ **Requirement 9.3**: Persists theme preference in localStorage  
✅ **Requirement 9.4**: Updates Design_Tokens smoothly with 200ms transition  
✅ **Requirement 9.7**: Applies theme without page reload or flicker  

## Troubleshooting

### Theme doesn't persist after refresh
- Check browser console for localStorage errors
- Verify `theme.js` is loaded correctly
- Check that the initialization script in `index.html` is present

### Theme changes are not smooth
- Verify `design-tokens.css` includes transition rules
- Check that `prefers-reduced-motion` is not enabled
- Inspect CSS transitions in browser DevTools

### System theme detection not working
- Verify browser supports `prefers-color-scheme` media query
- Check browser console for JavaScript errors
- Test in a modern browser (Chrome 76+, Firefox 67+, Safari 12.1+)

### Component can't access ThemeProvider
- Ensure ThemeProvider wraps the component in the component hierarchy
- Verify `[CascadingParameter]` attribute is used correctly
- Check that the component is rendered inside ThemeProvider's ChildContent

## Next Steps

After integrating the ThemeProvider:

1. **Task 1.3**: Create theme switching JavaScript interop (✅ Already done - `theme.js`)
2. **Task 1.4**: Define dark mode color palette (✅ Already done - in `design-tokens.css`)
3. **Task 2.x**: Implement other components using the theme tokens
4. **Task 9.x**: Refactor existing pages to use the new design system

## Additional Resources

- [Design Tokens Documentation](../../../wwwroot/css/design-tokens.css)
- [ThemeProvider API Reference](./README.md)
- [Blazor Cascading Values](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/cascading-values-and-parameters)
- [CSS Custom Properties](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
