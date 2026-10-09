using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RecycleSmartBE
{
    public class UsuarioEmpresaLogistica:UsuarioRecycleSmart
    {
        public string Rol { get;set;  }=string.Empty;
        public int EmpresaLogisticaId { get; set;  }
    }
}
