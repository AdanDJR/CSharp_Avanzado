using DomainLayer.Factories; // 🔹
using DomainLayer.DTO;
using DomainLayer.Models;
using InfrastructureLayer.Repositorio.Commons;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.TaskServices
{
    public class TaskService
    {
        private readonly ICommonsProces<Tarea> _commonsProces; // Lecturas directas
        private readonly TaskQueueService _queue;               // Escrituras en cola

        public TaskService(ICommonsProces<Tarea> commonsProces, TaskQueueService queue)
        {
            _commonsProces = commonsProces;
            _queue = queue;
        }

        // Delegado
        private delegate bool ValidateTask(Tarea tarea);

        // Action 
        private Action<Tarea> notifyCreation = tarea =>
            Console.WriteLine($"Tarea creada (encolada): {tarea.Descripcion}, vencimiento: {tarea.DueData}");

        // Func 
        private Func<Tarea, int> calculateDaysLeft = tarea =>
            (tarea.DueData - DateTime.Now).Days;


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
                    response.Message = "No se encontro informacion";
                }
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
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
                response.Successful = true;
                response.Message = $"Eliminación de la tarea Id={id} encolada.";
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
            try
            {
                var allTasks = await _commonsProces.GetAllAsync();

                response.DataList = allTasks
                    .Where(t => !string.IsNullOrWhiteSpace(t.Status) &&
                                t.Status.Trim().ToLower() == "pendiente")
                    .ToList();

                response.Successful = true;
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
