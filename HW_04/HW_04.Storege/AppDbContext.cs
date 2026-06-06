using HW_04.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HW_04.Storege;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>()
            .HasOne(m => m.Author)
            .WithMany(a => a.Movies)
            .HasForeignKey(m => m.AuthorId);
    }
    
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Author> Authors { get; set; }
}