using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;
using static RecycleSmartBE.DiasSemana;

namespace RecycleSmart.Controllers
{
    public class TarjetaRFIDController : Controller
    {
        private readonly TarjetaRFIDBL _tarjetaRFIDBL;

        
        public TarjetaRFIDController(IConfiguration configuration)
        {
            string conectar = configuration.GetConnectionString("BDTFI")!;
            _tarjetaRFIDBL=new TarjetaRFIDBL(conectar);
        }

        int empresaid => Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);

        private void CargarFormulario(UsuarioInstitucion ui)
        {
            ViewBag.Usuario = ui;
            ViewBag.Contenedores = _tarjetaRFIDBL.ContenedoresDisponibles(ui.InstitucionId, empresaid);
        }
        public IActionResult Asignar(int usuarioid)
        {
            try
            {
                var usuario = _tarjetaRFIDBL.ObtnerUsuario(usuarioid, empresaid);

                CargarFormulario(usuario);

                return View(new TarjetaRFID
                {
                    UsuarioInstitucionId = usuarioid,
                    DiasHabilitados = DiasSemana.DiaSemana.Lunes | DiasSemana.DiaSemana.Martes | DiasSemana.DiaSemana.Miercoles | DiasSemana.DiaSemana.Jueves | DiasSemana.DiaSemana.Viernes,
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "UsuarioInstitucion");
            }
        }


        [HttpPost]

        public IActionResult Asignar(int usuarioinstitucionId,string ? uid,DateTime? fechavencimiento, int[]? dias,TimeSpan?horainicio,TimeSpan? horafin, int[]?contenedores )
        {
            var tarjeta = new TarjetaRFID
            {
                UID = uid ?? "",
                FechaVecimiento = fechavencimiento ?? DateTime.MinValue,
                DiasHabilitados = (DiaSemana)(dias?.Sum() ?? 0),
                HoraInicio = horainicio ?? TimeSpan.Zero,
                HoraFin = horafin ?? TimeSpan.Zero,
                ContenedoresId = contenedores?.ToList() ?? new List<int>()
            };

            try
            {
                var usuario = _tarjetaRFIDBL.ObtnerUsuario(usuarioinstitucionId, empresaid);
                _tarjetaRFIDBL.AsignarTarjeta(tarjeta, empresaid);

                TempData["Mensaje"] = $"Tarjeta {tarjeta.UID} Asignada a {usuario.Nombre} {usuario.Apellido}";
                return RedirectToAction("Index", "UsuarioInstitucion", new { institucionId = usuario.InstitucionId });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                try
                {
                    CargarFormulario(_tarjetaRFIDBL.ObtnerUsuario(usuarioinstitucionId,empresaid))
                }
                catch(Exception e)
                {
                    TempData["Error"] = e.Message;
                    return RedirectToAction("Index", "UsuarioInstitucion");
                }
            }
            return View(tarjeta);
        }
    }
}
