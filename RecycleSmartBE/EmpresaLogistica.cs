using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class EmpresaLogistica
    {
        public string Id { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public string Nombre {  get;set; }= string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public DateTime FechaRegistro{  get;set; }
        public string PlanId { get; set; } = string.Empty;
        public string PlanNombre { get; set; } = string.Empty;
        public TipoEstado.Estados Estados{  get;set; }
    }
}
