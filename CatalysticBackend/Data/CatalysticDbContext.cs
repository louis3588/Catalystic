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
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<PractitionerNote> PractitionerNotes => Set<PractitionerNote>();
    public DbSet<SharedClientNote> SharedClientNotes => Set<SharedClientNote>();
    public DbSet<ClientTask> ClientTasks => Set<ClientTask>();
    public DbSet<ClientInvitation> ClientInvitations => Set<ClientInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientInvitation>(entity =>
        {
            entity.Property(e => e.NormalizedEmail).HasMaxLength(254).IsRequired();
            entity.Property(e => e.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(e => e.NormalizedEmail).IsUnique();
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.IssuedBy).WithMany().HasForeignKey(e => e.IssuedById).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PractitionerNote>(entity =>
        {
            entity.Property(e => e.Content).HasMaxLength(4000).IsRequired();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ClientId, e.CreatedAt });
        });

        modelBuilder.Entity<SharedClientNote>(entity =>
        {
            entity.Property(e => e.Content).HasMaxLength(4000).IsRequired();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ClientId, e.SharedAt });
        });

        modelBuilder.Entity<ClientTask>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(160).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.Pillar).HasMaxLength(20).IsRequired();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ClientId, e.CreatedAt });
        });

        modelBuilder.Entity<Enquiry>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(254).IsRequired();
            entity.Property(e => e.ContactPreference).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Goals).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
        });

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
            entity.HasIndex(e => new { e.IsPublished, e.ReleaseTime });
            
        });
        

    }
}
