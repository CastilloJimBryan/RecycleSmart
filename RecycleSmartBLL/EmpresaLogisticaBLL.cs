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
            if(!_planDAL.ListarPlanes().Any(p=>p.Id==e.PlanId)) throw new Exception("Seleccione un  plan Valido"); 

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
        public void CambiarEstado(int id,TipoEstado.Estados nuevo)
        {
            if (_empresaLogisticaDAL.BuscarXId(id) is not EmpresaLogistica actual)
            {
                throw new Exception("La Empresa no Existe");
            }
            if (actual.Estados == TipoEstado.Estados.Baja)
            {
                throw new Exception("No se Puede Editar una Empresa de Baja");
            }
            if(actual.Estados==nuevo)
            {
                throw new Exception(@"La empresa ya esta en Estado: {nuevo}");
            }

            _empresaLogisticaDAL.CambiarEstado(id, nuevo.ToString());
        }

    }
}
