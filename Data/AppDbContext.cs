using Mango.Services.CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Coupon> Coupons { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //in start without this line everything works.
            //but later with identity it will not
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Coupon>().HasData(
                new Coupon
                {
                    Id = 1,
                    CouponCode = "10OFF",
                    DiscountAmount = 10,
                    MinAmount = 50
                },
                new Coupon
                {
                    Id = 2,
                    CouponCode = "20OFF",
                    DiscountAmount = 20,
                    MinAmount = 100
                }
            );
        }
    }
}
