using DomainLayer.DTO;
using DomainLayer.Models;
using InfrastructureLayer.Repositorio.Commons;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.TaskServices
{
    public class TaskService
    {
        private readonly ICommonsProces<Tarea> _commonsProces;

        public TaskService(ICommonsProces<Tarea> commonsProces)
        {
            _commonsProces = commonsProces;
        }

        // Delegado
        private delegate bool ValidateTask(Tarea tarea);

        // Action 
        private Action<Tarea> notifyCreation = tarea =>
            Console.WriteLine($"Tarea creada: {tarea.Descripcion}, vencimiento: {tarea.DueData}");

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

                var resul = await _commonsProces.AddAsync(tarea);

                response.Message = resul.Message;
                response.Successful = resul.IsSuccess;

                int daysLeft = calculateDaysLeft(tarea);
                Console.WriteLine($"Días restantes para completar la tarea: {daysLeft}");
            }
            catch (Exception e)
            {
                response.Errors.Add(e.Message);
            }
            return response;
        }

        public async Task<Response<string>> UpdateTaskAllAsync(Tarea tarea)
        {
            var response = new Response<string>();
            try
            {
                var resul = await _commonsProces.UpdateAsync(tarea);
                response.Message = resul.Message;
                response.Successful = resul.IsSuccess;
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
                var resul = await _commonsProces.DeleteAsync(id);
                response.Message = resul.Message;
                response.Successful = resul.IsSuccess;
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



