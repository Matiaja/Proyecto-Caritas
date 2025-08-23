using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs;

public class PurchaseDTO
{
    public int Id { get; set; }
    public string PurchaseDate { get; set; }
    public string Type { get; set; }
    public int CenterId { get; set; }
    public string CenterName { get; set; }
    public int OriginalCenterId { get; set; }
    public string OriginalCenterName { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public List<ItemPurchaseDTO> Items { get; set; }
    public List<DistributionDTO> Distributions { get; set; } = new List<DistributionDTO>();

    public PurchaseDTO(Purchase purchase)
    {
        Id = purchase.Id;
        PurchaseDate = purchase.PurchaseDate.ToString("dd/MM/yyyy");
        Type = purchase.Type;
        CenterId = purchase.CenterId;
        CenterName = purchase.Center?.Name ?? "";
        OriginalCenterId = purchase.OriginalCenterId;
        OriginalCenterName = purchase.OriginalCenter?.Name ?? "";
        BuyerName = purchase.BuyerName ?? "";
        Items = purchase.Items.Select(i => new ItemPurchaseDTO(i)).ToList();
        Distributions = purchase.Distributions.Select(d => new DistributionDTO(d)).ToList();
    }
}
