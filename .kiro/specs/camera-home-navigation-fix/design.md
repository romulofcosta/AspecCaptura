# Camera Home Navigation Fix — Bugfix Design

## Overview

Dois bugs relacionados ao ciclo de vida da câmera no componente `Camera.razor` precisam ser corrigidos:

1. **Race condition no JS**: `cameraInterop.startCamera` chama `document.getElementById("camera-feed")` imediatamente após a navegação Blazor, mas o elemento pode ainda não estar no DOM nesse momento. O resultado é um retorno silencioso sem iniciar a câmera.

2. **Vazamento de stream**: `Camera.razor` implementa `DisposeAsync()` mas não declara `@implements IAsyncDisposable`, fazendo com que o Blazor não invoque o dispose automaticamente ao destruir o componente. Isso causa vazamento do stream de câmera entre navegações e pode resultar em `NotReadableError` na próxima abertura.

A estratégia de correção é mínima e cirúrgica: adicionar polling/retry no JS e adicionar a declaração `@implements IAsyncDisposable` no componente.

## Glossary

- **Bug_Condition (C)**: A condição que dispara o bug — `document.getElementById(videoElementId)` retorna `null` no momento em que `startCamera` é chamado
- **Property (P)**: O comportamento correto esperado — o elemento DOM deve ser localizado (via polling) antes de `getUserMedia` ser chamado, e o stream deve ser atribuído ao elemento
- **Preservation**: Comportamentos existentes que não devem ser alterados pelo fix — troca de câmera, captura de foto, fechamento, tratamento de erros de permissão, reconhecimento automático
- **cameraInterop.startCamera**: Função em `wwwroot/js/camera-interop.js` que localiza o elemento `<video>` e inicia o stream via `getUserMedia`
- **Camera.razor**: Componente Blazor em `Pages/Camera.razor` que gerencia o ciclo de vida da câmera e implementa `DisposeAsync()`
- **IAsyncDisposable**: Interface .NET que, quando declarada com `@implements`, instrui o Blazor a chamar `DisposeAsync()` automaticamente ao destruir o componente
- **Race condition**: Situação em que `startCamera` é invocado antes do Blazor ter concluído a renderização do elemento `#camera-feed` no DOM

## Bug Details

### Bug Condition

O bug se manifesta em dois cenários distintos mas relacionados:

**Bug 1 — Race condition no JS**: Quando o usuário navega de Home para Camera, `OnAfterRenderAsync` é chamado e invoca `CameraService.StartCameraAsync`, que por sua vez chama `cameraInterop.startCamera`. Nesse momento, o elemento `#camera-feed` pode ainda não estar presente no DOM, pois a renderização Blazor é assíncrona. A função retorna silenciosamente sem iniciar a câmera.

**Bug 2 — Vazamento de stream**: O componente `Camera.razor` define `DisposeAsync()` mas não declara `@implements IAsyncDisposable`. Sem essa declaração, o Blazor não reconhece o componente como descartável e não chama `DisposeAsync()` ao navegar para fora da tela, deixando o stream de câmera ativo.

**Especificação Formal:**

```
FUNCTION isBugCondition(X)
  INPUT: X de tipo CameraStartContext
         X.videoElementId: string (ex: "camera-feed")
         X.elementPresentInDOM: boolean
  OUTPUT: boolean

  // Bug 1: elemento não está no DOM no momento da chamada
  IF document.getElementById(X.videoElementId) = null THEN
    RETURN true
  END IF

  RETURN false
END FUNCTION
```

```
FUNCTION isDisposeBugCondition(C)
  INPUT: C de tipo ComponentLifecycle
         C.implementsIAsyncDisposable: boolean
         C.blazorCallsDisposeOnDestroy: boolean
  OUTPUT: boolean

  // Bug 2: Blazor não chama DisposeAsync porque a interface não está declarada
  RETURN NOT C.implementsIAsyncDisposable
         AND NOT C.blazorCallsDisposeOnDestroy
END FUNCTION
```

### Examples

- **Bug 1 — Câmera em branco**: Usuário navega Home → Camera. Tela fica preta. Nenhum erro visível. `startCamera` retornou na linha `if (!video) return;` sem iniciar o stream.
- **Bug 1 — Elemento ausente**: `document.getElementById("camera-feed")` retorna `null` porque o Blazor ainda não renderizou o `<video>` no DOM no momento da chamada JS.
- **Bug 2 — Stream vazado**: Usuário navega Camera → Home. O stream continua ativo (LED da câmera permanece aceso). `DisposeAsync()` nunca foi chamado.
- **Bug 2 → Bug 1 encadeado**: Usuário tenta abrir a câmera novamente. `getUserMedia` falha com `NotReadableError: Could not start video source` porque o stream anterior ainda está em uso.

## Expected Behavior

### Preservation Requirements

