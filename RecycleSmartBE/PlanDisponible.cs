using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class PlanDisponible
    {
        public Plan Plan { get; set; } = new();
        public bool esActual {  get; set; }
        public int InstitucionesDeMas {  get; set; }
        public int ContenedoresDeMes {  get; set; }
        public bool Compatible => InstitucionesDeMas == 0 && ContenedoresDeMes == 0;
    }
}
