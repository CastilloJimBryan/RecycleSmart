using System;
using System.Collections.Generic;
using System.Text;

namespace Servicio
{
    public static class Validaciones
    {
        public static string NormalizarCUIT(string? cuit)
        {
            var limpio = new string((cuit ?? "").Where(char.IsDigit).ToArray());
            if (limpio.Length != 11)
            {
                throw new Exception("El CUIT debe tener 11 digitos");
            }
            return limpio;
        }
    }
}
