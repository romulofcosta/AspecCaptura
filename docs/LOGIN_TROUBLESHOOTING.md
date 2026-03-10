# Troubleshooting - Problema de Login

## Problema Identificado
O login não está funcionando corretamente. O usuário vê o erro:
- "Não foi possível conectar ao servidor. Verifique sua conexão com a internet."
- Ou: "Connection error: TypeError: Failed to fetch"

## Causa Raiz
A API backend (`pwa-camera-poc-api`) não está rodando ou não está acessível em `http://localhost:5069`.

## Solução

### 1. Verificar se a API está rodando

**Terminal 1 - Iniciar a API:**
```bash
cd pwa-camera-poc-api
dotnet run
```

A API deve estar rodando em `http://localhost:5069`

### 2. Verificar a configuração da API

**Arquivo:** `pwa-camera-poc-api/appsettings.Development.json`

Certifique-se de que:
- A porta está configurada como `5069`
- CORS está habilitado para `http://localhost:5230` (porta do Blazor)

### 3. Verificar a configuração do Frontend

**Arquivo:** `pwa-camera-poc-blazor/wwwroot/appsettings.json`

Deve conter:
```json
{
    "ApiBaseUrl": "http://localhost:5069"
}
```

Se estiver com `"__API_BASE_URL__"`, o fallback automático em `Program.cs` usará `http://localhost:5069`.

### 4. Fluxo de Login Correto

1. **Usuário insere credenciais** e clica em "Entrar"
2. **Frontend envia POST** para `http://localhost:5069/api/auth/login`
3. **API valida credenciais** e retorna token JWT
4. **Frontend armazena token** em localStorage
5. **Frontend redireciona** para `/configuracao-sessao`

### 5. Verificar Console do Browser

**Chrome DevTools → Console:**

Procure por mensagens como:
- `Connection error: TypeError: Failed to fetch` → API não está rodando
- `POST http://localhost:5069/api/auth/login net::ERR_CONNECTION_REFUSED` → Porta errada ou API não respondendo
- `CORS error` → Problema de CORS na API

### 6. Verificar Network Tab

**Chrome DevTools → Network:**

1. Tente fazer login
2. Procure pela requisição `login` (POST)
3. Verifique:
   - **Status:** Deve ser 200 (sucesso) ou 401 (credenciais inválidas)
   - **Response:** Deve conter `token` e dados do usuário
   - **Headers:** Verifique `Content-Type: application/json`

### 7. Credenciais de Teste

Verifique com o administrador da API quais são as credenciais de teste válidas.

## Checklist de Diagnóstico

- [ ] API está rodando em `http://localhost:5069`?
- [ ] Frontend está rodando em `http://localhost:5230`?
- [ ] CORS está configurado na API?
- [ ] Credenciais estão corretas?
- [ ] Console do browser mostra erros de conexão?
- [ ] Network tab mostra a requisição POST?
- [ ] Resposta da API contém token?

## Próximos Passos

Se o problema persistir:

1. Verifique os logs da API em `pwa-camera-poc-api`
2. Teste a API diretamente com Postman/Insomnia
3. Verifique se há firewall bloqueando a porta 5069
4. Verifique se há proxy configurado no navegador

## Referências

- **Frontend:** `pwa-camera-poc-blazor/Pages/Login.razor`
- **Auth Service:** `pwa-camera-poc-blazor/Services/Auth/AuthService.cs`
- **Program Config:** `pwa-camera-poc-blazor/Program.cs`
- **API:** `pwa-camera-poc-api/Program.cs`
