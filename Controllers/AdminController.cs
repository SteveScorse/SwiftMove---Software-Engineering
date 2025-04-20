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

            //Create the view model and pass the data into the view:
            var viewModel = new AdminDashboardViewModel
            {
                Users = users,
                Roles = roles,
                Services = services,
                UserRoles = userRoles
            };



            return View(viewModel);
        }
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

        



    }
}
