using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmart.Models;
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
        private readonly UsuarioEmpresaLogisticaBL _usuarioempresaLogisticabl;
        private readonly ContenedorBL _contenedorBL;
        private readonly InstitucionClienteBLL _institucionclienteBL;
        private readonly PlanDAL _planBLL;

        public EmpresaLogisticaController(IConfiguration configuracion)
        {
            string conectar = configuracion.GetConnectionString("BDTFI")!;
            _empresaLogisticaBLL = new EmpresaLogisticaBLL(conectar);
            _planBLL = new PlanDAL(conectar);
            _usuarioempresaLogisticabl = new UsuarioEmpresaLogisticaBL(conectar);
            _contenedorBL = new ContenedorBL(conectar);
            _institucionclienteBL = new InstitucionClienteBLL(conectar);
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
                int nuevoid=_empresaLogisticaBLL.AgregarEmpresa(empresa);
                TempData["Mensaje"] = $"Empresa {empresa.Nombre} Registrada Correctamente";
                return RedirectToAction("CrearAdmin", new {id=nuevoid});
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
        public IActionResult Editar(int id,string? nombre, string? cuit, string? direccion, int planId)
        {
            var empresa = new EmpresaLogistica
            {
                Id = id,
                Nombre = nombre ?? "",
                Cuit = cuit ?? "",
                Direccion = direccion ?? "",
                PlanId = planId,
            };

            try
            {
                _empresaLogisticaBLL.ModificarEmpresa(empresa);
                TempData["Mensaje"] = $"Empresa {empresa.Nombre} Actualizada";
                return RedirectToAction("Detalle", new { id=empresa.Id });
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
            
            try
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
            return RedirectToAction("Detalle",new {id});
        }

        public IActionResult Detalle(int id)
        {
            if (_empresaLogisticaBLL.BuscarXId(id) is not EmpresaLogistica empresa)
                return NotFound();

            var (institucionesUsadas, institucionesMax) = _institucionclienteBL.UsoDelPlan(id);
            var (contenedoresUsados, contenedoresMax) = _contenedorBL.UsoxPlan(id);

            var vm = new EmpresaDetalleViewModel
            {
                Empresa = empresa,
                Usuarios = _usuarioempresaLogisticabl.ListarUsuarioxEmpresa(id),
                TieneAdministrador = _usuarioempresaLogisticabl.tieneAdmin(id),
                Uso = new UsoPlan
                {
                    PlanNombre = empresa.PlanNombre,
                    Instituciones = institucionesUsadas,
                    MaxInstituciones = institucionesMax,
                    Contenedores = contenedoresUsados,
                    MaxContenedores = contenedoresMax
                }
            };
            return View(vm);
        }

        public IActionResult CrearAdmin(int id)
        {
            if (_empresaLogisticaBLL.BuscarXId(id) is not EmpresaLogistica empresa)
                return NotFound();

            if (empresa.Estados == TipoEstado.Estados.Baja)
            {
                TempData["Error"] = "La empresa está dada de baja.";
                return RedirectToAction("Detalle", new { id });
            }
            if (_usuarioempresaLogisticabl.tieneAdmin(id))
            {
                TempData["Error"] = "La empresa ya tiene un administrador activo.";
                return RedirectToAction("Detalle", new { id });
            }

            ViewBag.Empresa = empresa;
            return View(new UsuarioEmpresaLogistica { EmpresaLogisticaId = id });
        }

        [HttpPost]
        public IActionResult CrearAdmin(int empresaLogisticaId, string? nombre, string? apellido, string? correo, string? clave)
        {
            var usuario = new UsuarioEmpresaLogistica
            {
                EmpresaLogisticaId = empresaLogisticaId,
                Nombre = nombre ?? "",
                Apellido = apellido ?? "",
                Correo = correo ?? ""
            };

            try
            {
                _usuarioempresaLogisticabl.CrarAdmin(usuario, clave);
                TempData["Mensaje"] = $"Administrador {usuario.Nombre} {usuario.Apellido} creado correctamente.";
                return RedirectToAction("Detalle", new { id = empresaLogisticaId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Empresa = _empresaLogisticaBLL.BuscarXId(empresaLogisticaId);
                return View(usuario);
            }
        }

    }
}
