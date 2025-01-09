namespace ProyectoCaritas.Models.DTOs
{
    public class OrderLineDTO
    {
        public int Id { get; set; }
        public int? RequestId { get; set; }
        public int? DonationRequestId { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public int? ProductId { get; set; }
    }
}