**Comportamentos que devem permanecer inalterados:**
- Cliques em "Trocar câmera" devem continuar parando o stream atual e iniciando um novo com a câmera alternada
- Captura de foto via `takePhoto` deve continuar retornando os dados da imagem e exibindo o formulário
- Clique em "Fechar" deve continuar parando o stream e navegando de volta para Home
- Tratamento de `NotAllowedError` (permissão negada) deve continuar exibindo mensagem de erro adequada
- Reconhecimento automático deve continuar iniciando após a câmera estar ativa

**Escopo:**
Todos os inputs que NÃO envolvem a condição de bug (elemento já presente no DOM, ou interações que não dependem do timing de renderização) devem ser completamente não afetados por este fix. Isso inclui:
- Troca de câmera (elemento já está no DOM quando SwitchCamera é chamado)
- Captura de foto (câmera já está ativa)
- Fechamento da câmera
- Qualquer interação após a câmera ter sido iniciada com sucesso

## Hypothesized Root Cause

Com base na análise do código em `wwwroot/js/camera-interop.js` e `Pages/Camera.razor`:

1. **Retorno silencioso no JS (linha crítica)**: A função `startCamera` em `camera-interop.js` contém `if (!video) return;` — quando `getElementById` retorna `null`, a função simplesmente retorna sem lançar erro e sem tentar novamente. Não há mecanismo de retry ou espera.

2. **Timing de renderização Blazor**: `OnAfterRenderAsync(firstRender)` é chamado após o primeiro render do componente, mas o Blazor WASM pode ainda estar processando atualizações do DOM quando o JS é invocado via interop. O elemento `#camera-feed` só existe no DOM quando o bloco `@if (!isFormVisible)` é renderizado.

3. **Interface não declarada**: Em `Camera.razor`, a linha `public async ValueTask DisposeAsync()` implementa a interface, mas sem `@implements IAsyncDisposable` no topo do arquivo, o Blazor não registra o componente como `IAsyncDisposable` e não chama o método ao destruir o componente.

4. **Ausência de tratamento de erro visível**: Quando `startCamera` retorna silenciosamente (Bug 1), nenhuma exceção é lançada, então o `try/catch` em `StartCamera()` no componente não captura nada e o usuário não recebe feedback.

## Correctness Properties

Property 1: Bug Condition — Polling garante elemento DOM antes de getUserMedia

_For any_ contexto de início de câmera onde `document.getElementById(videoElementId)` retorna `null` no momento da chamada, a função `startCamera` corrigida SHALL aguardar o elemento ficar disponível no DOM (via polling com até 10 tentativas e intervalo de 50ms) antes de chamar `getUserMedia`, e SHALL lançar um erro descritivo caso o elemento não seja encontrado após todas as tentativas.

**Validates: Requirements 2.1, 2.2**

Property 2: Preservation — Comportamento inalterado quando elemento já está no DOM

_For any_ contexto de início de câmera onde `document.getElementById(videoElementId)` retorna um elemento válido imediatamente (isBugCondition retorna false), a função `startCamera` corrigida SHALL produzir exatamente o mesmo resultado que a função original, preservando o comportamento de inicialização de câmera, troca de câmera, captura de foto e tratamento de erros de permissão.

**Validates: Requirements 3.1, 3.2, 3.3, 3.4, 3.5**

## Fix Implementation

### Changes Required

Assumindo que a análise de causa raiz está correta:

**Arquivo 1**: `pwa-camera-poc-blazor/wwwroot/js/camera-interop.js`

**Função**: `startCamera`

**Mudanças específicas**:

1. **Substituir retorno silencioso por polling/retry**: Remover `if (!video) return;` e substituir por um loop de polling que tenta localizar o elemento até 10 vezes com intervalo de 50ms entre tentativas.

2. **Lançar erro descritivo após tentativas esgotadas**: Se após 10 tentativas (500ms total) o elemento ainda não for encontrado, lançar `new Error("Elemento #camera-feed não encontrado no DOM após 10 tentativas. Verifique se o componente foi renderizado.")`.

3. **Manter lógica existente após localizar o elemento**: Todo o código de `getUserMedia`, atribuição de `srcObject` e tratamento de erros permanece inalterado.

```
// Pseudocódigo da mudança no JS
FUNCTION startCamera(videoElementId, facingMode)
  attempts ← 0
  maxAttempts ← 10
  intervalMs ← 50

  WHILE attempts < maxAttempts DO
    video ← document.getElementById(videoElementId)
    IF video ≠ null THEN
      BREAK
    END IF
    AWAIT sleep(intervalMs)
    attempts ← attempts + 1
  END WHILE

  IF video = null THEN
    THROW Error("Elemento #" + videoElementId + " não encontrado no DOM após " + maxAttempts + " tentativas")
  END IF

  // ... resto da lógica existente (getUserMedia, srcObject, etc.)
END FUNCTION
```

**Arquivo 2**: `pwa-camera-poc-blazor/Pages/Camera.razor`

**Mudança**: Adicionar `@implements IAsyncDisposable` nas diretivas do componente (após as diretivas `@inject` existentes).

