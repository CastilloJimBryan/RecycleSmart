using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class ResumenDashboard
    {
        public int ContenedorCriticos {  get; set; }
        public int AlertasAbiertas {  get; set; }
        public int SensoresOffline {  get; set; }
        public int AccesosHoy {  get; set; }
        public int AccesosDenegadosHoy {  get; set; }
        public decimal LitrosHoy { get; set; }


    }
}
