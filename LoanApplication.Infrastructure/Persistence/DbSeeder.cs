using LoanApplication.Domain.Constants;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Persistence.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoanApplication.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Ensure roles exist
        foreach (var role in Roles.List)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        // Seed an admin user
        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        if (admins.Count == 0)
        {
            var adminEmail = "admin@example.com";
            var admin = new User
            {
                FirstName = "Admin",
                LastName = "Test",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedBy = "Seeder",
                UpdatedBy = "Seeder",
            };

            var result = await userManager.CreateAsync(admin, "Admin@123"); // secure password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }
            else
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed an admin user
        var loanAdmins = await userManager.GetUsersInRoleAsync(Roles.LoanAdmin);
        if (loanAdmins.Count == 0)
        {
            var loanAdminEmail = "loanadmin@example.com";
            var admin = new User
            {
                FirstName = "LoanAdmin",
                LastName = "Test",
                UserName = loanAdminEmail,
                Email = loanAdminEmail,
                EmailConfirmed = true,
                CreatedBy = "Seeder",
                UpdatedBy = "Seeder",
            };

            var result = await userManager.CreateAsync(admin, "LoanAdmin@123"); // secure password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.LoanAdmin);
            }
            else
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Seed a user
        var users = await userManager.GetUsersInRoleAsync(Roles.User);
        if (users.Count == 0)
        {
            var userEmail = "user@example.com";
            var user = new User
            {
                FirstName = "User",
                LastName = "Test",
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                CreatedBy = "Seeder",
                UpdatedBy = "Seeder",
            };

            var result = await userManager.CreateAsync(user, "User@123"); // secure password
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Roles.User);
            }
            else
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}