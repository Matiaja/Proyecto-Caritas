namespace ProyectoCaritas.Models.DTOs
{
    public class MovementDTO
    {
        public int DonationRequestId { get; set; }
        public required string FromCenter { get; set; }
        public required string ToCenter { get; set; }
        public required string ProductName { get; set; }
        public required int Quantity { get; set; }
        public required string Status { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? Description { get; set; }
    }
}