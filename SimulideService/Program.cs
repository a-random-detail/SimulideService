using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped(typeof(ITransactionManager<>), typeof(TransactionManager<>));

if (builder.Environment.EnvironmentName == "Test")
    builder.Configuration.AddJsonFile("appsettings.Test.json", optional: false);

var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDb");
if (useInMemory)
{
    builder.Services.AddDbContext<CollabContext>(opts =>
    {
        opts.UseInMemoryDatabase("CollabDb");
        opts.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    });
} else {
    builder.Services.AddDbContext<CollabContext>(opts =>
    {
        opts.UseNpgsql(
            @"Server=simulide-db;Port=5432;Database=simulide;User Id=simulide;Password=simulide;");
    });
}

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

if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<CollabContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.Run();