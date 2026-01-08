using System;
using System.Text.Json.Serialization;

namespace pwa_camera_poc_blazor.Models
{
    /// <summary>
    /// Modelo de metadados compatível com o padrão do módulo Desktop.
    /// Este JSON será salvo no S3 junto com a imagem para integração.
    /// </summary>
    public class ItemMetadata
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [JsonPropertyName("localizacao")]
        public string Localizacao { get; set; } = string.Empty;

        [JsonPropertyName("observacoes")]
        public string Observacoes { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "ativo";

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("unitId")]
        public int? UnitId { get; set; }

        /// <summary>
        /// Campo obrigatório: Username do fiscal que realizou o envio.
        /// Usado para isolamento de dados e integração com o módulo Desktop.
        /// </summary>
        [JsonPropertyName("usuarioEnvio")]
        public string UsuarioEnvio { get; set; } = string.Empty;

        [JsonPropertyName("dataEnvio")]
        public DateTime DataEnvio { get; set; } = DateTime.Now;

        /// <summary>
        /// Converte um InventoryItem para ItemMetadata
        /// </summary>
        public static ItemMetadata FromInventoryItem(InventoryItem item, string username)
        {
            return new ItemMetadata
            {
                Id = item.Id,
                Nome = item.Name,
                Codigo = item.Code,
                Categoria = item.Category,
                Localizacao = item.Location,
                Observacoes = item.Observations,
                Status = item.Status,
                Timestamp = item.Timestamp,
                UnitId = item.UnitId,
                UsuarioEnvio = username,
                DataEnvio = DateTime.Now
            };
        }
    }
}
