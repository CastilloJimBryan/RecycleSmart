using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class HitoLote
    {
        public int Id {  get; set; }
        public TipoEstado.TipoHito TipoHito { get; set; }
        public DateTime Fecha {  get; set; } 
        public string? Responsable {  get; set; }
        public string? Metodo {  get; set; }
        public string? NumeroDocumento {  get; set; } 
        public string? Observaciones {  get; set; }
        public bool? DentroDelPlazo {  get; set; }
        public string HashAnterior { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public int LoteId {  get; set; }
        public int? UsuarioId {  get; set; }
        public string? UsuarioNombre {  get; set; }
        public int? UsuarioInstitucionId {  get; set; }
    }
}
