# Camera Recognition Fix — Bugfix Design

## Overview

Dois bugs foram identificados na tela de câmera do app Blazor WASM PWA:

**Bug 1 — Badges QR/Barcode/OCR não são interativos:** Os elementos `<span class="scanner-badge">` em `Camera.razor` não possuem handler `@onclick`. O usuário não consegue selecionar ou alternar entre os modos de reconhecimento. O fix converte os badges em botões clicáveis que alternam o campo `selectedMode` (enum: QR, Barcode, OCR, All), e o `RecognitionService` usa os flags `Settings.QREnabled`, `Settings.OCREnabled`, `Settings.BarcodeEnabled` para filtrar qual worker processa frames.

**Bug 2 — DotNetObjectReference coletado pelo GC:** Em `StartRecognitionAsync`, o `DotNetObjectReference.Create(this)` é criado como variável local sem ser armazenado em campo do componente. O GC pode coletar a referência antes dos workers JS invocarem os callbacks `OnQRDetectedAsync`, `OnOCRDetectedAsync` ou `OnBarcodeDetectedAsync`, causando falha silenciosa no reconhecimento automático. O fix adiciona o campo `private DotNetObjectReference<RecognitionService>? _dotNetRef` e faz dispose em `StopRecognitionAsync`.

## Glossary

- **Bug_Condition (C)**: A condição que dispara o bug — badges sem `@onclick` (Bug 1) ou `_dotNetRef` não armazenado como campo (Bug 2)
- **Property (P)**: O comportamento correto esperado — badges respondem ao clique alternando `selectedMode`; callbacks JS são invocados com sucesso enquanto o reconhecimento está ativo
- **Preservation**: Comportamentos existentes que não devem ser alterados pelo fix — toggle Scanner On/Off, captura manual, auto-fill do formulário, dispose do componente
- **selectedMode**: Enum `RecognitionMode { All, QR, Barcode, OCR }` a ser adicionado em `Camera.razor` para controlar qual worker está ativo
- **_dotNetRef**: Campo `private DotNetObjectReference<RecognitionService>? _dotNetRef` a ser adicionado em `RecognitionService.cs` para manter a referência viva enquanto o reconhecimento estiver ativo
- **RecognitionService**: Serviço em `Services/Recognition/RecognitionService.cs` que coordena os workers JS via `recognitionInterop.startRecognition`
- **recognitionInterop**: Objeto JS em `wwwroot/js/recognition-interop.js` que gerencia os workers QR, OCR e Barcode e envia frames via `setInterval`

## Bug Details

### Bug Condition

**Bug 1** manifesta quando o usuário clica em um elemento `<span class="scanner-badge">` na tela de câmera. O elemento não possui `@onclick` nem qualquer handler interativo, portanto o clique é ignorado pelo Blazor.

**Formal Specification:**
```
FUNCTION isBugCondition_Badge(X)
  INPUT: X de tipo UIInteractionContext
  OUTPUT: boolean

  RETURN X.element.tagName = "SPAN"
     AND X.element.classList.contains("scanner-badge")
     AND X.element.onclick = null
     AND X.userAction = "click"
END FUNCTION
```

**Bug 2** manifesta quando `StartRecognitionAsync` é chamado e cria `DotNetObjectReference.Create(this)` como variável local `dotNetRef`. Após o retorno do método, nenhuma referência forte mantém o objeto vivo, permitindo que o GC o colete antes dos workers JS invocarem os callbacks.

```
FUNCTION isBugCondition_DotNetRef(X)
  INPUT: X de tipo RecognitionStartContext
  OUTPUT: boolean

  RETURN X.componentField._dotNetRef = null
     AND X.jsWorkerCallbackPending = true
     AND X.gcCollected = true
END FUNCTION
```

### Examples

**Bug 1:**
- Usuário clica no badge "QR" → nenhuma resposta visual, `selectedMode` não muda, todos os workers continuam ativos
- Usuário clica no badge "OCR" → nenhuma resposta visual, workers QR e Barcode continuam processando frames desnecessariamente
- Usuário clica no badge "Barcode" → nenhuma resposta visual, modo não é alterado

