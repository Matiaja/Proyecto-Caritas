using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs
{
    public class RequestDTO
    {
        public int Id { get; set; }
        public required int RequestingCenterId { get; set; }
        public required string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
        public required ICollection<OrderLineDTO> OrderLines { get; set; }
        public GetCenterDTO? RequestingCenter { get; set; }

    }
}
