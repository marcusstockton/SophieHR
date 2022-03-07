using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SophieHR.Api.Models;
using System.Text;

namespace SophieHR.Api.Data
{
    public class DataSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var adminId = await EnsureUser(serviceProvider, testUserPw, "Dave", "Shell", "admin@contoso.com");
                await EnsureRole(serviceProvider, adminId, "Admin");

                var managerId = await EnsureUser(serviceProvider, testUserPw, "Mona", "Lisa", "manager@contoso.com");
                await EnsureRole(serviceProvider, managerId, "Manager");

                var userId = await EnsureUser(serviceProvider, testUserPw, "Shirley", "Temple", "user@contoso.com");
                await EnsureRole(serviceProvider, userId, "Basic User");

                await SeedDBAsync(context, adminId, managerId, userId);
            }
        }
        private static async Task<Guid> EnsureUser(IServiceProvider serviceProvider, string testUserPw, string firstName, string lastName, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            if (userManager == null)
            {
                throw new Exception("userManager is null");
            }
            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = UserName,
                    Email = UserName,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    NormalizedEmail = UserName.ToUpper(),
                    NormalizedUserName = UserName.ToUpper(),
                };
                var result = await userManager.CreateAsync(user, testUserPw);
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.Select(x => x.Description).ToList().ToString());
                }
            }
            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }
            return user.Id;
        }
        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, Guid userId, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole<Guid>>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            if (userManager == null)
            {
                throw new Exception("userManager is null");
            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            if (!await userManager.IsInRoleAsync(user, role))
            {
                IR = await userManager.AddToRoleAsync(user, role);
                if (!IR.Succeeded)
                {
                    throw new Exception(IR.Errors.Select(x => x.Description).ToList().ToString());
                }
            }
            
            return null;
        }
        public static async Task SeedDBAsync(ApplicationDbContext context, Guid adminId, Guid managerId, Guid userId)
        {
            if (!context.Companies.Any())
            {
                // Create some companies and logos:
                var logo1 = Convert.FromBase64String("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");
                var logo2 = Convert.FromBase64String("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");
                await context.Companies.AddRangeAsync(
                    new Company
                    {
                        Name = "Daves Window Cleaners",
                        Logo = logo1,
                        Address = new CompanyAddress { Line1 = "1", Line2 = "The Vale", Postcode = "EX11EX", County = "Devon" }
                    },
                    new Company
                    {
                        Name = "Flowers By Irene",
                        Logo = logo2,
                        Address = new CompanyAddress { Line1 = "Flat 1", Line2 = "12", Line3 = "Eastern Way", Postcode = "EX22EX", County = "Devon" }
                    });

                await context.SaveChangesAsync();
            }
            if (!context.Departments.Any())
            {
                // Add some departments
                var company1 = context.Companies.First();
                var company2 = context.Companies.Skip(1).First();

                await context.Departments.AddRangeAsync(
                    new Department
                    {
                        Company = company1,
                        CompanyId = company1.Id,
                        Name = "Sales"
                    }, new Department
                    {
                        Company = company1,
                        CompanyId = company1.Id,
                        Name = "IT"
                    }, new Department
                    {
                        Company = company2,
                        CompanyId = company2.Id,
                        Name = "Marketing"
                    }, new Department
                    {
                        Company = company2,
                        CompanyId = company2.Id,
                        Name = "IT"
                    }
                    );
                await context.SaveChangesAsync();
            }
            if (!context.Employees.Any())
            {
                // Add some employees

            }
        }
    }
}
