namespace ProyectoCaritas.Models.Entities
{
    public class DonationRequest
    {
        public int Id { get; set; }
        public int AssignedCenterId { get; set; }
        public DateTime ShipmentDate { get; set; }
        public DateTime ReceptionDate { get; set; }
        public string Status { get; set; }

        public Center AssignedCenter { get; set; } // Navigation property
    }
}
