# Componente de Filtro de Bens

## BemFilterPanel

Componente de filtro personalizado que substitui os botões de abas horizontais por campos de seleção dropdown, permitindo ao usuário filtrar bens por:

- **Estado de Conservação** (ConservationState): Novo, Bom, Regular, Péssimo, Inservível
- **Situação do Bem** (BemStatus): Avariado, Ausência de Plaqueta, Localização Divergente, etc.

### Características

- Interface limpa e intuitiva com dropdowns
- Filtros independentes que podem ser combinados
- Botão "Limpar Filtros" para resetar todos os filtros de uma vez
- Design responsivo que se adapta a dispositivos móveis
- Ícones Material Symbols para melhor UX
- Sticky positioning para manter os filtros visíveis durante scroll

### Uso

```razor
@using AspecCaptura.Components.Filters

<BemFilterPanel 
    SelectedConservationState="@selectedConservationState"
    SelectedStatus="@selectedStatus"
    OnConservationStateFilter="@HandleConservationStateFilter"
    OnStatusFilter="@HandleStatusFilter"
    OnClearFilters="@HandleClearFilters" />
```

### Parâmetros

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `SelectedConservationState` | `ConservationState?` | Estado de conservação atualmente selecionado |
| `SelectedStatus` | `BemStatus?` | Situação do bem atualmente selecionada |
| `OnConservationStateFilter` | `EventCallback<ConservationState?>` | Callback quando o filtro de estado muda |
| `OnStatusFilter` | `EventCallback<BemStatus?>` | Callback quando o filtro de situação muda |
| `OnClearFilters` | `EventCallback` | Callback quando o botão limpar é clicado |

### Implementação no Código

```csharp
private ConservationState? selectedConservationState = null;
private BemStatus? selectedStatus = null;

private async Task HandleConservationStateFilter(ConservationState? state)
{
    selectedConservationState = state;
    ApplyFilters();
    await InvokeAsync(StateHasChanged);
}

private async Task HandleStatusFilter(BemStatus? status)
{
    selectedStatus = status;
    ApplyFilters();
    await InvokeAsync(StateHasChanged);
}

private async Task HandleClearFilters()
{
    selectedConservationState = null;
    selectedStatus = null;
    ApplyFilters();
    await InvokeAsync(StateHasChanged);
}
```

### Vantagens sobre o Sistema Anterior

1. **Melhor UX**: Dropdowns são mais intuitivos que múltiplas abas horizontais
2. **Espaço**: Economiza espaço vertical na interface
3. **Combinação de Filtros**: Permite filtrar por múltiplos critérios simultaneamente
4. **Escalabilidade**: Fácil adicionar novos filtros sem poluir a interface
5. **Mobile-Friendly**: Melhor experiência em dispositivos móveis

### Personalização

O componente usa variáveis CSS do tema global:
- `--surface-color`: Cor de fundo
- `--divider-color`: Cor das bordas
- `--text-primary`, `--text-secondary`: Cores de texto
- `--primary-color`: Cor de destaque

Para customizar, ajuste os estilos no próprio componente ou sobrescreva via CSS global.
