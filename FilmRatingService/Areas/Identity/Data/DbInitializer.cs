using FilmRatingService.Areas.Identity.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Ensure this using directive is present

using System;
using System.Threading.Tasks;

namespace FilmRatingService.Data // I recommend keeping this namespace for clarity
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            // var logger = services.GetRequiredService<ILogger<DbInitializer>>(); // <<< OLD LINE CAUSING ERROR
            var loggerFactory = services.GetRequiredService<ILoggerFactory>(); // <<< NEW LINE
            var logger = loggerFactory.CreateLogger("DbInitializer");           // <<< NEW LINE - Use a string category name

            try
            {
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = services.GetRequiredService<IConfiguration>();

                string adminRoleName = "Admin";
                string userRoleName = "User";

                if (!await roleManager.RoleExistsAsync(adminRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                    logger.LogInformation($"Role '{adminRoleName}' created.");
                }

                if (!await roleManager.RoleExistsAsync(userRoleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(userRoleName));
                    logger.LogInformation($"Role '{userRoleName}' created.");
                }

                var adminEmail = configuration["DefaultAdminCredentials:Email"];
                var adminPassword = configuration["DefaultAdminCredentials:Password"];
                var adminName = configuration["DefaultAdminCredentials:Name"];

                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword) || string.IsNullOrEmpty(adminName))
                {
                    logger.LogError("DefaultAdminCredentials not found or incomplete in appsettings.json. Admin user will not be created.");
                    return;
                }

                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    ApplicationUser adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        Name = adminName,
                        EmailConfirmed = true
                    };

                    IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, adminRoleName);
                        logger.LogInformation($"Admin user '{adminEmail}' created and assigned to '{adminRoleName}' role.");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            logger.LogError($"Error creating admin user: {error.Description}");
                        }
                    }
                }
                else
                {
                    logger.LogInformation($"Admin user '{adminEmail}' already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }
    }
}