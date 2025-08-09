using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs;

public class ItemPurchaseDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public int DistributedQuantity { get; set; }
    public int RemainingQuantity => Quantity - DistributedQuantity;
    public string? Description { get; set; }
    public List<ItemDistributionDTO> ItemsDistribution { get; set; } = new List<ItemDistributionDTO>();

    public ItemPurchaseDTO(ItemPurchase item)
    {
        Id = item.Id;
        ProductId = item.ProductId;
        ProductName = item.Product?.Name ?? "";
        Quantity = item.Quantity;
        DistributedQuantity = item.ItemsDistribution.Count != 0 ? item.ItemsDistribution.Sum(d => d.Quantity) : 0;
        Description = item.Description;
        ItemsDistribution = item.ItemsDistribution.Select(d => new ItemDistributionDTO(d)).ToList();
    }
}
