using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RecycleSmartDAL
{
    public class ContenedorDAL
    {
        private readonly string _contectar;
        public ContenedorDAL(string conectar)
        {
            _contectar = conectar;
        }

        public List<Contenedor> ListasContenedorXInstitucion(int institucionId,int empresaId)
        {
            List<Contenedor> listar=new List<Contenedor>();
            using(var con=new SqlConnection(_contectar))
            using(var cmd=new SqlCommand("SELECT Contenedor.Id,Contenedor.Nombre,TipoResiduo,CapacidadLitro,Piso,Sector,PosicionX,PosicionY, Contenedor.Estado,InstitucionId " +
                " FROM Contenedor " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE InstitucionId = @InstitucionId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId ",con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);

                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        listar.Add(new Contenedor
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            TipoResiduo = leer.GetString(2),
                            CapacidadLitros = leer.GetFloat(3),
                            Piso = leer.IsDBNull(4) ? null : leer.GetInt32(4),
                            Sector=leer.IsDBNull(5)? null:leer.GetString(5),
                            PosicionX=leer.IsDBNull(6)?null : leer.GetInt32(6),
                            PosicionY=leer.IsDBNull(7)?null : leer.GetInt32(7),
                            Estado=Enum.Parse<TipoEstado.Estados>(leer.GetString(8)),
                            InstitucionId=leer.GetInt32(8),
                        });
                    }
                }
            }
            return listar;
        }

        public Contenedor?BuscarContenedorXId(int contenedorid,int empresaid)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("SELECT Contenedor.Id,Contenedor.Nombre,TipoResiduo,CapacidadLitro,Piso,Sector,PosicionX,PosicionY, Contenedor.Estado,InstitucionId " +
                " FROM Contenedor " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Contenedor.Id = @ContenedorId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId ", con))
            {
                cmd.Parameters.AddWithValue("@ContenedorId", contenedorid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new Contenedor
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            TipoResiduo = leer.GetString(2),
                            CapacidadLitros = leer.GetFloat(3),
                            Piso = leer.IsDBNull(4) ? null : leer.GetInt32(4),
                            Sector = leer.IsDBNull(5) ? null : leer.GetString(5),
                            PosicionX = leer.IsDBNull(6) ? null : leer.GetInt32(6),
                            PosicionY = leer.IsDBNull(7) ? null : leer.GetInt32(7),
                            Estado = Enum.Parse<TipoEstado.Estados>(leer.GetString(8)),
                            InstitucionId = leer.GetInt32(8),
                        };
                    }
                }
            }
            return null;
        }

        public bool ExisteContenedor(string nombre,int institucionId, int excluirId=0)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM Contenedor " +
                " WHERE Nombre=@Nombre AND InstitucionId=@institucionId AND Id <> @Id AND Estado <> 'Baja'", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);
                cmd.Parameters.AddWithValue("@Id", excluirId);

                con.Open();

                return (int) cmd.ExecuteScalar()>0;
               
            }
        }

        public int CantidadDeContenedoresXEmpresa(int empresaId)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM Contenedor " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId" +
                " WHERE InstitucionCliente.EmpresaLogisticaId = @EmpresaLogisticaId AND Contenedor.Estado <> 'Baja'", con))
            {
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaId);
                con.Open();

                return (int)cmd.ExecuteScalar();

            }
        }

        public int AltaContenedor(Contenedor c)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("INSERT INTO Contenedor (Nombre,TipoResiduo,CapacidadLitros,Piso,Sector,InstitucionId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@Nombre,@TipoResiduo,@CapacidadLitros,@Piso,@Sector,@InstitucionId) ", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
                cmd.Parameters.AddWithValue("@TipoResiduo", c.TipoResiduo);
                cmd.Parameters.AddWithValue("@CapacidadLitros", c.CapacidadLitros);
                cmd.Parameters.AddWithValue("@Piso",(object?)c.Piso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Sector", (object?)c.Sector ?? DBNull.Value );
                cmd.Parameters.AddWithValue("@InstitucionId", c.InstitucionId);
                con.Open();

                return (int)cmd.ExecuteScalar();

            }
        }

        public int ModificarContenedor(Contenedor c ,int empresaId)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("UPDATE SET Contenedor.Nombre=@Nombre,TipoResiduo=@TipoResiduo,CapacidadLitro=@CapacidadLitro,Piso=@Piso,Sector=@Sector" +
                " FROM Contenedor " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Contenedor.Id=@Id AND InstitucionCliente.EmpresaLogisticaId=@EmpresaLogistica ", con))
            {
                cmd.Parameters.AddWithValue("@Id", c.Id);
                cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
                cmd.Parameters.AddWithValue("@TipoResiduo", c.TipoResiduo);
                cmd.Parameters.AddWithValue("@CapacidadLitros", c.CapacidadLitros);
                cmd.Parameters.AddWithValue("@Piso", (object?)c.Piso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Sector", (object?)c.Sector ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpresaLogistica", empresaId);
                con.Open();

                return cmd.ExecuteNonQuery();

            }
        }

        public int CambiarEstado(int id,string estado,int empresaid)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("UPDATE SET Contenedor.Estado=@Estado " +
                " FROM Contenedor " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Contenedor.Id=@Id AND InstitucionCliente.EmpresaLogisticaId=@EmpresaLogistica ", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Estado",estado);
                cmd.Parameters.AddWithValue("@EmpresaLogistica", empresaid);
                con.Open();

                return cmd.ExecuteNonQuery();

            }
        }
    }
}
