using System;
using System.Collections.Generic;

namespace pwa_camera_poc_blazor.Models
{
    public class Usuario
    {
        public long Id { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string PrimeiroNome { get; set; } = string.Empty;
        public string UltimoNome { get; set; } = string.Empty;
        public string HashSenha { get; set; } = string.Empty;
        public List<int> IdsUnidadesGestoras { get; set; } = new();
        public int? UnidadeGestoraAtualId { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoLogin { get; set; }
    }
}