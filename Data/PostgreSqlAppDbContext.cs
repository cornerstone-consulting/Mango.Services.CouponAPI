
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Data
{
    public class PostgreSqlAppDbContext : CouponDbContext
    {
        public PostgreSqlAppDbContext(
            DbContextOptions<PostgreSqlAppDbContext> options)
            : base(options)
        {
        }
    }
}
