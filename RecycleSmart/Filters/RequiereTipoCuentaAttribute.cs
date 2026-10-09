using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Servicio;

namespace RecycleSmart.Filters
{
    public class RequiereTipoCuentaAttribute : Attribute , IAuthorizationFilter
    {
        private readonly string _tipoDeCuenta;

        public RequiereTipoCuentaAttribute(string tipocuenta)
        {
            _tipoDeCuenta = tipocuenta;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult(); return;
            }
            if(ManejosSesiones.ObtenerTipoCuenta(user)!=_tipoDeCuenta)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
