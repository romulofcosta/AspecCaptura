# Correção: Tipo Incorreto do Campo `estado` na Sincronização

## Problema Identificado

Após a correção inicial dos campos faltantes, o erro de conversão JSON persistia:
```
The JSON value could not be converted to System.Collections.Generic.List`1[pwa_camera_poc_blazor.Services.Sync.TombamentoWire]
```

## Causa Raiz

Incompatibilidade de tipo no campo `estado`:

### Dados Originais (JSON)
```json
{
  "estado": "BOM"  // String com valores: "NOVO", "BOM", "REGULAR", "PESSIMO", "INSERVIVEL"
}
```

### API (`TombamentoRecord`)
```csharp
public string? Estado { get; set; }  // ✓ Correto
```

### Frontend (`TombamentoWire`) - ANTES
```csharp
public int? estado { get; set; }  // ✗ ERRADO - esperava inteiro mas recebia string
```

### Frontend (`PatrimonioItem`)
```csharp
public ConservationState? Estado { get; set; }  // Enum
```

## Solução Aplicada

### 1. Corrigido tipo do campo em `TombamentoWire`
```csharp
public string? estado { get; set; }  // ✓ Agora aceita string
```

### 2. Adicionada função de conversão `ParseEstado`
```csharp
private static Models.ConservationState? ParseEstado(string? estado)
{
    if (string.IsNullOrWhiteSpace(estado))
        return null;

    return estado.ToUpperInvariant() switch
    {
        "NOVO" => Models.ConservationState.Novo,
        "BOM" => Models.ConservationState.Bom,
        "REGULAR" => Models.ConservationState.Regular,
        "PESSIMO" or "PÉSSIMO" => Models.ConservationState.Pessimo,
        "INSERVIVEL" or "INSERVÍVEL" => Models.ConservationState.Inservivel,
        _ => null
    };
}
```

### 3. Atualizado mapeamento em `MapToStore`
```csharp
Estado = ParseEstado(w.estado)  // Converte string para enum
```

## Valores Suportados

| Valor JSON | Enum ConservationState |
|------------|------------------------|
| "NOVO" | Novo |
| "BOM" | Bom |
| "REGULAR" | Regular |
| "PESSIMO" ou "PÉSSIMO" | Pessimo |
| "INSERVIVEL" ou "INSERVÍVEL" | Inservivel |
| null ou vazio | null |
| Outros valores | null (ignorado) |

## Impacto

- Deserialização JSON agora funciona corretamente
- Conversão automática de string para enum
- Suporte a acentuação (PÉSSIMO, INSERVÍVEL)
- Valores inválidos são tratados como null (não quebram a sincronização)

## Arquivos Modificados

- `pwa-camera-poc-blazor/Services/Sync/SyncService.cs`
  - Alterado tipo de `TombamentoWire.estado` de `int?` para `string?`
  - Adicionada função `ParseEstado` para conversão
  - Atualizado mapeamento em `MapToStore`

## Testes Recomendados

1. Autenticar no sistema
2. Iniciar sincronização de lotes
3. Verificar que os lotes são baixados sem erros
4. Confirmar que os dados são armazenados corretamente no IndexedDB
5. Verificar que o campo Estado é exibido corretamente na interface
