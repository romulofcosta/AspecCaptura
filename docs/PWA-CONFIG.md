# Configuração PWA - ASPEC Capture

Este documento detalha a configuração do Progressive Web App (PWA) para garantir a melhor experiência de instalação e identidade visual.

## 🎨 Identidade Visual e Ícones

O aplicativo utiliza o logotipo oficial da ASPEC Informática, configurado para suportar múltiplos tamanhos e o recurso de ícones adaptativos (**Maskable**).

### Arquivos de Ícone
- **Localização**: `wwwroot/images/aspec_logo.png` (512x512 original)
- **Favicon**: Configurado no `index.html` apontando para o logotipo oficial.
- **Apple Touch Icons**: Configurados para dispositivos iOS no `index.html`.

### manifest.json
O arquivo de manifesto foi atualizado com as seguintes definições:
- **name**: "Aspec Capture"
- **short_name**: "Aspec Capture"
- **background_color**: `#003366` (Azul Escuro ASPEC)
- **theme_color**: `#003366`
- **icons**:
  - Tamanhos: 72x72, 96x96, 128x128, 192x192, 512x512.
  - **Purpose**: "any maskable" - Garante que o ícone se adapte corretamente a diferentes formatos de ícones do Android (círculos, quadrados, esquilos).

## 🌑 Temas e Contraste (Acessibilidade)

A aplicação suporta temas Claro e Escuro, com otimizações específicas para acessibilidade:
- **Tema Escuro**:
  - Background: `#121212` / `#1E1E1E`
  - Texto Primário: `#FFFFFF` (Contraste Máximo)
  - Texto Secundário: `#B0BEC5`
  - **Dynamic Notch**: Os labels dos campos "Outlined" possuem um background dinâmico que se adapta à cor da superfície do tema, garantindo que o texto do label não seja "cortado" por linhas de fundo.

## 📦 Service Worker
- Localização: `wwwroot/service-worker.js`
- Comportamento: Cache de assets estáticos e runtime do Blazor WASM.
- **Offline-First**: A aplicação carrega instantaneamente se o service worker já estiver instalado, mesmo sem conexão.

## 📱 Responsividade
- **Breaking Points**:
  - `xs`: Mobile (< 600px)
  - `sm`: Tablet (600px - 960px)
  - `md`: Desktop (> 960px)
- **Toque**: Campos de texto possuem altura aumentada (`label-fix` e estilos MudBlazor customizados) para facilitar o uso com dedos em telas pequenas.
