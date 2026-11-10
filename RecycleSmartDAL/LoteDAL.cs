using Microsoft.AspNetCore.Mvc.Formatters;
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

        public (int loteId, string Codigo, bool Creado) RegistrarDeposito(long EventoAccesoId, int diasRecoleccion)
        {
            using (var con = new SqlConnection(_conectar))
            {
                con.Open();
                using (var tram = con.BeginTransaction())
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

                            using (var leer = cmd.ExecuteReader())
                            {
                                if (!leer.Read())
                                {
                                    throw new Exception($"El Evento {EventoAccesoId} no existe o ya tiene lote");
                                }
                                contenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId"));
                                institucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId"));
                                UsuarioInstitucionId = leer.IsDBNull(leer.GetOrdinal("UsuarioInstitucionId")) ? null : leer.GetInt32(leer.GetOrdinal("UsuarioInstitucionId"));
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
                        using (var cmd = new SqlCommand(@"SELECT Id ,Codigo FROM Lote WITH (UPDLOCK,HOLDLOCK) WHERE ContenedorId=@ContenedorId AND EstadoLote IN ('EnCurso','RecoleccionSolicitada')", con, tram))
                        {
                            cmd.Parameters.AddWithValue("@ContenedorId", contenedorId);
                            using (var leer = cmd.ExecuteReader())
                            {
                                if (leer.Read())
                                {
                                    loteid = leer.GetInt32(0);
                                    codigo = leer.GetString(1);
                                }
                            }
                        }

                        bool creado = false;
                        if (loteid == 0)
                        {
                            var fechacreacion = Integridad.Truncar(Fecha);
                            int anio = fechacreacion.Year;
                            int numero;
                            using (var cmd = new SqlCommand("SELECT ISNULL (MAX(Numero),0)+1 FROM Lote WITH (UPDLOCK , HOLDLOCK) WHERE InstitucionId=@InstitucionId AND Anio=@Anio", con, tram))
                            {
                                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);
                                cmd.Parameters.AddWithValue("@Anio", anio);
                                numero = (int)cmd.ExecuteScalar();
                            }
                            codigo = $"RS-{institucionId}-{anio}-{numero:D4}";
                            //lote
                            using (var cmd = new SqlCommand(@"INSERT INTO Lote(Codigo,Anio,Numero,TipoResiduo,EstadoLote,FechaGeneracion,FechaLimiteRecoleccion,VolumenTotal,CantidadDeposito,ContenedorId,InstitucionId) " +
                                " OUTPUT INSERTED.Id " +
                                " VALUES(@codigo,@Anio,@Numero,@TipoResiduo,@EstadoLote,@FechaGeneracion,@FechaLimiteRecoleccion,0,0,@ContenedorId,@InstitucionId) ", con, tram))
                            {
                                cmd.Parameters.AddWithValue("@Codigo", codigo);
                                cmd.Parameters.AddWithValue("@EstadoLote", TipoEstado.EstadoLote.EnCurso.ToString());
                                cmd.Parameters.AddWithValue("@Numero", numero);
                                cmd.Parameters.AddWithValue("@TipoResiduo", TipoResiduo);
                                cmd.Parameters.AddWithValue("@Anio", anio);
                                cmd.Parameters.AddWithValue("@FechaGeneracion", fechacreacion);
                                cmd.Parameters.AddWithValue("@FechaLimiteRecoleccion", fechacreacion.AddDays(diasRecoleccion));
                                cmd.Parameters.AddWithValue("@ContenedorId", contenedorId);
                                cmd.Parameters.AddWithValue("@InstitucionId", institucionId);

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
                            InsertarHito(hito, con, tram);

                            creado = true;
                        }

                        using (var cmd = new SqlCommand(@"UPDATE Lote SET VolumenTotal=VolumenTotal+@Volumen,CantidadDeposito=CantidadDeposito + 1 " +
                            " WHERE Id=@LoteId; " +
                            " UPDATE EventoAcceso SET LoteId=@LoteId" +
                            " WHERE EventoAcceso.Id=@EventoAccesoId ", con, tram))
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
                " VALUES(@TipoHito,@Fecha,@Responsable,@Metodo,@NumeroDocumento,@Observaciones,@DentroPlazo,@HashAnterior,@Hash,@LoteId,@UsuarioId,@UsuarioInstitucionId) ", con, st))
            {
                cmd.Parameters.AddWithValue("@TipoHito", hito.TipoHito.ToString());
                cmd.Parameters.AddWithValue("@Fecha", hito.Fecha);
                cmd.Parameters.AddWithValue("@Responsable", (object?)hito.Responsable ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Metodo", (object?)hito.Metodo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroDocumento", (object?)hito.NumeroDocumento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object?)hito.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DentroPlazo", (object?)hito.DentroDelPlazo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@HashAnterior", hito.HashAnterior);
                cmd.Parameters.AddWithValue("@Hash", hito.Hash);
                cmd.Parameters.AddWithValue("@LoteId", hito.LoteId);
                cmd.Parameters.AddWithValue("@UsuarioId", (object?)hito.UsuarioId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioInstitucionId", (object?)hito.UsuarioInstitucionId ?? DBNull.Value);


                cmd.ExecuteNonQuery();
            }
        }

        public List<Lote> ListarLote(int empresaid, int institucionid, bool cerrados)
        {
            var lista = new List<Lote>();

            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(@"SELECT Lote.Id,Lote.Codigo,Lote.TipoResiduo,Lote.EstadoLote,Lote.FechaGeneracion,Lote.VolumenTotal,Lote.CantidadDeposito, " +
                " Lote.FechaLimiteRecoleccion,Lote.FechaLimiteTratamiento,Lote.FechaCierre,Lote.Conforme,Lote.InstitucionId,Contenedor.Id AS ContenedorId,Contenedor.Nombre AS ContenedorNombre," +
                " InstitucionCliente.Nombre AS InstitucionNombre" +
                " FROM Lote" +
                " INNER JOIN Contenedor ON Contenedor.Id=Lote.ContenedorId" +
                " INNER JOIN InstitucionCliente on InstitucionCliente.Id=Contenedor.InstitucionId" +
                " WHERE InstitucionCliente.EmpresaLogisticaId=@EmpresaId " +
                " AND (@InstitucionId=0 OR lote.InstitucionId=@InstitucionId) AND ((@Cerrado=1 AND Lote.EstadoLote='Cerrado') OR (@Cerrado=0 AND Lote.EstadoLote<>'Cerrado'))" +
                " ORDER BY Lote.FechaGeneracion DESC ", con))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                cmd.Parameters.AddWithValue("@InstitucionId", institucionid);
                cmd.Parameters.AddWithValue("@Cerrado", cerrados);
                con.Open();

                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        lista.Add(new Lote
                        {
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            Codigo = leer.GetString(leer.GetOrdinal("Codigo")),
                            TipoResiduo = leer.GetString(leer.GetOrdinal("TipoResiduo")),
                            EstadoLote = Enum.Parse<TipoEstado.EstadoLote>(leer.GetString(leer.GetOrdinal("EstadoLote")), true),
                            FechaGeneracion = leer.GetDateTime(leer.GetOrdinal("FechaGeneracion")),
                            FechaLimiteTratamiento = leer.IsDBNull(leer.GetOrdinal("FechaLimiteTratamiento")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaLimiteTratamiento")),
                            FechaLimiteRecoleccion = leer.GetDateTime(leer.GetOrdinal("FechaLimiteRecoleccion")),
                            FechaCierre = leer.IsDBNull(leer.GetOrdinal("FechaCierre")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaCierre")),
                            VolumenTotal = leer.GetDecimal(leer.GetOrdinal("VolumenTotal")),
                            CantidadDepositada = leer.GetInt32(leer.GetOrdinal("CantidadDeposito")),
                            Conforme = leer.IsDBNull(leer.GetOrdinal("Conforme")) ? null : leer.GetBoolean(leer.GetOrdinal("Conforme")),
                            ContenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId")),
                            ContenedorNombre = leer.GetString(leer.GetOrdinal("ContenedorNombre")),
                            InstitucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId")),
                            InstitucionNombre = leer.GetString(leer.GetOrdinal("InstitucionNombre"))

                        });
                    }
                }
            }
            return lista;
        }

        public Lote? BuscarxId(int loteid, int empresaid)
        {
            Lote? lote = null;
            using (var con = new SqlConnection(_conectar))
            {
                con.Open();

                using (var cmd = new SqlCommand(@"SELECT Lote.Id, Lote.Codigo,Lote.TipoResiduo,Lote.EstadoLote,Lote.FechaGeneracion,Lote.VolumenTotal,lote.CantidadDeposito,Lote.FechaLimiteRecoleccion,Lote.FechaLimiteTratamiento, " +
                    " lote.FechaCierre,Lote.Conforme,Lote.HashFinal,Lote.ContenedorId,lote.InstitucionId,Contenedor.Nombre AS ContenedorNombre,InstitucionCliente.Nombre AS InstitucionNombre " +
                    " FROM Lote" +
                    " INNER JOIN Contenedor ON Contenedor.Id=Lote.ContenedorId" +
                    " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Contenedor.InstitucionId" +
                    " WHERE Lote.Id=@LoteId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId ", con))
                {
                    cmd.Parameters.AddWithValue("@LoteId", loteid);
                    cmd.Parameters.AddWithValue("@EmpresaId", empresaid);

                    using (var leer = cmd.ExecuteReader())
                    {
                        if (!leer.Read()) return null;
                        lote = new Lote
                        {
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            Codigo = leer.GetString(leer.GetOrdinal("Codigo")),
                            TipoResiduo = leer.GetString(leer.GetOrdinal("TipoResiduo")),
                            EstadoLote = Enum.Parse<TipoEstado.EstadoLote>(leer.GetString(leer.GetOrdinal("EstadoLote")), true),
                            FechaGeneracion = leer.GetDateTime(leer.GetOrdinal("FechaGeneracion")),
                            FechaLimiteTratamiento = leer.IsDBNull(leer.GetOrdinal("FechaLimiteTratamiento")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaLimiteTratamiento")),
                            FechaLimiteRecoleccion = leer.GetDateTime(leer.GetOrdinal("FechaLimiteRecoleccion")),
                            FechaCierre = leer.IsDBNull(leer.GetOrdinal("FechaCierre")) ? null : leer.GetDateTime(leer.GetOrdinal("FechaCierre")),
                            VolumenTotal = leer.GetDecimal(leer.GetOrdinal("VolumenTotal")),
                            CantidadDepositada = leer.GetInt32(leer.GetOrdinal("CantidadDeposito")),
                            Conforme = leer.IsDBNull(leer.GetOrdinal("Conforme")) ? null : leer.GetBoolean(leer.GetOrdinal("Conforme")),
                            ContenedorId = leer.GetInt32(leer.GetOrdinal("ContenedorId")),
                            ContenedorNombre = leer.GetString(leer.GetOrdinal("ContenedorNombre")),
                            InstitucionId = leer.GetInt32(leer.GetOrdinal("InstitucionId")),
                            InstitucionNombre = leer.GetString(leer.GetOrdinal("InstitucionNombre")),
                            HashFinal = leer.IsDBNull(leer.GetOrdinal("HashFinal")) ? null : leer.GetString(leer.GetOrdinal("HashFinal"))
                        };
                    }
                }

                //sus hitos
                using (var cmd = new SqlCommand(@"SELECT HitoLote.Id,HitoLote.LoteId,HitoLote.TipoHito,HitoLote.Fecha,HitoLote.Responsable,HitoLote.Metodo,HitoLote.NumeroDocumento,HitoLote.Observaciones, " +
                    " HitoLote.DentroPlazo,HitoLote.HashAnterior,HitoLote.Hash,HitoLote.UsuarioId,HitoLote.UsuarioInstitucionId," +
                    " COALESCE(UsuarioEmpresaLogistica.Nombre+' '+UsuarioEmpresaLogistica.Apellido, UsuarioInstitucion.Nombre+' '+UsuarioInstitucion.Apellido) AS UsuarioNombre " +
                    " FROM HitoLote" +
                    " LEFT JOIN UsuarioEmpresaLogistica ON UsuarioEmpresaLogistica.Id=HitoLote.UsuarioId" +
                    " LEFT JOIN UsuarioInstitucion ON UsuarioInstitucion.Id=HitoLote.UsuarioInstitucionId" +
                    " WHERE HitoLote.LoteId=@LoteId ORDER BY HitoLote.Id ", con))
                {
                    cmd.Parameters.AddWithValue("@LoteId", loteid);
                    using (var leer = cmd.ExecuteReader())
                    {
                        while (leer.Read())
                        {
                            lote.Hitos.Add(new HitoLote
                            {
                                Id = leer.GetInt32(leer.GetOrdinal("Id")),
                                TipoHito = Enum.Parse<TipoEstado.TipoHito>(leer.GetString(leer.GetOrdinal("TipoHito")), true),
                                Fecha = leer.GetDateTime(leer.GetOrdinal("Fecha")),
                                Responsable = leer.IsDBNull(leer.GetOrdinal("Responsable")) ? null : leer.GetString(leer.GetOrdinal("Responsable")),
                                Metodo = leer.IsDBNull(leer.GetOrdinal("Metodo")) ? null : leer.GetString(leer.GetOrdinal("Metodo")),
                                NumeroDocumento = leer.IsDBNull(leer.GetOrdinal("NumeroDocumento")) ? null : leer.GetString(leer.GetOrdinal("NumeroDocumento")),
                                Observaciones = leer.IsDBNull(leer.GetOrdinal("Observaciones")) ? null : leer.GetString(leer.GetOrdinal("Observaciones")),
                                DentroDelPlazo = leer.IsDBNull(leer.GetOrdinal("DentroPlazo")) ? null : leer.GetBoolean(leer.GetOrdinal("DentroPlazo")),
                                HashAnterior = leer.GetString(leer.GetOrdinal("HashAnterior")),
                                Hash = leer.GetString(leer.GetOrdinal("Hash")),
                                LoteId = leer.GetInt32(leer.GetOrdinal("LoteId")),
                                UsuarioId = leer.IsDBNull(leer.GetOrdinal("UsuarioId")) ? null : leer.GetInt32(leer.GetOrdinal("UsuarioId")),
                                UsuarioNombre = leer.IsDBNull(leer.GetOrdinal("UsuarioNombre")) ? null : leer.GetString(leer.GetOrdinal("UsuarioNombre")),
                                UsuarioInstitucionId = leer.IsDBNull(leer.GetOrdinal("UsuarioInstitucionId")) ? null : leer.GetInt32(leer.GetOrdinal("UsuarioInstitucionId")),
                            });
                        }
                    }
                }
            }
            return lote;
        }

        public List<(DateTime fecha, string usuario, decimal volumen)> Depositos(int loteid)
        {
            var lista = new List<(DateTime, string, decimal)>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(@"SELECT EventoAcceso.Fecha, ISNULL(UsuarioInstitucion.Nombre+' '+UsuarioInstitucion.Apellido,'-') AS Usuario,EventoAcceso.VolumenDepositado " +
                " FROM EventoAcceso" +
                " LEFT JOIN UsuarioInstitucion ON UsuarioInstitucion.Id=EventoAcceso.UsuarioInstitucionId" +
                " WHERE EventoAcceso.LoteId=@LoteId ORDER BY EventoAcceso.Fecha ", con))
            {
                cmd.Parameters.AddWithValue("@LoteId", loteid);
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        lista.Add((leer.GetDateTime(0), leer.GetString(1), leer.GetDecimal(2)));
                    }
                }
            }
            return lista;
        }


        public void RegistrarHito(int loteid, int empresaid, TipoEstado.EstadoLote estadoRequerido, TipoEstado.EstadoLote estadoNuevo, HitoLote hito, DateTime? limitetratamiento = null, bool? conforme=null)
        {
            using(var con=new SqlConnection(_conectar))
            {
                con.Open();
                using (var tran= con.BeginTransaction())
                {
                    try
                    {
                        string codigo;
                        string estadoActual;
                        using(var cmd=new SqlCommand(@"SELECT Lote.Codigo,Lote.EstadoLote FROM Lote WITH (UPDLOCK,HOLDLOCK)" +
                            " INNER JOIN InstitucionCliente ON InstitucionCliente.Id=Lote.InstitucionId " +
                            " WHERE Lote.Id=@LoteId AND InstitucionCliente.EmpresaLogisticaId=@EmpresaId ", con,tran))
                        {
                            cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                            cmd.Parameters.AddWithValue("@LoteId", loteid);

                            using(var leer=cmd.ExecuteReader())
                            {
                                if (!leer.Read()) throw new Exception("Lote no existe");
                                codigo = leer.GetString(0);
                                estadoActual = leer.GetString(1);
                            }
                        }
                        if (estadoActual != estadoRequerido.ToString())
                        {
                            throw new Exception($"El lote esta en estado {estadoActual} : Este paso no corresponde ahora");
                        }
                        using(var cmd=new SqlCommand(@"SELECT TOP 1 HitoLote.Hash FROM HitoLote WHERE HitoLote.LoteId=@LoteId ORDER BY Id DESC", con,tran))
                        {
                            cmd.Parameters.AddWithValue("@LoteId", loteid);
                            hito.HashAnterior = (string?)cmd.ExecuteScalar() ?? Integridad.HashInicial;
                        }

                        //hito firmado

                        hito.LoteId = loteid;
                        hito.Hash = Integridad.hashito(codigo, hito);
                        InsertarHito(hito, con, tran);

                        using (var cmd=new SqlCommand(@"UPDATE Lote SET EstadoLote=@EstadoLote,FechaLimiteTratamiento=ISNULL(@LimiteTratamiento,FechaLimiteTratamiento), " +
                            " Conforme=ISNULL(@Conforme,Conforme),FechaCierre=CASE WHEN @Conforme IS NULL THEN FechaCierre ELSE @Fecha END," +
                            " HashFinal=CASE WHEN @Conforme IS NULL THEN HashFinal ELSE @Hash END" +
                            " WHERE Id=@LoteId   ", con,tran))
                        {
                            cmd.Parameters.AddWithValue("@EstadoLote",estadoNuevo.ToString());
                            cmd.Parameters.AddWithValue("@LimiteTratamiento",(object?)limitetratamiento??DBNull.Value);
                            cmd.Parameters.AddWithValue("@Conforme",(object?)conforme??DBNull.Value);
                            cmd.Parameters.AddWithValue("@Fecha",hito.Fecha);
                            cmd.Parameters.AddWithValue("@Hash",hito.Hash);
                            cmd.Parameters.AddWithValue("@LoteId",loteid);
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
        // no cierra el contenedor si tiene lote activo
        public List<Lote> SinCerrarContenedor(int contenedorId)
        {
            var lista=new List<Lote>();
            using(var con=new SqlConnection(_conectar))
            using(var cmd=new SqlCommand(@"SELECT Id,Codigo,EstadoLote,FechaGeneracion,VolumenTotal FROM Lote " +
                " WHERE ContenedorId=@ContenedorId AND EstadoLote <>'Cerrado'" +
                " ORDER BY FechaGeneracion ", con))
            {
                cmd.Parameters.AddWithValue("@ContenedorId",contenedorId);
                con.Open();
                using(var leer=cmd.ExecuteReader())
                {
                    while (leer.Read())
                    {
                        lista.Add(new Lote
                        {
                            Id = leer.GetInt32(leer.GetOrdinal("Id")),
                            Codigo = leer.GetString(leer.GetOrdinal("Codigo")),
                            EstadoLote=Enum.Parse<TipoEstado.EstadoLote>(leer.GetString(leer.GetOrdinal("EstadoLote")),true),
                            FechaGeneracion=leer.GetDateTime(leer.GetOrdinal("FechaGeneracion")),
                            VolumenTotal=leer.GetDecimal(leer.GetOrdinal("VolumenTotal"))
                        });
                    }
                }
                return lista;
            }
        }
        //x Empresa

        public Dictionary<int, (int loteid,TipoEstado.EstadoLote)>AbiertosXEmpresa(int empresaid)
        {
            var dic = new Dictionary<int, (int, TipoEstado.EstadoLote)>();
            using (var con = new SqlConnection(_conectar))
            using (var cmd = new SqlCommand(@"SELECT Lote.ContenedorId, Lote.Id, Lote.EstadoLote FROM Lote " +
                " INNER JOIN InstitucionCliente ON InstitucionCliente.Id = Lote.InstitucionId " +
                " WHERE InstitucionCliente.EmpresaLogisticaId = @EmpresaId" +
                " AND Lote.EstadoLote IN ('EnCurso', 'RecoleccionSolicitada')", con))
            {
                cmd.Parameters.AddWithValue("@EmpresaId", empresaid);
                con.Open();
                using (var leer = cmd.ExecuteReader())
                {
                    while (leer.Read())
                        dic[leer.GetInt32(0)] = (leer.GetInt32(1), Enum.Parse<TipoEstado.EstadoLote>(leer.GetString(2), true));
                }
            }
            return dic;
        }
    }
}
