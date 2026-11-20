using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBE
{
    public class AuditoriaEmpresa
    {
        public int Id {  get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public string Nota { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int UsuarioRecycleSmartId {  get; set; }
        public string UsuarioNombre {  get; set; }= string.Empty;
        public int EmpresaLogisticaId {  get; set; }

    }
}
