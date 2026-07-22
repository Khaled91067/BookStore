using BookStore.Extensions;
using BookStore.Middleware;
using Serilog;

namespace BookStore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Bootstrap logger captures startup errors before the host is built
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("BookStore application starting up");

                var builder = WebApplication.CreateBuilder(args);

                // Replace default logging provider with Serilog
                builder.Host.UseSerilog((ctx, services, cfg) =>
                    cfg.ReadFrom.Configuration(ctx.Configuration)
                       .ReadFrom.Services(services)
                       .Enrich.FromLogContext());

                // Register services
                builder.Services.AddDatabaseAndIdentityServices(builder.Configuration);
                builder.Services.AddMemoryCache();
                builder.Services.AddRateLimitingPolicies();
                builder.Services.AddApplicationServices();
                builder.Services.AddControllersWithViews();
                builder.Services.AddSession(option => option.IdleTimeout = TimeSpan.FromMinutes(30));

                var app = builder.Build();

                // Middleware Pipeline
                app.UseMiddleware<ExceptionHandlingMiddleware>();
                app.UseSerilogRequestLogging(opts =>
                {
                    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });

                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseRateLimiter();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseSession();

                // Seed database data
                await app.SeedDatabaseAsync();

                app.MapStaticAssets();

                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                    .WithStaticAssets();

                app.MapRazorPages()
                   .WithStaticAssets();

                app.Run();
            }
            catch (Exception ex) when (ex is not HostAbortedException)
            {
                Log.Fatal(ex, "BookStore application terminated unexpectedly");
            }
            finally
            {
                Log.Information("BookStore application shutting down");
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
