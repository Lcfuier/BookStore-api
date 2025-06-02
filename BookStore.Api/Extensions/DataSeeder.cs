using BookStore.Domain.Constants;
using BookStore.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace BookStore.Api.Extensions
{
    public class DbSeeder
    {
        public static async Task SeedDefaulData(IServiceProvider serviceProvider)
        {
            var userMgr = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var roleMgr = serviceProvider.GetService<RoleManager<IdentityRole>>();
            //adding role
            if (!await roleMgr.RoleExistsAsync(Roles.User))
            {
                await roleMgr.CreateAsync(new IdentityRole(Roles.User));
            }
            if (!await roleMgr.RoleExistsAsync(Roles.Librarian))
            {
                await roleMgr.CreateAsync(new IdentityRole(Roles.Librarian));
            }
            if (!await roleMgr.RoleExistsAsync(Roles.Admin))
            {
                await roleMgr.CreateAsync(new IdentityRole(Roles.Admin));
            }
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "phamkhanhduy.contact@gmail.com",
                EmailConfirmed = true,
                FirstName="Duy",
                LastName="Khánh",
                PhoneNumber="0866690400"
            };
            var isAdminExist = await userMgr.FindByEmailAsync(admin.Email);
            if (isAdminExist is null)
            {
                await userMgr.CreateAsync(admin, "Duyvip@13");
                await userMgr.AddToRoleAsync(admin, Roles.Admin);
            }
            else
            {
                // Nếu user đã tồn tại nhưng chưa có role Admin, gán lại
                var roles = await userMgr.GetRolesAsync(isAdminExist);
                if (!roles.Contains(Roles.Admin))
                {
                    await userMgr.AddToRoleAsync(isAdminExist, Roles.Admin);
                }
            }
        }
    }
}
