using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models.CalendarEvents;
using LegalAndGeneralConsultantCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalAndGeneralConsultantCRM.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class MeetingController : Controller
    {
        private readonly LegalAndGeneralConsultantCRMContext _context;
		private readonly UserManager<LegalAndGeneralConsultantCRMUser> _userManager;

		public MeetingController(LegalAndGeneralConsultantCRMContext context, UserManager<LegalAndGeneralConsultantCRMUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}
		public IActionResult Index()
        {
            return View();
        }
        public IActionResult Event()
        {
            ViewBag.Leads = _context.Leads.ToList(); // Fetch all leads from the database
            return View();
        }

		[HttpPost]
		public async Task<IActionResult> AddEvent([FromForm] CalendarEventViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);  
			var currentUser = user.Id;  
			CalendarEvent calendarEvent = new CalendarEvent
			{
				EventDate = model.EventDate,
				Description = model.Description,
				ThemeColor = model.ThemeColor,
				Name = model.Name,
				UserId = currentUser,
                Priorty=model.Priorty,
			};

			_context.CalendarEvents.Add(calendarEvent);
			await _context.SaveChangesAsync();  

			return Json(new { success = true });
		}


		public async Task<IActionResult> GetEvents()
		{
			var user = await _userManager.GetUserAsync(User);
			var currentUser = user.Id;  
			var events = await _context.CalendarEvents
				.Where(e => e.UserId == currentUser)  
				.Select(e => new
				{
					id = e.CalendarEventId,
					title = e.Name,
					start = e.EventDate,
					end = e.EventDate,
					description = e.Description,
					color = e.ThemeColor,
					Priorty = e.Priorty,
					 
				})
				.ToListAsync();

			return Json(events);
		}


		[HttpPost]
        public IActionResult UpdateEvent([FromForm] CalendarEventViewModel model)
        {
            
                var eventToUpdate = _context.CalendarEvents.Find(model.CalendarEventId);
            if (eventToUpdate != null)
            {
                eventToUpdate.Name = model.Name;
                eventToUpdate.Description = model.Description;
                eventToUpdate.EventDate = model.EventDate;
                eventToUpdate.ThemeColor = model.ThemeColor;

                _context.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Event not found." });
            }
        }

        [HttpPost]
        public IActionResult DeleteEvent(int id)
        {
            var eventToDelete = _context.CalendarEvents.Find(id);
            if (eventToDelete != null)
            {
                _context.CalendarEvents.Remove(eventToDelete);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Event not found." });
            }
        }


    }

}
