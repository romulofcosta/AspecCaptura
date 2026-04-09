# ⚡ GUIA RÁPIDO - IMPLEMENTAÇÃO DE SEGURANÇA

**Tempo Estimado:** 1-2 horas  
**Nível:** Intermediário  
**Pré-requisitos:** .NET 8, conhecimento básico de C#

---

## 🚀 INÍCIO RÁPIDO (5 PASSOS)

### 1️⃣ Instalar Pacote JWT (2 minutos)

```bash
cd AspecCapturaApi
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

---

### 2️⃣ Criar Arquivo .env (2 minutos)

```bash
# Copiar template
cp .env.example .env

# Editar (use seu editor favorito)
nano .env
```

**Conteúdo mínimo do .env:**
```bash
AWS_ACCESS_KEY_ID=your_aws_access_key_here
AWS_SECRET_ACCESS_KEY=your_aws_secret_key_here
AWS_REGION=us-east-2
AWS_BUCKET_NAME=aspec-capture
JWT_SECRET=your-secure-jwt-secret-here-min-32-chars
JWT_ISSUER=aspec-capture-api
JWT_AUDIENCE=aspec-capture-client
```

---

### 3️⃣ Atualizar Program.cs (30 minutos)

#### 3.1. No INÍCIO do arquivo, adicionar:
```csharp
using AspecCapturaApi.Configuration;
using AspecCapturaApi.Services;
using AspecCapturaApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

// Carregar .env
EnvironmentHelper.LoadDotEnv();
```

#### 3.2. APÓS `builder.Services.AddControllers()`, adicionar:
```csharp
// Registrar AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Configurar JWT
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new InvalidOperationException("JWT_SECRET not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidateAudience = true,
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
```

#### 3.3. APÓS `app.UseCors()`, adicionar:
```csharp
app.UseRequestLogging();
app.UseSecurityHeaders();
app.UseAuthentication();
app.UseAuthorization();
```

#### 3.4. NO ENDPOINT `/api/auth/login`, SUBSTITUIR a validação de senha:
```csharp
// ❌ REMOVER:
// if (!string.Equals(user.PwdUsuario?.Trim(), senha, StringComparison.Ordinal))

// ✅ ADICIONAR:
var authService = app.Services.GetRequiredService<IAuthService>();

if (!authService.ValidatePassword(senha, user.PwdUsuario))
{
    log.LogWarning("Authentication failed for municipality {Prefix}", prefix);
    return Results.Unauthorized();
}

// Gerar token
var token = authService.GenerateJwtToken(
    user.IdUsuario?.ToString() ?? Guid.NewGuid().ToString(),
    user.NmUsuario,
    prefix,
    user.Esfera
);

// No return, usar o token gerado:
return Results.Ok(new AuthResponse(
    // ... outros campos
    Token: token  // Usar token gerado, não Guid
));
```

#### 3.5. PROTEGER endpoints críticos:
```csharp
// Exemplo: /api/capture/item
app.MapPost("/api/capture/item", 
    [Authorize] async (  // Adicionar [Authorize]
        [FromBody] CaptureItemRequest request,
        ClaimsPrincipal user,  // Adicionar para obter usuário
        [FromServices] IAmazonS3 s3,
        [FromServices] IConfiguration config,
        [FromServices] IMemoryCache cache,
        ILogger<Program> log) =>
{
    // Validar autorização
    var userPrefix = user.FindFirst("prefix")?.Value;
    if (userPrefix != request.Prefixo)
        return Results.Forbid();

    // ... resto do código
})
.RequireAuthorization();  // Adicionar
```

---

### 4️⃣ Testar (15 minutos)

```bash
# 1. Rodar aplicação
dotnet run

# 2. Testar health check
curl http://localhost:5000/api/health

# 3. Fazer login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"ce999.admin","senha":"senha123"}'

# Copiar o token retornado

# 4. Testar endpoint protegido
curl -X POST http://localhost:5000/api/capture/item \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{"prefixo":"CE999","idPatomb":"12345"}'
```

---

### 5️⃣ Verificar Security Headers (5 minutos)

```bash
curl -I http://localhost:5000/api/health
```

**Deve incluir:**
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`

---

## ✅ CHECKLIST RÁPIDO

- [ ] Pacote JWT instalado
- [ ] Arquivo .env criado
- [ ] Program.cs atualizado
- [ ] Aplicação compila
- [ ] Login retorna token
- [ ] Endpoint protegido funciona com token
- [ ] Endpoint protegido rejeita sem token
- [ ] Security headers presentes

---

## 🐛 TROUBLESHOOTING

### Erro: "JWT_SECRET not configured"
**Solução:** Verificar que .env existe e contém `JWT_SECRET`

### Erro: "401 Unauthorized" ao acessar endpoint
**Solução:** Verificar que está enviando header `Authorization: Bearer TOKEN`

### Erro: "403 Forbidden"
**Solução:** Verificar que o prefixo do usuário corresponde ao prefixo da requisição

### Aplicação não inicia
**Solução:** 
1. Verificar que pacote JWT foi instalado
2. Verificar que .env existe
3. Verificar logs de erro

---

## 📚 DOCUMENTAÇÃO COMPLETA

Para detalhes completos, consulte:
- `IMPLEMENTATION_CHECKLIST.md` - Checklist detalhado
- `SECURITY_IMPLEMENTATION_SUMMARY.md` - Resumo completo
- `IMPLEMENTATION_PLAN.md` - Plano detalhado

---

## 🎯 PRÓXIMOS PASSOS

Após implementação básica:
1. Atualizar frontend para usar JWT
2. Adicionar testes automatizados
3. Documentar para equipe
4. Preparar para produção

---

**Tempo Total:** ~1-2 horas  
**Dificuldade:** ⭐⭐⭐ (Intermediário)  
**Impacto:** ⭐⭐⭐⭐⭐ (Muito Alto)
