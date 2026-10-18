using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class DiasSemana
    {
        [Flags]
        public enum DiaSemana
        {
            Ninguno=0,
            Lunes=1,
            Martes=2,
            Miercoles=4,
            Jueves=8,
            Viernes=16,
            Sabado=32,
            Domingo=64,
        }
    }
}
