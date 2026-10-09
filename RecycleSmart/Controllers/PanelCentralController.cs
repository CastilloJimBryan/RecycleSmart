using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using Servicio;

namespace RecycleSmart.Controllers
{
    public class PanelCentralController :Controller
    {
        [Authorize]
        [RequiereTipoCuenta(TipoDeCuenta.RecycleSmart)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
