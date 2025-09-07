using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TaskManager.Hubs
{
    public class TasksHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", "Conectado al TasksHub!");
            await base.OnConnectedAsync();
        }
    }
}
