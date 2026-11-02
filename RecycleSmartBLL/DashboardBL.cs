using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class DashboardBL
    {
        private readonly DashboardDAL _dashboarDAL;
        private readonly AlertaBL _alertaBL;

        public DashboardBL(string conec)
        {
            _dashboarDAL=new DashboardDAL(conec);         
            _alertaBL=new AlertaBL(conec);
        }
        public ResumenDashboard ObtenerResumen(int empresaid, int institucionid)
        {
            return _dashboarDAL.ObtenerResumen(empresaid, institucionid);
        }

        public List<Alerta> Atencion(int empresaid,int institucionid)
        {
            var alertas = _alertaBL.ListarAlertasAbiertas(empresaid);
            if(institucionid!=0)
            {
                alertas=alertas.Where(a=>a.InstitucionId==institucionid).ToList();
            }
            return alertas;
        }
    }
}
