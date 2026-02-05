using pwa_camera_poc_blazor.Models;

namespace pwa_camera_poc_blazor.Services.Scanning
{
    public enum ScanResultType
    {
        Success,                // Item identificado e verificado
        ItemNotFoundInUg,       // Código extraído, mas não está na UG (Erro de Negócio)
        AlreadyRegistered,      // Item já está cadastrado localmente
        ReadError,              // Falha na extração (Confidence baixo ou vazio)
        ValidationFailed        // Falha na regex ou formato inválido
    }

    public class ScanDecision
    {
        public ScanResultType ResultType { get; set; }
        public string Message { get; set; } = string.Empty;
        public ItemPatrimonio? LocalItem { get; set; }      // Se já existir localmente
        public UnitInventoryItem? OfficialItem { get; set; } // Dados da UG para preenchimento
        public string ExtractedCode { get; set; } = string.Empty;
    }
}
