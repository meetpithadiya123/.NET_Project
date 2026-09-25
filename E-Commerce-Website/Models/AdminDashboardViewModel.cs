namespace E_Commerce_Website.Models
{
    public class AdminDashboardViewModel
    {
        public Admin? CurrentAdmin { get; set; }
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingCarts { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalFeedbacks { get; set; }
        public List<Cart> RecentOrders { get; set; } = new List<Cart>();
        public List<Customer> RecentCustomers { get; set; } = new List<Customer>();
        public List<Product> RecentProducts { get; set; } = new List<Product>();
        public List<Order> RecentPurchases { get; set; } = new List<Order>();
    }
}
