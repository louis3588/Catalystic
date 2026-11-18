using CatalyticBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalyticBackend.Data;

public class CatalysticDbContext : DbContext
{
    public CatalysticDbContext(DbContextOptions<CatalysticDbContext> options)
    {
        
    }
    
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(64);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(16);

            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(60);
        });
        
    }
}