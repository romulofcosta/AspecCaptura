using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace pwa_camera_poc_blazor.Models
{
    public class ItemPatrimonio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Codigo { get; set; } = string.Empty;

        public string Category { get; set; } = "Geral";

        [Required(ErrorMessage = "Campo obrigatório")]
        public string Localizacao { get; set; } = string.Empty;

        public string Observacoes { get; set; } = string.Empty;
        public string Status { get; set; } = "ativo";
        public DateTime DataHora { get; set; } = DateTime.Now;
        public bool Sincronizado { get; set; } = false;
        public int? UnidadeGestoraId { get; set; }

        // Username do usuário que criou o item localmente
        public string CriadoPor { get; set; } = string.Empty;

        // Armazenamento de imagens (Base64 local ou URLs remotas)
        public List<string> Photos { get; set; } = new();

        // URLs remotas do S3 após sincronização
        public List<string> RemoteUrls { get; set; } = new();

        // Propriedade auxiliar para capa (prioriza local para garantir exibição, ou remota se não houver local)
        public string? CoverImage => Photos.Any() ? Photos.FirstOrDefault() : RemoteUrls.FirstOrDefault();

        // v1.5.0 Hybrid Sync Fields
        /// <summary>Origem do item: 'CargaOficial' (provisioned from S3) ou 'CapturaLocal' (created locally)</summary>
        public string Origem { get; set; } = "CapturaLocal";

        /// <summary>Indica se o item foi sincronizado com S3</summary>
        public bool EstaRemoto { get; set; } = false;

        /// <summary>Data e hora da última sincronização com S3</summary>
        public DateTime? DataUltimaSincronizacao { get; set; }

        /// <summary>Marca o item como sincronizado sem deletá-lo (permite resolução de conflitos)</summary>
        public void MarcarSincronizado()
        {
            Sincronizado = true;
            EstaRemoto = true;
            DataUltimaSincronizacao = DateTime.Now;
        }

        /// <summary>Reseta o status de sincronização (ex: para retentar após erro)</summary>
        public void ResetarSincronizacao()
        {
            Sincronizado = false;
            EstaRemoto = false;
            DataUltimaSincronizacao = null;
        }

        // Extra property for 'EntityId' migration support?
        // public int? EntityId { get; set; }
    }
}