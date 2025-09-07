using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DomainLayer.Models;
using TaskManager.Hubs;

namespace TaskManager.Services
{
    public interface INotificationService
    {
        Task TaskCreated(Tarea tarea);
    }

    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<TasksHub> _hubContext;

        public SignalRNotificationService(IHubContext<TasksHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task TaskCreated(Tarea tarea)
        {
            await _hubContext.Clients.All.SendAsync("TaskCreated", new
            {
                tarea.Id,
                tarea.Descripcion,
                tarea.DueData,
                tarea.Status,
                tarea.AddicionalData
            });
        }
    }
}
