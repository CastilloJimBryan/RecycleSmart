using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize(Roles = "Administrador")]
    [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class UsuarioEmpresaLogisticaController : Controller
    {
        private readonly UsuarioEmpresaLogisticaBL _usuarioEmpresaLogisticaBL;


        private int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        private int usuarioActualId => Servicio.ManejosSesiones.ObtenerUsuarioId(User);
        public UsuarioEmpresaLogisticaController(IConfiguration configuracion)
        {
            string conect = configuracion.GetConnectionString("BDTFI")!;
            _usuarioEmpresaLogisticaBL = new UsuarioEmpresaLogisticaBL(conect);
        }

        public IActionResult Index()
        {
            ViewBag.UsuarioActual = usuarioActualId;
            return View(_usuarioEmpresaLogisticaBL.ListarUsuarioxEmpresa(empresaid));
        }

        public IActionResult Registrar()
        {
            ViewBag.Roles = "Administrador";
            return View(new UsuarioEmpresaLogistica());
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]

        public IActionResult Registrar(string? nombre, string? apellido, string? Correo, string? rol, string? clave)
        {
            var usuario = new UsuarioEmpresaLogistica
            {
                EmpresaLogisticaId = empresaid,
                Id = usuarioActualId,
                Nombre = nombre ?? "",
                Apellido = apellido ?? "",
                Correo = Correo ?? "",
                Rol = rol ?? "",
            };

            try
            {
                int id = _usuarioEmpresaLogisticaBL.AgregarUsuarioEmpresa(usuario, clave);
                TempData["Mensaje"] = $"Usuario : {usuario.Nombre} {usuario.Apellido} creado Correctamente";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(usuario);
            }
        }

        public IActionResult Detalle(int id)
        {
            if (_usuarioEmpresaLogisticaBL.BuscarUsuarioxId(id, empresaid) is not UsuarioEmpresaLogistica usuario)
            {
                return NotFound();
            }
            ViewBag.EsUsuarioActual = id == usuarioActualId;
            return View(usuario);
        }
        public IActionResult Editar(int id)
        {
            if (_usuarioEmpresaLogisticaBL.BuscarUsuarioxId(id, empresaid) is not UsuarioEmpresaLogistica usuario)
            {
                return NotFound();
            }
            ///falta roles
            ViewBag.EsUsuarioActual = id == usuarioActualId;
            return View(usuario);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]

        public IActionResult Editar(int id, string? nombre, string? apellido, string? correo, string? rol)
        {
            var usuario = new UsuarioEmpresaLogistica
            {
                Id = id,
                EmpresaLogisticaId = empresaid,
                Nombre = nombre ?? "",
                Apellido = apellido ?? "",
                Correo = correo ?? "",
                Rol = rol ?? ""
            };

            try
            {
                _usuarioEmpresaLogisticaBL.ModificarUsuarioEmpresa(usuario, usuarioActualId);
                TempData["Mensaje"] = "Cambios Guadados";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.EsUsuarioActual = id == usuarioActualId;
                //falta roles
                return View(usuario);
            }
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]

        public IActionResult RestablecerClave(int id, string? clavenueva)
        {
            try
            {
                _usuarioEmpresaLogisticaBL.CambiarClave(id, empresaid, clavenueva);
                TempData["Mensaje"] = "Clave Nueva Guardada";

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Detalle", new { id });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]

        public IActionResult CambiarEstado(int id, string? estadonuevo)
        {
            try
            {
                if(Enum.TryParse<TipoEstado.Estados>(estadonuevo,true,out var tipoEstado))
                {
                    throw new Exception("Estado no Valido");
                }
                _usuarioEmpresaLogisticaBL.CambiarEstado(id,empresaid, tipoEstado,usuarioActualId);
                TempData["Mensaje"] = "Se Actualizo el Estado";
            }
            catch(Exception ex)
            {
                TempData["Error"]=ex.Message;
            }
            return RedirectToAction("Detalle", new { id });
        }
    }
}
