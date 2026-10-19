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

        public List<Telemetria> UltimasLecturasxInstitucion(int institucionid,int empresaid)
        {
            var listar=new List<Telemetria>();
            using(var con=new SqlConnection(_Conectar))
            using(var cmd=new SqlCommand(" SELECT * FROM (SELECT Telemetria.* ,ROW_NUMBER() OVER (PARTITION BY Telemetria.ContenedorId ORDER BY Telemetria.FechaHora DESC , Telemetria.Id DESC ) AS Orden" +
                " FROM Telemetria" +
                " INNER JOIN Contenedor ON Contenedor.Id=Telemetria.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId" +
                " WHERE Contenedor.InstitucionId = @InstitucionId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId )" +
                " x WHERE Orden = 1 ",con))
            {

                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                con.Open();

                using(var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        int colorbateria = leer.GetOrdinal("Bateria");
                        int colorconexion =leer.GetOrdinal("NivelConexion");
                        listar.Add(new Telemetria
                        {
                            Id = leer.GetInt64(leer.GetOrdinal("Id")),
                            Nivel = leer.GetInt32(leer.GetOrdinal("Nivel")),
                            Temperatura = leer.GetDecimal(leer.GetOrdinal("Temperatura")),
                            Humedad = leer.GetInt32(leer.GetOrdinal("Humedad")),
                            Bateria = leer.IsDBNull(colorbateria) ? null : leer.GetInt32(colorbateria),
                            NivelConexion = leer.IsDBNull(colorconexion) ? null : leer.GetInt32(colorconexion),
                            FechaHora = leer.GetDateTime(leer.GetOrdinal("FechaHora")),
                            DispositivoIoTId = leer.GetInt32(leer.GetOrdinal("DispositivoIoTId")),
                            ContenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId")),
                        });
                    }
                }
            }
            return listar;
        }

    }
}
