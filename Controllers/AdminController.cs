using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SwiftMove.Data;
using SwiftMove.Models;
using System;

namespace SwiftMove.Controllers
{

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        //Inject the database into the controller
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<CustomUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            //Collects all nessacery data from DB
            var services = await _context.Services.ToListAsync();
            var users = await _userManager.Users.ToListAsync();
            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = new Dictionary<string, List<string>>();
            //Populating the dictionary with the user roles data.
            foreach (var user in users)
            {
                var userRole = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = userRole.ToList();
            }

            var bookings = await _context.Bookings
            .Include(b => b.Service)
            .Include(b => b.Customer)
            .Include(b => b.StaffAssignments)
            .ThenInclude(sa => sa.Staff)
            .ToListAsync();

            //Super Longggggg Booking Logic - Alot of validation and whatnot to keep data integrity throughout DB
            var allUsers = await _userManager.Users.ToListAsync();

            var allStaff = new List<CustomUserModel>();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Staff"))
                {
                    allStaff.Add(user);
                }
            }

            var availableStaffPerBooking = new Dictionary<int, List<CustomUserModel>>();

            //Stops Staff who have been Assigned, but then Unassigned from getting greyout on other bookings
            foreach (var booking in bookings)
            {
                var assignedStaffIdsElsewhere = _context.StaffAssignments
                    .Include(sa => sa.Booking)
                    .Where(sa => sa.Booking.BookingDate == booking.BookingDate && sa.BookingId != booking.Id)
                    .Select(sa => sa.StaffId)
                    .ToList();

                var staffAlreadyAssignedToThisBooking = booking.StaffAssignments.Select(sa => sa.StaffId).ToList();

                var available = allStaff
                    .Where(s => !assignedStaffIdsElsewhere.Contains(s.Id) || staffAlreadyAssignedToThisBooking.Contains(s.Id))
                    .ToList();

                availableStaffPerBooking[booking.Id] = available;
            }

            ViewBag.AvailableStaffPerBooking = availableStaffPerBooking;




            //Create the view model and pass the data into the view:
            var viewModel = new AdminDashboardViewModel
            {
                Users = users,
                Roles = roles,
                Services = services,
                UserRoles = userRoles,
                Bookings = bookings
            };



            return View(viewModel);
        }

        //Service Related ---------------------------------------------------------------------------------------------------------------
        //GET Request for Services/Edit
        [Authorize(Roles = "Admin, Staff")]
        public IActionResult Edit(int id)
        {
            var service = _context.Services.Find(id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        //POST Request for services/Edit
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ServicesModel services, IFormFile imageFile)
        {
            //Finds the Service via its matching ID in the DB
            var existingService = _context.Services.Find(services.Id);
            if (existingService == null)
            {
                return NotFound();
            }

            //Update Fields from tha form
            existingService.Title = services.Title;
            existingService.Price = services.Price;
            existingService.Description = services.Description;
            existingService.NumStaffRequired = services.NumStaffRequired;


            //Image Changes
            if (imageFile != null && imageFile.Length > 0)
            {
                var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(imagesDirectory))
                {
                    Directory.CreateDirectory(imagesDirectory);
                }

                var filePath = Path.Combine(imagesDirectory, imageFile.FileName);

                try
                {
                    //Save new image to server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    //Update Image only if a new one is uploaded
                    existingService.Image = imageFile.FileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An Error has occured whilst saving the uploaded image. Please try again!");
                    return View(services);
                }
            }
            else
            {
                // If no new image is uploaded, keep the existing image
                existingService.Image = existingService.Image;
            }


            //updates DB
            _context.Update(existingService);
            _context.SaveChanges();

            return RedirectToAction("Index");

        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpPost]
        public IActionResult DeleteService(int id)
        {
            var services = _context.Services.Find(id);

            if (services == null)
            {
                return NotFound();
            }

            _context.Services.Remove(services);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        //Service Related ---------------------------------------------------------------------------------------------------------------


        //Role Assignment Related -------------------------------------------------------------------------------------------------------
        //AssignRole POST Method
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> AssignRole(string userID, string roleName)
        {
            //Grabs user from their Id
            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                return BadRequest("Invalid User!");
            }
            //Add the Role
            await _userManager.AddToRoleAsync(user, roleName);

            return RedirectToAction("Index");
        }

        //Remove Role POST Method
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> RemoveRole(string userID, string roleName)
        {
            //Grabs user from their Id
            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                return BadRequest("Invalid User!");
            }
            //remove the Role
            await _userManager.RemoveFromRoleAsync(user, roleName);

            return RedirectToAction("Index");
        }

        //Role Creation POST Method
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> AddRole(string roleName)
        {
            //Standard Validation - Checks not blank and already exists
            if (!string.IsNullOrEmpty(roleName) && !await _roleManager.RoleExistsAsync(roleName))
            {
                //Create Role
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            else
            {
                return BadRequest("Role already exists or is empty");
            }
            //Page Refresh to Display new role.
            return RedirectToAction("Index");
        }

        //Role Deletion POST Method
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public async Task<IActionResult> DeleteRole(string roleID)
        {
            //Obtain the role
            var role = await _roleManager.FindByIdAsync(roleID);
            //Delete the role
            await _roleManager.DeleteAsync(role);
            return RedirectToAction("Index");
        }

        //Role Assignment Related -------------------------------------------------------------------------------------------------------

        //Bookings Related --------------------------------------------------------------------------------------------------------------




        //Im
        [HttpPost]
        public async Task<IActionResult> AssignOrUnassignStaff(int bookingId, string staffId, string action)
        {
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.StaffAssignments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return NotFound();

            if (action == "assign")
            {
                // Check if staff member is already assigned
                bool alreadyAssigned = booking.StaffAssignments.Any(sa => sa.StaffId == staffId);

                // Count current assignments vs service requirement
                int currentCount = booking.StaffAssignments.Count;
                int requiredCount = booking.Service.NumStaffRequired;

                //Validation Messgaes
                if (alreadyAssigned)
                {
                    TempData["Error"] = "This Staff Member has already been assigned to a Job.";
                    return RedirectToAction("Index");
                }

                if (currentCount >= requiredCount)
                {
                    TempData["Error"] = "There are too many staff assigned to this job already.";
                    return RedirectToAction("Index");
                }

                if (!alreadyAssigned && currentCount < requiredCount)
                {
                    _context.StaffAssignments.Add(new StaffAssignmentModel
                    {
                        BookingId = bookingId,
                        StaffId = staffId
                    });
                }

                //Validation Messages


            }
            else if (action == "unassign")
            {
                var assignment = await _context.StaffAssignments
                    .FirstOrDefaultAsync(sa => sa.BookingId == bookingId && sa.StaffId == staffId);

                if (assignment != null)
                {
                    _context.StaffAssignments.Remove(assignment);
                }
            }

            if (string.IsNullOrEmpty(staffId))
            {
                TempData["Error"] = "No staff selected for assignment.";
                return RedirectToAction("Index");
            }



            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        //Booking status update POST action
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public IActionResult UpdateBookingStatus(int bookingId, BookingStatus status)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = status;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        [HttpGet]
        [Authorize(Roles = "Admin, Staff")]
        public IActionResult EditBooking(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // If needed, populate dropdowns
            ViewBag.Services = new SelectList(_context.Services.ToList(), "Id", "Title");

            return View(booking);
        }



        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public IActionResult EditBooking(BookingsModel bookings)
        {
            var existingBooking = _context.Bookings
                .Include(b => b.StaffAssignments)
                .FirstOrDefault(b => b.Id == bookings.Id);

            if (existingBooking == null)
                return NotFound();

            // Update main fields
            existingBooking.ServiceId = bookings.ServiceId;
            existingBooking.BookingDate = bookings.BookingDate;
            existingBooking.Status = bookings.Status;
            existingBooking.Notes = bookings.Notes;

            // Get the new number of staff required
            var service = _context.Services.FirstOrDefault(s => s.Id == bookings.ServiceId);
            if (service != null)
            {
                int requiredStaff = service.NumStaffRequired;

                // Trim excess staff if over-assigned
                if (existingBooking.StaffAssignments.Count > requiredStaff)
                {
                    var toRemove = existingBooking.StaffAssignments
                                    .Skip(requiredStaff)
                                    .ToList(); // Get extras to remove

                    foreach (var assignment in toRemove)
                    {
                        _context.StaffAssignments.Remove(assignment);
                    }
                }
            }

            _context.Update(existingBooking);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        //Delete Functionality
        [HttpPost]
        [Authorize(Roles = "Admin, Staff")]
        public IActionResult DeleteBooking(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.StaffAssignments)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Remove any related staff assignments first
            if (booking.StaffAssignments != null && booking.StaffAssignments.Any())
            {
                _context.StaffAssignments.RemoveRange(booking.StaffAssignments);
            }

            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
