using Microsoft.EntityFrameworkCore;
using GLMS.Data.Entities;

namespace GLMS.Data.DbContext;

public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Contract>()
            .HasOne(c => c.Client)
            .WithMany(c => c.Contracts)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Contract)
            .WithMany(c => c.ServiceRequests)
            .HasForeignKey(sr => sr.ContractId)
            .OnDelete(DeleteBehavior.Restrict);

        // Add indexes for performance - CORRECTED SYNTAX
        modelBuilder.Entity<Contract>()
            .HasIndex(c => c.Status);


        modelBuilder.Entity<Contract>()
            .HasIndex(c => new { c.StartDate, c.EndDate });


        modelBuilder.Entity<Contract>()
            .HasIndex(c => c.ContractNumber)
            .IsUnique();


        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(sr => sr.RequestNumber)
            .IsUnique();


        modelBuilder.Entity<Client>()
            .HasIndex(c => c.Email)
            .IsUnique();
            

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed sample clients
        modelBuilder.Entity<Client>().HasData(
            new Client
            {
                ClientId = 1,
                Name = "Global Trading Co",
                Email = "contact@globaltrading.com",
                Phone = "+27 11 123 4567",
                Address = "123 Main St, Johannesburg",
                Region = "Africa",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Client
            {
                ClientId = 2,
                Name = "EuroLogistics GmbH",
                Email = "info@eurologistics.de",
                Phone = "+49 30 9876543",
                Address = "Berliner Str 45, Berlin",
                Region = "Europe",
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new Client
            {
                ClientId = 3,
                Name = "Asia Freight Solutions",
                Email = "support@asiafreight.sg",
                Phone = "+65 6789 0123",
                Address = "Raffles Place, Singapore",
                Region = "Asia",
                CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed sample contracts
        modelBuilder.Entity<Contract>().HasData(
            new Contract
            {
                ContractId = 1,
                ContractNumber = "CT-2024-001",
                ClientId = 1,
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = ServiceLevel.Premium,
                TermsAndConditions = "Standard premium terms apply",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Contract
            {
                ContractId = 2,
                ContractNumber = "CT-2024-002",
                ClientId = 2,
                StartDate = new DateTime(2024, 2, 1),
                EndDate = new DateTime(2025, 1, 31),
                Status = ContractStatus.Active,
                ServiceLevel = ServiceLevel.Enterprise,
                TermsAndConditions = "Custom enterprise terms with SLA",
                CreatedAt = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Contract
            {
                ContractId = 3,
                ContractNumber = "CT-2023-089",
                ClientId = 3,
                StartDate = new DateTime(2023, 1, 1),
                EndDate = new DateTime(2023, 12, 31),
                Status = ContractStatus.Expired,
                ServiceLevel = ServiceLevel.Standard,
                TermsAndConditions = "Standard terms",
                CreatedAt = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}