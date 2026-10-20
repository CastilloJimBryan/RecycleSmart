using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class AlertaDAL
    {
        private readonly string _Conectar;
        public AlertaDAL(string conectar)
        {
            _Conectar=conectar;
        }
        public int CrearAlerta(Alerta a)
        {
            using(var con=new SqlConnection(_Conectar))
            using(var cmd=new SqlCommand("INSERT INTO Alerta (TipoAlerta,TipoSeveridad,EstadoAlerta,ValorDetectado,ValorUmbral,Mensaje,FechaCreacion,ResueltaAutomaticamente,ContenedorId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@TipoAlerta,@TipoSeveridad,@EstadoAlerta,@ValorDetectado,@ValorUmbral,@Mensaje,@FechaCreacion,@Resuelta,@ContenedorId ) ",con))
            {
                cmd.Parameters.AddWithValue("@TipoAlerta", a.TipoAlerta.ToString());
                cmd.Parameters.AddWithValue("@TipoSeveridad", a.TipoSeveridad.ToString());
                cmd.Parameters.AddWithValue("@EstadoAlerta", a.EstadoAlerta.ToString());
                cmd.Parameters.AddWithValue("@ValorDetectado", a.ValorDetectado);
                cmd.Parameters.AddWithValue("@ValorUmbral", a.ValorUmbral);
                cmd.Parameters.AddWithValue("@Mensaje", a.Mensaje);
                cmd.Parameters.AddWithValue("@FechaCreacion", a.FechaCreacion);
                cmd.Parameters.AddWithValue("@Resuelta", a.ResueltaAutomaticamente);
                cmd.Parameters.AddWithValue("@ContenedorId", a.ContenedorId);
                con.Open();
                return(int) cmd.ExecuteScalar();
            }
        }

        public Alerta? BuscarAlertaActiva(int contendorId,TipoEstado.TipoAlerta tipo)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand("SELECT Id,Mensaje,FechaCreacion,FechaActualizacion,FechaResolucion,TipoAlerta,TipoSeveridad,EstadoAlerta," +
                " ValorDetectado,ValorUmbral,NotaResolucion,ResueltaAutomaticamente,ContenedorId,UsuarioGestionId FROM Alerta " +
                " WHERE ContenedorId=@ContenedorId AND TipoAlerta=@TipoAlerta AND EstadoAlerta <> 'Resuelta' ", con))
            {
                cmd.Parameters.AddWithValue("@TipoAlerta", tipo.ToString());
                cmd.Parameters.AddWithValue("@ContenedorId",contendorId);
               
                con.Open();

                using (var leer=cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new Alerta
                        {
                            Id = leer.GetInt32(0),
                            Mensaje = leer.GetString(1),
                            FechaCreacion = leer.GetDateTime(2),
                            FechaActualizacion =leer.IsDBNull(3)?null: leer.GetDateTime(3),
                            FechaResolucion = leer.IsDBNull(4) ? null : leer.GetDateTime(4),
                            TipoAlerta = Enum.Parse<TipoEstado.TipoAlerta>(leer.GetString(5),true),
                            TipoSeveridad = Enum.Parse<TipoEstado.Severidad>(leer.GetString(6),true),
                            EstadoAlerta = Enum.Parse<TipoEstado.EstadoAlerta>(leer.GetString(7), true),
                            ValorDetectado = leer.GetDecimal(8),
                            ValorUmbral = leer.GetDecimal(9),
                            NotaResolucion = leer.IsDBNull(10) ? null : leer.GetString(10),
                            ResueltaAutomaticamente = leer.GetBoolean(11),
                            ContenedorId=leer.GetInt32(12),
                            UsuarioGestionId= leer.IsDBNull(13) ? null : leer.GetInt32(13),
                        };
                    }
                }
            }
            return null;
        }

        public void EscalarAlerta(Alerta a)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand("UPDATE Alerta SET TipoSeveridad=@TipoSeveridad, ValorDetectado=@ValorDetectado,ValorUmbral=@ValorUmbral,Mensaje=@Mensaje , FechaActualizacion=@Fecha " +
                " WHERE Id = @Id ", con))
            {
                cmd.Parameters.AddWithValue("@Id", a.Id);
                cmd.Parameters.AddWithValue("@TipoSeveridad", a.TipoSeveridad.ToString());
                cmd.Parameters.AddWithValue("@ValorDetectado", a.ValorDetectado);
                cmd.Parameters.AddWithValue("@ValorUmbral", a.ValorUmbral);
                cmd.Parameters.AddWithValue("@Mensaje", a.Mensaje);
                cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                con.Open();
                cmd.ExecuteNonQuery();
            }
              
        }

        public void ResolucionAutomatica(int id)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand("UPDATE Alerta SET EstadoAlerta='Resuelta',FechaResolucion=GETDATE(),ResueltaAutomaticamente=1 " +
                " WHERE Id=@Id AND EstadoAlerta <> 'Resuelta' ", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
