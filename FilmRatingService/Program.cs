using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FilmRatingService.Areas.Identity.Data;
using FilmRatingService.Interfaces;
using FilmRatingService.Services;
using FilmRatingService.Data; // <<< ADD THIS LINE for DbInitializer

namespace FilmRatingService
{
    public class Program
    {
        public static async Task Main(string[] args) // <<< MAKE MAIN ASYNC
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
                ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => //
                options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() // <<< ADD THIS to enable RoleManager
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient("TMDB", client =>
            {
                client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            });

            builder.Services.AddScoped<IMovieService, MovieService>();

            var app = builder.Build();

            // Seed the database with Admin role and user <<< ADD THIS SECTION
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                // It's good practice to ensure migrations are applied before seeding
                // var dbContext = services.GetRequiredService<ApplicationDbContext>();
                // await dbContext.Database.MigrateAsync(); // Uncomment if you want to ensure migrations run at startup

                await DbInitializer.InitializeAsync(services);
            }
            // End of seeding section

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            await app.RunAsync(); // <<< MAKE RUN ASYNC (if Main is async, or just app.Run() if Main is not)
        }
    }
}