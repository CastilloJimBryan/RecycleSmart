using RecycleSmartBE;
using RecycleSmartDAL;
using Servicio;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class InstitucionClienteBLL
    {

        private readonly InstitucionClienteDAL _institucionClienteDAL;
        private readonly EmpresaLogisticaDAL _empresaLogisticaDAL;
        private readonly PlanDAL _planDAL;
        public InstitucionClienteBLL(string conexion)
        {
            _institucionClienteDAL=new InstitucionClienteDAL(conexion);
            _empresaLogisticaDAL=new EmpresaLogisticaDAL(conexion);
            _planDAL=new PlanDAL(conexion);
        }
        public static readonly string[] Tipos =
            { "Hospital", "Clínica", "Laboratorio", "Centro de Salud", "Consultorio", "Otro" };
        public List<InstitucionCliente> ListadoInstitucioneXEmpresa(int empresaid)
        {
            return _institucionClienteDAL.ListarInstucionXEmpresa(empresaid);
        }
        public InstitucionCliente? BuscarInstitucionXId(int id, int empresa)
        {
            return _institucionClienteDAL.BuscarXId(id,empresa);
        }
        public (int Usadas, int Maximo) UsoDelPlan(int empresaId)
        {
            var plan = ObtenerPlan(empresaId);
            return (_institucionClienteDAL.CantidadInstituciones(empresaId), plan.MaxInstituciones);
        }
        private void Validar(InstitucionCliente i)
        {
            i.Nombre = i.Nombre?.Trim() ?? "";
            i.Direccion = i.Direccion?.Trim() ?? "";
            i.Tipo = i.Tipo?.Trim() ?? "";
            i.Cuit = Validaciones.NormalizarCUIT(i.Cuit);

            if (i.Nombre == "") throw new Exception("El Nombre es Obligatorio");
            if (i.Direccion == "") throw new Exception("La Direccion es Obligatorio");
            if (_institucionClienteDAL.ExisteCuit(i.Cuit, i.Id)) throw new Exception("Ya Existe una Institucion con ese CUIT");
            if (!Tipos.Contains(i.Tipo)) throw new Exception("Seleccione un Tipo Valido");
        }
        private Plan ObtenerPlan(int empresaId)
        {
            if (_empresaLogisticaDAL.BuscarXId(empresaId) is not EmpresaLogistica empresa)
            {
                throw new Exception("Empresa No Encontrada");
            }
            if(_planDAL.BuscarPlanxId(empresa.PlanId) is not Plan plan)
            {
                throw new Exception("La Empresa No tiene ningun Plan Valido");
            }
            return plan;
        }
        private void VerificarLimitePlan(int empresaId)
        {
            if(_empresaLogisticaDAL.BuscarXId(empresaId) is not EmpresaLogistica empresa)
            {
                throw  new Exception("Empresa No Encotrada");
            }
            if(empresa.Estados != TipoEstado.Estados.Activo)
            {
                throw new Exception("Empresa No esta Activada");
            }

            var planes=ObtenerPlan(empresaId);

            if (planes.MaxInstituciones == 0) return;

            int actuales = _institucionClienteDAL.CantidadInstituciones(empresaId);
            if (actuales >= planes.MaxInstituciones)
            {
                throw new Exception($"Su Plan {planes.Nombre} Permite hasta {planes.MaxInstituciones} Instituciones " +
                    $" y ya tiene {actuales}, si quiere agregar mas debe actualizar su plan");
            }
        }
        public int AgregarInstitucion(InstitucionCliente ic)
        {
            Validar(ic);
            VerificarLimitePlan(ic.EmpresaLogisticaId);
            ic.Estados = TipoEstado.Estados.Activo;
            return _institucionClienteDAL.AgregarInstitucion(ic);
        }
        public void ModificarInstitucion(InstitucionCliente ic)
        {
            if(_institucionClienteDAL.BuscarXId(ic.Id,ic.EmpresaLogisticaId) is not InstitucionCliente actual)
            {
                throw new Exception("La institucion no existe");
            }
            if (actual.Estados == TipoEstado.Estados.Baja)
            {
                throw new Exception("No se puede editar una institucion de baja");
            }
            Validar(ic);
            _institucionClienteDAL.ModificarInstitucion(ic);
        }
        public void CambiarEstado(TipoEstado.Estados estado,int institucinId,int empresaId)
        {
            if(_institucionClienteDAL.BuscarXId(institucinId,empresaId) is not InstitucionCliente actual)
            {
                throw new Exception("No Existe institucion");
            }
            if(actual.Estados==TipoEstado.Estados.Baja)
            {
                throw new Exception("Institucion de baja no se puede cambiar el estado");

            }
            if(actual.Estados==estado)
            {

                throw new Exception(@"La institucion ya esta en estado {estado}");
            }
            _institucionClienteDAL.CambiarEstado(estado.ToString(), institucinId, empresaId);
        }
    }
}
