using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs;

public class DistributionDTO
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string Status { get; set; }
    public int CenterId { get; set; }
    public string CenterName { get; set; }
    public string PersonName { get; set; }
    public string PersonDNI { get; set; }
    public string PersonMemberFamily { get; set; }
    public List<ItemDistributionDTO> Items { get; set; }

    public DistributionDTO(Distribution dist)
    {
        Id = dist.Id;
        PurchaseId = dist.PurchaseId;
        DeliveryDate = dist.DeliveryDate;
        Status = dist.Status;
        CenterId = dist.CenterId ?? 0;
        CenterName = dist.Center?.Name ?? "";
        PersonName = dist.PersonName ?? "";
        PersonDNI = dist.PersonDNI ?? "";
        PersonMemberFamily = dist.PersonMemberFamily ?? "";
        Items = dist.Items.Select(i => new ItemDistributionDTO(i)).ToList();
    }
}
