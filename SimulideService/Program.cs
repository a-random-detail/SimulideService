using Microsoft.EntityFrameworkCore;
using SimulideService.Domain.Data;
using SimulideService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStatusRepository, StatusRepository>();

builder.Services.AddDbContext<CollabContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString(
        @"Server=simulide-db;Port=5432;Database=simulide;User Id=simulide;Password=simulide;"));

});

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