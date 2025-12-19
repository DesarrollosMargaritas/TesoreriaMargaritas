using System.Security.Cryptography;
using System.Text;

namespace TesoreriaMargaritas.Helpers
{
    public static class EncryptionHelper
    {
        public static string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                // Convertir la cadena a bytes
                var bytes = Encoding.UTF8.GetBytes(password);

                // Calcular el hash
                var hash = sha256.ComputeHash(bytes);

                // Convertir el array de bytes a string hexadecimal
                var stringBuilder = new StringBuilder();
                foreach (var b in hash)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}