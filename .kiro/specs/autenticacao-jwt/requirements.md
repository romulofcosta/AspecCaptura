# Requirements Document - Autenticação JWT

## Introduction

Este documento especifica os requisitos para implementação de um sistema de autenticação JWT completo na API BFF do Aspec Captura. O sistema substituirá a autenticação local atual (baseada em localStorage) por uma solução centralizada no servidor, utilizando tokens JWT com refresh tokens, proteção contra ataques de força bruta, e integração completa com o PWA Blazor existente.

## Glossary

- **Sistema_Auth**: Sistema de autenticação JWT implementado na API BFF
- **PWA**: Progressive Web Application Blazor WASM do Aspec Captura
- **Access_Token**: Token JWT de curta duração (1 hora) usado para autenticar requisições
- **Refresh_Token**: Token de longa duração (7 dias) usado para renovar access tokens
- **Usuario**: Entidade representando um usuário do sistema
- **Unidade_Gestora**: Entidade organizacional à qual usuários podem estar associados
- **Rate_Limiter**: Componente que limita tentativas de login por IP
- **Token_Blacklist**: Lista de tokens revogados que não devem mais ser aceitos
- **Claims**: Informações contidas no payload do JWT (userId, username, role, unitIds)
- **Audit_Log**: Registro de tentativas de autenticação (sucesso e falha)

## Requirements

### Requirement 1: Login com Credenciais

**User Story:** Como um usuário do sistema, eu quero fazer login com meu username e senha, para que eu possa acessar funcionalidades protegidas da aplicação.

#### Acceptance Criteria

1. WHEN um usuário envia credenciais válidas para o endpoint de login, THE Sistema_Auth SHALL gerar um Access_Token válido por 1 hora
2. WHEN um usuário envia credenciais válidas para o endpoint de login, THE Sistema_Auth SHALL gerar um Refresh_Token válido por 7 dias
3. WHEN um usuário envia credenciais inválidas, THE Sistema_Auth SHALL retornar erro 401 sem revelar se o username ou senha está incorreto
4. WHEN um login é bem-sucedido, THE Sistema_Auth SHALL atualizar o campo UltimoLogin do Usuario
5. WHEN um login é bem-sucedido, THE Sistema_Auth SHALL registrar a tentativa no Audit_Log com timestamp, IP e resultado
6. WHEN um login falha, THE Sistema_Auth SHALL registrar a tentativa no Audit_Log com timestamp, IP e resultado

#### Example Request/Response

**Request:**
```json
POST /api/auth/login
Content-Type: application/json

{
  "nomeUsuario": "joao.silva",
  "senha": "SenhaSegura123!"
}
```

