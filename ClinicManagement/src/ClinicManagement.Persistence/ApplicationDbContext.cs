using System.Reflection;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence;

/// <summary>
/// Root DbContext. Inherits IdentityDbContext so ApplicationUser + roles live
/// in the same database/model as business data. Global query filters are
/// applied automatically to every entity implementing IMustHaveTenant and/or
/// inheriting AuditableEntity — see ApplyGlobalFilters below. No handler or
/// repository needs to remember to add a "WHERE TenantId = ..." clause.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories => Set<AppointmentStatusHistory>();

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalRecordTemplate> MedicalRecordTemplates => Set<MedicalRecordTemplate>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<TreatmentPackage> TreatmentPackages => Set<TreatmentPackage>();
    public DbSet<PatientPackage> PatientPackages => Set<PatientPackage>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly (Configurations/*.cs).
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Rename Identity's default AspNetX tables to something less framework-y (cosmetic, optional).
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        ApplyGlobalFilters(modelBuilder);
    }

    /// <summary>
    /// Walks every mapped entity type and attaches:
    ///   - a tenant filter (TenantId == current tenant, bypassed for SuperAdmin) for IMustHaveTenant entities
    ///   - a soft-delete filter (IsDeleted == false) for AuditableEntity entities
    /// combined into one filter when an entity is both (the common case).
    /// EF Core only allows a single HasQueryFilter per entity, hence the combination.
    /// </summary>
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (clrType == typeof(ApplicationUser))
                continue;

            var isTenantScoped = typeof(IMustHaveTenant).IsAssignableFrom(clrType);
            var isAuditable = typeof(AuditableEntity).IsAssignableFrom(clrType);

            if (isTenantScoped && isAuditable)
            {
                CallGenericFilterMethod(nameof(SetTenantAndSoftDeleteFilter), clrType);
            }
            else if (isTenantScoped)
            {
                CallGenericFilterMethod(nameof(SetTenantFilter), clrType);
            }
            else if (isAuditable)
            {
                CallGenericFilterMethod(nameof(SetSoftDeleteFilter), clrType);
            }
        }

        void CallGenericFilterMethod(string methodName, Type clrType)
        {
            typeof(ApplicationDbContext)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(clrType)
                .Invoke(this, new object[] { modelBuilder });
        }
    }

    // ReSharper disable once UnusedMember.Local — invoked reflectively above.
    private void SetTenantAndSoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : AuditableEntity, IMustHaveTenant
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            !e.IsDeleted && (_currentUserService.IsSuperAdmin || e.TenantId == _currentUserService.TenantId));
    }

    private void SetTenantFilter<T>(ModelBuilder modelBuilder)
        where T : class, IMustHaveTenant
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            _currentUserService.IsSuperAdmin || e.TenantId == _currentUserService.TenantId);
    }

    private void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : AuditableEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }
}