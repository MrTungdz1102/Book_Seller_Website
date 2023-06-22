using Book_Seller_Website.Data;
using Book_Seller_Website.Models.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Book_Seller_Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unit;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unit)
        {
            _logger = logger;
            _unit = unit;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> result = _unit.ProductRepository.GetAll(includeProperties: "Category");
            return View(result);
        }
        public IActionResult Detail(int id)
        {
            var result = _unit.ProductRepository.Get(u => u.Id == id , includeProperties: "Category");
            return View(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}