using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;
using static RecycleSmartBE.TipoEstado;

namespace RecycleSmartDAL
{
    public class EmpresaLogisticaDAL
    {

        private readonly string _conectar;
        public EmpresaLogisticaDAL(string conectar)
        {
            _conectar = conectar;
        }

        public List<EmpresaLogistica> Listar()
        {
            List<EmpresaLogistica> listado = new List<EmpresaLogistica>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT EmpresaLogistica.Id,EmpresaLogistica.Nombre,CUIT,Direccion,FechaRegistro,EmpresaLogistica.Estado,PlanId,Planes.Nombre as PlanNombre ," +
                " (SELECT TOP 1 u.Nombre + ', ' + u.Apellido FROM UsuarioEmpresaLogistica u WHERE u.EmpresaLogisticaId = EmpresaLogistica.Id AND u.Rol = 'Administrador' AND u.Estado = 'Activo' ORDER BY u.Id) AS AdministradorNombre " +
                " FROM EmpresaLogistica " +
                " INNER JOIN Planes ON Planes.Id=Empresalogistica.PlanId ", con))
            {
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        listado.Add(new EmpresaLogistica
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Cuit = leer.GetString(2),
                            Direccion = leer.GetString(3),
                            FechaRegistro = leer.GetDateTime(4),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(5), true),
                            PlanId = leer.GetInt32(6),
                            PlanNombre = leer.GetString(7),
                            AdminNombre= leer.IsDBNull(leer.GetOrdinal("AdministradorNombre"))? null: leer.GetString(leer.GetOrdinal("AdministradorNombre")),
                        });
                    }
                }
            }
            return listado;
        }

        public EmpresaLogistica? BuscarXId(int Id)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT EmpresaLogistica.Id,EmpresaLogistica.Nombre,CUIT,Direccion,FechaRegistro,EmpresaLogistica.Estado,PlanId,Planes.Nombre as PlanNombre" +
                " FROM EmpresaLogistica " +
                " INNER JOIN Planes ON Planes.Id=Empresalogistica.PlanId " +
                " WHERE EmpresaLogistica.Id=@Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", Id);
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        return new EmpresaLogistica
                        {
                            Id = leer.GetInt32(0),
                            Nombre = leer.GetString(1),
                            Cuit = leer.GetString(2),
                            Direccion = leer.GetString(3),
                            FechaRegistro = leer.GetDateTime(4),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(5), true),
                            PlanId = leer.GetInt32(6),
                            PlanNombre = leer.GetString(7),
                        };
                    }
                }
            }
            return null;
        }

        public int AgregarEmpresaLogistica(EmpresaLogistica el)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("INSERT INTO EmpresaLogistica (Nombre,CUIT,Direccion,FechaRegistro,Estado,PlanId)" +
                " OUTPUT INSERTED.Id " +
                " VALUES (@Nombre,@CUIT,@Direccion,@FechaRegistro,@Estado,@PlanId) ", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", el.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", el.Cuit);
                cmd.Parameters.AddWithValue("@Direccion", el.Direccion);
                cmd.Parameters.AddWithValue("@FechaRegistro", el.FechaRegistro);
                cmd.Parameters.AddWithValue("@Estado", el.Estados.ToString());
                cmd.Parameters.AddWithValue("@PlanId", el.PlanId);
                con.Open();
                
                return (int)cmd.ExecuteScalar();
            }
        }

        public void ModificarEmpresaLogistica(EmpresaLogistica el)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE EmpresaLogistica SET Nombre=@Nombre,CUIT=@CUIT,Direccion=@Direccion,PlanId=@PlanId" +
                " WHERE Id=@Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", el.Id);
                cmd.Parameters.AddWithValue("@Nombre", el.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", el.Cuit);
                cmd.Parameters.AddWithValue("@Direccion", el.Direccion);
                cmd.Parameters.AddWithValue("@PlanId", el.PlanId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public void CambiarEstadoConAuditoria(int empresaId,TipoEstado.Estados nuevo,string accion,string nota,int usuarioId)
        {
            using(var cone=new SqlConnection(_conectar))
            {
                cone.Open();
                using(var trans = cone.BeginTransaction())
                {
                    try
                    {
                        string anterior;
                        using (var cmd = new SqlCommand("SELECT Estado FROM EmpresaLogistica WITH (UPDLOCK) WHERE Id=@EmpresaId", cone, trans))
                        {
                            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                            anterior = cmd.ExecuteScalar() as string ?? throw new Exception("Empresa No Existe");
                        }
                        using(var cmd=new SqlCommand("UPDATE EmpresaLogistica SET Estado=@Estado" +
                            " WHERE Id=@EmpresaId ", cone, trans))
                        {
                            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                            cmd.Parameters.AddWithValue("@Estado", nuevo.ToString());
                            cmd.ExecuteNonQuery();
                        }
                        using(var cmd=new SqlCommand(@"INSERT INTO AuditoriaEmpresa (EstadoAnterior,Accion,EstadoNuevo,Nota,EmpresaLogisticaId,UsuarioRecycleSmartId,Fecha) " +
                            " VALUES (@EstadoAnterior,@Accion,@EstadoNuevo,@Nota,@EmpresaId,@UsuarioId,@Fecha)",cone, trans))
                        {
                            cmd.Parameters.AddWithValue("@EstadoAnterior", anterior);
                            cmd.Parameters.AddWithValue("@Accion", accion);
                            cmd.Parameters.AddWithValue("@EstadoNuevo", nuevo.ToString());
                            cmd.Parameters.AddWithValue("@Nota", nota);
                            cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                            cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                            cmd.Parameters.AddWithValue("@Fecha", DateTime.Now);
                            cmd.ExecuteNonQuery();

                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<AuditoriaEmpresa> ListarAuditoriaEmpresa(int empresaId)
        {
            List<AuditoriaEmpresa> listado = new List<AuditoriaEmpresa>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(" SELECT AuditoriaEmpresa.Id,AuditoriaEmpresa.EstadoNuevo,AuditoriaEmpresa.EstadoAnterior,AuditoriaEmpresa.Accion,AuditoriaEmpresa.Nota, " +
                " AuditoriaEmpresa.Fecha, UsuarioRecycleSmart.Nombre + ' ' + UsuarioRecycleSmart.Apellido AS UsuarioNombre " +
                " FROM AuditoriaEmpresa" +
                " INNER JOIN UsuarioRecycleSmart ON UsuarioRecycleSmart.Id=AuditoriaEmpresa.UsuarioRecycleSmartId" +
                " WHERE AuditoriaEmpresa.EmpresaLogisticaId=@EmpresaId  ORDER BY AuditoriaEmpresa.Fecha DESC ", con))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        listado.Add(new AuditoriaEmpresa
                        {
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            EstadoAnterior = leer.GetString(leer.GetOrdinal("EstadoAnterior")),
                            Accion= leer.GetString(leer.GetOrdinal("Accion")),
                            EstadoNuevo = leer.GetString(leer.GetOrdinal("EstadoNuevo")),
                            Nota = leer.GetString(leer.GetOrdinal("Nota")),
                            Fecha = leer.GetDateTime(leer.GetOrdinal("Fecha")),
                            EmpresaLogisticaId = empresaId,
                            UsuarioNombre = leer.GetString(leer.GetOrdinal("UsuarioNombre")),
                        });
                    }
                }
            }
            return listado;
        }

        public bool ExisteCuit(string cuit,int IdExcluir=0)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT(*)" +
                " FROM EmpresaLogistica " +
                " WHERE CUIT=@CUIT AND Id <> @Id", con))
            {
                cmd.Parameters.AddWithValue("@CUIT", cuit);
                cmd.Parameters.AddWithValue("@Id", IdExcluir);
                con.Open();
                return  (int)cmd.ExecuteScalar()>0;
            }
        }
    }
}
