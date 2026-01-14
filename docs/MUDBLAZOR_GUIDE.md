# Guia de Uso do MudBlazor

## Introdução

MudBlazor é uma biblioteca de componentes Blazor gratuita e open-source que implementa Material Design. Este guia mostra como usar os componentes MudBlazor neste projeto.

## Documentação Oficial

- **Site**: https://mudblazor.com/
- **Exemplos**: https://try.mudblazor.com/
- **GitHub**: https://github.com/MudBlazor/MudBlazor
- **Licença**: MIT (gratuita para uso comercial)

## Componentes Disponíveis

MudBlazor oferece +60 componentes prontos para uso. Aqui estão os mais úteis para este projeto:

### Componentes de Layout

```razor
<!-- Container responsivo -->
<MudContainer MaxWidth="MaxWidth.Large">
    <MudText Typo="Typo.h4">Título</MudText>
</MudContainer>

<!-- Grid System -->
<MudGrid>
    <MudItem xs="12" sm="6" md="4">
        <!-- Conteúdo -->
    </MudItem>
</MudGrid>

<!-- Paper (Card com elevação) -->
<MudPaper Elevation="2" Class="pa-4">
    <MudText>Conteúdo do card</MudText>
</MudPaper>
```

### Componentes de Formulário

```razor
<!-- TextField com validação -->
<MudTextField @bind-Value="model.Name" 
              Label="Nome" 
              Variant="Variant.Outlined"
              Required="true"
              RequiredError="Campo obrigatório" />

<!-- Select -->
<MudSelect @bind-Value="model.Category" Label="Categoria">
    <MudSelectItem Value="@("Móveis")">Móveis</MudSelectItem>
    <MudSelectItem Value="@("Eletrônicos")">Eletrônicos</MudSelectItem>
</MudSelect>

<!-- Button -->
<MudButton Variant="Variant.Filled" 
           Color="Color.Primary" 
           OnClick="HandleSubmit">
    Salvar
</MudButton>
```

### Componentes de Feedback

```razor
<!-- Snackbar (Toast) -->
@inject ISnackbar Snackbar

@code {
    void ShowNotification()
    {
        Snackbar.Add("Item salvo com sucesso!", Severity.Success);
    }
}

<!-- Dialog -->
<MudDialog>
    <DialogContent>
        <MudText>Confirmar exclusão?</MudText>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancelar</MudButton>
        <MudButton Color="Color.Error" OnClick="Confirm">Excluir</MudButton>
    </DialogActions>
</MudDialog>

<!-- Progress -->
<MudProgressCircular Color="Color.Primary" Indeterminate="true" />
```

### Componentes de Dados

```razor
<!-- DataGrid -->
<MudDataGrid Items="@items" Filterable="true" SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Nome" />
        <PropertyColumn Property="x => x.Code" Title="Código" />
        <PropertyColumn Property="x => x.Category" Title="Categoria" />
    </Columns>
</MudDataGrid>

<!-- List -->
<MudList>
    @foreach (var item in items)
    {
        <MudListItem Icon="@Icons.Material.Filled.Inventory">
            @item.Name
        </MudListItem>
    }
</MudList>
```

## Temas e Customização

MudBlazor suporta temas customizáveis. Para configurar:

### Configuração no Program.cs

```csharp
// Program.cs
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
});
```

### Tema Customizado ASPEC

```razor
<!-- App.razor ou MainLayout.razor -->
<MudThemeProvider Theme="@customTheme" />
<MudDialogProvider />
<MudSnackbarProvider />

@code {
    private MudTheme customTheme = new MudTheme()
    {
        Palette = new PaletteLight()
        {
            Primary = "#0066CC",      // ASPEC Blue
            Secondary = "#4A90E2",
            AppbarBackground = "#003366",
            Background = "#F5F5F5",
            Surface = "#FFFFFF",
            TextPrimary = "#333333",
            TextSecondary = "#666666",
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#4A90E2",
            Secondary = "#6BB3F5",
            AppbarBackground = "#0052A3",
            Background = "#1A1D23",
            Surface = "#242830",
            TextPrimary = "#FFFFFF",
            TextSecondary = "#B8BCC8",
        }
    };
}
```

## Responsividade com MudBlazor

MudBlazor usa breakpoints padrão do Material Design:

- **xs**: < 600px (mobile)
- **sm**: 600px - 960px (tablet)
- **md**: 960px - 1280px (desktop pequeno)
- **lg**: 1280px - 1920px (desktop)
- **xl**: > 1920px (desktop grande)

### Grid Responsivo

