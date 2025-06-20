namespace ProyectoCaritas.Models.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public required int RequestingCenterId { get; set; }
        public required string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Status { get; set; } = "Pending"; // Estado por defecto

        public Center? RequestingCenter { get; set; } // Navigation property
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>(); // Una Solicitud tiene cero o más Líneas de pedido.
        public ICollection<User>? Users { get; set; } // Una Solicitud pertenece a cero o más Usuarios.
    }
}
