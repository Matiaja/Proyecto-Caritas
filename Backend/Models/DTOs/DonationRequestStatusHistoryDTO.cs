namespace ProyectoCaritas.Models.DTOs
{
    public class DonationRequestStatusHistoryDTO
    {
        public int Id { get; set; }
        public int DonationRequestId { get; set; }
        public required string Status { get; set; } // Estado de la solicitud
        public DateTime ChangeDate { get; set; } // Fecha del cambio de estado
    }
}