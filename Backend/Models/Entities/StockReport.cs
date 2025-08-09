using System.ComponentModel.DataAnnotations;

namespace ProyectoCaritas.Models.Entities
{
    public class StockReport
    {
        [Key]
        public int StockId { get; set; }
        public DateTime StockDate { get; set; }
        public int StockQuantity { get; set; }
        public string StockType { get; set; }
        public int CenterId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int StockAcumulado { get; set; }
    }
}
