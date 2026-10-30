using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace RecycleSmartDAL
{
    public class DispositivoIoTDAL
    {
        private readonly string _conectar;
        public DispositivoIoTDAL(string conectar)
        {
            _conectar = conectar;
        }

        public DispositivoIoT? BuscarXContenedor(int contenedorid,int empresaid)
        {
            using(var con=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("SELECT DispositivoIoT.Id,MacAddress,Mqqt,Bateria,NivelConexion,Firmware,FechaInstalacion,UltimaActualizacion,EstadoDispositivo,ContenedorId " +
                " FROM DispositivoIoT " +
                " INNER JOIN Contenedor ON Contenedor.Id= dispositivoIoT.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE dispositivoIoT.ContenedorId=@contenedorid " +
                " AND DispositivoIoT.EstadoDispositivo<>'Baja' AND InstitucionCliente.EmpresaLogisticaId=@empresaid ", con))
            {
                cmd.Parameters.AddWithValue("@ContenedorId", contenedorid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    if(leer.Read())
                    {
                        return new DispositivoIoT
                        {
                            Id = leer.GetInt32(0),
                            MacAdress = leer.GetString(1),
                            Mqtt = leer.GetString(2),
                            Bateria = leer.IsDBNull(3) ?null : leer.GetInt32(3),
                            NivelConexion =leer.IsDBNull(4)?null: leer.GetInt32(4),
                            Firmware =leer.IsDBNull(5)?null: leer.GetString(5),
                            FechaInstalacion = leer.IsDBNull(6) ? null : leer.GetDateTime(6),
                            UltimaActualizacion = leer.IsDBNull(7) ? null : leer.GetDateTime(7),
                            EstadoDispositivo = Enum.Parse<TipoEstado.EstadoDispositivo>(leer.GetString(8), true),
                            ContenedorId = leer.GetInt32(9),
                        };
                    }
                }
            }
            return null;
        }
        public DispositivoIoT? BuscarMAC(string mac)
        {
            using(var con=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("SELECT DispositivoIoT.Id,MacAddress,EstadoDispositivo,ContenedorId,InstitucionCliente.EmpresaLogisticaId FROM DispositivoIoT " +
                " INNER JOIN Contenedor ON Contenedor.Id=DispositivoIoT.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE MacAddress=@Mac AND EstadoDispositivo <> 'Baja' ", con))
            {
                cmd.Parameters.AddWithValue("@Mac", mac);

                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    if(leer.Read())
                    {
                        return new DispositivoIoT
                        {
                            Id = leer.GetInt32(0),
                            MacAdress = leer.GetString(1),
                            EstadoDispositivo = Enum.Parse<TipoEstado.EstadoDispositivo>(leer.GetString(2), true),
                            ContenedorId = leer.GetInt32(3),
                            EmpresaLogisticaId=leer.GetInt32(leer.GetOrdinal("EmpresaLogisticaId"))
                        };
                    }
                }
            }
            return null;
        }
        public List<DispositivoIoT> ListarDispositivoXInstitucion(int institucionId , int empresaId)
        {
            List<DispositivoIoT> listar=new List<DispositivoIoT>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT DispositivoIoT.Id,MacAddress,Mqqt,Bateria,NivelConexion,Firmware,FechaInstalacion,UltimaActualizacion,EstadoDispositivo,ContenedorId " +
                " FROM DispositivoIoT " +
                " INNER JOIN Contenedor ON Contenedor.Id= dispositivoIoT.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Contenedor.InstitucionId=@InstitucionId " +
                " AND DispositivoIoT.EstadoDispositivo<>'Baja' AND InstitucionCliente.EmpresaLogisticaId=@Empresaid ", con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);

                con.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        listar.Add (new DispositivoIoT
                        {
                            Id = leer.GetInt32(0),
                            MacAdress = leer.GetString(1),
                            Mqtt = leer.GetString(2),
                            Bateria = leer.IsDBNull(3) ? null : leer.GetInt32(3),
                            NivelConexion = leer.IsDBNull(4) ? null : leer.GetInt32(4),
                            Firmware = leer.IsDBNull(5) ? null : leer.GetString(5),
                            FechaInstalacion = leer.IsDBNull(6) ? null : leer.GetDateTime(6),
                            UltimaActualizacion = leer.IsDBNull(7) ? null : leer.GetDateTime(7),
                            EstadoDispositivo = Enum.Parse<TipoEstado.EstadoDispositivo>(leer.GetString(8), true),
                            ContenedorId = leer.GetInt32(9),
                        });
                    }
                }
            }
            return listar;
        }

        public bool ExisteMAC(string mac)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM DispositivoIoT " +
                " WHERE MacAddress=@MacAddress AND EstadoDispositivo<>'Baja'", con))
            {
                cmd.Parameters.AddWithValue("@MacAddress", mac);
                con.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public int CambiarEstado(string estado,int id,int empresaid)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE DispositivoIoT SET Estado=@Estado" +
                " INNER JOIN Contenedor ON Contenedor.Id=DispositivoIoT.ContenedorId" +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE DispositivoIoT.Id=@Id AND EmpresaLogisticaId=@Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                cmd.Parameters.AddWithValue("@Estado", estado);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }
        public int AgregarDispositivoIoT(DispositivoIoT iot)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("INSERT INTO DispositivoIoT (MacAddress,Mqqt,Firmware,FechaInstalacion,EstadoDispositivo,ContenedorId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@MacAddress,@Mqtt,@Firmware,@FechaInstalacion,@EstadoDispositivo,@ContenedorId) ", con))
            {
                cmd.Parameters.AddWithValue("@MacAddress", iot.MacAdress);
                cmd.Parameters.AddWithValue("@Mqtt", iot.Mqtt);
                cmd.Parameters.AddWithValue("@Firmware",(object?) iot.Firmware ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaInstalacion", iot.FechaInstalacion);
                cmd.Parameters.AddWithValue("@EstadoDispositivo",iot.EstadoDispositivo.ToString());
                cmd.Parameters.AddWithValue("@ContenedorId", iot.ContenedorId);
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }
        public void MarcarOffline(int dispositivoId)
        {
            using(var con = new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("UPDATE DispositivoIoT SET EstadoDispositivo = 'Offline'" +
                " WHERE Id = @Id AND EstadoDispositivo = 'Activo'", con))
            {
                cmd.Parameters.AddWithValue("@Id", dispositivoId);
                con.Open ();
                cmd.ExecuteNonQuery();
            }
        }
        public List<(int DispositivoId, int ContenedorId, int EmpresaId, bool Activo, int MinutosSinDatos, int MinutosLimite)> BuscarSinConexion()
        {
            var lista = new List<(int, int, int, bool, int, int)>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(
                "SELECT d.Id, d.ContenedorId, i.EmpresaLogisticaId, d.EstadoDispositivo, " +
                "       DATEDIFF(MINUTE, d.UltimaActualizacion, GETDATE()) AS MinutosSinDatos, " +
                "       ISNULL(u.MinutosSinConexion, 30) AS MinutosLimite " +
                " FROM DispositivoIoT d " +
                " INNER JOIN Contenedor c ON c.Id = d.ContenedorId " +
                " INNER JOIN InstitucionCliente i ON i.Id = c.InstitucionId " +
                " LEFT JOIN Umbral u ON u.ContenedorId = d.ContenedorId " +
                " WHERE d.EstadoDispositivo IN ('Activo', 'Offline') AND d.UltimaActualizacion IS NOT NULL " +
                "   AND DATEDIFF(MINUTE, d.UltimaActualizacion, GETDATE()) > ISNULL(u.MinutosSinConexion, 30)", con))
            {
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        lista.Add((
                            leer.GetInt32(0),
                            leer.GetInt32(1),
                            leer.GetInt32(2),
                            leer.GetString(3) == "Activo",
                            leer.GetInt32(4),
                            leer.GetInt32(5)));
                    }
                }
            }
            return lista;
        }
    }
}
