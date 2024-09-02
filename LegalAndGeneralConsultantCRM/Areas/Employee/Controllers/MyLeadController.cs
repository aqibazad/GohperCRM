using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models;
using LegalAndGeneralConsultantCRM.Models.ActivitiesLog;
using LegalAndGeneralConsultantCRM.Models.CalendarEvents;
using LegalAndGeneralConsultantCRM.Models.LeadFollowUp;
using LegalAndGeneralConsultantCRM.Models.Leads;
using LegalAndGeneralConsultantCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.IO;
using System.ComponentModel;
using LegalAndGeneralConsultantCRM.ViewModels.Leads;

namespace LegalAndGeneralConsultantCRM.Areas.Employee.Controllers
{

	[Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class MyLeadController : Controller
    {

        private readonly LegalAndGeneralConsultantCRMContext _context;
        private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor; // Add IHttpContextAccessor

        public MyLeadController(LegalAndGeneralConsultantCRMContext context, UserManager<LegalAndGeneralConsultantCRMUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
		// Action method to show history for a given leadId
		public async Task<IActionResult> HistoryLead(int leadId)
		{
			// Fetching history data related to the leadId
			var historyData = await _context.LeadHistories
				.Where(h => h.LeadId == leadId)
				.Include(h => h.User) // Include User data if needed
				.Select(h => new LeadHistoryViewModel
				{
					LeadHistoryId = h.LeadHistoryId,
					LeadId = h.LeadId,
					UserName = h.User != null ? h.User.UserName : "Unknown", // Adjust as per User property
					Status = h.Status,
					LeadFollowupDate = h.LeadFollowupDate
				})
				.ToListAsync();

			return View(historyData);
		}
		public IActionResult BulkUpload()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> UploadData(IFormFile file, List<MappingViewModel> mappings, string dropdownValue)
        {
            try
            {
                OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // For non-commercial use
                                                                                                        // Check if a file was provided
                if (file == null || file.Length == 0)
                {
                    ViewBag.FileError = "Please select a file.";
                    return View("BulkUpload"); // Assuming BulkUpload is the name of your upload view
                }

                // Process the uploaded file
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    // Load the Excel package
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.First();
                        var rowCount = worksheet.Dimension.Rows;
                       
                        // Check if dropdownValue is "meta"
                        if (dropdownValue == "meta")
                        {


                            for (int row = 2; row <= rowCount; row++)
                            {
                                var lead = new Lead();

                                // Iterate through the mappings to extract data from corresponding columns
                                foreach (var mapping in mappings)
                                {

                                    // Find the column index based on the mapping
                                    var columnIndex = worksheet.Cells["1:1"].First(cell => cell.Value.ToString().Equals(mapping.ExcelColumn)).Start.Column;
                                    var value = worksheet.Cells[row, columnIndex].Value?.ToString();
                                    var sourceId = worksheet.Cells[row, 1].Value?.ToString(); // Assuming SourceId is in the first column

                                    // Map the value to the corresponding property of the Lead model
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        // Set the SourceId and UserId
                                        lead.LeadSource = sourceId;
                                 
                                        switch (mapping.DbColumn)
                                        {
                                            case "FirstName":
                                                lead.FirstName = value;
                                                break;
                                            case "LastName":
                                                lead.LastName = value;
                                                break;
                                            case "PhoneNumber":
                                                // Check if value is not null or empty
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    // Trim any leading or trailing whitespace
                                                    value = value.Trim();
                                                    // Assign the value to the PhoneNumber property
                                                    lead.PhoneNumber = value;
                                                }
                                                break;
                                            case "Gender":
                                                lead.Gender = value;
                                                break;
                                            case "CompanyName":
                                                lead.CompanyName = value;
                                                break;
                                            case "JobTitle":
                                                lead.JobTitle = value;
                                                break;
                                            case "Email":
                                                lead.Email = value;
                                                break;
                                            case "CreatedDate":
                                                // You may want to handle the CreatedDate separately
                                                break;
                                            case "Address":
                                                lead.Address = value;
                                                break;
                                            case "City":
                                                lead.City = value;
                                                break;
                                        
                                           
                                            case "State":
                                                lead.State = value;
                                                break;
                                            case "ZipCode":
                                                lead.ZipCode = value;
                                                break;
                                            case "Country":
                                                lead.Country = value;
                                                break;
                                            case "Notes":
                                                lead.Notes = value;
                                                break;
                                            case "Industry":
                                                lead.Industry = value;
                                                break;

                                            case "LeadSourceDetails":
                                                lead.LeadSourceDetails = value;
                                                break;
                                          
                                         
                                          
                                            case "ReferralId":
                                                lead.ReferralId = int.Parse(value); // Assuming ReferralId is an integer
                                                break;
                                           
                                            default:
                                                // Handle unknown DbColumn values or add custom logic
                                                break;
                                        }
                                    }
                                }

                                // Add the lead to the context for saving
                                _context.Leads.Add(lead);
                                // Save changes to the database
                                await _context.SaveChangesAsync();
                                return RedirectToAction("AllLead");

                            }
                        }
                        else
                        {
                            // Store all values from the first column in SourceId

                            for (int row = 2; row <= rowCount; row++)
                            {
                                var lead = new Lead();

                                // Iterate through the mappings to extract data from corresponding columns
                                foreach (var mapping in mappings)
                                {
                                    // Find the column index based on the mapping
                                    var columnIndex = worksheet.Cells["1:1"].First(cell => cell.Value.ToString().Equals(mapping.ExcelColumn)).Start.Column;
                                    var value = worksheet.Cells[row, columnIndex].Value?.ToString();
                                    var sourceId = worksheet.Cells[row, 1].Value?.ToString(); // Assuming SourceId is in the first column

                                    // Map the value to the corresponding property of the Lead model
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        // Set the SourceId and UserId
                                        lead.LeadSource = sourceId;
                                       
                                        switch (mapping.DbColumn)
                                        {
                                            case "FirstName":
                                                lead.FirstName = value;
                                                break;
                                            case "LastName":
                                                lead.LastName = value;
                                                break;
                                            case "PhoneNumber":
                                                // Check if value is not null or empty
                                                if (!string.IsNullOrEmpty(value))
                                                {
                                                    // Trim any leading or trailing whitespace
                                                    value = value.Trim();
                                                    // Assign the value to the PhoneNumber property
                                                    lead.PhoneNumber = value;
                                                }
                                                break;
                                            case "Gender":
                                                lead.Gender = value;
                                                break;
                                            case "CompanyName":
                                                lead.CompanyName = value;
                                                break;
                                            case "JobTitle":
                                                lead.JobTitle = value;
                                                break;
                                            case "Email":
                                                lead.Email = value;
                                                break;
                                            case "CreatedDate":
                                                // You may want to handle the CreatedDate separately
                                                break;
                                            case "Address":
                                                lead.Address = value;
                                                break;
                                            case "City":
                                                lead.City = value;
                                                break;
                                          
                                            case "State":
                                                lead.State = value;
                                                break;
                                            case "ZipCode":
                                                lead.ZipCode = value;
                                                break;
                                            case "Country":
                                                lead.Country = value;
                                                break;
                                            case "Notes":
                                                lead.Notes = value;
                                                break;
                                            case "Industry":
                                                lead.Industry = value;
                                                break;

                                            case "LeadSourceDetails":
                                                lead.LeadSourceDetails = value;
                                                break;
                                            
                                           
                                            case "ReferralId":
                                                lead.ReferralId = int.Parse(value); // Assuming ReferralId is an integer
                                                break;
                                          
                                            default:
                                                // Handle unknown DbColumn values or add custom logic
                                                break;
                                        }
                                    }
                                }

                                // Add the lead to the context for saving
                                _context.Leads.Add(lead);
                                // Save changes to the database
                                await _context.SaveChangesAsync();
                                return RedirectToAction("AllLead");
                            }
                        }

                        


                    }
                   

                }

