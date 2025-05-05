using ProyectoCaritas.Models.Validation;

namespace ProyectoCaritas.Models.DTOs
{
    public class AddRequestDTO
    {
        public required int RequestingCenterId { get; set; }

        [UrgencyLevelValidation]
        public required string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
        public required ICollection<OrderLineDTO> OrderLines { get; set; }
    }
}
