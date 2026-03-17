# Recognition Interop Bugfix Design

## Overview

Três bugs de interoperabilidade JS/C# afetam o serviço de reconhecimento de código de barras:

1. **Bug 1** — `processBarcodeImage` ausente em `recognition-interop.js`: o C# invoca o método via `IJSRuntime` mas ele não existe, causando `Could not find 'recognitionInterop.processBarcodeImage'`.
2. **Bug 2** — `cleanupBarcodeWorker` chamado em loop: o timer C# dispara a cada 30s mesmo antes de `window.recognitionInterop` estar inicializado, gerando exceções repetidas.
3. **Bug 3** — Dupla chamada de `cleanupBarcodeWorker` no C#: a linha ~194 de `BarcodeRecognitionService.cs` invoca o método duas vezes consecutivas de forma idêntica.

A estratégia de correção é cirúrgica: adicionar o método ausente no JS, adicionar guard de `isInitialized` no JS e no C#, e remover a chamada duplicada no C#.

## Glossary

- **Bug_Condition (C)**: Conjunto de entradas/estados que ativam um dos três bugs descritos acima
- **Property (P)**: Comportamento correto esperado quando a condição de bug se aplica
- **Preservation**: Comportamentos existentes que não devem ser alterados pela correção
- **processBarcodeImage**: Método a ser adicionado em `recognition-interop.js` que recebe base64 e retorna JSON com resultados de detecção via `barcodeWorker`
- **cleanupBarcodeWorker**: Método em `recognition-interop.js` que envia mensagem `cleanup` ao `barcodeWorker`; deve ser no-op quando `isInitialized === false`
- **CleanupResources**: Callback do timer em `BarcodeRecognitionService.cs` que invoca `cleanupBarcodeWorker` via JS — atualmente com chamada duplicada
- **isInitialized**: Flag booleana em `window.recognitionInterop` que indica se os workers foram inicializados com sucesso

## Bug Details

### Bug Condition

Os três bugs manifestam-se em condições distintas mas relacionadas ao ciclo de vida do objeto `window.recognitionInterop`.

**Formal Specification:**
```
FUNCTION isBugCondition(input)
  INPUT: input de tipo { operation: string, state: RecognitionInteropState }
  OUTPUT: boolean

  // Bug 1: método ausente
  IF input.operation = 'processBarcodeImage'
     AND 'processBarcodeImage' NOT IN window.recognitionInterop
  THEN RETURN true

  // Bug 2: cleanup antes da inicialização
  IF input.operation = 'cleanupBarcodeWorker'
     AND window.recognitionInterop.isInitialized = false
  THEN RETURN true

  // Bug 3: dupla chamada no C#
  IF input.operation = 'CleanupResources'
     AND countInvocations('recognitionInterop.cleanupBarcodeWorker', input.callStack) >= 2
  THEN RETURN true

  RETURN false
END FUNCTION
```

### Examples

- **Bug 1**: `BarcodeRecognitionService.DetectBarcodesAsync(imageBytes)` é chamado → JS lança `Could not find 'recognitionInterop.processBarcodeImage'` → C# recebe `JSException` → retorna `Array.Empty<BarcodeResult>()`
- **Bug 2**: App inicia, timer dispara após 30s sem que o usuário tenha aberto a câmera → `isInitialized = false` → JS lança `cleanupBarcodeWorker was undefined` repetidamente a cada 30s
- **Bug 3**: `CleanupResources` é chamado → mock JS registra 2 invocações de `cleanupBarcodeWorker` em vez de 1 → worker recebe mensagem `cleanup` duas vezes desnecessariamente

## Expected Behavior

### Preservation Requirements

**Unchanged Behaviors:**
- `recognitionInterop.initialize()` deve continuar inicializando os três workers (QR, OCR, Barcode) e retornar `true` quando todos estiverem prontos
- `recognitionInterop.startRecognition(videoElementId, intervalMs, dotNetRef)` deve continuar iniciando o processamento de frames no intervalo configurado
- `recognitionInterop.cleanupBarcodeWorker()` deve continuar enviando a mensagem `{ type: 'cleanup' }` ao `barcodeWorker` quando `isInitialized === true` e o worker estiver ativo
- `BarcodeRecognitionService.DetectBarcodesAsync` deve continuar parseando, validando e ordenando resultados por confiança quando o JS retornar dados válidos
- `recognitionInterop.processCurrentFrame` deve continuar capturando o frame do vídeo e enviando para os três workers simultaneamente

**Scope:**
Todas as entradas que NÃO envolvem os três cenários de bug devem ser completamente inalteradas. Isso inclui:
- Chamadas a `initialize()`, `startRecognition()`, `stopRecognition()`, `terminate()`
- Processamento de frames via `processCurrentFrame` e `processImageData`
- Callbacks JS→C# (`OnQRDetectedAsync`, `OnOCRDetectedAsync`, `OnBarcodeDetectedAsync`)
- Pipeline de parse/validação/ordenação no C# após receber resultados válidos do JS

