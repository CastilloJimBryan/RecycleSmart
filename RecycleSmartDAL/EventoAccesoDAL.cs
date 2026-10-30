using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace RecycleSmartDAL
{
    public class EventoAccesoDAL
    {
        private readonly string _conectar;
        public EventoAccesoDAL(string contectar)
        {
            _conectar = contectar;
        }

        public long Registrar(EventoAcceso ea)
        {
            using (var con = new SqlConnection(_conectar))
            using(var cmd = new SqlCommand("INSERT INTO EventoAcceso (UIDLeido,ContenedorId,TarjetaRFIDId,UsuarioInstitucionId,Motivo,EstadoAcceso,NivelPrevio,Fecha) " +
                " OUTPUT INSERTED.Id" +
                " VALUES (@UID,@ContenedorId,@TarjetaRFIDId,@UsuarioInstitucionId,@Motivo,@EstadoAcceso,@NivelPrevio,@Fecha) ",con))
            {
                cmd.Parameters.AddWithValue("@UID", ea.UIDLeido);
                cmd.Parameters.AddWithValue("@ContenedorId", ea.ContenedorId);
                cmd.Parameters.AddWithValue("@TarjetaRFIDId", (object?)ea.TarjetaRfidId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioInstitucionId", (object?)ea.UsuarioInstitucionId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Motivo", ea.Motivo.ToString());
                cmd.Parameters.AddWithValue("@EstadoAcceso", ea.EstadoAcceso.ToString());
                cmd.Parameters.AddWithValue("@NivelPrevio", (object?)ea.NivelPrevio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Fecha", ea.Fecha);

                con.Open();

                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        public List<EventoAcceso> ListarXEmpresa(int empresaid,int institucionid,bool solodenegados)
        {
            var listar = new List<EventoAcceso>();

            string sql = " SELECT TOP 200 EventoAcceso.* ,Contenedor.Nombre  AS ContenedorNombre,InstitucionCliente.Nombre AS InstitucionNombre,UsuarioInstitucion.Nombre+' '+UsuarioInstitucion.Apellido AS UsuarioNombre " +
                " FROM EventoAcceso" +
                " INNER JOIN Contenedor ON Contenedor.Id=EventoAcceso.ContenedorId" +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " LEFT JOIN UsuarioInstitucion ON UsuarioInstitucion.Id=EventoAcceso.UsuarioInstitucionId " +
                " WHERE InstitucionCliente.EmpresaLogisticaId=@EmpresaId ";

            if (institucionid > 0) sql += " AND InstitucionCliente.Id= @InstitucionId ";
            if (solodenegados) sql += " AND EventoAcceso.EstadoAcceso='Denegado' ";

            sql += " ORDER BY EventoAcceso.Fecha DESC , EventoAcceso.Id DESC ";

            using(var con=new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(sql,con))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);

                con.Open();

                using ( var leer=cmd.ExecuteReader())
                {
                    while(leer.Read())
                    {
                        listar.Add(new EventoAcceso
                        {
                            Id = Convert.ToInt64(leer["Id"]),
                            UIDLeido = leer.GetString(leer.GetOrdinal("UIDLeido")),
                            EstadoAcceso = Enum.Parse<TipoEstado.EstadoAcceso>(leer.GetString(leer.GetOrdinal("EstadoAcceso")), true),
                            Motivo = Enum.Parse<TipoEstado.MotivoAcceso>(leer.GetString(leer.GetOrdinal("Motivo")), true),
                            NivelPrevio =leer.IsDBNull("NivelPrevio")?null: leer.GetInt32(leer.GetOrdinal("NivelPrevio")),
                            Fecha = leer.GetDateTime(leer.GetOrdinal("Fecha")),
                            ContenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId")),
                            TarjetaRfidId = leer.IsDBNull("TarjetaRFIDId") ? null : leer.GetInt32(leer.GetOrdinal("TarjetaRFIDId")),
                            UsuarioInstitucionId=leer.IsDBNull("UsuarioInstitucionId")?null:leer.GetInt32(leer.GetOrdinal("UsuarioInstitucionId")),
                            UsuarioNombre=leer.IsDBNull("UsuarioNombre")?null:leer.GetString(leer.GetOrdinal("UsuarioNombre")),
                            InstitucionNombre=leer.GetString(leer.GetOrdinal("InstitucionNombre")),
                            ContenedorNombre=leer.GetString(leer.GetOrdinal("ContenedorNombre")),
                            NivelPosterior=leer.IsDBNull("NivelPosterior")?null:leer.GetInt32(leer.GetOrdinal("NivelPosterior")),
                            VolumenDepositado=leer.IsDBNull("VolumenDepositado")?null:leer.GetDecimal(leer.GetOrdinal("VolumenDepositado"))
                        });
                    }
                }
                return listar;
            }
        }

        public (long Id, int NivelPrevio, int Capacidad)? BuscarPendienteDeCierre(int contenedorId, DateTime dateTime)
        {
            using (var con = new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("SELECT TOP 1 e.Id, e.NivelPrevio, c.CapacidadLitros FROM EventoAcceso e " +
                " INNER JOIN Contenedor c ON c.Id = e.ContenedorId " +
                " WHERE e.ContenedorId = @ContenedorId AND e.EstadoAcceso = 'Permitido' " +
                " AND e.NivelPosterior IS NULL AND e.NivelPrevio IS NOT NULL AND e.Fecha >= @Inicio " +
                " ORDER BY e.Fecha DESC", con))
            {
                cmd.Parameters.AddWithValue("@ContenedorId", contenedorId);
                cmd.Parameters.AddWithValue("@Inicio", dateTime);
                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    if (!leer.Read()) return null;
                    return (Convert.ToInt64(leer["Id"]), leer.GetInt32(1), leer.GetInt32(2));
                }
            }
        }

        public void CompletarCierre(long id,int nivelposterior,decimal volumen)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE EventoAcceso" +
                " SET NivelPosterior=@NivelPosterior , VolumenDepositado=@VolumenDepositado " +
                " WHERE Id=@Id", con))
            {
                cmd.Parameters.AddWithValue("@Id",id);
                cmd.Parameters.AddWithValue("@NivelPosterior",nivelposterior);
                cmd.Parameters.AddWithValue("@VolumenDepositado",volumen);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
