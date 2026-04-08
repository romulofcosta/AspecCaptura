# Análise de Inconsistências entre API e Frontend

## Data: 8 de abril de 2026
## Versão: Frontend 0.8.3 | API 0.3.3

---

## RESUMO EXECUTIVO

Análise profunda identificou **7 inconsistências críticas** e **5 melhorias recomendadas** entre o frontend Blazor e a API.

### Inconsistências Críticas Encontradas:

1. ✅ **CORRIGIDO**: Campo `estado` - tipo incompatível (int vs string)
2. ✅ **CORRIGIDO**: Campos faltantes em `TombamentoRecord` (descricao, localizacao, valorestimado)
3. ⚠️ **PENDENTE**: Campo `situacao` não mapeado no frontend
4. ⚠️ **PENDENTE**: Campos de data não mapeados (databomb, dataestado, datasituacao)
5. ⚠️ **PENDENTE**: Campo `cdprod` não mapeado no frontend
6. ⚠️ **PENDENTE**: Metadados de captura não sincronizados (fotoKey, capturedBy, capturedAt, source)
7. ⚠️ **PENDENTE**: Inconsistência em `LocalizacaoDto` vs `LocalizacaoRecord`

---

## 1. CAMPO `situacao` NÃO MAPEADO

### Problema
A API retorna o campo `situacao` mas o frontend não o captura nem armazena.

### API (`TombamentoRecord`)
```csharp
[JsonPropertyName("situacao")] public string? Situacao { get; set; }
[JsonPropertyName("datasituacao")] public int? Datasituacao { get; set; }
```

### Frontend (`TombamentoWire`)
```csharp
// ❌ Campo situacao não existe
```

### Frontend (`PatrimonioItem`)
```csharp
// ❌ Campo situacao não existe
```

### Impacto
- **Severidade**: MÉDIA
- Perda de informação sobre a situação do bem (Ativo, Baixado, Transferido, etc.)
- Pode ser necessário para regras de negócio futuras

### Solução Recomendada
```csharp
// TombamentoWire
public string? situacao { get; set; }

// PatrimonioItem
public string? Situacao { get; set; }

// MapToStore
Situacao = w.situacao
```

---

## 2. CAMPOS DE DATA NÃO MAPEADOS

### Problema
A API retorna 3 campos de data que não são capturados pelo frontend.

### API (`TombamentoRecord`)
```csharp
[JsonPropertyName("databomb")] public int? Databomb { get; set; }
[JsonPropertyName("dataestado")] public int? Dataestado { get; set; }
[JsonPropertyName("datasituacao")] public int? Datasituacao { get; set; }
```

### Frontend
```csharp
// ❌ Nenhum desses campos existe
```

### Impacto
- **Severidade**: BAIXA-MÉDIA
- Perda de informação temporal sobre tombamento, mudanças de estado e situação
- Pode ser útil para auditoria e histórico

### Formato dos Dados
Os campos de data estão no formato `YYYYMMDD` (int), exemplo: `20240315` = 15/03/2024

### Solução Recomendada
```csharp
// TombamentoWire
public int? databomb { get; set; }
public int? dataestado { get; set; }
public int? datasituacao { get; set; }

// PatrimonioItem
public DateTime? DataTombamento { get; set; }
public DateTime? DataEstado { get; set; }
public DateTime? DataSituacao { get; set; }

// MapToStore - Converter int para DateTime
DataTombamento = ParseDateInt(w.databomb),
DataEstado = ParseDateInt(w.dataestado),
DataSituacao = ParseDateInt(w.datasituacao)

// Função auxiliar
private static DateTime? ParseDateInt(int? dateInt)
{
    if (!dateInt.HasValue || dateInt.Value == 0)
        return null;
    
    var str = dateInt.Value.ToString();
    if (str.Length != 8)
        return null;
    
    try
    {
        var year = int.Parse(str.Substring(0, 4));
        var month = int.Parse(str.Substring(4, 2));
        var day = int.Parse(str.Substring(6, 2));
        return new DateTime(year, month, day);
    }
    catch
    {
        return null;
    }
}
```

