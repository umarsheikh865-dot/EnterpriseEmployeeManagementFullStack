using EnterpriseEmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagement.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============================================
        // TABLES
        // ============================================

        public DbSet<Employee> Employees => Set<Employee>();

        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Attendance> Attendances => Set<Attendance>();

        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // ============================================
        // RELATIONSHIPS
        // ============================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // Employee → Department
            // ========================================

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // Employee → Role
            // ========================================

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========================================
            // Attendance → Employee
            // ========================================

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========================================
            // LeaveRequest → Employee
            // ========================================

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========================================
            // RefreshToken → Employee
            // ========================================

            // ========================================
            // RefreshToken → Employee
            // ========================================

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(r => r.CreatedAt)
                    .IsRequired();

                entity.Property(r => r.ExpiresAt)
                    .IsRequired();

                entity.Property(r => r.RevokedAt)
                    .IsRequired(false);

                entity.HasOne(r => r.Employee)
                    .WithMany(e => e.RefreshTokens)
                    .HasForeignKey(r => r.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // EMPLOYEE EMAIL
            // ========================================

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            // ========================================
            // SALARY PRECISION
            // ========================================

            modelBuilder.Entity<Employee>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);
        }
    }
}