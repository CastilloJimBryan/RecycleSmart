using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using RecycleSmartBLL;

namespace RecycleSmart.Viewcomponents
{
    public class ContadorAlertasViewComponent : ViewComponent
    {
        private readonly AlertaBL _alerta;
        public ContadorAlertasViewComponent(IConfiguration config)
        {
            _alerta = new AlertaBL(config.GetConnectionString("BDTFI")!);
        }

        public IViewComponentResult Invoke()
        {
            int empresaid = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(UserClaimsPrincipal);
            int cantidad = empresaid > 0 ? _alerta.ContarAlertasAbiertas(empresaid) : 0;
            return View(cantidad);
        }
    }
}
