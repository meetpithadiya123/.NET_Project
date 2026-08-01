using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Website.Models
{
    public class Admin
    {
        [Key]
        public int admin_id { get; set; }
        public String admin_name { get; set; }
        public String admin_email { get; set; }
        public String admin_password { get; set; }

    }
}
