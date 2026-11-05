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
            using(var cmd=new SqlCommand("SELECT Contenedor.Id,Contenedor.Nombre,TipoResiduo,CapacidadLitros,Piso,Sector,PosicionX,PosicionY, Contenedor.Estado,InstitucionId " +
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
                            CapacidadLitros = leer.GetInt32(3),
                            Piso = leer.IsDBNull(4) ? null : leer.GetInt32(4),
                            Sector=leer.IsDBNull(5)? null:leer.GetString(5),
                            PosicionX=leer.IsDBNull(6)?null : leer.GetDouble(6),
                            PosicionY=leer.IsDBNull(7)?null : leer.GetDouble(7),
                            Estado=Enum.Parse<TipoEstado.Estados>(leer.GetString(8)),
                            InstitucionId=leer.GetInt32(9),
                        });
                    }
                }
            }
            return listar;
        }

        public Contenedor?BuscarContenedorXId(int contenedorid,int empresaid)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("SELECT Contenedor.Id,Contenedor.Nombre,TipoResiduo,CapacidadLitros,Piso,Sector,PosicionX,PosicionY, Contenedor.Estado,InstitucionId, " +
                " Contenedor.FechaBaja,Contenedor.MotivoBaja,Contenedor.ObservacionBaja,Contenedor.UsuarioBajaId , UsuarioEmpresaLogistica.Nombre + ' ' + UsuarioEmpresaLogistica.Apellido AS UsuarioBajaNombre " +
                " FROM Contenedor " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " LEFT JOIN UsuarioEmpresaLogistica ON UsuarioEmpresaLogistica.Id=Contenedor.UsuarioBajaId " +
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
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            Nombre = leer.GetString(leer.GetOrdinal("Nombre")),
                            TipoResiduo = leer.GetString(leer.GetOrdinal("TipoResiduo")),
                            CapacidadLitros = leer.GetInt32(leer.GetOrdinal("CapacidadLitros")),
                            Piso = leer.IsDBNull(4) ? null : leer.GetInt32(leer.GetOrdinal("Piso")),
                            Sector = leer.IsDBNull(5) ? null : leer.GetString((leer.GetOrdinal("Sector"))),
                            PosicionX = leer.IsDBNull(6) ? null : leer.GetDouble(leer.GetOrdinal("PosicionX")),
                            PosicionY = leer.IsDBNull(7) ? null : leer.GetDouble(leer.GetOrdinal("PosicionY")),
                            Estado = Enum.Parse<TipoEstado.Estados>(leer.GetString(leer.GetOrdinal("Estado"))),
                            InstitucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId")),
                            FechaBaja = leer.IsDBNull(leer.GetOrdinal("FechaBaja")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaBaja")),
                            MotivoBaja = leer.IsDBNull(leer.GetOrdinal("MotivoBaja")) ? null : Enum.Parse<TipoEstado.MotivoBaja>(leer.GetString(leer.GetOrdinal("MotivoBaja"))),
                            ObservacionBaja = leer.IsDBNull(leer.GetOrdinal("ObservacionBaja")) ? null : leer.GetString(leer.GetOrdinal("ObservacionBaja")),
                            UsuarioBajaNombre = leer.IsDBNull(leer.GetOrdinal("UsuarioBajaNombre")) ? null : leer.GetString(leer.GetOrdinal("UsuarioBajaNombre"))
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
            using (var cmd = new SqlCommand("INSERT INTO Contenedor (Nombre,TipoResiduo,CapacidadLitros,Piso,Sector,Estado,InstitucionId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@Nombre,@TipoResiduo,@CapacidadLitros,@Piso,@Sector,@Estado,@InstitucionId) ", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
                cmd.Parameters.AddWithValue("@TipoResiduo", c.TipoResiduo);
                cmd.Parameters.AddWithValue("@CapacidadLitros", c.CapacidadLitros);
                cmd.Parameters.AddWithValue("@Piso",(object?)c.Piso ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Sector", (object?)c.Sector ?? DBNull.Value );
                cmd.Parameters.AddWithValue("@Estado", c.Estado.ToString()); 
                cmd.Parameters.AddWithValue("@InstitucionId", c.InstitucionId);
                con.Open();

                return (int)cmd.ExecuteScalar();

            }
        }
        public bool BajaContenedor(int contenedorid,int empresaid,TipoEstado.MotivoBaja baja,string? observacion,int usuarioid)
        {
            using (var con = new SqlConnection(_contectar))
            {
                con.Open();
                using(var tran=con.BeginTransaction())
                {
                    try
                    {
                        int filas;
                        using (var cmd = new SqlCommand(@"UPDATE Contenedor SET Contenedor.Estado='Baja',Contenedor.FechaBaja=GETDATE(),Contenedor.MotivoBaja=@Motivo,Contenedor.ObservacionBaja=@Observacion,Contenedor.UsuarioBajaId=@UsuarioId " +
                            " FROM Contenedor" +
                            " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                            " WHERE Contenedor.Id=@ContenedorId " +
                            " AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId AND Contenedor.Estado<>'Baja'", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@ContenedorId", contenedorid);
                            cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                            cmd.Parameters.AddWithValue("@Motivo", baja.ToString());
                            cmd.Parameters.AddWithValue("@Observacion", (object?)observacion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@UsuarioId", usuarioid);

                            filas = cmd.ExecuteNonQuery();
                        }
                        if (filas != 1)
                        {
                            tran.Rollback();
                            return false;
                        }
                        //Dispositivo deja de mandar señal
                        using (var cmd = new SqlCommand("UPDATE DispositivoIoT SET EstadoDispositivo='Baja' WHERE DispositivoIoT.ContenedorId=@ContenedorId", con, tran))
                        {
                            cmd.Parameters.AddWithValue("@ContenedorId", contenedorid);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand("DELETE FROM Contenedor_Tarjeta WHERE Contenedor_Tarjeta.ContenedorId=@ContenedorId", con, tran))
                        {

                            cmd.Parameters.AddWithValue("@ContenedorId", contenedorid);
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                        return true;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
        public bool ModificarContenedor(Contenedor c ,int empresaId)
        {
            using (var con = new SqlConnection(_contectar))
            using (var cmd = new SqlCommand("UPDATE Contenedor SET Contenedor.Nombre=@Nombre,TipoResiduo=@TipoResiduo,CapacidadLitros=@CapacidadLitros,Piso=@Piso,Sector=@Sector" +
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

                return cmd.ExecuteNonQuery()==1;

            }
        }
 
    }
}
