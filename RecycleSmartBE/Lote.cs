using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class Lote
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public int Numero { get; set; }
        public string TipoResiduo { get; set; } = string.Empty;
        public TipoEstado.EstadoLote EstadoLote { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public decimal VolumenTotal {  get; set; }
        public int CantidadDepositada {  get; set; }
        public DateTime FechaLimiteRecoleccion {  get; set; }
        public DateTime? FechaLimiteTratamiento {  get; set; }
        public DateTime? FechaCierre {  get; set; }
        public bool? Conforme {  get; set; }
        public string? HashFinal {  get; set; }

        public int ContenedorId {  get; set; }
        public int InstitucionId {  get; set; }
        public string ContenedorNombre { get; set; } = string.Empty;
        public string InstitucionNombre { get; set; } = string.Empty;

        public List<HitoLote> Hitos { get; set; } = new List<HitoLote>();
    }
}
