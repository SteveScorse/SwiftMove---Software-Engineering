using Microsoft.AspNetCore.Mvc.Rendering;
using SwiftMove.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwiftMove.ViewModels
{
    public class BookingFormViewModel
    {
        // Booking Data
        [Required(ErrorMessage = "Please select a service.")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Please choose a date.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        // Populated in controller
        public List<SelectListItem> AvailableServices { get; set; } = new List<SelectListItem>();
    }
}
