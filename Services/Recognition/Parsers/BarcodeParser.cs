using System.Text.RegularExpressions;
using AspecCaptura.Models;

namespace AspecCaptura.Services.Recognition.Parsers;

public static class BarcodeParser
{
    private static readonly Regex NumericPatrimonioPattern = new Regex(
        @"^\d{6,12}$", RegexOptions.Compiled);
    
    private static readonly Regex AlphanumericPatrimonioPattern = new Regex(
        @"^[A-Z]{2,4}\d{4,8}$", RegexOptions.Compiled);

    /// <summary>
    /// Parse barcode data to extract and validate patrimonio codes
    /// </summary>
    public static BarcodeParseResult Parse(string barcodeData, string format)
    {
        if (string.IsNullOrWhiteSpace(barcodeData))
        {
            return new BarcodeParseResult
            {
                Success = false,
                ErrorMessage = "Barcode data is empty",
                Format = ParseBarcodeFormat(format)
            };
        }

        try
        {
            var barcodeFormat = ParseBarcodeFormat(format);
            var sanitizedCode = SanitizeCode(barcodeData);
            
            // Validate checksum for EAN/UPC formats
            var checksumValid = ValidateChecksum(sanitizedCode, barcodeFormat);
            
            // Extract patrimonio code
            var patrimonioCode = ExtractPatrimonioCode(sanitizedCode, barcodeFormat);
            
            if (string.IsNullOrEmpty(patrimonioCode))
            {
                return new BarcodeParseResult
                {
                    Success = false,
                    ErrorMessage = "No valid patrimonio code found in barcode data",
                    Format = barcodeFormat,
                    RawData = barcodeData,
                    ChecksumValid = checksumValid
                };
            }

            var confidence = CalculateConfidence(patrimonioCode, barcodeFormat, checksumValid);

            return new BarcodeParseResult
            {
                Success = true,
                PatrimonioCode = patrimonioCode,
                Format = barcodeFormat,
                Confidence = confidence,
                ChecksumValid = checksumValid,
                RawData = barcodeData
            };
        }
        catch (Exception ex)
        {
            return new BarcodeParseResult
            {
                Success = false,
                ErrorMessage = $"Error parsing barcode: {ex.Message}",
                Format = ParseBarcodeFormat(format),
                RawData = barcodeData
            };
        }
    }
    /// <summary>
    /// Sanitize barcode data by removing invalid characters and normalizing
    /// </summary>
    private static string SanitizeCode(string rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode))
            return string.Empty;

        // Remove whitespace and convert to uppercase
        var sanitized = rawCode.Trim().ToUpperInvariant();
        
        // Remove common invalid characters but keep alphanumeric and hyphens
        sanitized = Regex.Replace(sanitized, @"[^\w\-]", "");
        
        return sanitized;
    }

    /// <summary>
    /// Extract patrimonio code from barcode data based on format
    /// </summary>
    private static string? ExtractPatrimonioCode(string sanitizedCode, BarcodeFormat format)
    {
        if (string.IsNullOrWhiteSpace(sanitizedCode))
            return null;

        // Check numeric pattern (6-12 digits)
        if (NumericPatrimonioPattern.IsMatch(sanitizedCode))
        {
            return sanitizedCode;
        }

        // Check alphanumeric pattern ([A-Z]{2,4}[0-9]{4,8})
        if (AlphanumericPatrimonioPattern.IsMatch(sanitizedCode))
        {
            return sanitizedCode;
        }

        // For EAN/UPC codes, try to extract meaningful part
        if (IsEANOrUPC(format))
        {
            return ExtractFromEANUPC(sanitizedCode);
        }

        // For CODE_128/CODE_39, the entire code might be the patrimonio
        if (format == BarcodeFormat.CODE_128 || format == BarcodeFormat.CODE_39)
        {
            // Check if it's a reasonable patrimonio code length
            if (sanitizedCode.Length >= 4 && sanitizedCode.Length <= 20)
            {
                return sanitizedCode;
            }
        }

        return null;
    }

    /// <summary>
    /// Extract patrimonio code from EAN/UPC format
    /// </summary>
    private static string? ExtractFromEANUPC(string code)
    {
        if (string.IsNullOrEmpty(code))
            return null;

        // For EAN-13/UPC-A: Skip country code (first 3 digits) and check digit (last digit)
        if (code.Length == 13 || code.Length == 12)
        {
            var extracted = code.Length == 13 ? code.Substring(3, 9) : code.Substring(1, 10);
            
            // Remove leading zeros
            extracted = extracted.TrimStart('0');
            
            if (extracted.Length >= 6 && extracted.Length <= 10)
            {
                return extracted;
            }
        }

        // For EAN-8: Skip country code and check digit
        if (code.Length == 8)
        {
            var extracted = code.Substring(2, 5);
            extracted = extracted.TrimStart('0');
            
            if (extracted.Length >= 4)
            {
                return extracted;
            }
        }

        return null;
    }
    /// <summary>
    /// Validate checksum for EAN/UPC formats
    /// </summary>
    private static bool ValidateChecksum(string code, BarcodeFormat format)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        try
        {
            switch (format)
            {
                case BarcodeFormat.EAN_13:
                    return ValidateEAN13Checksum(code);
                case BarcodeFormat.EAN_8:
                    return ValidateEAN8Checksum(code);
                case BarcodeFormat.UPC_A:
                    return ValidateUPCAChecksum(code);
                case BarcodeFormat.UPC_E:
                    return ValidateUPCEChecksum(code);
                default:
                    return true; // No checksum validation for CODE_128, CODE_39
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate EAN-13 checksum
    /// </summary>
    private static bool ValidateEAN13Checksum(string code)
    {
        if (code.Length != 13 || !code.All(char.IsDigit))
            return false;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(code[i].ToString());
            sum += digit * (i % 2 == 0 ? 1 : 3);
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == int.Parse(code[12].ToString());
    }

    /// <summary>
    /// Validate EAN-8 checksum
    /// </summary>
    private static bool ValidateEAN8Checksum(string code)
    {
        if (code.Length != 8 || !code.All(char.IsDigit))
            return false;

        int sum = 0;
        for (int i = 0; i < 7; i++)
        {
            int digit = int.Parse(code[i].ToString());
            sum += digit * (i % 2 == 0 ? 3 : 1);
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == int.Parse(code[7].ToString());
    }

    /// <summary>
    /// Validate UPC-A checksum
    /// </summary>
    private static bool ValidateUPCAChecksum(string code)
    {
        if (code.Length != 12 || !code.All(char.IsDigit))
            return false;

        int sum = 0;
        for (int i = 0; i < 11; i++)
        {
            int digit = int.Parse(code[i].ToString());
            sum += digit * (i % 2 == 0 ? 3 : 1);
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == int.Parse(code[11].ToString());
    }
    /// <summary>
    /// Validate UPC-E checksum
    /// </summary>
    private static bool ValidateUPCEChecksum(string code)
    {
        if (code.Length != 8 || !code.All(char.IsDigit))
            return false;

        // Convert UPC-E to UPC-A for validation
        var upca = ExpandUPCE(code);
        if (string.IsNullOrEmpty(upca))
            return false;

        return ValidateUPCAChecksum(upca);
    }

    /// <summary>
    /// Expand UPC-E to UPC-A format for checksum validation
    /// </summary>
    private static string? ExpandUPCE(string upce)
    {
        if (upce.Length != 8 || !upce.All(char.IsDigit))
            return null;

        var firstDigit = upce[0];
        var lastDigit = upce[7];
        var middle = upce.Substring(1, 6);

        var upca = firstDigit.ToString();

        switch (lastDigit)
        {
            case '0':
            case '1':
            case '2':
                upca += middle.Substring(0, 2) + lastDigit + "0000" + middle.Substring(2, 3);
                break;
            case '3':
                upca += middle.Substring(0, 3) + "00000" + middle.Substring(3, 2);
                break;
            case '4':
                upca += middle.Substring(0, 4) + "00000" + middle.Substring(4, 1);
                break;
            default:
                upca += middle + "0000" + lastDigit;
                break;
        }

        // Calculate and append check digit
        int sum = 0;
        for (int i = 0; i < 11; i++)
        {
            sum += int.Parse(upca[i].ToString()) * (i % 2 == 0 ? 3 : 1);
        }
        upca += ((10 - (sum % 10)) % 10).ToString();

        return upca;
    }

    /// <summary>
    /// Calculate confidence based on format and code characteristics
    /// </summary>
    private static float CalculateConfidence(string code, BarcodeFormat format, bool checksumValid)
    {
        float confidence = 0.7f; // Base confidence

        // Adjust based on format
        switch (format)
        {
            case BarcodeFormat.EAN_13:
            case BarcodeFormat.EAN_8:
            case BarcodeFormat.UPC_A:
            case BarcodeFormat.UPC_E:
                confidence = 0.8f; // Higher confidence for EAN/UPC
                break;
            case BarcodeFormat.CODE_128:
            case BarcodeFormat.CODE_39:
                confidence = 0.7f; // Standard confidence
                break;
        }

        // Boost confidence if checksum is valid
        if (checksumValid)
        {
            confidence = Math.Min(confidence + 0.1f, 1.0f);
        }

        // Adjust based on code characteristics
        if (!string.IsNullOrEmpty(code))
        {
            // Prefer codes with good length
            if (code.Length >= 6 && code.Length <= 10)
            {
                confidence = Math.Min(confidence + 0.05f, 1.0f);
            }

            // Boost confidence for alphanumeric codes (more specific)
            if (AlphanumericPatrimonioPattern.IsMatch(code))
            {
                confidence = Math.Min(confidence + 0.1f, 1.0f);
            }
        }

        return confidence;
    }
    /// <summary>
    /// Parse barcode format string to enum
    /// </summary>
    private static BarcodeFormat ParseBarcodeFormat(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return BarcodeFormat.Unknown;

        return format.ToUpperInvariant() switch
        {
            "CODE_128" or "CODE128" => BarcodeFormat.CODE_128,
            "CODE_39" or "CODE39" => BarcodeFormat.CODE_39,
            "EAN_13" or "EAN13" => BarcodeFormat.EAN_13,
            "EAN_8" or "EAN8" => BarcodeFormat.EAN_8,
            "UPC_A" or "UPCA" => BarcodeFormat.UPC_A,
            "UPC_E" or "UPCE" => BarcodeFormat.UPC_E,
            "ITF" => BarcodeFormat.ITF,
            "CODABAR" => BarcodeFormat.CODABAR,
            _ => BarcodeFormat.Unknown
        };
    }

    /// <summary>
    /// Check if format is EAN or UPC
    /// </summary>
    private static bool IsEANOrUPC(BarcodeFormat format)
    {
        return format == BarcodeFormat.EAN_13 ||
               format == BarcodeFormat.EAN_8 ||
               format == BarcodeFormat.UPC_A ||
               format == BarcodeFormat.UPC_E;
    }
}

/// <summary>
/// Result of barcode parsing operation
/// </summary>
public class BarcodeParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PatrimonioCode { get; set; }
    public BarcodeFormat Format { get; set; }
    public float Confidence { get; set; }
    public bool ChecksumValid { get; set; }
    public string? RawData { get; set; }
}