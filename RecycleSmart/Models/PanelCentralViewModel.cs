using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class PanelCentralViewModel
    {
        public List<EmpresaResumen> Empresas { get; set; } = new();
        public int EmpresasActivas { get; set; }
        public int EmpresasSuspendidas { get; set; }
        public int Instituciones { get; set; }
        public int Contenedores { get; set; }
        public int AlertasCriticas { get; set; }
        public List<EmpresaResumen> SinAdministrador { get; set; } = new();
        public List<EmpresaResumen> CercaDelLimite { get; set; } = new();
        public List<(string Plan, int Cantidad)> PorPlan { get; set; } = new();
    }
}
