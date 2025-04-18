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

        // GET: Admin/Edit/1
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();


            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServicesModel service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("✅ Saved to DB");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Save failed: " + ex.Message);
                }
            }

            return View(service);
        }




    }
}
