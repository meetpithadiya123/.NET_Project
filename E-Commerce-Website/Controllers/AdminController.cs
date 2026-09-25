using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Website.Controllers
{
    public class AdminController : Controller
    {

        private readonly Mycontext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(Mycontext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // ADMIN DASHBOARD
        // =========================

        [HttpGet]
        public IActionResult Index()
        {
            string? adminSession = HttpContext.Session.GetString("admin_session");

            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            int.TryParse(adminSession, out int adminId);
            var currentAdmin = _context.tbl_admin.Find(adminId);

            var products = _context.tbl_product.Include(p => p.Category).ToList();
            var categories = _context.tbl_category.ToList();
            var customers = _context.tbl_customer.ToList();
            var carts = _context.tbl_cart.Include(c => c.products).Include(c => c.customers).ToList();
            var feedbacks = _context.tbl_feedback.ToList();

            var orders = _context.tbl_order.Include(o => o.Customer).Include(o => o.OrderItems).ToList();

            decimal totalRevenue = orders.Any() 
                ? orders.Sum(o => o.TotalAmount)
                : carts.Where(c => c.cart_status == 1).Sum(item => (item.products != null && decimal.TryParse(item.products.product_price, out decimal price)) ? price * item.product_quantity : 0);

            var viewModel = new AdminDashboardViewModel
            {
                CurrentAdmin = currentAdmin,
                TotalProducts = products.Count,
                TotalCategories = categories.Count,
                TotalCustomers = customers.Count,
                TotalOrders = orders.Count > 0 ? orders.Count : carts.Count,
                CompletedOrders = orders.Count > 0 ? orders.Count(o => o.OrderStatus == "Completed" || o.PaymentStatus == "Success") : carts.Count(c => c.cart_status == 1),
                PendingCarts = carts.Count(c => c.cart_status == 0),
                TotalRevenue = totalRevenue,
                TotalFeedbacks = feedbacks.Count,
                RecentOrders = carts.OrderByDescending(c => c.cart_id).Take(5).ToList(),
                RecentCustomers = customers.OrderByDescending(c => c.customer_id).Take(5).ToList(),
                RecentProducts = products.OrderByDescending(p => p.product_id).Take(5).ToList(),
                RecentPurchases = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList()
            };

            return View(viewModel);
        }


        // =========================
        // LOGIN - GET
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // =========================
        // LOGIN - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            var row = _context.tbl_admin
                .FirstOrDefault(a => a.admin_email == email);

            if (row != null && row.admin_password == password)
            {
                HttpContext.Session.SetString(
                    "admin_session",
                    row.admin_id.ToString()
                );

                return RedirectToAction("Index");
            }

            ViewBag.message = "Incorrect username or password";

            return View();
        }


        // =========================
        // LOGOUT
        // =========================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin_session");

            return RedirectToAction("Login");
        }


        // =========================
        // PROFILE - GET
        // =========================

        [HttpGet]
        public IActionResult Profile()
        {
            string? adminSession =
                HttpContext.Session.GetString("admin_session");

            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(adminSession, out int adminId))
            {
                HttpContext.Session.Remove("admin_session");

                return RedirectToAction("Login");
            }

            var row = _context.tbl_admin.Find(adminId);

            if (row == null)
            {
                HttpContext.Session.Remove("admin_session");

                return RedirectToAction("Login");
            }

            return View(row);
        }




        [HttpPost]
        public IActionResult Profile(Admin admin)
        {
            if (!ModelState.IsValid)
            {
                return View(admin);
            }

            string? adminSession =
                HttpContext.Session.GetString("admin_session");

            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(adminSession, out int sessionAdminId))
            {
                HttpContext.Session.Remove("admin_session");
                return RedirectToAction("Login");
            }

            if (sessionAdminId != admin.admin_id)
            {
                return Unauthorized();
            }

            var existingAdmin = _context.tbl_admin.Find(admin.admin_id);

            if (existingAdmin == null)
            {
                return NotFound();
            }

            existingAdmin.admin_name = admin.admin_name;
            existingAdmin.admin_email = admin.admin_email;
            existingAdmin.admin_password = admin.admin_password;

            _context.SaveChanges();

            // Success message
            TempData["SuccessMessage"] = "Profile updated successfully!";

            return RedirectToAction("Profile");
        }

        public IActionResult fetchCustomer()
        {
            return View(_context.tbl_customer.ToList());
        }

        public IActionResult customerDetails(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
        }



        // GET: Admin/updateCustomer/5
        [HttpGet]
        public IActionResult updateCustomer(int id)
        {
            var customer = _context.tbl_customer.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Admin/updateCustomer (Handles form submission)
        [HttpPost]
        public IActionResult updateCustomer(Customer customer)
        {
            _context.tbl_customer.Update(customer);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer updated successfully!";
            return RedirectToAction("fetchCustomer");
        }



        public IActionResult deleteCustomer(int id)
        {
            var customer = _context.tbl_customer.Find(id);
            _context.tbl_customer.Remove(customer);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Customer deleted successfully!";
            return RedirectToAction("fetchCustomer");
        }



        // =========================
        // CATEGORY CRUD
        // =========================

        [HttpGet]
        public IActionResult fetchCategory()
        {
            var categories = _context.tbl_category.ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult addCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addCategory(Category cat)
        {
            if (!string.IsNullOrWhiteSpace(cat.category_name))
            {
                _context.tbl_category.Add(new Category
                {
                    category_name = cat.category_name
                });
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Category added successfully!";
                return RedirectToAction("fetchCategory");
            }

            ViewBag.Error = "Category Name cannot be empty";
            return View(cat);
        }

        [HttpGet]
        public IActionResult updateCategory(int id)
        {
            var category = _context.tbl_category.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult updateCategory(Category cat)
        {
            var existingCategory = _context.tbl_category.Find(cat.category_id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(cat.category_name))
            {
                existingCategory.category_name = cat.category_name;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction("fetchCategory");
            }

            ViewBag.Error = "Category Name cannot be empty";
            return View(cat);
        }

        [HttpGet]
        public IActionResult deleteCategory(int id)
        {
            var category = _context.tbl_category.Find(id);
            if (category != null)
            {
                try
                {
                    _context.tbl_category.Remove(category);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Category deleted successfully!";
                }
                catch (Exception)
                {
                    // Triggered if products in tbl_product reference this category_id (Foreign Key constraint)
                    TempData["ErrorMessage"] = "Cannot delete: products are linked to this category!";
                }
            }

            return RedirectToAction("fetchCategory");
        }




        // =========================
        // FETCH PRODUCT
        // =========================
        [HttpGet]
        public IActionResult fetchProduct()
        {
            // Includes Category data to display category name instead of raw cat_id
            var products = _context.tbl_product.Include(p => p.Category).ToList();
            return View(products);
        }

        // =========================
        // ADD PRODUCT - GET
        // =========================
        [HttpGet]
        public IActionResult addProduct()
        {
            // Pass categories to the view for dropdown list
            ViewBag.Categories = _context.tbl_category.ToList();
            return View();
        }

        // =========================
        // ADD PRODUCT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult addProduct(Product prod, IFormFile? product_image)
        {
            if (product_image != null && product_image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
                var ext = Path.GetExtension(product_image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext) || product_image.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Invalid image file format or file size exceeds 10MB limit.");
                    ViewBag.Categories = _context.tbl_category.AsNoTracking().ToList();
                    return View(prod);
                }

                // Generate a unique file name using GUID to avoid name collisions
                string uniqueFileName = Guid.NewGuid().ToString() + ext;

                // Define the destination path in wwwroot/product_images
                string folderPath = Path.Combine(_env.WebRootPath, "product_images");

                // Ensure directory exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, uniqueFileName);

                // Save image file to wwwroot/product_images
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    product_image.CopyTo(fileStream);
                }

                // Store the filename into the database model
                prod.product_image = uniqueFileName;
            }

            _context.tbl_product.Add(prod);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("fetchProduct");
        }




        // =========================
        // PRODUCT - DETAILS
        // =========================
        [HttpGet]
        public IActionResult productDetails(int id)
        {
            var product = _context.tbl_product
                .Include(p => p.Category)
                .FirstOrDefault(p => p.product_id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // =========================
        // PRODUCT - UPDATE (GET)
        // =========================
        [HttpGet]
        public IActionResult updateProduct(int id)
        {
            var product = _context.tbl_product.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.tbl_category.ToList();
            return View(product);
        }

        // =========================
        // PRODUCT - UPDATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult updateProduct(Product prod, IFormFile? product_image)
        {
            var existingProduct = _context.tbl_product.Find(prod.product_id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Handle new image upload if provided
            if (product_image != null && product_image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
                var ext = Path.GetExtension(product_image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext) || product_image.Length > 10 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "Invalid image file format or file size exceeds 10MB limit.");
                    ViewBag.Categories = _context.tbl_category.AsNoTracking().ToList();
                    return View(prod);
                }

                string uploadFolder = Path.Combine(_env.WebRootPath, "product_images");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Delete old image if it exists
                if (!string.IsNullOrEmpty(existingProduct.product_image))
                {
                    string oldPath = Path.Combine(uploadFolder, existingProduct.product_image);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Save new image
                string newFileName = Guid.NewGuid().ToString() + ext;
                string newPath = Path.Combine(uploadFolder, newFileName);

                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    product_image.CopyTo(stream);
                }

                existingProduct.product_image = newFileName;
            }

            // Update product fields
            existingProduct.product_name = prod.product_name;
            existingProduct.product_price = prod.product_price;
            existingProduct.product_description = prod.product_description;
            existingProduct.cat_id = prod.cat_id;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction("fetchProduct");
        }

        // =========================
        // PRODUCT - DELETE
        // =========================
        [HttpGet]
        public IActionResult deleteProduct(int id)
        {
            var product = _context.tbl_product.Find(id);
            if (product != null)
            {
                // Delete image file from wwwroot/product_images if exists
                if (!string.IsNullOrEmpty(product.product_image))
                {
                    string imagePath = Path.Combine(_env.WebRootPath, "product_images", product.product_image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.tbl_product.Remove(product);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Product deleted successfully!";
            }

            return RedirectToAction("fetchProduct");
        }


        // 1. Fetch Cart View
        public IActionResult fetchCart()
        {
            var cartList = _context.tbl_cart
                                   .Include(c => c.products)
                                   .Include(c => c.customers)
                                   .ToList();
            return View(cartList);
        }

        public IActionResult updateCart(int id)
        {
            var cart = _context.tbl_cart.Find(id);
            return View(cart);
        }

        // 2. Update Cart Status (matches your screenshot logic)
        [HttpPost]
        public IActionResult updateCart(int cart_id, int cart_status)
        {
            var cart = _context.tbl_cart.Find(cart_id);
            if (cart != null)
            {
                cart.cart_status = cart_status;
                _context.tbl_cart.Update(cart);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cart status updated successfully!";
            }
            return RedirectToAction("fetchCart");
        }

        // 3. Delete Cart Item
        public IActionResult deletecart(int id)
        {
            var cartItem = _context.tbl_cart.Find(id);
            if (cartItem != null)
            {
                _context.tbl_cart.Remove(cartItem);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cart item deleted successfully!";
            }
            return RedirectToAction("fetchCart");
        }

        // =========================
        // FEEDBACK MANAGEMENT
        // =========================
        [HttpGet]
        public IActionResult fetchfeedback()
        {
            string? adminSession = HttpContext.Session.GetString("admin_session");
            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            var feedbacks = _context.tbl_feedback.ToList();
            return View(feedbacks);
        }

        [HttpGet]
        public IActionResult deletePermissionFeedback(int id)
        {
            string? adminSession = HttpContext.Session.GetString("admin_session");
            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            var feedback = _context.tbl_feedback.Find(id);
            if (feedback != null)
            {
                _context.tbl_feedback.Remove(feedback);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Feedback deleted successfully!";
            }
            return RedirectToAction("fetchfeedback");
        }

        // =========================
        // PAYMENTS & ORDER PURCHASES MANAGEMENT
        // =========================

        [HttpGet]
        public async Task<IActionResult> Payments(string? search, string? paymentMode, string? status)
        {
            string? adminSession = HttpContext.Session.GetString("admin_session");
            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            var query = _context.tbl_order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Search by order ID, transaction ID, customer name, email, phone, or city
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim();
                query = query.Where(o =>
                    o.TransactionId.Contains(s) ||
                    o.OrderId.ToString().Contains(s) ||
                    (o.Customer != null && (o.Customer.customer_name.Contains(s) || o.Customer.customer_email.Contains(s))) ||
                    o.City.Contains(s) ||
                    o.Phone.Contains(s));
            }

            // Filter by Payment Mode
            if (!string.IsNullOrWhiteSpace(paymentMode) && paymentMode != "All")
            {
                query = query.Where(o => o.PaymentMode == paymentMode);
            }

            // Filter by Order Status
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.Search = search;
            ViewBag.PaymentMode = paymentMode;
            ViewBag.Status = status;

            // KPI summaries for all paid orders
            var allOrders = await _context.tbl_order.AsNoTracking().ToListAsync();
            ViewBag.TotalRevenue = allOrders.Sum(o => o.TotalAmount);
            ViewBag.TotalTransactions = allOrders.Count;
            ViewBag.SuccessfulPayments = allOrders.Count(o => o.PaymentStatus == "Success");
            ViewBag.AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.TotalAmount) : 0;

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string orderStatus)
        {
            string? adminSession = HttpContext.Session.GetString("admin_session");
            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            var order = await _context.tbl_order.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                _context.tbl_order.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Order #{orderId} status successfully updated to '{orderStatus}'!";
            }

            return RedirectToAction("Payments");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentReceipt(int id)
        {
            string? adminSession = HttpContext.Session.GetString("admin_session");
            if (string.IsNullOrEmpty(adminSession))
            {
                return RedirectToAction("Login");
            }

            var order = await _context.tbl_order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return RedirectToAction("Payments");
            }

            return View(order);
        }

    }
}