using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RecycleSmart.HUB
{
    public class MonitoreoHubs:Hub
    {
        [Authorize]


        public override async Task OnConnectedAsync()
        {
            int empresaId = Servicio.ManejosSesiones.ObtenerEmpresaLogisticaId(Context.User!);
            if(empresaId >0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"empresa-{empresaId}");
            }
            await base.OnConnectedAsync();
        }

    }
}
