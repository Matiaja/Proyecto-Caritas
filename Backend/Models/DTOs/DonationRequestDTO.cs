using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs
{
    public class DonationRequestDTO
    {
        public int AssignedCenterId { get; set; }
        public int OrderLineId { get; set; }
        public int Quantity { get; set; }
        public DateTime AssignmentDate { get; set; } // Fecha de asignación de la solicitud
        public required string Status { get; set; }
        public DateTime? LastStatusChangeDate { get; set; } // Fecha del último cambio de estado
        public Center? AssignedCenter { get; set; }

    }
}
