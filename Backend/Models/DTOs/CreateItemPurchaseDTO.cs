namespace ProyectoCaritas.Models.DTOs;

public class CreateItemPurchaseDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
}
