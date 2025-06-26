using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Data;

namespace GiaNguyenCheck.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Đảm bảo database được tạo
            await context.Database.MigrateAsync();

            // Tạo roles nếu chưa có
            await CreateRoles(roleManager);

            // Tạo SystemAdmin nếu chưa có
            await CreateSystemAdmin(userManager);

            // Tạo demo tenant và users
            await CreateDemoTenant(context, userManager);

            // Tạo events mẫu
            await CreateSampleEvents(context);

            await context.SaveChangesAsync();
        }

        private static async Task CreateRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "SystemAdmin", "TenantAdmin", "EventManager", "Staff", "Viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task CreateSystemAdmin(UserManager<User> userManager)
        {
            var adminEmail = "admin@gianguyencheck.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator",
                    Role = UserRole.SystemAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SystemAdmin");
                }
            }
        }

        private static async Task CreateDemoTenant(ApplicationDbContext context, UserManager<User> userManager)
        {
            // Kiểm tra xem demo tenant đã tồn tại chưa
            var demoTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "demo");
            
            if (demoTenant == null)
            {
                // Tạo demo tenant
                demoTenant = new Tenant
                {
                    Name = "Công ty TNHH Demo",
                    Subdomain = "demo",
                    Description = "Công ty demo để test hệ thống",
                    AdminEmail = "admin@demo.com",
                    LogoUrl = "/images/demo-logo.png",
                    PrimaryColor = "#0066cc",
                    SecondaryColor = "#ff6600",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Plan = new Plan
                    {
                        Name = "Pro",
                        Description = "Gói Pro cho doanh nghiệp",
                        Price = 500000,
                        BillingCycle = "Monthly",
                        MaxEvents = 50,
                        MaxGuests = 1000,
                        MaxUsers = 20,
                        Features = "Advanced Analytics, Custom Branding, Priority Support"
                    }
                };

                context.Tenants.Add(demoTenant);
                await context.SaveChangesAsync();

                // Tạo TenantAdmin cho demo tenant
                var tenantAdmin = new User
                {
                    UserName = "admin@demo.com",
                    Email = "admin@demo.com",
                    EmailConfirmed = true,
                    FullName = "Nguyễn Văn Giám Đốc",
                    PhoneNumber = "0901234567",
                    Role = UserRole.TenantAdmin,
                    TenantId = demoTenant.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(tenantAdmin, "TenantAdmin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(tenantAdmin, "TenantAdmin");
                }

                // Tạo EventManager cho demo tenant
                var eventManager = new User
                {
                    UserName = "manager@demo.com",
                    Email = "manager@demo.com",
                    EmailConfirmed = true,
                    FullName = "Trần Thị Quản Lý",
                    PhoneNumber = "0901234568",
                    Role = UserRole.EventManager,
                    TenantId = demoTenant.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                result = await userManager.CreateAsync(eventManager, "Manager@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(eventManager, "EventManager");
                }

                // Tạo Staff cho demo tenant
                var staff1 = new User
                {
                    UserName = "staff1@demo.com",
                    Email = "staff1@demo.com",
                    EmailConfirmed = true,
                    FullName = "Lê Văn Nhân Viên",
                    PhoneNumber = "0901234569",
                    Role = UserRole.Staff,
                    TenantId = demoTenant.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                result = await userManager.CreateAsync(staff1, "Staff@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staff1, "Staff");
                }

                var staff2 = new User
                {
                    UserName = "staff2@demo.com",
                    Email = "staff2@demo.com",
                    EmailConfirmed = true,
                    FullName = "Phạm Thị Checkin",
                    PhoneNumber = "0901234570",
                    Role = UserRole.Staff,
                    TenantId = demoTenant.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                result = await userManager.CreateAsync(staff2, "Staff@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staff2, "Staff");
                }

                // Tạo Viewer cho demo tenant
                var viewer = new User
                {
                    UserName = "viewer@demo.com",
                    Email = "viewer@demo.com",
                    EmailConfirmed = true,
                    FullName = "Hoàng Văn Xem Báo Cáo",
                    PhoneNumber = "0901234571",
                    Role = UserRole.Viewer,
                    TenantId = demoTenant.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                result = await userManager.CreateAsync(viewer, "Viewer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(viewer, "Viewer");
                }
            }
        }

        private static async Task CreateSampleEvents(ApplicationDbContext context)
        {
            var demoTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "demo");
            var eventManager = await context.Users.FirstOrDefaultAsync(u => u.Email == "manager@demo.com");

            if (demoTenant != null && eventManager != null)
            {
                // Kiểm tra xem đã có events chưa
                var existingEvents = await context.Events.Where(e => e.TenantId == demoTenant.Id).AnyAsync();
                
                if (!existingEvents)
                {
                    var events = new List<Event>
                    {
                        new Event
                        {
                            Name = "Hội nghị khách hàng 2024",
                            Description = "Hội nghị thường niên với khách hàng quan trọng",
                            StartDate = DateTime.Now.AddDays(30),
                            EndDate = DateTime.Now.AddDays(30).AddHours(4),
                            Location = "Khách sạn 5 sao, TP.HCM",
                            MaxGuests = 200,
                            EventType = "Conference",
                            Status = EventStatus.Published,
                            TenantId = demoTenant.Id,
                            CreatedByUserId = eventManager.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Name = "Workshop Digital Marketing",
                            Description = "Workshop về marketing số cho đội ngũ sales",
                            StartDate = DateTime.Now.AddDays(15),
                            EndDate = DateTime.Now.AddDays(15).AddHours(3),
                            Location = "Văn phòng công ty, Hà Nội",
                            MaxGuests = 50,
                            EventType = "Workshop",
                            Status = EventStatus.Published,
                            TenantId = demoTenant.Id,
                            CreatedByUserId = eventManager.Id,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Event
                        {
                            Name = "Lễ kỷ niệm 10 năm thành lập",
                            Description = "Lễ kỷ niệm 10 năm thành lập công ty",
                            StartDate = DateTime.Now.AddDays(60),
                            EndDate = DateTime.Now.AddDays(60).AddHours(6),
                            Location = "Trung tâm hội nghị quốc gia",
                            MaxGuests = 500,
                            EventType = "Celebration",
                            Status = EventStatus.Draft,
                            TenantId = demoTenant.Id,
                            CreatedByUserId = eventManager.Id,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.Events.AddRange(events);
                    await context.SaveChangesAsync();

                    // Tạo guests mẫu cho event đầu tiên
                    var firstEvent = events.First();
                    var guests = new List<Guest>
                    {
                        new Guest
                        {
                            FullName = "Nguyễn Văn Khách 1",
                            Email = "khach1@example.com",
                            PhoneNumber = "0901234001",
                            Company = "Công ty ABC",
                            Position = "Giám đốc",
                            EventId = firstEvent.Id,
                            IsVIP = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Guest
                        {
                            FullName = "Trần Thị Khách 2",
                            Email = "khach2@example.com",
                            PhoneNumber = "0901234002",
                            Company = "Công ty XYZ",
                            Position = "Trưởng phòng",
                            EventId = firstEvent.Id,
                            IsVIP = false,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Guest
                        {
                            FullName = "Lê Văn Khách 3",
                            Email = "khach3@example.com",
                            PhoneNumber = "0901234003",
                            Company = "Công ty DEF",
                            Position = "Nhân viên",
                            EventId = firstEvent.Id,
                            IsVIP = false,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.Guests.AddRange(guests);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
} 