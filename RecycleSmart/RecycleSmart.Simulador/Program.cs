using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;

// ===== Configuración: poné tus MAC y una tarjeta asignada =====
var sensores = new List<Sensor>
{
    new Sensor { Mac = "AA:BB:CC:DD:AA:12", Nivel = 20 },
    // new Sensor { Mac = "AA:BB:CC:DD:AA:13", Nivel = 50 },
};
string uidTarjeta = "12345678";
int segundosEntreLecturas = 3;
// ================================tiempo====son 10 segundos pero para el ejemplo es de 3 seg ==========================

var rnd = new Random();
var factory = new MqttFactory();
var cliente = factory.CreateMqttClient();
var opciones = new MqttClientOptionsBuilder()
    .WithTcpServer("broker.hivemq.com", 1883)
    .WithClientId("recyclesmart-sim-" + Guid.NewGuid().ToString("N")[..8])
    .Build();

// Muestra lo que responde el servidor cuando se pasa una tarjeta
cliente.ApplicationMessageReceivedAsync += e =>
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"  ← Respuesta lector: {e.ApplicationMessage.ConvertPayloadToString()}");
    Console.ResetColor();
    return Task.CompletedTask;
};

await cliente.ConnectAsync(opciones);
await cliente.SubscribeAsync(factory.CreateSubscribeOptionsBuilder()
    .WithTopicFilter("recyclesmart/+/rfid/respuesta").Build());

Console.WriteLine("Simulador conectado.... Teclas:");
Console.WriteLine("  1-9 elegir sensor | V vaciar | T calor on/off | C cortar/reconectar | R pasar tarjeta | Q salir");

int elegido = 0;
var proxima = DateTime.MinValue;

while (true)
{
    // ---- Teclas ----
    if (Console.KeyAvailable)
    {
        var tecla = Console.ReadKey(true).KeyChar;
        var s = sensores[elegido];

        if (char.IsDigit(tecla) && tecla != '0' && tecla - '1' < sensores.Count)
        {
            elegido = tecla - '1';
            Console.WriteLine($"Sensor elegido: {sensores[elegido].Mac}");
        }
        else switch (char.ToUpper(tecla))
            {
                case 'V':
                    s.Nivel = rnd.Next(0, 5);
                    Console.WriteLine($"[{s.Mac}] Vaciado");
                    break;
                case 'T':
                    s.Calor = !s.Calor;
                    Console.WriteLine($"[{s.Mac}] Calor {(s.Calor ? "ACTIVADO" : "Desactivado")}");
                    break;
                case 'C':
                    s.Conectado = !s.Conectado;
                    Console.WriteLine($"[{s.Mac}] {(s.Conectado ? "Reconectado" : "SIN CONEXIÓN (deja de publicar)")}");
                    break;
                case 'R':
                    await Publicar($"recyclesmart/{s.Mac}/rfid", new { Uid = uidTarjeta });
                    s.Nivel = Math.Min(100, s.Nivel + rnd.Next(3, 9)); // el usuario deposita residuos
                    Console.WriteLine($"[{s.Mac}] Tarjeta {uidTarjeta} pasada");
                    break;
                case 'Q':
                    await cliente.DisconnectAsync();
                    return;
            }
    }

    // ---- Lecturas periódicas ----
    if (DateTime.Now >= proxima)
    {
        proxima = DateTime.Now.AddSeconds(segundosEntreLecturas);

        foreach (var s in sensores.Where(x => x.Conectado))
        {
            s.Avanzar(rnd);
            await Publicar($"recyclesmart/{s.Mac}/telemetria", new
            {
                Nivel = (int)Math.Round(s.Nivel),
                Temperatura = Math.Round(s.Temperatura, 1),
                Humedad = (int)Math.Round(s.Humedad),
                Bateria = (int)Math.Round(s.Bateria),
                NivelConexion = s.NivelConexion
            });
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {s.Mac} → nivel {s.Nivel:0}% · {s.Temperatura:0.0}°C ·hum {s.Humedad:0}%· bat {s.Bateria:0}% · señal {s.NivelConexion}");
        }
    }

    await Task.Delay(200);
}
// Publica un objeto como JSON; si se cortó la conexión con el broker, reconecta
async Task Publicar(string topic, object datos)
{
    try
    {
        if (!cliente.IsConnected)
            await cliente.ConnectAsync(opciones);

        var mensaje = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonSerializer.Serialize(datos))
            .Build();
        await cliente.PublishAsync(mensaje);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al publicar: {ex.Message}");
    }
}

class Sensor
{
    public string Mac { get; set; } = "";
    public double Nivel { get; set; }
    public int LecturaLLeno {  get; set; }
    public double Temperatura { get; set; } = 22;
    public double Humedad { get; set; } = 55;
    public double Bateria { get; set; } = 100;
    public int NivelConexion { get; set; } = -60;
    public bool Calor { get; set; }
    public bool Conectado { get; set; } = true;

    // Un "paso" de simulación: cómo cambian los valores entre una lectura y la siguiente
    public void Avanzar(Random rnd)
    {
        if (Nivel >= 100)
        {
            LecturaLLeno++;
            if(LecturaLLeno>=6)
            {
                Nivel = rnd.Next(0, 5);
                LecturaLLeno = 0;
            }
        }
        else
        {
            Nivel = Math.Min(100, Nivel + rnd.NextDouble() * 3);
        }
        // Temperatura: se acerca al objetivo y oscila alrededor (±0,5 °C)
        double objetivo = Calor ? 45 : 22;
        Temperatura += (objetivo - Temperatura) * 0.15 + (rnd.NextDouble() - 0.5);

        Humedad = Math.Clamp(Humedad + (rnd.NextDouble() - 0.5) * 4, 40, 75);
        Bateria = Math.Max(0, Bateria - 0.05);
        NivelConexion = rnd.Next(-75, -54);
    }
}