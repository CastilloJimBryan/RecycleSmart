using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
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
                " WHERE EmpresaLogisticaId=@EmpresaLogisticaId " +
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
                " FROM InstitucionCliente " +
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
        public int AgregarInstitucion(InstitucionCliente Ic)
        {
            using(var con=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand("INSERT INTO InstitucionCliente (Nombre,CUIT,Tipo,Direccion,Estado,EmpresaLogisticaId) " +
                " OUTPUT INSERTED.Id " +
                " VALUES (@Nombre,@CUIT,@Tipo,@Direccion,@Estado,@EmpresaLogisticaId)",con))
            {
                cmd.Parameters.AddWithValue("@Nombre", Ic.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", Ic.Cuit);
                cmd.Parameters.AddWithValue("@Tipo", Ic.Tipo);
                cmd.Parameters.AddWithValue("@Direccion", Ic.Direccion);
                cmd.Parameters.AddWithValue("@Estado", Ic.Estados);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", Ic.EmpresaLogisticaId);

                con.Open();

                return (int)cmd.ExecuteScalar();
            }
        }

        public void ModificarInstitucion (InstitucionCliente ic)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("UPDATE InstitucionCliente SET Nombre=@Nombre, CUIT=@CUIT,Tipo=@Tipo,Direccion=@Direccion,EmpresaLogisticaId=@EmpresaLogisticaId" +
                " WHERE Id=@Id AND EmpresaLogisticaId=@EmpresaLogisticaId ", con))
            {
                cmd.Parameters.AddWithValue("@Id", ic.Id);
                cmd.Parameters.AddWithValue("@Nombre", ic.Nombre);
                cmd.Parameters.AddWithValue("@CUIT", ic.Cuit);
                cmd.Parameters.AddWithValue("@Tipo", ic.Tipo);
                cmd.Parameters.AddWithValue("@Direccion", ic.Direccion);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", ic.EmpresaLogisticaId);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
        public void CambiarEstado(string estado,int Id,int empresaId)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(" UPDATE InstitucionCliente SET Estado=@Estado " +
                " WHERE Id=@Id AND EmpresaLogisticaId=@EmpresaLogisticaId ", con))
            {
                cmd.Parameters.AddWithValue("@Id", Id);
                cmd.Parameters.AddWithValue("@Estado", estado);
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaId);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
        public bool ExisteCuit(string cuit,int idExcluir=0)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM InstitucionCliente " +
                " WHERE CUIT=@Cuit AND Id <> @Id", con))
            {
                cmd.Parameters.AddWithValue("@CUIT", cuit);
                cmd.Parameters.AddWithValue("@Id", idExcluir);
                con.Open();

                return (int)cmd.ExecuteScalar()>0;
            }
        }

        public int CantidadInstituciones (int empresaId)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM InstitucionCliente " +
                " WHERE EmpresaLogisticaId=@EmpresaLogisticaId AND EStado <> 'Baja'", con))
            {
                cmd.Parameters.AddWithValue("@EmpresaLogisticaId", empresaId);
                con.Open();

                return (int)cmd.ExecuteScalar() ;
            }
        }
    }
}
