using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class DashboardViewModel
    {
        public ResumenDashboard ResumenDashboard { get; set; }=new ResumenDashboard();
        public List<Alerta> Cola {  get; set; } = new List<Alerta>();
        public List<InstitucionCliente> InstitucionClientes { get; set; } = new List<InstitucionCliente>();
        public int InstitucionId {  get; set; }
        public int AlertasAbiertaXEmpresa { get; set; }
        public UsoPlan usoPlan{  get; set; }=new UsoPlan();

    }
}
