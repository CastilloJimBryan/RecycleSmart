using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class DashboardDAL
    {
        private readonly string _conectar;
        public DashboardDAL(string conectar)
        {
            _conectar = conectar;
        }
        public ResumenDashboard ObtenerResumen(int empresaid, int institucionid)
        { 
            var r=new ResumenDashboard();
            using(var conec=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand(@" WITH CONT AS (SELECT Contenedor.Id " +
                " FROM Contenedor" +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId" +
                " WHERE InstitucionCliente.EmpresaLogisticaId=@EmpresaId AND (@InstitucionId=0 OR InstitucionCliente.Id=@InstitucionId)) " +
                " SELECT (SELECT COUNT (DISTINCT Alerta.ContenedorId) FROM Alerta " +
                " WHERE Alerta.ContenedorId IN (SELECT Id FROM CONT) " +
                " AND Alerta.EstadoAlerta<>'Resuelta' AND Alerta.TipoSeveridad='Critico') AS ContenedoresCriticos, " +
                " (SELECT COUNT (*) FROM Alerta WHERE Alerta.ContenedorId IN (SELECT Id FROM CONT) " +
                " AND Alerta.EstadoAlerta<>'Resuelta') AS AlertasAbiertas," +
                " (SELECT COUNT (*) FROM DispositivoIoT WHERE DispositivoIoT.ContenedorId IN (SELECT Id FROM CONT)" +
                " AND DispositivoIoT.EstadoDispositivo='Offline') AS SensoresOffline," +
                " (SELECT COUNT (*) FROM EventoAcceso WHERE EventoAcceso.ContenedorId IN (SELECT Id FROM CONT)" +
                " AND EventoAcceso.Fecha>=CAST(GETDATE() AS DATE )) AS AccesoHoy," +
                " (SELECT COUNT (*) FROM EventoAcceso WHERE EventoAcceso.ContenedorId IN (SELECT Id FROM CONT)" +
                " AND EventoAcceso.Fecha>=CAST(GETDATE() AS DATE )" +
                " AND EventoAcceso.EstadoAcceso='Denegado') AS AccesoDenegadoHoy," +
                " (SELECT ISNULL(SUM(EventoAcceso.VolumenDepositado),0) FROM EventoAcceso" +
                " WHERE EventoAcceso.ContenedorId IN (SELECT Id FROM CONT)" +
                " AND EventoAcceso.Fecha>=CAST(GETDATE() AS DATE )) AS LitrosHoy ", conec))
            {


                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);

                conec.Open();
                using(var leer=cmd.ExecuteReader())
                {
                    if (leer.Read())
                    {
                        r.ContenedorCriticos = leer.GetInt32(leer.GetOrdinal("ContenedoresCriticos"));
                        r.AlertasAbiertas = leer.GetInt32(leer.GetOrdinal("AlertasAbiertas"));
                        r.SensoresOffline = leer.GetInt32(leer.GetOrdinal("SensoresOffline"));
                        r.AccesosHoy = leer.GetInt32(leer.GetOrdinal("AccesoHoy"));
                        r.AccesosDenegadosHoy = leer.GetInt32(leer.GetOrdinal("AccesoDenegadoHoy"));
                        r.LitrosHoy = leer.GetDecimal(leer.GetOrdinal("LitrosHoy"));
                    }
                }
            }
            return r;
        }
    }
}
