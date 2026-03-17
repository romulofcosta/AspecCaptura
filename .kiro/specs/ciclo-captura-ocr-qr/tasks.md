# Plano de Implementação: Ciclo de Captura OCR/QR Code

## Visão Geral

Este plano implementa o sistema de reconhecimento automático de QR Code e OCR para captura de patrimônio no PWA Blazor Aspec Captura. A implementação segue uma abordagem incremental, integrando as funcionalidades de reconhecimento ao fluxo existente sem quebrar compatibilidade.

## Tarefas

- [ ] 1. Configurar estrutura base e interfaces
  - Criar interfaces principais para serviços de reconhecimento
  - Definir modelos de dados para QR, OCR e validação
  - Configurar injeção de dependência para novos serviços
  - _Requisitos: 1.1, 2.1, 3.1_

- [ ] 2. Implementar Web Workers para processamento
  - [ ] 2.1 Criar QR Worker com ZXing-js
    - Implementar qr-worker.js com detecção de QR Code
    - Configurar carregamento assíncrono da biblioteca ZXing-js
    - Implementar processamento de frames e retorno de resultados
    - _Requisitos: 1.1, 1.2, 1.3_
  
  - [ ]* 2.2 Escrever teste de propriedade para QR Worker
    - **Propriedade 1: Detecção de Reconhecimento em Tempo Real**
    - **Valida: Requisitos 1.1, 1.2**
  
  - [ ] 2.3 Criar OCR Worker com Tesseract.js
    - Implementar ocr-worker.js com extração de texto
    - Configurar Tesseract.js com modelos otimizados para português
    - Implementar extração de códigos de patrimônio do texto
    - _Requisitos: 2.1, 2.3, 2.4_
  
  - [ ]* 2.4 Escrever teste de propriedade para OCR Worker
    - **Propriedade 4: Foco em Códigos de Patrimônio**
    - **Valida: Requisitos 2.4**

- [ ] 3. Checkpoint - Verificar Workers funcionando
  - Garantir que todos os testes passem, perguntar ao usuário se surgem dúvidas.

- [ ] 4. Implementar serviços de reconhecimento C#
  - [ ] 4.1 Criar QRCodeRecognitionService
    - Implementar IQRCodeService com detecção de QR Code
    - Integrar com QR Worker via JavaScript Interop
    - Implementar tratamento de múltiplos códigos QR
    - _Requisitos: 1.1, 1.4, 1.5_
  
  - [ ]* 4.2 Escrever teste de propriedade para QRCodeService
    - **Propriedade 2: Robustez do Reconhecimento**
    - **Valida: Requisitos 1.5**
  
  - [ ] 4.3 Criar OCRRecognitionService
    - Implementar IOCRService com extração de texto
    - Integrar com OCR Worker via JavaScript Interop
    - Implementar filtros para códigos alfanuméricos
    - _Requisitos: 2.1, 2.4, 2.5_
  
  - [ ]* 4.4 Escrever testes unitários para OCRService
    - Testar extração de códigos de diferentes formatos
    - Testar filtros de ruído e caracteres irrelevantes
    - _Requisitos: 2.4, 2.5_
- [ ] 5. Implementar serviço de busca e validação
  - [ ] 5.1 Criar PatrimonioSearchService
    - Implementar busca otimizada no IndexedDB
    - Implementar cache inteligente para consultas
    - Configurar índices para campos 'code' e 'nutomb'
    - _Requisitos: 3.1, 3.2, 3.3_
  
  - [ ]* 5.2 Escrever teste de propriedade para busca automática
    - **Propriedade 5: Busca Automática no Banco Local**
    - **Valida: Requisitos 3.1, 3.2**
  
  - [ ] 5.3 Criar ValidationService
    - Implementar validação de esfera de acesso
    - Implementar sanitização e normalização de códigos
    - Configurar regras de negócio para patrimônio
    - _Requisitos: 5.1, 5.2, 6.1, 6.2_
  
  - [ ]* 5.4 Escrever teste de propriedade para validação de esfera
    - **Propriedade 11: Validação de Acesso por Esfera**
    - **Valida: Requisitos 5.1, 5.3, 5.4**

