using Book_Seller_Website.Data;
using Book_Seller_Website.Data.ViewModel;
using Book_Seller_Website.Models.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Book_Seller_Website.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unit;
        public CartController(IUnitOfWork unit)
        {
            _unit = unit;
        }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            CartVM result = new()
            {
                ListCart = _unit.ShopingCartRepository.GetAll(u => u.Userid == userId, includeProperties: "Product")
            };
            foreach (var item in result.ListCart)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                result.GrandTotal += item.Price * item.Count;
            }
            return View(result);
        }

        private double GetPriceBasedOnQuantity(ShopingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

        public IActionResult Summary()
        {
            return View();
        }
        
        public IActionResult Plus(int id)
        {
            var result = _unit.ShopingCartRepository.Get(x => x.Id == id);
            result.Count += 1;
            _unit.ShopingCartRepository.Update(result);
            _unit.Save();
            return RedirectToAction(nameof(Index));
        }
        
        public IActionResult Minus(int id)
        {
            var result = _unit.ShopingCartRepository.Get(x => x.Id == id);
            if(result.Count <= 1)
            {
                _unit.ShopingCartRepository.Delete(result);
            }
            else
            {
                result.Count -= 1;
                _unit.ShopingCartRepository.Update(result);
            }
            _unit.Save();
            return RedirectToAction(nameof(Index)); 
        }

        public IActionResult Clear(int id)
        {
            var result = _unit.ShopingCartRepository.Get(x => x.Id == id);
            if (result != null)
            {
                _unit.ShopingCartRepository.Delete(result);
            }
            _unit.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
