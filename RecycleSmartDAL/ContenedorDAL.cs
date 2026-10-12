using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class ContenedorDAL
    {
        private readonly string _contectar;
        public ContenedorDAL(string conectar)
        {
            _contectar = conectar;
        }

        public List<Contenedor> ListasContenedorXInstitucion(int institucionId,int empresaId)
        {
            List<Contenedor> listar=new List<Contenedor>();
            using(var con=new SqlConnection(_contectar))
            using(var cmd=new SqlCommand("",con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaId);

                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        listar.Add(new Contenedor
                        {

                        });
                    }
                }
            }
            return listar;
        }
    }
}
