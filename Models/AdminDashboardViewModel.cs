using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SwiftMove.Models
{
    public class AdminDashboardViewModel
    {
        public List<CustomUserModel> Users { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public List<ServicesModel> Services { get; set; }
        public Dictionary<string, List<string>> UserRoles { get; set; }
        public List<BookingsModel> Bookings { get; set; }
    }
}
