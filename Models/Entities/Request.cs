namespace ProyectoCaritas.Models.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public int RequestingCenterId { get; set; }
        public string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }

        public Center RequestingCenter { get; set; } // Navigation property
        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
