# Débito Técnico — AspecCaptura

## PROPOSTA: Separar `usuarios/{PREFIX}.json` em dois arquivos

```
usuarios/{PREFIX}.json   ← DONO: legado desktop (Harbour), READ-ONLY para PWA
capturas/{PREFIX}.json   ← DONO: PWA, lido pelo legado para importar capturas de campo
```

### Motivação

- Elimina race condition em uso concorrente (legado regenerando o arquivo enquanto o PWA escreve)
- Elimina risco de sobrescrita pelo legado ao regenerar o arquivo de inventário
- Permite cache agressivo do arquivo de inventário (nunca muda pelo PWA)
- Simplifica drasticamente os endpoints de captura (sem leitura + modificação + escrita do arquivo completo)

### Dependência

Aprovação do time desktop para ler `capturas/{PREFIX}.json` ao importar capturas de campo.

### Impacto

Médio — requer mudança nos endpoints de captura (`/api/capture/item`, `/api/capture/sync`) e no `SyncService` do PWA para separar leitura de inventário de leitura de capturas.

### Prioridade

Baixa para o ciclo atual. Reavaliar após validação do protótipo em campo.
