using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class UmbralDAL
    {
        private readonly string _conectar;
        public UmbralDAL(string contectar)
        {
            _conectar= contectar;
        }
        public Umbral?BuscarxContenedor(int contenedorid,int empresaid)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT Umbral.Id,NivelAdvertencia,NivelCritico,TemperaturaMaxima,HumedadMaxima, " +
                " MinutoSinConexion,UltimaActualizacion,EstadoSync,ContenedorId " +
                " FROM Umbral " +
                " INNER JOIN Contenedor ON Contenedor.Id=Umbral.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Umbral.ContenedorId=@ContenedorId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId",con))
            {
                cmd.Parameters.AddWithValue("@ContenedorId", contenedorid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    if(leer.Read())
                    {
                        return new Umbral
                        {
                            Id = leer.GetInt32(0),
                            NivelAdvertencia = leer.GetInt32(1),
                            NivelCritico = leer.GetInt32(2),
                            TemperaturaMaxima = leer.GetDecimal(3),
                            HumedadMaxima = leer.GetInt32(4),
                            MinutosSinConexion = leer.GetInt32(5),
                            UltimaActualizacion = leer.GetDateTime(6),
                            EstadoSync =Enum.Parse<TipoEstado.EstadoSync>(leer.GetInt32(7).ToString()),
                            ContenedorId = leer.GetInt32(8)
                        };
                    }
                }
            }
            return null;
        }
        public int AgregarUmbral(Umbral u)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("INSERT INTO Umbral (NivelAdvertencia,NivelCritico,TemperaturaMaxima,HumedadMaxima,MinutosSinConexion,EstadoSync,ContenedorId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES () ", con))
            {
                cmd.Parameters.AddWithValue("@NivelAdvertencia", u.NivelAdvertencia);
                cmd.Parameters.AddWithValue("@NivelCritico", u.NivelCritico);
                cmd.Parameters.AddWithValue("@TemperaturaMaxima",u.TemperaturaMaxima);
                cmd.Parameters.AddWithValue("@HumedadMaxima", u.HumedadMaxima);
                cmd.Parameters.AddWithValue("@MinutosSinConexion", u.MinutosSinConexion);
                cmd.Parameters.AddWithValue("@EstadoSync", u.EstadoSync.ToString());
                cmd.Parameters.AddWithValue("@ContenedorId", u.ContenedorId);

                con.Open();
                return (int)cmd.ExecuteScalar();

            }
        }

        public int ModificarUmbral(Umbral u,int empresaid)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE SET NivelAdvertencia=@NivelAdvertencia,NivelCritico=@NivelCritico," +
                "TemperaturaMaxima=@TemperaturaMaxima,HumedadMaxima=@HumedadMaxima,MinutosSinConexion=@MinutoSinConexion, " +
                "UltimaActualizacion=@UltimaActualizacion , EstadoSync=@EstadoSync " +
                " FROM Umbral " +
                " INNER JOIN Contenedor ON Contenedor.Id=Umbral.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Umbral.ContenedorId=@ContenedorId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId", con))
            {
                cmd.Parameters.AddWithValue("@", u.NivelAdvertencia);
                cmd.Parameters.AddWithValue("@", u.NivelCritico);
                cmd.Parameters.AddWithValue("@", u.TemperaturaMaxima);
                cmd.Parameters.AddWithValue("@", u.HumedadMaxima);
                cmd.Parameters.AddWithValue("@", u.MinutosSinConexion);
                cmd.Parameters.AddWithValue("@", u.UltimaActualizacion);
                cmd.Parameters.AddWithValue("@", u.EstadoSync);
                cmd.Parameters.AddWithValue("@ContenedorId", u.ContenedorId);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public List<Umbral> ContenedorConUmbral(int institucionid,int empresaid)
        {
            List<Umbral> Listado=new List<Umbral>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT Umbral.Id,NivelAdvertencia,NivelCritico,TemperaturaMaxima,HumedadMaxima, " +
                " MinutoSinConexion,UltimaActualizacion,EstadoSync,ContenedorId " +
                " FROM Umbral " +
                " INNER JOIN Contenedor ON Contenedor.Id=Umbral.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Contenedor.InstitucionId=@InstitucionId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId", con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                con.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        Listado.Add(new Umbral
                        {
                            Id = leer.GetInt32(0),
                            NivelAdvertencia = leer.GetInt32(1),
                            NivelCritico = leer.GetInt32(2),
                            TemperaturaMaxima = leer.GetDecimal(3),
                            HumedadMaxima = leer.GetInt32(4),
                            MinutosSinConexion = leer.GetInt32(5),
                            UltimaActualizacion = leer.GetDateTime(6),
                            EstadoSync = Enum.Parse<TipoEstado.EstadoSync>(leer.GetInt32(7).ToString()),
                            ContenedorId = leer.GetInt32(8)
                        });
              
                    }
                }
            }
            return Listado;
        }
    }
}
