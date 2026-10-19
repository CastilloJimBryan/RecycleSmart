using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class TelemetriaDAL
    {
        private readonly string _Conectar;
        public TelemetriaDAL(string conectar)
        {
            _Conectar = conectar;            
        }

        public long RegistrarLectura(Telemetria tele)
        {
            using(var con=new SqlConnection(_Conectar))
            {
                con.Open();
                using(var tran=con.BeginTransaction())
                {
                    try
                    {
                        long id;
                        //lectura
                        using(var cmd=new SqlCommand("INSERT INTO Telemetria (Nivel,Temperatura,Humedad,Bateria,NivelConexion,FechaHora,DispositivoIoTId,ContenedorId) " +
                            " OUTPUT INSERTED.Id" +
                            " VALUES (@Nivel,@Temperatura,@Humedad,@Bateria,@NivelConexion,@FechaHora,@DispositivoIoTId,@ContenedorId) ",con,tran))
                        {
                            cmd.Parameters.AddWithValue("@Nivel", tele.Nivel);
                            cmd.Parameters.AddWithValue("@Temperatura", tele.Temperatura);
                            cmd.Parameters.AddWithValue("@Humedad", tele.Humedad);
                            cmd.Parameters.AddWithValue("@Bateria", (object?)tele.Bateria ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NivelConexion", (object?)tele.NivelConexion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@FechaHora", tele.FechaHora);
                            cmd.Parameters.AddWithValue("@DispositivoIoTId", tele.DispositivoIoTId);
                            cmd.Parameters.AddWithValue("@ContenedorId", tele.ContenedorId);
                            id=(long)cmd.ExecuteScalar();
                        }

                        using (var cmd = new SqlCommand(" UPDATE DispositivoIoT SET EstadoDispositivo = 'Activo', " +
                            " Bateria = COALESCE (@Bateria,Bateria)," +
                            " NivelConexion= COALESCE (@NivelConexion,NivelConexion)," +
                            " UltimaActualizacion=@FechaHora " +
                            " WHERE Id=@DispositivoIoTId ",con,tran))
                        {
                            cmd.Parameters.AddWithValue("@Bateria", (object?)tele.Bateria ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@NivelConexion", (object?)tele.NivelConexion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@FechaHora", tele.FechaHora);
                            cmd.Parameters.AddWithValue("@DispositivoIoTId", tele.DispositivoIoTId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return id;
                    }
                    catch 
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