---

## 3. CAMPO `cdprod` NÃO MAPEADO

### Problema
A API retorna `cdprod` (código do produto) mas o frontend não o captura.

### API (`TombamentoRecord`)
```csharp
[JsonPropertyName("cdprod")] public int? Cdprod { get; set; }
```

### Frontend
```csharp
// ❌ Campo cdprod não existe
// ✅ Existe apenas deprod (descrição do produto)
```

### Impacto
- **Severidade**: BAIXA
- Perda do código numérico do produto
- O frontend já tem `deprod` (descrição) que pode ser suficiente

### Solução Recomendada
```csharp
// TombamentoWire
public int? cdprod { get; set; }

// PatrimonioItem
public int? CdProd { get; set; }

// MapToStore
CdProd = w.cdprod
```

---

## 4. METADADOS DE CAPTURA NÃO SINCRONIZADOS

### Problema
A API armazena metadados de captura mas o frontend não os sincroniza de volta.

### API (`TombamentoRecord`)
```csharp
[JsonPropertyName("fotoKey")] public string? FotoKey { get; set; }
[JsonPropertyName("capturedBy")] public string? CapturedBy { get; set; }
[JsonPropertyName("capturedAt")] public string? CapturedAt { get; set; }
[JsonPropertyName("source")] public string? Source { get; set; }
```

### Frontend (`TombamentoWire`)
```csharp
// ❌ Nenhum desses campos existe
```

### Frontend (`PatrimonioItem`)
```csharp
// ✅ Tem campos de reconhecimento mas não de captura
public DateTime? LastRecognized { get; set; }
public RecognitionSource? RecognitionSource { get; set; }
public float? RecognitionConfidence { get; set; }
```

### Impacto
- **Severidade**: MÉDIA-ALTA
- Perda de rastreabilidade de quem capturou o item
- Perda de informação sobre quando foi capturado
- Perda de referência à foto associada
- Dificulta auditoria e troubleshooting

### Solução Recomendada
```csharp
// TombamentoWire
public string? fotoKey { get; set; }
public string? capturedBy { get; set; }
public string? capturedAt { get; set; }
public string? source { get; set; }

// PatrimonioItem
public string? FotoKey { get; set; }
public string? CapturedBy { get; set; }
public DateTime? CapturedAt { get; set; }
public string? Source { get; set; }

// MapToStore
FotoKey = w.fotoKey,
CapturedBy = w.capturedBy,
CapturedAt = ParseDateTime(w.capturedAt),
Source = w.source

// Função auxiliar
private static DateTime? ParseDateTime(string? dateStr)
{
    if (string.IsNullOrWhiteSpace(dateStr))
        return null;
    
    if (DateTime.TryParse(dateStr, out var date))
        return date;
    
    return null;
}
```

---

## 5. INCONSISTÊNCIA EM `LocalizacaoDto`

### Problema
O DTO usado no frontend não tem o campo `dtestr` que existe na API.

### API (`LocalizacaoRecord`)
```csharp
public record LocalizacaoRecord(
    [property: JsonPropertyName("idlocalizacao")] long IdLocalizacao,
    [property: JsonPropertyName("cdorgao")] string CdOrgao,
    [property: JsonPropertyName("cdunid")] string CdUnid,
    [property: JsonPropertyName("cdarea")] string CdArea,
    [property: JsonPropertyName("cdsarea")] string CdSArea,
    [property: JsonPropertyName("dtestr")] int DtEstr = 0
);
```

### Frontend (`LocalizacaoDto`)
```csharp
public record LocalizacaoDto(
    long idlocalizacao, 
    string cdorgao, 
    string cdunid, 
    string cdarea, 
    string cdsarea
);
// ❌ Campo dtestr não existe
```

