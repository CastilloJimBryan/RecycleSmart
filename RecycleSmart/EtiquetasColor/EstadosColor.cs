namespace RecycleSmart.EtiquetasColor
{
    public class EstadosColor
    {
        public static string EstadoBadge(string estado)
        {
            return estado switch
            {
                "Activo" or "Sincronizado" or "Autorizado" or "Cerrado" or "EnPlazo"=> "bg-success",
                "Pendiente" or "Recolectado" or"Atencion" => "bg-warning text-dark",
                "Suspendido" or "Offline" or "Tratado" => "bg-secondary",
                "RecoleccionSolicitada" => "bg-info text-dark",
                "EnCurso"=>"bg-primary",
                "Error" or "Denegado" or "NoConforme" or "Vencida" or "Baja" or"PorVencer" or "Vencido"=> "bg-danger",
                _ => "bg-light text-dark"
            };
        }
    }
}
