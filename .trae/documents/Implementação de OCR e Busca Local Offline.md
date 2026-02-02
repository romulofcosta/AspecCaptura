# Implementação de OCR e Busca Local Offline para Aspec Capture

Este plano detalha a implementação do sistema de captura de patrimônio com OCR (Tesseract WASM) e leitura de códigos de barras, operando 100% offline.

## Etapa 1: Setup do Core WASM & Web Worker
1. **Criação do Worker**: Criar `wwwroot/js/ocr-worker.js` para isolar o processamento do Tesseract em uma thread separada.
2. **Carregamento Offline**: Configurar o carregamento do `tesseract-core.wasm` e do modelo de linguagem `por.traineddata` a partir da pasta local `wwwroot`.
3. **Interface do Worker**: Implementar o listener para mensagens `recognize` que processa bytes de imagem e retorna o texto extraído.

## Etapa 2: Pipeline de Visão Computacional (Pre-processing)
1. **Melhoria de Imagem**: Adicionar funções em `wwwroot/js/camera-interop.js` para:
    - Converter para escala de cinza (Grayscale).
    - Aplicar Thresholding (binarização) para destacar caracteres em placas metálicas reflexivas.
    - Recorte da ROI (Region of Interest) baseado no guia visual.
2. **Guia Visual**: Adicionar um overlay em `Pages/Camera.razor` que delimita a área onde a placa deve ser posicionada.

## Etapa 3: Integração Blazor Interop
1. **OcrService.cs**: Criar o serviço em C# para gerenciar a comunicação com o Web Worker.
2. **Configuração Tesseract**: Definir a whitelist de caracteres (`0-9`, `A-Z`, `-`, `/`, `.`) para aumentar a precisão e velocidade.
3. **Gerenciamento de Ciclo de Vida**: Implementar `IDisposable` para liberar os recursos do Worker ao navegar para fora da página.

## Etapa 4: Busca Local e Normalização
1. **Processamento de Resultados**: Aplicar Regex para limpar o texto retornado pelo OCR (ex: remover espaços extras, caracteres inválidos).
2. **Integração com IndexedDB**: Realizar busca automática no store `items` usando o índice `code`.
3. **Fluxo de UI**: 
    - Se houver match: Preencher automaticamente os dados do patrimônio no formulário.
    - Se não houver match: Permitir redigitação ou captura manual.

Deseja que eu prossiga com a implementação da Etapa 1?