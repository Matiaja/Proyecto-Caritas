namespace ProyectoCaritas.Models.DTOs
{
    public class DonationNotificationDTO
    {
        public int DonationRequestId { get; set; }
        public required string Status { get; set; }
        public int Quantity { get; set; }
        public required string ProductName { get; set; }
        public required string DonorCenterName { get; set; }
        public required string RequestingCenterName { get; set; }
    }
}