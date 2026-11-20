using RecycleSmartBE;
using RecycleSmartDAL;
using Servicio;

namespace RecycleSmartBLL
{
    public class EmpresaLogisticaBLL
    {
        private readonly EmpresaLogisticaDAL _empresaLogisticaDAL;
        private readonly PlanDAL _planDAL;

        public EmpresaLogisticaBLL(string conectar)
        {
            _empresaLogisticaDAL = new EmpresaLogisticaDAL(conectar);
            _planDAL = new PlanDAL(conectar);
        }
        private void Validar(EmpresaLogistica e )
        {
            e.Nombre = e.Nombre?.Trim() ?? "";
            e.Direccion = e.Direccion?.Trim() ?? "";
            e.Cuit =Validaciones.NormalizarCUIT(e.Cuit);

            if (e.Nombre == "") throw new Exception("El Nombre es Obligatorio");
            if (e.Direccion == "") throw new Exception("La Direccion es Obligatorio");
            if (_empresaLogisticaDAL.ExisteCuit(e.Cuit, e.Id)) throw new Exception("Ya Existe una Empresa con ese CUIT");
            if(!_planDAL.ListarPlanes().Any(p=>p.Id==e.PlanId)) throw new Exception("Seleccione un  Plan Valido"); 

        }
        
        public List<EmpresaLogistica> ListarEmpresa()
        {
          return  _empresaLogisticaDAL.Listar();
        }

        public int AgregarEmpresa(EmpresaLogistica el)
        {
            Validar(el);
            el.Estados = TipoEstado.Estados.Activo;
            el.FechaRegistro =DateTime.Today;
            return _empresaLogisticaDAL.AgregarEmpresaLogistica(el);
        }
        public void ModificarEmpresa(EmpresaLogistica e)
        {
            if(_empresaLogisticaDAL.BuscarXId(e.Id) is not EmpresaLogistica actual)
            {
                throw new Exception("La Empresa no Existe");
            }
            if (actual.Estados == TipoEstado.Estados.Baja)
            {
                throw new Exception("No se Puede Editar una Empresa de Baja");
            }
            Validar(e);
            _empresaLogisticaDAL.ModificarEmpresaLogistica(e);
        }
        public EmpresaLogistica? BuscarXId(int id)
        {
            return _empresaLogisticaDAL.BuscarXId(id);
        }
        public const int largonota = 15;
        public void CambiarEstado(int empresaid,TipoEstado.Estados nuevo,string?nota,int usuarioId)
        {
            nota = nota?.Trim() ?? "";
            if(nota.Length<=largonota)
            {
                throw new Exception($"La nota debe tener al menos {largonota} caracteres ");
            }
            var empresa = BuscarXId(empresaid) ?? throw new Exception("La Empresa no Existe");
            var actual = empresa.Estados;
           
            if (actual== TipoEstado.Estados.Baja)
            {
                throw new Exception("No se Puede Editar una Empresa de Baja");
            }
            if(actual==nuevo)
            {
                throw new Exception($"La empresa ya esta en Estado: {nuevo}");
            }
            string accion = nuevo switch
            {
                TipoEstado.Estados.Suspendido => "Suspendido",
                TipoEstado.Estados.Activo => "Activo",
                TipoEstado.Estados.Baja => "Baja",
                _ => throw new Exception("Estado no Valido")
            };

            _empresaLogisticaDAL.CambiarEstadoConAuditoria(empresaid, nuevo,accion,nota,usuarioId);
        }
        public List<AuditoriaEmpresa> ListadoAuditoriaEmpresa(int empresaid)
        {
            return _empresaLogisticaDAL.ListarAuditoriaEmpresa(empresaid);
        }
    }
}
