using System;
using System.Collections.Generic;

namespace pwa_camera_poc_blazor.Models
{
    public class Usuario
    {
        public string UsuarioNome { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string Prefixo { get; set; } = string.Empty;
        public string Esfera { get; set; } = string.Empty;
        public List<Orgao> Orgaos { get; set; } = new();
        public List<PatrimonioItem> Patrimonio { get; set; } = new();
        public string Token { get; set; } = string.Empty;
    }



    public class Orgao
    {
        public string IdOrgao { get; set; } = string.Empty;
        public string NomeOrgao { get; set; } = string.Empty;
        public List<UnidadeOrcamentaria> UnidadesOrcamentarias { get; set; } = new();
    }

    public class UnidadeOrcamentaria
    {
        public string IdUO { get; set; } = string.Empty;
        public string NomeUO { get; set; } = string.Empty;
        public List<Area> Areas { get; set; } = new();
    }

    public class Area
    {
        public string IdArea { get; set; } = string.Empty;
        public string NomeArea { get; set; } = string.Empty;
        public List<Subarea> Subareas { get; set; } = new();
    }

    public class Subarea
    {
        public string IdSubarea { get; set; } = string.Empty;
        public string NomeSubarea { get; set; } = string.Empty;
    }

    public class PatrimonioItem
    {
        public long IdPatomb { get; set; }
        public string Nutomb { get; set; } = string.Empty;
        public string Code => Nutomb; // Alias para compatibilidade
        public string Esfera { get; set; } = string.Empty;
        public string Deprod { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Localizacao { get; set; }
        public decimal? ValorEstimado { get; set; }
        public ConservationState? Estado { get; set; }
        
        // Metadados de reconhecimento
        public DateTime? LastRecognized { get; set; }
        public RecognitionSource? RecognitionSource { get; set; }
        public float? RecognitionConfidence { get; set; }
    }
}

