using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class EmpresaDetalleViewModel
    {
        public EmpresaLogistica Empresa { get; set; } = new();
        public UsoPlan? Uso { get; set; }
        public List<UsuarioEmpresaLogistica> Usuarios { get; set; } = new();
        public bool TieneAdministrador { get; set; }
    }
}
