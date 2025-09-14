using Xunit;
using Moq;
using TaskManager.Controllers;
using ApplicationLayer.Services.TaskServices;
using TaskManager.Services;
using DomainLayer.Models;
using DomainLayer.DTO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace TaskManager.Tests
{
    public class TareasControllerTests
    {
        private readonly Mock<TaskService> _taskServiceMock;
        private readonly Mock<TaskQueueService> _queueMock;
        private readonly Mock<INotificationService> _notifierMock;
        private readonly TareasController _controller;

        public TareasControllerTests()
        {
            _taskServiceMock = new Mock<TaskService>(null, null);
            _queueMock = new Mock<TaskQueueService>();
            _notifierMock = new Mock<INotificationService>();
            _controller = new TareasController(_taskServiceMock.Object, _queueMock.Object, _notifierMock.Object);
        }

        [Fact]
        public async Task GetTaskAllAsync_ReturnsOkResult()
        {
            var mockResponse = new Response<Tarea> { Successful = true, DataList = new List<Tarea>() };
            _taskServiceMock.Setup(s => s.GetTaskAllAsync()).ReturnsAsync(mockResponse);

            var result = await _controller.GetTaskAllAsync();

            Assert.IsType<ActionResult<Response<Tarea>>>(result);
            Assert.True(result.Value.Successful);
        }

        [Fact]
        public async Task GetTaskByIdAllAsync_ExistingId_ReturnsTask()
        {
            var tarea = new Tarea { Id = 1, Descripcion = "Test" };
            var mockResponse = new Response<Tarea> { Successful = true, SingleData = tarea };
            _taskServiceMock.Setup(s => s.GetTaskByIdAllAsync(1)).ReturnsAsync(mockResponse);

            var result = await _controller.GetTaskByIdAllAsync(1);

            Assert.True(result.Value.Successful);
            Assert.Equal("Test", result.Value.SingleData.Descripcion);
        }

        [Fact]
        public async Task GetTaskByIdAllAsync_NonExistingId_ReturnsFailure()
        {
            var mockResponse = new Response<Tarea> { Successful = false };
            _taskServiceMock.Setup(s => s.GetTaskByIdAllAsync(99)).ReturnsAsync(mockResponse);

            var result = await _controller.GetTaskByIdAllAsync(99);

            Assert.False(result.Value.Successful);
        }

        [Fact]
        public async Task AddTaskAllAsync_ValidTask_ReturnsSuccess()
        {
            var tarea = new Tarea { Descripcion = "Nueva tarea", DueData = System.DateTime.Now.AddDays(1) };
            var mockResponse = new Response<string> { Successful = true };
            _taskServiceMock.Setup(s => s.AddTaskAllAsync(tarea)).ReturnsAsync(mockResponse);

            var result = await _controller.AddTaskAllAsync(tarea);

            Assert.True(result.Value.Successful);
        }

        [Fact]
        public async Task AddTaskAllAsync_InvalidTask_ReturnsFailure()
        {
            var tarea = new Tarea { Descripcion = "", DueData = System.DateTime.Now.AddDays(-1) };
            var mockResponse = new Response<string> { Successful = false };
            _taskServiceMock.Setup(s => s.AddTaskAllAsync(tarea)).ReturnsAsync(mockResponse);

            var result = await _controller.AddTaskAllAsync(tarea);

            Assert.False(result.Value.Successful);
        }

        [Fact]
        public async Task UpdateTaskAllAsync_ReturnsSuccess()
        {
            var tarea = new Tarea { Id = 1, Descripcion = "Actualizar tarea" };
            var mockResponse = new Response<string> { Successful = true };
            _taskServiceMock.Setup(s => s.UpdateTaskAllAsync(tarea)).ReturnsAsync(mockResponse);

            var result = await _controller.UpdateTaskAllAsync(tarea);

            Assert.True(result.Value.Successful);
        }

        [Fact]
        public async Task DeleteTaskAllAsync_ExistingId_ReturnsSuccess()
        {
            var mockResponse = new Response<string> { Successful = true };
            _taskServiceMock.Setup(s => s.DeleteTaskAllAsync(1)).ReturnsAsync(mockResponse);

            var result = await _controller.DeleteTaskAllAsync(1);

            Assert.True(result.Value.Successful);
        }

        [Fact]
        public async Task GetPendingTasksAsync_ReturnsPendingTasks()
        {
            var tareas = new List<Tarea> { new Tarea { Descripcion = "Pendiente", Status = "pendiente" } };
            var mockResponse = new Response<Tarea> { Successful = true, DataList = tareas };
            _taskServiceMock.Setup(s => s.GetPendingTasksAsync()).ReturnsAsync(mockResponse);

            var result = await _controller.GetPendingTasksAsync();

            Assert.True(result.Value.Successful);
            Assert.Single(result.Value.DataList);
        }

        [Fact]
        public async Task AddHighPriorityTask_ReturnsSuccess()
        {
            var mockResponse = new Response<string> { Successful = true };
            _taskServiceMock.Setup(s => s.AddHighPriorityTaskAsync("Urgente")).ReturnsAsync(mockResponse);

            var result = await _controller.AddHighPriorityTask("Urgente");

            Assert.True(result.Value.Successful);
        }

        [Fact]
        public async Task AddLowPriorityTask_ReturnsSuccess()
        {
            var mockResponse = new Response<string> { Successful = true };
            _taskServiceMock.Setup(s => s.AddLowPriorityTaskAsync("Baja")).ReturnsAsync(mockResponse);

            var result = await _controller.AddLowPriorityTask("Baja");

            Assert.True(result.Value.Successful);
        }
    }
}
