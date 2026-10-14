using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class DispositivoIoTController : Controller
    {
        readonly DispositivoIoTBL _dispositivoIoTBLL;
        readonly ContenedorBL _contenedorBL;

        public DispositivoIoTController(IConfiguration configucion)
        {
            string conectar = configucion.GetConnectionString("BDTFI")!;
            _contenedorBL=new ContenedorBL(conectar);
            _dispositivoIoTBLL=new DispositivoIoTBL(conectar);
        }
        private int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);

        public IActionResult RegistrarDispositivo (int contenedorid)
        {
           if (_contenedorBL.BuscarContenedorXId(contenedorid,empresaid) is not Contenedor contenedor)
            {
                return NotFound();
            }
            if(_dispositivoIoTBLL.BuscarXContenedor(contenedorid,empresaid)!=null)
            {
                TempData["Error"] = $"El Contenedor{contenedor.Nombre}, ya tiene Asociado un Dispositivo IoY";
                return RedirectToAction("Index", "Contenedor", new { institucionId = contenedor.InstitucionId });
            }

            ViewBag.Contenedor = contenedor;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult RegistrarDispositivo(int contenedorid,string? mac,string? firmware)
        {
            if(_contenedorBL.BuscarContenedorXId(contenedorid,empresaid)is not Contenedor contenedor)
            {
                return NotFound();
            }
            try
            {
                _dispositivoIoTBLL.AgregarDispositivo(contenedorid,mac,firmware,empresaid);
                TempData["Mensaje"] = $"Dispositivo Vinculado a {contenedor.Nombre} , pendiende para su conexion!";
                return RedirectToAction("Index", "Contenedor", new { institucionId = contenedor.InstitucionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Contenedor = contenedor;
                ViewBag.MAC=mac;
                ViewBag.Firmware=firmware;
                return View();
            }
        }
    }
}
