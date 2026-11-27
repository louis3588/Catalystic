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
    public DbSet<BlogLabel> BlogLabels => Set<BlogLabel>();
    public DbSet<Enquiry> Enquiries => Set<Enquiry>();
    public DbSet<PractitionerNote> PractitionerNotes => Set<PractitionerNote>();
    public DbSet<SharedClientNote> SharedClientNotes => Set<SharedClientNote>();
    public DbSet<ClientTask> ClientTasks => Set<ClientTask>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<ScheduledCall> ScheduledCalls => Set<ScheduledCall>();
    public DbSet<ScheduledMeeting> ScheduledMeetings => Set<ScheduledMeeting>();
    public DbSet<CallRequest> CallRequests => Set<CallRequest>();
    public DbSet<ClientInvitation> ClientInvitations => Set<ClientInvitation>();
    public DbSet<WellnessAssessment> WellnessAssessments => Set<WellnessAssessment>();
    public DbSet<ClientData> ClientData => Set<ClientData>();
    public DbSet<WeeklyReview> WeeklyReviews => Set<WeeklyReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeeklyReview>(entity =>
        {
            entity.HasOne(review => review.ClientData).WithMany().HasForeignKey(review => review.ClientDataId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(review => new { review.ClientDataId, review.ReviewDate }).IsUnique();
            entity.HasIndex(review => review.ClientDataId).IsUnique().HasFilter("\"Status\" <> 'Completed'");
            entity.Property(review => review.Status).HasMaxLength(24).IsRequired();
            entity.ToTable(table => table.HasCheckConstraint("CK_WeeklyReview_Status", "\"Status\" IN ('PendingContact', 'Contacted', 'Completed')"));
            foreach (var field in new[] { "Physical", "Dietary", "Social", "Emotional", "Intellectual", "Habitual", "Financial" })
            {
                entity.Property<decimal>(field).HasPrecision(4, 2);
                entity.ToTable(table => table.HasCheckConstraint($"CK_WeeklyReview_{field}", $"\"{field}\" >= 0 AND \"{field}\" <= 10"));
            }
        });
        modelBuilder.Entity<ClientData>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(64);
            entity.HasOne(e => e.User).WithOne().HasForeignKey<ClientData>(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        modelBuilder.Entity<WellnessAssessment>(entity =>
        {
            entity.HasOne(e => e.ClientData).WithMany().HasForeignKey(e => e.ClientDataId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ClientDataId, e.AssessmentDate }).IsUnique();
            foreach (var field in new[] { "Physical", "Dietary", "Social", "Emotional", "Intellectual", "Habitual", "Financial", "PREOS" })
            {
                entity.Property<decimal>(field).HasPrecision(4, 2);
                entity.ToTable(table => table.HasCheckConstraint($"CK_WellnessAssessment_{field}", $"\"{field}\" >= 0 AND \"{field}\" <= 10"));
            }
        });
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

        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.ToTable("ClientTasks");
            entity.HasDiscriminator<string>("EventType")
                .HasValue<ClientTask>("Task")
                .HasValue<ScheduledCall>("Call")
                .HasValue<ScheduledMeeting>("Meeting");
            entity.Property(e => e.Title).HasMaxLength(160).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(4000).IsRequired();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ClientId, e.CreatedAt });
        });
        modelBuilder.Entity<TimedEvent>();
        modelBuilder.Entity<ClientTask>().Property(e => e.Pillar).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<CallRequest>(entity =>
        {
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(16).IsRequired().IsConcurrencyToken();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ScheduledCall).WithMany().HasForeignKey(e => e.ScheduledCallId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.ClientId, e.CreatedAt });
            entity.ToTable(table => table.HasCheckConstraint("CK_CallRequests_Status", "\"Status\" IN ('Pending', 'Scheduled', 'Declined')"));
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
            entity.ToTable(table => table.HasCheckConstraint("CK_Users_AccountState", "\"AccountState\" IN (0, 1, 2)"));
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
            entity.HasMany(e => e.Labels).WithMany(e => e.Posts);
            
        });
        

    }
}
