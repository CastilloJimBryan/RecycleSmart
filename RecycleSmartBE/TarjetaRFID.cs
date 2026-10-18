using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class TarjetaRFID
    {
        public int Id {  get; set; }
        public string UID { get; set; } = string.Empty;
        public DateTime FechaVecimiento { get; set; }
        public DiasSemana.DiaSemana DiasHabilitados {  get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin {  get; set; }
        public TipoEstado.Estados TipoEstado { get; set; }
        public TipoEstado.EstadoSync estadoSync { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int UsuarioInstitucionId {  get; set; }

        public List<int> ContenedoresId { get; set; } = new();

    }
}
