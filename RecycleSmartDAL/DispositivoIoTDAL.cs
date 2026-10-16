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

        
    }
}