**Bug 2:**
- `StartRecognitionAsync` é chamado → `dotNetRef` criado localmente → método retorna → GC coleta `dotNetRef` → worker JS detecta QR → `invokeMethodAsync('OnQRDetectedAsync', ...)` falha com `ObjectDisposedException` ou silenciosamente → reconhecimento aparentemente ativo mas sem processar resultados
- Após navegação e retorno à tela de câmera → nova instância do componente → mesmo problema se reproduz

## Expected Behavior

### Preservation Requirements

**Unchanged Behaviors:**
- O botão "Scanner On/Off" (`scanner-toggle-btn`) deve continuar alternando `recognitionEnabled` e chamando `StartRecognition()` / `StopRecognition()`
- A detecção de código válido de patrimônio deve continuar disparando o evento `PatrimonioFound` e preenchendo automaticamente o formulário
- O botão de captura manual (`scanner-capture-btn`) deve continuar capturando o frame, parando a câmera e exibindo o formulário
- O `DisposeAsync` do componente deve continuar chamando `StopRecognitionAsync()` e liberando recursos
- A animação de scan-line e o viewfinder ativo devem continuar sendo exibidos quando `recognitionEnabled = true`
- Os overlays de detecção (qr-detection-box, ocr-text-region, barcode-detection-box) devem continuar sendo exibidos sobre o feed de vídeo

**Scope:**
Todos os inputs que NÃO envolvem clique nos badges de modo ou inicialização do reconhecimento devem ser completamente não afetados pelo fix. Isso inclui:
- Cliques no botão toggle Scanner On/Off
- Cliques no botão de captura manual
- Navegação entre telas
- Detecção e processamento de códigos pelos workers JS

## Hypothesized Root Cause

**Bug 1:**
1. **Ausência de `@onclick` nos badges**: Os elementos `<span class="scanner-badge">` foram criados apenas para exibição visual, sem considerar interatividade. Não há campo `selectedMode` no componente nem lógica de alternância de modo.
2. **Ausência de enum de modo**: Não existe um tipo `RecognitionMode` para representar os modos disponíveis (QR, Barcode, OCR, All), o que impede a implementação do toggle.
3. **Workers sempre ativos**: O `recognitionInterop.processCurrentFrame` envia frames para todos os workers sem verificar qual modo está selecionado, causando processamento desnecessário.

**Bug 2:**
1. **Variável local sem campo de instância**: Em `StartRecognitionAsync`, `var dotNetRef = DotNetObjectReference.Create(this)` cria a referência como variável local. Após o `await _jsRuntime.InvokeVoidAsync(...)`, o método retorna e a variável sai de escopo, tornando o objeto elegível para coleta pelo GC.
2. **Ausência de dispose explícito**: Mesmo que o objeto sobreviva ao GC por algum tempo, não há `Dispose()` chamado em `StopRecognitionAsync`, podendo causar vazamento de memória.
3. **Sem campo `_dotNetRef` na classe**: A classe `RecognitionService` não possui campo para armazenar a referência, o que é o padrão correto para objetos que precisam sobreviver além do escopo de um método.

## Correctness Properties

Property 1: Bug Condition — Badges Respondem ao Clique e Alternam Modo

_For any_ interação de clique em um badge de modo (QR, Barcode ou OCR) onde `isBugCondition_Badge` retorna true, o componente Camera corrigido SHALL atualizar `selectedMode` para o modo correspondente ao badge clicado, destacar visualmente o badge ativo, e configurar `Settings.QREnabled`, `Settings.BarcodeEnabled`, `Settings.OCREnabled` de acordo com o modo selecionado.

**Validates: Requirements 2.1, 2.2**

Property 2: Bug Condition — DotNetObjectReference Mantido Vivo

_For any_ chamada a `StartRecognitionAsync` onde `isBugCondition_DotNetRef` retornaria true no código original, o `RecognitionService` corrigido SHALL armazenar a referência em `_dotNetRef`, garantindo que callbacks JS (`OnQRDetectedAsync`, `OnOCRDetectedAsync`, `OnBarcodeDetectedAsync`) sejam invocados com sucesso enquanto o reconhecimento estiver ativo.

**Validates: Requirements 2.3, 2.5**

Property 3: Preservation — Comportamentos Existentes Inalterados

_For any_ input que NÃO seja clique em badge de modo nem inicialização do reconhecimento (isBugCondition_Badge = false AND isBugCondition_DotNetRef = false), o código corrigido SHALL produzir exatamente o mesmo resultado que o código original, preservando toggle Scanner On/Off, captura manual, auto-fill do formulário, dispose do componente e exibição de overlays de detecção.

**Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5, 3.6**

## Fix Implementation

### Changes Required

**Arquivo 1: `Pages/Camera.razor`**

**Mudanças específicas:**

1. **Adicionar enum `RecognitionMode`** (ou usar `RecognitionSource` existente):
   ```csharp
   private enum RecognitionMode { All, QR, Barcode, OCR }
   private RecognitionMode selectedMode = RecognitionMode.All;
   ```

2. **Converter `<span class="scanner-badge">` em botões clicáveis** com `@onclick` e classe CSS condicional para indicar modo ativo:
   ```razor
   <button class="scanner-badge @GetBadgeClass(RecognitionMode.QR)"
           @onclick="() => SelectMode(RecognitionMode.QR)" ...>
       QR
   </button>
   ```

3. **Adicionar método `SelectMode`** que atualiza `selectedMode` e configura os flags do `RecognitionService.Settings`:
   ```csharp
   private void SelectMode(RecognitionMode mode)
   {
       selectedMode = mode;
       RecognitionService.Settings.QREnabled = mode is RecognitionMode.All or RecognitionMode.QR;
       RecognitionService.Settings.BarcodeEnabled = mode is RecognitionMode.All or RecognitionMode.Barcode;
       RecognitionService.Settings.OCREnabled = mode is RecognitionMode.All or RecognitionMode.OCR;
   }
   ```

4. **Adicionar método `GetBadgeClass`** para retornar classe CSS correta baseada no modo selecionado.

---

**Arquivo 2: `Services/Recognition/RecognitionService.cs`**

**Mudanças específicas:**

1. **Adicionar campo `_dotNetRef`** na classe:
   ```csharp
   private DotNetObjectReference<RecognitionService>? _dotNetRef;
   ```

2. **Substituir variável local por campo** em `StartRecognitionAsync`:
   ```csharp
   // Antes (buggy):
   var dotNetRef = DotNetObjectReference.Create(this);
   await _jsRuntime.InvokeVoidAsync("recognitionInterop.startRecognition", videoElementId, Settings.ProcessingIntervalMs, dotNetRef);

   // Depois (fixed):
   _dotNetRef = DotNetObjectReference.Create(this);
   await _jsRuntime.InvokeVoidAsync("recognitionInterop.startRecognition", videoElementId, Settings.ProcessingIntervalMs, _dotNetRef);
   ```

3. **Fazer dispose em `StopRecognitionAsync`**:
   ```csharp
   _dotNetRef?.Dispose();
   _dotNetRef = null;
   ```

---

