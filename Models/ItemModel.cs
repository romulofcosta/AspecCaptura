namespace AspecCaptura.Models;

/// <summary>
/// Item model for the refactored UI (as per spec)
/// This is an alias/wrapper for InventoryItem to match the spec requirements
/// </summary>
public class Item
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal ValorLiquidoContabil { get; set; }
    public string Location { get; set; } = string.Empty;
    public ConservationState State { get; set; }
    public string Observations { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
    public bool IsSynchronized { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    
    // Encrypted fields (stored as base64)
    public string? EncryptedDescription { get; set; }
    public string? EncryptedLocation { get; set; }
    public string? EncryptedObservations { get; set; }

    // Convert from InventoryItem
    public static Item FromInventoryItem(InventoryItem inventoryItem)
    {
        return new Item
        {
            Id = inventoryItem.Id,
            Code = inventoryItem.Code,
            Description = inventoryItem.Name,
            ValorLiquidoContabil = inventoryItem.ValorLiquidoContabil,
            Location = inventoryItem.Location,
            State = inventoryItem.State,
            Observations = inventoryItem.Observations,
            PhotoPath = inventoryItem.PhotoPath,
            IsSynchronized = inventoryItem.IsSynchronized,
            CreatedAt = inventoryItem.CreatedAt,
            UpdatedAt = inventoryItem.UpdatedAt,
            UserId = inventoryItem.UserId,
            SessionId = inventoryItem.SessionId,
            EncryptedDescription = inventoryItem.EncryptedDescription,
            EncryptedLocation = inventoryItem.EncryptedLocation,
            EncryptedObservations = inventoryItem.EncryptedObservations
        };
    }

    // Convert to InventoryItem
    public InventoryItem ToInventoryItem()
    {
        return new InventoryItem
        {
            Id = Id,
            Code = Code,
            Name = Description,
            ValorLiquidoContabil = ValorLiquidoContabil,
            Location = Location,
            State = State,
            Observations = Observations,
            PhotoPath = PhotoPath,
            IsSynchronized = IsSynchronized,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            UserId = UserId,
            SessionId = SessionId,
            EncryptedDescription = EncryptedDescription,
            EncryptedLocation = EncryptedLocation,
            EncryptedObservations = EncryptedObservations
        };
    }
}
