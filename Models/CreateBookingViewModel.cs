using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using SwiftMove.Models;
using System.Collections.Generic;

namespace SwiftMove.ViewModels
{
    public class CreateBookingViewModel
    {
        public BookingsModel Booking { get; set; }
        public List<ServicesModel> Services { get; set; }
    }
}