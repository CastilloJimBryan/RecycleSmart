using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using System;
using System.Collections.Generic;
using System.Text;
using static RecycleSmartBE.DiasSemana;

namespace RecycleSmartDAL
{
    public class TarjeraRFIDDAL
    {
        private readonly string _conectar;
        public TarjeraRFIDDAL(string conectar)
        {
            _conectar = conectar;
        }

        public bool ExisteUID(string uid)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT (*) FROM TarjetaRFID" +
                " WHERE UID=@UID AND Estado <> 'Baja' ", con))
            {
                cmd.Parameters.AddWithValue("@UID", uid);
                con.Open();

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
        public bool TarjetaEnUso(int usuarioinstitcionid)
        {
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM TarjetaRFID " +
                " WHERE UsuarioInstitucionId=@UsuarioInstitucionId AND Estado <> 'Baja' ", con))
            {
                cmd.Parameters.AddWithValue("@UsuarioInstitucionId", usuarioinstitcionid);
                con.Open();

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public int AgregarTarjeta(TarjetaRFID t)
        {
            using (var con = new SqlConnection(_conectar))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        int tarjetarfid;
                        using (var cmd = new SqlCommand("INSERT INTO TarjetaRFID (UID,FechaVencimiento,DiasHabilitados,HoraInicio,HoraFin," +
                            " Estado,EstadoSync,FechaRegistro,UsuarioInstitucionId) " +
                            " OUTPUT INSERTED.Id " +
                            " VALUES (@UID,@FechaVencimiento,@DiasHabilitados,@HoraInicio,@HoraFin,@TipoEstado,@EstadoSync,@FechaRegistro,@UsuarioInstitucionId) ", con, trans))
                        {
                            cmd.Parameters.AddWithValue("@UID", t.UID);
                            cmd.Parameters.AddWithValue("@FechaVencimiento", t.FechaVecimiento.Date);
                            cmd.Parameters.AddWithValue("@DiasHabilitados", (int)t.DiasHabilitados);
                            cmd.Parameters.AddWithValue("@HoraInicio", t.HoraInicio);
                            cmd.Parameters.AddWithValue("@HoraFin", t.HoraFin);
                            cmd.Parameters.AddWithValue("@TipoEstado", t.TipoEstado.ToString());
                            cmd.Parameters.AddWithValue("@EstadoSync", t.estadoSync.ToString());
                            cmd.Parameters.AddWithValue("@FechaRegistro", t.FechaRegistro);
                            cmd.Parameters.AddWithValue("@UsuarioInstitucionId", t.UsuarioInstitucionId);

                            tarjetarfid = (int)cmd.ExecuteScalar();
                        }
                        foreach (var item in t.ContenedoresId)
                        {
                            using (var cmd = new SqlCommand("INSERT INTO Contenedor_Tarjeta (TarjetaRFIDId,ContenedorId) " +
                                " VALUES (@TarjetaId,@ContenedorId)", con, trans))
                            {
                                cmd.Parameters.AddWithValue("@TarjetaId", tarjetarfid);
                                cmd.Parameters.AddWithValue("@ContenedorId", item);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        trans.Commit();
                        return tarjetarfid;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<TarjetaRFID> ListarXInstitucion(int institucionid,int empresaid)
        {
            List<TarjetaRFID> Listar=new List<TarjetaRFID>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand("SELECT TarjetaRFID.Id,UID,TarjetaRFID.FechaRegistro,DiasHabilitados,HoraInicio,HoraFin,TarjetaRFID.Estado,EstadoSync,FechaVencimiento,UsuarioInstitucionId FROM TarjetaRFID " +
                            " INNER JOIN UsuarioInstitucion ON UsuarioInstitucion.Id=TarjetaRFID.UsuarioInstitucionId" +
                            " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=UsuarioInstitucion.InstitucionId" +
                            " WHERE UsuarioInstitucion.InstitucionId=@InstitucionId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId ", con))
            {
                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                con.Open();

                using(var leer=cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        Listar.Add(new TarjetaRFID
                        {
                            Id = leer.GetInt32(0),
                            UID = leer.GetString(1),
                            FechaRegistro = leer.GetDateTime(2),
                            DiasHabilitados = (DiaSemana)leer.GetInt32(3),
                            HoraInicio = leer.GetTimeSpan(4),
                            HoraFin = leer.GetTimeSpan(5),
                            TipoEstado = Enum.Parse<TipoEstado.Estados>(leer.GetString(6)),
                            estadoSync   = Enum.Parse<TipoEstado.EstadoSync>(leer.GetString(7)),
                            FechaVecimiento=leer.GetDateTime(8),
                            UsuarioInstitucionId = leer.GetInt32(9),
                        });
                    }
                }
                return Listar;

            }
        }
    }
}
