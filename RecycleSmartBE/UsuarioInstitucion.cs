using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class UsuarioInstitucion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido {  get; set; }=string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string ? Sector {  get; set; }
        public TipoEstado.Estados Estado {  get; set; }
        public DateTime FechaRegistro {  get; set; }
        public int InstitucionId {  get; set; }
    }
}
