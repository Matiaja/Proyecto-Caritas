using System.ComponentModel.DataAnnotations;

namespace ProyectoCaritas.Models.DTOs
{
    public class UserRegisterDTO
    {
        public required string UserName { get; set; }

        [Required(ErrorMessage = "The email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Role { get; set; }
        public  required string PhoneNumber { get; set; }
        public int? CenterId { get; set; }
    }
}
