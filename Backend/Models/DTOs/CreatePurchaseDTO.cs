namespace ProyectoCaritas.Models.DTOs;

public class CreatePurchaseDTO
{
    public string PurchaseDate { get; set; } = string.Empty;
    public string Type { get; set; } = "General";
    public int CenterId { get; set; }
    public int? OriginalCenterId { get; set; }
    public string? BuyerName { get; set; } // Quién compró (persona/responsable)
    public required List<CreateItemPurchaseDTO> Items { get; set; }
}
