namespace ProyectoCaritas.Models.DTOs
{
    public class ProductStockSummaryDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CenterId { get; set; }
        public string CenterName { get; set; }
        public int TotalStock { get; set; }
        public int TotalIngresos { get; set; }
        public int TotalEgresos { get; set; }
        public DateOnly LastMovementDate { get; set; }
        public int MovementCount { get; set; }
    }
}
