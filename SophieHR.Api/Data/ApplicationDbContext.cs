﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Models;

namespace SophieHR.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAddress> CompanyAddresses { get; set; }
        public DbSet<EmployeeAddress> EmployeeAddresses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeAvatar> EmployeeAvatars { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Company>(b =>
            {
                b.Property(x => x.Name).IsRequired().HasMaxLength(200);
                b.HasMany(x => x.Employees).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(x => x.FirstName).HasMaxLength(50);
                b.Property(x => x.LastName).HasMaxLength(50);
            });

            builder.Entity<Address>(b =>
            {
                b.Property(x => x.Line1).IsRequired().HasMaxLength(50);
                b.Property(x => x.Line2).HasMaxLength(50);
                b.Property(x => x.Line3).HasMaxLength(50);
                b.Property(x => x.Line4).HasMaxLength(50);
                b.Property(x => x.County).HasMaxLength(50);
                b.Property(x => x.Postcode).IsRequired().HasMaxLength(8);
            });

            builder.Entity<Department>(b =>
            {
                b.Property(x => x.CompanyId).IsRequired();
                b.Property(x => x.Name).IsRequired().HasMaxLength(100);
                b.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<EmployeeAvatar>(b =>
            {
                b.HasOne(x => x.Employee).WithOne(x => x.Avatar).HasForeignKey<Employee>(x => x.EmployeeAvatarId);
            });

            builder.Entity<Employee>(b =>
            {
                b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
                b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
                b.Property(x => x.HolidayAllowance).IsRequired();
                b.Property(x => x.StartOfEmployment).IsRequired();
                b.Property(x => x.MiddleName).HasMaxLength(100);
                b.Property(x => x.PersonalEmailAddress).HasMaxLength(100);
                b.Property(x => x.WorkPhoneNumber).HasMaxLength(50);
                b.Property(x => x.WorkEmailAddress).IsRequired().HasMaxLength(100);
                b.Property(x => x.PassportNumber).HasMaxLength(9);
                b.Property(x => x.NationalInsuranceNumber).HasMaxLength(9);
                b.Property(x => x.Title).HasConversion<string>().HasMaxLength(10);
                b.Property(x => x.Gender).HasConversion<string>().HasMaxLength(10);
                b.HasOne(x => x.Avatar).WithOne(x => x.Employee).HasForeignKey<EmployeeAvatar>(x => x.EmployeeId);
                b.HasOne(x => x.Company).WithMany(x => x.Employees).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(builder);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                        .Entries()
                        .Where(e => e.Entity is Base && (
                                e.State == EntityState.Added
                                || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Modified)
                {
                    ((Base)entityEntry.Entity).UpdatedDate = DateTime.Now;
                }
                if (entityEntry.State == EntityState.Added)
                {
                    ((Base)entityEntry.Entity).CreatedDate = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                        .Entries()
                        .Where(e => e.Entity is Base && (
                                e.State == EntityState.Added
                                || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Modified)
                {
                    ((Base)entityEntry.Entity).UpdatedDate = DateTime.Now;
                }

                if (entityEntry.State == EntityState.Added)
                {
                    ((Base)entityEntry.Entity).CreatedDate = DateTime.Now;
                }
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}