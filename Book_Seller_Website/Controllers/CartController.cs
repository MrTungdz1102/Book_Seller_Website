using Book_Seller_Website.Data;
using Book_Seller_Website.Data.ViewModel;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Book_Seller_Website.Controllers
{
	[Authorize]
	public class CartController : Controller
	{
		[BindProperty]
		public CartVM CartVM { get; set; }
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
				ListCart = _unit.ShopingCartRepository.GetAll(u => u.Userid == userId, includeProperties: "Product"),
				OrderHeader = new OrderHeader()
			};
			foreach (var item in result.ListCart)
			{
				item.Price = GetPriceBasedOnQuantity(item);
				result.OrderHeader.OrderTotal += item.Price * item.Count;
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
			var claimIdentity = (ClaimsIdentity)User.Identity;
			string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			CartVM = new()
			{
				ListCart = _unit.ShopingCartRepository.GetAll(u => u.Userid == userId, includeProperties: "Product"),
				OrderHeader = new OrderHeader()
			};
			CartVM.OrderHeader.User = _unit.UserRepository.Get(x => x.Id == userId);

			CartVM.OrderHeader.Name = CartVM.OrderHeader.User.Name;
			CartVM.OrderHeader.PhoneNumber = CartVM.OrderHeader.User.PhoneNumber;
			CartVM.OrderHeader.StreetAddress = CartVM.OrderHeader.User.StreetAddress;
			CartVM.OrderHeader.City = CartVM.OrderHeader.User.City;
			CartVM.OrderHeader.State = CartVM.OrderHeader.User.State;
			CartVM.OrderHeader.PostalCode = CartVM.OrderHeader.User.PostalCode;


			foreach (var item in CartVM.ListCart)
			{
				item.Price = GetPriceBasedOnQuantity(item);
				CartVM.OrderHeader.OrderTotal += item.Price * item.Count;
			}
			return View(CartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			CartVM.ListCart = _unit.ShopingCartRepository.GetAll(u => u.Userid == userId, includeProperties: "Product");

			CartVM.OrderHeader.Id = 0;
			CartVM.OrderHeader.OrderDate = System.DateTime.Now;
			CartVM.OrderHeader.UserId = userId;

			// cart.OrderHeader.User = _unit.UserRepository.Get(x => x.Id == userId);
			// khong su dung cart.OrderHeader.User vi khi add and save no se hieu nham thanh update do da co userid trong db
			User user = _unit.UserRepository.Get(x => x.Id == userId);
			foreach (var item in CartVM.ListCart)
			{
				item.Price = GetPriceBasedOnQuantity(item);
				CartVM.OrderHeader.OrderTotal += item.Price * item.Count;
			}
			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				// normal customer
				CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				CartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				// company customer
				CartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				CartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			_unit.OrderHeaderRepository.Add(CartVM.OrderHeader);
			_unit.Save();

			foreach (var item in CartVM.ListCart)
			{
				OrderDetail orderDetails = new()
				{
					ProductId = item.ProductId,
					OrderHeaderId = CartVM.OrderHeader.Id,
					Price = item.Price,
					Count = item.Count
				};
				_unit.OrderDetailRepository.Add(orderDetails);
				_unit.Save();
			}
			if (user.CompanyId.GetValueOrDefault() == 0)
			{
				// stripe payment
				var domain = "https://localhost:7084/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"cart/OrderConfirmation?id={CartVM.OrderHeader.Id}",
					CancelUrl = domain + "cart/index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in CartVM.ListCart)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							}
						},
						Quantity = item.Count
					};
					options.LineItems.Add(sessionLineItem);
				}

				var service = new SessionService();
				Session session = service.Create(options);
				_unit.OrderHeaderRepository.UpdateStripePaymentID(CartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unit.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);

			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = CartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = _unit.OrderHeaderRepository.Get(u => u.Id == id, includeProperties: "User");
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				// customer
				var service = new SessionService();
				
				Session session = service.Get(orderHeader.SessionId);

				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unit.OrderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
					_unit.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unit.Save();
				}
				HttpContext.Session.Clear();
			}
			List<ShopingCart> shoppingCarts = _unit.ShopingCartRepository
			   .GetAll(u => u.Userid == orderHeader.UserId).ToList();
			_unit.ShopingCartRepository.DeleteRange(shoppingCarts);
			_unit.Save();
			return View(id);
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
			if (result.Count <= 1)
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
