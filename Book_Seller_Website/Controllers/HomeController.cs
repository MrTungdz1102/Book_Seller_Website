using Book_Seller_Website.Data;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

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
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(userId != null)
			{
                HttpContext.Session.SetInt32(SD.SessionCart, (_unit.ShopingCartRepository.GetAll(x => x.Userid == userId.Value)).Count());
            }
            IEnumerable<Product> result = _unit.ProductRepository.GetAll(includeProperties: "Category");
			return View(result);
		}

		public IActionResult Detail(int id)
		{
			ShopingCart result = new ShopingCart()
			{
				Product = _unit.ProductRepository.Get(u => u.Id == id, includeProperties: "Category"),
				Count = 1,
				ProductId = id
			};
			return View(result);
		}

		[HttpPost]
		[Authorize]
		public IActionResult Detail(ShopingCart shopingCart)
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value; // lay ra id nguoi dung
            shopingCart.Userid = userId;
            ShopingCart cart = _unit.ShopingCartRepository.Get(x => x.Userid == userId && x.ProductId == shopingCart.ProductId);
			if (cart != null)
			{
				cart.Count += shopingCart.Count;
				_unit.ShopingCartRepository.Update(cart);
				TempData["success"] = "update thanh cong";
                _unit.Save();
            }
			else
			{
				shopingCart.Id = 0;
                // id khong the them thu cong ma no se them tu dong, khong hieu sao truong hop nay id lai = 1,2,3... trong khi mac dinh phai la 0
                _unit.ShopingCartRepository.Add(shopingCart);
                _unit.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, (_unit.ShopingCartRepository.GetAll(x => x.Userid == userId)).Count());
                TempData["success"] = "add thanh cong";
			}
           
            return RedirectToAction(nameof(Index));
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