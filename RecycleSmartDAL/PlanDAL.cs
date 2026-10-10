using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class PlanDAL
    {
        private readonly string _conectar;
        public PlanDAL(string conectar)
        {
            _conectar = conectar;
        }

        public List<Plan> ListarPlanes()
        {
            List<Plan> listar = new List<Plan>();
            using(var con=new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT Id,Nombre,MaxInstituciones,MaxContenedores,PrecioMensual,CostoInstalacion,Estado " +
                " FROM Planes " +
                " WHERE Estado='Activo'",con))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listar.Add(new Plan
                        {
                            Id=reader.GetInt32(0),
                            Nombre=reader.GetString(1),
                            MaxInstituciones=reader.GetInt32(2),
                            MaxContenedores=reader.GetInt32(3),
                            PrecioMensual=reader.GetDecimal(4),
                            CostoInstalacion=reader.GetDecimal(5),
                            Estado=Enum.Parse<TipoEstado.Estados>(reader.GetString(6)),
                        });
                    }
                }
            }
            return listar;
        }
    }
}