```razor
<MudGrid>
    <!-- Mobile: 12 colunas, Tablet: 6 colunas, Desktop: 4 colunas -->
    <MudItem xs="12" sm="6" md="4">
        <MudCard>
            <MudCardContent>
                <MudText>Item 1</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>
```

### Hidden Components

```razor
<!-- Hidden em mobile -->
<MudHidden Breakpoint="Breakpoint.SmAndDown">
    <MudText>Visível apenas em tablet e desktop</MudText>
</MudHidden>

<!-- Hidden em desktop -->
<MudHidden Breakpoint="Breakpoint.MdAndUp">
    <MudText>Visível apenas em mobile</MudText>
</MudHidden>
```

## Ícones Material

MudBlazor inclui todos os Material Icons:

```razor
<!-- Usando ícones -->
<MudIcon Icon="@Icons.Material.Filled.Camera" />
<MudIcon Icon="@Icons.Material.Outlined.Inventory" Size="Size.Large" />
<MudIcon Icon="@Icons.Material.Rounded.Delete" Color="Color.Error" />

<!-- Em botões -->
<MudIconButton Icon="@Icons.Material.Filled.Add" 
               Color="Color.Primary" 
               OnClick="AddItem" />

<!-- Ícones customizados -->
<MudIcon Icon="@Icons.Custom.Brands.GitHub" />
```

### Categorias de Ícones

- **Filled**: Ícones preenchidos (padrão)
- **Outlined**: Ícones com contorno
- **Rounded**: Ícones com cantos arredondados
- **TwoTone**: Ícones com duas cores
- **Sharp**: Ícones com cantos retos

## Exemplos Práticos para o Projeto

### Card de Item de Inventário

```razor
<MudCard>
    <MudCardMedia Image="@item.CoverImage" Height="200" />
    <MudCardContent>
        <MudText Typo="Typo.h6">@item.Name</MudText>
        <MudText Typo="Typo.body2" Color="Color.Secondary">Código: @item.Code</MudText>
        <MudChip Size="Size.Small" Color="Color.Info">@item.Category</MudChip>
        <MudText Typo="Typo.caption" Class="mt-2">
            <MudIcon Icon="@Icons.Material.Filled.LocationOn" Size="Size.Small" />
            @item.Location
        </MudText>
    </MudCardContent>
    <MudCardActions>
        <MudIconButton Icon="@Icons.Material.Filled.Edit" 
                       Color="Color.Primary" 
                       OnClick="() => EditItem(item)" />
        <MudIconButton Icon="@Icons.Material.Filled.Delete" 
                       Color="Color.Error" 
                       OnClick="() => DeleteItem(item)" />
    </MudCardActions>
</MudCard>
```

### Formulário de Captura

```razor
<MudForm @ref="form" @bind-IsValid="@success">
    <MudTextField @bind-Value="item.Name" 
                  Label="Nome do Item" 
                  Required="true" 
                  RequiredError="Nome é obrigatório"
                  Variant="Variant.Outlined" />
    
    <MudTextField @bind-Value="item.Code" 
                  Label="Código" 
                  Required="true"
                  RequiredError="Código é obrigatório"
                  Variant="Variant.Outlined" />
    
    <MudSelect @bind-Value="item.Category" 
               Label="Categoria"
               Variant="Variant.Outlined">
        <MudSelectItem Value="@("Móveis")">Móveis</MudSelectItem>
        <MudSelectItem Value="@("Eletrônicos")">Eletrônicos</MudSelectItem>
        <MudSelectItem Value="@("Equipamentos")">Equipamentos</MudSelectItem>
        <MudSelectItem Value="@("Ferramentas")">Ferramentas</MudSelectItem>
    </MudSelect>
    
    <MudTextField @bind-Value="item.Location" 
                  Label="Localização" 
                  Required="true"
                  RequiredError="Localização é obrigatória"
                  Variant="Variant.Outlined" />
    
    <MudTextField @bind-Value="item.Observations" 
                  Label="Observações" 
                  Lines="3"
                  Variant="Variant.Outlined" />
    
    <MudButton Variant="Variant.Filled" 
               Color="Color.Primary" 
               Disabled="@(!success)" 
               FullWidth="true"
               OnClick="SaveItem">
        <MudIcon Icon="@Icons.Material.Filled.Save" Class="mr-2" />
        Salvar Item
    </MudButton>
</MudForm>
```

### Lista de Itens com Busca

