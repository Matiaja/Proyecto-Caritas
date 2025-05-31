using System.ComponentModel.DataAnnotations;

namespace ProyectoCaritas.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Message { get; set; }
        public NotificationType Type { get; set; } // "Assignment", "Shipment", "Reception", "Other"
        public int OrderLineId { get; set; } // Order line que esta relacionada
        public int DonationRequestId { get; set; } // Donacion que esta relacionada
        public int RecipientCenterId { get; set; } // Para notificaciones de asignación y envío a centros
        public int UserId { get; set; } // Usuario que generó la notificación
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsRead { get; set; }
        public required string Status { get; set; } // "Active", "Completed", "Expired"
    }

    public enum NotificationType
    {
        Assignment,
        Acceptance,
        Rejection,
        Shipping,
        Receipt,
        Reminder,
        System
    }
}