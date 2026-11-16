using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class LoteNoConforme
    {
        public int LoteId {  get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string InstitucionNombre { get; set; } = string.Empty;
        public int DiasExcedidos {  get; set; }
        public string HitoVencido {  get; set; }= string.Empty;
    }
}
