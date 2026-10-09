using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using Servicio;

namespace RecycleSmart.Controllers
{
        [Authorize]
        [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class DashboardController :Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
