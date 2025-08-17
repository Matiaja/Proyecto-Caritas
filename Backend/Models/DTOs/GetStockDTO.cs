namespace ProyectoCaritas.Models.DTOs
{
    public class GetStockDTO
    {
        public int Id { get; set; }
        public int CenterId { get; set; }
        public int? ProductId { get; set; }
        public string Date { get; set; }
        public string? Type { get; set; }
        public string? ExpirationDate { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public double? Weight { get; set; }
        public string? Origin { get; set; }

        public GetProductDTO? Product { get; set; }
        //public required string Status { get; set; } 
    }
}
