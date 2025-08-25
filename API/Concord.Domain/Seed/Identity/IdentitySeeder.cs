using Concord.Domain.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Concord.Domain.Seed.Identity
{
    public static class IdentitySeeder
    {
        // Seed Roles:
        public static async Task SeedRoles(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "Provider"};

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Optionally, you can also seed an admin user with the roles
            var adminUser = await userManager.FindByEmailAsync("admin@omayya.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    DisplayName = "admin",
                    UserName = "admin",
                    Email = "admin@omayya.com",
                    Role = "Admin",
                    TenantId = new Guid("11111111-1111-1111-1111-111111111111")
                };

                adminUser.PhoneNumberConfirmed = true;
                adminUser.EmailConfirmed = true;

                var result = await userManager.CreateAsync(adminUser, "P@ssw0rd");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                adminUser.TenantId = new Guid("11111111-1111-1111-1111-111111111111");
                var updateResult = await userManager.UpdateAsync(adminUser);

                if (!updateResult.Succeeded)
                {
                    // Handle update failure if needed
                    throw new InvalidOperationException($"Failed to update admin user tenant: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }

            }
        }
    }
}
