using System.ComponentModel.DataAnnotations;

namespace ProyectoCaritas.Models.DTOs
{
    public class UserDTO
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Role { get; set; }
        public required string PhoneNumber { get; set; }
        public int? CenterId { get; set; }
        public string? CenterName { get; set; }
    }
}
