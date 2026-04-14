# Aspec Captura — Guia de Contexto e Convenções

## Sobre o desenvolvedor

- **Nome**: Rômulo
- **Perfil**: Desenvolvedor do projeto, responsável por todas as decisões técnicas e de produto
- Trabalha em dois ambientes: notebook pessoal e máquina do trabalho
- Usa o Kiro como parceiro de desenvolvimento — espera respostas diretas, sem enrolação, com foco em qualidade e boas práticas

## O que é este projeto

**Aspec Captura** é um PWA (Progressive Web App) de inventário patrimonial público desenvolvido em Blazor WebAssembly. O app é usado por servidores públicos municipais para realizar o inventário físico de bens patrimoniais (móveis, equipamentos, etc.) diretamente no campo, com câmera do celular.

O sistema é um protótipo em evolução, sendo apresentado para times e áreas de governo. Há dois ambientes:
- **Stage** (`8c591cc1.pwa-camera-poc-blazor.pages.dev`) — ambiente de testes do desenvolvedor
- **Prod** — ambiente de apresentação para o time e demais áreas

### Stack
- **Frontend**: Blazor WebAssembly (.NET 8), MudBlazor, Tailwind CSS, PWA com Service Worker
- **Backend**: ASP.NET Core Minimal API (.NET 8), hospedado no Render (Docker, plano free)
- **Storage**: AWS S3 — arquivos JSON por município (`usuarios/{PREFIXO}.json`)
- **Auth**: JWT gerado pela API após validar credenciais no S3
- **Deploy frontend**: Cloudflare Pages (build via `build.sh`)
- **Deploy backend**: Render (Docker, `autoDeploy: true` na branch `desenvolvimento_v3`)

### Arquitetura de dados
- Cada município tem um arquivo JSON no S3: `usuarios/{PREFIXO}.json` (ex: `CE999.json`, ~36MB)
- O arquivo contém: lista de usuários, tombamentos, tabelas de órgãos/unidades/áreas/subáreas
- O login carrega os dados do município e gera o token JWT
- O frontend sincroniza os tombamentos em lotes via `/api/tombamentos/lote/{id}`

### Fluxo principal do usuário
1. Login com `municipio.nome.sobrenome` + senha
2. Configuração de sessão: seleciona Órgão → UO → Área → Subárea (onde está fisicamente)
3. Scan de bens patrimoniais com câmera (OCR, Barcode, QR Code)
4. Captura de foto + confirmação do estado de conservação
5. Sincronização com o backend

---

## Regras de Negócio

### Login — Formato de usuário
- Campo aceita `municipio.nome.sobrenome` (ex: `ce999.joao.silva`) — **não é e-mail**
- O placeholder deve refletir o formato exato
- A API extrai o prefixo (`CE999`) do username para buscar o arquivo no S3
- O token JWT contém o prefixo — endpoints protegidos validam que o prefixo do token bate com o da requisição (retorna `403` se divergir)

### Scan — Comportamento intencional
- O scan **não filtra** por área/subárea da sessão — correto por regra de negócio
- Um bem pode estar fisicamente em área diferente da registrada (transferências, deslocamentos)
- O usuário configura **onde está fisicamente** e o sistema deve **mostrar divergência** quando a localização registrada ≠ sessão atual
- O usuário precisa identificar visualmente que o bem pertence a outra área/órgão

### Roadmap — Flag de Deslocamento (pendente)
- Quando houver divergência, o usuário poderá marcar o bem como "deslocado"
- Definições a serem especificadas em próxima iteração

---

## Infraestrutura e Deploy

### Variáveis de ambiente — Render (stage/prod)
O Render usa formato `AWS__BucketName` (duplo underscore, padrão ASP.NET Core).
O `.env` local usa `AWS_BUCKET_NAME`. O `Program.cs` faz mapeamento bidirecional (débito técnico).

> ⚠️ Nome de bucket S3 é exato — `aspec-captura` ≠ `aspec-capture`. O erro retornado é genérico (`Município não encontrado`) porque o S3 retorna 404 tanto para bucket errado quanto para arquivo inexistente. Sempre validar o nome exato no console AWS.

