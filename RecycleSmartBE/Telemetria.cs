using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class Telemetria
    {
        public long Id { get; set; }
        public int Nivel {  get; set; }
        public decimal Temperatura {  get; set; }
        public int Humedad {  get; set; }
        public int? Bateria {  get; set; }
        public int? NivelConexion {  get; set; }
        public DateTime FechaHora {  get; set; }
        public int DispositivoIoTId {  get; set; }
        public int ContenedorId {  get; set; }
        public TipoEstado.ClasificacionLectura Clasificacion{  get; set; }
        public int EmpresaLogisticaId {  get; set; }
       
    }
}
