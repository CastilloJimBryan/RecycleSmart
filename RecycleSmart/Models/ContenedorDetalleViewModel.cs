using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class ContenedorDetalleViewModel
    {
        public Contenedor Contenedor {  get; set; }=new Contenedor();
        public DispositivoIoT? Dispositivos { get; set; }
        public Umbral? Umbral { get; set; }
        public Telemetria? Telemetria { get; set; }
        public List<Alerta> AlertasAbiertas { get; set; } = new List<Alerta>();
        public string InstitucionNombre { get; set; } = string.Empty;
        public List<Lote> Lotes { get; set; } = new();
    }
}
