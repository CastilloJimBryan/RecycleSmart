using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class PlanesViewModel
    {
        public List<PlanDisponible> Planes { get; set; } = new();
        public int Instituciones { get; set; }
        public int Contenedores { get; set; }
    }
}