**Response (Success - 200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "usuario": {
    "id": 123,
    "nomeUsuario": "joao.silva",
    "primeiroNome": "João",
    "ultimoNome": "Silva",
    "idsUnidadesGestoras": [1, 5, 8],
    "unidadeGestoraAtualId": 1
  }
}
```

**Response (Failure - 401 Unauthorized):**
```json
{
  "error": "InvalidCredentials",
  "message": "Nome de usuário ou senha inválidos"
}
```

### Requirement 2: Geração e Estrutura de Tokens JWT

**User Story:** Como desenvolvedor do sistema, eu quero que os tokens JWT contenham informações necessárias do usuário, para que as requisições possam ser autorizadas sem consultas adicionais ao banco de dados.

#### Acceptance Criteria

1. WHEN um Access_Token é gerado, THE Sistema_Auth SHALL incluir claim userId com o ID do Usuario
2. WHEN um Access_Token é gerado, THE Sistema_Auth SHALL incluir claim username com o nome de usuário
3. WHEN um Access_Token é gerado, THE Sistema_Auth SHALL incluir claim role com a role do Usuario
4. WHEN um Access_Token é gerado, THE Sistema_Auth SHALL incluir claim unitIds com a lista de IDs das Unidades_Gestoras associadas
5. WHEN um Access_Token é gerado, THE Sistema_Auth SHALL definir expiração (exp) para 1 hora a partir da emissão
6. WHEN um Refresh_Token é gerado, THE Sistema_Auth SHALL armazená-lo no banco de dados associado ao Usuario
7. THE Sistema_Auth SHALL assinar todos os tokens com chave secreta configurada

#### Example JWT Payload

**Access Token Decoded Payload:**
```json
{
  "userId": 123,
  "username": "joao.silva",
  "role": "Fiscal",
  "unitIds": [1, 5, 8],
  "iat": 1705756800,
  "exp": 1705760400,
  "iss": "aspec-capture-api",
  "aud": "aspec-capture-pwa"
}
```

### Requirement 3: Refresh de Access Token

**User Story:** Como um usuário autenticado, eu quero que meu access token seja renovado automaticamente quando expirar, para que eu não precise fazer login novamente durante minha sessão ativa.

#### Acceptance Criteria

1. WHEN um Refresh_Token válido é enviado ao endpoint de refresh, THE Sistema_Auth SHALL gerar um novo Access_Token
2. WHEN um Refresh_Token expirado é enviado, THE Sistema_Auth SHALL retornar erro 401
3. WHEN um Refresh_Token revogado é enviado, THE Sistema_Auth SHALL retornar erro 401
4. WHEN um Refresh_Token inválido é enviado, THE Sistema_Auth SHALL retornar erro 401
5. WHEN um novo Access_Token é gerado via refresh, THE Sistema_Auth SHALL manter as mesmas claims do token original

#### Example Request/Response

**Request:**
```json
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
}
```

**Response (Success - 200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Response (Failure - 401 Unauthorized):**
```json
{
  "error": "InvalidRefreshToken",
  "message": "Refresh token inválido ou expirado"
}
```

### Requirement 4: Logout e Revogação de Tokens

**User Story:** Como um usuário autenticado, eu quero fazer logout do sistema, para que meus tokens sejam invalidados e não possam mais ser usados.

#### Acceptance Criteria

1. WHEN um usuário faz logout, THE Sistema_Auth SHALL adicionar o Refresh_Token à Token_Blacklist
2. WHEN um usuário faz logout, THE Sistema_Auth SHALL marcar o Refresh_Token como revogado no banco de dados
3. WHEN um token revogado é usado, THE Sistema_Auth SHALL retornar erro 401
4. WHEN um logout é realizado, THE Sistema_Auth SHALL registrar a ação no Audit_Log

#### Example Request/Response

**Request:**
```json
POST /api/auth/logout
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "refreshToken": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
}
```

**Response (Success - 200 OK):**
```json
{
  "message": "Logout realizado com sucesso"
}
```

**Response (Failure - 401 Unauthorized):**
```json
{
  "error": "Unauthorized",
  "message": "Token de acesso inválido"
}
```

### Requirement 5: Proteção de Endpoints com Middleware JWT

**User Story:** Como desenvolvedor do sistema, eu quero que endpoints protegidos validem automaticamente tokens JWT, para que apenas usuários autenticados possam acessá-los.

#### Acceptance Criteria

1. WHEN uma requisição sem token é feita a um endpoint protegido, THE Sistema_Auth SHALL retornar erro 401
2. WHEN uma requisição com token inválido é feita, THE Sistema_Auth SHALL retornar erro 401
3. WHEN uma requisição com token expirado é feita, THE Sistema_Auth SHALL retornar erro 401
4. WHEN uma requisição com token válido é feita, THE Sistema_Auth SHALL extrair as claims e disponibilizá-las no contexto da requisição
5. WHEN uma requisição com token revogado é feita, THE Sistema_Auth SHALL retornar erro 401

### Requirement 6: Proteção contra Brute Force

**User Story:** Como administrador do sistema, eu quero que tentativas excessivas de login sejam bloqueadas, para que o sistema esteja protegido contra ataques de força bruta.

#### Acceptance Criteria

1. WHEN um IP realiza mais de 10 tentativas de login em 1 minuto, THE Rate_Limiter SHALL bloquear novas tentativas desse IP
2. WHEN um IP é bloqueado pelo Rate_Limiter, THE Sistema_Auth SHALL retornar erro 429 (Too Many Requests)
3. WHEN o período de bloqueio expira, THE Rate_Limiter SHALL permitir novas tentativas do IP
4. WHEN um IP é bloqueado, THE Sistema_Auth SHALL registrar o evento no Audit_Log

#### Example Response

**Response (Rate Limited - 429 Too Many Requests):**
```json
{
  "error": "TooManyRequests",
  "message": "Muitas tentativas de login. Tente novamente em 60 segundos",
  "retryAfter": 60
}
```

### Requirement 7: Segurança de Senhas

**User Story:** Como administrador do sistema, eu quero que senhas sejam armazenadas de forma segura, para que não possam ser recuperadas em caso de vazamento de dados.

#### Acceptance Criteria

1. WHEN uma senha é armazenada, THE Sistema_Auth SHALL aplicar hash usando bcrypt com salt rounds >= 10
2. WHEN uma senha é validada, THE Sistema_Auth SHALL usar comparação segura para prevenir timing attacks
3. THE Sistema_Auth SHALL nunca retornar ou logar senhas em texto plano
4. WHEN uma senha é validada, THE Sistema_Auth SHALL usar a mesma quantidade de tempo para senhas corretas e incorretas

### Requirement 8: Integração com PWA Blazor

**User Story:** Como desenvolvedor do PWA, eu quero que o serviço de autenticação se integre com a API BFF, para que o PWA use autenticação centralizada no servidor.

#### Acceptance Criteria

1. WHEN o PWA faz login, THE Sistema_Auth SHALL retornar Access_Token e Refresh_Token em formato JSON
2. WHEN o PWA faz uma requisição autenticada, THE Sistema_Auth SHALL aceitar o Access_Token no header Authorization com formato "Bearer {token}"
3. WHEN o Access_Token expira durante uma requisição do PWA, THE Sistema_Auth SHALL retornar erro 401 com indicação de token expirado
4. THE Sistema_Auth SHALL configurar CORS para aceitar requisições da origem do PWA
5. THE Sistema_Auth SHALL exigir HTTPS para todos os endpoints de autenticação

### Requirement 9: Endpoint de Informações do Usuário

**User Story:** Como usuário autenticado, eu quero obter minhas informações de perfil, para que o PWA possa exibir meus dados e configurações.

#### Acceptance Criteria

1. WHEN um usuário autenticado acessa o endpoint /api/auth/me, THE Sistema_Auth SHALL retornar os dados do Usuario (exceto HashSenha)
2. WHEN um usuário não autenticado acessa o endpoint /api/auth/me, THE Sistema_Auth SHALL retornar erro 401
3. WHEN o endpoint /api/auth/me é acessado, THE Sistema_Auth SHALL usar o userId das claims do token para buscar os dados
4. THE Sistema_Auth SHALL incluir no retorno: Id, NomeUsuario, PrimeiroNome, UltimoNome, IdsUnidadesGestoras, UnidadeGestoraAtualId, DataCriacao, UltimoLogin

#### Example Request/Response

**Request:**
```json
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (Success - 200 OK):**
```json
{
  "id": 123,
  "nomeUsuario": "joao.silva",
  "primeiroNome": "João",
  "ultimoNome": "Silva",
  "idsUnidadesGestoras": [1, 5, 8],
  "unidadeGestoraAtualId": 1,
  "dataCriacao": "2024-01-15T10:30:00Z",
  "ultimoLogin": "2024-01-20T14:22:00Z"
}
```

