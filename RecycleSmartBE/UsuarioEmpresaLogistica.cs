using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RecycleSmartBE
{
    public class UsuarioEmpresaLogistica:UsuarioRecycleSmart
    {
        public int EmpresaLogisticaId { get; set;  }
        public string Rol { get;set;  }=string.Empty;
    }
}
