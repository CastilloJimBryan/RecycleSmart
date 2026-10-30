using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using RecycleSmart.HUB;
using RecycleSmartBE;
using RecycleSmartBLL;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace RecycleSmart.Servicios
{
    public class MotorIoT : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MotorIoT> _log;
        private readonly TelemetriaBL _telemetriabl;
        private readonly AlertaBL _alerta;

        private IMqttClient? _mqttClient;
        private readonly EventoAccesoBL _eventoAcceso;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
        private readonly IHubContext<MonitoreoHubs> _hub;
        public MotorIoT(IConfiguration confi,ILogger<MotorIoT>log,IHubContext<MonitoreoHubs>hub)
        {
            _config = confi;
            _log = log;
            _telemetriabl = new TelemetriaBL(confi.GetConnectionString("BDTFI")!);
            _hub = hub;
            _alerta = new AlertaBL(confi.GetConnectionString("BDTFI")!);
            _eventoAcceso = new EventoAccesoBL(confi.GetConnectionString("BDTFI")!);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            string host = _config["Mqtt:Host"]!;
            int port = int.Parse(_config["Mqtt:Port"]!);
            string topic = _config["Mqtt:TopicTelemetria"]!;
            string topicrfid = _config["Mqtt:TopicRFID"]!;

            var factory = new MqttFactory();
            using var cliente = factory.CreateMqttClient();
            _mqttClient = cliente;

            var opciones = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId("recyclesmart-motor-" + Guid.NewGuid().ToString("N")[..8])
                .WithCleanSession()
                .Build();

            // Qué hacer con cada mensaje que llega
            cliente.ApplicationMessageReceivedAsync += e => ProcesarMensaje(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
            var ultimaRevision = DateTime.MinValue;
            // Bucle de conexión: si se corta, reintenta cada 5 segundos
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    if (!cliente.IsConnected)
                    {
                        await cliente.ConnectAsync(opciones, cancellation);

                        var suscripcion=factory.CreateSubscribeOptionsBuilder()
                            .WithTopicFilter(topic).WithTopicFilter(topicrfid).Build();

                        await cliente.SubscribeAsync(suscripcion,cancellation);
                        _log.LogInformation("Motor IoT conectado a {Host}, escuchando {Topic} y {TopicRFID}", host, topic,topicrfid);
                    }
                }
                catch (Exception ex) when (!cancellation.IsCancellationRequested)
                {
                    _log.LogWarning("Motor IoT sin conexión al broker: {Mensaje}. Reintento en 5 s.", ex.Message);
                }
                if((DateTime.Now-ultimaRevision).TotalSeconds>=60)
                {
                    ultimaRevision= DateTime.Now;
                    try
                    {
                        foreach (var (empresaId, alertas) in _telemetriabl.RevisarSensoresSinConexion())
                            await AvisarAlertas(alertas, empresaId);
                    }
                    catch(Exception ex)
                    {
                        _log.LogWarning(ex, "Error al revisar sensores sin conexión: {Mensaje}", ex.Message);
                    }
                }
                else
                {

                }
                await Task.Delay(5000, cancellation);
            }
        }
        private async Task ProcesarMensaje(string topic, string json)
        {
            try
            {
                // recyclesmart/{mac}/{tipo}
                var partes = topic.Split('/');
                if (partes.Length != 3)
                    throw new Exception($"Topic inválido: {topic}");

                string mac = partes[1];
                switch (partes[2])
                {
                    case "telemetria":
                        await ProcesarTelemetria(mac, json);
                        break;
                    case "rfid":
                        await ProcesarRFID(mac, json);
                        break;
                    default:
                        _log.LogWarning("Topic no reconocido: {Topic}", topic);
                        break;
                }
            }
            catch (JsonException)
            {
                _log.LogWarning("Mensaje descartado en {Topic}: el contenido no es un JSON válido.", topic);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "{Mensaje}", ex.Message);
            }


        }
        private  async Task AvisarAlertas(List<Alerta> alertaList, int empresaId)
        {
            string grupo = $"empresa-{empresaId}";

            foreach (var a in alertaList)
            {

                _log.LogWarning("ALERTA {Severidad} ({Tipo}) en contenedor {Contenedor}: {Mensaje}",
                                    a.TipoSeveridad, a.TipoAlerta, a.ContenedorId, a.Mensaje);
                var completa = _alerta.BuscarxId(a.Id, empresaId);
                if (completa == null) continue;
                await _hub.Clients.Group(grupo).SendAsync("NuevaAlerta", new
                {
                    id = completa.Id,
                    severidad = completa.TipoSeveridad.ToString(),
                    tipo = completa.TipoAlerta.ToString(),
                    mensaje = completa.Mensaje,
                    contenedor = completa.ContenedorNombre,
                    institucion = completa.InstitucionNombre
                });
            }
            await _hub.Clients.Group(grupo).SendAsync("AlertasAbiertas", _alerta.ContarAlertasAbiertas(empresaId));
        }
        private async Task ProcesarTelemetria(string mac,string json)
        {
            var lectura = JsonSerializer.Deserialize<LecturaSensor>(json, _json)
                              ?? throw new Exception("El mensaje está vacío.");

            var t = _telemetriabl.ProcesarLectura(mac, lectura);
            _log.LogInformation("Lectura {Id}: contenedor {Contenedor}, nivel {Nivel}%", t.Id, t.ContenedorId, t.Nivel);
            string grupo = $"empresa-{t.EmpresaLogisticaId}";
            await _hub.Clients.Group(grupo).SendAsync("NuevaLectura", new
            {
                contenedorId = t.ContenedorId,
                nivel = t.Nivel,
                temperatura = t.Temperatura.ToString("0.0"),
                fecha = t.FechaHora.ToString("dd/MM HH:mm"),
                clase = t.Clasificacion.ToString()
            });
            if(t.CompletoAcceso)
            {
                await _hub.Clients.Group(grupo).SendAsync("NuevoAcceso", new { ContenedorId = t.ContenedorId });
            }
            await AvisarAlertas(t.ALertasNuevas, t.EmpresaLogisticaId);
        }
        private async Task ProcesarRFID(string mac,string json)
        {
            var lectura = JsonSerializer.Deserialize<LecturaRFID>(json, _json)
              ?? throw new Exception("El mensaje está vacío.");

            var e = _eventoAcceso.ProcesarAcceso(mac, lectura);
            _log.LogInformation("Acceso {Resultado} en contenedor {Contenedor}: {Motivo} (UID {Uid})",
                                e.EstadoAcceso, e.ContenedorId, e.Motivo, e.UIDLeido);

            // 1) Responderle al lector: abrir (LED verde) o denegar (LED rojo)
            var respuesta = JsonSerializer.Serialize(new
            {
                acceso = e.Permitido,
                motivo = e.Motivo.ToString(),
                usuario = e.Permitido ? e.UsuarioNombre : null
            });
            var mensaje = new MqttApplicationMessageBuilder()
                .WithTopic($"recyclesmart/{mac}/rfid/respuesta")
                .WithPayload(respuesta)
                .Build();
            if (_mqttClient != null && _mqttClient.IsConnected)
                await _mqttClient.PublishAsync(mensaje);

            // 2) Avisar a la pantalla
            await _hub.Clients.Group($"empresa-{e.EmpresaLogisticaId}").SendAsync("NuevoAcceso", new
            {
                contenedorId = e.ContenedorId,
                permitido = e.Permitido,
                motivo = AlertaBL.MotivoText(e.Motivo),
                usuario = e.UsuarioNombre,
                uid = e.UIDLeido,
                fecha = e.Fecha.ToString("dd/MM HH:mm:ss")
            });

            // 3) Si se denegó y se creó o escaló la alerta, aviso emergente y contador
            await AvisarAlertas(e.AlertasNuevas, e.EmpresaLogisticaId);
        }
    }
}
