using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Website.Models
{
    public class Cart
    {
        [Key]
        public int cart_id { get; set; }
        public int product_id { get; set; }
        public int customer_id { get; set; }
        public int product_quantity { get; set; }
        public int cart_status { get; set; }

    }
}
