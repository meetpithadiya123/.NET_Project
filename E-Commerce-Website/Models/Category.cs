using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Website.Models
{
    public class Category
    {
        [Key]
        public int category_id { get; set; }
        public int category_name { get; set; }
    }
}
