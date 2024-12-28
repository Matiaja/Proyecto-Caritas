namespace ProyectoCaritas.Models.Entities
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }

        public Request Request { get; set; } // Navigation property
        public Product Product { get; set; } // Navigation property
    }
}
