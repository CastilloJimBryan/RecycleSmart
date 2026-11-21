using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Models;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class SuscripcionController : Controller
    {

        private readonly SuscripcionesBL _suscripcionesBL;
        private readonly FacturaBL _facturabl;

        private int empresaid=> Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        public SuscripcionController(IConfiguration configuration)
        {
            string contectar = configuration.GetConnectionString("BDTFI")!;
            _suscripcionesBL=new SuscripcionesBL(contectar);
            _facturabl=new FacturaBL(contectar);
        }
        private static DateTime ProximoMes()
        {
            var hoy = DateTime.Now;
            return new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1);
        }
        public IActionResult Index()
        {
            var planes = _suscripcionesBL.PlanesDisponibles(empresaid);
            var actual = planes.FirstOrDefault(p => p.esActual)?.Plan;
            if(actual== null)
            {
                TempData["Error"] = "No se Encontro el Plan de tu empresa,Contacta a Soporte";
                return RedirectToAction("Index", "Dashboard");
            }
            var (instituciones, contenedores) = _suscripcionesBL.UsoActual(empresaid);
            var facturas = _facturabl.ListarFacturaXEmpresa(empresaid);

            return View(new SuscripcionViewModel
            {
                PlanActual = actual,
                Instituciones = instituciones,
                Contenedores = contenedores,
                Facturas = facturas,
                UltimaFactura = facturas.FirstOrDefault(),
                ProximaFacturacion = ProximoMes()
            });
        }

        public IActionResult Planes()
        {
            var (instituciones, contenedores) = _suscripcionesBL.UsoActual(empresaid);
            return View(new PlanesViewModel
            {
                Planes = _suscripcionesBL.PlanesDisponibles(empresaid),
                Instituciones = instituciones,
                Contenedores = contenedores
            });
        }

        public IActionResult Resumen(int planId)
        {
            var planes = _suscripcionesBL.PlanesDisponibles(empresaid);
            var actual = planes.FirstOrDefault(p => p.esActual);
            var nuevo = planes.FirstOrDefault(p => p.Plan.Id == planId);

            if (actual == null || nuevo == null || nuevo.esActual || !nuevo.Compatible)
            {
                TempData["Error"] = "Ese plan no se puede elegir.";
                return RedirectToAction("Planes");
            }

            return View(new ResumenCambioViewModel
            {
                PlanActual = actual.Plan,
                PlanNuevo = nuevo.Plan,
                RigeDesde = ProximoMes()
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(int planId)
        {
            try
            {
                _suscripcionesBL.CambiarPlan(empresaid, planId);
                TempData["PlanConfirmado"] = planId;
                return RedirectToAction("Confirmado");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Planes");
            }
        }
        public IActionResult Confirmado()
        {
            // Solo se llega desde Confirmar: si se entra escribiendo la URL, vuelve al plan actual
            if (TempData["PlanConfirmado"] is not int)
                return RedirectToAction("Index");

            var actual = _suscripcionesBL.PlanesDisponibles(empresaid).First(p => p.esActual).Plan;
            ViewBag.RigeDesde = ProximoMes();
            return View(actual);
        }

    }



}
