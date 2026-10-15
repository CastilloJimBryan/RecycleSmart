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
    }
}
