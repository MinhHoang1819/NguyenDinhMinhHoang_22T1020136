using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020136.Shop;
using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Get Connection String early and initialize BusinessLayer configuration
        string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
            ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");
        SV22T1020136.BusinessLayers.Configuration.Initialize(connectionString);

        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllersWithViews()
                        .AddMvcOptions(option =>
                        {
                            option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                        });

        // Configure Authentication
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                        .AddCookie(option =>
                        {
                            option.Cookie.Name = "SV22T1020136.Shop";
                            option.LoginPath = "/Account/Login";
                            option.AccessDeniedPath = "/Account/AccessDenied";
                            option.ExpireTimeSpan = TimeSpan.FromDays(7);
                            option.SlidingExpiration = true;
                            option.Cookie.HttpOnly = true;
                            option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        });

        // Configure Session
        builder.Services.AddSession(option =>
        {
            option.IdleTimeout = TimeSpan.FromHours(2);
            option.Cookie.HttpOnly = true;
            option.Cookie.IsEssential = true;
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSession();

        //Configure Routing
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        //Configure default format
        var cultureInfo = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        //Configure Application Context
        ApplicationContext.Configure
        (
            httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
            webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
            configuration: app.Configuration
        );

        app.Run();
    }
}