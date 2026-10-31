using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;
using System.Globalization;

namespace RecycleSmart.Controllers
{

    public class UmbralController : Controller
    {
        private readonly UmbralBL _UmbralBL;
        private readonly ContenedorBL _ContenedorBL;
        int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        public UmbralController(IConfiguration config)
        {
            string conectar = config.GetConnectionString("BDTFI")!;
            _UmbralBL = new UmbralBL(conectar);
            _ContenedorBL = new ContenedorBL(conectar);
        }
        [Authorize(Roles = Roles.Control)]
        public IActionResult RegistrarUmbral(int contenedorid)
        {
            if (_ContenedorBL.BuscarContenedorXId(contenedorid, empresaid) is not Contenedor contenedor)
            {
                return NotFound();
            }
            try
            {
                ViewBag.Contenedor = contenedor;
                return View(_UmbralBL.Obtener(contenedorid, empresaid));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Contenedor", new { institucionId = contenedor.InstitucionId });
            }
        }

        [HttpPost]
        [Authorize(Roles=Roles.Gestion)]
        public IActionResult RegistrarUmbral(int contenedorid,int? nivelcritico,int?niveladvertencia,string?tempmaxima,int?humedad,int?minutosinconexion)
        {
            if(_ContenedorBL.BuscarContenedorXId(contenedorid,empresaid) is not Contenedor contenedor)
            {
                return NotFound();
            }

            var umbral = new Umbral
            {
                ContenedorId = contenedorid,
                NivelCritico = nivelcritico ?? 0,
                NivelAdvertencia = niveladvertencia ?? 0,
                TemperaturaMaxima = LeerDecimal(tempmaxima),
                HumedadMaxima = humedad ?? 0,
                MinutosSinConexion = minutosinconexion ?? 0
            };

            try
            {
                bool creado = _UmbralBL.Guardar(umbral, empresaid);
                TempData["Mensaje"] = creado ? $"Umbrales Configurados para contenedor {contenedor.Nombre} " :
                    $"Umbrales de {contenedor.Nombre} Actualizado.";

                return RedirectToAction("Index", "Contenedor", new { institucionId = contenedor.InstitucionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Contenedor=contenedor;
                return View(umbral);
            }
        }
        private static decimal LeerDecimal(string? texto)
        {
            return decimal.TryParse((texto ?? "").Replace(',', '.'), NumberStyles.Number,
                                    CultureInfo.InvariantCulture, out var valor)
                ? valor
                : -999;
        }
    }
}
