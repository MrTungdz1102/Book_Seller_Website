using Book_Seller_Website.Controllers;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Book_Seller_Website.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unit;
        public ShoppingCartViewComponent(IUnitOfWork unit)
        {
            _unit = unit;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userId != null)
            {

                if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(SD.SessionCart,
                    _unit.ShopingCartRepository.GetAll(u => u.Userid == userId.Value).Count());
                }

                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
