using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using DomainLayer.Models;
using InfrastructureLayer.Repositorio.Commons;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationLayer.Services.TaskServices
{
    // Tipos de acción que soporta la cola
    internal enum TaskActionType { Add, Update, Delete }

    // Paquete que se encola
    internal sealed class QueuedTaskAction
    {
        public TaskActionType Type { get; init; }
        public Tarea? Task { get; init; }
        public int? Id { get; init; }
        public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
    }

  
    public sealed class TaskQueueService : IDisposable
    {
        private readonly IServiceProvider _provider;
      
        private readonly Subject<QueuedTaskAction> _stream;
        private readonly IDisposable _subscription;

   
        private readonly int _processingDelayMs = 0;

  
        private int _pendingCount = 0;
        private int _processedCount = 0;
        private volatile bool _isProcessing = false;

        public TaskQueueService(IServiceProvider provider)
        {
            _provider = provider;

            _stream = new Subject<QueuedTaskAction>();

            _subscription = _stream
                .Synchronize() 
                .Select(action => Observable.FromAsync(() => ProcessAsync(action)))
                .Concat()  
                .Subscribe(
                    _ => { },
                    ex => Console.WriteLine($"[QUEUE ERROR] {ex}"));
        }

        public void EnqueueAdd(Tarea task)
        {
            Interlocked.Increment(ref _pendingCount);
            _stream.OnNext(new QueuedTaskAction { Type = TaskActionType.Add, Task = task });
        }

        public void EnqueueUpdate(Tarea task)
        {
            Interlocked.Increment(ref _pendingCount);
            _stream.OnNext(new QueuedTaskAction { Type = TaskActionType.Update, Task = task });
        }

        public void EnqueueDelete(int id)
        {
            Interlocked.Increment(ref _pendingCount);
            _stream.OnNext(new QueuedTaskAction { Type = TaskActionType.Delete, Id = id });
        }

        private async Task ProcessAsync(QueuedTaskAction action)
        {
            _isProcessing = true;
            try
            {
                using var scope = _provider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ICommonsProces<Tarea>>();

                switch (action.Type)
                {
                    case TaskActionType.Add:
                        if (action.Task is null) return;
                        Console.WriteLine($"⏳ [ADD] {action.Task.Descripcion}");
                        if (_processingDelayMs > 0) await Task.Delay(_processingDelayMs);
                        await repo.AddAsync(action.Task);
                        Console.WriteLine("✅ [ADD] OK");
                        break;

                    case TaskActionType.Update:
                        if (action.Task is null) return;
                        Console.WriteLine($"⏳ [UPDATE] {action.Task.Descripcion}");
                        if (_processingDelayMs > 0) await Task.Delay(_processingDelayMs);
                        await repo.UpdateAsync(action.Task);
                        Console.WriteLine("✅ [UPDATE] OK");
                        break;

                    case TaskActionType.Delete:
                        if (!action.Id.HasValue) return;
                        Console.WriteLine($"⏳ [DELETE] Id={action.Id.Value}");
                        if (_processingDelayMs > 0) await Task.Delay(_processingDelayMs);
                        await repo.DeleteAsync(action.Id.Value);
                        Console.WriteLine("✅ [DELETE] OK");
                        break;
                }
            }
            finally
            {
                Interlocked.Decrement(ref _pendingCount);
                Interlocked.Increment(ref _processedCount);
                _isProcessing = false;
            }
        }

        public object GetStatus()
        {
            return new
            {
                HasObservers = _stream.HasObservers,
                IsProcessing = _isProcessing,
                PendingCount = Math.Max(0, _pendingCount),
                ProcessedCount = Math.Max(0, _processedCount),
                Info = "Cola Rx.NET procesando en FIFO y sin solaparse."
            };
        }

        public void Dispose() => _subscription.Dispose();
    }
}
