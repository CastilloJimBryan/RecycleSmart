using RecycleSmartBE;
using RecycleSmartDAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecycleSmartBLL
{
    public class AlertaBL
    {
        private readonly AlertaDAL _alertaDAL;

        public AlertaBL(string conectar)
        {
            _alertaDAL=new AlertaDAL(conectar);
        }

        public List<Alerta> EvaluarLectura(Telemetria t, Umbral? umbral)
        {
            var u = umbral ?? new Umbral();
            var novedades = new List<Alerta>();

            var clase = UmbralBL.ClasificarLectura(t, umbral);
            TipoEstado.Severidad? setNivel = clase switch
            {
                TipoEstado.ClasificacionLectura.Critico => TipoEstado.Severidad.Critico,
                TipoEstado.ClasificacionLectura.Advertencia => TipoEstado.Severidad.Advertencia,
                _ => null
            };
            decimal umbralnivel = setNivel == TipoEstado.Severidad.Critico ? u.NivelCritico : u.NivelAdvertencia;
            Procesar(t.ContenedorId, TipoEstado.TipoAlerta.Nivel, setNivel, t.Nivel, umbralnivel ,
                $"Nivel de LLenado {t.Nivel}% (umbral{umbralnivel}%)",novedades);

            TipoEstado.Severidad? sevtemp = null;
            if (t.Temperatura > u.TemperaturaMaxima + 5)
            {
                sevtemp = TipoEstado.Severidad.Critico;
            }
            else if(t.Temperatura>u.TemperaturaMaxima)
            {
                sevtemp = TipoEstado.Severidad.Advertencia; 
            }
            Procesar(t.ContenedorId, TipoEstado.TipoAlerta.Temperatura, sevtemp, t.Temperatura, u.TemperaturaMaxima,
               $"Temperatura {t.Temperatura:0.0}°C (Maxima{u.TemperaturaMaxima:0.0}°C)", novedades);
            return novedades;
        }

        private void Procesar(int contenedorId,TipoEstado.TipoAlerta tipoalerta,TipoEstado.Severidad?severidad,decimal valor,decimal valorumbral,string mensaje,List<Alerta> novedades)
        {
            var activa = _alertaDAL.BuscarAlertaActiva(contenedorId, tipoalerta);
            //System.Diagnostics.Debug.WriteLine($"DEBUG contenedor={contenedorId} tipo={tipoalerta} abierta={(activa?.Id.ToString() ?? "NINGUNA")}");
            if (severidad is null)
            {
                if(activa!=null)
                {
                    _alertaDAL.ResolucionAutomatica(activa.Id);
                }
                return;
            }
            if(activa==null)
            {
                var nueva = new Alerta
                {
                    TipoAlerta = tipoalerta,
                    TipoSeveridad = severidad.Value,
                    EstadoAlerta = TipoEstado.EstadoAlerta.Activa,
                    ValorDetectado = valor,
                    ValorUmbral = valorumbral,
                    Mensaje = mensaje,
                    FechaCreacion = DateTime.Now,
                    ContenedorId=contenedorId
                };

                nueva.Id = _alertaDAL.CrearAlerta(nueva);
                novedades.Add(nueva);
                return;
            }

            if(activa.TipoSeveridad==TipoEstado.Severidad.Advertencia && severidad== TipoEstado.Severidad.Critico)
            {
                activa.TipoSeveridad = TipoEstado.Severidad.Critico;
                activa.ValorDetectado = valor;
                activa.ValorUmbral= valorumbral;
                activa.Mensaje = mensaje;
                _alertaDAL.EscalarAlerta(activa);
                novedades.Add(activa);
            }
        }
    }
}
