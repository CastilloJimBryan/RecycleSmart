using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartDAL
{
    public class InstitucionClienteDAL
    {
        private readonly string _conectar;
        public InstitucionClienteDAL(string conectar)
        {
            _conectar=conectar;
        }

        public List<InstitucionCliente> ListarInstucionXEmpresa(int empresaId)
        {
            List<InstitucionCliente> Listar=new List<InstitucionCliente>();
            using(var con=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("SELECT Id,Nombre,CUIT,Tipo,Direccion,Estado,EmpresaLogisticaId" +
                " FROM InstitucionCliente " +
                " WHERE EmpresaLogistica=@EmpresaLogistica " +
                " ORDER BY Nombre",con))
            {
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaId);
                con.Open();
                using(var leer=cmd.ExecuteReader())
                {
                    while(leer.Read())
                    {
                        Listar.Add(new InstitucionCliente
                        {
                            Id=leer.GetInt32(0),
                            Nombre=leer.GetString(1),
                            Cuit=leer.GetString(2),
                            Tipo=leer.GetString(3),
                            Direccion=leer.GetString(4),
                            Estados=Enum.Parse<TipoEstado.Estados>(leer.GetString(5)),
                            EmpresaLogisticaId=leer.GetInt32(6),
                        });
                    }
                }
            }
            return Listar;
        }

        public InstitucionCliente?BuscarXId(int institucionId,int empresaId)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT Id,Nombre,CUIT,Tipo,Direccion,Estado,EmpresaLogisticaId" +
                " FROM Institucion " +
                " WHERE Id=@InstitucionId AND EmpresaLogisticaId=@EmpresaLogisticaId ", con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaId);

                con.Open();

                using(var leer= cmd.ExecuteReader())
                {
                    if(leer.Read())
                    {
                        return new InstitucionCliente
                        {
                            Id= leer.GetInt32(0),
                            Nombre= leer.GetString(1),
                            Cuit= leer.GetString(2),
                            Tipo= leer.GetString(3),
                            Direccion= leer.GetString(4),
                            Estados = Enum.Parse<TipoEstado.Estados>(leer.GetString(5)),
                            EmpresaLogisticaId = leer.GetInt32(6)
                        };
                    }
                }
            }
            return null;
        }
    }
}
