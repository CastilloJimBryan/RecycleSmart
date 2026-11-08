using Microsoft.AspNetCore.Mvc;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    public class TrazabilidadController : Controller
    {
        private readonly LoteBL _lotebl;
        private readonly InstitucionClienteBLL _institucionClienteBLL;

        private int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        public TrazabilidadController(IConfiguration confi)
        {
            string conectar = confi.GetConnectionString("BDTFI")!;
            _lotebl = new LoteBL(conectar);
            _institucionClienteBLL = new InstitucionClienteBLL(conectar);
        }

        public IActionResult Index(int institucionid = 0, bool cerrado = false)
        {
            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid);
            if (institucionid != 0 && !instituciones.Any(i => i.Id == institucionid))
            {
                institucionid = 0;
            }
            ViewBag.Instituciones = instituciones;
            ViewBag.InstitucionId = institucionid;
            ViewBag.Cerrado = cerrado;
            return View(_lotebl.ListarLote(empresaid, institucionid, cerrado));
        }

        public IActionResult Detalle(int id, bool verificar = false)
        {
            try
            {
                var lote = _lotebl.ObtenerLote(id, empresaid);
                ViewBag.Depositos = _lotebl.Deposito(id);
                if (verificar)
                {
                    ViewBag.Verificacion = _lotebl.VerificarIntegridad(lote);
                }
                return View(lote);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public IActionResult SolicitarRecoleccion(int id, string? observacion)
        {
            try
            {
                int usuarioId = ManejosSesiones.ObtenerUsuarioEmpresaLogistica(User);
                _lotebl.SolicitarRecoleccion(id, empresaid, usuarioId, observacion);
                TempData["Mensaje"] = "Recolección solicitada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id });
        } 
    }
}
