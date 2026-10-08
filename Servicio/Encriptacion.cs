using System;
using System.Collections.Generic;
using System.Text;

namespace Servicio
{
    public static class Encriptacion
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
