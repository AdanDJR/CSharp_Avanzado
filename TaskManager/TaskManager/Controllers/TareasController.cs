using ApplicationLayer.Services.TaskServices;
using DomainLayer.DTO;
using DomainLayer.Models;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Services; // 👈 Para INotificationService

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly TaskService _service;
        private readonly TaskQueueService _queue;
        private readonly INotificationService _notifier;

        // Inyectamos TaskService, TaskQueueService y el servicio de notificación
        public TareasController(TaskService service, TaskQueueService queue, INotificationService notifier)
        {
            _service = service;
            _queue = queue;
            _notifier = notifier;
        }

        [HttpGet]
        public async Task<ActionResult<Response<Tarea>>> GetTaskAllAsync()
            => await _service.GetTaskAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<Tarea>>> GetTaskByIdAllAsync(int id)
            => await _service.GetTaskByIdAllAsync(id);

        [HttpPost]
        public async Task<ActionResult<Response<string>>> AddTaskAllAsync(Tarea tarea)
        {
            var result = await _service.AddTaskAllAsync(tarea);

            if (result.Successful)
            {
                // 🔔 Notificar a todos los clientes conectados
                await _notifier.TaskCreated(tarea);
            }

            return result;
        }

        [HttpPut]
        public async Task<ActionResult<Response<string>>> UpdateTaskAllAsync(Tarea tarea)
            => await _service.UpdateTaskAllAsync(tarea);

        [HttpDelete("{id}")]
        public async Task<ActionResult<Response<string>>> DeleteTaskAllAsync(int id)
            => await _service.DeleteTaskAllAsync(id);

        [HttpGet("pendientes")]
        public async Task<ActionResult<Response<Tarea>>> GetPendingTasksAsync()
            => await _service.GetPendingTasksAsync();

        [HttpPost("alta-prioridad")]
        public async Task<ActionResult<Response<string>>> AddHighPriorityTask(string descripcion)
        {
            var result = await _service.AddHighPriorityTaskAsync(descripcion);

            if (result.Successful)
            {
                // 🔔 Notificar a todos los clientes conectados
                await _notifier.TaskCreated(new Tarea { Descripcion = descripcion, Status = "Alta prioridad", DueData = DateTime.Now.AddDays(1) });
            }

            return result;
        }

        [HttpPost("baja-prioridad")]
        public async Task<ActionResult<Response<string>>> AddLowPriorityTask(string descripcion)
        {
            var result = await _service.AddLowPriorityTaskAsync(descripcion);

            if (result.Successful)
            {
                // 🔔 Notificar a todos los clientes conectados
                await _notifier.TaskCreated(new Tarea { Descripcion = descripcion, Status = "Baja prioridad", DueData = DateTime.Now.AddDays(1) });
            }

            return result;
        }

        // 🚀 Estado de la cola (para pruebas/diagnóstico)
        [HttpGet("cola/estado")]
        public IActionResult GetQueueStatus()
        {
            var status = _queue.GetStatus();
            return Ok(status);
        }
    }
}
