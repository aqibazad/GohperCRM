using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models.Branches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalAndGeneralConsultantCRM.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class BranchController : Controller
    {
        private readonly LegalAndGeneralConsultantCRMContext _context;
        private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;

        public BranchController(LegalAndGeneralConsultantCRMContext context, UserManager<LegalAndGeneralConsultantCRMUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            

            return View();
        }

        public async Task<JsonResult> GetBranches()
        {
            var universities = await _context.Branches.ToListAsync();

            return Json(new { data = universities });
        }
        [HttpPost]
        public async Task<IActionResult> AddBranch([FromBody] Branch branch)
        {
            if (ModelState.IsValid)
            {
                // Check if the BranchCode already exists in the database
                bool branchCodeExists = await _context.Branches
                    .AnyAsync(b => b.BranchCode == branch.BranchCode);

                if (branchCodeExists)
                {
                    return Json(new { success = false, message = "Branch Code already exists." });
                }

                // If the BranchCode does not exist, add the new branch
                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Branch added successfully." });
            }

            return Json(new { success = false, message = "Failed to add branch." });
        }

        [HttpPost]
        public async Task<IActionResult> EditBranch([FromBody] Branch branch)
        {
            if (ModelState.IsValid)
            {
                var existingBranch = await _context.Branches.FindAsync(branch.BranchId);
                if (existingBranch != null)
                {
                    existingBranch.BranchName = branch.BranchName;
                    existingBranch.BranchCode = branch.BranchCode;
                    existingBranch.Description = branch.Description;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Branch updated successfully." });
                }
                return Json(new { success = false, message = "Branch not found." });
            }
            return Json(new { success = false, message = "Failed to update branch." });
        }

        // POST: Admin/Branch/DeleteBranch
     
        [HttpPost]
        public async Task<IActionResult> DeleteBranch([FromBody] int branchId)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch != null)
            {
                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Branch deleted successfully." });
            }
            return Json(new { success = false, message = "Branch not found." });
        }
    }
}

