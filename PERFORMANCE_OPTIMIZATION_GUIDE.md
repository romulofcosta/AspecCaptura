# 🚀 Performance Optimization Guide

**Data**: 9 de abril de 2026  
**Spec**: pwa-mobile-design-system-refactor

---

## 📋 Visão Geral

Este guia documenta todas as otimizações de performance implementadas no AspecCaptura PWA para garantir carregamento rápido, renderização suave e experiência fluida em dispositivos móveis.

---

## ✅ Otimizações Implementadas

### 1. Lazy Loading de Componentes e Imagens

**Componente**: `Components/Base/OptimizedImage.razor`  
**Script**: `wwwroot/js/image-lazy-load.js`

#### Funcionalidades:
- ✅ Lazy loading nativo com `loading="lazy"`
- ✅ IntersectionObserver para controle preciso
- ✅ Blur-up placeholder effect
- ✅ Suporte a WebP com fallback
- ✅ Responsive images com srcset

#### Uso:
```razor
<OptimizedImage 
    Src="images/photo.jpg"
    WebPSrc="images/photo.webp"
    Srcset="images/photo-640w.jpg 640w, images/photo-1280w.jpg 1280w"
    Alt="Descrição da imagem"
    Width="800"
    Height="600"
    Lazy="true"
    UseWebP="true"
    UseResponsive="true" />
```

---

### 2. Otimização de Imagens

**Script**: `scripts/optimize-images.sh`

#### Processo de Otimização:
1. **Redimensionamento**: Máximo 1920px de largura
2. **Compressão**: JPEG quality 85%, WebP quality 80%
3. **Formatos**: Conversão para WebP com fallback JPEG/PNG
4. **Responsive**: Criação de variantes 640w, 1280w, 1920w
5. **Metadados**: Remoção de EXIF data

#### Como Usar:
```bash
cd AspecCaptura
chmod +x scripts/optimize-images.sh
./scripts/optimize-images.sh
```

#### Requisitos:
- ImageMagick: `brew install imagemagick` (macOS) ou `apt-get install imagemagick` (Linux)
- WebP tools: `brew install webp` (macOS) ou `apt-get install webp` (Linux)

---

### 3. Critical CSS Inlining

**Arquivo**: `wwwroot/css/critical.css`  
**Implementação**: Inlined em `wwwroot/index.html`

#### O que está incluído:
- ✅ CSS variables essenciais
- ✅ Reset e base styles
- ✅ Loading spinner
- ✅ Error UI
- ✅ Layout básico
- ✅ Typography crítica
- ✅ Botões primários
- ✅ Safe area insets
- ✅ Acessibilidade básica

#### Benefícios:
- Elimina render-blocking CSS
- Melhora First Contentful Paint (FCP)
- Reduz Cumulative Layout Shift (CLS)

---

### 4. CSS Containment e Content Visibility

**Arquivo**: `wwwroot/css/performance.css`

#### CSS Containment:
Isola componentes para otimizar layout, paint e style calculations:

```css
.card {
    contain: layout style;
}

.modal {
    contain: layout style paint;
}
```

#### Content Visibility:
Permite ao navegador pular renderização de conteúdo off-screen:

```css
.content-section {
    content-visibility: auto;
    contain-intrinsic-size: auto 500px;
}

.list-item {
    content-visibility: auto;
    contain-intrinsic-size: auto 80px;
}
```

#### Componentes Otimizados:
- Cards
- List items
- Modals e dialogs
- Navigation components
- Form components
- Badges e chips

---

### 5. Bundle Size Optimization

**Configuração**: `AspecCaptura.csproj`

#### Otimizações de Build:
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <RunAOTCompilation>true</RunAOTCompilation>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>partial</TrimMode>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
    <BlazorEnableCompression>true</BlazorEnableCompression>
</PropertyGroup>
```

#### Benefícios:
- **AOT Compilation**: Código compilado ahead-of-time para melhor performance
- **Trimming**: Remove código não utilizado
- **Compression**: Gzip/Brotli compression automática
- **Target**: Bundle < 500KB gzipped

---

### 6. GPU Acceleration

**Arquivo**: `wwwroot/css/performance.css`

#### Técnicas:
```css
.gpu-accelerated {
    transform: translateZ(0);
    backface-visibility: hidden;
    perspective: 1000px;
}

.smooth-scroll {
    -webkit-overflow-scrolling: touch;
    transform: translateZ(0);
}
```

#### Aplicado em:
- Animações
- Scroll containers
- Modals e overlays
- Transições

---

### 7. Animation Optimization

**Arquivo**: `wwwroot/css/animations.css`

#### Princípios:
- ✅ Apenas transform e opacity (60fps garantido)
- ✅ Evitar animação de layout properties
- ✅ Duração: 150-300ms para micro-interactions
- ✅ Easing: ease-out para entrances, ease-in para exits
- ✅ Suporte a prefers-reduced-motion

```css
@media (prefers-reduced-motion: no-preference) {
    .optimized-animation {
        transition: transform 0.3s ease-out, opacity 0.3s ease-out;
    }
}

@media (prefers-reduced-motion: reduce) {
    * {
        animation-duration: 0.01ms !important;
        transition-duration: 0.01ms !important;
    }
}
```

---

### 8. Font Loading Optimization

**Implementação**: `wwwroot/index.html`

#### Estratégias:
```html
<!-- Preconnect para Google Fonts -->
<link rel="preconnect" href="https://fonts.googleapis.com" crossorigin>
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>

