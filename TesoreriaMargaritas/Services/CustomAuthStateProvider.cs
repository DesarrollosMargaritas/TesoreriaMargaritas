using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TesoreriaMargaritas.Models; // Usamos Usuario

namespace TesoreriaMargaritas.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        // Recibimos 'Usuario' en lugar de 'User'
        public void MarkUserAsAuthenticated(Usuario user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.NumeroDocumento), // El nombre de usuario es el Documento
                new Claim(ClaimTypes.NameIdentifier, user.NumeroDocumento), // El ID también es el Documento
                new Claim(ClaimTypes.Role, user.Rol),
                new Claim("NombreCompleto", user.Nombre)
            };

            var identity = new ClaimsIdentity(claims, "apiauth");
            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}