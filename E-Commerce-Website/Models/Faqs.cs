using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Website.Models
{
    public class Faqs
    {
        [Key]
        public int faqs_id { get; set; }
        public String faqs_question { get; set; }
        public String faqs_answer { get; set; }
    }
}