<!-- Font display swap -->
<style>
    body {
        font-display: swap;
    }
</style>
```

#### Benefícios:
- Previne FOIT (Flash of Invisible Text)
- Mostra fallback font imediatamente
- Reduz CLS

---

### 9. Service Worker Caching

**Arquivo**: `wwwroot/service-worker.js`

#### Estratégias de Cache:
1. **App Shell**: Cache-First
2. **Static Assets**: Cache-First com atualização em background
3. **API Calls**: Network-First com timeout e fallback
4. **Images**: Cache-First

#### Benefícios:
- Carregamento instantâneo em visitas subsequentes
- Funcionamento offline
- Redução de uso de dados

---

### 10. Reduce Reflows e Layout Thrashing

**Arquivo**: `wwwroot/css/performance.css`

#### Técnicas:
```css
/* Reserve espaço para imagens */
img {
    aspect-ratio: attr(width) / attr(height);
    max-width: 100%;
    height: auto;
}

/* Previne layout shift */
.dynamic-content {
    min-height: var(--min-content-height, 100px);
}
```

---

## 📊 Métricas de Performance Alvo

### Core Web Vitals:
- **LCP (Largest Contentful Paint)**: < 2.5s ✅
- **FID (First Input Delay)**: < 100ms ✅
- **CLS (Cumulative Layout Shift)**: < 0.1 ✅

### Lighthouse Scores:
- **Performance**: ≥ 85 🎯
- **Accessibility**: ≥ 95 🎯
- **Best Practices**: ≥ 90 🎯
- **SEO**: ≥ 90 🎯
- **PWA**: ≥ 90 🎯

### Load Times:
- **FCP (First Contentful Paint)**: < 1.8s
- **TTI (Time to Interactive)**: < 3.8s
- **Speed Index**: < 3.4s

---

## 🔧 Ferramentas de Análise

### 1. Lighthouse (Chrome DevTools)
```bash
# Run Lighthouse audit
# Chrome DevTools > Lighthouse > Generate report
```

### 2. WebPageTest
```
https://www.webpagetest.org/
```

### 3. Chrome DevTools Performance
```
# Chrome DevTools > Performance > Record
```

### 4. Bundle Analyzer
```bash
# Analyze .NET bundle size
dotnet publish -c Release
# Check bin/Release/net8.0/publish/wwwroot/_framework/
```

---

## 📱 Mobile-Specific Optimizations

### 1. Touch Optimization
```css
.interactive {
    touch-action: manipulation; /* Prevent double-tap zoom */
    -webkit-tap-highlight-color: transparent;
}
```

### 2. Reduced Complexity
```css
@media (max-width: 768px) {
    .shadow-complex {
        box-shadow: var(--shadow-sm); /* Simpler shadows */
    }
    
    .backdrop-blur {
        backdrop-filter: none; /* Remove blur on mobile */
        background-color: rgba(0, 0, 0, 0.5);
    }
}
```

### 3. Safe Area Insets
```css
@supports (padding: env(safe-area-inset-top)) {
    body {
        padding-top: env(safe-area-inset-top);
        padding-bottom: env(safe-area-inset-bottom);
    }
}
```

---

## ✅ Checklist de Otimização

### Imagens:
- [ ] Converter para WebP com fallback
- [ ] Criar variantes responsive (640w, 1280w, 1920w)
- [ ] Adicionar lazy loading
- [ ] Comprimir com quality 80-85%
- [ ] Adicionar width/height para prevenir CLS

### CSS:
- [ ] Inline critical CSS
- [ ] Adicionar CSS containment
- [ ] Usar content-visibility para off-screen content
- [ ] Minificar CSS em produção
- [ ] Remover CSS não utilizado

### JavaScript:
- [ ] Enable AOT compilation
- [ ] Enable trimming
- [ ] Minificar em produção
- [ ] Lazy load componentes não críticos
- [ ] Usar service worker para caching

### Fonts:
- [ ] Preconnect para font providers
- [ ] Usar font-display: swap
- [ ] Subset fonts se possível
- [ ] Usar system fonts como fallback

### Animations:
- [ ] Apenas transform e opacity
- [ ] Duração 150-300ms
- [ ] Suporte a prefers-reduced-motion
- [ ] GPU acceleration

---

## 🎯 Próximos Passos

1. **Run Lighthouse Audit**: Verificar scores atuais
2. **Optimize Images**: Executar script de otimização
3. **Test on Real Devices**: Testar em dispositivos móveis reais
4. **Monitor Performance**: Configurar monitoring contínuo
5. **Iterate**: Melhorar baseado em métricas

---

## 📚 Recursos

- [Web.dev Performance](https://web.dev/performance/)
- [Chrome DevTools Performance](https://developer.chrome.com/docs/devtools/performance/)
- [Lighthouse](https://developers.google.com/web/tools/lighthouse)
- [WebPageTest](https://www.webpagetest.org/)
- [CSS Containment](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Containment)
- [Content Visibility](https://web.dev/content-visibility/)

---

**Mantido por**: Kiro AI Assistant  
**Última atualização**: 9 de abril de 2026
