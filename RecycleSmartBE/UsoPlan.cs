using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class UsoPlan
    {
        //esto es de la empresa
        public string PlanNombre {  get; set; }=string.Empty;
        public int Instituciones {  get; set; }
        public int Contenedores { get; set; }
        public int MaxInstituciones { get; set; }
        public int MaxContenedores { get; set; }

    }
}
