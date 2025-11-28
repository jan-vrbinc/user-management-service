using Microsoft.EntityFrameworkCore;
using UserManagementService.Models;

namespace UserManagementService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<ApiClient> ApiClients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Password).IsRequired();
            });

            modelBuilder.Entity<ApiClient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.ApiKey).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                entity.HasData(new ApiClient
                {
                    Id = 1,
                    Name = "Test Client",
                    ApiKey = "test-api-key-123"
                });
            });
        }
    }
}