**Response (Failure - 401 Unauthorized):**
```json
{
  "error": "Unauthorized",
  "message": "Token de acesso inválido ou expirado"
}
```

### Requirement 10: Armazenamento de Refresh Tokens

**User Story:** Como desenvolvedor do sistema, eu quero que refresh tokens sejam armazenados de forma persistente, para que possam ser validados e revogados quando necessário.

#### Acceptance Criteria

1. WHEN um Refresh_Token é gerado, THE Sistema_Auth SHALL armazená-lo em banco de dados com: token hash, userId, data de criação, data de expiração, status (ativo/revogado)
2. WHEN um Refresh_Token é usado, THE Sistema_Auth SHALL verificar sua existência e status no banco de dados
3. WHEN um Refresh_Token é revogado, THE Sistema_Auth SHALL atualizar seu status no banco de dados
4. THE Sistema_Auth SHALL permitir múltiplos Refresh_Tokens ativos por Usuario (para suportar múltiplos dispositivos)
5. WHEN um Refresh_Token expira, THE Sistema_Auth SHALL marcá-lo como expirado no banco de dados

### Requirement 11: Audit Log de Autenticação

**User Story:** Como administrador do sistema, eu quero visualizar logs de tentativas de autenticação, para que eu possa monitorar atividades suspeitas e investigar incidentes de segurança.

#### Acceptance Criteria

1. WHEN uma tentativa de login ocorre, THE Sistema_Auth SHALL registrar: timestamp, username tentado, IP de origem, resultado (sucesso/falha), motivo da falha
2. WHEN um logout ocorre, THE Sistema_Auth SHALL registrar: timestamp, userId, IP de origem
3. WHEN um refresh de token ocorre, THE Sistema_Auth SHALL registrar: timestamp, userId, IP de origem, resultado
4. WHEN um IP é bloqueado por rate limiting, THE Sistema_Auth SHALL registrar: timestamp, IP, número de tentativas
5. THE Sistema_Auth SHALL armazenar logs de auditoria em tabela separada no banco de dados

### Requirement 12: Configuração e Segredos

**User Story:** Como operador do sistema, eu quero que configurações sensíveis sejam gerenciadas de forma segura, para que credenciais não sejam expostas no código-fonte.

#### Acceptance Criteria

1. THE Sistema_Auth SHALL ler a chave secreta JWT de variáveis de ambiente ou configuração externa
2. THE Sistema_Auth SHALL ler configurações de expiração de tokens de arquivo de configuração
3. THE Sistema_Auth SHALL ler configurações de rate limiting de arquivo de configuração
4. THE Sistema_Auth SHALL validar a presença de configurações obrigatórias na inicialização
5. WHEN configurações obrigatórias estão ausentes, THE Sistema_Auth SHALL falhar na inicialização com mensagem clara
