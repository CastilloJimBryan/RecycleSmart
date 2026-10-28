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
            _Conectar = conectar;
        }
        public int CrearAlerta(Alerta a)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand("INSERT INTO Alerta (TipoAlerta,TipoSeveridad,EstadoAlerta,ValorDetectado,ValorUmbral,Mensaje,FechaCreacion,ResueltaAutomaticamente,ContenedorId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@TipoAlerta,@TipoSeveridad,@EstadoAlerta,@ValorDetectado,@ValorUmbral,@Mensaje,@FechaCreacion,@Resuelta,@ContenedorId ) ", con))
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
                return (int)cmd.ExecuteScalar();
            }
        }

        public Alerta? BuscarAlertaActiva(int contendorId, TipoEstado.TipoAlerta tipo)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand("SELECT Id,Mensaje,FechaCreacion,FechaActualizacion,FechaResolucion,TipoAlerta,TipoSeveridad,EstadoAlerta," +
                " ValorDetectado,ValorUmbral,NotaResolucion,ResueltaAutomaticamente,ContenedorId,UsuarioGestionId FROM Alerta " +
                " WHERE ContenedorId=@ContenedorId AND TipoAlerta=@TipoAlerta AND EstadoAlerta <> 'Resuelta' ", con))
            {
                cmd.Parameters.AddWithValue("@TipoAlerta", tipo.ToString());
                cmd.Parameters.AddWithValue("@ContenedorId", contendorId);

                con.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new Alerta
                        {
                            Id = leer.GetInt32(0),
                            Mensaje = leer.GetString(1),
                            FechaCreacion = leer.GetDateTime(2),
                            FechaActualizacion = leer.IsDBNull(3) ? null : leer.GetDateTime(3),
                            FechaResolucion = leer.IsDBNull(4) ? null : leer.GetDateTime(4),
                            TipoAlerta = Enum.Parse<TipoEstado.TipoAlerta>(leer.GetString(5), true),
                            TipoSeveridad = Enum.Parse<TipoEstado.Severidad>(leer.GetString(6), true),
                            EstadoAlerta = Enum.Parse<TipoEstado.EstadoAlerta>(leer.GetString(7), true),
                            ValorDetectado = leer.GetDecimal(8),
                            ValorUmbral = leer.GetDecimal(9),
                            NotaResolucion = leer.IsDBNull(10) ? null : leer.GetString(10),
                            ResueltaAutomaticamente = leer.GetBoolean(11),
                            ContenedorId = leer.GetInt32(12),
                            UsuarioGestionId = leer.IsDBNull(13) ? null : leer.GetInt32(13),
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

        public List<Alerta> ListarxEmpresa(int empresaid, bool resuelta)
        {
            List<Alerta> listar=new List<Alerta>();
            string Filtar = resuelta ? " AND EstadoAlerta = 'Resuelta' Order By FechaResolucion DESC " : " AND EstadoAlerta<>'Resuelta' ORDER BY CASE TipoSeveridad WHEN 'Critico' THEN 0 ELSE 1 END , FechaCreacion DESC ";
            
            using (var conec = new SqlConnection(_Conectar))
            {
                using (var cmd = new SqlCommand("SELECT TOP 100 a.*, c.Nombre AS ContenedorNombre, i.Nombre AS InstitucionNombre, i.Id AS InstitucionId, " +
        " u.Nombre + '  ' + u.Apellido AS UsuarioNombre " +
        " FROM Alerta a " +
        " INNER JOIN Contenedor c ON c.Id = a.ContenedorId " +
        " INNER JOIN InstitucionCliente i ON i.Id = c.InstitucionId " +
        " LEFT JOIN UsuarioEmpresaLogistica u ON u.Id = a.UsuarioGestionId " +
        " WHERE i.EmpresaLogisticaId = @EmpresaId" + Filtar, conec))
                {
                    cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                    conec.Open();
                    using(var leer=cmd.ExecuteReader())
                    {
                        while (leer.Read())
                        {
                            listar.Add(new Alerta
                            {
                                Id = leer.GetInt32(leer.GetOrdinal("Id")),
                                Mensaje = leer.GetString(leer.GetOrdinal("Mensaje")),
                                FechaCreacion = leer.GetDateTime(leer.GetOrdinal("FechaCreacion")),
                                FechaActualizacion = leer.IsDBNull(leer.GetOrdinal("FechaActualizacion")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaActualizacion")),
                                FechaResolucion = leer.IsDBNull(leer.GetOrdinal("FechaResolucion")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaResolucion")),
                                TipoAlerta = Enum.Parse<TipoEstado.TipoAlerta>(leer.GetString(leer.GetOrdinal("TipoAlerta")), true),
                                TipoSeveridad = Enum.Parse<TipoEstado.Severidad>(leer.GetString(leer.GetOrdinal("TipoSeveridad")), true),
                                EstadoAlerta = Enum.Parse<TipoEstado.EstadoAlerta>(leer.GetString(leer.GetOrdinal("EstadoAlerta")), true),
                                ValorDetectado = leer.GetDecimal(leer.GetOrdinal("ValorDetectado")),
                                ValorUmbral = leer.GetDecimal(leer.GetOrdinal("ValorUmbral")),
                                NotaResolucion = leer.IsDBNull(leer.GetOrdinal("NotaResolucion")) ? null : leer.GetString(leer.GetOrdinal("NotaResolucion")),
                                ResueltaAutomaticamente = leer.GetBoolean(leer.GetOrdinal("ResueltaAutomaticamente")),
                                ContenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId")),
                                ContenedorNombre = leer.GetString(leer.GetOrdinal("ContenedorNombre")),
                                InstitucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId")),
                                InstitucionNombre = leer.GetString(leer.GetOrdinal("InstitucionNombre")),
                                UsuarioGestionId = leer.IsDBNull(leer.GetOrdinal("UsuarioGestionId")) ? null : leer.GetInt32(leer.GetOrdinal("UsuarioGestionId")),
                                UsuarioGestionNombre = leer.IsDBNull(leer.GetOrdinal("UsuarioNombre")) ? null : leer.GetString(leer.GetOrdinal("UsuarioNombre"))
                            });
                        }
                    }
                    return listar;
                }
            }
        }

        public Alerta? BuscarXId(int id, int empresaid)
        {
            using (var conec = new SqlConnection(_Conectar))
            {
                using (var cmd = new SqlCommand(" SELECT Alerta.*, Contenedor.Nombre AS Contenedor,InstitucionCliente.Nombre AS Institucion,InstitucionCliente.Id AS InstitucionId,UsuarioEmpresaLogistica.Nombre+'  '+UsuarioEmpresaLogistica.Apellido AS UsuarioNombre " +
                    " FROM Alerta " +
                    " INNER JOIN Contenedor ON Contenedor.Id=Alerta.ContenedorId" +
                    " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId" +
                    " LEFT JOIN UsuarioEmpresaLogistica ON UsuarioEmpresaLogistica.Id=Alerta.UsuarioGestionId" +
                    " WHERE Alerta.Id=@Id AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId ", conec))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                    conec.Open();
                    using (var leer = cmd.ExecuteReader())
                    {
                        if (leer.Read())
                        {
                            return new Alerta
                            {
                                Id = leer.GetInt32(leer.GetOrdinal("Id")),
                                Mensaje = leer.GetString(leer.GetOrdinal("Mensaje")),
                                FechaCreacion = leer.GetDateTime(leer.GetOrdinal("FechaCreacion")),
                                FechaActualizacion = leer.IsDBNull(leer.GetOrdinal("FechaActualizacion")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaActualizacion")),
                                FechaResolucion = leer.IsDBNull(leer.GetOrdinal("FechaResolucion")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaResolucion")),
                                TipoAlerta = Enum.Parse<TipoEstado.TipoAlerta>(leer.GetString(leer.GetOrdinal("TipoAlerta")), true),
                                TipoSeveridad = Enum.Parse<TipoEstado.Severidad>(leer.GetString(leer.GetOrdinal("TipoSeveridad")), true),
                                EstadoAlerta = Enum.Parse<TipoEstado.EstadoAlerta>(leer.GetString(leer.GetOrdinal("EstadoAlerta")), true),
                                ValorDetectado = leer.GetDecimal(leer.GetOrdinal("ValorDetectado")),
                                ValorUmbral = leer.GetDecimal(leer.GetOrdinal("ValorUmbral")),
                                NotaResolucion = leer.IsDBNull(leer.GetOrdinal("NotaResolucion")) ? null : leer.GetString(leer.GetOrdinal("NotaResolucion")),
                                ResueltaAutomaticamente = leer.GetBoolean(leer.GetOrdinal("ResueltaAutomaticamente")),
                                ContenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId")),
                                ContenedorNombre = leer.GetString(leer.GetOrdinal("Contenedor")),
                                InstitucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId")),
                                InstitucionNombre = leer.GetString(leer.GetOrdinal("Institucion")),
                                UsuarioGestionId = leer.IsDBNull(leer.GetOrdinal("UsuarioGestionId")) ? null : leer.GetInt32(leer.GetOrdinal("UsuarioGestionId")),
                                UsuarioGestionNombre = leer.IsDBNull(leer.GetOrdinal("UsuarioNombre")) ? null : leer.GetString(leer.GetOrdinal("UsuarioNombre"))
                            };
                        }
                    }
                    return null;
                }
            }
        }
        
        public int Tomar(int id,int usuarioid,int empresaid)
        {

            using (var conec = new SqlConnection(_Conectar))
            {
                using (var cmd = new SqlCommand("UPDATE Alerta SET EstadoAlerta='EnGestion',UsuarioGestionId=@UsuarioId,FechaActualizacion=GETDATE() " +
                    " FROM Alerta " +
                    " INNER JOIN Contenedor ON Contenedor.Id=Alerta.ContenedorId " +
                    " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                    " WHERE Alerta.Id=@Id AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId AND Alerta.EstadoAlerta='Activa'",conec))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioid);
                    cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                    conec.Open();
                    
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Resolver(int id,int usuarioid,string nota,int empresaid)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand("UPDATE Alerta SET EstadoAlerta='Resuelta',FechaResolucion=GETDATE(),ResueltaAutomaticamente=0,UsuarioGestionId=@UsuarioId,NotaResolucion=@Nota " +
                " FROM Alerta " +
                " INNER JOIN Contenedor ON Contenedor.Id= Alerta.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE Alerta.Id=@Id AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId AND Alerta.EstadoAlerta <> 'Resuelta' ", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioid);
                cmd.Parameters.AddWithValue("@Nota", nota);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int ContarAlertasAbiertas(int empresaid)
        {
            using (var con = new SqlConnection(_Conectar))
            using (var cmd = new SqlCommand(" SELECT COUNT(*) " +
                " FROM Alerta " +
                " INNER JOIN Contenedor ON Contenedor.Id= Alerta.ContenedorId " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId " +
                " WHERE InstitucionCliente.EmpresaLogisticaId=@EmpresaId AND Alerta.EstadoAlerta <> 'Resuelta' ", con))
            {
                
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                con.Open();
                return (int) cmd.ExecuteScalar();
            }
        }
    }
}
