using AspecCaptura.Models;
using System.Text.RegularExpressions;

namespace AspecCaptura.Services.Recognition;

public class ValidationService : IValidationService
{
    public async Task<ValidationResult> ValidateAccessAsync(PatrimonioItem item, Usuario user)
    {
        return await Task.Run(() =>
        {
            var result = new ValidationResult
            {
                IsValid = true,
                HasAccess = true
            };

            // Validate item
            if (item == null)
            {
                result.IsValid = false;
                result.HasAccess = false;
                result.ErrorMessage = "Item não encontrado";
                return result;
            }

            // Validate user
            if (user == null)
            {
                result.IsValid = false;
                result.HasAccess = false;
                result.ErrorMessage = "Usuário não autenticado";
                return result;
            }

            // Check sphere access
            if (!HasSphereAccess(item.Esfera, user.Esfera))
            {
                result.HasAccess = false;
                result.RestrictedReason = GetSphereRestrictionMessage(item.Esfera, user.Esfera);
                result.ErrorMessage = result.RestrictedReason;
            }

            return result;
        });
    }

    public string SanitizeCode(string rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            return string.Empty;

        // Remove leading/trailing whitespace
        var sanitized = rawCode.Trim();

        // Convert to uppercase
        sanitized = sanitized.ToUpperInvariant();

        // Replace common OCR mistakes
        sanitized = sanitized.Replace("O", "0")  // O -> 0
                           .Replace("I", "1")    // I -> 1
                           .Replace("L", "1")    // L -> 1
                           .Replace("S", "5")    // S -> 5 (sometimes)
                           .Replace("Z", "2");   // Z -> 2 (sometimes)

        // Remove special characters except hyphens and alphanumeric
        sanitized = Regex.Replace(sanitized, @"[^A-Z0-9\-]", "");

        return sanitized;
    }

    public bool IsValidPatrimonioCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var sanitized = SanitizeCode(code);

        // Check minimum length
        if (sanitized.Length < 4)
            return false;

        // Check against known patrimonio patterns
        var patterns = new[]
        {
            @"^\d{6,12}$",              // Pure numeric codes (6-12 digits)
            @"^[A-Z]{2,4}\d{4,8}$",     // Letters followed by numbers
            @"^\d{4}-\d{4}$",           // Hyphenated numeric codes
            @"^[A-Z]{1,3}\d{3,6}[A-Z]?$" // Mixed alphanumeric codes
        };

        return patterns.Any(pattern => Regex.IsMatch(sanitized, pattern));
    }

    private bool HasSphereAccess(string itemSphere, string userSphere)
    {
        // User with sphere 'A' (All) has access to everything
        if (userSphere == "A")
            return true;

        // User can only access items from their own sphere
        return string.Equals(itemSphere, userSphere, StringComparison.OrdinalIgnoreCase);
    }

    private string GetSphereRestrictionMessage(string itemSphere, string userSphere)
    {
        var sphereNames = new Dictionary<string, string>
        {
            { "A", "Todos" },
            { "E", "Executiva" },
            { "M", "Municipal" },
            { "L", "Legislativa" }
        };

        var itemSphereName = sphereNames.GetValueOrDefault(itemSphere, itemSphere);
        var userSphereName = sphereNames.GetValueOrDefault(userSphere, userSphere);

        return $"Item pertence à Esfera {itemSphereName}. Seu perfil ({userSphereName}) não tem acesso a este item.";
    }
}