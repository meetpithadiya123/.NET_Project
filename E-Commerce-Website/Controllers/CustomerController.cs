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
            return View();
        }
    }
}