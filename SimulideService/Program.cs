using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SimulideService;
using SimulideService.Controllers;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Repositories.Queries;
using SimulideService.Services;
using WebSocketManager = SimulideService.Services.WebSocketManager;

var builder = WebApplication.CreateBuilder(args);

// Add Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseConfig>();
builder.Services.AddSingleton<NpgsqlConnectionFactory>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();

//Read layer - Dapper
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetDocumentByIdQueryHandler).Assembly));
builder.Services.AddScoped<IDocumentReadRepository, DocumentReadRepository>();

//Write layer - EF Core
builder.Services.AddScoped(typeof(ITransactionManager<>), typeof(TransactionManager<>));
builder.Services.AddScoped<IOperationWriteRepository, OperationWriteRepository>();
builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IDocumentWriteRepository, DocumentWriteRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

// signalr
builder.Services.AddSingleton<IWebSocketManager, WebSocketManager>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.SupportedProtocols.Add("json");
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); 
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalhost", builder =>
        {
            builder
            .WithOrigins("http://127.0.0.1:5173", "http://localhost:5173")
            .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

// All database connections
var configuration = builder.Configuration;
if (builder.Environment.EnvironmentName == "Test")
    builder.Configuration.AddJsonFile("appsettings.Test.json", optional: false);
var databaseConfig = DatabaseConfig.Load(configuration);
builder.Services.AddTransient<IDbConnection>((_) => new NpgsqlConnection(databaseConfig.GetConnectionString()));
builder.Services.AddDbContext<CollabContext>(options =>
    options.UseNpgsql(databaseConfig.GetConnectionString()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

ApplyMigrations(app);

app.UseAuthorization();
app.MapControllers();
app.UseWebSockets();
app.MapHub<CollaborationHub>("/collaboration");

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowLocalhost");
}

app.Run();
return;

static void ApplyMigrations(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();

    if (webApplication.Environment.EnvironmentName != "Test")
    {
        dbContext.Database.Migrate(); 
    }
}