```razor
<MudTextField @bind-Value="searchString" 
              Placeholder="Buscar itens..." 
              Adornment="Adornment.Start" 
              AdornmentIcon="@Icons.Material.Filled.Search" 
              IconSize="Size.Medium" 
              Class="mb-4" />

<MudList>
    @foreach (var item in FilteredItems)
    {
        <MudListItem Icon="@Icons.Material.Filled.Inventory" 
                     OnClick="() => ViewDetails(item)">
            <div class="d-flex justify-space-between align-center">
                <div>
                    <MudText Typo="Typo.body1">@item.Name</MudText>
                    <MudText Typo="Typo.caption" Color="Color.Secondary">@item.Code</MudText>
                </div>
                <MudChip Size="Size.Small" Color="Color.Info">@item.Category</MudChip>
            </div>
        </MudListItem>
        <MudDivider />
    }
</MudList>

@code {
    private string searchString = "";
    
    private IEnumerable<InventoryItem> FilteredItems => 
        items.Where(x => x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                         x.Code.Contains(searchString, StringComparison.OrdinalIgnoreCase));
}
```

### Dialog de Confirmação

```razor
@inject IDialogService DialogService

@code {
    private async Task DeleteItemWithConfirmation(InventoryItem item)
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = $"Deseja realmente excluir o item '{item.Name}'?",
            ["ButtonText"] = "Excluir",
            ["Color"] = Color.Error
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar Exclusão", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await DeleteItem(item);
            Snackbar.Add("Item excluído com sucesso!", Severity.Success);
        }
    }
}
```

### Floating Action Button (FAB) para Câmera

```razor
<MudFab Color="Color.Primary" 
        StartIcon="@Icons.Material.Filled.CameraAlt" 
        OnClick="OpenCamera"
        Style="position: fixed; bottom: 80px; right: 20px; z-index: 1000;" />

@code {
    private void OpenCamera()
    {
        Navigation.NavigateTo("/camera");
    }
}
```

## Migração Gradual

Para migrar componentes existentes para MudBlazor:

### Passo 1: Identificar Componentes

Comece com os componentes mais usados:
- Formulários (Login, Register, Camera)
- Cards (ItemCard)
- Listas (Home page)
- Botões e inputs

### Passo 2: Substituir Gradualmente

Não precisa migrar tudo de uma vez. Exemplo de migração de um input:

**Antes (HTML nativo):**
```razor
<input type="text" 
       class="form-input" 
       @bind-value="item.Name" 
       placeholder="Nome do item" />
<ValidationMessage For="@(() => item.Name)" />
```

**Depois (MudBlazor):**
```razor
<MudTextField @bind-Value="item.Name" 
              Label="Nome do item" 
              Variant="Variant.Outlined"
              Required="true"
              RequiredError="Campo obrigatório" />
```

### Passo 3: Testar Responsividade

Verifique em diferentes tamanhos de tela:
- Mobile (< 600px)
- Tablet (600px - 960px)
- Desktop (> 960px)

### Passo 4: Manter Consistência

Use o mesmo `Variant` e `Color` em toda a aplicação:
- **Variant**: `Outlined` para inputs, `Filled` para botões primários
- **Color**: `Primary` para ações principais, `Secondary` para ações secundárias

## Classes Utilitárias

MudBlazor fornece classes CSS utilitárias:

### Spacing

```razor
<!-- Padding -->
<div class="pa-4">Padding all sides: 16px</div>
<div class="pt-2">Padding top: 8px</div>
<div class="px-3">Padding horizontal: 12px</div>

<!-- Margin -->
<div class="ma-4">Margin all sides: 16px</div>
<div class="mt-2">Margin top: 8px</div>
<div class="mx-auto">Margin horizontal: auto (centraliza)</div>
```

### Flexbox

```razor
<div class="d-flex justify-space-between align-center">
    <div>Item 1</div>
    <div>Item 2</div>
</div>

<div class="d-flex flex-column gap-4">
    <div>Item 1</div>
    <div>Item 2</div>
</div>
```

### Display

```razor
<div class="d-none d-sm-block">Visível apenas em tablet+</div>
<div class="d-block d-md-none">Visível apenas em mobile/tablet</div>
```

## Próximos Passos

### Tarefas Recomendadas

- [ ] Migrar formulários de Login e Register para MudForm
- [ ] Implementar MudDataGrid na página Home
- [ ] Adicionar MudSnackbar para notificações (substituir ToastService)
- [ ] Criar MudDialog para confirmações
- [ ] Implementar MudAppBar e MudDrawer para navegação
- [ ] Adicionar MudFab para câmera
- [ ] Configurar tema ASPEC customizado
- [ ] Implementar skeleton loaders com MudSkeleton

