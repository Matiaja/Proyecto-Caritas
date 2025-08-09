namespace ProyectoCaritas.Models.DTOs;

public class CreatePurchaseDTO
{
    public DateTime PurchaseDate { get; set; }
    public string Type { get; set; } = "General";
    public int CenterId { get; set; }
    public required List<CreateItemPurchaseDTO> Items { get; set; }
}
