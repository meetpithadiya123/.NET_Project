using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Website.Controllers
{
    public class CustomerController : Controller
    {
        private readonly Mycontext _context;

        public CustomerController(Mycontext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            string? customerSession = HttpContext.Session.GetString("customerSession");
            if (!string.IsNullOrEmpty(customerSession) && string.IsNullOrEmpty(HttpContext.Session.GetString("customerName")))
            {
                if (int.TryParse(customerSession, out int id))
                {
                    var cust = _context.tbl_customer.Find(id);
                    if (cust != null && !string.IsNullOrEmpty(cust.customer_name))
                    {
                        HttpContext.Session.SetString("customerName", cust.customer_name);
                    }
                }
            }
        }

        public IActionResult Index(string? search)
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            ViewBag.SearchQuery = search;

            var query = _context.tbl_product.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(p => p.product_name.Contains(term) ||
                                        (p.product_description != null && p.product_description.Contains(term)) ||
                                        (p.Category != null && p.Category.category_name.Contains(term)));
            }

            List<Product> products = query.ToList();
            return View(products);
        }

        public IActionResult customerLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult customerLogin(string customer_email, string customer_password)
        {
            var customer = _context.tbl_customer
                .FirstOrDefault(c => c.customer_email == customer_email);

            if (customer != null && customer.customer_password == customer_password)
            {
                HttpContext.Session.SetString("customerSession", customer.customer_id.ToString());
                HttpContext.Session.SetString("customerName", customer.customer_name ?? "Customer");
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = "Incorrect Username or Password";
                return View();
            }
        }
        public IActionResult customerRegistration()
        {
            return View();
        }

        [HttpPost]
        public IActionResult customerRegistration(Customer customer)
        {
            _context.tbl_customer.Add(customer);
            _context.SaveChanges();
            return RedirectToAction("customerLogin");
        }

        public IActionResult customerLogout()
        {
            HttpContext.Session.Remove("customerSession");
            HttpContext.Session.Remove("customerName");
            return RedirectToAction("Index");
        }

        public IActionResult customerProfile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("customerSession")))
            {
                return RedirectToAction("customerLogin");
            }
            else
            {
                List<Category> category = _context.tbl_category.ToList();
                ViewData["category"] = category;
                var customerId = HttpContext.Session.GetString("customerSession");
                var row = _context.tbl_customer.Where(c => c.customer_id == int.Parse(customerId)).ToList();
                return View(row);
            }
        }
        [HttpPost]
        public IActionResult updatecustomerProfile(Customer customer)
        {
            _context.tbl_customer.Update(customer);
            _context.SaveChanges();
            if (!string.IsNullOrEmpty(customer.customer_name))
            {
                HttpContext.Session.SetString("customerName", customer.customer_name);
            }
            return RedirectToAction("customerProfile");
        }

        [HttpGet]
        public IActionResult feedback()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            return View();
        }

        [HttpPost]
        public IActionResult feedback(Feedback feedback)
        {
            _context.tbl_feedback.Add(feedback);
            _context.SaveChanges();

            // Reload categories for layout/navbar
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            // Set the success message
            ViewBag.FeedbackSuccess = "Thank you! Your feedback has been submitted successfully.";

            ModelState.Clear();
            return View();
        }

        [HttpGet]
        public IActionResult About()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            return View();
        }

        public IActionResult deletePermissionFeedback(int id)
        {
            var feedback = _context.tbl_feedback.Find(id);
            if (feedback != null)
            {
                _context.tbl_feedback.Remove(feedback);
                _context.SaveChanges();
            }
            return RedirectToAction("fetchfeedback");
        }


        // 1. Action to show all products from the database with search support
        public IActionResult AllProducts(string? search)
        {
            return allProduct(search);
        }

        public IActionResult allProduct(string? search)
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            ViewBag.SearchQuery = search;

            var query = _context.tbl_product.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(p => p.product_name.Contains(term) ||
                                        (p.product_description != null && p.product_description.Contains(term)) ||
                                        (p.Category != null && p.Category.category_name.Contains(term)));
            }

            var products = query.ToList();
            return View("allProduct", products);
        }

        // Live autocomplete search suggestions API
        [HttpGet]
        public IActionResult SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            string term = query.Trim();
            var results = _context.tbl_product
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
                .ToList();

            return Json(results);
        }

        // 2. Action to show a single product detail
        public IActionResult productDetails(int id)
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            var product = _context.tbl_product.FirstOrDefault(p => p.product_id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }





        [HttpPost]
        public IActionResult AddToCart(int prod_id)
        {
            string? isLogin = HttpContext.Session.GetString("customerSession");

            // If not logged in, notify frontend to redirect to login
            if (string.IsNullOrEmpty(isLogin))
            {
                return Json(new { success = false, redirect = Url.Action("customerLogin", "Customer") });
            }

            int customerId = int.Parse(isLogin);

            // Check if the item already exists in customer's cart
            var existingCartItem = _context.tbl_cart.FirstOrDefault(c =>
                c.prod_id == prod_id &&
                c.cust_id == customerId &&
                c.cart_status == 0);

            if (existingCartItem != null)
            {
                existingCartItem.product_quantity += 1;
                _context.tbl_cart.Update(existingCartItem);
            }
            else
            {
                Cart cart = new Cart
                {
                    prod_id = prod_id,
                    cust_id = customerId,
                    product_quantity = 1,
                    cart_status = 0
                };
                _context.tbl_cart.Add(cart);
            }

            _context.SaveChanges();

            return Json(new { success = true, message = "Product successfully added to cart!" });
        }

        public IActionResult fetchCart()
        {
            // 1. Fetch categories for navbar / menu layout
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            // 2. Retrieve customer ID from session
            string customerID = HttpContext.Session.GetString("customerSession");

            // 3. Check if user is logged in before parsing customerID
            if (string.IsNullOrEmpty(customerID))
            {
                // Redirect to your login action if session does not exist
                return RedirectToAction("customerLogin", "Customer");
            }

            // 4. Safely query the active cart items since customerID is verified
            var cart = _context.tbl_cart
                .Include(c => c.products)
                .Where(c => c.cust_id == int.Parse(customerID) && c.cart_status == 0)
                .ToList();

            return View(cart);
        }

        [HttpGet]
        public IActionResult deletecart(int id)
        {
            // 1. Verify user is logged in
            string customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            int custId = int.Parse(customerID);

            // 2. Locate the specific cart item belonging to this logged-in customer
            var cartItem = _context.tbl_cart.FirstOrDefault(c => c.cart_id == id && c.cust_id == custId);

            // 3. Remove the item if found
            if (cartItem != null)
            {
                _context.tbl_cart.Remove(cartItem);
                _context.SaveChanges();
            }

            // 4. Redirect back to the cart page to show updated items and totals
            return RedirectToAction("fetchCart", "Customer");
        }

        // ==========================================
        // CHECKOUT & PAYMENT WORKFLOW
        // ==========================================

        [HttpGet]
        public IActionResult Checkout()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            string customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            int custId = int.Parse(customerID);
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == custId);
            if (customer == null)
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            var cartItems = _context.tbl_cart
                .Include(c => c.products)
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .ToList();

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
        public IActionResult ProcessPayment(string payment_method, string? shipping_address, string? shipping_city, string? shipping_phone)
        {
            string customerID = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerID))
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            int custId = int.Parse(customerID);
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == custId);
            if (customer == null)
            {
                return RedirectToAction("customerLogin", "Customer");
            }

            // Optionally persist any updated customer contact info
            if (!string.IsNullOrWhiteSpace(shipping_address))
                customer.customer_address = shipping_address;
            if (!string.IsNullOrWhiteSpace(shipping_city))
                customer.customer_city = shipping_city;
            if (!string.IsNullOrWhiteSpace(shipping_phone))
                customer.customer_phone = shipping_phone;

            var cartItems = _context.tbl_cart
                .Include(c => c.products)
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("fetchCart", "Customer");
            }

            decimal totalAmount = 0;
            int totalItemsCount = 0;
            foreach (var item in cartItems)
            {
                if (item.products != null && decimal.TryParse(item.products.product_price, out decimal price))
                {
                    totalAmount += price * item.product_quantity;
                }
                totalItemsCount += item.product_quantity;

                // Move status from 0 (In-Cart) to 1 (Paid / Ordered)
                item.cart_status = 1;
                _context.tbl_cart.Update(item);
            }

            _context.SaveChanges();

            // Generate Mock Order & Transaction IDs
            string orderId = "ORD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);
            string transactionId = "TXN-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

            TempData["OrderId"] = orderId;
            TempData["TransactionId"] = transactionId;
            TempData["TotalAmount"] = totalAmount.ToString("N0");
            TempData["ItemsCount"] = totalItemsCount.ToString();
            TempData["PaymentMethod"] = string.IsNullOrEmpty(payment_method) ? "Credit/Debit Card" : payment_method;
            TempData["CustomerName"] = customer.customer_name;
            TempData["CustomerEmail"] = customer.customer_email;
            TempData["CustomerPhone"] = customer.customer_phone ?? "";
            TempData["DeliveryAddress"] = (customer.customer_address ?? "") + (string.IsNullOrEmpty(customer.customer_city) ? "" : ", " + customer.customer_city);

            return RedirectToAction("OrderSuccess");
        }

        [HttpGet]
        public IActionResult OrderSuccess()
        {
            if (TempData["OrderId"] == null)
            {
                return RedirectToAction("Index", "Customer");
            }

            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            TempData.Keep();

            return View();
        }
    }
}