using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class AccesoController:Controller
    {
        private readonly EventoAccesoBL _evoAccesoBL;
        private readonly InstitucionClienteBLL _institucionClienteBLL;
        public AccesoController(IConfiguration config)
        {
            string conectar = config.GetConnectionString("BDTFI")!;
            _evoAccesoBL=new EventoAccesoBL(conectar);
            _institucionClienteBLL= new InstitucionClienteBLL(conectar);
        }

        int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);

        public IActionResult Index(int institucionId = 0, bool denegados = false)
        {
            ViewBag.Instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid)
                                                  .Where(i => i.Estados != TipoEstado.Estados.Baja)
                                                  .ToList();
            ViewBag.InstitucionId = institucionId;
            ViewBag.Denegados = denegados;

            return View(_evoAccesoBL.ListarXEmpresa(empresaid, institucionId, denegados));
        }
    }
}
