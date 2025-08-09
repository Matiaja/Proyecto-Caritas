namespace ProyectoCaritas.Models.DTOs;

public class CreateItemDistributionDTO
{
    public int ItemPurchaseId { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
}
