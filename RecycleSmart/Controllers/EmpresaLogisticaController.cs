using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmartBE;
using RecycleSmartBLL;
using RecycleSmartDAL;
using Servicio;
using System.Globalization;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.RecycleSmart)]
    public class EmpresaLogisticaController : Controller
    {
        private readonly EmpresaLogisticaBLL _empresaLogisticaBLL;
        private readonly PlanDAL _planBLL;

        public EmpresaLogisticaController(IConfiguration configuracion)
        {
            string conectar = configuracion.GetConnectionString("BDTFI")!;
            _empresaLogisticaBLL = new EmpresaLogisticaBLL(conectar);
            _planBLL = new PlanDAL(conectar);
        }

        public IActionResult Index()
        {
            return View(_empresaLogisticaBLL.ListarEmpresa());
        }
        public IActionResult Registrar()
        {
            ViewBag.Planes = _planBLL.ListarPlanes();
            return View(new EmpresaLogistica());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(string? nombre, string? cuit, string? direccion, int planId)
        {
            var empresa = new EmpresaLogistica
            {
                Nombre = nombre ?? "",
                Cuit = cuit ?? "",
                Direccion = direccion ?? "",
                PlanId = planId,
            };

            try
            {
                _empresaLogisticaBLL.AgregarEmpresa(empresa);
                TempData["Mensaje"] = $"Empresa {empresa.Nombre} Registrada Correctamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Planes = _planBLL.ListarPlanes();
                return View(empresa);
            }
        }

        public IActionResult Editar(int Id)
        {
            if (_empresaLogisticaBLL.BuscarXId(Id) is not EmpresaLogistica empresa)
            {
                return NotFound();
            }
            ViewBag.Planes = _planBLL.ListarPlanes();
            return View(empresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(string? nombre, string? cuit, string? direccion, int planId)
        {
            var empresa = new EmpresaLogistica
            {
                Nombre = nombre ?? "",
                Cuit = cuit ?? "",
                Direccion = direccion ?? "",
                PlanId = planId,
            };

            try
            {
                _empresaLogisticaBLL.ModificarEmpresa(empresa);
                TempData["Mensaje"] = $"Empresa {empresa.Nombre} Actualizada";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Planes = _planBLL.ListarPlanes();
                return View(empresa);
            }
        }

        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult CambiarEstado(int id,string? estado)
        {
            return Content($"DEBUG: id={id} estado='{estado}'");
           /* try
            {
                if(!Enum.TryParse<TipoEstado.Estados>(estado, true, out var nuevo))
                {
                    throw new Exception("Estado no Valido!");
                }
                _empresaLogisticaBLL.CambiarEstado(id, nuevo);
                TempData["Mensaje"] = $"Estado Cambiado a {nuevo}";
            }
            catch(Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");*/
        }

    }
}