### Recursos Adicionais

- **Playground**: https://try.mudblazor.com/ - Teste componentes ao vivo
- **Templates**: https://github.com/MudBlazor/Templates - Templates de projeto
- **Discord**: https://discord.gg/mudblazor - Comunidade ativa
- **YouTube**: Tutoriais em vídeo disponíveis

## Troubleshooting

### Problema: Componentes não aparecem

**Solução**: Verifique se adicionou os providers no layout:
```razor
<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

### Problema: Ícones não carregam

**Solução**: Certifique-se de que o Material Icons está no `index.html`:
```html
<link href="https://fonts.googleapis.com/css?family=Material+Icons" rel="stylesheet">
```

### Problema: Tema não aplica

**Solução**: Verifique a ordem de carregamento dos CSS no `index.html`:
```html
<!-- MudBlazor deve vir antes do CSS customizado -->
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<link rel="stylesheet" href="css/app.css" />
```

## Conclusão

MudBlazor é uma ferramenta poderosa para criar UIs modernas e responsivas em Blazor. Com +60 componentes, temas customizáveis e excelente documentação, é uma escolha ideal para este projeto PWA.

Para dúvidas ou suporte, consulte a [documentação oficial](https://mudblazor.com/) ou a [comunidade no Discord](https://discord.gg/mudblazor).
    <MudTextField @bind-Value="item.Name" 
                  Label="Nome do Item" 
                  Required="true" 
                  RequiredError="Nome é obrigatório"
                  Variant="Variant.Outlined" />
    
    <MudTextField @bind-Value="item.Code" 
                  Label="Código" 
                  Required="true"
                  RequiredError="Código é obrigatório"
                  Variant="Variant.Outlined" />
    
    <MudSelect @bind-Value="item.Category" 
               Label="Categoria"
               Variant="Variant.Outlined">
        <MudSelectItem Value="@("Móveis")">Móveis</MudSelectItem>
        <MudSelectItem Value="@("Eletrônicos")">Eletrônicos</MudSelectItem>
        <MudSelectItem Value="@("Equipamentos")">Equipamentos</MudSelectItem>
        <MudSelectItem Value="@("Ferramentas")">Ferramentas</MudSelectItem>
    </MudSelect>
    
    <MudTextField @bind-Value="item.Location" 
                  Label="Localização" 
                  Required="true"
                  RequiredError="Localização é obrigatória"
                  Variant="Variant.Outlined" />
    
    <MudTextField @bind-Value="item.Observations" 
                  Label="Observações" 
                  Lines="3"
                  Variant="Variant.Outlined" />
    
    <MudButton Variant="Variant.Filled" 
               Color="Color.Primary" 
               Disabled="@(!success)" 
               FullWidth="true"
               OnClick="SaveItem">
        <MudIcon Icon="@Icons.Material.Filled.Save" Class="mr-2" />
        Salvar Item
    </MudButton>
</MudForm>
```

### Lista de Itens com Busca

```razor
<MudTextField @bind-Value="searchString" 
              Placeholder="Buscar itens..." 
              Adornment="Adornment.Start" 
              AdornmentIcon="@Icons.Material.Filled.Search" 
              IconSize="Size.Medium" 
              Class="mb-4" />

<MudList>
    @foreach (var item in FilteredItems)
    {
        <MudListItem Icon="@Icons.Material.Filled.Inventory" 
                     OnClick="() => ViewDetails(item)">
            <div class="d-flex justify-space-between align-center">
                <div>
                    <MudText Typo="Typo.body1">@item.Name</MudText>
                    <MudText Typo="Typo.caption" Color="Color.Secondary">@item.Code</MudText>
                </div>
                <MudChip Size="Size.Small" Color="Color.Info">@item.Category</MudChip>
            </div>
        </MudListItem>
        <MudDivider />
    }
</MudList>

@code {
    private string searchString = "";
    
    private IEnumerable<InventoryItem> FilteredItems => 
        items.Where(x => x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                         x.Code.Contains(searchString, StringComparison.OrdinalIgnoreCase));
}
```

### Dialog de Confirmação

```razor
@inject IDialogService DialogService

@code {
    private async Task DeleteItemWithConfirmation(InventoryItem item)
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = $"Deseja realmente excluir o item '{item.Name}'?",
            ["ButtonText"] = "Excluir",
            ["Color"] = Color.Error
        };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("Confirmar Exclusão", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await DeleteItem(item);
            Snackbar.Add("Item excluído com sucesso!", Severity.Success);
        }
    }
}
```

### Floating Action Button (FAB) para Câmera

