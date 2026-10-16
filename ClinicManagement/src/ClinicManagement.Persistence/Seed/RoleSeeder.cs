using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Persistence.Seed;

public static class RoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }
}
