using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class SuscripcionesBL
    {
        private readonly PlanDAL _plandal;
        private readonly EmpresaLogisticaBLL _empresaLogisticaBLL;
        private readonly EmpresaLogisticaDAL _empresalogisticadal;
        private readonly InstitucionClienteBLL _institucionClienteBLL;
        private readonly ContenedorBL _contenedorbl;


        public SuscripcionesBL(string conectar)
        {
            _empresaLogisticaBLL = new EmpresaLogisticaBLL(conectar);
            _plandal = new PlanDAL(conectar);
            _empresalogisticadal = new EmpresaLogisticaDAL(conectar);
            _institucionClienteBLL = new InstitucionClienteBLL(conectar);
            _contenedorbl = new ContenedorBL(conectar);
        }

        public (int instituciones, int contenedores) UsoActual(int empresaId)
        {
            var (instituciones, _) = _institucionClienteBLL.UsoDelPlan(empresaId);
            var (contenedores, _) = _contenedorbl.UsoxPlan(empresaId);
            return (instituciones, contenedores);
        }

        public List<PlanDisponible>PlanesDisponibles(int empresaId)
        {
            var empresa = _empresaLogisticaBLL.BuscarXId(empresaId) ?? throw new Exception("Empresa no Existente");
            var (instituciones, contenedores) = UsoActual(empresaId);

            return _plandal.ListarPlanes().Select(p => new PlanDisponible
            {
                Plan = p,
                esActual=p.Id==empresa.PlanId,
                InstitucionesDeMas=p.MaxInstituciones>0&&instituciones>p.MaxInstituciones?instituciones-p.MaxInstituciones:0,
                ContenedoresDeMes=p.MaxContenedores>0&&contenedores>p.MaxContenedores?instituciones-p.MaxContenedores:0
            }).ToList();
        }

        public void CambiarPlan(int empresaId,int planid)
        {
            var empresa = _empresaLogisticaBLL.BuscarXId(empresaId) ?? throw new Exception("La Empresa no existe");
            if(empresa.Estados!=TipoEstado.Estados.Activo)
            {
                throw new Exception("Solo una Empresa Activa puede realizar el Cambio");
            }
            var opcion = PlanesDisponibles(empresaId).FirstOrDefault(p => p.Plan.Id == planid) ?? throw new Exception("El Plan no esta disponible");

            if(opcion.esActual)
            {
                throw new Exception("Ya tiene este plan");
            }
            if(!opcion.Compatible)
            {
                var partes = new List<string>();
                if (opcion.InstitucionesDeMas > 0) partes.Add($"{opcion.InstitucionesDeMas} Instituciones");
                if (opcion.InstitucionesDeMas > 0) partes.Add($"{opcion.ContenedoresDeMes} Contenedores");
                throw new Exception($"El uso actual supera el plan {opcion.Plan.Nombre}. Para Elegirlo,tiene que dar de baja {string.Join("y",partes)}.");
            }

            _empresalogisticadal.CambiarPlan(empresaId, planid);
        }
    }
}
