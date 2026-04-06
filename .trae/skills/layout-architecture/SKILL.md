---
name: "layout-architecture"
description: "Estrutura layouts Blazor (ShellLayout, MainLayout) de forma clara e escalável. Invoque ao definir a estrutura global da aplicação ou refatorar a navegação."
---

# Layout Architecture

Focada na organização estrutural da aplicação Blazor, utilizando herança de layouts e componentes persistentes.

## Diretrizes
- **ShellLayout**: Layout raiz para elementos globais como frames de visualização e containers de PWA.
- **MainLayout**: Layout para páginas autenticadas, contendo cabeçalhos, menus e rodapés.
- **AuthLayout**: Layout minimalista para fluxos de login e configuração.
- Organizar a navegação de forma intuitiva e responsiva.
