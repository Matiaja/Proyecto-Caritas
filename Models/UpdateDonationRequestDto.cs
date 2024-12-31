namespace ProyectoCaritas.Models
{
    public class UpdateDonationRequestDto
    {
        public DateTime ShipmentDate { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public required string Status { get; set; }

    }
}
