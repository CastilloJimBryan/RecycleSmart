namespace RecycleSmart.EtiquetasColor
{
    public class EstadosColor
    {
        public static string EstadoBadge(string estado)
        {
            return estado switch
            {
                "Activo" or "Sincronizado" or "Autorizado" or "Cerrado" => "bg-success",
                "Pendiente" or "EnCurso" or "Recolectado" => "bg-warning text-dark",
                "Suspendido" or "Offline" or "Tratado" => "bg-secondary",
                "RecoleccionSolicitada" => "bg-info text-dark",
                "Error" or "Denegado" or "NoConforme" or "Vencida" or "Baja" => "bg-danger",
                _ => "bg-light text-dark"
            };
        }
    }
}
