using E_Commerce_Website.Models;
using E_Commerce_Website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Website.Controllers
{
    public class CustomerController : Controller
    {
        private readonly Mycontext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(Mycontext context, IEmailService emailService, ILogger<CustomerController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            string? customerSession = HttpContext.Session.GetString("customerSession");
            if (!string.IsNullOrEmpty(customerSession) && string.IsNullOrEmpty(HttpContext.Session.GetString("customerName")))
            {
                if (int.TryParse(customerSession, out int id))
                {
                    var cust = _context.tbl_customer.AsNoTracking().FirstOrDefault(c => c.customer_id == id);
                    if (cust != null && !string.IsNullOrEmpty(cust.customer_name))
                    {
                        HttpContext.Session.SetString("customerName", cust.customer_name);
                    }
                }
            }
        }

        public async Task<IActionResult> Index(string? search)
        {
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            ViewBag.SearchQuery = search;

            var query = _context.tbl_product.AsNoTracking().Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(p => p.product_name.Contains(term) ||
                                        (p.product_description != null && p.product_description.Contains(term)) ||
                                        (p.Category != null && p.Category.category_name.Contains(term)));
            }

            List<Product> products = await query.ToListAsync();
            return View(products);
        }

        public IActionResult customerLogin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> customerLogin(string customer_email, string customer_password)
        {
            if (string.IsNullOrWhiteSpace(customer_email) || string.IsNullOrEmpty(customer_password))
            {
                ViewBag.message = "Incorrect Email or Password";
                return View();
            }

            string email = customer_email.Trim();

            var customer = await _context.tbl_customer
                .AsNoTracking()
                .OrderByDescending(c => c.customer_id)
                .FirstOrDefaultAsync(c => c.customer_email == email && c.customer_password == customer_password);

            if (customer != null)
            {
                HttpContext.Session.SetString("customerSession", customer.customer_id.ToString());
                HttpContext.Session.SetString("customerName", customer.customer_name ?? "Customer");
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = "Incorrect Email or Password";
                return View();
            }
        }

        public IActionResult customerRegistration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> customerRegistration(Customer customer)
        {
            // Prevent duplicate emails
            if (await _context.tbl_customer.AnyAsync(c => c.customer_email == customer.customer_email))
            {
                ViewBag.message = "An account with this email already exists.";
                return View(customer);
            }

            await _context.tbl_customer.AddAsync(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("customerLogin");
        }

        public IActionResult customerLogout()
        {
            HttpContext.Session.Remove("customerSession");
            HttpContext.Session.Remove("customerName");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> customerProfile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("customerSession")))
            {
                return RedirectToAction("customerLogin");
            }
            else
            {
                List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
                ViewData["category"] = category;
                var customerId = HttpContext.Session.GetString("customerSession");
                var row = await _context.tbl_customer
                    .AsNoTracking()
                    .Where(c => c.customer_id == int.Parse(customerId!))
                    .ToListAsync();
                return View(row);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> updatecustomerProfile(Customer customer)
        {
            _context.tbl_customer.Update(customer);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(customer.customer_name))
            {
                HttpContext.Session.SetString("customerName", customer.customer_name);
            }
            return RedirectToAction("customerProfile");
        }

        [HttpGet]
        public async Task<IActionResult> feedback()
        {
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> feedback(Feedback feedback)
        {
            await _context.tbl_feedback.AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Reload categories for layout/navbar
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;

            // Set the success message
            ViewBag.FeedbackSuccess = "Thank you! Your feedback has been submitted successfully.";

            ModelState.Clear();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> About()
        {
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;
            return View();
        }

        public async Task<IActionResult> deletePermissionFeedback(int id)
        {
            var feedback = await _context.tbl_feedback.FindAsync(id);
            if (feedback != null)
            {
                _context.tbl_feedback.Remove(feedback);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("fetchfeedback");
        }

        // 1. Action to show all products from the database with search support
        public async Task<IActionResult> AllProducts(string? search)
        {
            return await allProduct(search);
        }

        public async Task<IActionResult> allProduct(string? search)
        {
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;
            ViewBag.SearchQuery = search;

            var query = _context.tbl_product.AsNoTracking().Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(p => p.product_name.Contains(term) ||
                                        (p.product_description != null && p.product_description.Contains(term)) ||
                                        (p.Category != null && p.Category.category_name.Contains(term)));
            }

            var products = await query.ToListAsync();
            return View("allProduct", products);
        }

        // Live autocomplete search suggestions API
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            string term = query.Trim();
            var results = await _context.tbl_product
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.product_name.Contains(term) ||
                            (p.Category != null && p.Category.category_name.Contains(term)))
                .Take(6)
                .Select(p => new
                {
                    id = p.product_id,
                    name = p.product_name,
                    price = p.product_price,
                    image = p.product_image,
                    category = p.Category != null ? p.Category.category_name : ""
                })
                .ToListAsync();

            return Json(results);
        }

        // 2. Action to show a single product detail with category and related products
        public async Task<IActionResult> productDetails(int id)
        {
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;

            var product = await _context.tbl_product
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.product_id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Fetch related products (same category or flagship companions)
            var related = await _context.tbl_product
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.product_id != id && p.cat_id == product.cat_id)
                .Take(4)
                .ToListAsync();

            if (related.Count < 4)
            {
                var extra = await _context.tbl_product
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.product_id != id && !related.Select(r => r.product_id).Contains(p.product_id))
                    .Take(4 - related.Count)
                    .ToListAsync();
                related.AddRange(extra);
            }

            ViewBag.RelatedProducts = related;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int prod_id, int quantity = 1)
        {
            string? isLogin = HttpContext.Session.GetString("customerSession");

            // If not logged in, notify frontend to redirect to login
            if (string.IsNullOrEmpty(isLogin))
            {
                return Json(new { success = false, redirect = Url.Action("customerLogin", "Customer") });
            }

            if (quantity < 1) quantity = 1;
            int customerId = int.Parse(isLogin);

            // Check if the item already exists in customer's cart
            var existingCartItem = await _context.tbl_cart.FirstOrDefaultAsync(c =>
                c.prod_id == prod_id &&
                c.cust_id == customerId &&
                c.cart_status == 0);

            if (existingCartItem != null)
            {
                existingCartItem.product_quantity += quantity;
                _context.tbl_cart.Update(existingCartItem);
            }
            else
            {
                Cart cart = new Cart
                {
                    prod_id = prod_id,
                    cust_id = customerId,
                    product_quantity = quantity,
                    cart_status = 0
                };
                await _context.tbl_cart.AddAsync(cart);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Product successfully added to cart!" });
        }

        public async Task<IActionResult> fetchCart()
        {
            // 1. Fetch categories for navbar / menu layout
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;

            // 2. Retrieve customer ID from session
            string? customerID = HttpContext.Session.GetString("customerSession");

            // 3. Check if user is logged in before parsing customerID
            if (string.IsNullOrEmpty(customerID))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            // 4. Safely query the active cart items since customerID is verified
            var cart = await _context.tbl_cart
                .AsNoTracking()
                .Include(c => c.products)
                .Where(c => c.cust_id == int.Parse(customerID) && c.cart_status == 0)
                .ToListAsync();

            return View(cart);
        }

        [HttpGet]
        public async Task<IActionResult> deletecart(int id)
        {
            // 1. Verify user is logged in
            string? customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            int custId = int.Parse(customerID);

            // 2. Locate the specific cart item belonging to this logged-in customer
            var cartItem = await _context.tbl_cart.FirstOrDefaultAsync(c => c.cart_id == id && c.cust_id == custId);

            // 3. Remove the item if found
            if (cartItem != null)
            {
                _context.tbl_cart.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            // 4. Redirect back to the cart page to show updated items and totals
            return RedirectToAction("fetchCart", "Customer");
        }

        // ==========================================
        // CHECKOUT & PAYMENT WORKFLOW
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;

            string? customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            int custId = int.Parse(customerID);
            var customer = await _context.tbl_customer.AsNoTracking().FirstOrDefaultAsync(c => c.customer_id == custId);
            if (customer == null)
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            var cartItems = await _context.tbl_cart
                .AsNoTracking()
                .Include(c => c.products)
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("fetchCart", "Customer");
            }

            decimal subtotal = 0;
            foreach (var item in cartItems)
            {
                if (item.products != null && decimal.TryParse(item.products.product_price, out decimal price))
                {
                    subtotal += price * item.product_quantity;
                }
            }

            var model = new CheckoutViewModel
            {
                Customer = customer,
                CartItems = cartItems,
                Subtotal = subtotal,
                ShippingFee = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(string payment_method, string? shipping_address, string? shipping_city, string? shipping_postal_code, string? shipping_phone, string? shipping_email)
        {
            string? customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID) || !int.TryParse(customerID, out int custId))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            var customer = await _context.tbl_customer.FirstOrDefaultAsync(c => c.customer_id == custId);
            if (customer == null)
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            // Persist any updated customer contact info
            if (!string.IsNullOrWhiteSpace(shipping_address))
                customer.customer_address = shipping_address;
            if (!string.IsNullOrWhiteSpace(shipping_city))
                customer.customer_city = shipping_city;
            if (!string.IsNullOrWhiteSpace(shipping_phone))
                customer.customer_phone = shipping_phone;

            var cartItems = await _context.tbl_cart
                .Include(c => c.products)
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("fetchCart", "Customer");
            }

            decimal totalAmount = 0;
            foreach (var item in cartItems)
            {
                decimal price = 0;
                if (item.products != null && decimal.TryParse(item.products.product_price, out decimal p))
                {
                    price = p;
                }
                totalAmount += price * item.product_quantity;
            }

            // Generate unique transaction reference code
            string transactionId = "TXN-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            string finalPaymentMethod = string.IsNullOrWhiteSpace(payment_method) ? "Dummy Card" : payment_method;
            string enteredEmail = !string.IsNullOrWhiteSpace(shipping_email) 
                ? shipping_email.Trim() 
                : (customer.customer_email ?? string.Empty).Trim();

            // Begin EF Core database transaction to guarantee consistency
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CustomerId = custId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    OrderStatus = "Placed",
                    ShippingAddress = !string.IsNullOrWhiteSpace(shipping_address) ? shipping_address : (customer.customer_address ?? "Standard Delivery"),
                    City = !string.IsNullOrWhiteSpace(shipping_city) ? shipping_city : (customer.customer_city ?? "Default City"),
                    PostalCode = !string.IsNullOrWhiteSpace(shipping_postal_code) ? shipping_postal_code : "400001",
                    Phone = !string.IsNullOrWhiteSpace(shipping_phone) ? shipping_phone : (customer.customer_phone ?? "N/A"),
                    ShippingEmail = enteredEmail,
                    TransactionId = transactionId,
                    PaymentMode = finalPaymentMethod,
                    PaymentStatus = "Success"
                };

                _context.tbl_order.Add(order);
                await _context.SaveChangesAsync();

                var orderItemsList = new List<OrderItem>();
                foreach (var item in cartItems)
                {
                    decimal unitPrice = 0;
                    if (item.products != null && decimal.TryParse(item.products.product_price, out decimal p))
                    {
                        unitPrice = p;
                    }

                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.prod_id,
                        ProductName = item.products?.product_name ?? ("Product #" + item.prod_id),
                        UnitPrice = unitPrice,
                        Quantity = item.product_quantity,
                        SubTotal = unitPrice * item.product_quantity
                    };

                    _context.tbl_order_item.Add(orderItem);
                    orderItemsList.Add(orderItem);

                    // Mark as purchased in cart
                    item.cart_status = 1;
                    _context.tbl_cart.Update(item);
                }

                order.OrderItems = orderItemsList;
                order.Customer = customer;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Send transaction receipt email to user via Mailtrap SMTP
                try
                {
                    if (!string.IsNullOrWhiteSpace(enteredEmail))
                    {
                        await _emailService.SendOrderConfirmationEmailAsync(order, enteredEmail);
                    }
                }
                catch (Exception ex)
                {
                    // Log error so that a network or SMTP failure does not cancel a successfully placed order
                    _logger.LogError(ex, "Failed to send Mailtrap confirmation email for Order ID {OrderId}. Check delivery logs at https://mailtrap.io/sending/email_logs", order.OrderId);
                }

                // Pass the purchase-time entered email to OrderSuccess view
                TempData["EmailRecipient"] = enteredEmail;
                TempData["EmailSubject"] = $"Order #{order.OrderId} Confirmed - UrbanCart";

                return RedirectToAction("OrderSuccess", "Customer", new { id = order.OrderId });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";
                return RedirectToAction("Checkout", "Customer");
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int? id)
        {
            string? customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID) || !int.TryParse(customerID, out int custId))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            if (!id.HasValue)
            {
                return RedirectToAction("OrderHistory", "Customer");
            }

            var order = await _context.tbl_order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id.Value && o.CustomerId == custId);

            if (order == null)
            {
                return RedirectToAction("OrderHistory", "Customer");
            }

            if (TempData["EmailRecipient"] == null && !string.IsNullOrWhiteSpace(order.ShippingEmail))
            {
                TempData["EmailRecipient"] = order.ShippingEmail;
            }
            TempData.Keep("EmailRecipient");
            TempData.Keep("EmailSubject");

            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> OrderHistory()
        {
            string? customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID) || !int.TryParse(customerID, out int custId))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            List<Category> category = await _context.tbl_category.AsNoTracking().ToListAsync();
            ViewData["category"] = category;

            var orders = await _context.tbl_order
                .AsNoTracking()
                .Where(o => o.CustomerId == custId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

    }
}