using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class EventoLineaTiempo
    {
        public string Tipo { get; set; } = string.Empty;
        public string Contenedor {  get; set; }=string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha {  get; set; }
    }
}
