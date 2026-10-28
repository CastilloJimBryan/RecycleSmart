using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class AlertasController: Controller
    {
        private readonly AlertaBL _alertabl;
 

        public AlertasController(IConfiguration configuration)
        {
            string conectar = configuration.GetConnectionString("BDTFI")!;
            _alertabl=new AlertaBL(conectar);
       
        }

        int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        int usuarioid=>Servicio.ManejosSesiones.ObtenerUsuarioId(User);

        public IActionResult Index(bool resuelta=false)
        {
            ViewBag.Resuelta=resuelta;
            var lista = resuelta ? _alertabl.ListarAlertasResueltas(empresaid) : _alertabl.ListarAlertasAbiertas(empresaid);

            return View(lista);

        }

        public IActionResult Resolver(int id)
        {
            try
            {
                var alerta = _alertabl.BuscarxId(id, empresaid);
                if(alerta.EstadoAlerta==TipoEstado.EstadoAlerta.Resuelta)
                {
                    TempData["Error"] = "Alerta ya Resuelta";
                    return RedirectToAction("Index");
                }
                return View(alerta);
            }
            catch(Exception ex) 
            {
                TempData["Error"]=ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]

        public IActionResult Resolver(int id, string? nota)
        {
            try
            {
                _alertabl.Resolver(id,usuarioid,nota,empresaid) ;
                TempData["Mensaje"] = "Alerta Resuelta";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",ex.Message);
                try
                {
                    var alerta=_alertabl.BuscarxId(id,empresaid);
                    alerta.NotaResolucion=nota;
                    return View(alerta);
                }
                catch 
                {
                    TempData["Error"] = ex.Message;
                    return RedirectToAction("Index");
                }

            }
        }
        [HttpPost]

        public IActionResult Tomar(int id)
        {
            try
            {
                _alertabl.Tomar(id, usuarioid, empresaid);
                TempData["Mensaje"] = "Alerta Tomada!";
            }
            catch(Exception ex)
            {
                TempData["Error"]=ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
