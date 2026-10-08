using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class UsuarioRecycleSmart
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public TipoEstado.Estados Estados { get; set; }
    }
}
