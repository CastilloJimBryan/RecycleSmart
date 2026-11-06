namespace RecycleSmartBE
{
    public class TipoEstado
    {
        public enum Estados
        {
            Activo, Suspendido, Baja
        }
        public enum EstadoDispositivo { Pendiente, Offline, Activo, Baja }
        public enum EstadoSync { Pendiente, Sincronizando, Error }

        public enum ClasificacionLectura { Normal, Advertencia, Critico }

        public enum TipoAlerta { Nivel, Temperatura, SinConexion, AccesoNoAutorizado }
        public enum Severidad { Advertencia, Critico }
        public enum EstadoAlerta { Activa, EnGestion, Resuelta }

        public enum EstadoAcceso { Permitido,Denegado}
        public enum MotivoAcceso { Autorizado,TarjetaDesconocida,TarjetaVencida,FueraHorario,TarjetaInactiva,ContenedorNoHabilitado}

        public enum MotivoBaja { FallaEquipo, Reemplazo, CierreArea, Otro }

        public enum EstadoLote { EnCurso,RecoleccionSolicitadas,Recolectado,Tratado,Cerrado}

        public enum TipoHito { Generacion,SolicitudRecoleccion,Recoleccion,Tratamiento,DisposicionFinal}
        public enum MetodoTratamiento { Autoclave,Incineracion,Microondas,Otro}
    }
}
