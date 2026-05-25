using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourGuideMarketplace.Application.Common.Security;
using TourGuideMarketplace.Domain.Bookings;
using TourGuideMarketplace.Domain.Common;
using TourGuideMarketplace.Domain.Guides;
using TourGuideMarketplace.Domain.Payments;
using TourGuideMarketplace.Domain.Reviews;
using TourGuideMarketplace.Domain.Tourists;
using TourGuideMarketplace.Infrastructure.Identity;

namespace TourGuideMarketplace.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private static readonly Guid TouristRoleId = Guid.Parse("7f3eb40e-dfeb-4d9c-a978-3ecbe439f158");
    private static readonly Guid GuideRoleId = Guid.Parse("ab6eaedb-d0be-483f-b76e-a0b2ec2d0cb1");
    private static readonly Guid AdminRoleId = Guid.Parse("1647b982-7a3f-4648-9f01-f912fc2989ae");

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<GuideProfile> GuideProfiles => Set<GuideProfile>();
    public DbSet<GuideSpecialty> GuideSpecialties => Set<GuideSpecialty>();
    public DbSet<GuideLanguage> GuideLanguages => Set<GuideLanguage>();
    public DbSet<GuideVerification> GuideVerifications => Set<GuideVerification>();
    public DbSet<TouristProfile> TouristProfiles => Set<TouristProfile>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Payment> Payments => Set<Payment>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentity(builder);
        ConfigureProfiles(builder);
        ConfigureBookings(builder);
        ConfigureReviews(builder);
        ConfigurePayments(builder);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(user => user.FullName).HasMaxLength(160).IsRequired();
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        builder.Entity<IdentityRole<Guid>>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasData(
                new IdentityRole<Guid>
                {
                    Id = TouristRoleId,
                    Name = AppRoles.Tourist,
                    NormalizedName = AppRoles.Tourist.ToUpperInvariant(),
                    ConcurrencyStamp = TouristRoleId.ToString()
                },
                new IdentityRole<Guid>
                {
                    Id = GuideRoleId,
                    Name = AppRoles.Guide,
                    NormalizedName = AppRoles.Guide.ToUpperInvariant(),
                    ConcurrencyStamp = GuideRoleId.ToString()
                },
                new IdentityRole<Guid>
                {
                    Id = AdminRoleId,
                    Name = AppRoles.Admin,
                    NormalizedName = AppRoles.Admin.ToUpperInvariant(),
                    ConcurrencyStamp = AdminRoleId.ToString()
                });
        });

        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(token => token.CreatedByIp).HasMaxLength(64);
            entity.Property(token => token.RevokedByIp).HasMaxLength(64);
            entity.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasOne(token => token.User)
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureProfiles(ModelBuilder builder)
    {
        builder.Entity<GuideProfile>(entity =>
        {
            entity.ToTable("GuideProfiles");
            entity.HasKey(profile => profile.Id);
            entity.HasQueryFilter(profile => !profile.IsDeleted);
            entity.HasIndex(profile => profile.UserId).IsUnique();
            entity.HasIndex(profile => new { profile.City, profile.Country });
            entity.Property(profile => profile.Bio).HasMaxLength(2000);
            entity.Property(profile => profile.City).HasMaxLength(120).IsRequired();
            entity.Property(profile => profile.Country).HasMaxLength(120).IsRequired();
            entity.Property(profile => profile.Currency).HasMaxLength(3).IsRequired();
            entity.Property(profile => profile.HourlyRate).HasPrecision(10, 2);
            entity.Property(profile => profile.AverageRating).HasPrecision(3, 2);
            entity.Property(profile => profile.Latitude).HasPrecision(9, 6);
            entity.Property(profile => profile.Longitude).HasPrecision(9, 6);
            entity.HasMany(profile => profile.Specialties)
                .WithOne()
                .HasForeignKey(specialty => specialty.GuideProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(profile => profile.Languages)
                .WithOne()
                .HasForeignKey(language => language.GuideProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GuideSpecialty>(entity =>
        {
            entity.ToTable("GuideSpecialties");
            entity.HasKey(specialty => specialty.Id);
            entity.HasQueryFilter(specialty => !specialty.IsDeleted);
            entity.Property(specialty => specialty.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(specialty => new { specialty.GuideProfileId, specialty.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<GuideLanguage>(entity =>
        {
            entity.ToTable("GuideLanguages");
            entity.HasKey(language => language.Id);
            entity.HasQueryFilter(language => !language.IsDeleted);
            entity.Property(language => language.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(language => new { language.GuideProfileId, language.Name })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<GuideVerification>(entity =>
        {
            entity.ToTable("GuideVerifications");
            entity.HasKey(verification => verification.Id);
            entity.HasQueryFilter(verification => !verification.IsDeleted);
            entity.Property(verification => verification.DocumentType).HasMaxLength(80).IsRequired();
            entity.Property(verification => verification.DocumentUrl).HasMaxLength(600).IsRequired();
            entity.Property(verification => verification.LicenseNumber).HasMaxLength(120);
            entity.Property(verification => verification.RejectionReason).HasMaxLength(500);
            entity.HasOne<GuideProfile>()
                .WithMany()
                .HasForeignKey(verification => verification.GuideProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TouristProfile>(entity =>
        {
            entity.ToTable("TouristProfiles");
            entity.HasKey(profile => profile.Id);
            entity.HasQueryFilter(profile => !profile.IsDeleted);
            entity.HasIndex(profile => profile.UserId).IsUnique();
            entity.Property(profile => profile.Country).HasMaxLength(120);
            entity.Property(profile => profile.PreferredLanguage).HasMaxLength(80);
        });
    }

    private static void ConfigureBookings(ModelBuilder builder)
    {
        builder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(booking => booking.Id);
            entity.HasQueryFilter(booking => !booking.IsDeleted);
            entity.Property(booking => booking.MeetingPoint).HasMaxLength(300).IsRequired();
            entity.Property(booking => booking.TotalAmount).HasPrecision(12, 2);
            entity.Property(booking => booking.Currency).HasMaxLength(3).IsRequired();
            entity.Property(booking => booking.TouristNotes).HasMaxLength(1000);
            entity.Property(booking => booking.CancellationReason).HasMaxLength(500);
            entity.HasIndex(booking => new { booking.GuideProfileId, booking.StartsAt, booking.EndsAt });
            entity.HasOne<TouristProfile>()
                .WithMany()
                .HasForeignKey(booking => booking.TouristProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<GuideProfile>()
                .WithMany()
                .HasForeignKey(booking => booking.GuideProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReviews(ModelBuilder builder)
    {
        builder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(review => review.Id);
            entity.HasQueryFilter(review => !review.IsDeleted);
            entity.Property(review => review.Comment).HasMaxLength(1000);
            entity.HasIndex(review => review.BookingId).IsUnique();
            entity.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(review => review.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TouristProfile>()
                .WithMany()
                .HasForeignKey(review => review.TouristProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<GuideProfile>()
                .WithMany()
                .HasForeignKey(review => review.GuideProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePayments(ModelBuilder builder)
    {
        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(payment => payment.Id);
            entity.HasQueryFilter(payment => !payment.IsDeleted);
            entity.Property(payment => payment.Amount).HasPrecision(12, 2);
            entity.Property(payment => payment.PlatformFeeAmount).HasPrecision(12, 2);
            entity.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
            entity.Property(payment => payment.Provider).HasMaxLength(80);
            entity.Property(payment => payment.ProviderPaymentId).HasMaxLength(200);
            entity.HasIndex(payment => payment.BookingId);
            entity.HasOne<Booking>()
                .WithMany()
                .HasForeignKey(payment => payment.BookingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ApplyAuditValues()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }

            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
