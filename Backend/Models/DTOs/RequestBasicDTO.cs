namespace ProyectoCaritas.Models.DTOs
{
    public class RequestBasicDTO
    {
        public int Id { get; set; }
        public required int RequestingCenterId { get; set; }
        public required string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
        public GetCenterDTO? RequestingCenter { get; set; }

    }
}
