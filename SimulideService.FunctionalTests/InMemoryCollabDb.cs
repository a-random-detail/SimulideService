using Microsoft.EntityFrameworkCore;
using SimulideService.Domain.Data;

namespace SimulideService.FunctionalTests;

public class InMemoryCollabDb: DbContext
{
    public InMemoryCollabDb(DbContextOptions<InMemoryCollabDb> options) : base(options) { }
    public DbSet<Domain.Data.Document> Documents { get; set; } 
    public DbSet<Operation> Operations { get; set; } 
}