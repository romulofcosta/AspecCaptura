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
```

