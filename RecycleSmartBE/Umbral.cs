using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class Umbral
    {
        public int Id { get; set; }
        public int NivelAdvertencia { get; set; }
        public int NivelCritico {  get; set; }
        public decimal TemperaturaMaxima {  get; set; }
        public int HumedadMaxima {  get; set; }
        public int MinutosSinSenal {  get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public TipoEstado.EstadoSync EstadoSync{  get; set; }
        public int ContenedorId {  get; set; }
    }
}
