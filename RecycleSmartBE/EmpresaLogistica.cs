using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class EmpresaLogistica
    {
        public int Id { get; set; }
        public string Nombre {  get;set; }= string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public DateTime FechaRegistro{  get;set; }
        public TipoEstado.Estados Estados{  get;set; }
        public int PlanId { get; set; } 
        public string PlanNombre { get; set; } = string.Empty;
    }
}