- [ ] 6. Implementar serviço principal de reconhecimento
  - [ ] 6.1 Criar RecognitionService
    - Implementar IRecognitionService como coordenador principal
    - Configurar priorização QR Code sobre OCR
    - Implementar throttling de processamento de frames
    - _Requisitos: 8.1, 8.2, 9.1, 9.2_
  
  - [ ]* 6.2 Escrever teste de propriedade para priorização
    - **Propriedade 21: Prioridade QR sobre OCR**
    - **Valida: Requisitos 9.1, 9.2, 9.3**
  
  - [ ] 6.3 Implementar gerenciamento de cache e performance
    - Configurar cache de resultados com TTL de 5 minutos
    - Implementar adaptação automática baseada em memória
    - Configurar processamento máximo de 10 FPS
    - _Requisitos: 8.3, 8.4, 8.5_
  
  - [ ]* 6.4 Escrever teste de propriedade para performance
    - **Propriedade 18: Throttling de Frame Rate**
    - **Valida: Requisitos 8.2**

- [ ] 7. Checkpoint - Verificar serviços integrados
  - Garantir que todos os testes passem, perguntar ao usuário se surgem dúvidas.

- [ ] 8. Implementar JavaScript Interop
  - [ ] 8.1 Criar recognition-interop.js
    - Implementar interface JavaScript para coordenação de Workers
    - Configurar captura de frames do elemento de vídeo
    - Implementar handlers de mensagens dos Workers
    - _Requisitos: 1.1, 2.1, 8.1_
  
  - [ ] 8.2 Integrar com camera-interop.js existente
    - Estender funcionalidades existentes sem quebrar compatibilidade
    - Adicionar suporte para reconhecimento em tempo real
    - Manter todas as APIs existentes funcionando
    - _Requisitos: 10.1, 10.4_
  
  - [ ]* 8.3 Escrever testes de integração JavaScript
    - Testar comunicação entre Workers e serviços C#
    - Testar captura de frames e processamento
    - _Requisitos: 8.1_

- [ ] 9. Implementar parsers de códigos
  - [ ] 9.1 Criar QRParser e QRPrettyPrinter
    - Implementar parser para códigos QR de patrimônio
    - Implementar pretty printer para formatação de QR
    - Configurar suporte para formatos específicos de patrimônio público
    - _Requisitos: 12.1, 12.2, 12.3_
  
  - [ ]* 9.2 Escrever teste de propriedade para QR round-trip
    - **Propriedade 29: Round-trip do Parser QR**
    - **Valida: Requisitos 12.4**
  
  - [ ] 9.3 Criar OCRParser
    - Implementar parser para texto extraído via OCR
    - Configurar regex patterns para códigos de patrimônio
    - Implementar filtros de ruído e validação de formato
    - _Requisitos: 13.1, 13.2, 13.3_
  
  - [ ]* 9.4 Escrever teste de propriedade para OCR parsing
    - **Propriedade 33: Reconhecimento de Padrões OCR**
    - **Valida: Requisitos 13.1, 13.2, 13.3**

- [ ] 10. Integrar com Camera.razor existente
  - [ ] 10.1 Adicionar overlay de reconhecimento
    - Implementar indicadores visuais para QR Code detectado
    - Adicionar overlay para texto OCR destacado
    - Configurar indicadores de status de reconhecimento
    - _Requisitos: 7.1, 7.2, 7.5_
  
  - [ ] 10.2 Implementar controles de usuário
    - Adicionar toggle para habilitar/desabilitar reconhecimento
    - Implementar configurações de sensibilidade OCR
    - Salvar preferências no localStorage
    - _Requisitos: 11.1, 11.2, 11.4_
  
  - [ ]* 10.3 Escrever testes unitários para UI
    - Testar componentes de overlay e controles
    - Testar salvamento de preferências
    - _Requisitos: 11.1, 11.4_

- [ ] 11. Implementar feedback visual e tátil
  - [ ] 11.1 Adicionar feedback visual
    - Implementar moldura verde para QR Code detectado
    - Configurar highlight para regiões de texto OCR
    - Adicionar animações de detecção e confirmação
    - _Requisitos: 7.1, 7.2_
  
  - [ ] 11.2 Implementar feedback tátil e sonoro
    - Configurar vibração de 200ms para código decodificado
    - Adicionar som de confirmação para patrimônio encontrado
    - Implementar indicadores de status em tempo real
    - _Requisitos: 7.3, 7.4, 7.5_
  
  - [ ]* 11.3 Escrever teste de propriedade para feedback
    - **Propriedade 15: Feedback Visual de Reconhecimento**
    - **Valida: Requisitos 7.1, 7.2, 7.5**

