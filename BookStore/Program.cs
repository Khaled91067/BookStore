using BookStore.Data;
using BookStore.Middleware;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading.RateLimiting;

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

                // Replace the default logging provider with Serilog, reading config from appsettings
                builder.Host.UseSerilog((ctx, services, cfg) =>
                    cfg.ReadFrom.Configuration(ctx.Configuration)
                       .ReadFrom.Services(services)
                       .Enrich.FromLogContext());

                // Add services to the container.
                var connectionString = builder.Configuration
                    .GetConnectionString("DefaultConnection") ??
                        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                options.UseSqlServer(connectionString));

                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                                .AddRoles<IdentityRole>()
                                .AddEntityFrameworkStores<ApplicationDbContext>();

                builder.Services.AddMemoryCache();

                builder.Services.AddRateLimiter(options =>
                {
                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                    options.OnRejected = async (context, token) =>
                    {
                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync(
                            "{\"error\": \"Too many requests. Please try again later.\"}", cancellationToken: token);
                    };

                    options.AddPolicy("GlobalPolicy", httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 100,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 5,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                            }));

                    options.AddPolicy("StrictPolicy", httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 10,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0
                            }));
                });

                builder.Services.AddControllersWithViews();

                builder.Services.AddSession(option =>
                {
                    option.IdleTimeout = TimeSpan.FromMinutes(30);
                    //option.Cookie.HttpOnly = true;
                    //option.Cookie.IsEssential = true;
                });

                builder.Services.AddScoped<BookService>();
                builder.Services.AddScoped<OrderService>();
                builder.Services.AddScoped<UserService>();
                builder.Services.AddScoped<AuthorService>();
                builder.Services.AddScoped<CategoryService>();
                builder.Services.AddScoped<DashboardService>();
                builder.Services.AddScoped<HomeService>();
                builder.Services.AddHttpClient<IPaymobService, PaymobService>();

                var app = builder.Build();

                // Global exception handler — logs and redirects; registered before routing
                app.UseMiddleware<ExceptionHandlingMiddleware>();

                // Serilog HTTP request logging — one structured line per request
                app.UseSerilogRequestLogging(opts =>
                {
                    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();
                app.UseRouting();

                app.UseRateLimiter();

                app.UseAuthentication();
                app.UseAuthorization();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                    var dbLogger = loggerFactory.CreateLogger("BookStore.Data.DbInitializer");
                    await DbInitializer.SeedDataAsync(context, userManager, roleManager, dbLogger);
                }

                app.UseSession();//

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
