using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs
{
    public class DonationRequestDTO
    {
        public int? AssignedCenterId { get; set; } 
        public DateTime ShipmentDate { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public required string Status { get; set; }
        public Center? AssignedCenter { get; set; }

    }
}
