using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;


namespace SwiftMove.Models
{
    public class CustomUserModel : IdentityUser //Inheriting the class identity user
    {
        [Required]
        [MaxLength(100, ErrorMessage = "First Name has a maximum of 100 Characters!")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Last Name has a maximum of 100 Characters!")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Last Name has a maximum of 100 Characters!")]
        public string Address { get; set; }
    }
}
