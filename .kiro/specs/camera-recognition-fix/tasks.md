# Implementation Plan

- [x] 1. Escrever teste de exploração da condição de bug (Bug Condition)
  - **Property 1: Bug Condition** - Badges QR/Barcode/OCR sem @onclick e DotNetRef como variável local
  - **CRÍTICO**: Este teste DEVE FALHAR no código não corrigido — a falha confirma que o bug existe
  - **NÃO tente corrigir o teste ou o código quando ele falhar**
  - **NOTA**: Este teste codifica o comportamento esperado — ele validará o fix quando passar após a implementação
  - **OBJETIVO**: Expor contraexemplos que demonstram a existência dos bugs
  - **Abordagem PBT Scoped**: Para bugs determinísticos, escopo a casos concretos de falha para garantir reprodutibilidade
  - Bug 1 — Simular clique no badge QR e verificar que `selectedMode` muda para `RecognitionMode.QR` (falhará — campo não existe)
  - Bug 1 — Simular clique no badge Barcode e verificar que `Settings.BarcodeEnabled = true` e `Settings.QREnabled = false` (falhará — sem handler)
  - Bug 1 — Simular clique no badge OCR e verificar que apenas `Settings.OCREnabled = true` (falhará — sem handler)
  - Bug 2 — Após `StartRecognitionAsync`, verificar via reflection que `_dotNetRef != null` (falhará — campo não existe na classe original)
  - Bug 2 — Após `StopRecognitionAsync`, verificar que `_dotNetRef` foi disposed (falhará — sem dispose)
  - Executar no código NÃO corrigido
  - **RESULTADO ESPERADO**: Testes FALHAM (isso é correto — prova que os bugs existem)
  - Documentar contraexemplos encontrados para entender a causa raiz
  - Marcar tarefa como completa quando os testes estiverem escritos, executados e as falhas documentadas
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 2. Escrever testes de preservation (ANTES de implementar o fix)
  - **Property 2: Preservation** - Toggle Scanner On/Off, Captura Manual e Auto-fill preservados
  - **IMPORTANTE**: Seguir metodologia observation-first
  - Observar: `ToggleRecognition()` alterna `recognitionEnabled` e chama `StartRecognition()`/`StopRecognition()` no código não corrigido
  - Observar: `CapturePhoto()` captura frame, para câmera e exibe formulário no código não corrigido
  - Observar: `OnPatrimonioFound` preenche automaticamente `itemModel.Code`, `itemModel.Name`, `itemModel.Location` no código não corrigido
  - Observar: `DisposeAsync` chama `StopRecognitionAsync` e libera recursos no código não corrigido
  - Escrever teste de propriedade: para qualquer sequência de toggle (on/off), `recognitionEnabled` sempre reflete o último estado (de Preservation Requirements no design)
  - Escrever teste de propriedade: para qualquer `PatrimonioItem` válido, `AutoFillForm` sempre preenche os campos Code, Name, Location (de Preservation Requirements no design)
  - Verificar que os testes PASSAM no código não corrigido
  - **RESULTADO ESPERADO**: Testes PASSAM (confirma comportamento baseline a preservar)
  - Marcar tarefa como completa quando os testes estiverem escritos, executados e passando no código não corrigido
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5, 3.6_

- [x] 3. Fix — Badges interativos e DotNetRef como campo de instância

  - [x] 3.1 Adicionar enum `RecognitionMode` e campo `selectedMode` em `Camera.razor`
  - [x] 3.2 Converter `<span class="scanner-badge">` em `<button>` com `@onclick` em `Camera.razor`
  - [x] 3.3 Adicionar método `SelectMode` em `Camera.razor`
  - [x] 3.4 Adicionar método `GetBadgeClass` em `Camera.razor`
  - [x] 3.5 Adicionar campo `_dotNetRef` em `RecognitionService.cs`
  - [x] 3.6 Substituir variável local `dotNetRef` pelo campo `_dotNetRef` em `StartRecognitionAsync`
  - [x] 3.7 Adicionar dispose de `_dotNetRef` em `StopRecognitionAsync`
  - [x] 3.8 Verificar que o teste de exploração da condição de bug agora passa
  - [x] 3.9 Verificar que os testes de preservation ainda passam

- [x] 4. Checkpoint — Garantir que todos os testes passam
