using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Website.Models
{
    [Table("tbl_category")]
    public class Category
    {
        [Key]
        public int category_id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        public string category_name { get; set; } = string.Empty;

        // Make this nullable so EF model validation doesn't block form submissions
        public List<Product>? Product { get; set; }
    }
}