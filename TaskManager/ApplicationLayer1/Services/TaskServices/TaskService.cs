using DomainLayer.Factories;
using DomainLayer.DTO;
using DomainLayer.Models;
using InfrastructureLayer.Repositorio.Commons;
using System.Collections.Concurrent;

namespace ApplicationLayer.Services.TaskServices
{
    public class TaskService
    {
        private readonly ICommonsProces<Tarea> _commonsProces;
        private readonly TaskQueueService _queue;
        private readonly ConcurrentDictionary<string, object> _cache = new();

        private delegate bool ValidateTask(Tarea tarea);
        private Action<Tarea> notifyCreation = tarea =>
            Console.WriteLine($"Tarea creada (encolada): {tarea.Descripcion}, vencimiento: {tarea.DueData}");
        private Func<Tarea, int> calculateDaysLeft = tarea =>
            (tarea.DueData - DateTime.Now).Days;

        private void InvalidateCache()
        {
            _cache.Clear();
        }

        public TaskService(ICommonsProces<Tarea> commonsProces, TaskQueueService queue)
        {
            _commonsProces = commonsProces;
            _queue = queue;
        }

        public async Task<Response<string>> AddTaskAllAsync(Tarea tarea)
        {
            var response = new Response<string>();
            try
            {
                ValidateTask validate = t =>
                    !string.IsNullOrWhiteSpace(t.Descripcion) && t.DueData > DateTime.Now;

                if (!validate(tarea))
                {
                    response.Successful = false;
                    response.Message = "La tarea no es válida: descripción vacía o fecha de vencimiento inválida.";
                    return response;
                }

                notifyCreation(tarea);
                _queue.EnqueueAdd(tarea);
                InvalidateCache();

                int daysLeft = calculateDaysLeft(tarea);
                Console.WriteLine($"Días restantes estimados: {daysLeft}");

                response.Successful = true;
                response.Message = $"Tarea '{tarea.Descripcion}' encolada para procesamiento secuencial.";
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<string>> AddHighPriorityTaskAsync(string descripcion)
        {
            var tarea = TareaFactory.CreateHighPriorityTask(descripcion);
            return await AddTaskAllAsync(tarea);
        }

        public async Task<Response<string>> AddLowPriorityTaskAsync(string descripcion)
        {
            var tarea = TareaFactory.CreateLowPriorityTask(descripcion);
            return await AddTaskAllAsync(tarea);
        }

        public async Task<Response<string>> UpdateTaskAllAsync(Tarea tarea)
        {
            var response = new Response<string>();
            try
            {
                _queue.EnqueueUpdate(tarea);
                InvalidateCache();
                response.Successful = true;
                response.Message = $"Actualización de la tarea '{tarea.Descripcion}' encolada.";
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<string>> DeleteTaskAllAsync(int id)
        {
            var response = new Response<string>();
            try
            {
                _queue.EnqueueDelete(id);
                InvalidateCache();
                response.Successful = true;
                response.Message = $"Eliminación de la tarea Id={id} encolada.";
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<Tarea>> GetTaskAllAsync()
        {
            var response = new Response<Tarea>();
            try
            {
                response.DataList = await _commonsProces.GetAllAsync();
                response.Successful = true;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<Tarea>> GetTaskByIdAllAsync(int id)
        {
            var response = new Response<Tarea>();
            try
            {
                var result = await _commonsProces.GetIdAsync(id);
                if (result != null)
                {
                    response.SingleData = result;
                    response.Successful = true;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No se encontró información";
                }
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<Tarea>> GetPendingTasksAsync()
        {
            var response = new Response<Tarea>();
            const string cacheKey = "pendingTasks";

            try
            {
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    response.DataList = new List<Tarea>((List<Tarea>)cached);
                    response.Successful = true;
                    return response;
                }

                var allTasks = await _commonsProces.GetAllAsync();

                var pendingTasks = allTasks
                    .Where(t => !string.IsNullOrWhiteSpace(t.Status) &&
                                t.Status.Trim().ToLower() == "pendiente")
                    .ToList();

                response.DataList = pendingTasks;
                response.Successful = true;

                _cache[cacheKey] = pendingTasks;
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
                response.Successful = false;
            }

            return response;
        }
    }
}
