using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FilmRatingService.Areas.Identity.Data;
using FilmRatingService.Interfaces;
using FilmRatingService.Services;
using FilmRatingService.Data;

namespace FilmRatingService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
                ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient("TMDB", client =>
            {
                client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
            });

            builder.Services.AddScoped<IMovieService, MovieService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await DbInitializer.InitializeAsync(services);
            }

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

            // Endpoint mapping
            app.UseEndpoints(endpoints => // Modified this part
            {
                // Route for Admin Area
                endpoints.MapControllerRoute(
                    name: "AdminArea",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}" // Default to Dashboard controller in areas
                );

                // Default route for non-area controllers
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"); //

                endpoints.MapRazorPages(); // Needed for Identity UI pages
            });


            // Your old mapping looked like this, we are replacing it with app.UseEndpoints block above
            // app.MapControllerRoute(
            //     name: "default",
            //     pattern: "{controller=Home}/{action=Index}/{id?}");
            // app.MapRazorPages(); 

            await app.RunAsync();
        }
    }
}