namespace pwa_camera_poc_blazor.Models
{
    public class UnidadeGestora
    {
        public int Id { get; set; }
        public int CidadeId { get; set; }
        public string Nome { get; set; } = string.Empty;
    }

    public class Cidade
    {
        public int Id { get; set; }
        public string EstadoId { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }

    public class Estado
    {
        public string Id { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
    }
}