- [ ] 12. Implementar preenchimento automático de formulário
  - [ ] 12.1 Criar mapeamento PatrimonioItem para InventoryItem
    - Implementar FormMappingService para conversão de dados
    - Configurar mapeamento de campos específicos
    - Manter associação com foto capturada
    - _Requisitos: 4.1, 4.2, 4.5_
  
  - [ ] 12.2 Adicionar indicadores de preenchimento automático
    - Implementar destaque visual para campos preenchidos
    - Manter editabilidade de todos os campos
    - Adicionar ícones indicativos de auto-preenchimento
    - _Requisitos: 4.3, 4.4_
  
  - [ ]* 12.3 Escrever teste de propriedade para preenchimento
    - **Propriedade 8: Preenchimento Automático de Formulário**
    - **Valida: Requisitos 4.1, 4.2**

- [ ] 13. Checkpoint - Verificar integração completa
  - Garantir que todos os testes passem, perguntar ao usuário se surgem dúvidas.

- [ ] 14. Implementar tratamento de erros e recuperação
  - [ ] 14.1 Criar RecognitionErrorHandler
    - Implementar recuperação automática para erros consecutivos
    - Configurar fallback gracioso para modo manual
    - Implementar logging estruturado de erros
    - _Requisitos: 10.5_
  
  - [ ] 14.2 Adicionar monitoramento de performance
    - Implementar tracking de tempo de processamento
    - Configurar alertas para uso de memória
    - Monitorar taxa de acerto do cache
    - _Requisitos: 8.4, 8.5_
  
  - [ ]* 14.3 Escrever testes unitários para tratamento de erros
    - Testar cenários de falha e recuperação
    - Testar degradação gracioso para modo manual
    - _Requisitos: 10.5_

- [ ] 15. Implementar sanitização e normalização
  - [ ] 15.1 Criar SanitizadorCodigo
    - Implementar remoção de espaços e caracteres especiais
    - Configurar conversão de caracteres similares (O→0, I→1)
    - Implementar normalização para maiúsculas
    - _Requisitos: 6.1, 6.2, 6.3_
  
  - [ ]* 15.2 Escrever teste de propriedade para sanitização
    - **Propriedade 13: Normalização de Códigos**
    - **Valida: Requisitos 6.1, 6.2, 6.3**
  
  - [ ] 15.3 Implementar histórico de códigos detectados
    - Configurar cache de códigos para evitar reprocessamento
    - Implementar seleção da versão mais confiável
    - Manter histórico de detecções por sessão
    - _Requisitos: 6.4, 6.5_

- [ ] 16. Configurar compatibilidade e fallbacks
  - [ ] 16.1 Garantir compatibilidade com fluxo existente
    - Verificar que modo manual continua funcionando
    - Manter todas as APIs existentes do CameraService
    - Implementar alternância entre modo manual e automático
    - _Requisitos: 10.1, 10.2, 10.3_
  
  - [ ]* 16.2 Escrever teste de propriedade para compatibilidade
    - **Propriedade 23: Compatibilidade com Versões Anteriores**
    - **Valida: Requisitos 10.1, 10.2, 10.4**
  
  - [ ] 16.3 Implementar fallback para entrada manual
    - Configurar entrada manual quando reconhecimento falha
    - Manter funcionalidade quando reconhecimento está desabilitado
    - Implementar degradação automática em caso de erros
    - _Requisitos: 9.5, 10.5, 11.3_

- [ ] 17. Checkpoint final - Testes de integração completa
  - Garantir que todos os testes passem, perguntar ao usuário se surgem dúvidas.

- [ ] 18. Otimizações finais e documentação
  - [ ] 18.1 Implementar otimizações de performance
    - Configurar FrameThrottler para controle de FPS
    - Implementar RecognitionMemoryManager para gestão de memória
    - Otimizar consultas IndexedDB com batch operations
    - _Requisitos: 8.2, 8.4, 8.5_
  
  - [ ] 18.2 Adicionar estilos CSS para reconhecimento
    - Implementar animações para detecção QR e OCR
    - Configurar estilos para campos auto-preenchidos
    - Adicionar indicadores visuais responsivos
    - _Requisitos: 7.1, 7.2, 4.3_
  
  - [ ]* 18.3 Escrever testes de performance
    - **Propriedade 17: Processamento em Web Worker**
    - **Valida: Requisitos 8.1**

## Notas

- Tarefas marcadas com `*` são opcionais e podem ser puladas para MVP mais rápido
- Cada tarefa referencia requisitos específicos para rastreabilidade
- Checkpoints garantem validação incremental
- Testes de propriedade validam propriedades universais de correção
- Testes unitários validam exemplos específicos e casos extremos
- A implementação mantém compatibilidade total com funcionalidades existentes
- Processamento em Web Workers garante responsividade da UI
- Sistema de cache otimiza performance de consultas repetidas