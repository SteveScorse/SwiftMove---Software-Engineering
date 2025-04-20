using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwiftMove.Models;

namespace SwiftMove.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUserModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        // Each of these represents a table in the DB
        public DbSet<ServicesModel> Services { get; set; }
        public DbSet<BookingsModel> Bookings { get; set; }
    }
}
