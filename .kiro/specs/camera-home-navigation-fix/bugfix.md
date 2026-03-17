# Bugfix Requirements Document

## Introduction

Ao navegar da tela Home para a tela Camera no app Blazor WASM PWA, a câmera não é iniciada corretamente. O problema ocorre porque o JavaScript tenta localizar o elemento `#camera-feed` no DOM imediatamente após a navegação, mas o elemento ainda não foi renderizado pelo Blazor nesse momento. Além disso, o componente `Camera.razor` implementa `DisposeAsync()` sem declarar `@implements IAsyncDisposable`, fazendo com que o Blazor não chame o dispose automaticamente — o que causa vazamento de stream de câmera entre navegações.

## Bug Analysis

### Current Behavior (Defect)

1.1 WHEN o usuário navega de Home para Camera THEN o sistema chama `document.getElementById("camera-feed")` antes do elemento estar no DOM e retorna silenciosamente sem iniciar a câmera

1.2 WHEN `document.getElementById("camera-feed")` retorna `null` THEN o sistema não lança erro e não exibe nenhuma mensagem de falha ao usuário, deixando a tela de câmera em branco

1.3 WHEN o usuário sai da tela Camera (por navegação ou fechamento) THEN o sistema não chama `DisposeAsync()` automaticamente porque `@implements IAsyncDisposable` não está declarado no componente, causando vazamento do stream de câmera

1.4 WHEN há um stream de câmera vazado de uma navegação anterior THEN o sistema pode falhar ao tentar abrir a câmera novamente com o erro `NotReadableError` (câmera em uso)

### Expected Behavior (Correct)

2.1 WHEN o usuário navega de Home para Camera THEN o sistema SHALL aguardar o elemento `#camera-feed` estar disponível no DOM antes de chamar `getUserMedia` e atribuir o stream ao elemento

2.2 WHEN `document.getElementById("camera-feed")` retorna `null` após tentativas de espera THEN o sistema SHALL lançar um erro descritivo e exibir mensagem de falha ao usuário

2.3 WHEN o componente Camera é destruído pelo Blazor THEN o sistema SHALL chamar `DisposeAsync()` automaticamente, parando todos os tracks do stream e liberando o recurso de câmera

2.4 WHEN a câmera é iniciada com sucesso THEN o sistema SHALL exibir o feed de vídeo ao vivo no elemento `#camera-feed`

### Unchanged Behavior (Regression Prevention)

3.1 WHEN a câmera já está ativa e o usuário clica em "Trocar câmera" THEN o sistema SHALL CONTINUE TO parar o stream atual e iniciar um novo stream com a câmera alternada

3.2 WHEN o usuário captura uma foto THEN o sistema SHALL CONTINUE TO retornar os dados da imagem via `takePhoto` e exibir o formulário de preenchimento

3.3 WHEN o usuário fecha a câmera clicando em "Fechar" THEN o sistema SHALL CONTINUE TO parar o stream e navegar de volta para Home

3.4 WHEN a permissão de câmera é negada pelo usuário THEN o sistema SHALL CONTINUE TO exibir mensagem de erro adequada (`NotAllowedError`)

3.5 WHEN o reconhecimento automático está habilitado THEN o sistema SHALL CONTINUE TO iniciar o serviço de reconhecimento após a câmera estar ativa

---

## Bug Condition (Pseudocódigo)

```pascal
FUNCTION isBugCondition(X)
  INPUT: X de tipo CameraStartContext
  OUTPUT: boolean

  // O bug ocorre quando startCamera é chamado e o elemento DOM ainda não existe
  RETURN document.getElementById(X.videoElementId) = null
END FUNCTION
```

```pascal
// Property: Fix Checking — Elemento DOM disponível antes de getUserMedia
FOR ALL X WHERE isBugCondition(X) DO
  result ← startCamera'(X)
  ASSERT element_exists(X.videoElementId) AND stream_assigned(result)
END FOR
```

```pascal
// Property: Preservation Checking
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT startCamera(X) = startCamera'(X)
END FOR
```
