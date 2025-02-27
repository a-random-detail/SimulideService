using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SimulideService;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Repositories.Queries;
using SimulideService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseConfig>();
builder.Services.AddSingleton<NpgsqlConnectionFactory>();
builder.Services.AddScoped<IDocumentReadRepository, DocumentReadRepository>();
builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IDocumentWriteRepository, DocumentWriteRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetDocumentByIdQueryHandler).Assembly));
builder.Services.AddScoped(typeof(ITransactionManager<>), typeof(TransactionManager<>));
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
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
return;

static void ApplyMigrations(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();

    if (webApplication.Environment.EnvironmentName != "Test")
    {
        dbContext.Database.Migrate(); // Applies any pending migrations
    }
}
