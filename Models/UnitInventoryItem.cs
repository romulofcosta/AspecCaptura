using System.Text.Json.Serialization;

namespace pwa_camera_poc_blazor.Models
{
    /// <summary>
    /// Representa um item do inventário oficial da Unidade Gestora (UG).
    /// Este modelo é usado para validar se um patrimônio escaneado pertence à UG.
    /// </summary>
    public class UnitInventoryItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
