using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProyectoCaritas.Models.Entities;
using System;
using System.Threading.Tasks;

namespace ProyectoCaritas.Models
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAdminUserAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();

            // Check if the admin user already exists
            if (await userManager.FindByNameAsync("admin") == null)
            {
                // Create the admin user
                var adminUser = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com", // Bogus email, can be configured
                    FirstName = "Super",
                    LastName = "Admin",
                    Role = "Admin",
                    EmailConfirmed = true // Confirm email immediately
                };

                // Create the user with the specified password
                var result = await userManager.CreateAsync(adminUser, "superadmin10");

                if (result.Succeeded)
                {
                    // Assign the 'Admin' role to the user
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
