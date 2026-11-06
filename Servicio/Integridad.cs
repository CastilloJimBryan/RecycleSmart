using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Servicio
{
    public static class Integridad
    {
        private static string _clave = "";
        public const string HashInicial = "0000000000000000000000000000000000000000000000000000000000000000";

        public static void Configurar(string Clave)
        {
            if (string.IsNullOrWhiteSpace(Clave) || Clave.Length < 32)
            {
                throw new Exception("La Integridad tiene que tener 32 caracteres");
            }
            _clave = Clave;
        }

        public static DateTime Truncar(DateTime fecha) => new DateTime(fecha.Ticks - fecha.Ticks % TimeSpan.TicksPerSecond, fecha.Kind);

        public static string TextoHito(string codigoLote, HitoLote hito) => string.Join("|", codigoLote,
            hito.TipoHito.ToString(),
            hito.Fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            hito.Responsable??"",
            hito.Metodo??"",
            hito.NumeroDocumento ?? "",
            hito.Observaciones ?? "",
            hito.DentroDelPlazo?.ToString() ?? "",
            hito.HashAnterior,
            hito.UsuarioId?.ToString(CultureInfo.InvariantCulture)??"",
            hito.UsuarioInstitucionId?.ToString(CultureInfo.InvariantCulture)??""
            );

        public static string CalcularHash(string texto)
        {
            if (_clave == "")
            {
                throw new Exception("La Clave Integrada no esta Configurada");
            }
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_clave)))
            {
                return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(texto)));
            }
            
        }

        public static string hashito(string codigolote,HitoLote h)=>CalcularHash(TextoHito(codigolote, h));

    }
}
