# RELATÓRIO DE ANÁLISE: Estrutura JSON e Fluxo de Dados

**Data:** 07/04/2026  
**Arquivos Analisados:** CE999_redistribuido.json e CE999_final.json  
**Objetivo:** Mapear diferenças estruturais e identificar inconsistências com a lógica de serialização/desserialização

---

## 1. RESUMO EXECUTIVO

Foram identificadas **diferenças críticas** entre os dois arquivos JSON que impactam diretamente o fluxo de dados do sistema. O arquivo `CE999_final.json` possui **104.061 linhas a mais** que o `CE999_redistribuido.json`, indicando dados históricos adicionais.

### Principais Achados:
- ✅ Estrutura JSON compatível com os modelos da aplicação
- ⚠️ Diferenças no campo `dtestr` (exercício fiscal)
- ⚠️ Diferenças no campo `idlocalizacao` (padrão de IDs)
- ⚠️ Problemas de encoding (caracteres especiais corrompidos no CE999_final.json)
- ⚠️ Volume de dados históricos diferente entre arquivos

---

## 2. COMPARAÇÃO ESTRUTURAL DOS ARQUIVOS

### 2.1 Metadados dos Arquivos

| Métrica | CE999_redistribuido.json | CE999_final.json | Diferença |
|---------|--------------------------|------------------|-----------|
| **Linhas** | 1.184.351 | 1.288.412 | +104.061 (+8.8%) |
| **Tamanho** | ~42 MB | ~46 MB | +4 MB |
| **Encoding** | UTF-8 (correto) | UTF-8 (com problemas) | Corrupção |

### 2.2 Estrutura Raiz (Compatível)

Ambos os arquivos seguem a estrutura esperada pelo modelo `UnifiedDataRecord`:

```json
{
  "cliente": "ce999",
  "usuarios": [...],
  "tabelas": {
    "xxorga": [...],
    "xxunid": [...],
    "paarea": [...],
    "pasarea": [...],
    "localizacao": [...],
    "tombamentos": [...]
  }
}
```

✅ **Compatibilidade:** Estrutura raiz está correta em ambos os arquivos.

---

## 3. ANÁLISE DETALHADA POR SEÇÃO

### 3.1 Seção `usuarios`

**Estrutura Esperada:**
```csharp
public record UserRecord(
    string IdUsuario,
    string NmUsuario,
    string PwdUsuario,
    string Esfera,
    string? NomeCompleto
);
```

**CE999_redistribuido.json:**
```json
{
  "idusuario": "1",
  "nmusuario": "ce999.nome1.sbnome1",
  "pwdusuario": "passwd",
  "esfera": "E"
}
```

**CE999_final.json:**
```json
{
  "idusuario": "1",
  "nmusuario": "ce999.nome1.sbnome1",
  "pwdusuario": "passwd",
  "esfera": "E"
}
```

✅ **Status:** Idênticos. Campo `nomecompleto` ausente em ambos (nullable no modelo).

---

### 3.2 Seção `tabelas.xxorga` (Órgãos)

**Estrutura Esperada:**
```csharp
public record XxOrgaRecord(
    string CdOrgao,
    string NmOrgao,
    int DtEstr = 0  // Exercício fiscal YYYYMMDD
);
```

#### ⚠️ DIFERENÇA CRÍTICA #1: Campo `dtestr`

**CE999_redistribuido.json:**
```json
{
  "dtestr": 20260101,
  "cdorgao": "01",
  "nmorgao": "Secretaria de Gabinete do Prefeito"
}
```
- **Exercício:** 2026 (ano atual)
- **Padrão:** Todos os registros com `dtestr: 20260101`
- **Interpretação:** Dados do exercício fiscal de 2026

**CE999_final.json:**
```json
{
  "dtestr": 19930101,
  "cdorgao": "99",
  "nmorgao": "Órgão 99 do Exercício 1993"
}
```
- **Exercício:** 1993-2026 (histórico completo)
- **Padrão:** Múltiplos registros por órgão, um para cada exercício
- **Interpretação:** Dados históricos de 33 anos

#### 🔴 IMPACTO NO SISTEMA:

1. **SessionConfig.AnoExercicio:**
   ```csharp
   public int AnoExercicio { get; set; }  // Derivado de DtEstr / 10000
   ```
   - CE999_redistribuido: Sempre 2026
   - CE999_final: Varia de 1993 a 2026

2. **Filtro de Órgãos:**
   - Sistema atual **NÃO filtra por exercício** ao carregar órgãos
   - Com CE999_final, o mesmo `cdorgao` aparece múltiplas vezes (um por ano)
   - **BUG POTENCIAL:** Dropdown de órgãos pode mostrar duplicatas

