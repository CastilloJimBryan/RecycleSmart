namespace RecycleSmart.EtiquetasColor
{
    public class EstadosColor
    {
        public static string EstadoBadge(string estado)
        {
            return estado switch
            {
                "Activo" or "Sincronizado" or "Autorizado" or "Cerrado" or "EnPlazo" or "Pagada" or "Al día" => "bg-success",
                "Suspendido" or"Pendiente" or "Recolectado" or"Atencion" or "Vencida" => "bg-warning text-dark",
                "Offline" or "Tratado" => "bg-secondary",
                "RecoleccionSolicitada" => "bg-info text-dark",
                "EnCurso"=>"bg-primary",
                "Error" or "Denegado" or "NoConforme" or "Vencida" or "Baja" or"PorVencer" or "MoraGrave" or"EnMora" or "Facturas Vencidas" => "bg-danger",
                "Vencido"=> "bg-dark-red text-white",
                _ => "bg-light text-dark"
            };
        }

    }
}
