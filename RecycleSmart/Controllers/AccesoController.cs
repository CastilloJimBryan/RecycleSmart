using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class AccesoController:Controller
    { 

        private readonly EventoAccesoBL _eventoAccesoBL;
        public AccesoController(IConfiguration config)
        {
            string conectar = config.GetConnectionString("BDTFI")!;
            _eventoAccesoBL=new EventoAccesoBL(conectar);
        }

        public IActionResult Prueba(string mac,string uid)
        {
            try
            {
                var e = _eventoAccesoBL.ProcesarAcceso(mac, new LecturaRFID { UID = uid });
                return Content($"{e.EstadoAcceso}: {AlertaBL.MotivoText(e.Motivo)} · usuario {e.UsuarioNombre ?? "-"} · nivel previo {e.NivelPrevio?.ToString() ?? "-"}");
            }
            catch (Exception ex)
            {
                return Content("ERROR: " + ex.ToString());
                //return Content("ERROR: " + ex.Message);
            }
        }
    }
}
