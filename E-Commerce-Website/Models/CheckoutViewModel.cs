namespace E_Commerce_Website.Models
{
    public class CheckoutViewModel
    {
        public Customer Customer { get; set; } = new Customer();
        public List<Cart> CartItems { get; set; } = new List<Cart>();
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; } = 0;
        public decimal GrandTotal => Subtotal + ShippingFee;
    }
}
