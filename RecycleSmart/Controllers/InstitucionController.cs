using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class InstitucionController : Controller
    {
        private readonly InstitucionClienteBLL _institucionClienteBL;
        public InstitucionController(IConfiguration confi)
        {
            string conect = confi.GetConnectionString("BDTFI")!;
            _institucionClienteBL = new InstitucionClienteBLL(conect);
        }
        private int empresaId => ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        public IActionResult Index()
        {
            var (usadas, maximo) = _institucionClienteBL.UsoDelPlan(empresaId);

            ViewBag.Usadas = usadas;
            ViewBag.Maximo = maximo;

            return View(_institucionClienteBL.ListadoInstitucioneXEmpresa(empresaId));
        }
        public IActionResult Detalle(int id)
        {
            if (_institucionClienteBL.BuscarInstitucionXId(id,empresaId) is not InstitucionCliente institucion)
            {
                return NotFound();
            }
            return View(institucion);
        }
        public IActionResult Registrar()
        {
            ViewBag.Tipos = InstitucionClienteBLL.Tipos;
            return View(new InstitucionCliente());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Registrar(string? nombre, string? cuit, string? direccion, string? tipo)
        {
            var institucion = new InstitucionCliente
            {
                EmpresaLogisticaId = empresaId,
                Nombre = nombre ?? "",
                Cuit = cuit ?? "",
                Direccion = direccion ?? "",
                Tipo = tipo ?? ""
            };

            try
            {
                int id = _institucionClienteBL.AgregarInstitucion(institucion);
                TempData["Mensaje"] = $"Institucion{institucion.Nombre} Se Registro Correctamente";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Tipos = InstitucionClienteBLL.Tipos;
                return View(institucion);
            }
        }

        public IActionResult Editar(int id)
        {
            if (_institucionClienteBL.BuscarInstitucionXId(id, empresaId) is not InstitucionCliente institucion)
            {
                return NotFound();
            }
            ViewBag.Tipos = InstitucionClienteBLL.Tipos;
            return View(institucion);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]

        public IActionResult Editar(int id, string? nombre, string? cuit, string? direccion,string? tipo)
        {
            var insti = new InstitucionCliente
            {
                Id = id,
                EmpresaLogisticaId = empresaId,
                Nombre = nombre ??"",
                Cuit = cuit??"",
                Direccion = direccion??"",
                Tipo = tipo ??""
            };
            try
            {
                _institucionClienteBL.ModificarInstitucion(insti);
                TempData["Mensaje"] = $"Cambios Guardados";
                return RedirectToAction("Detalle", new { id });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Tipos = InstitucionClienteBLL.Tipos;
                return View(insti);
            }
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        
        public IActionResult CambiarEstado(int id,string? estado)
        {
            try
            {
                if (!Enum.TryParse<TipoEstado.Estados>(estado, true, out var nuevo))
                    throw new Exception("Estado no válido.");

                _institucionClienteBL.CambiarEstado(nuevo, id,empresaId );
                TempData["Mensaje"] = $"Estado cambiado a {nuevo}.";
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id });
        }
    }
}
