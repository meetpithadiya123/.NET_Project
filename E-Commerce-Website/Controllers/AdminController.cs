using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Website.Controllers
{
    public class AdminController : Controller
    {
        private Mycontext _context;
        public AdminController(Mycontext context) 
        { 
            _context = context;
        }

        public IActionResult Index()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if(admin_session != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var row = _context.tbl_admin.FirstOrDefault(a => a.admin_email == email);
            if (row != null && row.admin_password == password)
            {
                HttpContext.Session.SetString("admin_session", row.admin_id.ToString());
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = "Incorrect username or password";
            }
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin_session");
            return RedirectToAction("login");
        }

    }
}