## Hypothesized Root Cause

1. **Método não implementado (Bug 1)**: O `BarcodeRecognitionService.cs` foi escrito assumindo que `processBarcodeImage` existiria em `recognition-interop.js`, mas o método nunca foi adicionado ao objeto `window.recognitionInterop`. O fluxo de detecção via imagem estática (base64) é diferente do fluxo de detecção via frame de vídeo — o segundo usa `processCurrentFrame` com callback, o primeiro precisaria de uma Promise que aguarda o resultado do worker.

2. **Ausência de guard de inicialização no JS (Bug 2)**: `cleanupBarcodeWorker` verifica `this.barcodeWorker` mas não verifica `this.isInitialized`. Quando o timer C# dispara antes da inicialização, o JS tenta acessar `this.barcodeWorker` que pode ser `null`, mas o erro real é que o próprio objeto pode não estar pronto. A verificação correta deve ser `if (!this.isInitialized) return;`.

3. **Ausência de guard no C# (Bug 2 complementar)**: `CleanupResources` em `BarcodeRecognitionService.cs` não verifica `_isInitialized` antes de invocar o JS. Adicionar `if (!_isInitialized) return;` no início do bloco de cleanup evita a chamada JS desnecessária.

4. **Chamada duplicada por copiar/colar (Bug 3)**: A linha ~194 de `BarcodeRecognitionService.cs` contém duas chamadas consecutivas idênticas a `recognitionInterop.cleanupBarcodeWorker`. Provavelmente resultado de um erro de edição — uma das chamadas deve ser removida.

## Correctness Properties

Property 1: Bug Condition - processBarcodeImage retorna resultado válido

_For any_ entrada onde `isBugCondition` retorna `true` para o Bug 1 (chamada a `processBarcodeImage`), o método adicionado em `recognition-interop.js` SHALL processar a imagem base64 via `barcodeWorker` e retornar uma string JSON com array de resultados (podendo ser array vazio `"[]"` se nenhum código for detectado), nunca lançando `Could not find`.

**Validates: Requirements 2.1**

Property 2: Bug Condition - cleanupBarcodeWorker é no-op quando não inicializado

_For any_ estado onde `window.recognitionInterop.isInitialized === false`, a chamada a `recognitionInterop.cleanupBarcodeWorker()` SHALL retornar sem lançar exceção e sem enviar mensagem ao worker.

**Validates: Requirements 2.2**

Property 3: Bug Condition - cleanupBarcodeWorker invocado exatamente uma vez

_For any_ execução de `CleanupResources` no C# onde a condição de inatividade é satisfeita, o método `recognitionInterop.cleanupBarcodeWorker` SHALL ser invocado exatamente uma vez via `IJSRuntime`.

**Validates: Requirements 2.3**

Property 4: Preservation - comportamento inalterado para entradas não-bugadas

_For any_ operação onde `isBugCondition` retorna `false` (initialize, startRecognition, processCurrentFrame, callbacks, pipeline de parse), o código corrigido SHALL produzir exatamente o mesmo resultado que o código original, preservando todo o comportamento existente.

**Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5**

## Fix Implementation

### Changes Required

**File 1**: `pwa-camera-poc-blazor/wwwroot/js/recognition-interop.js`

**Specific Changes**:

1. **Adicionar método `processBarcodeImage`**: Implementar como Promise que envia mensagem `process-image` ao `barcodeWorker` com o base64, aguarda a resposta `barcode-result` via handler temporário com timeout, e retorna JSON string com array de resultados.

```
FUNCTION processBarcodeImage(base64Image, options)
  IF NOT this.isInitialized THEN RETURN "[]"
  IF NOT this.barcodeWorker THEN RETURN "[]"
  
  RETURN Promise(resolve =>
    handler = (e) =>
      IF e.data.type = 'barcode-result' THEN
        REMOVE handler
        IF e.data.success THEN resolve(JSON.stringify([e.data.result]))
        ELSE resolve("[]")
    
    ADD handler to barcodeWorker
    POST { type: 'process-image', data: { base64: base64Image, options: options } }
    
    TIMEOUT(5000) => resolve("[]")
  )
END FUNCTION
```

2. **Corrigir guard em `cleanupBarcodeWorker`**: Adicionar verificação de `isInitialized` no início do método:

```
FUNCTION cleanupBarcodeWorker()
  IF NOT this.isInitialized THEN RETURN  // guard adicionado
  IF this.barcodeWorker AND worker not terminated THEN
    POST { type: 'cleanup' }
END FUNCTION
```

**File 2**: `pwa-camera-poc-blazor/Services/Recognition/BarcodeRecognitionService.cs`

3. **Remover chamada duplicada**: Na linha ~194, remover uma das duas chamadas consecutivas idênticas a `recognitionInterop.cleanupBarcodeWorker`.

4. **Adicionar guard de `_isInitialized` no C#**: No início do bloco de cleanup do timer, adicionar:

```csharp
if (!_isInitialized) return;
```

