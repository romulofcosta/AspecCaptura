# Bugfix Requirements Document

## Introduction

O sistema de reconhecimento (QR, OCR, Barcode) em um Blazor PWA usa JavaScript workers coordenados por `recognition-interop.js`. O C# chama métodos JS via `IJSRuntime`. Foram identificados três bugs que causam erros de interoperabilidade JS/C#: um método JS ausente (`processBarcodeImage`), uma chamada duplicada de cleanup, e uma condição de corrida onde o timer de cleanup dispara antes do objeto JS estar inicializado.

## Bug Analysis

### Current Behavior (Defect)

1.1 WHEN `BarcodeRecognitionService.DetectBarcodesAsync` é chamado com dados de imagem THEN o sistema falha com `Could not find 'recognitionInterop.processBarcodeImage'` porque o método não existe em `recognition-interop.js`

1.2 WHEN o timer `CleanupResources` dispara após 30s de inatividade E `window.recognitionInterop` ainda não foi inicializado pelo usuário THEN o sistema lança repetidamente `Could not find 'recognitionInterop.cleanupBarcodeWorker' ('cleanupBarcodeWorker' was undefined)` em loop

1.3 WHEN o método de cleanup é invocado no `BarcodeRecognitionService` THEN o sistema chama `recognitionInterop.cleanupBarcodeWorker` duas vezes consecutivas de forma idêntica, causando dupla tentativa de cleanup do worker

### Expected Behavior (Correct)

2.1 WHEN `BarcodeRecognitionService.DetectBarcodesAsync` é chamado com dados de imagem em base64 THEN o sistema SHALL invocar `recognitionInterop.processBarcodeImage` em `recognition-interop.js`, processar a imagem via `barcodeWorker` e retornar um JSON com os resultados de detecção

2.2 WHEN o timer `CleanupResources` dispara E `window.recognitionInterop` não está inicializado (`isInitialized === false`) THEN o sistema SHALL ignorar a chamada de cleanup sem lançar exceção

2.3 WHEN o método de cleanup é invocado no `BarcodeRecognitionService` THEN o sistema SHALL chamar `recognitionInterop.cleanupBarcodeWorker` exatamente uma vez

### Unchanged Behavior (Regression Prevention)

3.1 WHEN `recognitionInterop.initialize()` é chamado com todos os workers disponíveis THEN o sistema SHALL CONTINUE TO inicializar os três workers (QR, OCR, Barcode) e retornar `true`

3.2 WHEN `recognitionInterop.startRecognition` é chamado com um `videoElementId` válido THEN o sistema SHALL CONTINUE TO iniciar o processamento de frames em intervalo configurável

3.3 WHEN `recognitionInterop.cleanupBarcodeWorker` é chamado E o `barcodeWorker` está ativo THEN o sistema SHALL CONTINUE TO enviar a mensagem de `cleanup` ao worker corretamente

3.4 WHEN `BarcodeRecognitionService.DetectBarcodesAsync` recebe resultados válidos do JS THEN o sistema SHALL CONTINUE TO parsear, validar e ordenar os resultados por confiança antes de retorná-los

3.5 WHEN `recognitionInterop.processCurrentFrame` é chamado durante reconhecimento ativo THEN o sistema SHALL CONTINUE TO capturar o frame do vídeo e enviá-lo aos três workers simultaneamente
