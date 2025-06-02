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
                client.BaseAddress = new Uri("https://api.themoviedb.org/3/"); // Corrected this in a previous step
            });

            builder.Services.AddScoped<IMovieService, MovieService>();

            // <<< ADD THIS LINE to register response caching services >>>
            builder.Services.AddResponseCaching();

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
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePagesWithReExecute("/Home/HttpStatusCodeHandler/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // <<< ADD THIS LINE to enable the response caching middleware >>>
            // It's generally recommended to place it before middleware that serves responses you want to cache,
            // like UseAuthentication, UseAuthorization, and UseEndpoints.
            // Also, place it after UseRouting so it can use routing information if needed for cache variance.
            app.UseResponseCaching();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "AdminArea",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
                );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });

            await app.RunAsync();
        }
    }
}