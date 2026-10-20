using Microsoft.AspNetCore.SignalR;

namespace RecycleSmart.HUB
{
    public class MonitoreoHubs:Hub
    {
        public override async Task OnConnectedAsync()
        {
            int empresaId = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(Context.User!);
            if(empresaId >0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Empresa-{empresaId}");
            }
            await base.OnConnectedAsync();
        }

    }
}
