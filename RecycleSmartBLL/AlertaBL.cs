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
                $"Nivel de llenado {t.Nivel} % (umbral {umbralnivel}%)",novedades);

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
               $"Temperatura {t.Temperatura:0.0}°C (Maxima {u.TemperaturaMaxima: 0.0}°C)", novedades);
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

        public Alerta BuscarxId(int id,int empresaid)
        {
            return _alertaDAL.BuscarXId(id, empresaid) ?? throw new Exception("La alerta no existe.");
        }

        public List<Alerta> ListarAlertasAbiertas(int empresaid)
        {
            return _alertaDAL.ListarxEmpresa(empresaid, false);
        }

        public List<Alerta>ListarAlertasResueltas(int empresaid)
        {
            return _alertaDAL.ListarxEmpresa(empresaid, true);
        }

        public void Tomar(int id,int usuarioid,int empresaid)
        {
            var alerta = BuscarxId(id, empresaid);
            if (alerta.EstadoAlerta != TipoEstado.EstadoAlerta.Activa)
            {
                throw new Exception(alerta.EstadoAlerta == TipoEstado.EstadoAlerta.EnGestion
                    ? $"Alerta Esta Siendo Gestionada Por {alerta.UsuarioGestionNombre}"
                    : "Alerta Resuelta");
            }
            if(_alertaDAL.Tomar(id,usuarioid,empresaid)==0)
            {
                throw new Exception("No se Puede tomar alerta,ya la tomo otro usuario");
            }

        }

        public void Resolver(int id,int usuarioid, string? nota,int empresaid)
        {
            var alerta = BuscarxId(id, empresaid);
            if (alerta.EstadoAlerta == TipoEstado.EstadoAlerta.Resuelta)
            {
                throw new Exception("Alerta Resuelta");
            }
            nota=(nota??"").Trim();
            if(nota.Length<10)
            {
                throw new Exception("La nota de resolucion debe tener un minimo de 10 caracteres");
            }

            if (_alertaDAL.Resolver(id, usuarioid, nota, empresaid) == 0)
            {
                throw new Exception("No se pudo resolver la alerta");
            }
        }
        public int ContarAlertasAbiertas(int empresaid)
        {
            return _alertaDAL.ContarAlertasAbiertas(empresaid);
        }
    }
}
