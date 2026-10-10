using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class InstitucionCliente:EmpresaLogistica
    {
        public string Tipo { get; set; } = string.Empty;
        public int EmpresaLogisticaId {  get; set; }

    }
}
