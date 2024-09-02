using LegalAndGeneralConsultantCRM.Areas.Identity.Data;
using LegalAndGeneralConsultantCRM.Models.Universiies;
using LegalAndGeneralConsultantCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LegalAndGeneralConsultantCRM.Areas.Admin.Controllers
{

	[Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class UniversityController : Controller
    {

        private readonly LegalAndGeneralConsultantCRMContext _context;
       
       
        public UniversityController(LegalAndGeneralConsultantCRMContext context)
        {
            _context = context;
            
        }


        
        public async Task<IActionResult> Index()
        {
            var followup = await _context.Universities.ToListAsync();
            return View(followup);
        }


        public async Task<IActionResult> AddUniversity()
        {
            ViewBag.AllOfferPrograms = _context.Programs.ToList();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(University university, List<string> OfferPrograms)
        {
            try
            {
                university.OfferProgram = string.Join(",", OfferPrograms);

                _context.Universities.Add(university);
                await _context.SaveChangesAsync(); // Use async method to save changes

                TempData["SuccessMessage"] = "University created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the university. Please try again.";
                return RedirectToAction("Index");
            }
        }



        public async Task<IActionResult> EditUniversity(int id)
        {

            var uniexit = await _context.Universities.FindAsync(id);
            ViewBag.AllOfferPrograms = _context.Programs.ToList();
            return View(uniexit);
        }


        [HttpPost]
         
        public async Task<IActionResult> EditUniversity(  University university, List<string> OfferPrograms)
        {
             
 
                try
                {
                    // Retrieve the existing entity from the database
                    var existingUniversity = await _context.Universities.FindAsync(university.UniversityId);

                    if (existingUniversity == null)
                    {
                        return NotFound();
                    }

                    existingUniversity.Name = university.Name;
                    existingUniversity.Location = university.Location;
                    existingUniversity.Country = university.Country;
                    existingUniversity.StreetAddress = university.StreetAddress;
                    existingUniversity.City = university.City;
                    existingUniversity.State = university.State;
                    existingUniversity.ZipCode = university.ZipCode;
                    existingUniversity.Description = university.Description;
                    existingUniversity.Founded = university.Founded;
                    existingUniversity.Website = university.Website;
                    existingUniversity.ContactEmail = university.ContactEmail;
                    existingUniversity.ContactPhone = university.ContactPhone;
                    existingUniversity.Type = university.Type;
                    existingUniversity.Chancellor = university.Chancellor;
                    existingUniversity.Ranking = university.Ranking;
                    existingUniversity.ResearchAreas = university.ResearchAreas;
                    existingUniversity.OffersInternationalPrograms = university.OffersInternationalPrograms;
                    // Update the OfferProgram property
                    existingUniversity.OfferProgram = string.Join(",", OfferPrograms);

                    _context.Update(existingUniversity);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "University data edited successfully.";

                    return RedirectToAction(nameof(Index)); // Replace 'Index' with the action you want to redirect to after editing
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    
                }
          
            return View(university);
        }



        public async Task<JsonResult> GetUniversity()
        {
            var universities = await _context.Universities.ToListAsync();

            return Json(new { data = universities });
        }
      
      
        [HttpPost]
        public IActionResult EditUni([FromBody] University university)
        {
            try
            {
                // Find the existing university by UniversityId
                var existingUniversity = _context.Universities.Find(university.UniversityId);

                if (existingUniversity != null)
                {
                    // Update properties of existing university with values from the updated university
                    existingUniversity.Name = university.Name;
                    existingUniversity.Country = university.Country;
                    existingUniversity.StreetAddress = university.StreetAddress;
                    existingUniversity.Founded = university.Founded;
                    existingUniversity.Website = university.Website;
                    existingUniversity.ContactEmail = university.ContactEmail;
                    existingUniversity.ContactPhone = university.ContactPhone;
                    existingUniversity.Chancellor = university.Chancellor;
                    existingUniversity.Ranking = university.Ranking;

                    // Save changes to database
                    _context.SaveChanges();

                    return Json(new { success = true, message = "University updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "University not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating university: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUniversity([FromBody] DeleteUniversityRequest request)
        {
            try
            {
                var existingUniversity = await _context.Universities.FindAsync(request.UniversityId);
                if (existingUniversity != null)
                {
                    _context.Universities.Remove(existingUniversity);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "University deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "University not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting university: " + ex.Message });
            }
        }

        public class DeleteUniversityRequest
        {
            public int UniversityId { get; set; }
        }
        //Program
        public async Task<IActionResult> AddProgram()
        {
            return View();
        }

        //[HttpPost]
        //public IActionResult AddProgram(OfferProgram program)
        //{ 
        //    _context.Programs.Add(program);
        //    _context.SaveChanges();

        //    return RedirectToAction("Index");
        //}
        public JsonResult GetProgramlData()
        {
            var program = _context.Programs.ToList();

            return Json(new { data = program });
        }
        public async Task<IActionResult> CreatProgram()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreateProgram(List<string> programNames)
        {
            // Assuming you have a database context or service to interact with the database
            foreach (var programName in programNames)
            {
                var newProgram = new OfferProgram { Name = programName };
                _context.Programs.Add(newProgram);
               
            }
            _context.SaveChanges();
            return RedirectToAction("AddProgram"); // Redirect to the desired action
        }


        public async Task<IActionResult> Edit(int id)
        {
            var edt = await _context.Programs.FindAsync(id);
            return View(edt);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, OfferProgram pro)
        {
            var Prog =  await _context.Programs.FindAsync(id);

            if (Prog == null)
            {
                return NotFound();
            }
            Prog.OfferProgramId = pro.OfferProgramId;
             Prog.Name = pro.Name;
              _context.Update(Prog);
           await _context.SaveChangesAsync();

            return RedirectToAction("AddProgram");
         
            
        }


        public async Task<IActionResult> Courses()
        {
            
            return View();
        }
        public async Task<JsonResult> GetCourses()
        {
            var courses = await _context.Courses.ToListAsync();

            return Json(new { data = courses });
        }
        public async Task<IActionResult> AddCourses()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCourses(Course course)
        {
             
                try
                {
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Course added successfully!";
                    return RedirectToAction("Courses");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the course. Please try again.";
                }
            
            return View(course);
        }
        [HttpPost]
        public async Task<IActionResult> EditCourse([FromBody] Course course)
        {
            try
            {
                // Update course in the database
                var existingCourse = await _context.Courses.FindAsync(course.CourseId);
                if (existingCourse != null)
                {
                    existingCourse.Name = course.Name;
                    existingCourse.OtherCosts = course.OtherCosts;
                    existingCourse.DurationInYears = course.DurationInYears;
                    existingCourse.Decription = course.Decription;

                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Course updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Course not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating course: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult EditCourses(CourseViewModel courseViewModel)
        {
            try
            {
                var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseViewModel.CourseId);

                if (course == null)
                {
                    return Json(new { success = false, message = "Course not found." });
                }

                course.Name = courseViewModel.Name;
                course.DurationInYears = courseViewModel.DurationInYears;

                var universityCourse = _context.UniversityCourses
                    .FirstOrDefault(uc => uc.CourseId == courseViewModel.CourseId && uc.UniversityId == courseViewModel.UniversityId);

                if (universityCourse != null)
                {
                    universityCourse.TuitionFee = courseViewModel.TuitionFee;
                }
                else
                {
                    return Json(new { success = false, message = "UniversityCourse not found." });
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Course updated successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError(ex, "Error updating course with ID {CourseId} and University ID {UniversityId}", courseViewModel.CourseId, courseViewModel.UniversityId);

                return Json(new { success = false, message = "An error occurred while updating the course." });
            }
        }
        [HttpPost]
        public IActionResult DelCourse(int courseId, int universityId)
        {
            try
            {
                var universityCourse = _context.UniversityCourses
                    .FirstOrDefault(uc => uc.CourseId == courseId && uc.UniversityId == universityId);

                if (universityCourse == null)
                {
                    return Json(new { success = false, message = "Course not found for this university." });
                }

                _context.UniversityCourses.Remove(universityCourse);
                _context.SaveChanges();

                return Json(new { success = true, message = "Course deleted successfully." });
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError(ex, "Error deleting course with ID {CourseId} and University ID {UniversityId}", courseId, universityId);

                return Json(new { success = false, message = "An error occurred while deleting the course." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCourse([FromBody] DeleteCourseRequest request)
        {
            try
            {
                var course = await _context.Courses.FindAsync(request.CourseId);
                if (course != null)
                {
                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Course deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Course not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting course: " + ex.Message });
            }
        }
        public class DeleteCourseRequest
        {
            public int CourseId { get; set; }
        }
        public async Task<IActionResult> UniversityCourses()
        {
            ViewBag.Uni = await _context.Universities.ToListAsync();
            ViewBag.Courses = await _context.Courses.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UniversityCourses(int universityId, List<UniversityCourse> universityCourses)
        {
            try
            {
                // Ensure the selected university exists
                var universityExists = await _context.Universities.AnyAsync(u => u.UniversityId == universityId);

                if (universityExists)
                {
                    foreach (var universityCourse in universityCourses)
                    {
                        // Assign the UniversityId to each UniversityCourse
                        universityCourse.UniversityId = universityId;

                        // Check if the same UniversityCourse already exists in the database
                        var existingUniversityCourse = await _context.UniversityCourses.FirstOrDefaultAsync(uc =>
                            uc.UniversityId == universityCourse.UniversityId &&
                            uc.CourseId == universityCourse.CourseId);

                        if (existingUniversityCourse == null)
                        {
                            // Ensure the selected course exists
                            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == universityCourse.CourseId);

                            if (courseExists)
                            {
                                // Add the university course to the database
                                _context.Add(universityCourse);
                            }
                            else
                            {
                                // Course not found, handle accordingly (e.g., return an error message)
                                ModelState.AddModelError("", "Invalid course selection.");
                                return View();
                            }
                        }
                        else
                        {
                            // Skip adding the duplicate entry
                            continue;
                        }
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();

                    // Optionally, set a success message in TempData to be displayed on the next request
                    TempData["SuccessMessage"] = "University courses added successfully.";

                    // Redirect to a different action or view
                    return RedirectToAction("Uni", "University");
                }
                else
                {
                    // University not found, handle accordingly (e.g., return an error message)
                    ModelState.AddModelError("", "Invalid university selection.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it according to your application's requirements
                ModelState.AddModelError("", "An error occurred while saving university courses.");
                return View();
            }
        }


        public async Task<IActionResult> Incentives()
        {
             return View();
        }

        public async Task<JsonResult> GetIncentive()
        {
 
            var incentives = await _context.Incentives.Include(i => i.Course).ToListAsync();

            var result = incentives.Select(i => new
            {
                IncentiveId = i.IncentiveId,
                Description = i.Description,
                CourseName = i.Course != null ? i.Course.Name : null
            });

            return Json(new { data = result });
        }
        public async Task<IActionResult> AddIncentive()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddIncentive(Incentive  incentive)
        {

            try
            {
                _context.Incentives.Add(incentive);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Incentives added successfully!";
                return RedirectToAction("Incentives");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the course. Please try again.";
            }

            return View(incentive);
        }



        public async Task<IActionResult> Scholarship()
        {
            return View();
        }

        public async Task<JsonResult> GetScholarshipsWithDetails()
        {
            var scholarships = await _context.Scholarships
                .Include(s => s.Course)
                .Include(s => s.University)
                .Select(s => new
                {
                    UniversityName = s.University.Name,
                    CourseName = s.Course.Name,
                    ScholarshipId = s.ScholarshipId,
                    ScholarshipName = s.Name,
                    EligibilityCriteria = s.EligibilityCriteria,
                    ApplicationDeadline = s.ApplicationDeadline
                })
                .ToListAsync();

            return Json(new { data = scholarships });
        }
        public async Task<IActionResult> AddScholarship()
        {
            ViewBag.Courses = await _context.Courses.ToListAsync();
            ViewBag.Uni = await _context.Universities.ToListAsync();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddScholarship(Scholarship  scholarship)
        {

            try
            {
                _context.Scholarships.Add(scholarship);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Scholarship added successfully!";
                return RedirectToAction("Scholarship");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while adding the course. Please try again.";
            }

            return View(scholarship);
        }

        public async Task<IActionResult> Uni()
        {
             
            return View();
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
                    Location = u.Country, // Assuming you want to include the country as the location
                    Courses = u.UniversityCourses.Select(uc => new
                    {
                        UniversityCourseId = uc.UniversityCourseId,  
                        CourseId = uc.CourseId,
                        CourseName = uc.Course.Name,
                        TuitionFee = uc.TuitionFee
                        // Include other course properties as needed
                    }).ToList() // Ensure you call ToList() to avoid deferred execution issues
                })
                .ToListAsync();

            return Json(new { data = universities });
        }



        [HttpPost]
        public async Task<IActionResult> DeleteUniversityCourse(int universityCourseId)
        {
            try
            {
                var universityCourse = await _context.UniversityCourses
                               .FirstOrDefaultAsync(uc => uc.UniversityCourseId == universityCourseId);
                if (universityCourse == null)
                {
                    return NotFound(new { message = "University course not found" });
                }

                _context.UniversityCourses.Remove(universityCourse);
                await _context.SaveChangesAsync();

                return Ok(new { message = "University course deleted successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Exception occurred: {ex}");

                // Return a 500 Internal Server Error response with an error message
                return StatusCode(500, new { message = "Error deleting university course", error = ex.Message });
            }
        }




        public async Task<IActionResult> Unidetail(int universityId)
        {
            var university = await _context.Universities
                .Include(u => u.UniversityCourses)
                    .ThenInclude(uc => uc.Course)
                .Where(u => u.UniversityId == universityId)
                .FirstOrDefaultAsync();

            if (university == null)
            {
                return NotFound(new { message = "University not found" });
            }

            var viewModel = new UniversityDetailViewModel
            {
                University = new UniversityViewModel
                {
                    UniversityId = university.UniversityId,
                    Name = university.Name,
                    // Assign other properties
                },
                UniversityCourses = university.UniversityCourses.Select(uc => new CourseViewModel
                {
                    CourseId = (int)(uc.CourseId ?? 0), // Assuming 0 is the default value
                    Name = uc.Course.Name,
                    TuitionFee = (decimal)(uc.TuitionFee ?? 0.0m), // Assuming 0.0m is the default value
                    DurationInYears = uc.Course.DurationInYears, // Assign DurationInYears property
                                                                 // Assign other properties
                }).ToList()
            };

            return View(viewModel); // Pass viewModel to the view
        }

    }

}
