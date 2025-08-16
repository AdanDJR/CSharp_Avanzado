using InfrastructureLayer.Repositorio.TaskRepository;
using InfrastructureLayer;
using DomainLayer.Models;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositorio.Commons;
using Microsoft.EntityFrameworkCore;
using ApplicationLayer.Services.TaskServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskManagerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerDB"));
});

// Repositorios
builder.Services.AddScoped<ICommonsProces<Tarea>, TaskRepository>();

// Servicios de aplicación
builder.Services.AddScoped<TaskService>();

// Cola secuencial (Singleton para que quede activa todo el tiempo)
builder.Services.AddSingleton<TaskQueueService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
