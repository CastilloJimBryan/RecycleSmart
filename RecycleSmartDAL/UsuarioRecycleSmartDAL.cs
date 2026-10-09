using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class UsuarioRecycleSmartDAL
    {
        private readonly string _conectar;
        public UsuarioRecycleSmartDAL(string conectar)
        {
            _conectar = conectar;
        }
        public UsuarioRecycleSmart? BuscarXCorreo(string correo)
        {
            using (var conect = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(" SELECT Id,Nombre,Apellido,Correo,Rol,Clave,Estados,EmpresaLogisticaId" +
                " FROM UsuarioRecycleSmart " +
                " WHERE Correo=@Correo ", conect))
            {
                cmd.Parameters.AddWithValue("@Correo", correo);
                conect.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new UsuarioRecycleSmart
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Apellido = leer.GetString(2),
                            Correo = leer.GetString(3),
                            Clave = leer.GetString(4),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(5), true),
                        };
                    }
                }
                return null;
            }
        }
    }
}
