using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class UsuarioInstitucionDAL
    {
        private readonly string _conectar;
        public UsuarioInstitucionDAL(string conectar)
        {
            _conectar=conectar;
        }

        public UsuarioInstitucion?BuscarXId(int id,int empresaid)
        {
            using(var con=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("SELECT * FROM UsuarioIntitucion " +
                " INNER JOIN  InstitucionCliente ON InstitucionCliente.Id = UsuarioInstitucion.InstitucionId" +
                " WHERE UsuarioInstitucion.Id=@Id AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId",con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();
                using(var buscar=cmd.ExecuteReader())
                {
                    if (buscar.Read())
                    {
                        return new UsuarioInstitucion
                        {
                            Id = buscar.GetInt32(0),
                            Nombre = buscar.GetString(1),
                            Apellido = buscar.GetString(2),
                            DNI = buscar.GetString(3),
                            Rol = buscar.GetString(4),
                            Sector = buscar.IsDBNull(5) ? null : buscar.GetString(5),
                            Estado = Enum.Parse<TipoEstado.Estados>(buscar.GetString(6)),
                            FechaRegistro = buscar.GetDateTime(7),
                            InstitucionId = buscar.GetInt32(8),
                        };
                    }
                }
                return null;
            }
        }
        public List<UsuarioInstitucion> ListarXInstitucion(int institucionid,int empresaid)
        {
            List<UsuarioInstitucion> Listado = new List<UsuarioInstitucion>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT * FROM UsuarioInstitucion " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=UsuarioInstitucion.InstitucionId " +
                " WHERE InstitucionCliente.Id=@InstitucionId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId " +
                " AND UsuarioInstitucion.Estado <> 'Baja' ", con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();
                using (var buscar = cmd.ExecuteReader())
                {
                    if (buscar.Read())
                    {
                        Listado.Add(new UsuarioInstitucion
                        {
                            Id = buscar.GetInt32(0),
                            Nombre = buscar.GetString(1),
                            Apellido = buscar.GetString(2),
                            DNI = buscar.GetString(3),
                            Rol=buscar.GetString(4),
                            Sector = buscar.IsDBNull(5) ? null : buscar.GetString(5),
                            Estado = Enum.Parse<TipoEstado.Estados>(buscar.GetString(6)),
                            FechaRegistro = buscar.GetDateTime(7),
                            InstitucionId= buscar.GetInt32(8),
                        }); ;
                    }
                }
                return Listado;
            }
        }

        public bool ExisteDNI(string dni,int institucionid)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM UsuarioInstitucion" +
                " WHERE DNI=@DNI AND InstitucionId=@InstitucionId AND Estado <> 'Baja' ", con))
            {
                cmd.Parameters.AddWithValue("@DNI", dni);
                cmd.Parameters.AddWithValue("@InsitucionId", institucionid);

                con.Open();
               return (int) cmd.ExecuteScalar()>0;
            }
        }

        public int AltaUsuarioInstitucion(UsuarioInstitucion ui)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("INSERT INTO UsuarioInstitucion (Nombre,Apellido,DNI,Rol,Sector,Estado,FechaRegistro,InstitucionId ) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@Nombre,@Apellido,@DNI,@Rol,@Sector,@Estado,@FechaRegistro,@InstitucionId) ", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", ui.Nombre);
                cmd.Parameters.AddWithValue("@Apellido", ui.Apellido);
                cmd.Parameters.AddWithValue("@DNI", ui.DNI);
                cmd.Parameters.AddWithValue("@Rol", ui.Rol);
                cmd.Parameters.AddWithValue("@Sector", (object?)ui.Sector ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", ui.Estado.ToString());
                cmd.Parameters.AddWithValue("@FechaRegistro", ui.FechaRegistro);
                cmd.Parameters.AddWithValue("@InstitucionId", ui.InstitucionId);

                con.Open();
                
                return (int) cmd.ExecuteScalar();
            }
        }
    }
}
