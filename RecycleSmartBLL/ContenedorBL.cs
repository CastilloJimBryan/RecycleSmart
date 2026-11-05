using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class ContenedorBL
    {
        private readonly ContenedorDAL _contenedorDAL;
        private readonly EmpresaLogisticaDAL _empresaLogisticaDAL;
        private readonly InstitucionClienteDAL _institucionClienteDAL;
        private readonly PlanDAL _planDAL;
        private readonly AlertaBL _alertaBL;
        public ContenedorBL(string conectar)
        {
            _contenedorDAL = new ContenedorDAL(conectar);
            _empresaLogisticaDAL = new EmpresaLogisticaDAL(conectar);
            _institucionClienteDAL=new InstitucionClienteDAL(conectar);
            _planDAL=new PlanDAL(conectar);
            _alertaBL= new AlertaBL(conectar);
        }
        public static readonly string[] TiposResiduo ={ "Patogénico", "Cortopunzante", "Anatomopatológico", "Farmacéutico", "Químico" };

        public List<Contenedor> ListadoContenedorXInstitucion(int institucionId, int empresaid)
        {
            return _contenedorDAL.ListasContenedorXInstitucion(institucionId, empresaid);
        } 
        public Contenedor? BuscarContenedorXId(int mid,int empresaid)
        {
            return _contenedorDAL.BuscarContenedorXId(mid, empresaid);
        }
        private Plan ObtenerPlan(int empresaId)
        {
            if(_empresaLogisticaDAL.BuscarXId(empresaId) is not EmpresaLogistica empresa)
            {
                throw new Exception("Empresa No Encontrada");
            }
            if(_planDAL.BuscarPlanxId(empresa.PlanId) is not Plan plan)
            {
                throw new Exception("La Empresa No tiene Plan Valido");
            }
            return plan;
        }
        public (int usadas,int maximo) UsoxPlan(int empresaId)
        {
            var plan = ObtenerPlan(empresaId);
            return (_contenedorDAL.CantidadDeContenedoresXEmpresa(empresaId), plan.MaxContenedores);
        }
        private void ValidarCampos(Contenedor c)
        {
            c.Nombre = c.Nombre?.Trim() ?? "";
            c.TipoResiduo = c.TipoResiduo?.Trim() ?? "";
            if(c.Piso is < -5 or >200)
            {
                throw new Exception("Debe Poner un piso valido");
            }
            c.Sector = string.IsNullOrWhiteSpace(c.Sector) ? null : c.Sector;

            if (c.Nombre == "") throw new Exception("Debe Completar Nombre");
            if (!TiposResiduo.Contains(c.TipoResiduo)) throw new Exception("Debe Elegir un tipo Valido");
            if (c.CapacidadLitros <= 0) throw new Exception("Debe ser un valor positivo");

            if (_contenedorDAL.ExisteContenedor(c.Nombre, c.InstitucionId, c.Id))
            {
                throw new Exception("Ya existe ese contenedor");
            }
            
           
        }

        private void VerificarLimitePlan(int empresaId)
        {
            if(_empresaLogisticaDAL.BuscarXId(empresaId) is not EmpresaLogistica empresa)
            {
                throw new Exception("No Existe Empresa");
            }
            if (empresa.Estados == TipoEstado.Estados.Baja)
            {
                throw new Exception("Empresa Inactiva");
            }
            var plan = ObtenerPlan(empresaId);
            if (plan.MaxContenedores == 0) return;

            int Actual = _contenedorDAL.CantidadDeContenedoresXEmpresa(empresaId);
            if (Actual >= plan.MaxContenedores)
            {
                throw new Exception($"Su plan {plan.Nombre} permite hasta {plan.MaxContenedores} contenedores " +
                    $"y ya tiene {Actual}. Para agregar más, actualice su plan.");
            }
            
        }
        private Contenedor ObtenerContenedores(int id,int empresaId)
        {
            if(_contenedorDAL.BuscarContenedorXId(id,empresaId) is not Contenedor contenedor)
            {
                throw new Exception("El contenedor no existe");
            }
            if(contenedor.Estado==TipoEstado.Estados.Baja)
            {
                throw new Exception("Contenedor de baja");
            }
            return contenedor;
          
        }
        public int AgregarContenedor(Contenedor c, int empresaid)
        {
            if(_institucionClienteDAL.BuscarXId(c.InstitucionId,empresaid) is not InstitucionCliente institucion)
            {
                throw new Exception("Seleccione una Institucion Valida");
            }
            if(institucion.Estados!=TipoEstado.Estados.Activo)
            {
                throw new Exception("Institucion Inactiva, No se le Puede agregar Conteneedores");
            }

            ValidarCampos(c);
            VerificarLimitePlan(empresaid);
            c.Estado = TipoEstado.Estados.Activo;
            return _contenedorDAL.AltaContenedor(c);
        }

        public void ModificarContenedor(Contenedor c,int empresaId)
        {
            var actual = ObtenerContenedores(c.Id, empresaId);
            if (actual == null) throw new Exception("No Existe Contenedor");
            c.InstitucionId = actual.InstitucionId;
            ValidarCampos(c);

            if (!_contenedorDAL.ModificarContenedor(c, empresaId))
            {
                throw new Exception("No Existe Contenedor");
            }
        }
        public void BajaContenedor(int contenedorid, int empresaid, TipoEstado.MotivoBaja motivo, string? observacion, int usuarioid)
        {
            var actual=ObtenerContenedores(contenedorid, empresaid);
            if (actual == null) throw new Exception("No Existe Contenedor");
            if (actual.Estado.ToString() == "Baja") throw new Exception("El Contenedor ya esta dado de baja");
            if (_alertaBL.ListarAlertasAbiertas(empresaid).Any(a => a.ContenedorId == contenedorid)) throw new Exception("El contenedor tiene alertas abiertas , no se puede dar de baja");
            observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if(motivo==TipoEstado.MotivoBaja.Otro && observacion==null)
            {
                throw new Exception("Explique cual es la observacion");
            }
            if(observacion!=null && observacion.Length>100)
            {
                throw new Exception("");
            }
            if(!_contenedorDAL.BajaContenedor(contenedorid,empresaid,motivo,observacion,usuarioid))
            {
                throw new Exception("El contenedor no se puede dar de baja , no existe o ya fue dado de baja");
            }
        }
      
    }
}
