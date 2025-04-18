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

        public DbSet<ServicesModel> Services { get; set; }
    }
}
