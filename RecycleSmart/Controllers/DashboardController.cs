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
        [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class DashboardController :Controller
    {
        private readonly DashboardBL _dashboardBL;
        private readonly AlertaBL _alertaBL;
        private readonly InstitucionClienteBLL _institucionClienteBLL;
        private readonly ContenedorBL _contenedorBL;

        public DashboardController(IConfiguration configuration)
        {
            string conexion = configuration.GetConnectionString("BDTFI")!;
            _dashboardBL=new DashboardBL(conexion);
            _alertaBL=new AlertaBL(conexion);
            _contenedorBL=new ContenedorBL(conexion);
            _institucionClienteBLL = new InstitucionClienteBLL(conexion);
        }

        public IActionResult Index(int institucionId = 0)
        {
            int empresaid = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid);

            if (institucionId != 0 && !instituciones.Any(i => i.Id == institucionId))
            {
                institucionId = 0;
            }

            var (institucionUsadas, InstitucionMaxima) = _institucionClienteBLL.UsoDelPlan(empresaid);
            var (contenedoresUsados, ContenedoresMaximo) = _contenedorBL.UsoxPlan(empresaid);

            var ds = new DashboardViewModel()
            {
                ResumenDashboard = _dashboardBL.ObtenerResumen(empresaid, institucionId),
                InstitucionId = institucionId,
                InstitucionClientes = instituciones,
                Cola = _dashboardBL.Atencion(empresaid, institucionId),
                AlertasAbiertaXEmpresa = _alertaBL.ContarAlertasAbiertas(empresaid),


                usoPlan = new UsoPlan()
                {
                    Instituciones = institucionUsadas,
                    MaxInstituciones = InstitucionMaxima,
                    Contenedores = contenedoresUsados,
                    MaxContenedores = ContenedoresMaximo,
                }

            };
            return View(ds);
        }

        public IActionResult Resumen(int institucionId=0)
        {
            int empresaid = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
            return Json(_dashboardBL.ObtenerResumen(empresaid, institucionId));
        }
    }
}
