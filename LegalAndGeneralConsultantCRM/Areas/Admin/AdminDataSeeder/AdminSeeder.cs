using Microsoft.AspNetCore.Identity;
using LegalAndGeneralConsultantCRM.Areas.Identity.Data;

namespace LegalAndGeneralConsultantCRM.Areas.Admin.AdminDataSeeder
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminUserAndRole(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<LegalAndGeneralConsultantCRMUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create the "Admin" role if it doesn't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create the admin user if it doesn't exist
            var adminUser = await userManager.FindByNameAsync("admin@gmail.com");
            if (adminUser == null)
            {
                var user = new LegalAndGeneralConsultantCRMUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true,
                    FirstName = "Admin"
                };


                var password = "Admin123@#$";
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }

}
