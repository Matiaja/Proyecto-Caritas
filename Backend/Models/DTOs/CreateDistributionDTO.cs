namespace ProyectoCaritas.Models.DTOs;

public class CreateDistributionDTO
{
    public int PurchaseId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public int? CenterId { get; set; }
    public string? PersonName { get; set; }
    public string? PersonDNI { get; set; }
    public string? PersonMemberFamily { get; set; }
    public string? PersonLocation { get; set; }
    public required List<CreateItemDistributionDTO> Items { get; set; }
}
