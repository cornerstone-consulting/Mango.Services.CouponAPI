using System.ComponentModel.DataAnnotations;

namespace Mango.Services.CouponAPI.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        //This is not made unique. Not sure needed or not
        [Required]
        public string? CouponCode { get; set; }
        [Required]
        public double DiscountAmount { get; set; }
        public double MinAmount { get; set; }

        //TODO: Advanced versions needs these
        //public DateTime CreatedDate { get; set; }
        //public DateTime ExpiryDate { get; set; }
        //public bool IsActive { get; set; }
        //public string? CreatedBy { get; set; }
        //public string? UpdatedBy { get; set; }
        //public DateTime UpdatedDate { get; set; }
        //public string? CouponType { get; set; } // e.g., "Percentage", "Amount"
        //public string? Description { get; set; }
        //public string? ProductName { get; set; } // Optional, if the coupon is specific to a product
        //public string? ProductCategory { get; set; } // Optional, if the coupon is specific to a category
        //public string? ProductDescription { get; set; }

    }
}
