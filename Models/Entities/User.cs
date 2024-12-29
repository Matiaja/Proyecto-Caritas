namespace ProyectoCaritas.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public int? StorageCenterId { get; set; }
        public string? Email { get; set; }
        public required string Phone { get; set; }
        public int? CenterId { get; set; } // Un Usuario está en cero o un Centro.

        public Center? Center { get; set; } // Un Usuario está en cero o un Centro.
        public ICollection<Request>? Requests { get; set; } // Un Usuario realiza cero o más Solicitudes.
    }
}
