# Deployment Guide - Aspec Captura PWA

## Build de Produção

### Requisitos
- .NET 8.0 SDK
- Node.js (para minificação de assets)
- HTTPS obrigatório

### Build Command

```bash
dotnet publish -c Release -o ./publish
```

### Otimizações Habilitadas

O build de produção inclui:
- **AOT Compilation**: Compilação ahead-of-time para melhor performance
- **IL Trimming**: Remoção de código não utilizado
- **Brotli Compression**: Compressão de assets
- **CSS/JS Minification**: Minificação automática

### Bundle Size Target
- Bundle principal: ≤ 500KB (gzipped)
- Total assets: ≤ 2MB (gzipped)

## Infraestrutura

### Requisitos de Servidor

1. **HTTPS Obrigatório**
   - Certificado SSL/TLS válido
   - TLS 1.2 ou superior
   - Redirect automático HTTP → HTTPS

2. **Headers de Segurança**
   ```
   Strict-Transport-Security: max-age=31536000; includeSubDomains
   X-Content-Type-Options: nosniff
   X-Frame-Options: DENY
   X-XSS-Protection: 1; mode=block
   ```

3. **CORS Configuration**
   - Permitir origem do domínio da aplicação
   - Incluir credenciais se necessário

### Service Worker

O service worker é registrado automaticamente no `index.html`. Certifique-se de que:
- `service-worker.js` está acessível na raiz
- Cache é atualizado em novas versões
- Background sync está habilitado

## Atualização

### Processo de Atualização

1. Build nova versão
2. Deploy para servidor
3. Service worker detecta nova versão
4. Usuário é notificado
5. Usuário aceita reload
6. Nova versão é carregada

### Limpeza de Cache

O service worker automaticamente:
- Remove caches antigos na ativação
- Mantém apenas versão atual
- Atualiza app shell

## Monitoramento

### Métricas de Performance

A aplicação coleta automaticamente:
- Initial Page Load Time
- Time to Interactive (TTI)
- First Contentful Paint (FCP)
- Tempo de resposta a interações
- Frame rate durante animações

### Logging de Erros

Erros client-side são logados no console. Para produção, configure envio para servidor:

```javascript
// Em performance.js, descomentar:
fetch('/api/metrics', { method: 'POST', body: JSON.stringify(metrics) });
```

## Validação Pós-Deploy

### Checklist

- [ ] App é instalável (manifest válido)
- [ ] Service worker está registrado
- [ ] HTTPS está funcionando
- [ ] Certificado SSL é válido
- [ ] Redirect HTTP → HTTPS funciona
- [ ] Headers de segurança estão configurados
- [ ] Câmera funciona em dispositivos móveis
- [ ] Sincronização funciona
- [ ] Notificações push funcionam (se habilitadas)
- [ ] Performance: Load time < 3s em 3G
- [ ] Performance: TTI < 5s em 3G

### Testes em Dispositivos

Testar em:
- Android (Chrome, Samsung Internet)
- iOS (Safari)
- Desktop (Chrome, Firefox, Edge, Safari)

## Troubleshooting

### Service Worker não atualiza
- Limpar cache do navegador
- Verificar `updateViaCache: 'none'` no registro
- Incrementar versão em `service-worker.js`

### App não é instalável
- Validar `manifest.json`
- Verificar ícones (192x192 e 512x512)
- Confirmar HTTPS
- Verificar service worker registrado

### Performance ruim
- Verificar bundle size
- Habilitar compressão Brotli
- Verificar cache do service worker
- Reduzir animações em dispositivos lentos

## Suporte

Para problemas ou dúvidas, consulte:
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Arquitetura da aplicação
- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Guia de troubleshooting
- [README.md](./README.md) - Documentação geral
