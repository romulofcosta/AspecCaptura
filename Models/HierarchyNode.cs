namespace AspecCaptura.Models;

/// <summary>
/// Nível hierárquico organizacional
/// </summary>
public enum HierarchyLevel
{
    /// <summary>
    /// Órgão (nível 1)
    /// </summary>
    Orgao,
    
    /// <summary>
    /// Unidade Orçamentária (nível 2)
    /// </summary>
    Unidade,
    
    /// <summary>
    /// Área (nível 3)
    /// </summary>
    Area,
    
    /// <summary>
    /// Subárea (nível 4)
    /// </summary>
    Subarea
}

/// <summary>
/// Nó da hierarquia organizacional
/// </summary>
public class HierarchyNode
{
    /// <summary>
    /// ID único do nó
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome do nó
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// ID do nó pai (null para raiz)
    /// </summary>
    public string? ParentId { get; set; }
    
    /// <summary>
    /// Nível hierárquico
    /// </summary>
    public HierarchyLevel Level { get; set; }
}