**File 3**: `pwa-camera-poc-blazor/wwwroot/js/workers/barcode-worker.js`

5. **Adicionar handler para `process-image`**: O worker precisa suportar o novo tipo de mensagem `process-image` que recebe base64, converte para `ImageData` via `OffscreenCanvas` e processa com `processFrame`.

```
CASE 'process-image':
  IF data AND data.base64 THEN
    imageData = base64ToImageData(data.base64)
    result = await processFrame(imageData, data.options)
    POST barcode-result com result
```

## Testing Strategy

### Validation Approach

A estratégia segue duas fases: primeiro executar testes exploratórios no código não corrigido para confirmar os bugs e a análise de causa raiz; depois verificar a correção e a preservação.

### Exploratory Bug Condition Checking

**Goal**: Confirmar os três bugs no código não corrigido antes de implementar a correção.

**Test Plan**: Escrever testes que invocam os caminhos bugados e verificam as falhas esperadas. Executar no código ORIGINAL para observar as falhas e confirmar a causa raiz.

**Test Cases**:
1. **Bug 1 - Método ausente**: Chamar `recognitionInterop.processBarcodeImage` com base64 válido → deve lançar `JSException` com "Could not find" (falha no código original)
2. **Bug 2 - Cleanup antes da inicialização**: Criar `BarcodeRecognitionService`, aguardar 31s sem chamar `InitializeAsync`, verificar que `CleanupResources` lança exceção JS (falha no código original)
3. **Bug 3 - Dupla chamada**: Usar mock de `IJSRuntime` que conta invocações, chamar `CleanupResources` com inatividade > 30s, verificar que o mock registra 2 chamadas (falha no código original — esperamos 1)

**Expected Counterexamples**:
- `JSException: Could not find 'recognitionInterop.processBarcodeImage'`
- `JSException: Could not find 'recognitionInterop.cleanupBarcodeWorker' ('cleanupBarcodeWorker' was undefined)`
- Mock registra `InvokeVoidAsync("recognitionInterop.cleanupBarcodeWorker")` com count = 2

### Fix Checking

**Goal**: Verificar que para todas as entradas onde a condição de bug se aplica, o código corrigido produz o comportamento esperado.

**Pseudocode:**
```
FOR ALL input WHERE isBugCondition(input) DO
  result := fixedCode(input)
  ASSERT expectedBehavior(result)
END FOR
```

### Preservation Checking

**Goal**: Verificar que para todas as entradas onde a condição de bug NÃO se aplica, o código corrigido produz o mesmo resultado que o código original.

**Pseudocode:**
```
FOR ALL input WHERE NOT isBugCondition(input) DO
  ASSERT originalCode(input) = fixedCode(input)
END FOR
```

**Testing Approach**: Testes baseados em propriedades são recomendados para preservation checking porque:
- Geram automaticamente muitos casos de teste no domínio de entrada
- Capturam edge cases que testes unitários manuais podem perder
- Fornecem garantias fortes de que o comportamento é inalterado para todas as entradas não-bugadas

**Test Cases**:
1. **Preservation - initialize()**: Verificar que `initialize()` ainda retorna `true` e inicializa os 3 workers após a correção
2. **Preservation - startRecognition()**: Verificar que o processamento de frames continua funcionando com o intervalo configurado
3. **Preservation - cleanupBarcodeWorker com worker ativo**: Verificar que quando `isInitialized = true`, o cleanup ainda envia a mensagem `{ type: 'cleanup' }` ao worker
4. **Preservation - pipeline C# de parse/ordenação**: Verificar que resultados válidos retornados pelo JS ainda são parseados, validados e ordenados por confiança corretamente

### Unit Tests

- Testar `processBarcodeImage` com base64 válido → retorna JSON com array de resultados
- Testar `processBarcodeImage` com base64 inválido → retorna `"[]"` sem lançar exceção
- Testar `cleanupBarcodeWorker` quando `isInitialized = false` → retorna sem enviar mensagem
- Testar `cleanupBarcodeWorker` quando `isInitialized = true` → envia mensagem `cleanup` ao worker
- Testar `CleanupResources` no C# com `_isInitialized = false` → não invoca JS
- Testar `CleanupResources` no C# com inatividade > 30s → invoca JS exatamente uma vez

### Property-Based Tests

- Gerar base64 aleatórios (válidos e inválidos) e verificar que `processBarcodeImage` sempre retorna string JSON válida (nunca lança exceção)
- Gerar estados aleatórios de `isInitialized` (true/false) e verificar que `cleanupBarcodeWorker` nunca lança exceção
- Gerar múltiplas execuções de `CleanupResources` e verificar que o count de invocações JS é sempre ≤ 1 por execução

### Integration Tests

- Testar fluxo completo: `InitializeAsync` → `DetectBarcodesAsync(imageBytes)` → resultados parseados corretamente
- Testar que após 30s de inatividade sem inicialização, nenhuma exceção JS é lançada
- Testar que após 30s de inatividade com inicialização, o cleanup ocorre exatamente uma vez e o serviço continua funcional
