using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwiftMove.Data;
using SwiftMove.Models;
using SwiftMove.ViewModels;

namespace SwiftMove.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        //Injects Database into the controller
        private readonly ApplicationDbContext _context;

        private readonly UserManager<CustomUserModel> _userManager;

        //Gets CustomUserModel DB information
        public BookingsController(ApplicationDbContext context, UserManager<CustomUserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        //public IActionResult Index()
        //{
        //    var booking = _context.Booking.ToList();
        //
        //    return View(booking);
        //}


        // GET: Bookings/Create
        [Authorize(Roles = "Admin, Staff, Customer")]
        public async Task<IActionResult> Create()
        {
            var services = await _context.Services.ToListAsync();
            var viewModel = new CreateBookingViewModel
            {
                Services = services,
                Booking = new BookingsModel
                {
                    BookingDate = DateTime.Today
                }
            };
            return View(viewModel);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Services = await _context.Services.ToListAsync();
                return View(viewModel);
            }

            var userId = _userManager.GetUserId(User);
            var booking = viewModel.Booking;
            booking.CustomerId = userId;
            booking.Status = BookingStatus.Pending;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
