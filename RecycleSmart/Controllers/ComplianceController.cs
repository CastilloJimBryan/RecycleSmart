using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecycleSmart.Filters;
using RecycleSmart.Models;
using RecycleSmartBE;
using RecycleSmartBLL;
using Servicio;

namespace RecycleSmart.Controllers
{
    [Authorize(Roles =Roles.Control)]
    [RequiereTipoCuenta(TipoDeCuenta.EmpresaLogistica)]
    public class ComplianceController:Controller
    {
        private readonly ComplianceBL _compliancebl;
        private readonly InstitucionClienteBLL _institucionClienteBLL;
        public ComplianceController(IConfiguration config)
        {
            string conectar = config.GetConnectionString("BDTFI")!;
            _compliancebl = new ComplianceBL(conectar);
            _institucionClienteBLL=new InstitucionClienteBLL(conectar);
        }

        public IActionResult Index(int institucionid=0,int dias=30)
        {
            int empresaid = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(User);

            var instituciones = _institucionClienteBLL.ListadoInstitucioneXEmpresa(empresaid);
            if (institucionid != 0 && !instituciones.Any(i => i.Id == institucionid))
                institucionid = 0;
            if (dias is not (0 or 30 or 90 or 365))
                dias = 30;
            var abiertos = _compliancebl.ListadoLotesAbiertos(empresaid, institucionid);
            var cerrados = _compliancebl.CerradosDelPeriodo(empresaid, institucionid, dias);
            int conformes = cerrados.Count(l => l.Conforme == true);

            var vm = new ComplianceViewModel
            {
                InstitucionId = institucionid,
                Dias = dias,
                
                ListadoInstituciones = instituciones,

                LotesAbiertos = abiertos,
                EnPlazo = abiertos.Count(l => l.EstadoPlazo == TipoEstado.EstadoPlazo.EnPlazo),
                Atencion = abiertos.Count(l => l.EstadoPlazo == TipoEstado.EstadoPlazo.Atencion),
                PorVencer = abiertos.Count(l => l.EstadoPlazo == TipoEstado.EstadoPlazo.PorVencer),
                Vencido = abiertos.Count(l => l.EstadoPlazo == TipoEstado.EstadoPlazo.Vencido),

                Cerrado = cerrados.Count,
                Conforme = conformes,
                PorcentajeCumplimiento = cerrados.Count == 0 ? 0 : conformes * 100 / cerrados.Count,
                NoConforme = _compliancebl.ListadoNoConforme(empresaid, cerrados),
                InstitucionesEnRiesgo = _compliancebl.InstitucionesEnRiesgo(cerrados)
            };
            return View(vm);
        
        }
    }
}
