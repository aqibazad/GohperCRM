using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models;
using LegalAndGeneralConsultantCRM.ViewModels;
using LegalAndGeneralConsultantCRM.ViewModels.Leads;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalAndGeneralConsultantCRM.Areas.Employee.Controllers
{
	[Area("Employee")]
    [Authorize(Roles = "Employee")]  

    public class EmployeeDashboardController : Controller
	{
		private readonly LegalAndGeneralConsultantCRMContext _dbContext;
		private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;

		public EmployeeDashboardController(LegalAndGeneralConsultantCRMContext dbContext, UserManager<LegalAndGeneralConsultantCRMUser> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}
		// GET: Employee/EmployeeDashboard/EditSocialMedia
		// GET: Employee/EmployeeDashboard/EditSocialMedia
		public async Task<IActionResult> EditSocialMedia()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound();
			}

			var socialMedia = await _dbContext.SocialMedia
				.FirstOrDefaultAsync(sm => sm.UserId == user.Id);

			var viewModel = new SocialMediaViewModel
			{
				Id = socialMedia?.Id ?? 0,
				UserId = user.Id,
				Facebook = socialMedia?.Facebook,
				Twitter = socialMedia?.Twitter,
				Instagram = socialMedia?.Instagram,
				TikTok = socialMedia?.TikTok,
				LinkedIn = socialMedia?.LinkedIn,
				YouTube = socialMedia?.YouTube,
				Snapchat = socialMedia?.Snapchat,
				Pinterest = socialMedia?.Pinterest,
				Reddit = socialMedia?.Reddit,
				Other = socialMedia?.Other
			};

			return View(viewModel);
		}

		// POST: Employee/EmployeeDashboard/EditSocialMedia
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditSocialMedia(SocialMediaViewModel model)
		{
			

			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound();
			}

			var socialMedia = await _dbContext.SocialMedia
				.FirstOrDefaultAsync(sm => sm.UserId == user.Id);

			if (socialMedia == null)
			{
				socialMedia = new SocialMedia
				{
					UserId = user.Id
				};
				_dbContext.SocialMedia.Add(socialMedia);
			}

			socialMedia.Facebook = model.Facebook;
			socialMedia.Twitter = model.Twitter;
			socialMedia.Instagram = model.Instagram;
			socialMedia.TikTok = model.TikTok;
			socialMedia.LinkedIn = model.LinkedIn;
			socialMedia.YouTube = model.YouTube;
			socialMedia.Snapchat = model.Snapchat;
			socialMedia.Pinterest = model.Pinterest;
			socialMedia.Reddit = model.Reddit;
			socialMedia.Other = model.Other;

			await _dbContext.SaveChangesAsync();

			return RedirectToAction(nameof(EditSocialMedia));
		}
		
        public async Task<IActionResult> Index()
		{
			 

			var user = await _userManager.GetUserAsync(User);
			var currentUserId = user.Id;

			if (string.IsNullOrEmpty(currentUserId))
			{
				// Handle the case where user ID is not found in session (not authenticated)
				return RedirectToAction("Login", "Account", new { area = "Identity" });
			}

			var mylead = await _dbContext.Leads
							.Where(sp => sp.UserId == currentUserId)
							.CountAsync();

			ViewBag.MyLead = mylead;
			var enrolled = await _dbContext.VisaApplications
										.Where(va => va.UserId == currentUserId && va.VisaStatus == "accept")
										.Select(va => va.LeadId)
										.Distinct()
										.CountAsync();
			ViewBag.Visa = enrolled;
			var userLeadIds = await _dbContext.Leads
										   .Where(l => l.UserId == currentUserId)
										   .Select(l => l.LeadId)
										   .ToListAsync();
			var hotLeadCount = await _dbContext.FollowUps
											.Where(f => userLeadIds.Contains(f.LeadId ?? 0) && f.Status == "Hot Lead")
											.CountAsync();
			ViewBag.HotLeadCount = hotLeadCount;
			var dead = await _dbContext.FollowUps
										.Where(f => userLeadIds.Contains(f.LeadId ?? 0) && f.Status == "Dead Lead")
										.CountAsync();
			ViewBag.dead = dead;

			var Future = await _dbContext.FollowUps
										.Where(f => userLeadIds.Contains(f.LeadId ?? 0) && f.Status == "Future Lead")
										.CountAsync();
			ViewBag.Future = Future;

			var visa = await _dbContext.VisaApplications
									   .Where(va => va.UserId == currentUserId)
									   .Select(va => va.LeadId)
									   .Distinct()
									   .CountAsync();
			ViewBag.Total = visa;



			var userLeads = await _dbContext.Leads
								   .Where(lead => lead.UserId == currentUserId)
								   .Select(lead => lead.LeadId)
								   .ToListAsync();

			var studentCount = await _dbContext.Students
											  .Where(student => userLeads.Contains(student.LeadId ?? 0))
											  .CountAsync();

			ViewBag.StudentCount = studentCount;





			if (currentUserId == null)
			{
				return Unauthorized();
			}
			var today = DateTime.UtcNow.Date; // Get today's date in UTC
			var activityLogs = await _dbContext.ActivityLogs
							.Where(al => al.UserId == currentUserId && al.ActivityLogDate.HasValue && al.ActivityLogDate.Value.Date == today)
							.OrderBy(al => al.ActivityLogDate) // Order by date to get the latest ones
							.Take(4) // Take only the first 4 records
							.ToListAsync();
			// Find the leads assigned to the current user
			var leads = await _dbContext.Leads
				.Include(l => l.StudentMessages)
				.Where(l => l.UserId == currentUserId)
				.ToListAsync();

			// Create a view model to store the messages
			var messagesVM = new List<MessageVM>();

			foreach (var lead in leads)
			{
				foreach (var studentMessage in lead.StudentMessages)
				{
					var leadActivityLogs = activityLogs
			.Where(al => al.LeadId == lead.LeadId)
			.ToList();
					messagesVM.Add(new MessageVM
					{
						Leads = lead,
						StudentMessages = studentMessage,
						ActivityLogs = leadActivityLogs // Now it's correctly a List<ActivityLog>

					});
					
				}
			}
			// Fetch social media data
			var socialMedia = await _dbContext.SocialMedia
									.Where(sm => sm.UserId == currentUserId)
									.FirstOrDefaultAsync();

			// Add social media data to ViewBag
			ViewBag.Facebook = socialMedia?.Facebook;
			ViewBag.Twitter = socialMedia?.Twitter;
			ViewBag.Instagram = socialMedia?.Instagram;
			ViewBag.TikTok = socialMedia?.TikTok;
			ViewBag.LinkedIn = socialMedia?.LinkedIn;
			ViewBag.YouTube = socialMedia?.YouTube;
			ViewBag.Snapchat = socialMedia?.Snapchat;
			ViewBag.Pinterest = socialMedia?.Pinterest;
			ViewBag.Reddit = socialMedia?.Reddit;
			ViewBag.Other = socialMedia?.Other;
			return View(messagesVM);
		}

		// GET: /Employee/EmployeeDashboard/NotificationsPartial
		[HttpGet]
		public async Task<IActionResult> NotificationsJson()
		{
			var userId = _userManager.GetUserId(User);
			var notifications = await _dbContext.Notifications
				.Where(n => n.UserId == userId && n.IsRead==false)
				.OrderByDescending(n => n.NotificationTime)
				.ToListAsync();

			var viewModel = notifications.Select(n => new NotificationViewModel
			{
				NotificationId = n.NotificationId,
				Message = n.Message,
				IsRead = n.IsRead,
				NotificationTime = n.NotificationTime
			}).ToList();

			return Json(viewModel);
		}

		// POST: /Employee/EmployeeDashboard/MarkAsRead
		[HttpPost]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var notification = await _dbContext.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _dbContext.SaveChangesAsync();
            }

            return Ok();
        }

        // GET: /Employee/EmployeeDashboard/UnreadCount
        public async Task<IActionResult> UnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            var unreadCount = await _dbContext.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new { count = unreadCount });
        }
		[HttpPost]
		public async Task<IActionResult> MarkAllAsRead()
		{
			var userId = _userManager.GetUserId(User);
			var notifications = await _dbContext.Notifications
				.Where(n => n.UserId == userId && !n.IsRead)
				.ToListAsync();

			foreach (var notification in notifications)
			{
				notification.IsRead = true;
			}

			await _dbContext.SaveChangesAsync();

			return Ok();
		}
		public async Task<JsonResult> GetUserActivity()
        {
            var user = await _userManager.GetUserAsync(User);
            var currentUserId = user.Id;

            // Get today's date
            var today = DateTime.Today;

            // Fetch all follow-ups with a status of "Dead Lead" for today for the current user
            var deadLeads = await _dbContext.ActivityLogs
                .Include(f => f.Lead)
                .Include(f => f.User) // Include the User data
                .Where(f => f.ActivityLogDate.HasValue
                            && f.ActivityLogDate.Value.Date == today
                            && f.UserId == currentUserId) // Filter by current user ID
                .OrderByDescending(f => f.ActivityLogDate) // Optional: order by date
                .Select(f => new
                {
                    f.Lead.LeadId,
                    f.Lead.FirstName,
                    f.Lead.LastName,
                    FullName = f.Lead.FirstName + " " + f.Lead.LastName,
                    LeadPhoneNumber = f.Lead.PhoneNumber,
                    Email = f.Lead.Email,
                    FollowUpStatus = f.Status,
                    UserName = f.User.FirstName + " " + f.User.LastName, // Include the UserName
                    UserId = f.UserId,
                    ActivityLogId = f.ActivityLogId,
                    Action = f.Action,
                    ActivityLogDate = f.ActivityLogDate
                })
                .ToListAsync();

            return Json(new { data = deadLeads });
        }



    }
}
