using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class FacturaDAL
    {
        private readonly string _conectar;

        public FacturaDAL(string conectar)
        {
            _conectar= conectar;
        }

        public int EmitirFacturaxMes(int anio, int mes, int diasVencimiento)
        {
            using (var conect = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(@"INSERT INTO Factura (EmpresaLogisticaId, Numero, Anio, Mes, Monto, FechaEmision, FechaVencimiento, EstadoFactura) " +
                " SELECT  EmpresaLogistica.Id,CONCAT(@Anio,RIGHT ('0'+CAST(@Mes AS VARCHAR(2)),2),'-',RIGHT('0000'+CAST(EmpresaLogistica.Id AS varchar(10)),4)), " +
                "@Anio,@Mes,CAST(Planes.PrecioMensual AS DECIMAL(12,2)),GETDATE(),DATEADD(DAY,@Dias,GETDATE()),'Pendiente'" +
                "FROM EmpresaLogistica " +
                "INNER JOIN Planes ON Planes.Id=EmpresaLogistica.PlanId " +
                "WHERE EmpresaLogistica.Estado='Activo' AND NOT EXISTS (SELECT 1 FROM Factura WHERE Factura.EmpresaLogisticaId=EmpresaLogistica.Id AND Factura.Anio=@Anio AND Factura.Mes=@Mes) ", conect))
            {
                cmd.Parameters.AddWithValue("@Anio", anio);
                cmd.Parameters.AddWithValue("@Mes", mes);
                cmd.Parameters.AddWithValue("@Dias", diasVencimiento);
                conect.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int RegistrarPago(int facturaid)
        {
            using(var conect=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("UPDATE Factura SET EstadoFactura = 'Pagado',FechaPago=GETDATE() " +
                " WHERE EstadoFactura='Pendiente' AND Id=@Id ",conect))
            {
                cmd.Parameters.AddWithValue("@Id", facturaid);
                conect.Open();
               return cmd.ExecuteNonQuery() ;
            }
        }

        public List<Factura> ListarFacturaXEmpresa(int empresaId=0)
        {
            var lista= new List<Factura>();
            using(var conec=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand(@"SELECT Factura.Id,Factura.EmpresaLogisticaId,EmpresaLogistica.Nombre AS EmpresaNombre,Factura.Numero,Factura.Anio,Factura.Mes,Factura.Monto,Factura.FechaEmision, " +
                " Factura.FechaVencimiento,Factura.FechaPago,Factura.EstadoFactura " +
                " FROM Factura " +
                " INNER JOIN EmpresaLogistica ON EmpresaLogistica.Id=Factura.EmpresaLogisticaId " +
                " WHERE (@EmpresaId=0 OR Factura.EmpresaLogisticaId=@EmpresaId) ORDER BY Factura.Anio DESC, Factura.Mes DESC,EmpresaLogistica.Nombre", conec))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);
                conec.Open();
                using(var leer=cmd.ExecuteReader())
                {
                    while(leer.Read())
                    {
                        lista.Add(new Factura
                        {
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            EmpresaNombre = leer.GetString(leer.GetOrdinal("EmpresaNombre")),
                            Numero = leer.GetString(leer.GetOrdinal("Numero")),
                            Mes = leer.GetInt32(leer.GetOrdinal("Mes")),
                            Anio = leer.GetInt32(leer.GetOrdinal("Anio")),
                            Monto = leer.GetDecimal(leer.GetOrdinal("Monto")),
                            FechaEmision = leer.GetDateTime(leer.GetOrdinal("FechaEmision")),
                            FechaVencimiento = leer.GetDateTime(leer.GetOrdinal("FechaVencimiento")),
                            FechaPago =leer.IsDBNull(leer.GetOrdinal("FechaPago"))?null: leer.GetDateTime(leer.GetOrdinal("FechaPago")),
                            EstadoFactura = Enum.Parse<TipoEstado.EstadoFactura>(leer.GetString(leer.GetOrdinal("EstadoFactura")),true),

                        });
                    }

                    return lista;
                }
            }
        }
    }
}
