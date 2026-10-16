using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Seed;

public static class PlanSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Plans.IgnoreQueryFilters().AnyAsync())
        {
            return; // already seeded
        }

        var plans = new List<Plan>
        {
            new()
            {
                Name = "Free", Tier = PlanTier.Free, Price = 0, DurationInDays = 30,
                MaxDoctors = 1, MaxEmployees = 2, MaxPatients = 50, MaxAppointmentsPerMonth = 100,
                StorageLimitMb = 100
            },
            new()
            {
                Name = "Starter", Tier = PlanTier.Starter, Price = 29, DurationInDays = 30,
                MaxDoctors = 3, MaxEmployees = 5, MaxPatients = 500, MaxAppointmentsPerMonth = 1000,
                StorageLimitMb = 1024
            },
            new()
            {
                Name = "Professional", Tier = PlanTier.Professional, Price = 99, DurationInDays = 30,
                MaxDoctors = 10, MaxEmployees = 25, MaxPatients = 5000, MaxAppointmentsPerMonth = 10000,
                StorageLimitMb = 10240
            },
            new()
            {
                Name = "Enterprise", Tier = PlanTier.Enterprise, Price = 299, DurationInDays = 30,
                MaxDoctors = int.MaxValue, MaxEmployees = int.MaxValue, MaxPatients = int.MaxValue,
                MaxAppointmentsPerMonth = int.MaxValue, StorageLimitMb = 102400
            }
        };

        await context.Plans.AddRangeAsync(plans);
        await context.SaveChangesAsync();
    }
}