                return RedirectToAction("AllLead");


            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine("Error occurred: " + ex.Message);

                // Display a generic error message to the user
                ViewBag.ErrorMessage = "An error occurred while uploading data. Please try again.";
                return View("BulkUpload"); // Assuming BulkUpload is the name of your upload view
            }
        }

        // Helper method to get the column index based on header name
        private int GetColumnIndex(ExcelWorksheet worksheet, string columnName)
        {
            var colCount = worksheet.Dimension?.Columns ?? 0;
            for (int col = 1; col <= colCount; col++)
            {
                if (worksheet.Cells[1, col].Value?.ToString().Trim() == columnName)
                {
                    return col;
                }
            }
            return -1; // Column not found
        }

        // Helper method to set Lead property value based on column mapping

        public async Task<IActionResult> Index()
        {
             
            return View();
        }
		public async Task<IActionResult> LeadHistory()
		{
            var user = await _userManager.GetUserAsync(User);
            var currentUser = user.Id;

            if (currentUser == null)
            {
                return Json(new { data = new List<object>() });
            }
            var leads = await _context.Leads.Where(lead => lead.UserId == currentUser).ToListAsync();
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

		public async Task<IActionResult> InProcessLead()
        {

            return View();
        }
        public async Task<JsonResult> GetAllocatedLeadData()
        {
			var user = await _userManager.GetUserAsync(User);
			var currentUser = user.Id;

			if (currentUser == null)
            {
                return Json(new { data = new List<object>() });
            }

            var excludedStatuses = new List<string> { "Dead Lead", "Converted Lead" };

            var allocatedLeads = _context.Leads
                      .Where(lead => lead.IsLeadAssign == true &&
                                     lead.Assignees.Any(assignee => assignee.EmployeeId == currentUser) &&
                                     (lead.FollowUps.Any(followUp => followUp.Status == null) ||
                                     !lead.FollowUps.Any(followUp => excludedStatuses.Contains(followUp.Status))))
                      .Include(l => l.Referral)
                      .Include(l => l.Assignees).ThenInclude(lae => lae.Employee)
                      .Include(l => l.FollowUps)
                      .ToList();


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

        public async Task<IActionResult> Edit(int id)
        {
            //var brand = await _context.Brands.ToListAsync();
            //var productTypes = await _context.ProductTypes.ToListAsync();
            //var product = await _context.Products.ToListAsync();
            //var interestTags = await _context.InterestedTags.ToListAsync();
            var referrals = await _context.Referrals.ToListAsync();

            //ViewBag.BrandList = new SelectList(brand, "BrandId", "ProjectName");
            //ViewBag.ProductTypes = new SelectList(productTypes, "ProductTypeId", "Description");
            //ViewBag.ProductsList = new SelectList(product, "ProductId", "ProductName");
            //ViewBag.InterestedTags = interestTags ?? new List<InterestedTag>();

            ViewBag.Referral = new SelectList(referrals, "ReferralId", "Name");
            var leadViewModel = await _context.Leads.FindAsync(id);
            if (leadViewModel == null)
            {
                return NotFound();
            }


            return View(leadViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditLeadForm(int? leadId, Lead vm)
        {
            if (leadId == null)
            {
                return NotFound();
            }

            try
            {
                var existingLead = await _context.Leads.FindAsync(leadId);

                if (existingLead == null)
                {
                    return NotFound();
                }

                // Update existing lead properties from vm
                existingLead.FirstName = vm.FirstName;
                existingLead.LastName = vm.LastName;
                existingLead.Gender = vm.Gender;
                existingLead.CompanyName = vm.CompanyName;
                existingLead.JobTitle = vm.JobTitle;
                existingLead.Email = vm.Email;
                existingLead.PhoneNumber = vm.PhoneNumber;
                existingLead.Address = vm.Address;
                existingLead.City = vm.City;
                existingLead.State = vm.State;
                existingLead.ZipCode = vm.ZipCode;
                existingLead.Country = vm.Country;
                existingLead.Notes = vm.Notes;
                existingLead.Industry = vm.Industry;
                existingLead.LeadSource = vm.LeadSource;
                existingLead.LeadSourceDetails = vm.LeadSourceDetails;
                //existingLead.SelectedInterestedTags = vm.SelectedInterestedTags;
                //existingLead.BrandId = vm.BrandId;
                //existingLead.ProductTypeId = vm.ProductTypeId;
                //existingLead.ProductId = vm.ProductId;
                existingLead.ReferralId = vm.ReferralId;
                existingLead.IsLeadAssign = true;
 
                 _context.Update(existingLead);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeadExists(leadId.Value))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["Success"] = "Lead data successfully edited.";
            return RedirectToAction("InProcessLead");
        }


        private bool LeadExists(int id)
        {
            return _context.Leads.Any(e => e.LeadId == id);
        }

        public async Task<IActionResult> CreateFollowUp(int id)
        {
            var lead = await _context.Leads.FindAsync(id);

            if (lead == null)
            {
                return NotFound();
            }

             ViewBag.FirstName = lead.FirstName;
            ViewBag.LeadId = id; // Set LeadId in ViewBag

            return View();
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
                        Status =  f.Status,
                        ReminderDate = f.Reminder.HasValue ? f.Reminder.Value.ToString("yyyy-MM-dd") : null,
                        Description =    f.Description
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


        public async Task<IActionResult> DeadLead()
        {
             
            return View();
        }
        public async Task<JsonResult> GetDeadLeads()
        {
            // Replace the current user retrieval with session based approach
            
			var user = await _userManager.GetUserAsync(User);
			var currentUserString = user.Id;
			if (currentUserString == null)
            {
                return Json(new { data = new List<object>() });
            }

            var deadLeads = _context.FollowUps
                .Where(followUp => followUp.EmployeeId == currentUserString && followUp.Status == "Dead Lead")
                .Include(f => f.Lead)
                .Select(f => new
                {
                    f.Lead.LeadId,
                    f.Lead.FirstName,
                    f.Lead.LastName,
                    FullName = f.Lead.FirstName + " " + f.Lead.LastName,  

                    f.Lead.PhoneNumber,
                    f.Lead.Email,
                    ReferralName = f.Lead.Referral != null ? f.Lead.Referral.Name : string.Empty,
                    FollowUpStatus = f.Status
                })
                .ToList();

            return Json(new { data = deadLeads });
        }

        public async Task<IActionResult> ConvertedLead()
        {

            return View();
        }

        public async Task<JsonResult> GetConvertedLead()
        {
			// Replace the current user retrieval with session based approach
			var user = await _userManager.GetUserAsync(User);
			var currentUserString = user.Id;
			if (currentUserString == null)
            {
                return Json(new { data = new List<object>() });
            }

            var convertedLeads = _context.FollowUps
                .Where(followUp => followUp.EmployeeId == currentUserString && followUp.Status == "Converted Lead")
                .Include(f => f.Lead)
                .ThenInclude(l => l.Assignees)
                .Select(f => new
                {
                    f.Lead.LeadId,
                    f.Lead.FirstName,
                    f.Lead.LastName,
                    f.Lead.PhoneNumber,
                    f.Lead.Email,
                    f.Lead.Gender,
                    FullName = $"{f.Lead.FirstName} {f.Lead.LastName}",
                    IsEnrolled = f.Lead.IsEnrolled, // Include IsEnrolled property
                    ReferralName = f.Lead.Referral != null ? f.Lead.Referral.Name : string.Empty,
                    EmployeeFullName = f.Lead.Assignees != null && f.Lead.Assignees.Any() && f.Lead.Assignees.First().Employee != null
                     ? $"{f.Lead.Assignees.First().Employee.FirstName} {f.Lead.Assignees.First().Employee.LastName}"
                     : string.Empty,
                    FollowUpStatus = f.Status
                })
                .ToList();

            return Json(new { data = convertedLeads });
        }

        public async Task<IActionResult> UniversityProgram()
        {

            return View();
        }
        public async Task<IActionResult> Country()
        {

            return View();
        }
		public async Task<IActionResult> Programs()
		{

			return View();
		}
		public async Task<IActionResult> Followup()
        {

            return View();
        }
        public async Task<IActionResult> Status()
        {

            return View();
        }

        public async Task<IActionResult> Follow()
        {

            return View();
        }

        public async Task<IActionResult> FollowupStatus()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LeadForm(Lead vm)
        {
            var source = vm.LeadSour;
            if (source != null)
            {
                vm.LeadSource = source;
            }
            var user = await _userManager.GetUserAsync(User);
			var currentUser = user.Id;
			try
            {
                
                // Check if the email or phone number already exists in the database
                var existingLead = await _context.Leads
                    .FirstOrDefaultAsync(l => l.Email == vm.Email || l.PhoneNumber == vm.PhoneNumber);

                if (existingLead != null)
                {
                    return Json(new { success = true, message = "A lead with the same email or phone number already exists." });
                }

                // Create a new Lead object
                var lead = new Lead
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Gender = vm.Gender,
                    UserId = currentUser,
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    CreatedDate = DateTime.UtcNow,
                    Industry = vm.Industry,
                    LeadSource = vm.LeadSource,
                    City = vm.City,
                    Notes = vm.Notes,
                    Address = vm.Address,
					Apt = vm.Apt,
					ZipCode = vm.ZipCode,
					phoneNumber2 = vm.phoneNumber2,
					Street1 = vm.Street1,
					Street2 = vm.Street2,
                    LeadSourceDetails = vm.LeadSourceDetails,
                    IsLeadAssign = true
                };

                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                var leadId = lead.LeadId;

                // Create a new ActivityLog object
                var activityLog = new ActivityLog
                {
                    LeadId = leadId,
                    UserId = currentUser,
                    Status = "New Lead",
                    Action = "New Lead Added",
                    ActivityLogDate = DateTime.Now
                };

                // Add ActivityLog record to the context
                _context.ActivityLogs.Add(activityLog);

                // Create a new LeadAssignEmployee object
                var leadAssignEmployee = new LeadAssignEmployee
                {
                    EmployeeId = currentUser,
                    LeadId = leadId,
                    AssignmentDate = DateTime.Now
                };

                _context.LeadAssignEmployees.Add(leadAssignEmployee);
                await _context.SaveChangesAsync();

				// Return a success response
				return RedirectToAction("InProcessLead", "MyLead", new { area = "Employee" });
			}
            catch (Exception ex)
            {
                // Log the exception (optional)
                // Return an error response
                return Json(new { success = false, message = "An error occurred while saving the lead. Please try again.", error = ex.Message });
            }
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


        [HttpPost]
        public async Task<IActionResult> EditLead(Lead vm)
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

        public async Task<JsonResult> GetAllUniversityData()
        {
            var universities = await _context.Universities
                .Include(u => u.UniversityCourses)
                    .ThenInclude(uc => uc.Course)
                .Select(u => new
                {
                    UniversityId = u.UniversityId,
                    Name = u.Name,
                    Country = u.Country,
                    Website = u.Website,  
                    Courses = u.UniversityCourses.Select(uc => new
                    {
                        UniversityCourseId = uc.UniversityCourseId,
                        CourseId = uc.CourseId,
                        CourseName = uc.Course.Name,
                        TuitionFee = uc.TuitionFee,
                        DurationInYears = uc.Course.DurationInYears,  
                        
                    }).ToList()  
                })
                .ToListAsync();

            return Json(new { data = universities });
        }
        public async Task<JsonResult> LeadByStatus(string status)
        {
			var user = await _userManager.GetUserAsync(User);
			var currentUser = user.Id;
			var leads = await _context.Leads
                .Where(l => l.UserId == currentUser)
                .SelectMany(l => l.FollowUps.Where(f => f.Status == status).Select(f => new
                {
                    l.FirstName,
                    l.LastName,
                    FullName = l.FirstName +  " "+  l.LastName,
                    l.Email,
                    l.Gender,
                    l.PhoneNumber,
                    Status = f.Status,
                    LeadId = l.LeadId
                }))
                .ToListAsync();

            return Json(new { data = leads });
        }
    }
}


