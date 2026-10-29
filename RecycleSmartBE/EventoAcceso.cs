using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class EventoAcceso
    {
        public long Id { get; set; }
        public int ContenedorId { get; set; }
        public int? TarjetaRfidId { get; set; }
        public int? UsuarioInstitucionId { get; set; }
        public string UIDLeido { get; set; } = "";
        public TipoEstado.MotivoAcceso Motivo { get; set; }
        public TipoEstado.EstadoAcceso EstadoAcceso { get; set; }
        public int? NivelPrevio { get; set; }
        public DateTime Fecha { get; set; }

        public string ContenedorNombre { get; set; } = "";
        public string InstitucionNombre { get; set; } = "";
        public string? UsuarioNombre { get; set; } = "";
        public int EmpresaLogisticaId { get; set; }

        public bool Permitido => EstadoAcceso == TipoEstado.EstadoAcceso.Permitido;
    }
}
