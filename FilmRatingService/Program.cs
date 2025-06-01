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
                client.BaseAddress = new Uri("https.api.themoviedb.org/3/");
            });

            builder.Services.AddScoped<IMovieService, MovieService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await DbInitializer.InitializeAsync(services);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error"); // For unhandled exceptions
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage(); // More details in development
            }

            // <<< ADD THIS MIDDLEWARE for handling HTTP status codes like 404 >>>
            // It should generally be placed after exception handling but before routing that might depend on it,
            // or before static files if you want it to handle errors for static files too (though less common).
            // A good place is often after UseDeveloperExceptionPage/UseExceptionHandler.
            app.UseStatusCodePagesWithReExecute("/Home/HttpStatusCodeHandler/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

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