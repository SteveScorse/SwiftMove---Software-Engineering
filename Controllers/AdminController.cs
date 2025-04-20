using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

            var staff = await _userManager.GetUsersInRoleAsync("Staff");

            // Dictionary to store available staff per booking ID
            var availableStaffPerBooking = new Dictionary<int, List<CustomUserModel>>();

            foreach (var booking in bookings)
            {
                var unavailableStaffIds = _context.StaffAssignments
                    .Where(sa => sa.Booking.BookingDate == booking.BookingDate)
                    .Select(sa => sa.StaffId)
                    .ToHashSet();

                var available = staff.Where(s => !unavailableStaffIds.Contains(s.Id)).ToList();

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
            //Finds the Property via its matching ID in the DB
            var existingService= _context.Services.Find(services.Id);
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



            //updates DB
            _context.Update(existingService);
            _context.SaveChanges();

            return RedirectToAction("Index");

        }
        //Service Related ---------------------------------------------------------------------------------------------------------------


        //Role Assignment Related -------------------------------------------------------------------------------------------------------
        //AssignRole POST Method
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

        [HttpPost]
        public async Task<IActionResult> AssignStaff(int bookingId, string staffId)
        {
            // Check if already assigned
            var alreadyAssigned = await _context.StaffAssignments
                .AnyAsync(sa => sa.BookingId == bookingId && sa.StaffId == staffId);

            if (alreadyAssigned)
            {
                // Optional: return a message or alert to indicate already assigned
                return RedirectToAction("Index");
            }

            // Get the booking with the associated service info
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.StaffAssignments)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound();
            }

            // Check how many staff are currently assigned
            int currentlyAssignedCount = booking.StaffAssignments.Count;
            int maxStaffRequired = booking.Service.NumStaffRequired;

            if (currentlyAssignedCount >= maxStaffRequired)
            {
                // Optional: TempData message to show in the view
                TempData["Error"] = $"This booking already has the required {maxStaffRequired} staff assigned.";
                return RedirectToAction("Index");
            }

            // Assign new staff
            var assignment = new StaffAssignmentModel
            {
                BookingId = bookingId,
                StaffId = staffId
            };
            _context.StaffAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        //Booking update POST action
        [HttpPost]
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






    }
}
