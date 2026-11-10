using RecycleSmartBE;
using RecycleSmartDAL;
using Servicio;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class LoteBL
    {
        public const int DiasRecoleccion = 5;
        public const int HorasTratamiento = 72;
        private readonly LoteDAL _loteDal;
        public LoteBL(string contectar)
        {
            _loteDal=new LoteDAL(contectar);
        }
        public(int Loteid,string codigo,bool creado)  RegistrarDeposito(long EventoAccesoId)
        {
            return _loteDal.RegistrarDeposito(EventoAccesoId,DiasRecoleccion);
        }

        public List<Lote> ListarLote(int empresaid,int institucionid,bool cerrada)
        {
            return _loteDal.ListarLote(empresaid,institucionid,cerrada);
        }

        public Lote ObtenerLote(int loteid,int empresaid)
        {
            return _loteDal.BuscarxId(loteid, empresaid)??throw new Exception("Lote no existe");
        }
        public List<(DateTime fecha,string usuario,decimal volumen)> Deposito(int loteid)
        {
            return _loteDal.Depositos(loteid);
        }

        public (bool integro,string mensaje) VerificarIntegridad(Lote lote)
        {
            string anterior = Integridad.HashInicial;
            foreach (var item in lote.Hitos)
            {
                if(item.HashAnterior!=anterior)
                {
                    return(false,$"La Cadena Esta Ropta en el hito {item.TipoHito} : No Continua ");
                }
                if (Integridad.hashito(lote.Codigo, item) != item.Hash)
                {
                    return (false, $"El Hito {item.TipoHito} del {item.Fecha: dd/MM/yyyy HH:mm} fue alterado: los datos no coinciden con la firma");
                }
                anterior = item.Hash;
            }
            if(lote.HashFinal!=null && lote.HashFinal!=anterior)
            {
                return (false, $"El sello de cierre del lote no coincide con sus hitos");

            }
            return (true, $"Integro: se verificaroon {lote.Hitos.Count} hitos y la cadena esta completa");
        }

        // Qué plazo corre ahora y cuánto se consumió (para la barra)
        public static (string Etapa, DateTime? Limite, int Porcentaje, bool Vencido) PlazoActual(Lote l)
        {
            DateTime desde, limite;
            string etapa;

            if (l.EstadoLote is TipoEstado.EstadoLote.EnCurso or TipoEstado.EstadoLote.RecoleccionSolicitada)
            {
                etapa = "Recolección";
                desde = l.FechaGeneracion;
                limite = l.FechaLimiteRecoleccion;
            }
            else if (l.EstadoLote == TipoEstado.EstadoLote.Recolectado && l.FechaLimiteTratamiento != null)
            {
                etapa = "Tratamiento";
                limite = l.FechaLimiteTratamiento.Value;
                desde = limite.AddHours(-HorasTratamiento);
            }
            else
            {
                return ("", null, 0, false);   // tratado o cerrado: no corre ningún plazo de la ley
            }

            double total = (limite - desde).TotalMinutes;
            double usado = (DateTime.Now - desde).TotalMinutes;
            int porcentaje = (int)Math.Clamp(usado / total * 100, 0, 100);
            return (etapa, limite, porcentaje, DateTime.Now > limite);
        }

        public void SolicitarRecoleccion(int loteid, int empresaid, int usuarioid, string? observacion)
        {
            var lote = ObtenerLote(loteid, empresaid);
            if (lote.EstadoLote != TipoEstado.EstadoLote.EnCurso)
                throw new Exception("Solo se puede solicitar la recolección de un lote en curso.");

            observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if (observacion != null && observacion.Length > 500)
                throw new Exception("Las observaciones pueden tener hasta 500 caracteres.");

            var hito = new HitoLote
            {
                TipoHito = TipoEstado.TipoHito.SolicitudRecoleccion,
                Fecha = Integridad.Truncar(DateTime.Now),
                UsuarioId = usuarioid,
                Observaciones = observacion
                // DentroDelPlazo queda null: la solicitud no tiene plazo propio
            };

            _loteDal.RegistrarHito(loteid, empresaid,TipoEstado.EstadoLote.EnCurso, TipoEstado.EstadoLote.RecoleccionSolicitada, hito);
        }

        public void RegistrarRecoleccion(int loteid,int empresaid,int usuarioid,DateTime fecharecoleccion,string? responsable,string? manifiesto,string?observacion)
        {
            var lote = ObtenerLote(loteid, empresaid);
            if(lote.EstadoLote!=TipoEstado.EstadoLote.RecoleccionSolicitada)
            {
                throw new Exception("La Recoleccion solo se puede registrar despues de solicitarla");
            }

            var fecha = Integridad.Truncar(fecharecoleccion);
            if (fecha > DateTime.Now.AddMinutes(5))
            {
                throw new Exception("La fecha de recolecion no puede ser futura");
            }
            if(fecha<lote.FechaGeneracion)
            {
                throw new Exception("la fecha de coleccion no puede ser anterior a la fecha que se genero");
            }

            responsable = responsable?.Trim();
            manifiesto = manifiesto?.Trim();
            observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if (string.IsNullOrEmpty(responsable) || responsable.Length > 100)
                throw new Exception("Indicá el operario y/o vehículo que retiró el lote (hasta 100 caracteres).");
            if (string.IsNullOrEmpty(manifiesto) || manifiesto.Length > 50)
                throw new Exception("El número de manifiesto de transporte es obligatorio (hasta 50 caracteres).");

            var hito = new HitoLote
            {
                TipoHito = TipoEstado.TipoHito.Recoleccion,
                Fecha = fecha,
                UsuarioId = usuarioid,
                Responsable = responsable,
                NumeroDocumento = manifiesto,
                Observaciones = observacion,
                DentroDelPlazo = fecha <= lote.FechaLimiteRecoleccion
            };

            _loteDal.RegistrarHito(loteid, empresaid,TipoEstado.EstadoLote.RecoleccionSolicitada, TipoEstado.EstadoLote.Recolectado,hito, limitetratamiento: fecha.AddHours(HorasTratamiento));
        }


        public void RegistrarTratamiento(int loteid,int empresaid,int usuarioid,DateTime fechatratamiento,TipoEstado.MetodoTratamiento metodo,string? operario,string?certificado,string?observacion)
        {
            var lote = ObtenerLote(loteid, empresaid);
            if (lote.EstadoLote != TipoEstado.EstadoLote.Recolectado)
                throw new Exception("El tratamiento solo se puede registrar después de la recolección.");

            var recoleccion = lote.Hitos.First(h => h.TipoHito == TipoEstado.TipoHito.Recoleccion);
            var fecha = Integridad.Truncar(fechatratamiento);
            if (fecha > DateTime.Now.AddMinutes(5))
                throw new Exception("La fecha de tratamiento no puede ser futura.");
            if (fecha < recoleccion.Fecha)
                throw new Exception($"El tratamiento no puede ser anterior a la recolección ({recoleccion.Fecha:dd/MM/yyyy HH:mm}).");
            if (!Enum.IsDefined(metodo))
                throw new Exception("Elegí un método de tratamiento válido.");

            operario = operario?.Trim();
            certificado = certificado?.Trim();
            observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if (string.IsNullOrEmpty(operario) || operario.Length > 100)
                throw new Exception("Indicá la planta u operador que realizó el tratamiento (hasta 100 caracteres).");
            if (string.IsNullOrEmpty(certificado) || certificado.Length > 50)
                throw new Exception("El número de certificado de tratamiento es obligatorio (hasta 50 caracteres).");
            if (metodo == TipoEstado.MetodoTratamiento.Otro && observacion == null)
                throw new Exception("Si el método es «Otro», indicá cuál en las observaciones.");

            var hito = new HitoLote
            {
                TipoHito = TipoEstado.TipoHito.Tratamiento,
                Fecha = fecha,
                UsuarioId = usuarioid,
                Responsable = operario,
                Metodo = metodo.ToString(),
                NumeroDocumento = certificado,
                Observaciones = observacion,
                DentroDelPlazo = fecha <= lote.FechaLimiteTratamiento   // usá tu nombre de propiedad
            };

            _loteDal.RegistrarHito(loteid, empresaid,TipoEstado.EstadoLote.Recolectado, TipoEstado.EstadoLote.Tratado, hito);
        }

        public bool RegistrarDisposicion(int loteid,int empresaid,int usuarioid,DateTime fechadisposicion,string? destino,string?manifiesto,string?observacion)
        {
            var lote = ObtenerLote(loteid, empresaid);
            if (lote.EstadoLote != TipoEstado.EstadoLote.Tratado)
                throw new Exception("La disposición final solo se puede registrar después del tratamiento.");

            var tratamiento = lote.Hitos.First(h => h.TipoHito == TipoEstado.TipoHito.Tratamiento);
            var fecha = Integridad.Truncar(fechadisposicion);
            if (fecha > DateTime.Now.AddMinutes(5))
                throw new Exception("La fecha de disposición no puede ser futura.");
            if (fecha < tratamiento.Fecha)
                throw new Exception($"La disposición no puede ser anterior al tratamiento ({tratamiento.Fecha:dd/MM/yyyy HH:mm}).");

            destino = destino?.Trim();
            manifiesto = manifiesto?.Trim();
            observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            if (string.IsNullOrEmpty(destino) || destino.Length > 100)
                throw new Exception("Indicá el destino final (empresa o sitio de disposición, hasta 100 caracteres).");
            if (string.IsNullOrEmpty(manifiesto) || manifiesto.Length > 50)
                throw new Exception("El número de manifiesto final es obligatorio (hasta 50 caracteres).");

            // Conforme: ningún hito con plazo quedó fuera de plazo
            bool conforme = lote.Hitos.All(h => h.DentroDelPlazo != false);

            var hito = new HitoLote
            {
                TipoHito = TipoEstado.TipoHito.DisposicionFinal,
                Fecha = fecha,
                UsuarioId = usuarioid,
                Responsable = destino,
                NumeroDocumento = manifiesto,
                Observaciones = observacion
                // DentroDelPlazo queda null: la disposición no tiene plazo propio
            };

            _loteDal.RegistrarHito(loteid, empresaid,TipoEstado.EstadoLote.Tratado, TipoEstado.EstadoLote.Cerrado, hito, conforme: conforme);

            return conforme;
        }
        ///Contenedor
        ///
        public List<Lote> SinCerrarContenedor(int contenedorid)
        {
            return _loteDal.SinCerrarContenedor(contenedorid);
        }
        public Dictionary<int,(int loteid,TipoEstado.EstadoLote estado)>AbiertosXEmpresa(int empresaid)
        {
            return _loteDal.AbiertosXEmpresa(empresaid);
        }
    }
}
