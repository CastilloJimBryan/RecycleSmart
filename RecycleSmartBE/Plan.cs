using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class Plan
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int MaxInstituciones { get; set; }
        public int MaxContenedores { get; set; }
        public decimal PrecioMensual { get; set; }
        public decimal CostoInstalacion { get; set; }
        public TipoEstado.Estados TipoEstado {  get; set; }
    }
}
