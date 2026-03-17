using WebApplication1.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;

namespace WebApplication1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<FavouritesService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddLogging();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "Alsha.Auth";
                    options.LoginPath = "/api/account/login";
                    options.LogoutPath = "/api/account/logout";
                    options.AccessDeniedPath = "/Error";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.IsEssential = true;
                });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "Alsha.Session";
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddDbContext<WebApplication1Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("WebApplication1Context") ?? throw new InvalidOperationException("Connection string 'WebApplication1Context' not found.")));

            builder.Services.AddRazorPages();

            var app = builder.Build();

            async Task TestDatabaseConnection()
            {
                using (var scope = app.Services.CreateScope())
                {
                    try
                    {
                        var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                        var isConnected = await dbService.TestConnectionAsync();

                        if (!isConnected)
                        {
                            Console.WriteLine("Ошибка подключения к базе данных!");
                        }
                        else
                        {
                            Console.WriteLine("Успешное подключение к базе данных PostgreSQL!");
                            try
                            {
                                var tables = await dbService.QueryAsync<string>(
                                    "SELECT tablename FROM pg_catalog.pg_tables WHERE schemaname = 'public' ORDER BY tablename");

                                Console.WriteLine($"Найдено таблиц: {tables.Count()}");
                                foreach (var table in tables)
                                {
                                    Console.WriteLine($"  - {table}");
                                }

                                var userCount = await dbService.GetCountAsync("users");
                                Console.WriteLine($"Пользователей в базе: {userCount}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Ошибка при получении таблиц: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при создании DatabaseService: {ex.Message}");
                    }
                }
            }

            if (app.Environment.IsDevelopment())
            {
                await TestDatabaseConnection();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapControllers();
            app.MapRazorPages();
            app.Run();
        }
    }
}