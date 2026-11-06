
using Mango.Services.CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Data
{
    public abstract class CouponDbContext : DbContext
    {
        protected CouponDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Coupon> Coupons { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Shared seed data for SQL Server and PostgreSQL.
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
