using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Acceso/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.AccessDeniedPath = "/Home/Privacy";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                var user = context.HttpContext.User;
                if (user.HasClaim(c => c.Type == ClaimTypes.Role))
                {
                    var role = user.FindFirst(c => c.Type == ClaimTypes.Role).Value;

                    // Ejemplo de redirección según el rol
                    if (role == "Administrador")
                    {
                        context.RedirectUri = "/Libro/MantenimientoLibro";
                    }
                    else if (role == "Cliente")
                    {
                        context.RedirectUri = "/ECommerce/Index";
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

// Configuración de la cadena de conexión
builder.Services.AddSingleton(builder.Configuration.GetConnectionString("DefaultConnection"));

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Index}/{id?}");

app.Run();