3. **Endpoint `/api/auth/login`:**
   ```csharp
   var orgaos = root.Tabelas?.XxOrga
       .Select(o => new OrgaoRecord(o.CdOrgao, o.NmOrgao, o.DtEstr))
       .ToList() ?? new();
   ```
   - **Problema:** Não há filtro por exercício atual
   - **Resultado:** Retorna TODOS os exercícios históricos

---

### 3.3 Seção `tabelas.xxunid` (Unidades)

**Estrutura Esperada:**
```csharp
public record XxUnidRecord(
    string CdOrgao,
    string CdUnid,
    string NmUnid
);
```

**CE999_redistribuido.json:**
```json
{
  "dtestr": 20260101,
  "cdorgao": "09",
  "cdunid": "09",
  "nmunid": "Secretaria de Cultura"
}
```

**CE999_final.json:**
- Não possui campo `dtestr` na amostra analisada
- Estrutura similar, mas sem exercício fiscal

✅ **Status:** Compatível, mas falta campo `dtestr` no modelo `XxUnidRecord`.

---

### 3.4 Seção `tabelas.paarea` (Áreas)

**Estrutura Esperada:**
```csharp
public record PaAreaRecord(
    string CdArea,
    string NmArea
);
```

**CE999_redistribuido.json:**
```json
{
  "cdarea": "001",
  "nmarea": "Deposito"
}
```

**CE999_final.json:**
- Estrutura idêntica
- Sem campo `dtestr`

✅ **Status:** Compatível.

---

### 3.5 Seção `tabelas.tombamentos` (Patrimônio)

**Estrutura Esperada:**
```csharp
public class TombamentoRecord
{
    public long IdPatomb { get; set; }
    public string Nutomb { get; set; }
    public int? Databomb { get; set; }
    public int? Cdprod { get; set; }
    public string Deprod { get; set; }
    public string? Estado { get; set; }
    public int? Dataestado { get; set; }
    public string? Situacao { get; set; }
    public int? Datasituacao { get; set; }
    public long? IdLocalizacao { get; set; }
    public string Esfera { get; set; }
    // ... metadados de captura
}
```

#### ⚠️ DIFERENÇA CRÍTICA #2: Campo `idlocalizacao`

**CE999_redistribuido.json (final do arquivo):**
```json
{
  "idpatomb": 263678936188,
  "nutomb": "01803731",
  "databomb": 20121031,
  "cdprod": 65351,
  "deprod": "CANETA ALTA ROTAA+O",
  "estado": "BOM",
  "dataestado": 20201130,
  "situacao": "Baixado",
  "datasituacao": 20201130,
  "idlocalizacao": 1000741,
  "esfera": "E"
}
```
- **Padrão:** IDs sequenciais (1000734, 1000735, ..., 1000741)
- **Range:** 1000000 - 1999999 (estimado)

**CE999_final.json (final do arquivo):**
```json
{
  "idpatomb": 263678936188,
  "nutomb": "01803731",
  "databomb": 20121031,
  "cdprod": 65351,
  "deprod": "CANETA ALTA ROTAA+O",
  "estado": "BOM",
  "dataestado": 20201130,
  "situacao": "Baixado",
  "datasituacao": 20201130,
  "idlocalizacao": 123456794368,
  "esfera": "E"
}
```
- **Padrão:** IDs sequenciais (123456794361, 123456794362, ..., 123456794368)
- **Range:** 123456000000 - 123456999999 (estimado)

#### 🔴 IMPACTO NO SISTEMA:

1. **Tabela `localizacao`:**
   - Sistema espera que `idlocalizacao` seja FK para `tabelas.localizacao`
   - Se os IDs não existem na tabela `localizacao`, há **referência órfã**

2. **Endpoint `/api/capture/item`:**
   ```csharp
   existing.IdLocalizacao = request.IdLocalizacao ?? existing.IdLocalizacao;
   ```
   - Atualiza `idlocalizacao` sem validar se existe na tabela
   - **BUG POTENCIAL:** Pode criar referências inválidas

---

### 3.6 Problema de Encoding

#### ⚠️ DIFERENÇA CRÍTICA #3: Corrupção de Caracteres

**CE999_redistribuido.json:**
```json
"nmorgao": "Secretaria de Trânsito e Transporte"
"nmorgao": "Sec da Segurança Pública e Defesa Civil"
"nmorgao": "Procuradoria Geral do Município"
```
- **Encoding:** UTF-8 correto
- **Caracteres especiais:** ã, ç, ê, ú renderizados corretamente

