using System;
using DomainLayer.Models;

namespace DomainLayer.Factories
{
    public static class TareaFactory
    {
        public static Tarea CreateHighPriorityTask(string descripcion)
        {
            return new Tarea
            {
                Descripcion = descripcion,
                DueData = DateTime.Now.AddDays(1), 
                Status = "Pendiente",
                AddicionalData = "Alta Prioridad"
            };
        }

        public static Tarea CreateLowPriorityTask(string descripcion)
        {
            return new Tarea
            {
                Descripcion = descripcion,
                DueData = DateTime.Now.AddDays(7), 
                Status = "Pendiente",
                AddicionalData = "Baja Prioridad"
            };
        }

        public static Tarea CreateCustomTask(string descripcion, DateTime dueDate, string status, string additionalData)
        {
            return new Tarea
            {
                Descripcion = descripcion,
                DueData = dueDate,
                Status = status,
                AddicionalData = additionalData
            };
        }
    }
}
