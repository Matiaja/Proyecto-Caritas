namespace ProyectoCaritas.Models.Entities
{
    public class DonationRequestStatus
    {
        public int Id { get; set; }
        public int DonationRequestId { get; set; }
        public required string Status { get; set; }
        public DateTime ChangeDate { get; set; }
        public DonationRequest? DonationRequest { get; set; }
    }
}