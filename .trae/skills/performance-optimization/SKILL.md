---
name: "performance-optimization"
description: "Otimiza o carregamento e execução da aplicação Blazor PWA. Invoque ao identificar lentidão no carregamento, renderização pesada ou alto consumo de recursos."
---

# Performance Optimization

Focada em garantir que o PWA carregue rápido e responda instantaneamente às interações do usuário.

## Diretrizes
- Otimizar imagens e recursos estáticos.
- Implementar estratégias de cache via Service Workers.
- Utilizar `StateHasChanged()` de forma eficiente para evitar re-renderizações desnecessárias.
- Minimizar o tamanho do payload inicial e utilizar Lazy Loading onde aplicável.
