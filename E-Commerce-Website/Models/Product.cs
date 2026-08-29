using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Website.Models
{
    public class Product
    {
        [Key]
        public int product_id { get; set; }

        public string product_name { get; set; } = string.Empty;

        public string product_price { get; set; } = string.Empty;

        public string product_description { get; set; } = string.Empty;

        public string? product_image { get; set; }

        public int cat_id { get; set; }

        [ForeignKey("cat_id")]
        public Category? Category { get; set; }
    }
}