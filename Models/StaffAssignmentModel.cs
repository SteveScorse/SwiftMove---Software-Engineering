using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftMove.Models
{
    public class StaffAssignmentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StaffId { get; set; }

        [ForeignKey("StaffId")]
        public CustomUserModel Staff { get; set; }

        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public BookingsModel Booking { get; set; }
    }
}
