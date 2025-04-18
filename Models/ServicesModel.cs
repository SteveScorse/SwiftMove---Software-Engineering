using System.ComponentModel.DataAnnotations;

namespace SwiftMove.Models
{
    //Class to structure services
    public class ServicesModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Service Title is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Service Description is required")]
        [StringLength(500, ErrorMessage = "Service must be below 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Number of staff needed is required")]
        public int NumStaffRequired { get; set; }

        public string Image { get; set; }

    }
}
