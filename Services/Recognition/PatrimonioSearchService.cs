using AspecCaptura.Models;
using AspecCaptura.Services.Storage;
using System.Collections.Concurrent;

namespace AspecCaptura.Services.Recognition;

public class PatrimonioSearchService : IPatrimonioSearchService
{
    private readonly IIndexedDbService _dbService;
    private readonly ConcurrentDictionary<string, RecognitionCache> _cache = new();
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public PatrimonioSearchService(IIndexedDbService dbService)
    {
        _dbService = dbService;
    }

    public async Task<PatrimonioItem?> SearchByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var sanitizedCode = SanitizeCode(code);

        // Check cache first
        if (_cache.TryGetValue(sanitizedCode, out var cached) && !cached.IsExpired)
        {
            return cached.Result;
        }

        try
        {
            // Search in patrimonio store by nutomb field
            var result = await _dbService.GetPatrimonioByNutombAsync(sanitizedCode);
            
            // Also try searching by code field if nutomb search fails
            if (result == null)
            {
                var allPatrimonio = await _dbService.GetAllAsync<PatrimonioItem>("patrimonio");
                result = allPatrimonio.FirstOrDefault(p => 
                    p.Code.Equals(sanitizedCode, StringComparison.OrdinalIgnoreCase) ||
                    p.Nutomb.Equals(sanitizedCode, StringComparison.OrdinalIgnoreCase));
            }

            // Cache the result
            _cache[sanitizedCode] = new RecognitionCache
            {
                Code = sanitizedCode,
                Result = result,
                CachedAt = DateTime.Now,
                TTL = _cacheTimeout
            };

            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error searching patrimonio by code {sanitizedCode}: {ex.Message}");
            return null;
        }
    }

    public async Task<PatrimonioItem[]> SearchByCodesAsync(string[] codes)
    {
        if (codes == null || codes.Length == 0)
            return Array.Empty<PatrimonioItem>();

        var results = new List<PatrimonioItem>();
        var uncachedCodes = new List<string>();

        // Check cache for each code
        foreach (var code in codes)
        {
            var sanitizedCode = SanitizeCode(code);
            if (_cache.TryGetValue(sanitizedCode, out var cached) && !cached.IsExpired)
            {
                if (cached.Result != null)
                    results.Add(cached.Result);
            }
            else
            {
                uncachedCodes.Add(sanitizedCode);
            }
        }

        // Search uncached codes
        if (uncachedCodes.Count > 0)
        {
            try
            {
                var allPatrimonio = await _dbService.GetAllAsync<PatrimonioItem>("patrimonio");
                
                foreach (var code in uncachedCodes)
                {
                    var result = allPatrimonio.FirstOrDefault(p => 
                        p.Code.Equals(code, StringComparison.OrdinalIgnoreCase) ||
                        p.Nutomb.Equals(code, StringComparison.OrdinalIgnoreCase));

                    // Cache the result
                    _cache[code] = new RecognitionCache
                    {
                        Code = code,
                        Result = result,
                        CachedAt = DateTime.Now,
                        TTL = _cacheTimeout
                    };

                    if (result != null)
                        results.Add(result);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error searching patrimonio by codes: {ex.Message}");
            }
        }

        return results.ToArray();
    }

    public async Task<bool> IsCachedAsync(string code)
    {
        var sanitizedCode = SanitizeCode(code);
        return await Task.FromResult(_cache.ContainsKey(sanitizedCode) && 
                                   _cache[sanitizedCode] != null && 
                                   !_cache[sanitizedCode].IsExpired);
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    private string SanitizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        return code.Trim()
                  .ToUpperInvariant()
                  .Replace("O", "0")  // Common OCR mistake
                  .Replace("I", "1")  // Common OCR mistake
                  .Replace("L", "1")  // Common OCR mistake
                  .Replace(" ", "");  // Remove spaces
    }
}