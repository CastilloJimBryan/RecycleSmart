using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class EmpresaResumen
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public TipoEstado.Estados Estado{  get; set; }
        public string NombrePlan { get; set; }=string.Empty;
        public int Instituciones {  get; set; }
        public int MaxInstituciones { get; set; }
        public int Contenedores {  get; set; }
        public int MaxContenedores { get; set; }
        public string? AdministradorNombre {  get; set; }

        public int PorcentajeContenedores => MaxContenedores == 0 ? 0 : Math.Min(100, Contenedores * 100 / MaxContenedores);
    }
}
