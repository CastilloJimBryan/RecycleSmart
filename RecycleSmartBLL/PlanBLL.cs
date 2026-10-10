using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class PlanBLL
    {
        private readonly PlanDAL _planBL;
        public PlanBLL(string conectar)
        {
            _planBL=new PlanDAL(conectar);
        }

        public List<Plan> ListarPlan()
        {
            return _planBL.ListarPlanes();
        }
    }
}
