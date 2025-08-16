using ApplicationLayer.Services.TaskServices;
using DomainLayer.DTO;
using DomainLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TareasController : ControllerBase
    {
        private readonly TaskService _service;
        private readonly TaskQueueService _queue;

        public TareasController(TaskService service, TaskQueueService queue)
        {
            _service = service;
            _queue = queue;
        }

        [HttpGet]
        public async Task<ActionResult<Response<Tarea>>> GetTaskAllAsync()
            => await _service.GetTaskAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Response<Tarea>>> GetTaskByIdAllAsync(int id)
            => await _service.GetTaskByIdAllAsync(id);

        [HttpPost]
        public async Task<ActionResult<Response<string>>> AddTaskAllAsync(Tarea tarea)
            => await _service.AddTaskAllAsync(tarea);

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
            => await _service.AddHighPriorityTaskAsync(descripcion);

        [HttpPost("baja-prioridad")]
        public async Task<ActionResult<Response<string>>> AddLowPriorityTask(string descripcion)
            => await _service.AddLowPriorityTaskAsync(descripcion);

        // 🚀 Estado de la cola (para pruebas/diagnóstico)
        [HttpGet("cola/estado")]
        public IActionResult GetQueueStatus()
        {
            var status = _queue.GetStatus();
            return Ok(status);
        }
    }
}
