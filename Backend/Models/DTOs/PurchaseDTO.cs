using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs;

public class PurchaseDTO
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Type { get; set; }
    public int CenterId { get; set; }
    public string CenterName { get; set; }
    public List<ItemPurchaseDTO> Items { get; set; }
    public List<DistributionDTO> Distributions { get; set; } = new List<DistributionDTO>();

    public PurchaseDTO(Purchase purchase)
    {
        Id = purchase.Id;
        PurchaseDate = purchase.PurchaseDate;
        Type = purchase.Type;
        CenterId = purchase.CenterId;
        CenterName = purchase.Center?.Name ?? "";
        Items = purchase.Items.Select(i => new ItemPurchaseDTO(i)).ToList();
        Distributions = purchase.Distributions.Select(d => new DistributionDTO(d)).ToList();
    }
}
