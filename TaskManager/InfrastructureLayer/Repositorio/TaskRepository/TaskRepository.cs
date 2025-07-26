using InfrastructureLayer.Repositorio.Commons;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using InfrastructureLayer.Context;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositorio.TaskRepository
{
    public class TaskRepository : ICommonsProces<Tarea>
    {
        private readonly TaskManagerContext _context;
        public TaskRepository(TaskManagerContext taskManagerContext)
        {
            _context = taskManagerContext;
        }

        public async Task<IEnumerable<Tarea>> GetAllAsync()
            => await _context.Tarea.ToListAsync();

        public async Task<Tarea> GetIdAsync(int id)
        => await _context.Tarea.FirstOrDefaultAsync(x => x.Id ==id);

        public async Task<(bool IsSuccess, string Message)> AddAsync(Tarea entry)
        {
            try {
                var exists =  _context.Tarea.Any(x => x.Descripcion == entry.Descripcion);
                if(exists)
                {
                    return (false, "Ya esiste una tarea con ese nombre...");
                }
                await _context.Tarea.AddAsync(entry);
                await _context.SaveChangesAsync();
                return (true,"La tarea se guardo correctamente");
            }
            catch (Exception)
            {
                return (false, "No se puedo guardar la tarea");
            }
            
        }
        public async Task<(bool IsSuccess, string Message)> UpdateAsync(Tarea entry)
        {
            try
            {
                 _context.Tarea.Update(entry);
                await _context.SaveChangesAsync();
                return (true, "La tarea se actualizo correctamente");
            }
            catch (Exception)
            {
                return (false, "No se puedo actualizar la tarea");
            }
        }

        public async Task<(bool IsSuccess, string Message)> DeleteAsync(int id)
        {
            try
            {
                var tarea = await _context.Tarea.FindAsync(id);
                if (tarea != null)
                {
                    _context.Tarea.Remove(tarea);
                    await _context.SaveChangesAsync();
                    return (true, "La tarea elimino correctamente");
                }
                else
                {
                    return (false, "No se encontro la tarea");
                }
            }
            catch (Exception)
            {
                return (false, "No se pudo eliminar la tarea");
            }
            
          
        }

    }
}
