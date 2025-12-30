using Microsoft.EntityFrameworkCore;

namespace SimulideService.Domain.Data;

public class CollabContext: DbContext
{
    public CollabContext(DbContextOptions<CollabContext> options) : base(options) { }
    
    public DbSet<Operation> Operations { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Operation>().ToTable("operations");
        modelBuilder.Entity<Document>().ToTable("documents");
        
        base.OnModelCreating(modelBuilder);
    }
}