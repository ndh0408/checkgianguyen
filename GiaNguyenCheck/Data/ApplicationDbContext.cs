using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Data
{
    /// <summary>
    /// Database context chính của ứng dụng với hỗ trợ multi-tenant
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        // Core entities for MVP
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<EventPayment> EventPayments { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            ConfigureIdentityTables(builder);
            ConfigureEntities(builder);
            ConfigureIndexes(builder);
        }
        
        private void ConfigureIdentityTables(ModelBuilder builder)
        {
            // Cấu hình Identity tables với int key
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        }
        
        private void ConfigureEntities(ModelBuilder builder)
        {
            // Tenant entity
            builder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.LogoUrl).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CurrentPlan).HasConversion<int>();
                entity.Property(e => e.CustomSettings).HasColumnType("nvarchar(max)");
                
                // Unique constraints
                entity.HasIndex(e => e.Email).IsUnique();
            });
            
            // User entity extensions
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AvatarUrl).HasMaxLength(200);
                entity.Property(e => e.Role).HasConversion<int>();
                
                // Relationship with Tenant
                entity.HasOne(e => e.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Event entity (simplified)
            builder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
                entity.Property(e => e.BannerImageUrl).HasMaxLength(300);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Settings).HasColumnType("nvarchar(max)");
                
                // Relationships (simplified to avoid cascade issues)
                entity.HasOne(e => e.Tenant)
                      .WithMany(t => t.Events)
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Guest entity (simplified)
            builder.Entity<Guest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Company).HasMaxLength(200);
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.AvatarUrl).HasMaxLength(300);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.TableNumber).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.QRCode).IsRequired().HasMaxLength(500);
                entity.Property(e => e.QRCodeHash).IsRequired().HasMaxLength(200);
                entity.Property(e => e.InvitationStatus).HasConversion<int>();
                
                // Relationship (simplified)
                entity.HasOne(e => e.Event)
                      .WithMany(ev => ev.Guests)
                      .HasForeignKey(e => e.EventId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Payment entity
            builder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Plan).HasConversion<int>();
                entity.Property(e => e.Method).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ReferenceCode).IsRequired().HasMaxLength(100);
                
                // Index
                entity.HasIndex(e => e.ReferenceCode).IsUnique();
                entity.HasIndex(e => e.TransactionId);
                
                // Relationship
                entity.HasOne(e => e.Tenant)
                      .WithMany(t => t.Payments)
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            
            // Invitation entity
            builder.Entity<Invitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).HasConversion<int>();
                entity.Property(e => e.Message).HasMaxLength(500);
                
                // Indexes
                entity.HasIndex(e => e.Code).IsUnique();
                entity.HasIndex(e => new { e.TenantId, e.Email });
                entity.HasIndex(e => new { e.Code, e.IsUsed });
                
                // Relationships
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.UsedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.UsedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
        
        private void ConfigureIndexes(ModelBuilder builder)
        {
            // Performance indexes
            builder.Entity<Event>()
                .HasIndex(e => new { e.TenantId, e.Status });
                
            builder.Entity<Event>()
                .HasIndex(e => new { e.TenantId, e.StartTime });
                
            builder.Entity<Guest>()
                .HasIndex(e => new { e.TenantId, e.EventId });
                
            builder.Entity<User>()
                .HasIndex(e => new { e.TenantId, e.Role });
        }
        
        /// <summary>
        /// Override SaveChanges để tự động set audit fields
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        /// <summary>
        /// Override SaveChanges đồng bộ
        /// </summary>
        public override int SaveChanges()
        {
            SetAuditFields().GetAwaiter().GetResult();
            return base.SaveChanges();
        }
        
        /// <summary>
        /// Tự động set các trường audit (CreatedAt, UpdatedAt, etc.)
        /// </summary>
        private async Task SetAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            
            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.UpdatedAt = null;
                        break;
                        
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        // Không cho phép thay đổi CreatedAt
                        entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                        break;
                }
            }
            
            await Task.CompletedTask;
        }
    }
} 