using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class SuscripcionViewModel
    {
        public Plan PlanActual { get; set; } = new();
        public int Instituciones { get; set; }
        public int Contenedores { get; set; }
        public List<Factura> Facturas { get; set; } = new();
        public DateTime ProximaFacturacion { get; set; }
        public Factura? UltimaFactura { get; set; }
    }
}