**CE999_final.json:**
```json
"nmorgao": "ï¿½rgï¿½o 99 do Exercï¿½cio 1993"
"nmorgao": "Cï¿½MARA MUNICIPAL DE AQUIRAZ"
"nmorgao": "SEC. DE ADMINISTRAï¿½ï¿½O DE AQUIRAZ"
```
- **Encoding:** UTF-8 com corrupção
- **Caracteres especiais:** Substituídos por `ï¿½` (replacement character)
- **Causa provável:** Conversão incorreta de ISO-8859-1 para UTF-8

#### 🔴 IMPACTO NO SISTEMA:

1. **Interface do Usuário:**
   - Dropdowns e labels exibem caracteres corrompidos
   - Experiência do usuário degradada

2. **Busca e Filtros:**
   - Busca por "Câmara" não encontra "Cï¿½MARA"
   - Filtros de texto podem falhar

3. **Relatórios:**
   - PDFs e exports com texto ilegível

---

## 4. ANÁLISE DO FLUXO DE SERIALIZAÇÃO/DESSERIALIZAÇÃO

### 4.1 Configuração Global de JSON

**API (Program.cs):**
```csharp
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
};
```

✅ **Compatibilidade:** Ambos os arquivos são compatíveis com essas configurações.

### 4.2 Fluxo Completo: Câmera → API → S3

```
┌─────────────┐
│  Camera.razor│
│  (Captura)  │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│ RecognitionService│
│ (QR/OCR/Barcode)│
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ InventoryItem   │
│ (IndexedDB)     │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ ItemSyncService │
│ (Batch Sync)    │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ CaptureItemDto  │
│ (Serialização)  │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ POST /api/capture│
│ /item           │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ S3 GetObject    │
│ (Lê JSON)       │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ UnifiedDataRecord│
│ (Desserialização)│
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ TombamentoRecord│
│ (Atualização)   │
└──────┬──────────┘
       │
       ▼
┌─────────────────┐
│ S3 PutObject    │
│ (Escreve JSON)  │
└─────────────────┘
```

### 4.3 Inconsistências Identificadas

#### 🔴 INCONSISTÊNCIA #1: Filtro de Exercício Ausente

**Problema:**
- Sistema não filtra órgãos por exercício fiscal atual
- Com CE999_final.json, retorna 33 anos de dados históricos

**Código Afetado:**
```csharp
// pwa-camera-poc-api/Program.cs (linha ~200)
app.MapPost("/api/auth/login", async (...) =>
{
    var orgaos = root.Tabelas?.XxOrga
        .Select(o => new OrgaoRecord(o.CdOrgao, o.NmOrgao, o.DtEstr))
        .ToList() ?? new();
    // ❌ Falta filtro: .Where(o => o.DtEstr / 10000 == anoAtual)
});
```

**Impacto:**
- Dropdown de órgãos com duplicatas
- Performance degradada (mais dados para serializar)
- Confusão do usuário (múltiplos "Gabinete do Prefeito")

---

#### 🔴 INCONSISTÊNCIA #2: Validação de `idlocalizacao`

**Problema:**
- Sistema não valida se `idlocalizacao` existe na tabela `localizacao`
- Pode criar referências órfãs

**Código Afetado:**
```csharp
// pwa-camera-poc-api/Program.cs (linha ~880)
existing.IdLocalizacao = request.IdLocalizacao ?? existing.IdLocalizacao;
// ❌ Falta validação: if (localizacoes.Any(l => l.IdLocalizacao == request.IdLocalizacao))
```

**Impacto:**
- Dados inconsistentes no S3
- Relatórios de localização quebrados
- Impossível rastrear onde o item foi capturado

---

#### 🔴 INCONSISTÊNCIA #3: Modelo `XxUnidRecord` Incompleto

**Problema:**
- Modelo não possui campo `dtestr` (exercício fiscal)
- JSON possui o campo, mas não é mapeado

**Código Afetado:**
```csharp
// pwa-camera-poc-api/Models/ApiModels.cs
public record XxUnidRecord(
    string CdOrgao,
    string CdUnid,
    string NmUnid
    // ❌ Falta: int DtEstr = 0
);
```

**Impacto:**
- Perda de informação histórica
- Impossível filtrar unidades por exercício
- Inconsistência com `XxOrgaRecord` (que possui `DtEstr`)

---

#### 🔴 INCONSISTÊNCIA #4: Encoding UTF-8

