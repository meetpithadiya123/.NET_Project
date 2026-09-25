using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Website.Models
{
    [Table("tbl_order")]
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderStatus { get; set; } = "Placed"; // Placed, Completed, Processing, Cancelled

        [Required]
        [StringLength(255)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(25)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ShippingEmail { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PaymentMode { get; set; } = string.Empty; // Dummy Card, UPI, NetBanking, COD

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Success"; // Success, Failed

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
