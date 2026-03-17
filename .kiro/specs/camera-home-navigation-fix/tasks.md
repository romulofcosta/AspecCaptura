# Implementation Plan

- [x] 1. Escrever teste exploratório de condição de bug (ANTES do fix)
  - **Property 1: Bug Condition** - Race Condition e Vazamento de Stream
  - **CRÍTICO**: Este teste DEVE FALHAR no código não corrigido — a falha confirma que o bug existe
  - **NÃO tente corrigir o teste ou o código quando ele falhar**
  - **NOTA**: Este teste codifica o comportamento esperado — ele validará o fix quando passar após a implementação
  - **OBJETIVO**: Evidenciar contraexemplos que demonstram os bugs
  - **Abordagem PBT Scoped**: Para bugs determinísticos, escopo a casos concretos de falha para garantir reprodutibilidade
  - Adicionar os testes em `pwa-camera-poc-blazor/tests/Pages/CameraComponentTests.cs`
  - **Teste 1a — Race condition (Bug 1)**: Simular chamada de `cameraInterop.startCamera("camera-feed", "environment")` quando não há elemento `#camera-feed` no DOM. Verificar que o mock de JSRuntime é chamado e que o retorno é silencioso (sem exceção). Confirmar que a câmera NÃO foi iniciada (stream não atribuído).
  - **Teste 1b — Elemento com delay (Bug 1)**: Simular cenário onde o elemento aparece no DOM 100ms após a chamada de `startCamera`. Verificar que sem retry, a câmera não inicia.
  - **Teste 1c — DisposeAsync não chamado (Bug 2)**: Verificar que sem `@implements IAsyncDisposable`, o Blazor não invoca `DisposeAsync` automaticamente ao destruir o componente. Usar bUnit para renderizar e descartar o componente e confirmar que o stream não é parado.
  - Executar os testes no código NÃO corrigido
  - **RESULTADO ESPERADO**: Testes FALHAM (isso é correto — prova que os bugs existem)
  - Documentar os contraexemplos encontrados (ex: `startCamera` retorna sem iniciar stream, `DisposeAsync` não é chamado)
  - Marcar tarefa como completa quando os testes estiverem escritos, executados e as falhas documentadas
  - _Requirements: 1.1, 1.2, 1.3_

- [x] 2. Escrever testes de preservação (ANTES do fix)
  - **Property 2: Preservation** - Comportamentos Existentes Não Afetados
  - **IMPORTANTE**: Seguir metodologia observation-first
  - Adicionar os testes em `pwa-camera-poc-blazor/tests/Pages/CameraComponentTests.cs`
  - **Observar no código não corrigido**: Executar os cenários abaixo e registrar o comportamento atual
  - **Teste 2a — Troca de câmera**: Observar que `SwitchCamera` para o stream atual e inicia novo stream com câmera alternada quando o elemento já está no DOM. Escrever teste que verifica esse comportamento.
  - **Teste 2b — Captura de foto**: Observar que `TakePhotoAsync` retorna dados de imagem não-nulos quando a câmera está ativa. Escrever teste que verifica esse comportamento.
  - **Teste 2c — Fechamento da câmera**: Observar que `CloseCamera` para o stream e navega para `/home`. Escrever teste que verifica esse comportamento.
  - **Teste 2d — NotAllowedError**: Observar que quando `getUserMedia` lança `NotAllowedError`, o componente exibe mensagem de erro adequada. Escrever teste que verifica esse comportamento.
  - Executar os testes no código NÃO corrigido
  - **RESULTADO ESPERADO**: Testes PASSAM (confirma comportamento baseline a preservar)
  - Marcar tarefa como completa quando os testes estiverem escritos, executados e passando no código não corrigido
  - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_

- [x] 3. Fix: race condition e vazamento de stream

  - [x] 3.1 Implementar polling/retry em `cameraInterop.startCamera`
    - Arquivo: `pwa-camera-poc-blazor/wwwroot/js/camera-interop.js`
    - Remover `if (!video) return;` da função `startCamera`
    - Substituir por loop de polling: até 10 tentativas com intervalo de 50ms entre cada uma
    - Se após 10 tentativas o elemento ainda não for encontrado, lançar `new Error("Elemento #" + videoElementId + " não encontrado no DOM após 10 tentativas. Verifique se o componente foi renderizado.")`
    - Manter toda a lógica existente de `getUserMedia`, `srcObject` e tratamento de erros após localizar o elemento
    - _Bug_Condition: `document.getElementById(videoElementId) === null` no momento da chamada (isBugCondition do design)_
    - _Expected_Behavior: elemento localizado via polling antes de `getUserMedia`; erro descritivo lançado se não encontrado após 10 tentativas × 50ms_
    - _Preservation: lógica de `getUserMedia`, `srcObject`, tratamento de `NotAllowedError`/`NotFoundError`/`NotSupportedError`/`NotReadableError` permanece inalterada_
    - _Requirements: 2.1, 2.2_

  - [x] 3.2 Adicionar `@implements IAsyncDisposable` em `Camera.razor`
    - Arquivo: `pwa-camera-poc-blazor/Pages/Camera.razor`
    - Adicionar a diretiva `@implements IAsyncDisposable` junto às outras diretivas no topo do arquivo (após os `@inject`)
    - Nenhuma alteração no método `DisposeAsync()` é necessária — ele já está implementado corretamente
    - _Bug_Condition: `@implements IAsyncDisposable` ausente → Blazor não chama `DisposeAsync` ao destruir o componente (isDisposeBugCondition do design)_
    - _Expected_Behavior: Blazor invoca `DisposeAsync` automaticamente ao navegar para fora da tela Camera, parando todos os tracks do stream_
    - _Preservation: comportamento do `DisposeAsync` existente (parar reconhecimento, parar câmera, limpar eventos) permanece inalterado_
    - _Requirements: 2.3_

  - [x] 3.3 Verificar que o teste exploratório de condição de bug agora passa
    - **Property 1: Expected Behavior** - Race Condition e Vazamento de Stream Corrigidos
    - **IMPORTANTE**: Re-executar os MESMOS testes da tarefa 1 — NÃO escrever novos testes
    - Os testes da tarefa 1 codificam o comportamento esperado
    - Quando esses testes passarem, confirma que o comportamento esperado foi satisfeito
    - Executar os testes exploratórios do passo 1
    - **RESULTADO ESPERADO**: Testes PASSAM (confirma que os bugs foram corrigidos)
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

  - [x] 3.4 Verificar que os testes de preservação ainda passam
    - **Property 2: Preservation** - Comportamentos Existentes Não Afetados
    - **IMPORTANTE**: Re-executar os MESMOS testes da tarefa 2 — NÃO escrever novos testes
    - Executar os testes de preservação do passo 2
    - **RESULTADO ESPERADO**: Testes PASSAM (confirma que não há regressões)
    - Confirmar que todos os testes passam após o fix (sem regressões)

- [x] 4. Checkpoint — Executar toda a suite de testes
  - Executar a suite completa do projeto Blazor: `dotnet test pwa-camera-poc-blazor/tests`
  - Executar a suite completa do projeto API: `dotnet test pwa-camera-poc-api/tests`
  - Todos os testes devem passar. Em caso de falhas inesperadas, investigar e corrigir antes de prosseguir.
  - Perguntar ao usuário se houver dúvidas sobre falhas encontradas.
