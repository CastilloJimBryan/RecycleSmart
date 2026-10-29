using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RecycleSmartBLL
{
    public class EventoAccesoBL
    {
        private readonly EventoAccesoDAL _eventoAccesoDAL;
        private readonly TarjeraRFIDDAL _tarjetarefidDAL;
        private readonly TelemetriaDAL _telemetriaDAL;
        private readonly DispositivoIoTDAL _dispositivoDAL;
        private readonly AlertaBL _alertaBL;

        public EventoAccesoBL(string conectar)
        {
            _eventoAccesoDAL = new EventoAccesoDAL(conectar);
            _telemetriaDAL = new TelemetriaDAL(conectar);
            _tarjetarefidDAL = new TarjeraRFIDDAL(conectar);
            _dispositivoDAL = new DispositivoIoTDAL(conectar);
            _alertaBL = new AlertaBL(conectar);
        }

        public EventoAcceso ProcesarAcceso(string mac, LecturaRFID lectura)
        {
            //de que contenedor es el lector
            string macnormalizada = DispositivoIoTBL.NormalizarMac(mac);
            if (_dispositivoDAL.BuscarMAC(macnormalizada) is not DispositivoIoT dispositivo)
            {
                throw new Exception($"Accesos Denegado: no hay Dispositivo Vigente con esa mac {mac}");
            }

            string uidleida = (lectura.UID ?? "").Trim();
            if (uidleida == "")
            {
                throw new Exception($"Acceso Denegado: No Tiene UID");
            }

            if (uidleida.Length > 20)
            {
                uidleida = uidleida[..20];
            }
            var fechaActual = DateTime.Now;

            var evento = new EventoAcceso
            {
                ContenedorId = dispositivo.ContenedorId,
                EmpresaLogisticaId = dispositivo.EmpresaLogisticaId,
                UIDLeido = uidleida,
                Fecha = fechaActual,
                NivelPrevio = _telemetriaDAL.UltimoNivelContenedor(dispositivo.ContenedorId)
            };

            string uid = TarjetaRFIDBL.NormalizarUID(uidleida);
            var tarjeta = uid == "" ? null : _tarjetarefidDAL.BuscarTarjetaxUID(uid);
            evento.Motivo = Validar(tarjeta, dispositivo.ContenedorId, fechaActual);

            if (tarjeta != null)
            {
                evento.TarjetaRfidId = tarjeta.Id;
                evento.UsuarioInstitucionId = tarjeta.UsuarioInstitucionId;
                evento.UsuarioNombre = tarjeta.UsuarioNombre;
            }

            evento.EstadoAcceso = evento.Motivo == TipoEstado.MotivoAcceso.Autorizado ? TipoEstado.EstadoAcceso.Permitido : TipoEstado.EstadoAcceso.Denegado;
            evento.Id = _eventoAccesoDAL.Registrar(evento);
            if (!evento.Permitido)
            {
                evento.AlertasNuevas = _alertaBL.RegistrarAccesoDenegado(evento.ContenedorId, evento.Motivo, uidleida);
            }
            return evento;
        }

        private static TipoEstado.MotivoAcceso Validar(TarjetaRFID? tarjeta, int contenedorid, DateTime actual)
        {
            //aca se pregunta si existe

            if (tarjeta == null)
            {
                return TipoEstado.MotivoAcceso.TarjetaDesconocida;
            }
            //si usuario y la tarjeta estan activas

            if (tarjeta.TipoEstado != TipoEstado.Estados.Activo || tarjeta.EstadoUsuario != TipoEstado.Estados.Activo)
            {
                return TipoEstado.MotivoAcceso.TarjetaInactiva;
            }
            //si contenedor esta habilitado para esa tarjeta

            if (!tarjeta.ContenedoresId.Contains(contenedorid))
            {
                return TipoEstado.MotivoAcceso.ContenedorNoHabilitado;
            }

            //sigue vigente

            if (tarjeta.FechaVecimiento.Date < actual.Date)
            {
                return TipoEstado.MotivoAcceso.TarjetaVencida;
            }

            //horario permitido

            if (!tarjeta.DiasHabilitados.HasFlag(DiaActual(actual)) || actual.TimeOfDay < tarjeta.HoraInicio ||
                actual.TimeOfDay > tarjeta.HoraFin)
            {
                return TipoEstado.MotivoAcceso.FueraHorario;
            }
            return TipoEstado.MotivoAcceso.Autorizado;
        }

        public static DiasSemana.DiaSemana DiaActual(DateTime fecha)
            => fecha.DayOfWeek switch
            {
                DayOfWeek.Monday => DiasSemana.DiaSemana.Lunes,
                DayOfWeek.Tuesday => DiasSemana.DiaSemana.Martes,
                DayOfWeek.Wednesday => DiasSemana.DiaSemana.Miercoles,
                DayOfWeek.Thursday => DiasSemana.DiaSemana.Jueves,
                DayOfWeek.Friday => DiasSemana.DiaSemana.Viernes,
                DayOfWeek.Saturday => DiasSemana.DiaSemana.Sabado,
                _ => DiasSemana.DiaSemana.Domingo
            };

        public List<EventoAcceso> ListarXEmpresa(int empresaid,int institucionid,bool solodenegados)
        {
            return _eventoAccesoDAL.ListarXEmpresa(empresaid, institucionid, solodenegados);
        }
    }
}
