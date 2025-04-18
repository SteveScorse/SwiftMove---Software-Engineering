using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SwiftMove.Data;
using SwiftMove.Models;

namespace SwiftMove.Controllers
{
    public class ServicesController : Controller
    {
        //Injects Database into the controller
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {


            return View(services);
        }


        [Authorize(Roles = "Admin, Staff")]
        //Get Request for Create
        public IActionResult Create()
        {
            //Gets the view
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ServicesModel services, IFormFile imageFile)
        {
            //Checks if the uploaded image has gone to the backend
            if (imageFile != null && imageFile.Length > 0)
            {
                //Creates root path to save image in
                var imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images");

                if (!Directory.Exists(imagesDirectory))
                {
                    //If not made yet, creates it
                    Directory.CreateDirectory(imagesDirectory);
                }
                //Creating the filepath
                var filePath = Path.Combine(imagesDirectory, imageFile.FileName);


                try
                {
                    //Saves uploaded image to DB
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    //Sets file name for the the ServicesModel
                    services.Image = imageFile.FileName;

                    _context.Services.Add(services);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Image", "An error occured while saving the image. Please try again!");
                }
            }
            else
            {
                ModelState.AddModelError("Image", "An error occured while saving the image. Please try again!");
            }
            return View(services);
        }


    }
}
