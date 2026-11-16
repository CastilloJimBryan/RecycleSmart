using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class PanelCentralDAL
    {
        private readonly string _conectar;
        public PanelCentralDAL(string conectar)
        {
            _conectar = conectar;
        }

        public int ContarAlertasCriticasAbiertas()
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Alerta WHERE TipoSeveridad = 'Critico' AND EstadoAlerta <> 'Resuelta'", con))
            {
                con.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        public List<EmpresaResumen>ListarEmpresa()
        {
            var lista=new List<EmpresaResumen>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(" SELECT e.Id, e.Nombre, e.Estado, p.Nombre AS PlanNombre, p.MaxInstituciones, p.MaxContenedores, " +
                "(SELECT COUNT(*) FROM InstitucionCliente i WHERE i.EmpresaLogisticaId = e.Id AND i.Estado <> 'Baja') AS Instituciones," +
                " (SELECT COUNT(*) FROM Contenedor c INNER JOIN InstitucionCliente i ON i.Id = c.InstitucionId WHERE i.EmpresaLogisticaId = e.Id AND c.Estado <> 'Baja') AS Contenedores, " +
                " (SELECT TOP 1 u.Apellido + ', ' + u.Nombre FROM UsuarioEmpresaLogistica u WHERE u.EmpresaLogisticaId = e.Id AND u.Rol = 'Administrador' AND u.Estado = 'Activo' ORDER BY u.Id) AS AdministradorNombre " +
                " FROM EmpresaLogistica e INNER JOIN Planes p ON p.Id = e.PlanId WHERE e.Estado <> 'Baja' ORDER BY e.Nombre ", con))
            {
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        lista.Add(new EmpresaResumen
                        {
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            Nombre = leer.GetString(leer.GetOrdinal("Nombre")),
                            Estado = Enum.Parse<TipoEstado.Estados>(leer.GetString(leer.GetOrdinal("Estado")), true),
                            NombrePlan = leer.GetString(leer.GetOrdinal("PlanNombre")),
                            MaxInstituciones = leer.GetInt32(leer.GetOrdinal("MaxInstituciones")),
                            MaxContenedores = leer.GetInt32(leer.GetOrdinal("MaxContenedores")),
                            Instituciones = leer.GetInt32(leer.GetOrdinal("Instituciones")),
                            Contenedores = leer.GetInt32(leer.GetOrdinal("Contenedores")),
                            AdministradorNombre = leer.IsDBNull(leer.GetOrdinal("AdministradorNombre")) ? null : leer.GetString(leer.GetOrdinal("AdministradorNombre"))
                        });
                    }
                }
            }
            return lista;
        }
    }
}
