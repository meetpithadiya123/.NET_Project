using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Website.Controllers
{
    public class AdminController : Controller
    {
        private readonly Mycontext _context;

        public AdminController(Mycontext context)
        {
            _context = context;
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

            return View();
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





    }

        
}