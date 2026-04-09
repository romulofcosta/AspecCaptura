namespace AspecCaptura.Models;

/// <summary>
/// Configuração de sessão hierárquica (Órgão/Unidade/Área/Subárea)
/// </summary>
public class SessionConfig
{
    /// <summary>
    /// ID único da sessão
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// ID do órgão selecionado
    /// </summary>
    public string OrgaoId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome do órgão selecionado
    /// </summary>
    public string OrgaoName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID da unidade selecionada
    /// </summary>
    public string UnidadeId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome da unidade selecionada
    /// </summary>
    public string UnidadeName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID da área selecionada
    /// </summary>
    public string AreaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome da área selecionada
    /// </summary>
    public string AreaName { get; set; } = string.Empty;
    
    /// <summary>
    /// ID da subárea selecionada (opcional)
    /// </summary>
    public string? SubareaId { get; set; }
    
    /// <summary>
    /// Nome da subárea selecionada
    /// </summary>
    public string? SubareaName { get; set; }
    
    /// <summary>
    /// Data de criação da sessão
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// ID do usuário que criou a sessão
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Ano do exercício fiscal selecionado (ex: 1997). Derivado de DtEstr / 10000.
    /// </summary>
    public int AnoExercicio { get; set; } = 0;

    /// <summary>
    /// Data de início do exercício fiscal no formato YYYYMMDD (ex: 19970101). 0 quando ausente.
    /// </summary>
    public int DtEstr { get; set; } = 0;
}