### Impacto
- **Severidade**: BAIXA
- Perda de informação sobre exercício fiscal da localização
- Pode causar problemas se a API começar a filtrar por exercício

### Solução Recomendada
```csharp
public record LocalizacaoDto(
    long idlocalizacao, 
    string cdorgao, 
    string cdunid, 
    string cdarea, 
    string cdsarea,
    int dtestr = 0
);
```

---

## 6. VALIDAÇÃO DE TOMBAMENTO - CAMPOS FALTANTES

### Problema
O DTO `ValidateTombamentoDto` no frontend pode não ter todos os campos retornados pela API.

### API (`ValidateTombamentoResponse`)
```csharp
public record ValidateTombamentoResponse(
    bool Exists,
    long? IdPatomb,
    string? Nutomb,
    string? Esfera,
    string? Deprod,
    int? Cdprod,
    string? Estado,
    string? Situacao,
    long? IdLocalizacao
);
```

### Frontend
Preciso verificar se `ValidateTombamentoDto` existe e se tem todos os campos.

---

## 7. CAPTURE DTO - VERIFICAR CONSISTÊNCIA

### Problema Potencial
Verificar se `CaptureItemDto` no frontend está alinhado com `CaptureItemRequest` na API.

### API (`CaptureItemRequest`)
```csharp
public record CaptureItemRequest(
    string Prefixo,
    long IdPatomb,
    string Nutomb,
    string? Estado,
    string? Situacao,
    long? IdLocalizacao,
    string? FotoKey,
    string? CapturedBy,
    string? CapturedAt,
    string? Source
);
```

### Frontend
Preciso verificar `CaptureItemDto`.

---

## MELHORIAS RECOMENDADAS

### 1. Criar Enum para Situacao
Similar ao que foi feito com `ConservationState` para `estado`, criar enum para `situacao`:

```csharp
public enum SituacaoBem
{
    Ativo,
    Baixado,
    Transferido,
    EmManutencao,
    Inativo
}
```

### 2. Adicionar Validação de Tipos
Adicionar validação para garantir que conversões de tipo não falhem silenciosamente.

### 3. Logging de Campos Ignorados
Adicionar logs quando campos da API são ignorados durante deserialização.

### 4. Testes de Integração
Criar testes que validem a compatibilidade entre DTOs da API e Frontend.

### 5. Documentação de Contrato
Manter documentação atualizada do contrato de dados entre API e Frontend.

---

## PRIORIZAÇÃO DE CORREÇÕES

### Prioridade ALTA (Implementar Imediatamente)
1. ✅ Campo `estado` - **CORRIGIDO**
2. ✅ Campos faltantes (descricao, localizacao, valorestimado) - **CORRIGIDO**
3. ⚠️ Metadados de captura (fotoKey, capturedBy, capturedAt, source)

### Prioridade MÉDIA (Implementar em Sprint Atual)
4. Campo `situacao`
5. Inconsistência em `LocalizacaoDto`

### Prioridade BAIXA (Backlog)
6. Campos de data (databomb, dataestado, datasituacao)
7. Campo `cdprod`

---

## PRÓXIMOS PASSOS

1. Revisar e validar esta análise com a equipe
2. Criar issues/tasks para cada inconsistência
3. Implementar correções por ordem de prioridade
4. Criar testes de integração
5. Atualizar documentação de API

---

## NOTAS TÉCNICAS

### Convenções de Nomenclatura
- **API**: PascalCase com JsonPropertyName em camelCase
- **Frontend Wire**: camelCase (para deserialização JSON)
- **Frontend Models**: PascalCase (padrão C#)

### Estratégia de Migração
- Adicionar campos como nullable para não quebrar dados existentes
- Implementar conversões de tipo com fallback seguro
- Manter retrocompatibilidade durante transição

---

**Documento gerado automaticamente pela análise de código**
**Última atualização**: 8 de abril de 2026
