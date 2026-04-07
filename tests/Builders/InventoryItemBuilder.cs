using pwa_camera_poc_blazor.Models;

namespace Tests.Builders;

/// <summary>
/// Builder pattern for creating InventoryItem test data
/// </summary>
public class InventoryItemBuilder
{
    private readonly InventoryItem _item;

    public InventoryItemBuilder()
    {
        _item = new InventoryItem
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Item de Teste",
            Code = "TST001",
            Category = "Equipamentos",
            Location = "Sala 101",
            Observations = "Item para testes automatizados",
            State = ConservationState.Bom,
            ValorLiquidoContabil = 1000.00m,
            Status = null,
            Timestamp = DateTime.Now,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Synced = false,
            Esfera = "E",
            CreatedBy = "test-user",
            UserId = "user-123",
            SessionId = "session-456"
        };
    }

    public static InventoryItemBuilder Create() => new();

    public InventoryItemBuilder WithId(string id)
    {
        _item.Id = id;
        return this;
    }

    public InventoryItemBuilder WithName(string name)
    {
        _item.Name = name;
        return this;
    }

    public InventoryItemBuilder WithCode(string code)
    {
        _item.Code = code;
        return this;
    }

    public InventoryItemBuilder WithCategory(string category)
    {
        _item.Category = category;
        return this;
    }

    public InventoryItemBuilder WithLocation(string location)
    {
        _item.Location = location;
        return this;
    }

    public InventoryItemBuilder WithObservations(string observations)
    {
        _item.Observations = observations;
        return this;
    }

    public InventoryItemBuilder WithState(ConservationState state)
    {
        _item.State = state;
        return this;
    }

    public InventoryItemBuilder WithValorLiquidoContabil(decimal value)
    {
        _item.ValorLiquidoContabil = value;
        return this;
    }

    public InventoryItemBuilder WithStatus(BemStatus? status)
    {
        _item.Status = status;
        return this;
    }

    public InventoryItemBuilder WithEsfera(string esfera)
    {
        _item.Esfera = esfera;
        return this;
    }

    public InventoryItemBuilder WithUser(string userId, string username, string sessionId)
    {
        _item.UserId = userId;
        _item.CreatedBy = username;
        _item.SessionId = sessionId;
        return this;
    }

    public InventoryItemBuilder WithHierarchy(string? idOrgao = null, string? idUO = null, 
        string? idArea = null, string? idSubarea = null)
    {
        _item.IdOrgao = idOrgao;
        _item.IdUO = idUO;
        _item.IdArea = idArea;
        _item.IdSubarea = idSubarea;
        return this;
    }

    public InventoryItemBuilder WithPhotos(params string[] photos)
    {
        _item.Photos = photos.ToList();
        return this;
    }

    public InventoryItemBuilder WithRemoteUrls(params string[] urls)
    {
        _item.RemoteUrls = urls.ToList();
        return this;
    }

    public InventoryItemBuilder AsSynced(bool synced = true)
    {
        _item.Synced = synced;
        _item.IsSynchronized = synced;
        return this;
    }

    public InventoryItemBuilder WithTimestamps(DateTime? createdAt = null, DateTime? updatedAt = null)
    {
        _item.CreatedAt = createdAt ?? DateTime.UtcNow;
        _item.UpdatedAt = updatedAt ?? DateTime.UtcNow;
        _item.Timestamp = _item.UpdatedAt;
        return this;
    }

    public InventoryItemBuilder WithEncryption(string? description = null, string? location = null, 
        string? observations = null)
    {
        _item.EncryptedDescription = description;
        _item.EncryptedLocation = location;
        _item.EncryptedObservations = observations;
        return this;
    }

    public InventoryItem Build() => _item;

    // Predefined common scenarios
    public static InventoryItemBuilder Computer() => Create()
        .WithName("Computador Desktop")
        .WithCode("CPU001")
        .WithCategory("Informática")
        .WithLocation("Sala de TI")
        .WithValorLiquidoContabil(2500.00m);

    public static InventoryItemBuilder Furniture() => Create()
        .WithName("Mesa de Escritório")
        .WithCode("MOB001")
        .WithCategory("Mobiliário")
        .WithLocation("Sala 205")
        .WithValorLiquidoContabil(800.00m);

    public static InventoryItemBuilder Vehicle() => Create()
        .WithName("Veículo Oficial")
        .WithCode("VEI001")
        .WithCategory("Transporte")
        .WithLocation("Garagem")
        .WithValorLiquidoContabil(45000.00m);

    public static InventoryItemBuilder WithQRCode(string qrCode) => Create()
        .WithCode(qrCode)
        .WithName($"Item {qrCode}")
        .WithObservations($"Item identificado via QR Code: {qrCode}");

    public static InventoryItemBuilder WithOCRCode(string ocrCode) => Create()
        .WithCode(ocrCode)
        .WithName($"Item {ocrCode}")
        .WithObservations($"Item identificado via OCR: {ocrCode}");
}