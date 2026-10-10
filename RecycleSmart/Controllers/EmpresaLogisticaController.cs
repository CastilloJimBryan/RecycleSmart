using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmartBE;
using RecycleSmartBLL;
using RecycleSmartDAL;
using Servicio;
using System.Globalization;

namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class EmpresaLogisticaController :Controller
    {
        private readonly EmpresaLogisticaBLL _empresaLogisticaBLL;
        private readonly PlanDAL _planBLL;

        public EmpresaLogisticaController(IConfiguration configuracion)
        {
            string conectar = configuracion.GetConnectionString("BDTFI")!;
            _empresaLogisticaBLL=new EmpresaLogisticaBLL(conectar);
            _planBLL = new PlanDAL(conectar);
        }

        public IActionResult Index()
        {
            return View(_empresaLogisticaBLL.ListarEmpresa());
        }
        public IActionResult Registrar()
        {
            ViewBag.Planes=_planBLL.ListarPlanes();
            return View(new EmpresaLogistica());
        }



    }
}
