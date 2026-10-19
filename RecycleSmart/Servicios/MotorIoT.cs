using MQTTnet;
using MQTTnet.Client;
using RecycleSmartBE;
using RecycleSmartBLL;
using System.Text;
using System.Text.Json;

namespace RecycleSmart.Servicios
{
    public class MotorIoT : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<MotorIoT> _log;
        private readonly TelemetriaBL _telemetriabl;

        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };
        public MotorIoT(IConfiguration confi,ILogger<MotorIoT>log)
        {
            _config = confi;
            _log = log;
            _telemetriabl = new TelemetriaBL(confi.GetConnectionString("BDTFI")!);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            string host = _config["Mqtt:Host"]!;
            int port = int.Parse(_config["Mqtt:Port"]!);
            string topic = _config["Mqtt:TopicTelemetria"]!;

            var factory = new MqttFactory();
            using var cliente = factory.CreateMqttClient();

            var opciones = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .WithClientId("recyclesmart-motor-" + Guid.NewGuid().ToString("N")[..8])
                .WithCleanSession()
                .Build();

            // Qué hacer con cada mensaje que llega
            cliente.ApplicationMessageReceivedAsync += e =>
            {
                ProcesarMensaje(e.ApplicationMessage.Topic,
                                Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment));
                return Task.CompletedTask;
            };

            // Bucle de conexión: si se corta, reintenta cada 5 segundos
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    if (!cliente.IsConnected)
                    {
                        await cliente.ConnectAsync(opciones, cancellation);
                        await cliente.SubscribeAsync(topic, cancellationToken: cancellation);
                        _log.LogInformation("Motor IoT conectado a {Host}, escuchando {Topic}", host, topic);
                    }
                }
                catch (Exception ex) when (!cancellation.IsCancellationRequested)
                {
                    _log.LogWarning("Motor IoT sin conexión al broker: {Mensaje}. Reintento en 5 s.", ex.Message);
                }

                await Task.Delay(5000, cancellation);
            }
        }
        private void ProcesarMensaje(string topic, string json)
        {
            try
            {
                // recyclesmart/AA:BB:CC:DD:AA:12/telemetria → la MAC es la parte del medio
                var partes = topic.Split('/');
                if (partes.Length != 3)
                    throw new Exception($"Topic inválido: {topic}");

                var lectura = JsonSerializer.Deserialize<LecturaSensor>(json, _json)
                              ?? throw new Exception("El mensaje está vacío.");

                var t = _telemetriabl.ProcesarLectura(partes[1], lectura);
                _log.LogInformation("Lectura {Id}: contenedor {Contenedor}, nivel {Nivel}%",
                                    t.Id, t.ContenedorId, t.Nivel);
            }
            catch (JsonException)
            {
                _log.LogWarning("Mensaje descartado en {Topic}: el contenido no es un JSON válido.", topic);
            }
            catch (Exception ex)
            {
                _log.LogWarning("{Mensaje}", ex.Message);
            }
        }

    }
}
