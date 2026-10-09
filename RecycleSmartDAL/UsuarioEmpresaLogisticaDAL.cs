using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class UsuarioEmpresaLogisticaDAL
    {
        private readonly string _conectar;
        public UsuarioEmpresaLogisticaDAL(string conectar)
        {
            _conectar=conectar;
        }


        public UsuarioEmpresaLogistica? BuscarXCorreo(string correo)
        {
            using(var conect=new SqlConnection(_conectar))
            using(var cmd=new  SqlCommand(" SELECT Id,Nombre,Apellido,Correo,Rol,Clave,Estados,EmpresaLogisticaId" +
                " FROM UsuarioEmpresaLogistica " +
                " WHERE Correo=@Correo ",conect))
            {
                cmd.Parameters.AddWithValue("@Correo", correo);
                conect.Open();

                using (var leer=cmd.ExecuteReader())
                {
                    if(leer.Read())
                    {
                        return new UsuarioEmpresaLogistica
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Apellido = leer.GetString(2),
                            Correo = leer.GetString(3),
                            Rol = leer.GetString(4),
                            Clave = leer.GetString(5),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(6), true),
                            EmpresaLogisticaId = leer.GetInt32(7),
                        };
                    }
                }
                return null;
            }
        }
    }
}
