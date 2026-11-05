using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmart.Models;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;


namespace RecycleSmart.Controllers
{
    [Authorize]
    [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class ContenedorController : Controller
    {
        readonly ContenedorBL _contenedorBL;
        readonly DispositivoIoTBL _dispositivoIoT;
        readonly InstitucionClienteBLL _institucionClienteBLL;
        readonly UmbralBL _usmbralBL;
        readonly TelemetriaBL _telemetriaBL;
        readonly AlertaBL _alertaBL;
        private int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);

        public ContenedorController(IConfiguration configuracion)
        {
            string conectar = configuracion.GetConnectionString("BDTFI")!;

            _contenedorBL = new ContenedorBL(conectar);
            _institucionClienteBLL = new InstitucionClienteBLL(conectar);
            _dispositivoIoT = new DispositivoIoTBL(conectar);
            _usmbralBL=new UmbralBL(conectar);
            _telemetriaBL=new TelemetriaBL(conectar);
            _alertaBL=new AlertaBL(conectar);
        }

        public IActionResult Index(int institucionId = 0,bool bajas=false)
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
            ViewBag.Umbrales = _usmbralBL.UmbralxInstitucion(institucionId, empresaid);
            ViewBag.Lecturas=_telemetriaBL.UltimasLecturas(institucionId,empresaid);
            ViewBag.Bajas = bajas;
            var contenedores = _contenedorBL.ListadoContenedorXInstitucion(institucionId, empresaid, bajas);

            return View(_contenedorBL.ListadoContenedorXInstitucion(institucionId,empresaid));
        }
        public IActionResult Detalle(int id)
        {
            int empresaid = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);
            var contenedor = _contenedorBL.BuscarContenedorXId(id, empresaid);
             
            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid);
            var institucion = contenedor == null ? null : instituciones.FirstOrDefault(i => i.Id == contenedor.InstitucionId);
            if(contenedor==null || institucion==null)
            {
                TempData["Error"] = "El Contenedor NO Existe";
                return RedirectToAction("Index");
            }
            var cd = new ContenedorDetalleViewModel()
            {
                Contenedor= contenedor,
                InstitucionNombre=institucion.Nombre,
                Dispositivos=_dispositivoIoT.BuscarXContenedor(contenedor.Id,empresaid),
                Telemetria=_telemetriaBL.UltimasLecturas(institucion.Id,empresaid).GetValueOrDefault(contenedor.Id),
                Umbral=_usmbralBL.Obtener(contenedor.Id, empresaid),
                AlertasAbiertas=_alertaBL.ListarAlertasAbiertas(empresaid).Where(c=>c.ContenedorId==id).ToList(),
            };
            return View(cd);
        }

        [Authorize(Roles =Roles.Administrador)]
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
        [Authorize(Roles = Roles.Administrador)]
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

        [Authorize(Roles = Roles.Administrador)]
        public IActionResult Editar(int id)
        {
            var contenedor = _contenedorBL.BuscarContenedorXId(id, empresaid);
            if(contenedor== null)
            {
                TempData["Error"] = "El Contenedor No Existe";
                return RedirectToAction("Index");
            }
            ViewBag.Tipos = ContenedorBL.TiposResiduo;
            return View(contenedor);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Administrador)]

        public IActionResult Editar(Contenedor contenedor)
        {
            try
            {
                _contenedorBL.ModificarContenedor(contenedor, empresaid);
                TempData["Mensaje"] = "Contenedor Actualizado";
                return RedirectToAction("Detalle", new {id=contenedor.Id});
            }
            catch(Exception e)
            {
                ModelState.AddModelError("", e.Message);
                ViewBag.Tipos = ContenedorBL.TiposResiduo;
                return View(contenedor);
            }
        }

        public IActionResult Baja(int id)
        {
            var contenedor = _contenedorBL.BuscarContenedorXId(id, empresaid);
            if (contenedor == null)
            {
                TempData["Error"] = "El contenedor no existe";
                return RedirectToAction("Index");
            }

            if (contenedor.Estado.ToString() == "Baja")
            {
                TempData["Error"] = "El Contenedor ya esta dado de baja";
                return RedirectToAction("Detalle", new { id });
            }

            if(_alertaBL.ListarAlertasAbiertas(empresaid).Any(a=>a.ContenedorId==id))
            {

                TempData["Error"] = "El Contenedor No se puede dar de baja , debido que tiene alertas activas";
                return RedirectToAction("Detalle", new { id });
            }
            return View(contenedor);
        }

        [HttpPost]

        public IActionResult Baja(int contenedorid,TipoEstado.MotivoBaja motivo,string? observacion,bool confirmar)
        {
            var contenedor = _contenedorBL.BuscarContenedorXId(contenedorid, empresaid);
            if(contenedor== null)
            {
                TempData["Error"] = "El contenedor no Existe";
                return RedirectToAction("Index");
            }
            try
            {
                if(!confirmar)
                {
                    throw new Exception("Debe Confirmar para realizar la baja del contenedor");
                }
                int usuarioid = Servicio.ManejosSesiones.ObtenerUsuarioEmpresaLogistica(User);
                _contenedorBL.BajaContenedor(contenedorid, empresaid, motivo, observacion, usuarioid);
                TempData["Mensaje"] = $"El Contenedor {contenedor.Nombre} fue dado de baja Correctamente";
                return RedirectToAction("Index", new { institucionId = contenedor.InstitucionId });
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Motivo = motivo;
                ViewBag.Observacion = observacion;
                return View(contenedor);
            }
        }
    }
}
