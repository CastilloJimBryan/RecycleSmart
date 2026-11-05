namespace RecycleSmart.EtiquetasColor
{
    public class EstadosColor
    {
        public static string EstadoBadge(string estado)
        {
            return estado switch
            {
                "Activo" or "Sincronizado" or "Autorizado" or "Cerrad" => "bg-success",
                "Pendiente" or "EnCurso" => "bg-warning text-dark",
                "Suspendido" or "Offline" => "bg-secondary",
                "Error" or "Denegado" or "NoConforme" or "Vencida" or "Baja" => "bg-danger",
                _ => "bg-light text-dark"
            };
        }
    }
}
