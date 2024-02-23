using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebShopProjekt.Models;

namespace WebShopProjekt.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories {get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> orders { get; set; }

    }
}
