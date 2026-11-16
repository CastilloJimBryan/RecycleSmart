using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class ComplianceLote
    {
        public int LoteId { get;set;  }
        public string Codigo { get; set; } = string.Empty;
        public int InstitucionId {  get;set; }
        public string InstitucionNombre {  get;set; } = string.Empty;
        public string ContenedorNombre { get; set; } = string.Empty;
        public TipoEstado.EstadoLote EstadoLote { get;set; }
        public string EtapaControlada { get; set; } = string.Empty;
        public DateTime FechaLimite {  get; set; }
        public int PorcentajePlazo {  get; set; }
        public TipoEstado.EstadoPlazo EstadoPlazo { get;set; }
    }
}
