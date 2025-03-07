﻿using Microsoft.AspNetCore.Identity;

namespace ProyectoCaritas.Models.Entities
{
    public class User : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Role { get; set; }
        public int? CenterId { get; set; } // Un Usuario está en cero o un Centro.

        public Center? Center { get; set; } // Un Usuario está en cero o un Centro.
        public ICollection<Request>? Requests { get; set; } // Un Usuario realiza cero o más Solicitudes.
    }
}
