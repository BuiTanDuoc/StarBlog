using Microsoft.EntityFrameworkCore;

namespace TaskManagementSystem.Data
{
    public class TaskManagementDbContext : DbContext
    {
        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
        public DbSet<ProjectTask> Tasks { get; set; } = null!;
        public DbSet<TaskComment> TaskComments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============= USER ENTITY =============
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Firstname).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Lastname).HasMaxLength(255).IsRequired();

                entity.Property(e => e.UserRole)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.InsertedAt)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.ModifiedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();
                entity.HasIndex(e => e.Username, "IX_Users_Username").IsUnique();
            });

            // ============= PROJECT ENTITY =============
            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.InsertedAt)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.ModifiedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Owner relationship - CASCADE (delete user = delete projects)
                entity.HasOne(p => p.Owner)
                    .WithMany(u => u.OwnedProjects)
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============= PROJECT MEMBER ENTITY =============
            modelBuilder.Entity<ProjectMember>(entity =>
            {
                entity.ToTable("ProjectMembers");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.InsertedAt)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.ModifiedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Project relationship - CASCADE (delete project = delete members)
                entity.HasOne(pm => pm.Project)
                    .WithMany(p => p.Members)
                    .HasForeignKey(pm => pm.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                // User relationship - NO ACTION (prevents multiple cascade paths)
                // This is CRITICAL to avoid the SQL Server error
                entity.HasOne(pm => pm.User)
                    .WithMany(u => u.ProjectMemberships)
                    .HasForeignKey(pm => pm.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Unique constraint: One user per project
                entity.HasIndex(pm => new { pm.ProjectId, pm.UserId })
                    .IsUnique();
            });

            // ============= PROJECT TASK ENTITY =============
            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.ToTable("Tasks");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(2000);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.Priority)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(e => e.InsertedAt)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.ModifiedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Project relationship - CASCADE (delete project = delete tasks)
                entity.HasOne(t => t.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Assigned user relationship - SET NULL (delete user = unassign task)
                // IMPORTANT: This must be SetNull, NOT NoAction or Cascade
                entity.HasOne(t => t.AssignedToUser)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(t => t.AssignedToUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ============= TASK COMMENT ENTITY =============
            modelBuilder.Entity<TaskComment>(entity =>
            {
                entity.ToTable("TaskComments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(e => e.InsertedAt)
                    .ValueGeneratedOnAdd()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.ModifiedAt)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Task relationship - CASCADE (delete task = delete comments)
                entity.HasOne(tc => tc.Task)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(tc => tc.ProjectTaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                // User relationship - NO ACTION (prevents multiple cascade paths)
                // This is CRITICAL to avoid the SQL Server error
                entity.HasOne(tc => tc.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(tc => tc.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}