using Aliyar.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Aliyar.Web.Data;

public sealed class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    public DbSet<Car> Cars => Set<Car>();

    public DbSet<CarSale> CarSales => Set<CarSale>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<CarPhoto> CarPhotos => Set<CarPhoto>();

    public DbSet<ManagerProfile> ManagerProfiles => Set<ManagerProfile>();

    public DbSet<CarReservation> CarReservations => Set<CarReservation>();

    public DbSet<CarSpecification> CarSpecifications => Set<CarSpecification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>(entity =>
        {
            entity.ToTable("car_shop");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Make).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Year).IsRequired();
            entity.Property(x => x.Price).IsRequired();
            entity.Property(x => x.PurchasePrice).HasDefaultValue(0);
            entity.Property(x => x.StockQuantity).HasDefaultValue(1);
            entity.Property(x => x.Vin).HasMaxLength(32);

            entity.HasIndex(x => x.Vin).IsUnique().HasFilter("\"Vin\" IS NOT NULL");
            entity.Property(x => x.IsSold).HasDefaultValue(false);
            entity.Property(x => x.IsArchived).HasDefaultValue(false);
            entity.Property(x => x.ArchivedByUserId).HasMaxLength(450);
            entity.Property(x => x.Kind).HasConversion<int>().HasDefaultValue(ListingKind.Store);
            entity.Property(x => x.OwnerUserId).HasMaxLength(450);
            entity.HasIndex(x => x.Kind);
            entity.HasIndex(x => x.IsArchived);
            entity.HasIndex(x => x.OwnerUserId);

            entity.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CarPhoto>(entity =>
        {
            entity.ToTable("car_photos");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Path).HasMaxLength(260).IsRequired();
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.CreatedAtUtc).IsRequired();

            entity.HasIndex(x => x.CarId);
            entity.HasIndex(x => new { x.CarId, x.SortOrder });

            entity.HasOne(x => x.Car)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarSale>(entity =>
        {
            entity.ToTable("car_sales");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SalePrice).IsRequired();
            entity.Property(x => x.PaymentMethod).HasConversion<int>().HasDefaultValue(PaymentMethod.Cash);
            entity.Property(x => x.Quantity).HasDefaultValue(1);
            entity.Property(x => x.SoldAtUtc).IsRequired();
            entity.Property(x => x.SoldByUserId).HasMaxLength(450);

            entity.HasOne(x => x.Car)
                .WithMany()
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Client)
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => x.CarId);
            entity.HasIndex(x => x.ClientId);
            entity.HasIndex(x => x.SoldAtUtc);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("clients");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();

            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ManagerProfile>(entity =>
        {
            entity.ToTable("manager_profiles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.PassportNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(400).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Age).IsRequired();
            entity.Property(x => x.Gender).HasConversion<int>().HasDefaultValue(ManagerGender.Unknown);
            entity.Property(x => x.PhotoPath).HasMaxLength(260).IsRequired();

            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarReservation>(entity =>
        {
            entity.ToTable("car_reservations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CustomerDetails).HasMaxLength(400);
            entity.Property(x => x.Quantity).HasDefaultValue(1);
            entity.Property(x => x.ReservedUntilUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active").IsRequired();

            entity.HasIndex(x => x.CarId);
            entity.HasIndex(x => x.ReservedUntilUtc);
            entity.HasIndex(x => x.Status);

            entity.HasOne(x => x.Car)
                .WithMany()
                .HasForeignKey(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarSpecification>(entity =>
        {
            entity.ToTable("car_specifications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BodyType).HasConversion<int>().HasDefaultValue(BodyType.Sedan);
            entity.Property(x => x.Mileage).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(50);
            entity.Property(x => x.EngineVolumeLiters).HasPrecision(4, 2);
            entity.Property(x => x.EngineType).HasConversion<int>().HasDefaultValue(EngineType.Unknown);
            entity.Property(x => x.Transmission).HasConversion<int>().HasDefaultValue(TransmissionType.Unknown);
            entity.Property(x => x.Drive).HasConversion<int>().HasDefaultValue(CarDriveType.Unknown);
            entity.Property(x => x.FuelConsumptionL100Km).HasPrecision(4, 1);
            entity.Property(x => x.EmissionClass).HasMaxLength(20);
            entity.Property(x => x.Documents).HasMaxLength(300);
            entity.Property(x => x.Condition).HasMaxLength(400);
            entity.Property(x => x.UpdatedAtUtc).IsRequired();

            entity.HasIndex(x => x.CarId).IsUnique();
            entity.HasIndex(x => x.BodyType);
            entity.HasIndex(x => x.Mileage);

            entity.HasOne(x => x.Car)
                .WithOne(x => x.Specification)
                .HasForeignKey<CarSpecification>(x => x.CarId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

