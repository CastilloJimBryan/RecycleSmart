using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class UsuarioInstitucionController : Controller
    {
        private readonly UsuarioInstitucionBL _usuarioInstitucionBL;
        private readonly InstitucionClienteBLL _institucionClienteBLL;
        private readonly TarjetaRFIDBL _tarjetaRFIDBL;

        public UsuarioInstitucionController(IConfiguration configuration)
        {
            string conectar = configuration.GetConnectionString("BDTFI")!;
            _usuarioInstitucionBL = new UsuarioInstitucionBL(conectar);
            _institucionClienteBLL = new InstitucionClienteBLL(conectar);
            _tarjetaRFIDBL=new TarjetaRFIDBL(conectar);
        }
        int empresaId => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
        public IActionResult Index(int institucionid=0)
        {
            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaId).Where(i => i.Estados != RecycleSmartBE.TipoEstado.Estados.Baja).ToList();
           /* if(institucionid==0 && instituciones.Count>0)
            {
                institucionid = instituciones[0].Id;
            }*/

            ViewBag.Instituciones=instituciones;
            ViewBag.InstitucionesId=institucionid;

            var usuarios = institucionid == 0 ? new List<UsuarioInstitucion>() : _usuarioInstitucionBL.ListarxInstitucion(institucionid, empresaId);

            ViewBag.Tarjetas = institucionid == 0 ? new Dictionary<int, TarjetaRFID>() : _tarjetaRFIDBL.TarjetaXInstitucion(institucionid, empresaId);
            
            return View(usuarios);
        }

        public IActionResult RegistrarUsuario(int institucionid)
        {
            var institucion = _institucionClienteBLL.BuscarInstitucionXId(institucionid, empresaId);
            if(institucion==null || institucion.Estados!=TipoEstado.Estados.Activo)
            {
                TempData["Error"] = "Solo se pueden Agregar Usuarios a Instituciones Activas";

                return RedirectToAction("Index");
            }

            ViewBag.InstitucionNombre=institucion?.Nombre;
            return View(new UsuarioInstitucion { InstitucionId=institucionid});
        }

        [HttpPost]

        public IActionResult RegistrarUsuario(string? nombre,string? apellido,string? dni,string? rol,string ? sector,int institucionid)
        {
            var usuario = new UsuarioInstitucion
            {
                Nombre = nombre ?? "",
                Apellido = apellido ?? "",
                DNI = dni ?? "",
                Rol = rol ?? "",
                Sector = sector ?? "",
                InstitucionId = institucionid,
            };

            try
            {
                _usuarioInstitucionBL.AgregarUsuarioInstitucion(usuario, empresaId);
                TempData["Mensaje"] = "Se Registro Nuevo Usuario";
                return RedirectToAction("Index", new { institucionid });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.InstitucionNombre = _institucionClienteBLL.BuscarInstitucionXId(institucionid, empresaId)?.Nombre;
                return View(usuario);
            }
        }
    }
}
