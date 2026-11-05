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
        public int CapacidadLitros {  get; set; }
        public int? Piso {  get; set; }
        public string? Sector { get; set; } = string.Empty;
        public double? PosicionX {  get; set; }
        public double? PosicionY {  get; set; }
        public TipoEstado.Estados Estado {  get; set; }
        public int InstitucionId {  get; set; }

        public DateTime? FechaBaja {  get; set; }
        public TipoEstado.MotivoBaja? MotivoBaja {  get; set; }
        public string? ObservacionBaja {  get; set; }
        public int? UsuarioBajaId {  get; set; }
        public string? UsuarioBajaNombre {  get; set; }
    }
}
