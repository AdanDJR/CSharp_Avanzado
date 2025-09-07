using InfrastructureLayer.Repositorio.TaskRepository;
using InfrastructureLayer;
using DomainLayer.Models;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositorio.Commons;
using Microsoft.EntityFrameworkCore;
using ApplicationLayer.Services.TaskServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApplicationLayer.Services;
using Microsoft.OpenApi.Models;

// 👇 Importar SignalR y namespace del Hub/servicio
using TaskManager.Hubs;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuración de DbContext
builder.Services.AddDbContext<TaskManagerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerDB"));
});

// Inyección de dependencias
builder.Services.AddScoped<ICommonsProces<Tarea>, TaskRepository>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<TaskQueueService>();

builder.Services.AddControllers();

// 👇 Registrar SignalR
builder.Services.AddSignalR();

// 👇 Registrar el servicio de notificación de SignalR (usando namespace completo para evitar ambigüedad)
builder.Services.AddScoped<TaskManager.Services.INotificationService, TaskManager.Services.SignalRNotificationService>();

// Swagger/OpenAPI con soporte para JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskManager API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido de un espacio y su token JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ====== Configuración JWT ======
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 👇 Mapear Hub de SignalR
app.MapHub<TasksHub>("/taskshub");

app.Run();
