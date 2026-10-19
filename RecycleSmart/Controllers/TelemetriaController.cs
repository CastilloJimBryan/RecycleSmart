using Microsoft.AspNetCore.Mvc;
using RecycleSmartBE;
using RecycleSmartBLL;

namespace RecycleSmart.Controllers
{
    public class TelemetriaController: Controller
    {
        private readonly TelemetriaBL _telemetriaBL;
        public TelemetriaController(IConfiguration configura)
        {
            string conect = configura.GetConnectionString("BDTFI")!;
            _telemetriaBL=new TelemetriaBL(conect);
        }

        public ActionResult Prueba(string mac,int nivel)
        {
            try
            {
                var t = _telemetriaBL.ProcesarLectura(mac, new LecturaSensor
                {
                    Nivel= nivel,Temperatura=24.5m,Humedad=60,Bateria=80,NivelConexion=-65
                });
                return Content($"Ok: Lectura {t.Id} Guardada para el Contenedor {t.ContenedorId}");
            }
            catch(Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
    }
}
