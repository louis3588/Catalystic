using CatalysticBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Data;

public class CatalysticDbContext : DbContext
{
    public CatalysticDbContext(DbContextOptions<CatalysticDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ConfirmationTokenHash)
                .HasColumnName("ComfirmationTokenHash");

            entity.Property(e => e.Role).HasMaxLength(16);

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
                .HasMaxLength(128);
        });

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(8192);
            
        });
        

    }
}
