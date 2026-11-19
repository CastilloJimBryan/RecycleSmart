using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class DispositivoConProblemas
    {
        public int Id {  get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public string ContenedorNombre {  get; set; }= string.Empty;
        public string InstitucionNombre { set; get; } = string.Empty;
        public DateTime? UltimaLectura {  get; set; }
    }
}
