namespace ProyectoCaritas.Models.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public required int RequestingCenterId { get; set; }
        public required string UrgencyLevel { get; set; }
        public DateTime RequestDate { get; set; }
        public string? Status { get; set; } = "Pendiente"; // Estado por defecto
        public string? ClosedByUserId { get; set; } // ID del usuario que cierra la solicitud
        public DateTime? ClosedDate { get; set; } // Fecha de cierre de la solicitud

        public Center? RequestingCenter { get; set; } // Navigation property
        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>(); // Una Solicitud tiene cero o más Líneas de pedido.
        public ICollection<User>? Users { get; set; } // Una Solicitud pertenece a cero o más Usuarios.
    }
}
