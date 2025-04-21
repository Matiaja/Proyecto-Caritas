namespace ProyectoCaritas.Models.DTOs
{
    public class CategoryStockDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TotalStock { get; set; }
    }

}
