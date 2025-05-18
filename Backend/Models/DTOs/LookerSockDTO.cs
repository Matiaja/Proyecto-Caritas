namespace ProyectoCaritas.Models.DTOs
{
    public class LookerStockDTO
    {
        public int CenterId { get; set; }
        public string ProductName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public int TotalStock { get; set; }
    }

}
