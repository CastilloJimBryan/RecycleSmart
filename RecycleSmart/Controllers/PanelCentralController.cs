using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmart.Models;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.RecycleSmart)]
    public class PanelCentralController : Controller
    {
        private readonly PanelCentralBL _panelcentralbl;
        public PanelCentralController(IConfiguration config)
        {
            string conect = config.GetConnectionString("BDTFI")!;
            _panelcentralbl= new PanelCentralBL(conect);
        }
        public IActionResult Index()
        {
            var empresa = _panelcentralbl.ListarEmpresa();
            var vm = new PanelCentralViewModel
            {
                Empresas = empresa,
                EmpresasActivas = empresa.Count(e => e.Estado == TipoEstado.Estados.Activo),
                EmpresasSuspendidas = empresa.Count(e => e.Estado == TipoEstado.Estados.Suspendido),
                Instituciones = empresa.Sum(e => e.Instituciones),
                Contenedores = empresa.Sum(e => e.Contenedores),
                AlertasCriticas = _panelcentralbl.AlertasCriticasActivas(),
                SinAdministrador = empresa.Where(e => e.AdministradorNombre == null).ToList(),
                CercaDelLimite = PanelCentralBL.EmpresasCercaLimite(empresa),
                PorPlan = empresa.GroupBy(e => e.NombrePlan)
                             .Select(g => (g.Key, g.Count()))
                             .OrderByDescending(p => p.Item2)
                             .ToList()
            };
            return View(vm);
        }
    }
}
