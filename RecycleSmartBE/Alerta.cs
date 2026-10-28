using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class Alerta
    {
        public int Id {  get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion {  get; set; }
        public DateTime? FechaResolucion { get; set; }
        public TipoEstado.TipoAlerta TipoAlerta {  get; set; }
        public TipoEstado.Severidad TipoSeveridad {  get; set; }
        public TipoEstado.EstadoAlerta EstadoAlerta {  get; set; }
        public decimal ValorDetectado {  get; set; }
        public decimal ValorUmbral {  get; set; }
        public string? NotaResolucion { get; set; }
        public bool ResueltaAutomaticamente {  get; set; }
        public int ContenedorId {  get; set; }
        public int InstitucionId {  get; set; }
        public int? UsuarioGestionId {  get; set; }

        public TimeSpan Duracion => (FechaResolucion ?? DateTime.Now) - FechaCreacion;

        public string ContenedorNombre {  get; set; }=string.Empty;
        public string InstitucionNombre { get; set; }=string.Empty;
        public string? UsuarioGestionNombre { get; set; }=string.Empty;

    }
}