```razor
<MudFab Color="Color.Primary" 
        StartIcon="@Icons.Material.Filled.CameraAlt" 
        OnClick="OpenCamera"
        Style="position: fixed; bottom: 80px; right: 20px; z-index: 1000;" />

@code {
    private void OpenCamera()
    {
        Navigation.NavigateTo("/camera");
    }
}
```

## Migração Gradual

Para migrar componentes existentes para MudBlazor:

### Passo 1: Identificar Componentes

Comece com os componentes mais usados:
- Formulários (Login, Register, Camera)
- Cards (ItemCard)
- Listas (Home page)
- Botões e inputs

### Passo 2: Substituir Gradualmente

Não precisa migrar tudo de uma vez. Exemplo de migração de um input:

**Antes (HTML nativo):**
```razor
<input type="text" 
       class="form-input" 
       @bind-value="item.Name" 
       placeholder="Nome do item" />
<ValidationMessage For="@(() => item.Name)" />
```

**Depois (MudBlazor):**
```razor
<MudTextField @bind-Value="item.Name" 
              Label="Nome do item" 
              Variant="Variant.Outlined"
              Required="true"
              RequiredError="Campo obrigatório" />
```

### Passo 3: Testar Responsividade

Verifique em diferentes tamanhos de tela:
- Mobile (< 600px)
- Tablet (600px - 960px)
- Desktop (> 960px)

### Passo 4: Manter Consistência

Use o mesmo `Variant` e `Color` em toda a aplicação:
- **Variant**: `Outlined` para inputs, `Filled` para botões primários
- **Color**: `Primary` para ações principais, `Secondary` para ações secundárias

## Classes Utilitárias

MudBlazor fornece classes CSS utilitárias:

### Spacing

```razor
<!-- Padding -->
<div class="pa-4">Padding all sides: 16px</div>
<div class="pt-2">Padding top: 8px</div>
<div class="px-3">Padding horizontal: 12px</div>

<!-- Margin -->
<div class="ma-4">Margin all sides: 16px</div>
<div class="mt-2">Margin top: 8px</div>
<div class="mx-auto">Margin horizontal: auto (centraliza)</div>
```

### Flexbox

```razor
<div class="d-flex justify-space-between align-center">
    <div>Item 1</div>
    <div>Item 2</div>
</div>

<div class="d-flex flex-column gap-4">
    <div>Item 1</div>
    <div>Item 2</div>
</div>
```

### Display

```razor
<div class="d-none d-sm-block">Visível apenas em tablet+</div>
<div class="d-block d-md-none">Visível apenas em mobile/tablet</div>
```

## Próximos Passos

### Tarefas Recomendadas

- [ ] Migrar formulários de Login e Register para MudForm
- [ ] Implementar MudDataGrid na página Home
- [ ] Adicionar MudSnackbar para notificações (substituir ToastService)
- [ ] Criar MudDialog para confirmações
- [ ] Implementar MudAppBar e MudDrawer para navegação
- [ ] Adicionar MudFab para câmera
- [ ] Configurar tema ASPEC customizado
- [ ] Implementar skeleton loaders com MudSkeleton

### Recursos Adicionais

- **Playground**: https://try.mudblazor.com/ - Teste componentes ao vivo
- **Templates**: https://github.com/MudBlazor/Templates - Templates de projeto
- **Discord**: https://discord.gg/mudblazor - Comunidade ativa
- **YouTube**: Tutoriais em vídeo disponíveis

## Troubleshooting

### Problema: Componentes não aparecem

**Solução**: Verifique se adicionou os providers no layout:
```razor
<MudThemeProvider />
<MudDialogProvider />
<MudSnackbarProvider />
```

### Problema: Ícones não carregam

**Solução**: Certifique-se de que o Material Icons está no `index.html`:
```html
<link href="https://fonts.googleapis.com/css?family=Material+Icons" rel="stylesheet">
```

### Problema: Tema não aplica

**Solução**: Verifique a ordem de carregamento dos CSS no `index.html`:
```html
<!-- MudBlazor deve vir antes do CSS customizado -->
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<link rel="stylesheet" href="css/app.css" />
```

## Conclusão

MudBlazor é uma ferramenta poderosa para criar UIs modernas e responsivas em Blazor. Com +60 componentes, temas customizáveis e excelente documentação, é uma escolha ideal para este projeto PWA.

Para dúvidas ou suporte, consulte a [documentação oficial](https://mudblazor.com/) ou a [comunidade no Discord](https://discord.gg/mudblazor).
=======
```

>>>>>>> main
