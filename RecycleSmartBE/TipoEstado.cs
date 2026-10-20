namespace RecycleSmartBE
{
    public class TipoEstado
    {
        public enum Estados
        {
            Activo,Suspendido,Baja
        }
        public enum EstadoDispositivo { Pendiente,Offline,Activo,Baja }
        public enum EstadoSync { Pendiente,Sincronizado,Error }

        public enum ClasificacionLectura { Normal,Advertencia,Critico}

        public enum TipoAlerta { Nivel,Temperatura,SinConexion}
        public enum Severidad { Advertencia,Critico}
        public enum EstadoAlerta { Activa,EnGestion,Resuelta}
    }
}
