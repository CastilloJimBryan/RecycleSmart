using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class DispositivoIoT
    {
        public int Id { get; set; }
        public string MacAdress { get; set; } = string.Empty;
        public string Mqtt {  get; set; }=string.Empty;
        public int? Bateria {  get; set; }
        public int? NivelConexion {  get; set; }
        public string? Firmware {  get; set; }
        public DateTime? FechaInstalacion {  get; set; }
        public DateTime? UltimaActualizacion {  get; set; }
        public TipoEstado.EstadoDispositivo EstadoDispositivo { get; set; }
        public int ContenedorId {  get; set; }
        public int EmpresaLogisticaId { get; set; }
    }
}
