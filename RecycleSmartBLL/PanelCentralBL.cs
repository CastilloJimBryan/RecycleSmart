using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class PanelCentralBL
    {
        readonly PanelCentralDAL _pandelcentralbl;
        public PanelCentralBL(string conectar)
        {
            _pandelcentralbl = new PanelCentralDAL(conectar);
        }
        public int AlertasCriticasActivas()
        {
            return _pandelcentralbl.ContarAlertasCriticasAbiertas();
        }
        public static List<EmpresaResumen> EmpresasCercaLimite(List<EmpresaResumen> listado)
        {
            return listado.Where(e => e.Estado == TipoEstado.Estados.Activo && (e.PorcentajeContenedores >= 80 || (e.MaxInstituciones > 0 && e.Instituciones * 100 >= e.MaxInstituciones * 80))).ToList();
        }
        public  List<EmpresaResumen> ListarEmpresa()
        {
            return _pandelcentralbl.ListarEmpresa();
        }
    }
}
