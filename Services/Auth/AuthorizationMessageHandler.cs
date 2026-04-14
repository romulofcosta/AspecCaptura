using Microsoft.AspNetCore.Components;

namespace AspecCaptura.Services.Auth;

/// <summary>
/// DelegatingHandler que intercepta respostas 401 do backend e redireciona
/// automaticamente para a tela de login, evitando falhas silenciosas quando
/// o token JWT expira durante o uso em campo.
/// </summary>
public class AuthorizationMessageHandler(NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var uri = new Uri(navigation.Uri);
            // Evita loop de redirecionamento se já estiver na tela de login
            if (!uri.AbsolutePath.Equals("/login", StringComparison.OrdinalIgnoreCase))
            {
                navigation.NavigateTo("/login");
            }
        }

        return response;
    }
}