### Render — plano free
- O serviço hiberna após inatividade — primeira requisição pode demorar até 50s para "acordar"
- `autoDeploy: true` na branch `desenvolvimento_v3`

### Cloudflare Pages
- Build via `build.sh` — substitui `__API_BASE_URL__` no `appsettings.json`
- Variável `API_BASE_URL` deve apontar para a URL do Render correspondente ao ambiente

### Dockerfile
- Referencia `AspecCapturaApi.csproj` e `AspecCapturaApi.dll` (nome atual)
- Nome antigo `pwa-camera-poc-api` foi descontinuado — nunca usar
- Se renomear o projeto, atualizar: `COPY`, `dotnet restore`, `dotnet build`, `dotnet publish`, `ENTRYPOINT`

---

## Versionamento

Ambos os projetos (AspecCaptura e AspecCapturaApi) são versionados **juntos** com a mesma versão.
Padrão semântico: `MAJOR.MINOR.PATCH` (ex: `0.11.2`)

### Arquivos a atualizar antes de cada commit

**AspecCaptura:**
- `AspecCaptura.csproj` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>`, `<InformationalVersion>`
- `wwwroot/manifest.json` — campo `"version"`
- `wwwroot/service-worker.js` — constante `APP_VERSION`
- `Pages/Login.razor` — texto de versão visível na tela (obrigatório — feedback visual ao usuário)

**AspecCapturaApi:**
- `AspecCapturaApi.csproj` — `<Version>`, `<AssemblyVersion>`, `<FileVersion>`, `<InformationalVersion>`
- `Program.cs` — SwaggerDoc version

### Padrão de commit
```
v{VERSAO} - {Descrição resumida das mudanças}
```

### Branch ativa
`desenvolvimento_v3` — tags criadas no commit de versão: `git tag v0.11.2`

### Quando commitar
- Apenas alterações importantes a nível de código que precisam ser versionadas
- Steerings podem ser commitadas junto com o próximo versionamento de código, ou quando solicitado explicitamente

---

## Mantra de Qualidade — Build e Testes

**Obrigatório após qualquer alteração de código E antes de qualquer commit:**
1. Rodar build e testes
2. Reportar resultado antes de pedir teste manual ao usuário

### AspecCapturaApi
```powershell
dotnet build AspecCapturaApi.csproj -c Release
dotnet build tests/Tests.csproj -c Release
dotnet test tests/Tests.csproj -c Release --no-build
# Esperado: 14/14 testes passando, 0 erros
```

### AspecCaptura
```powershell
dotnet build AspecCaptura.csproj -c Release
# Requer wasm-tools instalado como admin: dotnet workload install wasm-tools
# Se falhar com NETSDK1147 = falta o workload ou disco cheio (~1.5GB necessários)
# Fallback: usar getDiagnostics no Kiro para validar erros de código sem o workload
```

---

## Princípios de Código

Critérios de revisão obrigatórios. Todo código novo ou refatorado deve seguir:

- **KISS**: solução mais simples que resolve o problema. Complexidade só quando necessária.
- **DRY**: lógica duplicada vira função/serviço. Copiou e colou? Algo está errado.
- **YAGNI**: não implemente o que não é necessário agora. Sem abstrações prematuras.
- **Clean Code**: nomes descritivos, funções pequenas com responsabilidade única, sem código morto.
- **SOLID**: Single Responsibility, Open/Closed, Liskov, Interface Segregation, Dependency Inversion.

---

## Débito Técnico Conhecido

### Variáveis AWS — dois formatos paralelos (viola DRY)
Plano de simplificação:
1. Padronizar tudo em `AWS__*` via `IConfiguration`
2. Remover `GetEnvironmentVariable("AWS_*")` direto do código
3. Remover bloco `MapAspNetToEnv` do `Program.cs`
4. No Render e `.env` local: usar apenas `AWS__BucketName`, `AWS__Region`, `AWS__AccessKey`, `AWS__SecretKey`

### Testes — Autenticação
- Testes usam `CaptureTestFactory.CreateAuthenticatedClient()`
- `JWT_SECRET` injetado via `Environment.SetEnvironmentVariable` na factory
- Endpoints `[Authorize]` retornam `403` quando prefixo do token ≠ prefixo da requisição (comportamento correto de segurança)
