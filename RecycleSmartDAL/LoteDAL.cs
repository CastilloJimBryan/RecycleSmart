using Microsoft.Data.SqlClient;
using RecycleSmartBE;
using Servicio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace RecycleSmartDAL
{
    public class LoteDAL
    {
        private readonly string _conectar;
        public LoteDAL(string conectar)
        {
            _conectar = conectar;
        }

        public (int loteId,string Codigo,bool Creado) RegistrarDeposito(long EventoAccesoId,int diasRecoleccion)
        {
            using(var con=new SqlConnection(_conectar))
            {
                con.Open();
                using(var tram=con.BeginTransaction())
                {
                    try
                    {
                        //datos de acceso y contenedor
                        int contenedorId;
                        int institucionId;
                        int? UsuarioInstitucionId;
                        DateTime Fecha;
                        int NivelPre;
                        int NivelPost;
                        decimal Volumen;
                        string TipoResiduo;

                        using (var cmd = new SqlCommand(@" SELECT EventoAcceso.ContenedorId,EventoAcceso.UsuarioInstitucionId,EventoAcceso.Fecha,EventoAcceso.NivelPrevio,EventoAcceso.NivelPosterior,EventoAcceso.VolumenDepositado," +
                            " Contenedor.TipoResiduo,Contenedor.InstitucionId" +
                            " FROM EventoAcceso" +
                            " INNER JOIN Contenedor ON Contenedor.Id=EventoAcceso.ContenedorId" +
                            " WHERE EventoAcceso.Id=@EventoAccesoId AND EventoAcceso.LoteId IS NULL ", con, tram))
                        {
                            cmd.Parameters.AddWithValue("@EventoAccesoId", EventoAccesoId);

                            using(var leer = cmd.ExecuteReader())
                            {
                                if(!leer.Read())
                                {
                                    throw new Exception($"El Evento {EventoAccesoId} no existe o ya tiene lote");
                                }
                                contenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId"));
                                institucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId"));
                                UsuarioInstitucionId =leer.IsDBNull(leer.GetOrdinal("UsuarioInstitucionId"))?null: leer.GetInt32(leer.GetOrdinal("UsuarioInstitucionId"));
                                Fecha = leer.GetDateTime(leer.GetOrdinal("Fecha"));
                                NivelPre = leer.GetInt32(leer.GetOrdinal("NivelPrevio"));
                                NivelPost = leer.GetInt32(leer.GetOrdinal("NivelPosterior"));
                                Volumen = leer.GetDecimal(leer.GetOrdinal("VolumenDepositado"));
                                TipoResiduo = leer.GetString(leer.GetOrdinal("TipoResiduo"));
                            }
                        }
                        int loteid = 0;
                        string codigo = "";
                        // busca si hay lotes activos
                        using (var cmd=new SqlCommand(@"SELECT Id ,Codigo FROM Lote WITH (UPDLOCK,HOLDLOCK) WHERE ContenedorId=@ContenedorId AND EstadoLote IN ('EnCurso','RecoleccionSolicitada')", con,tram))
                        {
                            cmd.Parameters.AddWithValue("@ContenedorId", contenedorId);
                            using (var leer=cmd.ExecuteReader())
                            {
                                if(leer.Read())
                                {
                                    loteid = leer.GetInt32(0);
                                    codigo = leer.GetString(1);
                                }
                            }
                        }

                        bool creado=false;
                        if (loteid == 0)
                        {
                            var fechacreacion = Integridad.Truncar(Fecha);
                            int anio = fechacreacion.Year;
                            int numero;
                            using (var cmd=new SqlCommand("SELECT ISNULL (MAX(Numero),0)+1 FROM Lote WITH (UPDLOCK , HOLDLOCK) WHERE InstitucionId=@InstitucionId AND Anio=@Anio", con,tram))
                            {
                                cmd.Parameters.AddWithValue("@InstitucionId",institucionId);
                                cmd.Parameters.AddWithValue("@Anio",anio);
                                numero=(int)cmd.ExecuteScalar();
                            }
                            codigo = $"RS-{institucionId}-{anio}-{numero:D4}";
                            //lote
                            using(var cmd=new SqlCommand(@"INSERT INTO Lote(Codigo,Anio,Numero,TipoResiduo,EstadoLote,FechaGeneracion,FechaLimiteRecoleccion,VolumenTotal,CantidadDeposito,ContenedorId,InstitucionId) " +
                                " OUTPUT INSERTED.Id " +
                                " VALUES(@codigo,@Anio,@Numero,@TipoResiduo,@EstadoLote,@FechaGeneracion,@FechaLimiteRecoleccion,0,0,@ContenedorId,@InstitucionId) ", con,tram))
                            {
                                cmd.Parameters.AddWithValue("@Codigo",codigo);
                                cmd.Parameters.AddWithValue("@EstadoLote",TipoEstado.EstadoLote.EnCurso.ToString());
                                cmd.Parameters.AddWithValue("@Numero",numero);
                                cmd.Parameters.AddWithValue("@TipoResiduo",TipoResiduo);
                                cmd.Parameters.AddWithValue("@Anio",anio);
                                cmd.Parameters.AddWithValue("@FechaGeneracion",fechacreacion);
                                cmd.Parameters.AddWithValue("@FechaLimiteRecoleccion",fechacreacion.AddDays(diasRecoleccion));
                                cmd.Parameters.AddWithValue("@ContenedorId",contenedorId);
                                cmd.Parameters.AddWithValue("@InstitucionId",institucionId);

                                loteid = (int)cmd.ExecuteScalar();
                            }
                            //hito
                            var hito = new HitoLote()
                            {
                                LoteId = loteid,
                                TipoHito = TipoEstado.TipoHito.Generacion,
                                Fecha = fechacreacion,
                                UsuarioInstitucionId = UsuarioInstitucionId,
                                HashAnterior = Integridad.HashInicial,
                                Observaciones = $"Primer Deposito nivel {NivelPre} % -> {NivelPost} %, {Volumen.ToString("0.0", CultureInfo.InvariantCulture)} L"
                            };
                            hito.Hash = Integridad.hashito(codigo, hito);
                            InsertarHito(hito,con,tram);

                            creado = true;
                        }

                        using (var cmd=new SqlCommand(@"UPDATE Lote SET VolumenTotal=VolumenTotal+@Volumen,CantidadDeposito=CantidadDeposito + 1 " +
                            " WHERE Id=@LoteId; " +
                            " UPDATE EventoAcceso SET LoteId=@LoteId" +
                            " WHERE EventoAcceso.Id=@EventoAccesoId ", con,tram))
                        {
                            cmd.Parameters.AddWithValue("@Volumen", Volumen);
                            cmd.Parameters.AddWithValue("@LoteId", loteid);
                            cmd.Parameters.AddWithValue("@EventoAccesoId", EventoAccesoId);
                            cmd.ExecuteNonQuery();
                        }
                        tram.Commit();
                        return (loteid, codigo, creado);
                    }
                    catch
                    {
                        tram.Rollback();
                        throw;
                    }
                }
            }
        }

        public void InsertarHito(HitoLote hito, SqlConnection con, SqlTransaction st)
        {
            using (var cmd = new SqlCommand(@"INSERT INTO HitoLote (TipoHito,Fecha,Responsable,Metodo,NumeroDocumento,Observaciones,DentroPlazo,HashAnterior,Hash,LoteId,UsuarioId,UsuarioInstitucionId) " +
                " VALUES(@TipoHito,@Fecha,@Responsable,@Metodo,@NumeroDocumento,@Observaciones,@DentroPlazo,@HashAnterior,@Hash,@LoteId,@UsuarioId,@UsuarioInstitucionId) ", con,st))
            {
                cmd.Parameters.AddWithValue("@TipoHito",hito.TipoHito.ToString());
                cmd.Parameters.AddWithValue("@Fecha",hito.Fecha);
                cmd.Parameters.AddWithValue("@Responsable",(object?)hito.Responsable??DBNull.Value);
                cmd.Parameters.AddWithValue("@Metodo",(object?)hito.Metodo??DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroDocumento",(object?)hito.NumeroDocumento??DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones",(object?)hito.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DentroPlazo",(object?)hito.DentroDelPlazo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HashAnterior",hito.HashAnterior);
                cmd.Parameters.AddWithValue("@Hash",hito.Hash);
                cmd.Parameters.AddWithValue("@LoteId",hito.LoteId);
                cmd.Parameters.AddWithValue("@UsuarioId",(object?)hito.UsuarioId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioInstitucionId",(object?)hito.UsuarioInstitucionId ?? DBNull.Value);
                

                cmd.ExecuteNonQuery();
            }
        }

    }
}
