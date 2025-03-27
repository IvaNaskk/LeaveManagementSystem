using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using project1.Models;

namespace project1.Data
{
    public class ApplicationDbContext : IdentityDbContext<Employee>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<VacationRequest> VacationRequests { get; set; }
        public virtual DbSet<SickLeaveRequest> SickLeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee entity
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.Id);

            // default vrednost za BonusLeaveDays
            modelBuilder.Entity<Employee>()
                .Property(e => e.BonusLeaveDays)
                .HasDefaultValue(0); // Default value

            // VacationRequest entity
            modelBuilder.Entity<VacationRequest>()
                .HasOne(v => v.Employee)
                .WithMany(e => e.VacationRequests)
                .HasForeignKey(v => v.EmployeeId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // SickLeaveRequest entity
            modelBuilder.Entity<SickLeaveRequest>()
                .HasOne(s => s.Employee)
                .WithMany(e => e.SickLeaveRequests)
                .HasForeignKey(s => s.EmployeeId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // SQLite
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                modelBuilder.Entity<Employee>()
                    .Property(e => e.Email)
                    .HasMaxLength(255);

                modelBuilder.Entity<VacationRequest>()
                    .Property(v => v.EmployeeId)
                    .HasMaxLength(255);

                modelBuilder.Entity<SickLeaveRequest>()
                    .Property(s => s.EmployeeId)
                    .HasMaxLength(255);
            }
        }
    }
}
