using CsvHelper;
using LegalAndGeneralConsultantCRM.Areas.Admin.SendNotificationHub;
using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models;
using LegalAndGeneralConsultantCRM.Models.ActivitiesLog;
using LegalAndGeneralConsultantCRM.Models.CalendarEvents;
using LegalAndGeneralConsultantCRM.Models.LeadFollowUp;
using LegalAndGeneralConsultantCRM.Models.Leads;
using LegalAndGeneralConsultantCRM.ViewModels.Leads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LegalAndGeneralConsultantCRM.Areas.Admin.Controllers
{

	[Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class LeadController : Controller
    {

        private readonly LegalAndGeneralConsultantCRMContext _context;
        private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public LeadController(LegalAndGeneralConsultantCRMContext context, UserManager<LegalAndGeneralConsultantCRMUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }
        public async Task<IActionResult> LeadForm()
        { 
            var referrals = await _context.Referrals.ToListAsync();
 
            ViewBag.Referral = new SelectList(referrals, "ReferralId", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LeadForm(Lead vm)
        {
            // Check if a lead with the same email or phone number already exists
            var existingLead = await _context.Leads
                .FirstOrDefaultAsync(l => l.Email == vm.Email || l.PhoneNumber == vm.PhoneNumber);

            if (existingLead != null)
            {
               
                TempData["error"] = "A lead with the same email or phone number already exists.";
                return View("LeadForm"); // Return the view with the existing model to display errors
            }

            // If no existing lead is found, proceed with adding the new lead
            var lead = new Lead
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Gender = vm.Gender,
                CompanyName = vm.CompanyName,
                JobTitle = vm.JobTitle,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                CreatedDate = DateTime.Now,
                Address = vm.Address,
                City = vm.City,
                State = vm.State,
                ZipCode = vm.ZipCode,
                Country = vm.Country,
                Notes = vm.Notes,
                Industry = vm.Industry,
                LeadSource = vm.LeadSource,
                LeadSourceDetails = vm.LeadSourceDetails,
                ReferralId = vm.ReferralId,
                
            };

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            // Set a success message in TempData
            TempData["SuccessMessage"] = "Lead saved successfully.";

            return RedirectToAction("AllLead");
        }

        [HttpPost]
        public async Task<IActionResult> EditLead([FromBody] Lead lead)
        {
            try
            {
                // Update lead in the database
                var existingLead = await _context.Leads.FindAsync(lead.LeadId);
                if (existingLead != null)
                {
                    existingLead.FirstName = lead.FirstName;
                    existingLead.LastName = lead.LastName;
                    existingLead.PhoneNumber = lead.PhoneNumber;
                    existingLead.Email = lead.Email;
                    existingLead.LeadSource = lead.LeadSource;
                    existingLead.Address = lead.Address;
                    existingLead.Gender = lead.Gender;
                    existingLead.City = lead.City;
                    existingLead.Notes = lead.Notes;

                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Lead updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Lead not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating lead: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLead([FromBody] DeleteLeadRequest request)
        {
            try
            {
                var existingLead = await _context.Leads.FindAsync(request.LeadId);
                if (existingLead != null)
                {
                    _context.Leads.Remove(existingLead);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Lead deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Lead not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting lead: " + ex.Message });
            }
        }

        public class DeleteLeadRequest
        {
            public int LeadId { get; set; }
        }

        public IActionResult BulkUpload()
        {
            return View();
        }

        // POST: /Admin/Lead/UploadBulkData

        [HttpPost]
        public async Task<IActionResult> UploadBulkData(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("file", "Please select a file for upload.");
                return View("BulkUpload");
            }

            var leads = new List<Lead>();
            var duplicateEntries = new List<string>(); // To keep track of duplicates
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            try
            {
                if (fileExtension == ".xlsx")
                {
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            if (package.Workbook.Worksheets.Count == 0)
                            {
                                ModelState.AddModelError("file", "Excel file doesn't contain any worksheets.");
                                return View("BulkUpload");
                            }

                            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                            int rowCount = worksheet.Dimension.Rows;

                            for (int row = 2; row <= rowCount; row++)
                            {
                                var email = worksheet.Cells[row, 5].Value?.ToString();
                                var phone = worksheet.Cells[row, 3].Value?.ToString();
                                var gender = worksheet.Cells[row, 4].Value?.ToString();

                                // Validate email format
                                if (!IsValidEmail(email))
                                {
                                    ModelState.AddModelError("file", $"Invalid email format at row {row}: {email}");
                                    return View("BulkUpload");
                                }

                                // Validate gender
                                if (!IsValidGender(gender))
                                {
                                    ModelState.AddModelError("file", $"Invalid gender value at row {row}: {gender}");
                                    return View("BulkUpload");
                                }

                                // Check for existing lead with the same email or phone number
                                if (LeadExists(email, phone))
                                {
                                    duplicateEntries.Add($"Row {row} (Email: {email}, Phone: {phone})");
                                    continue; // Skip adding this lead to the list
                                }

                                var lead = new Lead
                                {
                                    FirstName = worksheet.Cells[row, 1].Value?.ToString(),
                                    LastName = worksheet.Cells[row, 2].Value?.ToString(),
                                    PhoneNumber = phone,
                                    Gender = gender,
                                    Email = email,
                                    LeadSource = worksheet.Cells[row, 6].Value?.ToString(),
                                    Address = worksheet.Cells[row, 7].Value?.ToString(),
                                    CreatedDate = DateTime.Now
                                };

                                leads.Add(lead);
                            }
                        }
                    }
                }
                else if (fileExtension == ".csv")
                {
                    using (var stream = new StreamReader(file.OpenReadStream()))
                    using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<dynamic>().ToList();

                        foreach (var record in records)
                        {
                            var email = record.Email;
                            var phone = record.PhoneNumber;
                            var gender = record.Gender;

                            // Validate email format
                            if (!IsValidEmail(email))
                            {
                                ModelState.AddModelError("file", $"Invalid email format: {email}");
                                return View("BulkUpload");
                            }

                            // Validate gender
                            if (!IsValidGender(gender))
                            {
                                ModelState.AddModelError("file", $"Invalid gender value: {gender}");
                                return View("BulkUpload");
                            }

                            // Check for existing lead with the same email or phone number
                            if (LeadExists(email, phone))
                            {
                                duplicateEntries.Add($"Email: {email}, Phone: {phone}");
                                continue; // Skip adding this lead to the list
                            }

                            var lead = new Lead
                            {
                                FirstName = record.FirstName,
                                LastName = record.LastName,
                                PhoneNumber = phone,
                                Gender = gender,
                                Email = email,
                                LeadSource = record.LeadSource,
                                Address = record.Address,
                                CreatedDate = DateTime.Now
                            };

                            leads.Add(lead);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("file", "Unsupported file format. Please upload an Excel or CSV file.");
                    return View("BulkUpload");
                }

                if (duplicateEntries.Any())
                {
                    var duplicatesMessage = "Duplicate entries found and skipped: " + string.Join(", ", duplicateEntries);
                    ModelState.AddModelError("file", duplicatesMessage);
                    return View("BulkUpload");
                }

                // Save the valid leads to the database
                _context.Leads.AddRange(leads);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Bulk leads uploaded successfully.";
                return RedirectToAction("BulkUpload", "Lead");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("file", $"An error occurred while processing the file: {ex.Message}");
                return View("BulkUpload");
            }
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return emailRegex.IsMatch(email);
        }

        private bool IsValidGender(string gender)
        {
            var validGenders = new[] { "male", "female" };
            return validGenders.Contains(gender?.ToLower());
        }

        private bool LeadExists(string email, string phone)
        {
            return _context.Leads.Any(l => l.Email == email || l.PhoneNumber == phone);
        }



        public async Task<IActionResult> AssignLeadToEmployee()
        {
            // Get leads with IsSelected == null
            var leads = await _context.Leads.Where(l => l.IsLeadAssign == null || l.IsLeadAssign == false).ToListAsync();

            // Get users with the role "Employee"
            var users = await _userManager.GetUsersInRoleAsync("Employee");

            ViewBag.Leads = leads;

            ViewBag.UserList = new SelectList(users.Select(u => new
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}"
            }), "Id", "FullName");

            return View(new LeadEmployeeAssignmentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> AssignLeadToEmployee(string Id, List<int> leadIds)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int numberOfLeadsAssigned = 0;

                    foreach (var leadId in leadIds)
                    {
                        var lead = await _context.Leads.FindAsync(leadId);

                        if (lead != null)
                        {
                            lead.IsLeadAssign = true;
							lead.UserId = Id;
							var leadAssignEmployee = new LeadAssignEmployee
                            {
                                EmployeeId = Id,
                                LeadId = leadId,
                                AssignmentDate = DateTime.Now
                            };

                            _context.LeadAssignEmployees.Add(leadAssignEmployee);
                            numberOfLeadsAssigned++;
                        }
                    }

                    _context.Leads.UpdateRange(await _context.Leads.Where(l => leadIds.Contains(l.LeadId)).ToListAsync());
                    await _context.SaveChangesAsync();

                    var assignedEmployee = await _userManager.FindByIdAsync(Id.ToString());
                    assignedEmployee.Notification = $"You have {numberOfLeadsAssigned} new lead{(numberOfLeadsAssigned > 1 ? "s" : "")} assigned by Admin.";

                    await _userManager.UpdateAsync(assignedEmployee);

                    // Check if a notification already exists for the user
                     
                        // Create and store a new notification in the database
                        var notification = new Notification
                        {
                            UserId = assignedEmployee.Id,
                            Message = assignedEmployee.Notification,
							NotificationTime = DateTime.Now
						};

                        _context.Notifications.Add(notification);
                     

                    await _context.SaveChangesAsync();

                    // Send the message to the specific user
                    await _hubContext.Clients.User(assignedEmployee.Id).SendAsync("ReceiveNotification", assignedEmployee.Notification);

                    // Set a success message in TempData
                    TempData["SuccessMessage"] = $"{numberOfLeadsAssigned} lead{(numberOfLeadsAssigned > 1 ? "s" : "")} assigned successfully.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in AssignLeadToEmployee: {ex.Message}");
            }

            return RedirectToAction("AssignLeadToEmployee");
        }
         

        [HttpGet]
        public IActionResult GetContactInfo(int referralId)
        {
            // Fetch the Referral object based on the referralId
            var referral = _context.Referrals.FirstOrDefault(r => r.ReferralId == referralId);

            return Json(new { Contact1 = referral.Contact1 });

        }

         
        public async Task<IActionResult>  AllLead()
        {
             
            return View();
        }

        public async Task<JsonResult> GetAllLeadData()
        {
            var leads = await _context.Leads
                .Include(l => l.Referral)
                .ToListAsync();

            var leadData = leads.Select(l => new
            {
                l.LeadId,
                l.FirstName,
                l.LastName,
                l.PhoneNumber,
                FullName = l.FirstName + " " + l.PhoneNumber,
                l.Email,
                l.Address,
                l.Gender,
                l.Notes,
                l.City,
                l.LeadSource,
                CreatedDate = l.CreatedDate?.Date,
             });

            return Json(new { data = leadData });
        }



        public IActionResult ExportToCSV()
        {
            var leads = _context.Leads
                .Include(l => l.Referral)
                .ToList();

            var leadData = leads.Select(l => new
            {
                l.FirstName,
                l.LastName,
                l.PhoneNumber,
                l.Email,
                CreatedDate = l.CreatedDate?.ToString("yyyy-MM-dd"), // Format date as needed
                ReferralName = l.Referral != null ? l.Referral.Name : ""
            }).ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("First Name,Last Name,Phone Number,Email,Creation Date,Referral Name");

            foreach (var lead in leadData)
            {
                sb.AppendLine($"{Escape(lead.FirstName)},{Escape(lead.LastName)},{Escape(lead.PhoneNumber)},{Escape(lead.Email)},{lead.CreatedDate},{Escape(lead.ReferralName)}");
            }

            byte[] result = Encoding.UTF8.GetBytes(sb.ToString());

            return File(result, "text/csv", "leads.csv");
        }

        private string Escape(object obj)
        {
            if (obj == null)
            {
                return "";
            }

            string text = obj.ToString();

            // Escape double quotes with double quotes
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        public IActionResult ExportToExcel()
        {
            var leads = _context.Leads
                .Include(l => l.Referral)
                .ToList();

            var leadData = leads.Select(l => new
            {
                l.FirstName,
                l.LastName,
                l.PhoneNumber,
                l.Email,
                CreatedDate = l.CreatedDate?.ToString("yyyy-MM-dd"), // Format date as needed
             }).ToList();

            byte[] result;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Leads");

                // Header row
                worksheet.Cells["A1"].Value = "First Name";
                worksheet.Cells["B1"].Value = "Last Name";
                worksheet.Cells["C1"].Value = "Phone Number";
                worksheet.Cells["D1"].Value = "Email";
                worksheet.Cells["E1"].Value = "Creation Date";
 
                // Data rows
                int row = 2;
                foreach (var lead in leadData)
                {
                    worksheet.Cells[row, 1].Value = lead.FirstName;
                    worksheet.Cells[row, 2].Value = lead.LastName;
                    worksheet.Cells[row, 3].Value = lead.PhoneNumber;
                    worksheet.Cells[row, 4].Value = lead.Email;
                    worksheet.Cells[row, 5].Value = lead.CreatedDate;
                     row++;
                }

                result = package.GetAsByteArray();
            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "leads.xlsx");
        }

        public async Task<IActionResult> AllocatedLead()
        {

            return View();
        }
        public JsonResult GetAllocatedLeadData()
         {
            var allocatedLeads = _context.Leads
                .Where(lead => lead.IsLeadAssign == true)
                .Include(l => l.Referral)
                .Include(l => l.Assignees)
                    .ThenInclude(lae => lae.Employee).Include(l => l.FollowUps)
                .ToList();

            var leadData = allocatedLeads.Select(l => new
            {
                l.FirstName,
                l.LastName,
                l.PhoneNumber,
                l.City,
                CreatedDate = l.CreatedDate?.Date,
                ReferralName = l.Referral != null ? l.Referral.Name : null,
                EmployeeFullName = l.Assignees?.FirstOrDefault()?.Employee?.FirstName + " " + l.Assignees?.FirstOrDefault()?.Employee?.LastName,
                FollowUpStatus = l.FollowUps.FirstOrDefault()?.Status,
                    FullName = $"{l.FirstName} {l.LastName}"

            });

            return Json(new { data = leadData });
        }

        public IActionResult ExportAllocatedLeadsToExcel()
        {
            var allocatedLeads = _context.Leads
                .Where(lead => lead.IsLeadAssign == true)
                .Include(l => l.Referral)
                .Include(l => l.Assignees).ThenInclude(lae => lae.Employee)
                .Include(l => l.FollowUps)
                .ToList();

            var leadData = allocatedLeads.Select(l => new
            {
                l.FirstName,
                l.LastName,
                l.PhoneNumber,
                CreatedDate = l.CreatedDate?.ToString("yyyy-MM-dd"), // Format date as needed
                ReferralName = l.Referral != null ? l.Referral.Name : "",
                EmployeeFullName = l.Assignees?.FirstOrDefault()?.Employee?.FirstName + " " + l.Assignees?.FirstOrDefault()?.Employee?.LastName,
                FollowUpStatus = l.FollowUps.FirstOrDefault()?.Status
            }).ToList();

            byte[] result;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Allocated Leads");

                // Header row
                worksheet.Cells["A1"].Value = "First Name";
                worksheet.Cells["B1"].Value = "Last Name";
                worksheet.Cells["C1"].Value = "Phone Number";
                worksheet.Cells["D1"].Value = "Creation Date";
                worksheet.Cells["F1"].Value = "Assigned Employee";
                worksheet.Cells["G1"].Value = "Follow Up Status";

                // Data rows
                int row = 2;
                foreach (var lead in leadData)
                {
                    worksheet.Cells[row, 1].Value = lead.FirstName;
                    worksheet.Cells[row, 2].Value = lead.LastName;
                    worksheet.Cells[row, 3].Value = lead.PhoneNumber;
                    worksheet.Cells[row, 4].Value = lead.CreatedDate;
                    worksheet.Cells[row, 6].Value = lead.EmployeeFullName;
                    worksheet.Cells[row, 7].Value = lead.FollowUpStatus;
                    row++;
                }

                result = package.GetAsByteArray();
            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "allocated_leads.xlsx");
        }
        public IActionResult ExportAllocatedLeadsToCSV()
        {
            var allocatedLeads = _context.Leads
                .Where(lead => lead.IsLeadAssign == true)
                .Include(l => l.Assignees).ThenInclude(lae => lae.Employee)
                .Include(l => l.FollowUps)
                .ToList();

            var leadData = allocatedLeads.Select(l => new
            {
                l.FirstName,
                l.LastName,
                l.PhoneNumber,
                CreatedDate = l.CreatedDate?.ToString("yyyy-MM-dd"), // Format date as needed
                EmployeeFullName = l.Assignees?.FirstOrDefault()?.Employee?.FirstName + " " + l.Assignees?.FirstOrDefault()?.Employee?.LastName,
                FollowUpStatus = l.FollowUps.FirstOrDefault()?.Status
            }).ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("First Name,Last Name,Phone Number,Creation Date,Assigned Employee,Follow Up Status");

            foreach (var lead in leadData)
            {
                sb.AppendLine($"{Escape(lead.FirstName)},{Escape(lead.LastName)},{Escape(lead.PhoneNumber)},{lead.CreatedDate},{Escape(lead.EmployeeFullName)},{lead.FollowUpStatus}");
            }

            byte[] result = Encoding.UTF8.GetBytes(sb.ToString());

            return File(result, "text/csv", "allocated_leads.csv");
        }

        public async Task<IActionResult> UnAllocatedLead()
        {

            return View();
        }
        public JsonResult GetUnAllocatedLeadData()
         {
            var allocatedLeads = _context.Leads
              .Where(lead => lead.IsLeadAssign == false)
              .Include(l => l.Referral)
              .ToList();

            var leadData = allocatedLeads.Select(l => new
            {
                l.FirstName,
                l.PhoneNumber,
                l.Email,
                l.City,
                CreatedDate = l.CreatedDate?.Date,
                ReferralName = l.Referral != null ? l.Referral.Name : null,
            });

            return Json(new { data = leadData });

        }

        public IActionResult ExportUnAllocatedLeadsToCSV()
        {
            var unallocatedLeads = _context.Leads
                .Where(lead => lead.IsLeadAssign == false)
                .ToList();

            var leadData = unallocatedLeads.Select(l => new
            {
                l.FirstName,
                l.PhoneNumber,
                l.Email,
                CreatedDate = l.CreatedDate?.ToString("yyyy-MM-dd") // Format date as needed
            }).ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("First Name,Phone Number,Email,Creation Date");

            foreach (var lead in leadData)
            {
                sb.AppendLine($"{Escape(lead.FirstName)},{Escape(lead.PhoneNumber)},{Escape(lead.Email)},{lead.CreatedDate}");
            }

            byte[] result = Encoding.UTF8.GetBytes(sb.ToString());

            return File(result, "text/csv", "unallocated_leads.csv");
        }
        public IActionResult ExportUnAllocatedLeadsToExcel()
        {
            var unallocatedLeads = _context.Leads
                .Where(lead => lead.IsLeadAssign == false)
                .ToList();

            var leadData = unallocatedLeads.Select(l => new
            {
                l.FirstName,
                l.PhoneNumber,
                l.Email,
                CreatedDate = l.CreatedDate?.ToString("yyyy-MM-dd") // Format date as needed
            }).ToList();

            byte[] result;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Unallocated Leads");

                // Header row
                worksheet.Cells["A1"].Value = "First Name";
                worksheet.Cells["B1"].Value = "Phone Number";
                worksheet.Cells["C1"].Value = "Email";
                worksheet.Cells["D1"].Value = "Creation Date";

                // Data rows
                int row = 2;
                foreach (var lead in leadData)
                {
                    worksheet.Cells[row, 1].Value = lead.FirstName;
                    worksheet.Cells[row, 2].Value = lead.PhoneNumber;
                    worksheet.Cells[row, 3].Value = lead.Email;
                    worksheet.Cells[row, 4].Value = lead.CreatedDate;
                    row++;
                }

                result = package.GetAsByteArray();
            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "unallocated_leads.xlsx");
        }

        public async Task<IActionResult> LeadHistory()
        {
            var leads = await _context.Leads.ToListAsync();
            var selectList = leads.Select(l => new SelectListItem
            {
                Value = l.LeadId.ToString(),
                Text = l.FirstName + " " + l.LastName
            }).ToList();

            ViewBag.Lead = selectList;
            return View();
        }

        public async Task<JsonResult> GetLeadDetails(int leadId)
        {
            var leadHistoryData = await _context.LeadHistories
                .Include(lh => lh.Lead)
                .Include(lh => lh.User)
                .Where(lh => lh.LeadId == leadId)
                .Select(lh => new
                {
                    LeadFullName = lh.Lead.FirstName + " " + lh.Lead.LastName,
                    Email = lh.Lead.Email,
                    PhoneNumber = lh.Lead.PhoneNumber,
                    Status = lh.Status,
                    SLeadFollowupDatetatus = lh.LeadFollowupDate,
                    Username = lh.User.FirstName + " " + lh.User.LastName // Assuming UserName is what you want to return
                })
                .ToListAsync();

            return Json(new { data = leadHistoryData });
        }
        public async Task<IActionResult> DeAssign()
        {

            return View();
        }
        public JsonResult Leads()
        {
            var allocatedLeads = _context.Leads
                .Where(lead => lead.IsLeadAssign == true)
                .Include(l => l.Referral)
                .Include(l => l.Assignees)
                    .ThenInclude(lae => lae.Employee)
                .Include(l => l.FollowUps)
                .ToList();

            var leadData = allocatedLeads
                .Where(l => l.FollowUps.FirstOrDefault()?.Status != "Converted Lead") // Filter out converted leads
                .Select(l => new
                {
                    l.LeadId,
                    l.FirstName,
                    l.LastName,
                    l.PhoneNumber,
                    l.City,
                    CreatedDate = l.CreatedDate?.Date,
                    ReferralName = l.Referral != null ? l.Referral.Name : null,
                    EmployeeFullName = l.Assignees?.FirstOrDefault()?.Employee?.FirstName + " " + l.Assignees?.FirstOrDefault()?.Employee?.LastName,
                    FollowUpStatus = l.FollowUps.FirstOrDefault()?.Status,
                    FullName = $"{l.FirstName} {l.LastName}"
                });

            return Json(new { data = leadData });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int leadid)
        {
            var lead = await _context.Leads.FindAsync(leadid);
            if (lead == null)
            {
                return Json(new { success = false, message = "Lead not found." });
            }

            var leadAssignEmployees = await _context.LeadAssignEmployees
                                                     .Where(l => l.LeadId == leadid)
                                                     .ToListAsync();

            var followUps = await _context.FollowUps
                                          .Where(f => f.LeadId == leadid)
                                          .ToListAsync();

            lead.IsEnrolled = false;
            lead.IsLeadAssign = false;
            _context.Leads.Update(lead);

            if (leadAssignEmployees != null && leadAssignEmployees.Any())
            {
                _context.LeadAssignEmployees.RemoveRange(leadAssignEmployees);
            }

            if (followUps != null && followUps.Any())
            {
                _context.FollowUps.RemoveRange(followUps);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Lead de-assigned successfully." });
        }


		public async Task<IActionResult> Followups()
		{

			return View();
		}
		public async Task<JsonResult> GetAllocatedLeadsData()
		{
			// Exclude leads with these statuses
			var excludedStatuses = new List<string> { "Dead Lead", "Converted Lead" };

			// Fetch all allocated leads that match the criteria
			var allocatedLeads = _context.Leads
				.Where(lead =>
					   (lead.FollowUps.Any(followUp => followUp.Status == null) ||
						!lead.FollowUps.Any(followUp => excludedStatuses.Contains(followUp.Status))))
		.Include(l => l.Referral)
		.Include(l => l.Assignees).ThenInclude(lae => lae.Employee)
		.Include(l => l.FollowUps)
		.ToList();

			// Select and project the data you need
			var leadData = allocatedLeads.Select(l => new
			{
				l.LeadId,
				l.FirstName,
				l.LastName,
				FullName = $"{l.FirstName} {l.LastName}",
				l.PhoneNumber,
				l.Email,
				l.Gender,
				l.LeadSource,
				l.City,
				l.Notes,
				CreatedDate = l.CreatedDate?.Date,
				ReferralName = l.Referral != null ? l.Referral.Name : null,
				EmployeeFirstName = l.Assignees.FirstOrDefault()?.Employee?.FirstName,
				FollowUpStatus = l.FollowUps.FirstOrDefault()?.Status,
				FollowUpDate = l.FollowUps.FirstOrDefault()?.FollowUpDate
			});

			return Json(new { data = leadData });
		}
		[HttpPost]
		public async Task<IActionResult> DeleteLead(int id)
		{
			try
			{
				// Find the lead to delete
				var lead = await _context.Leads.FindAsync(id);

				if (lead == null)
				{
					return NotFound(); // Lead not found
				}

				// Find and delete associated entries in LeadAssignEmployee
				var assignments = _context.LeadAssignEmployees.Where(a => a.LeadId == id);
				if (assignments.Any())
				{
					_context.LeadAssignEmployees.RemoveRange(assignments);
				}

				// Find and delete associated entries in FollowUp
				var followUps = _context.FollowUps.Where(f => f.LeadId == id);
				if (followUps.Any())
				{
					_context.FollowUps.RemoveRange(followUps);
				}

				// Delete from Leads
				_context.Leads.Remove(lead);

				await _context.SaveChangesAsync();

				// Return success response
				return Json(new { success = true, message = "Lead deleted successfully." });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = "An error occurred while deleting the lead. Please try again.", error = ex.Message });
			}
		}
		
		[HttpGet]
		public async Task<IActionResult> GetFollowUpDetails(int leadId)
		{
			try
			{
				var followUp = await _context.FollowUps
					.Where(f => f.LeadId == leadId)
					.Select(f => new
					{
						Status = f.Status,
						ReminderDate = f.Reminder.HasValue ? f.Reminder.Value.ToString("yyyy-MM-dd") : null,
						Description = f.Description
					})
					.FirstOrDefaultAsync();

				if (followUp == null)
				{
					return Json(new { success = false, message = "Follow-up not found" });
				}

				return Json(new { success = true, data = followUp });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		[HttpPost]
		public async Task<IActionResult> EditLeads(Lead vm)
		{
			try
			{
				var lead = await _context.Leads.FindAsync(vm.LeadId);

				if (lead == null)
				{
					return NotFound(); // Lead not found
				}

				lead.FirstName = vm.FirstName;
				lead.LastName = vm.LastName;
				lead.Email = vm.Email;
				lead.Gender = vm.Gender;
				lead.PhoneNumber = vm.PhoneNumber;
				lead.LeadSource = vm.LeadSource;
				lead.City = vm.City;
				lead.Notes = vm.Notes;


				_context.Leads.Update(lead);
				await _context.SaveChangesAsync();

				return Json(new { success = true, message = "Lead updated successfully." });
			}
			catch (Exception ex)
			{
				// Log the exception (optional)
				return Json(new { success = false, message = "An error occurred while updating the lead. Please try again.", error = ex.Message });
			}
		}
       [HttpPost]
public async Task<IActionResult> Create(FollowUp model)
{
    try
    {
        // Fetch the selected number of reminder hours from the view
        int reminderHours = 0;
        if (int.TryParse(Request.Form["Reminder"], out int hours))
        {
            reminderHours = hours;
        }

        // Check if a FollowUp record with the given LeadId already exists
        var existingFollowUp = await _context.FollowUps
            .FirstOrDefaultAsync(f => f.LeadId == model.LeadId);

        // Get the current user ID from session
        var user = await _userManager.GetUserAsync(User);
        var currentUserString = user?.Id;

        // If the current user ID is null, throw an exception
        if (string.IsNullOrEmpty(currentUserString))
        {
            throw new ApplicationException("User not logged in");
        }

        // Calculate the FollowUpReminder date
        if (model.FollowUpReminder.HasValue && reminderHours > 0)
        {
            model.Reminder = model.FollowUpReminder.Value.AddHours(-reminderHours);
        }

        if (existingFollowUp != null)
        {
					var lead = await _context.Leads.FirstOrDefaultAsync(l => l.LeadId == model.LeadId);

					var fullName = lead.FirstName + " " + (lead.LastName ?? "");

					var notification = new Notification
					{
						UserId = currentUserString,
						Message = $"Follow-up reminder for lead {fullName} check you calender for further details",
						IsRead = false,
						NotificationTime = model.Reminder  // Set notification time as FollowUpReminder
					};

					await _context.Notifications.AddAsync(notification);
					// Update existing FollowUp record
					existingFollowUp.EmployeeId = currentUserString;
            existingFollowUp.FollowUpDate = model.FollowUpDate;
            existingFollowUp.Status = model.Status;
            existingFollowUp.Reminder = model.Reminder;
            existingFollowUp.Description = model.Description;
            existingFollowUp.FollowUpCompleted = model.FollowUpCompleted;
            existingFollowUp.FollowUpReminder = model.FollowUpReminder;
            existingFollowUp.ThemeColor = model.ThemeColor;
            existingFollowUp.Priorty = model.Priorty;

            _context.FollowUps.Update(existingFollowUp);
        }
        else
        {
            // Create new FollowUp record
            model.FollowUpDate = DateTime.UtcNow;
            model.EmployeeId = currentUserString; // Assign current user as the EmployeeId
            await _context.FollowUps.AddAsync(model);
        }

        // Handle CalendarEvent creation or update
        if (model.Reminder != null)
        {
            var lead = await _context.Leads.FirstOrDefaultAsync(l => l.LeadId == model.LeadId);
            if (lead != null)
            {
                        var fullName = lead.FirstName + " " + (lead.LastName ?? "");

                        var notification = new Notification
                        {
                            UserId = currentUserString,
                            Message = $"Follow-up reminder for lead {fullName} check you calender for further details",
                            IsRead = false,
                            NotificationTime = model.Reminder  // Set notification time as FollowUpReminder
                        };

                        await _context.Notifications.AddAsync(notification);
                        var calendarEvent = await _context.CalendarEvents.FirstOrDefaultAsync(c => c.LeadId == model.LeadId);

                if (calendarEvent != null)
                {
                    // Update existing CalendarEvent
                    calendarEvent.EventDate = model.Reminder.Value;
                    calendarEvent.Name = fullName;
                    calendarEvent.ThemeColor = model.ThemeColor;
                    calendarEvent.Priorty = model.Priorty;

                    calendarEvent.Description = model.Description;
                    calendarEvent.UserId = currentUserString;

                    _context.CalendarEvents.Update(calendarEvent);
                }
                else
                {
                    // Create a new CalendarEvent
                    var cal = new CalendarEvent
                    {
                        LeadId = model.LeadId,
                        EventDate = model.Reminder.Value,
                        Name = fullName,
						ThemeColor = model.ThemeColor,
						Priorty = model.Priorty,
						UserId = currentUserString,
                        Description = model.Description
                    };
                    await _context.CalendarEvents.AddAsync(cal);
                }
            }
        }

        // Create and add LeadHistory record
        var leadHistory = new LeadHistory
        {
            LeadId = model.LeadId,
            UserId = currentUserString,
            Status = model.Status,
            LeadFollowupDate = DateTime.Now
        };
        _context.LeadHistories.Add(leadHistory);

        // Create and add ActivityLog entry
        var activityLog = new ActivityLog
        {
            LeadId = model.LeadId,
            UserId = currentUserString,
            Status = model.Status,
            Action = existingFollowUp != null ? "Follow-up updated" : "Follow-up created",
            ActivityLogDate = DateTime.Now
        };
        _context.ActivityLogs.Add(activityLog);

        // Commit all changes in a single SaveChangesAsync call
        await _context.SaveChangesAsync();

        // Return JSON success response
        return Json(new { success = true, message = "Follow-up saved successfully" });
    }
    catch (Exception ex)
    {
        // Handle exceptions
        TempData["Error"] = $"An error occurred: {ex.Message}";
        return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
    }
}


    }
}