**Problema:**
- CE999_final.json possui caracteres corrompidos
- Sistema não trata conversão de encoding

**Código Afetado:**
```csharp
// pwa-camera-poc-api/Program.cs (linha ~870)
using (var obj = await s3.GetObjectAsync(bucket, s3Key))
using (var stream = obj.ResponseStream)
{
    root = await JsonSerializer.DeserializeAsync<UnifiedDataRecord>(stream, writeOptions);
    // ❌ Falta: Encoding.UTF8.GetString(Encoding.Convert(...))
}
```

**Impacto:**
- Interface com texto ilegível
- Busca e filtros quebrados
- Relatórios com caracteres corrompidos

---

## 5. MAPEAMENTO DE CAMPOS CRÍTICOS

### 5.1 Campos de Hierarquia

| Campo | Tipo | Origem | Destino | Validação |
|-------|------|--------|---------|-----------|
| `cdorgao` | string | xxorga | SessionConfig.OrgaoId | ✅ OK |
| `cdunid` | string | xxunid | SessionConfig.UnidadeId | ✅ OK |
| `cdarea` | string | paarea | SessionConfig.AreaId | ✅ OK |
| `cdsarea` | string | pasarea | SessionConfig.SubareaId | ✅ OK |
| `dtestr` | int | xxorga | SessionConfig.DtEstr | ⚠️ Sem filtro |

### 5.2 Campos de Patrimônio

| Campo | Tipo | Origem | Destino | Validação |
|-------|------|--------|---------|-----------|
| `idpatomb` | long | tombamentos | CaptureItemDto.IdPatomb | ✅ OK |
| `nutomb` | string | tombamentos | CaptureItemDto.Nutomb | ✅ OK |
| `estado` | string | tombamentos | CaptureItemDto.Estado | ✅ OK |
| `situacao` | string | tombamentos | CaptureItemDto.Situacao | ✅ OK |
| `idlocalizacao` | long | tombamentos | CaptureItemDto.IdLocalizacao | ❌ Sem validação |
| `esfera` | string | tombamentos | CaptureItemDto (não mapeado) | ⚠️ Falta campo |

### 5.3 Campos de Captura (Metadados)

| Campo | Tipo | Origem | Destino | Validação |
|-------|------|--------|---------|-----------|
| `fotoKey` | string | CaptureItemDto | TombamentoRecord.FotoKey | ✅ OK |
| `capturedBy` | string | CaptureItemDto | TombamentoRecord.CapturedBy | ✅ OK |
| `capturedAt` | string | CaptureItemDto | TombamentoRecord.CapturedAt | ✅ OK |
| `source` | string | CaptureItemDto | TombamentoRecord.Source | ✅ OK |

---

## 6. PROBLEMAS IDENTIFICADOS E SEVERIDADE

| # | Problema | Severidade | Impacto | Arquivos Afetados |
|---|----------|------------|---------|-------------------|
| 1 | Filtro de exercício ausente | 🔴 CRÍTICO | Duplicatas no dropdown | CE999_final.json |
| 2 | Validação de `idlocalizacao` | 🔴 CRÍTICO | Referências órfãs | Ambos |
| 3 | Modelo `XxUnidRecord` incompleto | 🟡 MÉDIO | Perda de dados históricos | Ambos |
| 4 | Encoding UTF-8 corrompido | 🔴 CRÍTICO | Interface ilegível | CE999_final.json |
| 5 | Campo `esfera` não mapeado em DTO | 🟡 MÉDIO | Perda de contexto | Ambos |
| 6 | Padrão de `idlocalizacao` diferente | 🟡 MÉDIO | Inconsistência de dados | Ambos |
| 7 | Volume de dados históricos | 🟢 BAIXO | Performance | CE999_final.json |

---

## 7. RECOMENDAÇÕES

### 7.1 Correções Imediatas (Críticas)

1. **Adicionar filtro de exercício fiscal:**
   ```csharp
   var anoAtual = DateTime.UtcNow.Year;
   var orgaos = root.Tabelas?.XxOrga
       .Where(o => o.DtEstr / 10000 == anoAtual)
       .Select(o => new OrgaoRecord(o.CdOrgao, o.NmOrgao, o.DtEstr))
       .ToList() ?? new();
   ```

2. **Validar `idlocalizacao` antes de atualizar:**
   ```csharp
   if (request.IdLocalizacao.HasValue)
   {
       var localizacaoExists = root.Tabelas?.Localizacao
           .Any(l => l.IdLocalizacao == request.IdLocalizacao.Value) ?? false;
       
       if (!localizacaoExists)
           return Results.BadRequest(new { error = "idlocalizacao inválido" });
       
       existing.IdLocalizacao = request.IdLocalizacao;
   }
   ```

