using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        //Booking GET Request
        [HttpGet]
        public IActionResult Index()
        {
            var services = _context.Services.ToList();

            var viewModel = new BookingFormViewModel
            {
                AvailableServices = services.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Title
                }).ToList()
            };

            return View(viewModel);
        }

        //Booking POST Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BookingFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // repopulate services dropdown
                model.AvailableServices = _context.Services.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Title
                }).ToList();
                return View(model);
            }

            var selectedService = await _context.Services.FindAsync(model.ServiceId);
            if (selectedService == null)
            {
                ModelState.AddModelError("ServiceId", "Selected service not found.");
                return View(model);
            }

            var booking = new BookingsModel
            {
                CustomerId = _userManager.GetUserId(User),
                ServiceId = model.ServiceId,
                BookingDate = model.BookingDate,
                Notes = model.Notes,
                AssignedStaffCount = selectedService.NumStaffRequired,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation");
        }

        //Booking Confirmation
        [HttpGet]
        public IActionResult Confirmation()
        {
            return View();
        }


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
