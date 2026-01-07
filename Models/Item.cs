using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace pwa_camera_poc_blazor.Models
{
    public class InventoryItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Code { get; set; } = string.Empty;

        public string Category { get; set; } = "Geral";

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Location { get; set; } = string.Empty;

        public string Observations { get; set; } = string.Empty;
        public string Status { get; set; } = "ativo";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool Synced { get; set; } = false;
        public int? UnitId { get; set; }

        // Armazenamento de imagens (Base64 local ou URLs remotas)
        public List<string> Photos { get; set; } = new();

        // URLs remotas do S3 após sincronização
        public List<string> RemoteUrls { get; set; } = new();

        // Propriedade auxiliar para capa (prioriza local se não sincronizado, ou remota se disponível)
        public string? CoverImage => RemoteUrls.Any() ? RemoteUrls.FirstOrDefault() : Photos.FirstOrDefault();

        // Extra property for 'EntityId' migration support?
        // public int? EntityId { get; set; }
    }
}
