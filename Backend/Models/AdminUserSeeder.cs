using Microsoft.AspNetCore.Identity;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Data
{
    public static class AdminUserSeeder
    {
        public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminRole = "Admin";
            string adminUsername = "admin";
            string adminPassword = "superadmin10";

            // Step 1: Ensure the "Admin" role exists
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Step 2: Check if the admin user already exists
            if (await userManager.FindByNameAsync(adminUsername) == null)
            {
                // Step 3: Create the new admin user
                User adminUser = new User
                {
                    UserName = adminUsername,
                    Email = "admin@caritas.com", // You can set a default email
                    FirstName = "Super",
                    LastName = "Admin",
                    Role = adminRole,
                    EmailConfirmed = true // Automatically confirm the email
                };

                IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

                // Step 4: Assign the "Admin" role to the new user
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
        }
    }
}