3. **Corrigir encoding UTF-8:**
   ```csharp
   // Opção 1: Reprocessar CE999_final.json com encoding correto
   // Opção 2: Adicionar conversão no código:
   using (var obj = await s3.GetObjectAsync(bucket, s3Key))
   using (var reader = new StreamReader(obj.ResponseStream, Encoding.UTF8))
   {
       var json = await reader.ReadToEndAsync();
       root = JsonSerializer.Deserialize<UnifiedDataRecord>(json, writeOptions);
   }
   ```

### 7.2 Melhorias de Médio Prazo

4. **Adicionar campo `dtestr` ao modelo `XxUnidRecord`:**
   ```csharp
   public record XxUnidRecord(
       string CdOrgao,
       string CdUnid,
       string NmUnid,
       int DtEstr = 0
   );
   ```

5. **Adicionar campo `esfera` ao `CaptureItemDto`:**
   ```csharp
   public class CaptureItemDto
   {
       // ... campos existentes
       public string? Esfera { get; set; }
   }
   ```

6. **Normalizar padrão de `idlocalizacao`:**
   - Decidir qual padrão usar (1000000+ ou 123456000000+)
   - Migrar dados para padrão único
   - Documentar range de IDs

### 7.3 Otimizações de Performance

7. **Implementar cache de exercício fiscal:**
   ```csharp
   var cacheKey = $"ORGAOS:{anoAtual}";
   if (!cache.TryGetValue(cacheKey, out List<OrgaoRecord> orgaos))
   {
       orgaos = root.Tabelas?.XxOrga
           .Where(o => o.DtEstr / 10000 == anoAtual)
           .Select(o => new OrgaoRecord(o.CdOrgao, o.NmOrgao, o.DtEstr))
           .ToList() ?? new();
       
       cache.Set(cacheKey, orgaos, TimeSpan.FromHours(24));
   }
   ```

8. **Adicionar índice de busca para `idlocalizacao`:**
   ```csharp
   // Criar HashSet para lookup O(1)
   var localizacaoIds = new HashSet<long>(
       root.Tabelas?.Localizacao.Select(l => l.IdLocalizacao) ?? Enumerable.Empty<long>()
   );
   ```

---

## 8. PLANO DE AÇÃO SUGERIDO

### Fase 1: Correções Críticas (1-2 dias)
- [ ] Implementar filtro de exercício fiscal no endpoint `/api/auth/login`
- [ ] Adicionar validação de `idlocalizacao` no endpoint `/api/capture/item`
- [ ] Reprocessar CE999_final.json com encoding UTF-8 correto
- [ ] Testar com ambos os arquivos JSON

### Fase 2: Melhorias de Modelo (2-3 dias)
- [ ] Adicionar campo `DtEstr` ao modelo `XxUnidRecord`
- [ ] Adicionar campo `Esfera` ao `CaptureItemDto`
- [ ] Atualizar testes unitários
- [ ] Validar serialização/desserialização

### Fase 3: Otimizações (3-5 dias)
- [ ] Implementar cache de exercício fiscal
- [ ] Adicionar índice de busca para `idlocalizacao`
- [ ] Normalizar padrão de IDs de localização
- [ ] Documentar estrutura de dados

### Fase 4: Validação e Deploy (2-3 dias)
- [ ] Testes de integração com ambos os arquivos
- [ ] Testes de performance com dados históricos
- [ ] Validação de encoding em produção
- [ ] Deploy gradual (canary)

---

## 9. CONCLUSÃO

Os dois arquivos JSON possuem **estruturas compatíveis** com os modelos da aplicação, mas apresentam **diferenças críticas** que impactam o funcionamento do sistema:

1. **CE999_redistribuido.json:** Dados do exercício fiscal de 2026, encoding correto, IDs de localização no range 1000000+
2. **CE999_final.json:** Dados históricos de 1993-2026, encoding corrompido, IDs de localização no range 123456000000+

O sistema atual **não está preparado** para lidar com dados históricos (múltiplos exercícios fiscais) e possui **falhas de validação** que podem gerar inconsistências.

As correções sugeridas são **essenciais** para garantir a integridade dos dados e a experiência do usuário, especialmente se o sistema for migrado para usar o arquivo CE999_final.json com dados históricos.

---

**Próximos Passos:**
Aguardando instruções para implementação do plano de ação ou esclarecimentos adicionais.
