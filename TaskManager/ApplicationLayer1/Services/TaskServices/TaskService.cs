using DomainLayer.DTO;
using DomainLayer.Models;
using InfrastructureLayer.Repositorio.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                if(result != null)
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
                var resul = await _commonsProces.AddAsync(tarea);
                response.Message = resul.Message;
                response.Successful = resul.IsSuccess;
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
    }
}
