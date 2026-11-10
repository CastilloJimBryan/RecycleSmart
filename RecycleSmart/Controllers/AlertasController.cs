using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RecycleSmart.Filters;
using RecycleSmart.HUB;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class AlertasController: Controller
    {
        private readonly AlertaBL _alertabl;

        private readonly IHubContext<MonitoreoHubs> _hub;
        public AlertasController(IConfiguration configuration,IHubContext<MonitoreoHubs>hub)
        {
            string conectar = configuration.GetConnectionString("BDTFI")!;
            _alertabl=new AlertaBL(conectar);
            _hub=hub;
       
        }

        int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        int usuarioid=>Servicio.ManejosSesiones.ObtenerUsuarioId(User);

        public IActionResult Index(bool resuelta=false)
        {
            ViewBag.Resuelta=resuelta;
            var lista = resuelta ? _alertabl.ListarAlertasResueltas(empresaid) : _alertabl.ListarAlertasAbiertas(empresaid);

            return View(lista);

        }

        [Authorize(Roles = Roles.Operacion)]
        public IActionResult Resolver(int id, string? returnUrl)
        {
            try
            {
                var alerta = _alertabl.BuscarxId(id, empresaid);
                if(alerta==null)
                {
                    TempData["Error"] = "La alerta no existe.";
                    return RedirectToAction("Index");
                }
                if(alerta.EstadoAlerta==TipoEstado.EstadoAlerta.Resuelta)
                {
                    TempData["Error"] = "Alerta ya Resuelta";
                    return RedirectToAction("Index");
                }
                ViewBag.ReturnUrl = returnUrl;
                return View(alerta);
            }
            catch(Exception ex) 
            {
                TempData["Error"]=ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = Roles.Operacion)]
        public async Task<IActionResult> Resolver(int id, string? nota, string? returnUrl)
        {
            try
            {
                _alertabl.Resolver(id, usuarioid, nota, empresaid);
                await _hub.Clients.Group($"empresa-{empresaid}").SendAsync("AlertasAbiertas", _alertabl.ContarAlertasAbiertas(empresaid));
                TempData["Mensaje"] = "Alerta Resuelta";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                try
                {
                    var alerta = _alertabl.BuscarxId(id, empresaid);
                    alerta.NotaResolucion = nota;
                    ViewBag.ReturnUrl = returnUrl;
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
        [Authorize(Roles = Roles.Operacion)]
        public IActionResult Tomar(int id,string? returnUrl)
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
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}
