using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SwiftMove.Data;
using SwiftMove.Models;
using Microsoft.EntityFrameworkCore;

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

        public async IActionResult Index()
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
    }
}
