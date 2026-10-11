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
            _conectar = conectar;
        }

        public List<UsuarioEmpresaLogistica> ListarInstitucionXEmpresa(int empresaId)
        {
            List<UsuarioEmpresaLogistica> listar = new List<UsuarioEmpresaLogistica>();
            using (var cone = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT UsuarioEmpresaLogistica.Id,UsuarioEmpresaLogistica.Nombre,UsuarioEmpresaLogistica.Apellido,UsuarioEmpresaLogistica.Correo,UsuarioEmpresaLogistica.Rol,UsuarioEmpresaLogistica.Estado,EmpresaLogisticaId " +
                " FROM UsuarioEmpresaLogistica " +
                " INNER JOIN EmpresaLogistica ON EmpresaLogistica.Id=UsuarioEmpresaLogistica.EmpresaLogisticaId " +
                " WHERE EmpresaLogisticaId=@EmpresaId", cone))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                cone.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        listar.Add(new UsuarioEmpresaLogistica
                        {
                            Id=leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Apellido = leer.GetString(2),
                            Correo = leer.GetString(3),
                            Rol = leer.GetString(4),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(5)),
                            EmpresaLogisticaId = leer.GetInt32(6),
                        });
                    }
                }
                return listar;
            }
        }
        public UsuarioEmpresaLogistica? BuscarUsuarioEmpresaXId(int id,int empresaId)
        {
            using (var conect = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(" SELECT Id,Nombre,Apellido,Correo,Rol,Estado,EmpresaLogisticaId" +
                " FROM UsuarioEmpresaLogistica " +
                " WHERE Id=@Id AND EmpresaLogisticaId=@Empresa", conect))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Empresa",empresaId);
                conect.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new UsuarioEmpresaLogistica
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Apellido = leer.GetString(2),
                            Correo = leer.GetString(3),
                            Rol = leer.GetString(4),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(5), true),
                            EmpresaLogisticaId = leer.GetInt32(6),
                        };
                    }
                }
                return null;
            }
        }
        public UsuarioEmpresaLogistica? BuscarXCorreo(string correo)
        {
            using (var conect = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(" SELECT Id,Nombre,Apellido,Correo,Clave,Rol,Estado,EmpresaLogisticaId" +
                " FROM UsuarioEmpresaLogistica " +
                " WHERE Correo=@Correo ", conect))
            {
                cmd.Parameters.AddWithValue("@Correo", correo);
                conect.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new UsuarioEmpresaLogistica
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Apellido = leer.GetString(2),
                            Correo = leer.GetString(3),
                            Clave = leer.GetString(4),
                            Rol = leer.GetString(5),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(6), true),
                            EmpresaLogisticaId = leer.GetInt32(7),
                        };
                    }
                }
                return null;
            }
        }

        public bool ExisteCorreo(string correo, int idExcluir = 0)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM UsuarioEmpresaLogistica " +
                " WHERE Correo=@Correo AND Id <> @Id + SELECT COUNT(*) FROM UsuarioRecycleSmart " +
                " WHERE Correo=@Correo ", con))
            {
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.Parameters.AddWithValue("@Id", idExcluir);
                con.Open();

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        public int AdministradoresActivos(int empresaId, int idExcluir)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM UsuarioEmpresaLogistica " +
                " WHERE EmpresaLogistica=@EmpresaId AND Rol='Administrador' AND Estado='Activo' AND Id <> @Id", con))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                cmd.Parameters.AddWithValue("@Id", idExcluir);
                con.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public int AgregarUsuarioEmpresa(UsuarioEmpresaLogistica ue)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("INSERT INTO UsuarioEmpresaLogistica (Nombre,Apellido,Correo,Clave,Rol,Estado,EmpresaLogisticaId) " +
                " VALUES (@Nombre,@Apellido,@Correo,@Clave,@Rol,@Estado,@EmpresaLogisticaId) ", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", ue.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", ue.Apellido);
                cmd.Parameters.AddWithValue("@Correo", ue.Correo);
                cmd.Parameters.AddWithValue("@Clave", ue.Clave);
                cmd.Parameters.AddWithValue("@Rol", ue.Rol);
                cmd.Parameters.AddWithValue("@Estado", ue.Estados);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", ue.EmpresaLogisticaId);

                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public void ModificarUsuarioEmpresa(UsuarioEmpresaLogistica ue)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE UsuarioEmpresaLogistica SET Nombre=@Nombre,Apellido=@Apellido,Correo=@Correo,Rol=@Rol " +
                " WHERE Id=@Id AND EmpresaLogisticaId=@EmpresaLogisticaId", con))
            {
                cmd.Parameters.AddWithValue("@Id", ue.Id);
                cmd.Parameters.AddWithValue("@Nombre", ue.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", ue.Apellido);
                cmd.Parameters.AddWithValue("@Correo", ue.Correo);
                cmd.Parameters.AddWithValue("@Rol", ue.Rol);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", ue.EmpresaLogisticaId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void CambiarEstado(int id,int empresaid,string estado)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE UsuarioEmpresaLogistica SET Estado=@Estado " +
                " WHERE Id=@Id AND EmpresaLogisticaId=@EmpresaLogisticaId", con))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaid);
                cmd.Parameters.AddWithValue("@Estado",estado);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void CambiarClave(int id,int empresaid,string Clave)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE UsuarioEmpresaLogistica SET Clave=@Clave " +
                " WHERE Id=@Id AND EmpresaLogisticaId=@EmpresaLogisticaId", con))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaid);
                cmd.Parameters.AddWithValue("@Clave",Clave);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
