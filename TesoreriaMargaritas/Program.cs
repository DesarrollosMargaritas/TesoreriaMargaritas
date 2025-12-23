using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics; // Namespace necesario
using TesoreriaMargaritas.Components;
using TesoreriaMargaritas.Data;
using TesoreriaMargaritas.Services;

namespace TesoreriaMargaritas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Agregar servicios al contenedor (Inyección de Dependencias)

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // --- CONFIGURACIÓN DE BASE DE DATOS MEJORADA ---
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Esto evita el error "Consider enabling transient error resiliency"
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                // --- CORRECCIÓN CRÍTICA ---
                // Ignoramos la advertencia de cambios pendientes para forzar la ejecución de la migración manual.
                // Esto soluciona el error: "The model for context 'ApplicationDbContext' has pending changes."
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            );

            // --- CONFIGURACIÓN DE AUTENTICACIÓN ---
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            // Servicios de la aplicación
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<TesoreriaService>();

            builder.Services.AddHttpClient();

            var app = builder.Build();

            // --- BLOQUE DE MIGRACIÓN AUTOMÁTICA ---
            // Este bloque intenta aplicar los cambios a la BD al iniciar.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate(); // Ejecuta las migraciones pendientes
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ocurrió un error al crear/migrar la base de datos.");
                }
            }
            // -------------------------------------------------------------

            // 2. Configurar el pipeline de solicitudes HTTP

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}