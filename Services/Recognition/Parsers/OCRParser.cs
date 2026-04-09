using System.Text.RegularExpressions;

namespace AspecCaptura.Services.Recognition.Parsers;

public static class OCRParser
{
    private static readonly Regex[] PatrimonioPatterns = new[]
    {
        new Regex(@"\b\d{6,12}\b", RegexOptions.Compiled),                    // Pure numeric codes (6-12 digits)
        new Regex(@"\b[A-Z]{2,4}\d{4,8}\b", RegexOptions.Compiled),          // Letters followed by numbers
        new Regex(@"\b\d{4}-\d{4}\b", RegexOptions.Compiled),                // Hyphenated numeric codes
        new Regex(@"\b[A-Z]{1,3}\d{3,6}[A-Z]?\b", RegexOptions.Compiled),    // Mixed alphanumeric codes
        new Regex(@"\b[A-Z]+\d+[A-Z]*\b", RegexOptions.Compiled),            // General alphanumeric pattern
        new Regex(@"\b\d+[A-Z]+\d*\b", RegexOptions.Compiled)                // Numbers with letters
    };

    private static readonly Regex NoisePattern = new Regex(
        @"[^\w\s\-]|(?:^|\s)(THE|AND|OR|OF|IN|ON|AT|TO|FOR|WITH|BY)(?=\s|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Extract patrimonio codes from OCR text
    /// </summary>
    public static OCRParseResult Parse(string ocrText)
    {
        if (string.IsNullOrWhiteSpace(ocrText))
        {
            return new OCRParseResult
            {
                Success = false,
                ErrorMessage = "OCR text is empty"
            };
        }

        try
        {
            // Clean and normalize the text
            var cleanText = CleanOCRText(ocrText);
            
            // Extract potential codes
            var extractedCodes = ExtractCodes(cleanText);
            
            // Validate and rank codes
            var validCodes = ValidateAndRankCodes(extractedCodes);

            if (!validCodes.Any())
            {
                return new OCRParseResult
                {
                    Success = false,
                    ErrorMessage = "No valid patrimonio codes found in text",
                    RawText = ocrText,
                    CleanedText = cleanText
                };
            }

            return new OCRParseResult
            {
                Success = true,
                ExtractedCodes = validCodes.Select(c => c.Code).ToArray(),
                BestCode = validCodes.First().Code,
                Confidence = validCodes.First().Confidence,
                RawText = ocrText,
                CleanedText = cleanText,
                CodeRegions = validCodes.Select(c => new CodeRegion
                {
                    Code = c.Code,
                    StartIndex = c.StartIndex,
                    Length = c.Length,
                    Confidence = c.Confidence
                }).ToArray()
            };
        }
        catch (Exception ex)
        {
            return new OCRParseResult
            {
                Success = false,
                ErrorMessage = $"Error parsing OCR text: {ex.Message}",
                RawText = ocrText
            };
        }
    }

    private static string CleanOCRText(string text)
    {
        // Convert to uppercase for consistency
        var cleaned = text.ToUpperInvariant();

        // Remove common OCR noise and artifacts
        cleaned = NoisePattern.Replace(cleaned, " ");

        // Fix common OCR character mistakes
        cleaned = cleaned.Replace("O", "0")    // O -> 0
                        .Replace("I", "1")     // I -> 1
                        .Replace("L", "1")     // L -> 1
                        .Replace("S", "5")     // S -> 5 (sometimes)
                        .Replace("Z", "2")     // Z -> 2 (sometimes)
                        .Replace("G", "6")     // G -> 6 (sometimes)
                        .Replace("B", "8");    // B -> 8 (sometimes)

        // Normalize whitespace
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        return cleaned;
    }

    private static List<ExtractedCode> ExtractCodes(string cleanText)
    {
        var codes = new List<ExtractedCode>();

        foreach (var pattern in PatrimonioPatterns)
        {
            var matches = pattern.Matches(cleanText);
            
            foreach (Match match in matches)
            {
                var code = match.Value;
                
                // Skip if too short or too long
                if (code.Length < 4 || code.Length > 20)
                    continue;

                // Calculate confidence based on pattern match and context
                var confidence = CalculateConfidence(code, match, cleanText);

                codes.Add(new ExtractedCode
                {
                    Code = code,
                    StartIndex = match.Index,
                    Length = match.Length,
                    Confidence = confidence,
                    Pattern = pattern.ToString()
                });
            }
        }

        return codes;
    }

    private static List<ExtractedCode> ValidateAndRankCodes(List<ExtractedCode> codes)
    {
        var validCodes = new List<ExtractedCode>();

        foreach (var code in codes)
        {
            // Additional validation
            if (IsValidPatrimonioCode(code.Code))
            {
                validCodes.Add(code);
            }
        }

        // Remove duplicates and rank by confidence
        return validCodes
            .GroupBy(c => c.Code)
            .Select(g => g.OrderByDescending(c => c.Confidence).First())
            .OrderByDescending(c => c.Confidence)
            .ThenBy(c => c.StartIndex) // Prefer codes that appear earlier in text
            .ToList();
    }

    private static float CalculateConfidence(string code, Match match, string fullText)
    {
        float confidence = 0.5f; // Base confidence

        // Length-based confidence (optimal length around 6-10 characters)
        if (code.Length >= 6 && code.Length <= 10)
            confidence += 0.2f;
        else if (code.Length >= 4 && code.Length <= 12)
            confidence += 0.1f;

        // Pattern-based confidence
        if (Regex.IsMatch(code, @"^\d{6,10}$")) // Pure numeric
            confidence += 0.2f;
        else if (Regex.IsMatch(code, @"^[A-Z]{2,3}\d{4,8}$")) // Letters + numbers
            confidence += 0.3f;
        else if (Regex.IsMatch(code, @"^\d{4}-\d{4}$")) // Hyphenated
            confidence += 0.25f;

        // Context-based confidence
        var contextBefore = GetContext(fullText, match.Index, -20);
        var contextAfter = GetContext(fullText, match.Index + match.Length, 20);
        
        if (ContainsPatrimonioKeywords(contextBefore + " " + contextAfter))
            confidence += 0.15f;

        // Position-based confidence (codes at the beginning are often more reliable)
        if (match.Index < fullText.Length * 0.3)
            confidence += 0.05f;

        return Math.Min(confidence, 1.0f);
    }

    private static string GetContext(string text, int startIndex, int length)
    {
        if (length > 0)
        {
            var endIndex = Math.Min(startIndex + length, text.Length);
            return startIndex < text.Length ? text.Substring(startIndex, endIndex - startIndex) : "";
        }
        else
        {
            var actualStart = Math.Max(0, startIndex + length);
            return actualStart < startIndex ? text.Substring(actualStart, startIndex - actualStart) : "";
        }
    }

    private static bool ContainsPatrimonioKeywords(string context)
    {
        var keywords = new[] { "PATRIMONIO", "PLACA", "CODIGO", "ID", "NUM", "TOMBAMENTO", "ATIVO" };
        return keywords.Any(keyword => context.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValidPatrimonioCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length < 4)
            return false;

        // Check for obvious non-codes
        if (Regex.IsMatch(code, @"^(0000|1111|2222|3333|4444|5555|6666|7777|8888|9999)"))
            return false;

        // Check for reasonable character distribution
        var digitCount = code.Count(char.IsDigit);
        var letterCount = code.Count(char.IsLetter);
        
        // Must have at least some digits
        if (digitCount == 0)
            return false;

        // If it's all letters, it's probably not a patrimonio code
        if (letterCount == code.Length)
            return false;

        return true;
    }

    private class ExtractedCode
    {
        public string Code { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public float Confidence { get; set; }
        public string Pattern { get; set; } = string.Empty;
    }
}

public class OCRParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string[] ExtractedCodes { get; set; } = Array.Empty<string>();
    public string? BestCode { get; set; }
    public float Confidence { get; set; }
    public string? RawText { get; set; }
    public string? CleanedText { get; set; }
    public CodeRegion[] CodeRegions { get; set; } = Array.Empty<CodeRegion>();
}

public class CodeRegion
{
    public string Code { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int Length { get; set; }
    public float Confidence { get; set; }
}