# Bugfix Requirements Document

## Introduction

Dois problemas foram identificados na tela de câmera (`Camera.razor`) do app Blazor WASM PWA:

**Problema 1 — Badges de modo não são interativos:** Os elementos visuais QR, Barcode e OCR são `<span>` com classe `scanner-badge`, sem handler de clique. O usuário não consegue selecionar ou alternar entre os modos de reconhecimento clicando neles.

**Problema 2 — Reconhecimento automático falha silenciosamente:** Embora `recognitionEnabled = true` por padrão e `StartRecognition()` seja chamado no `OnAfterRenderAsync`, o `DotNetObjectReference` criado dentro de `StartRecognitionAsync` pode ser coletado pelo GC antes dos workers JS invocarem os callbacks. Além disso, falhas de inicialização dos workers (QR, OCR, Barcode) são silenciadas sem feedback ao usuário, e o `setInterval` continua rodando mesmo quando todos os workers falharam.

## Bug Analysis

### Current Behavior (Defect)

1.1 WHEN o usuário clica em um badge de modo (QR, Barcode ou OCR) na tela de câmera THEN o sistema não responde ao clique, pois os badges são elementos `<span>` sem evento `@onclick` ou qualquer handler interativo

1.2 WHEN o usuário deseja alternar entre os modos de reconhecimento THEN o sistema não oferece mecanismo de seleção de modo, exibindo todos os badges sempre no mesmo estado visual (ativo ou inativo conforme `recognitionEnabled`)

1.3 WHEN `StartRecognitionAsync` é chamado e cria um `DotNetObjectReference` localmente THEN o sistema não mantém referência ao objeto no componente, permitindo que o GC o colete antes dos workers JS invocarem `OnQRDetectedAsync`, `OnOCRDetectedAsync` ou `OnBarcodeDetectedAsync`

1.4 WHEN um ou mais workers JS (qr-worker.js, ocr-worker.js, barcode-worker.js) falham na inicialização THEN o sistema silencia o erro via `.catch()` e continua o `setInterval` sem notificar o usuário, resultando em reconhecimento automático aparentemente ativo mas sem processar frames

1.5 WHEN todos os workers falham na inicialização THEN o sistema lança `throw new Error('Failed to initialize recognition workers')` no JS, mas o `StartRecognitionAsync` em C# captura a exceção e exibe apenas uma mensagem de toast genérica, sem indicar quais workers falharam

### Expected Behavior (Correct)

2.1 WHEN o usuário clica em um badge de modo (QR, Barcode ou OCR) THEN o sistema SHALL alternar o modo de reconhecimento selecionado, destacando visualmente o badge ativo e desativando os demais

2.2 WHEN um modo de reconhecimento é selecionado pelo usuário THEN o sistema SHALL processar frames apenas com o worker correspondente ao modo ativo, ignorando os demais

2.3 WHEN `StartRecognitionAsync` cria um `DotNetObjectReference` THEN o sistema SHALL armazenar a referência em um campo do componente (ex: `_dotNetRef`) para evitar coleta pelo GC enquanto o reconhecimento estiver ativo

2.4 WHEN um worker JS falha na inicialização THEN o sistema SHALL desabilitar o badge correspondente ao modo falho, exibir indicação visual de indisponibilidade e registrar o erro de forma que o usuário saiba qual modo está indisponível

2.5 WHEN o reconhecimento automático está ativo e pelo menos um worker está funcional THEN o sistema SHALL processar frames continuamente via `setInterval` e invocar os callbacks C# ao detectar códigos

### Unchanged Behavior (Regression Prevention)

3.1 WHEN o usuário clica no botão "Scanner On/Off" (`scanner-toggle-btn`) THEN o sistema SHALL CONTINUE TO alternar `recognitionEnabled` e chamar `StartRecognition()` ou `StopRecognition()` conforme o estado

3.2 WHEN o reconhecimento detecta um código válido de patrimônio THEN o sistema SHALL CONTINUE TO disparar o evento `PatrimonioFound` e preencher automaticamente o formulário

3.3 WHEN o usuário clica no botão de captura manual (`scanner-capture-btn`) THEN o sistema SHALL CONTINUE TO capturar o frame atual, parar a câmera e exibir o formulário de preenchimento

3.4 WHEN o componente Camera é destruído THEN o sistema SHALL CONTINUE TO chamar `StopRecognitionAsync()` e liberar o `DotNetObjectReference` via `DisposeAsync()`

3.5 WHEN a câmera está ativa e o reconhecimento está em execução THEN o sistema SHALL CONTINUE TO exibir a animação de scan-line e o viewfinder ativo

3.6 WHEN um código é detectado com sucesso THEN o sistema SHALL CONTINUE TO exibir o overlay de detecção correspondente (qr-detection-box, ocr-text-region ou barcode-detection-box) sobre o feed de vídeo

---

## Bug Condition (Pseudocódigo)

### Bug 1 — Badges não interativos

```pascal
FUNCTION isBugCondition_Badge(X)
  INPUT: X de tipo UIInteractionContext
  OUTPUT: boolean

  // O bug ocorre quando o elemento clicado é um badge de modo sem handler
  RETURN X.element.tagName = "SPAN"
     AND X.element.classList.contains("scanner-badge")
     AND X.element.onclick = null
END FUNCTION
```

```pascal
// Property: Fix Checking — Badge responde ao clique
FOR ALL X WHERE isBugCondition_Badge(X) DO
  result ← handleBadgeClick'(X)
  ASSERT selectedMode(result) IN {QR, Barcode, OCR}
     AND activeBadge(result) = X.badgeMode
END FOR
```

### Bug 2 — DotNetObjectReference coletado pelo GC

```pascal
FUNCTION isBugCondition_DotNetRef(X)
  INPUT: X de tipo RecognitionStartContext
  OUTPUT: boolean

  // O bug ocorre quando a referência não é mantida no componente
  RETURN X.componentField._dotNetRef = null
     AND X.jsWorkerCallbackPending = true
END FUNCTION
```

```pascal
// Property: Fix Checking — Referência mantida viva
FOR ALL X WHERE isBugCondition_DotNetRef(X) DO
  result ← startRecognition'(X)
  ASSERT X.componentField._dotNetRef != null
     AND GC.isAlive(X.componentField._dotNetRef)
     AND jsCallbackSucceeds(result)
END FOR
```

```pascal
// Property: Preservation Checking
FOR ALL X WHERE NOT isBugCondition_Badge(X) AND NOT isBugCondition_DotNetRef(X) DO
  ASSERT handleBadgeClick(X) = handleBadgeClick'(X)
     AND startRecognition(X) = startRecognition'(X)
END FOR
```
