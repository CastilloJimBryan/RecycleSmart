using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class ResumenCambioViewModel
    {
        public Plan PlanActual { get; set; } = new();
        public Plan PlanNuevo { get; set; } = new();
        public decimal Diferencia => PlanNuevo.PrecioMensual - PlanActual.PrecioMensual;
        public DateTime RigeDesde { get; set; }

    }
}
