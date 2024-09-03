using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalAndGeneralConsultantCRM.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class DashboardController : Controller
    {
		private readonly LegalAndGeneralConsultantCRMContext _dbContext;
		private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;

		public DashboardController(LegalAndGeneralConsultantCRMContext dbContext, UserManager<LegalAndGeneralConsultantCRMUser> userManager)
		{
			_dbContext = dbContext;
			_userManager = userManager;
		}
		[HttpPost]
		public async Task<IActionResult> MarkAllAsRead()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Json(new { success = false, message = "User not logged in" });
			}

			var currentUserId = user.Id;
			var notifications = await _dbContext.Notifications
				.Where(n => n.UserId == currentUserId && !n.IsRead)
				.ToListAsync();

			foreach (var notification in notifications)
			{
				notification.IsRead = true;
			}

			_dbContext.Notifications.UpdateRange(notifications);
			await _dbContext.SaveChangesAsync();

			return Json(new { success = true });
		}

		public async Task<IActionResult> GetTodayNotifications()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Json(new { success = false, message = "User not logged in" });
			}

			var currentUserId = user.Id;
			var today = DateTime.Today;

			// Fetch today's notifications
			var todayNotifications = await _dbContext.Notifications
				.Where(n => n.UserId == currentUserId && n.NotificationTime.HasValue && n.NotificationTime.Value.Date == today && !n.IsRead)
				.OrderBy(n => n.NotificationTime)
				.ToListAsync();

			// Get total unread notifications
			var unreadNotificationCount = await _dbContext.Notifications
				.Where(n => n.UserId == currentUserId && !n.IsRead)
				.CountAsync();

			// Set counts in ViewBag
			ViewBag.TodayNotificationCount = todayNotifications.Count;
			ViewBag.UnreadNotificationCount = unreadNotificationCount;

			return Json(new
			{
				success = true,
				notifications = todayNotifications,
				unreadCount = unreadNotificationCount
			});
		}
		[HttpPost]
		public async Task<IActionResult> MarkAsRead(int notificationId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Json(new { success = false, message = "User not logged in" });
			}

			var notification = await _dbContext.Notifications
				.FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == user.Id);

			if (notification == null)
			{
				return Json(new { success = false, message = "Notification not found" });
			}

			notification.IsRead = true;
			_dbContext.Notifications.Update(notification);
			await _dbContext.SaveChangesAsync();

			return Json(new { success = true });
		}

		public async Task<IActionResult> Index()
		{
			int convertedLeadCount = GetConvertedLeadCount();

			var leadcount = await _dbContext.Leads.CountAsync();
			var dead = await _dbContext.FollowUps.CountAsync(f => f.Status == "Dead Lead");
			ViewBag.ConvertedLeadCount = convertedLeadCount;
			ViewBag.LeadCount = leadcount;
			ViewBag.DeadLead = dead;

			var hot = await _dbContext.FollowUps.CountAsync(f => f.Status == "Hot Lead");
			ViewBag.HotLead = hot;

			var meeting = await _dbContext.CalendarEvents.CountAsync();
			ViewBag.Meeting = meeting;

			var cold = await _dbContext.FollowUps.CountAsync(f => f.Status == "Cold Lead");
			ViewBag.ColdLead = cold;

			var future = await _dbContext.FollowUps.CountAsync(f => f.Status == "Future Lead");
			ViewBag.FutureLead = future;

			var currentMonth = DateTime.Now.Month;

			var leadCountThisMonth = await _dbContext.Leads
				.Where(lead => lead.CreatedDate.HasValue && lead.CreatedDate.Value.Month == currentMonth)
				.CountAsync();
			ViewBag.LeadCountThisMonth = leadCountThisMonth;

			var studentCount = await _dbContext.Students.CountAsync();
			ViewBag.StudentCount = studentCount;

			
			var allocatedLeadsCount = await _dbContext.Leads
				.Where(lead => lead.IsLeadAssign == false)
				.CountAsync();
			ViewBag.AllocatedLeadsCount = allocatedLeadsCount;

			return View();
		}

		private int GetConvertedLeadCount()
        {
            // Assuming "Converted Lead" is the status you are looking for
            const string convertedLeadStatus = "Converted Lead";

            // Query the database to get the count of converted leads
            int convertedLeadCount = _dbContext.FollowUps
                .Count(f => f.Status == convertedLeadStatus);

            return convertedLeadCount;
        }
		public async Task<JsonResult> GetUserActivity()
		{
			// Get today's date
			var today = DateTime.Today;

			// Fetch all follow-ups with a status of "Dead Lead" for today
			var deadLeads = await _dbContext.ActivityLogs
				.Include(f => f.Lead)
				.Include(f => f.User) // Include the User data
				.Where(f => f.ActivityLogDate.HasValue && f.ActivityLogDate.Value.Date == today)
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



		public async Task<JsonResult> GetCustomer()
        {
            try
            {
                // Get the current date and the first day of the current month
                var currentDate = DateTime.Now;
                var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

                // Query to fetch leads with accepted visa applications in the current month
                var acceptedVisaApplications = await _dbContext.VisaApplications
                    .Where(va => va.VisaStatus == "accept")
                    .Select(va => new
                    {
                        va.Lead.LeadId,
                        LeadName = va.Lead.FirstName + " " + va.Lead.LastName,
                        PhoneNumber =   va.Lead.PhoneNumber,
                        va.VisaStatus,
                        va.SubmissionDate
                    })
                    .ToListAsync();

                return Json(acceptedVisaApplications);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                return Json(new { error = $"Error retrieving customer data: {ex.Message}" });
            }
        }


    }
}
