using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiroTime.Domain.Common;
using TiroTime.Domain.Entities;
using TiroTime.Domain.Identity;

namespace TiroTime.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<RecurringTimeEntry> RecurringTimeEntries => Set<RecurringTimeEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename Identity tables to remove AspNet prefix
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedByIp).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.ReasonRevoked).HasMaxLength(500);
        });

        // Configure Client
        builder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ContactPerson).HasMaxLength(200);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            // Configure Email value object
            entity.OwnsOne(e => e.Email, email =>
            {
                email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(255);
            });

            // Configure PhoneNumber value object
            entity.OwnsOne(e => e.PhoneNumber, phone =>
            {
                phone.Property(p => p.Value).HasColumnName("PhoneNumber").HasMaxLength(50);
            });

            // Configure Address value object
            entity.OwnsOne(e => e.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
                address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
                address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
            });
        });

        // Configure Project
        builder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.IsActive);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.ColorCode).HasMaxLength(7);

            // Configure HourlyRate value object
            entity.OwnsOne(e => e.HourlyRate, money =>
            {
                money.Property(m => m.Amount).HasColumnName("HourlyRateAmount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("HourlyRateCurrency").HasMaxLength(3);
            });

            // Configure Budget value object
            entity.OwnsOne(e => e.Budget, money =>
            {
                money.Property(m => m.Amount).HasColumnName("BudgetAmount").HasColumnType("decimal(18,2)");
                money.Property(m => m.Currency).HasColumnName("BudgetCurrency").HasMaxLength(3);
            });

            // Configure relationship with Client
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TimeEntry
        builder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => new { e.UserId, e.IsRunning });
            entity.HasIndex(e => e.RecurringTimeEntryId);

            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsRunning).IsRequired();

            // Configure relationship with Project
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with RecurringTimeEntry
            entity.HasOne(e => e.RecurringTimeEntry)
                .WithMany()
                .HasForeignKey(e => e.RecurringTimeEntryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure RecurringTimeEntry
        builder.Entity<RecurringTimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.IsActive, e.LastGeneratedDate })
                  .HasFilter("[IsActive] = 1");

            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsActive).IsRequired();

            // Configure RecurringPattern value object
            entity.OwnsOne(e => e.Pattern, pattern =>
            {
                pattern.Property(p => p.Frequency).HasColumnName("Pattern_Frequency").IsRequired();
                pattern.Property(p => p.Interval).HasColumnName("Pattern_Interval").IsRequired();
                pattern.Property(p => p.DaysOfWeek).HasColumnName("Pattern_DaysOfWeek")
                       .HasConversion(
                           v => v == null ? null : string.Join(",", v.Select(d => (int)d)),
                           v => v == null ? null : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s => (DayOfWeek)int.Parse(s)).ToArray());
                pattern.Property(p => p.DayOfMonth).HasColumnName("Pattern_DayOfMonth");
                pattern.Property(p => p.StartDate).HasColumnName("Pattern_StartDate").IsRequired();
                pattern.Property(p => p.EndDate).HasColumnName("Pattern_EndDate");
                pattern.Property(p => p.MaxOccurrences).HasColumnName("Pattern_MaxOccurrences");
            });

            // Configure relationship with Project
            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Clear domain events after saving
        foreach (var entity in ChangeTracker.Entries<AggregateRoot>().Select(e => e.Entity))
        {
            entity.ClearDomainEvents();
        }

        return result;
    }
}