**Arquivo 3: `wwwroot/js/recognition-interop.js`** (opcional — pode ser feito no C#)

O filtro de workers por modo pode ser implementado no lado C# via `Settings.*Enabled`. Se necessário, o JS pode receber o modo ativo como parâmetro em `startRecognition` e filtrar quais workers recebem frames em `processCurrentFrame`. A abordagem C# é preferida por manter a lógica de negócio no servidor.

## Testing Strategy

### Validation Approach

A estratégia segue duas fases: primeiro, reproduzir os bugs no código não corrigido para confirmar a causa raiz; depois, verificar que o fix funciona corretamente e não introduz regressões.

### Exploratory Bug Condition Checking

**Goal**: Reproduzir os bugs ANTES de implementar o fix. Confirmar ou refutar a análise de causa raiz.

**Test Plan**: Escrever testes que simulam cliques nos badges e verificam se `selectedMode` muda; escrever testes que verificam se `_dotNetRef` é nulo após `StartRecognitionAsync`. Executar no código NÃO corrigido para observar falhas.

**Test Cases:**

1. **Badge QR Click Test**: Simular clique no badge QR e verificar que `selectedMode` muda para `RecognitionMode.QR` (falhará no código não corrigido — `selectedMode` não existe)
2. **Badge Barcode Click Test**: Simular clique no badge Barcode e verificar que `Settings.BarcodeEnabled = true` e `Settings.QREnabled = false` (falhará — sem handler)
3. **Badge OCR Click Test**: Simular clique no badge OCR e verificar que apenas `Settings.OCREnabled = true` (falhará — sem handler)
4. **DotNetRef Field Test**: Após `StartRecognitionAsync`, verificar que `_dotNetRef != null` via reflection (falhará — campo não existe)
5. **DotNetRef Dispose Test**: Após `StopRecognitionAsync`, verificar que `_dotNetRef` foi disposed (falhará — sem dispose)

**Expected Counterexamples:**
- Cliques nos badges não alteram nenhum estado do componente
- `_dotNetRef` é null após `StartRecognitionAsync` (campo não existe na classe original)

### Fix Checking

**Goal**: Verificar que para todos os inputs onde a condição de bug se aplica, o código corrigido produz o comportamento esperado.

**Pseudocode:**
```
FOR ALL X WHERE isBugCondition_Badge(X) DO
  result ← handleBadgeClick'(X)
  ASSERT selectedMode(result) IN {QR, Barcode, OCR, All}
     AND activeBadge(result) = X.badgeMode
     AND Settings.QREnabled = (selectedMode IN {All, QR})
     AND Settings.BarcodeEnabled = (selectedMode IN {All, Barcode})
     AND Settings.OCREnabled = (selectedMode IN {All, OCR})
END FOR

FOR ALL X WHERE isBugCondition_DotNetRef(X) DO
  result ← startRecognition'(X)
  ASSERT _dotNetRef != null
     AND GC.isAlive(_dotNetRef)
     AND jsCallbackSucceeds(result)
END FOR
```

### Preservation Checking

**Goal**: Verificar que para todos os inputs onde a condição de bug NÃO se aplica, o código corrigido produz o mesmo resultado que o original.

**Pseudocode:**
```
FOR ALL X WHERE NOT isBugCondition_Badge(X) AND NOT isBugCondition_DotNetRef(X) DO
  ASSERT handleBadgeClick_original(X) = handleBadgeClick_fixed(X)
     AND startRecognition_original(X) = startRecognition_fixed(X)
END FOR
```

**Testing Approach**: Property-based testing é recomendado para preservation checking porque:
- Gera muitos casos de teste automaticamente no domínio de entrada
- Captura edge cases que testes unitários manuais podem perder
- Fornece garantias fortes de que o comportamento é preservado para todos os inputs não-buggy

**Test Cases:**
1. **Toggle Scanner On/Off Preservation**: Verificar que `ToggleRecognition()` continua alternando `recognitionEnabled` e chamando `StartRecognition()`/`StopRecognition()` após o fix
2. **Capture Button Preservation**: Verificar que `CapturePhoto()` continua capturando frame e exibindo formulário após o fix
3. **PatrimonioFound Event Preservation**: Verificar que `OnPatrimonioFound` continua preenchendo o formulário automaticamente após o fix
4. **Dispose Preservation**: Verificar que `DisposeAsync` continua chamando `StopRecognitionAsync` e liberando recursos após o fix

### Unit Tests

- Testar `SelectMode(RecognitionMode.QR)` → `Settings.QREnabled = true`, `Settings.BarcodeEnabled = false`, `Settings.OCREnabled = false`
- Testar `SelectMode(RecognitionMode.All)` → todos os flags `= true`
- Testar `SelectMode(RecognitionMode.Barcode)` → apenas `Settings.BarcodeEnabled = true`
- Testar que `_dotNetRef != null` após `StartRecognitionAsync`
- Testar que `_dotNetRef == null` após `StopRecognitionAsync`
- Testar que `GetBadgeClass(mode)` retorna classe CSS correta para modo ativo e inativo

### Property-Based Tests

- Gerar estados aleatórios de `selectedMode` e verificar que exatamente os flags corretos estão habilitados em `Settings`
- Gerar sequências aleatórias de `SelectMode` e verificar que o último modo selecionado é sempre o ativo
- Gerar múltiplos ciclos de `StartRecognitionAsync`/`StopRecognitionAsync` e verificar que `_dotNetRef` é sempre não-nulo durante ativo e nulo após parar

### Integration Tests

- Testar fluxo completo: abrir câmera → clicar badge QR → detectar código QR → auto-fill formulário
- Testar que após selecionar modo OCR, callbacks de QR e Barcode não são processados
- Testar que `StopRecognitionAsync` faz dispose correto do `_dotNetRef` e não causa `ObjectDisposedException` em chamadas subsequentes
- Testar que navegação para fora e retorno à tela de câmera reinicializa `_dotNetRef` corretamente
