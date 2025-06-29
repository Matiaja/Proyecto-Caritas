namespace ProyectoCaritas.Models.DTOs
{
    public class StockDTO
    {
        public int CenterId { get; set; }
        public int? ProductId { get; set; }
        public DateOnly Date { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public double? Weight { get; set; }
        //public required string Status { get; set; } 
    }
}
