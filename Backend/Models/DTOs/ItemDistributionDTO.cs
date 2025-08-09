using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs;

public class ItemDistributionDTO
{
    public int Id { get; set; }
    public int ItemPurchaseId { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }

    public ItemDistributionDTO(ItemDistribution item)
    {
        Id = item.Id;
        ItemPurchaseId = item.ItemPurchaseId;
        Quantity = item.Quantity;
        Description = item.Description;
        ProductId = item.ItemPurchase?.ProductId ?? 0;
        ProductName = item.ItemPurchase?.Product?.Name ?? "";
    }
}