**Mudança específica**:
- Adicionar a linha `@implements IAsyncDisposable` antes do bloco `<PageTitle>` ou junto às outras diretivas no topo do arquivo.
- Nenhuma alteração no método `DisposeAsync()` é necessária — ele já está implementado corretamente.

## Testing Strategy

### Validation Approach

A estratégia de testes segue duas fases: primeiro, evidenciar os bugs no código não corrigido (exploração); depois, verificar que o fix funciona corretamente e que os comportamentos existentes foram preservados.

### Exploratory Bug Condition Checking

**Objetivo**: Evidenciar os bugs ANTES de implementar o fix. Confirmar ou refutar a análise de causa raiz.

**Plano de teste**: Escrever testes que simulem a chamada de `startCamera` quando o elemento DOM não está presente, e verificar que o componente não chama `DisposeAsync` automaticamente sem a declaração da interface.

**Casos de teste**:
1. **Elemento ausente no momento da chamada** (falhará no código não corrigido): Chamar `cameraInterop.startCamera("camera-feed", "environment")` quando não há elemento `#camera-feed` no DOM — espera-se que retorne silenciosamente sem erro.
2. **Elemento presente com delay** (falhará no código não corrigido): Adicionar o elemento ao DOM 100ms após chamar `startCamera` — espera-se que a câmera não inicie porque não há retry.
3. **DisposeAsync não chamado** (falhará no código não corrigido): Destruir o componente Camera sem a declaração `@implements IAsyncDisposable` e verificar que `DisposeAsync` não é invocado pelo Blazor.
4. **NotReadableError por stream vazado** (edge case): Iniciar câmera, navegar sem dispose, tentar iniciar câmera novamente — espera-se `NotReadableError`.

**Contraexemplos esperados**:
- `startCamera` retorna sem iniciar o stream quando elemento está ausente
- Nenhuma exceção é lançada (retorno silencioso)
- Stream permanece ativo após navegação (LED da câmera aceso)

### Fix Checking

**Objetivo**: Verificar que para todos os inputs onde a condição de bug se aplica, a função corrigida produz o comportamento esperado.

**Pseudocódigo:**
```
FOR ALL X WHERE isBugCondition(X) DO
  result ← startCamera_fixed(X.videoElementId, X.facingMode)
  ASSERT element_found_before_getUserMedia(X.videoElementId)
  ASSERT stream_assigned_to_element(result)
END FOR
```

```
FOR ALL C WHERE isDisposeBugCondition(C) DO
  destroyComponent(C)
  ASSERT DisposeAsync_was_called()
  ASSERT camera_stream_stopped()
END FOR
```

### Preservation Checking

**Objetivo**: Verificar que para todos os inputs onde a condição de bug NÃO se aplica, a função corrigida produz o mesmo resultado que a função original.

**Pseudocódigo:**
```
FOR ALL X WHERE NOT isBugCondition(X) DO
  ASSERT startCamera_original(X) = startCamera_fixed(X)
END FOR
```

**Abordagem de testes**: Testes baseados em propriedades são recomendados para preservation checking porque:
- Geram muitos casos de teste automaticamente no domínio de entrada
- Capturam edge cases que testes unitários manuais podem perder
- Fornecem garantias fortes de que o comportamento é preservado para todos os inputs não-bugados

**Casos de teste de preservação**:
1. **Troca de câmera**: Verificar que `SwitchCamera` continua funcionando após o fix (elemento já está no DOM)
2. **Captura de foto**: Verificar que `takePhoto` continua retornando dados de imagem válidos
3. **Fechamento da câmera**: Verificar que `CloseCamera` continua parando o stream e navegando para Home
4. **NotAllowedError**: Verificar que o tratamento de permissão negada continua funcionando

### Unit Tests

- Testar `startCamera` com elemento ausente → deve aguardar e encontrar após polling
- Testar `startCamera` com elemento ausente após todas as tentativas → deve lançar erro descritivo
- Testar `startCamera` com elemento presente imediatamente → comportamento idêntico ao original
- Testar que `DisposeAsync` é chamado pelo Blazor quando `@implements IAsyncDisposable` está declarado

### Property-Based Tests

- Gerar contextos aleatórios de início de câmera onde o elemento aparece no DOM após delay variável (0–500ms) e verificar que a câmera sempre inicia corretamente após o fix
- Gerar sequências aleatórias de navegação (Camera → Home → Camera) e verificar que nenhum stream é vazado após o fix
- Verificar que para qualquer input onde o elemento já está presente, o comportamento é idêntico antes e depois do fix

### Integration Tests

- Testar fluxo completo: Home → Camera → câmera inicia → captura foto → salva → Home
- Testar fluxo de navegação múltipla: Camera → Home → Camera (verificar que não há `NotReadableError`)
- Testar que o LED da câmera apaga ao navegar de Camera para Home (stream liberado pelo dispose)
- Testar reconhecimento automático após câmera iniciada via polling
