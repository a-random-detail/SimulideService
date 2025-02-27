using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetDocumentByIdQueryHandler).Assembly));
builder.Services.AddScoped(typeof(ITransactionManager<>), typeof(TransactionManager<>));
var configuration = builder.Configuration;
if (builder.Environment.EnvironmentName == "Test")
    builder.Configuration.AddJsonFile("appsettings.Test.json", optional: false);
var databaseConfig = DatabaseConfig.Load(configuration);

builder.Services.AddDbContext<CollabContext>(options =>
    options.UseNpgsql(databaseConfig.GetConnectionString()));



// var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDb");
// if (useInMemory)
// {
//     builder.Services.AddDbContext<CollabContext>(opts =>
//     {
//         opts.UseInMemoryDatabase("CollabDb");
//         opts.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
//     });
// } else {
//     builder.Services.AddDbContext<CollabContext>(opts =>
//     {
//         opts.UseNpgsql(
//             @"Server=simulide-db;Port=5432;Database=simulide;User Id=simulide;Password=simulide;");
//     });
// }

var app = builder.Build();

// Configure the HTTP request pipeline.
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
static void ApplyMigrations(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();

    if (webApplication.Environment.IsProduction() || webApplication.Environment.IsStaging())
    {
        dbContext.Database.Migrate(); // Applies any pending migrations
    }
    else if (webApplication.Environment.IsDevelopment() || webApplication.Environment.IsEnvironment("Test"))
    {
        dbContext.Database.EnsureCreated(); // Creates DB if missing (useful for in-memory)
    }
}
