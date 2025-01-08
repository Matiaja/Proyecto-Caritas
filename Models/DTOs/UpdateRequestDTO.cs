namespace ProyectoCaritas.Models.DTOs
{
    public class UpdateRequestDTO
    {
        public required int RequestingCenterId { get; set; }
        public required string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
