using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
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
    }
}
