using Microsoft.EntityFrameworkCore;
using TesoreriaMargaritas.Data;
using TesoreriaMargaritas.Models; // Importante: Usamos Usuario
using TesoreriaMargaritas.Helpers;

namespace TesoreriaMargaritas.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // El login ahora recibe 'documento' y devuelve un objeto 'Usuario'
        public async Task<Usuario?> LoginAsync(string documento, string password)
        {
            string passwordHash = EncryptionHelper.EncryptPassword(password);

            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NumeroDocumento == documento && u.PasswordHash == passwordHash && u.Activo);
        }

        // Método auxiliar para crear un usuario inicial si no hay nadie en la BD
        public async Task RegistrarUsuarioInicialAsync()
        {
            if (!await _context.Usuarios.AnyAsync())

            {
                var admin = new Usuario
                {
                    NumeroDocumento = "admin",
                    Nombre = "Administrador Inicial",
                    Rol = "Administrador",
                    PasswordHash = EncryptionHelper.EncryptPassword("admin123"),
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };
                _context.Usuarios.Add(admin);
                await _context.SaveChangesAsync();
            }
        }
    }
}