using System;
using System.Collections.Generic;
using System.Text;
using static RecycleSmartBE.TipoEstado;

namespace RecycleSmartBE
{
    public class Factura
    {
        public int Id {  get; set; }
        public const int DiasdeMora = 15;
        public string EmpresaNombre { get; set; } = string.Empty;
        public string Numero {  get; set; }= string.Empty;
        public int Anio {  get; set; }
        public int Mes {  get; set; }
        public decimal Monto {  get; set; }
        public DateTime FechaEmision {  get; set; }
        public DateTime FechaVencimiento {  get; set; }
        public DateTime? FechaPago { get; set; }
        public TipoEstado.EstadoFactura EstadoFactura { get; set; }


        public string Periodo => $"{Mes:D2}/{Anio}";
        public bool Vencida => EstadoFactura == TipoEstado.EstadoFactura.Pendiente && DateTime.Now > FechaVencimiento;
        public int DiasVencida => Vencida ? (int)(DateTime.Now - FechaVencimiento).TotalDays : 0;
        public bool MoraGrave => DiasVencida > DiasdeMora;
    }
}
