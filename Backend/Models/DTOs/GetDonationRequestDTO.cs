using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs
{
    public class GetDonationRequestDTO
    {
        public int Id { get; set; }
        public int AssignedCenterId { get; set; }
        public int OrderLineId { get; set; }
        public int Quantity { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public required string Status { get; set; }
        public OrderLineDTO? OrderLine { get; set; }
        public GetCenterDTO? AssignedCenter { get; set; }
    }
}
