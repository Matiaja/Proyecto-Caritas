namespace ProyectoCaritas.Models.Entities
{
    public class Stock
    {
        public int Id { get; set; }
        public int RequestingCenterId { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public string Status { get; set; }

        public Center RequestingCenter { get; set; } // Navigation property
        public ICollection<Product> Products { get; set; }
    }
}
