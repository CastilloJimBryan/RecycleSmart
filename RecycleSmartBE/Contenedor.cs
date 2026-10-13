using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class Contenedor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string TipoResiduo { get; set; } = string.Empty;
        public double CapacidadLitros {  get; set; }
        public int? Piso {  get; set; }
        public string? Sector { get; set; } = string.Empty;
        public double? PosicionX {  get; set; }
        public double? PosicionY {  get; set; }
        public TipoEstado.Estados Estado {  get; set; }
        public int InstitucionId {  get; set; }
    }
}
