namespace ProyectoCaritas.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public byte[]? Image { get; set; }

        public Category Category { get; set; } // Navigation property
        public ICollection<Stock> Stocks { get; set; }
        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
