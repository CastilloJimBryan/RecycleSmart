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
            using (var cmd = new SqlCommand("SELECT EmpresaLogistica.Id,EmpresaLogistica.Nombre,CUIT,Direccion,FechaRegistro,EmpresaLogistica.Estado,PlanId,Planes.Nombre as PlanNombre" +
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
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(4), true),
                            PlanId = leer.GetInt32(5),
                            PlanNombre = leer.GetString(6),
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
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(4), true),
                            PlanId = leer.GetInt32(5),
                            PlanNombre = leer.GetString(6),
                        };
                    }
                }
            }
            return null;
        }

        public int AgregarEmpresaLogistica(EmpresaLogistica el)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("INSERT INTO (Nombre,CUIT,Direccion,FechaRegistro,Estado,PlanId)" +
                " OUTPUT INSERTED.Id" +
                " VALUES (@Nombre,@CUIT,@Direccion,@FechaRegistro,@Estado,@PlanId) ", con))
            {
                cmd.Parameters.AddWithValue("@Nombre", el.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", el.Cuit);
                cmd.Parameters.AddWithValue("@Direccion", el.Direccion);
                cmd.Parameters.AddWithValue("@FechaRegistro", el.FechaRegistro);
                cmd.Parameters.AddWithValue("@Estado", el.Estados);
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
                cmd.Parameters.AddWithValue("@FechaRegistro", el.FechaRegistro);
                cmd.Parameters.AddWithValue("@Estado", el.Estados);
                cmd.Parameters.AddWithValue("@PlanId", el.PlanId);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void CambiarEstado(int Id,string estado)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE EmpresaLogistica SET Estado=@Estado" +
                " WHERE Id=@Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", Id);
                cmd.Parameters.AddWithValue("@Estado", estado);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExisteCuit(string cuit,int IdExcluir=0)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT(*)" +
                " FROM EmpresaLogistica " +
                " WHERE CUIT=@CUIT AND ID <> @Id", con))
            {
                cmd.Parameters.AddWithValue("@CUIT", cuit);
                cmd.Parameters.AddWithValue("@Id", IdExcluir);
                con.Open();
                return  (int)cmd.ExecuteScalar()>0;
            }
        }
    }
}
