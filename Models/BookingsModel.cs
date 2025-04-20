using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftMove.Models
{
    //Class to structure bookings
    public class BookingsModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } // Foreign Key to Identity User

        [ForeignKey("CustomerId")]
        public CustomUserModel Customer { get; set; }

        [Required(ErrorMessage = "Please select a service")]
        public int ServiceId { get; set; } // FK to ServicesModel

        [ForeignKey("ServiceId")]
        public ServicesModel Service { get; set; }

        [Required(ErrorMessage = "Please select a booking date")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Required]
        [Range(1, 20, ErrorMessage = "You must assign at least 1 staff")]
        public int AssignedStaffCount { get; set; }
        public ICollection<StaffAssignmentModel> StaffAssignments { get; set; }

        // Optional: Notes for staff or customer
        [StringLength(500)]
        public string Notes { get; set; }

    }

    //Enumerable to Assign Booking Status
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        InProgress,
        Completed,
        Cancelled
    }
}
