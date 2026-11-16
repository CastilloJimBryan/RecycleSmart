using RecycleSmartBE;

namespace RecycleSmart.Models
{
    public class ComplianceViewModel
    {
        public List<InstitucionCliente> ListadoInstituciones { get; set; } =new ();
        public List<ComplianceLote> LotesAbiertos { get; set; } =new ();
        public int InstitucionId {  get; set; }
        public int Dias {  get; set; }

        public int EnPlazo {  get; set; }
        public int Atencion {  get; set; }
        public int PorVencer {  get; set; }
        public int Vencido {  get; set; }

        public int Cerrado {  get; set; }
        public int Conforme {  get; set; }
        public int PorcentajeCumplimiento {  get; set; }
        public List<LoteNoConforme> NoConforme {  get; set; }=new ();
        public List<string> InstitucionesEnRiesgo { get; set; }=new ();
    }
}
