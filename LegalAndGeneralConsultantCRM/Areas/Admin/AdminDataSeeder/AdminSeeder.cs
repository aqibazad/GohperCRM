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
            

            // Create the admin user if it doesn't exist
            
        }
    }

}
