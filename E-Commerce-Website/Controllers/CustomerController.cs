using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Website.Controllers
{
    public class CustomerController : Controller
    {
        private readonly Mycontext _context;

        public CustomerController(Mycontext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");

            // 1. Fetch products from the database
            List<Product> products = _context.tbl_product.ToList();

            // 2. Pass the list into View()
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


        // 1. Action to show all products from the database
        public IActionResult AllProducts()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            var products = _context.tbl_product.ToList();
            return View(products);
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
    }
}