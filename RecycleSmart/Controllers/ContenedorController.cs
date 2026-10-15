using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize]
    //[RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class ContenedorController : Controller
    {
        readonly ContenedorBL _contenedorBL;
        readonly DispositivoIoTBL _dispositivoIoT;
        readonly InstitucionClienteBLL _institucionClienteBLL;
        readonly UmbralBL _usmbralBL;
        private int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);

        public ContenedorController(IConfiguration configuracion)
        {
            string conectar = configuracion.GetConnectionString("BDTFI")!;

            _contenedorBL = new ContenedorBL(conectar);
            _institucionClienteBLL = new InstitucionClienteBLL(conectar);
            _dispositivoIoT = new DispositivoIoTBL(conectar);
            _usmbralBL=new UmbralBL(conectar);
        }

        public IActionResult Index(int institucionId = 0)
        {
            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid)
                .Where(i=>i.Estados!=TipoEstado.Estados.Baja).ToList();
            var dispositivos = _dispositivoIoT.DispositivosXInstitucion(institucionId, empresaid);

            ViewBag.InstitucionId = institucionId;
            ViewBag.Instituciones = instituciones;

            var (usado, maximo) = _contenedorBL.UsoxPlan(empresaid);
            ViewBag.Maximo = maximo;   
            ViewBag.Usado= usado;

            if(institucionId==0)
            {
                return View(new List<Contenedor>());
            }

            var institucion=instituciones.FirstOrDefault(i=>i.Id==institucionId);
            if (institucion == null) { return NotFound(); }

            ViewBag.InstitucionNombre = institucion.Nombre;
            ViewBag.Dispositivos = dispositivos;
            ViewBag.Umbrales = _usmbralBL.ContenedorContieneUmbral(institucionId, empresaid);
            return View(_contenedorBL.ListadoContenedorXInstitucion(institucionId,empresaid));
        }

        public IActionResult Registrar(int institucionId=0)
        {
            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid)
                .FirstOrDefault( i=>i.Id==institucionId && i.Estados==TipoEstado.Estados.Activo);
            if (instituciones == null) return RedirectToAction("Index");

            ViewBag.InstitucionNombre=instituciones.Nombre;
            ViewBag.Tipos = ContenedorBL.TiposResiduo;

            return View(new Contenedor { InstitucionId=institucionId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Registrar(int InstitucionId,string?nombre,string?tiporesiduo,int?capacidad,int?piso,string?sector)
        {
            var contenedor = new Contenedor
            {
                Nombre = nombre ?? "",
                TipoResiduo = tiporesiduo ?? "",
                CapacidadLitros = capacidad ?? 0,
                Piso = piso,
                Sector = sector ,
                InstitucionId= InstitucionId
            };

            try
            {
                _contenedorBL.AgregarContenedor(contenedor, empresaid);
                TempData["Mensaje"] = $"Contenedor {contenedor.Nombre} Se Agrego Correctameente";
                return RedirectToAction("Index", new { institucionId = InstitucionId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid)
                .FirstOrDefault(i => i.Id == InstitucionId)?.Nombre ?? "";

                ViewBag.InstitucionNombre = instituciones;
                ViewBag.Tipos = ContenedorBL.TiposResiduo;
                return View(contenedor);
            }
        }
    }
}
