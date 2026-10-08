using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Protocol;
using RecycleSmartBE;
using System.Security.Claims;


namespace Servicio
{
    public class ManejosSesiones
    {
        public const string esquema = CookieAuthenticationDefaults.AuthenticationScheme;
        private const string CaimEmpresa = "EmpresaLogisticaId";
        private const string CaimTipoCuenta = "TipoCuenta";

        public static async Task IniciarSesionEmpresa(HttpContext contex, UsuarioEmpresaLogistica usuarioEmpresaLogistica)
        {
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioEmpresaLogistica.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuarioEmpresaLogistica.Nombre} {usuarioEmpresaLogistica.Apellido}"),
                new Claim(ClaimTypes.Role, usuarioEmpresaLogistica.Rol),
                new Claim(CaimEmpresa , usuarioEmpresaLogistica.EmpresaLogisticaId.ToString()),
                new Claim(CaimTipoCuenta, TipoDeCuenta.EmpresaLogistica)
            };
            await Firmar(contex, claim);
        }

        public static async Task IniciarSesionRecycleSmart(HttpContext context,UsuarioRecycleSmart usuarioRecycleSmart)
        {
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioRecycleSmart.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuarioRecycleSmart.Nombre} {usuarioRecycleSmart.Apellido}"),
                new Claim(CaimTipoCuenta, TipoDeCuenta.RecycleSmart)
            };
            await Firmar(context, claim);
        }

        private static async Task Firmar(HttpContext httpContext, List<Claim> claims)
        {
            var identidad = new ClaimsIdentity(claims, esquema);
            await httpContext.SignInAsync(esquema, new ClaimsPrincipal(identidad));
        }
        public static async Task Cerrar(HttpContext context)
        {
            await context.SignOutAsync(esquema);
        }

        public static int ObtenerEmpresaLogisticaId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(CaimEmpresa);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
        public static string ObtenerTipoCuenta(ClaimsPrincipal user)
        {
            return user.FindFirst(CaimTipoCuenta)?.Value ?? "";
        }
        public static int ObtenerUsuarioEmpresaLogistica(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(CaimEmpresa);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
