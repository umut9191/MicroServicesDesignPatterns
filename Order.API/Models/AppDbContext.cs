using Microsoft.EntityFrameworkCore;

namespace Order.API.Models
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {

        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        //we are adding Address model here because it will not has a db table, it will be in Order table 
    }
}
