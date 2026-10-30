using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class TelemetriaBL
    {
        private readonly TelemetriaDAL _telemetriaDAl;
        private readonly UmbralDAL _umbralDAL;
        private readonly DispositivoIoTDAL _dispositivoIoTDAL;
        private readonly AlertaBL _alertaBL;
        private readonly EventoAccesoDAL _eventoaccesoDAL;
        public TelemetriaBL(string conectar)
        {
            _telemetriaDAl = new TelemetriaDAL(conectar);
            _dispositivoIoTDAL = new DispositivoIoTDAL(conectar);
            _umbralDAL=new UmbralDAL(conectar);
            _alertaBL=new AlertaBL(conectar);
            _eventoaccesoDAL=new EventoAccesoDAL(conectar);
        }

        public static void ValidarDatos(LecturaSensor l)
        {
            if (l.Nivel is null || l.Temperatura is null || l.Humedad is null)
            {
                throw new Exception("Lectura Descartada : Falta Nivel , Falta Humedad o Falta Temperatura");
            }
            if (l.Nivel < 0 || l.Nivel > 100)
            {
                throw new Exception($"Lectura Descartada : Nivel Fuera de Rango ({l.Nivel} %) ");
            }
            if (l.Humedad < 0 || l.Humedad > 100)
            {
                throw new Exception($"Lectura Descartada : Humedad Fuera de Rango ({l.Humedad} %) ");
            }
            if (l.Temperatura < -10 || l.Temperatura > 85)
            {
                throw new Exception($"Lectura Descartada : Temperatura Fuera de Rango({l.Temperatura}C°)");
            }
            if (l.Bateria is < 0 or > 100)
            {
                throw new Exception($"Lectura Descartada : Bateria Fuera de Rango({l.Bateria}C°)");
            }

        }

        public Telemetria ProcesarLectura(string mac, LecturaSensor lectura)
        {
            string macNormalizada = DispositivoIoTBL.NormalizarMac(mac);
            if (_dispositivoIoTDAL.BuscarMAC(macNormalizada) is not DispositivoIoT dispositivoIoT)
            {
                throw new Exception($"Lectura Descartada No hay Dispositivo con Mac Activo {mac}");
            }

            ValidarDatos(lectura);

            var telemetria = new Telemetria
            {
                DispositivoIoTId = dispositivoIoT.Id,
                ContenedorId = dispositivoIoT.ContenedorId,
                Nivel = lectura.Nivel!.Value,
                Temperatura = Math.Round(lectura.Temperatura!.Value, 1),
                Humedad = lectura.Humedad!.Value,
                Bateria = lectura.Bateria,
                NivelConexion = lectura.NivelConexion,
                FechaHora = DateTime.Now,
            };

            telemetria.Id = _telemetriaDAl.RegistrarLectura(telemetria);
            var pendiente = _eventoaccesoDAL.BuscarPendienteDeCierre(telemetria.ContenedorId,telemetria.FechaHora.AddMinutes(-5));
            if (pendiente != null)
            {
                var (idAcceso, nivelprevio, capacidad) = pendiente.Value;
                decimal volumen=Math.Round((telemetria.Nivel-nivelprevio)/100m*capacidad,1);
                _eventoaccesoDAL.CompletarCierre(idAcceso,telemetria.Nivel,volumen);
                telemetria.CompletoAcceso = true;
            }
            telemetria.EmpresaLogisticaId = dispositivoIoT.EmpresaLogisticaId;
            var umbral = _umbralDAL.BuscarxContenedor(telemetria.ContenedorId, dispositivoIoT.EmpresaLogisticaId);
            telemetria.Clasificacion = UmbralBL.ClasificarLectura(telemetria, umbral);
            telemetria.ALertasNuevas = _alertaBL.EvaluarLectura(telemetria, umbral);
            return telemetria;
        } 

        public Dictionary<int,Telemetria> UltimasLecturas(int institucionId,int empresaId)
        {
            return _telemetriaDAl.UltimasLecturasxInstitucion(institucionId, empresaId).ToDictionary(t => t.ContenedorId);
        }



    }
}
