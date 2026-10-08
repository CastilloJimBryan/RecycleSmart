namespace Servicio
{
    public class ManejosSesiones
    {
        public static string Hash(string clave)
        {
            return BCrypt.Net.BCrypt.HashPassword(clave);
        }

        public static bool Verificar(string clave, string Clavehash)
        {
            if (string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(Clavehash)) return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(clave, Clavehash);
            }
            catch
            {
                return false;
            }
        }
    }
}
