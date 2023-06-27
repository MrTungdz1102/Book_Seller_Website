using Book_Seller_Website.Data;
using Book_Seller_Website.Data.ViewModel;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Book_Seller_Website.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrdersController : Controller
	{
		private readonly IUnitOfWork _unit;
		public OrdersController(IUnitOfWork unit)
		{
			_unit = unit;
		}
		public IActionResult Index()
		{
            return View();
		}

        public IActionResult Detail(int orderId)
        {
            OrderDetailVM orderDetailVM = new()
            {
                OrderHeader = _unit.OrderHeaderRepository.Get(x => x.Id == orderId,includeProperties: "User"),
                OrderDetail = _unit.OrderDetailRepository.GetAll(x => x.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(orderDetailVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail([Bind] OrderDetailVM orderDetailVM)
        {
			var orderHeader = _unit.OrderHeaderRepository.Get(x => x.Id == orderDetailVM.OrderHeader.Id, includeProperties: "User");
			orderHeader.Name = orderDetailVM.OrderHeader.Name;
			orderHeader.PhoneNumber = orderDetailVM.OrderHeader.PhoneNumber;
			orderHeader.StreetAddress = orderDetailVM.OrderHeader.StreetAddress;
			orderHeader.City = orderDetailVM.OrderHeader.City;
			orderHeader.State = orderDetailVM.OrderHeader.State;
			orderHeader.PostalCode = orderDetailVM.OrderHeader.PostalCode;
			if (!string.IsNullOrEmpty(orderDetailVM.OrderHeader.Carrier))
			{
				orderHeader.Carrier = orderDetailVM.OrderHeader.Carrier;
			}
			if (!string.IsNullOrEmpty(orderDetailVM.OrderHeader.TrackingNumber))
			{
				orderHeader.Carrier = orderDetailVM.OrderHeader.TrackingNumber;
			}
			_unit.OrderHeaderRepository.Update(orderHeader);
			_unit.Save();
			@TempData["success"] = "Updated successfully";
			return RedirectToAction(nameof(Detail), new { orderId = orderHeader.Id });
		}

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(OrderDetailVM orderDetailVM)
		{
            _unit.OrderHeaderRepository.UpdateStatus(orderDetailVM.OrderHeader.Id, SD.StatusInProcess);
			_unit.Save();
            @TempData["success"] = "Updated successfully";
            return RedirectToAction(nameof(Detail), new { orderId = orderDetailVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder(OrderDetailVM orderDetailVM)
        {
            var orderHeader = _unit.OrderHeaderRepository.Get(x => x.Id == orderDetailVM.OrderHeader.Id, includeProperties: "User");
            orderHeader.Carrier = orderDetailVM.OrderHeader.Carrier;
            orderHeader.TrackingNumber = orderDetailVM.OrderHeader.TrackingNumber;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unit.OrderHeaderRepository.Update(orderHeader);
            _unit.OrderHeaderRepository.UpdateStatus(orderDetailVM.OrderHeader.Id, SD.StatusShipped);
            _unit.Save();
            @TempData["success"] = "Updated successfully";
            return RedirectToAction(nameof(Detail), new { orderId = orderDetailVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderDetailVM orderDetailVM)
		{
            var orderHeader = _unit.OrderHeaderRepository.Get(x => x.Id == orderDetailVM.OrderHeader.Id, includeProperties: "User");
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unit.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unit.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unit.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
            return RedirectToAction(nameof(Detail), new { orderId = orderDetailVM.OrderHeader.Id });
        }
        [HttpPost]
        public IActionResult PayNow(OrderDetailVM orderDetailVM)
        {
            orderDetailVM.OrderHeader = _unit.OrderHeaderRepository
               .Get(u => u.Id == orderDetailVM.OrderHeader.Id, includeProperties: "User");
            orderDetailVM.OrderDetail = _unit.OrderDetailRepository
                .GetAll(u => u.OrderHeaderId == orderDetailVM.OrderHeader.Id, includeProperties: "Product");
            var domain = "https://localhost:7084/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/orders/PaymentConfirmation?orderHeaderId={orderDetailVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/orders/detail?orderId={orderDetailVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderDetailVM.OrderDetail)
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
            // downgrade stripe package version below 39.199 to add payment intent
            _unit.OrderHeaderRepository.UpdateStripePaymentID(orderDetailVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unit.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unit.OrderHeaderRepository.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unit.OrderHeaderRepository.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unit.OrderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unit.Save();
                }
            }
            return View(orderHeaderId);
        }


        #region API CALLS

        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> objList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
                objList = _unit.OrderHeaderRepository.GetAll(includeProperties: "User").ToList();
            }
			else
			{
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objList = _unit.OrderHeaderRepository.GetAll(x =>x.UserId == userId,includeProperties: "User").ToList();
            }
			switch (status)
			{
				case "pending":
					objList = objList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
					break;
				case "inprocess":
					objList = objList.Where(u => u.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					objList = objList.Where(u => u.OrderStatus == SD.StatusShipped);
					break;
				case "approved":
					objList = objList.Where(u => u.OrderStatus == SD.StatusApproved);
					break;
				default:
					break;
			}
            return Json(new { data = objList });
        }
		#endregion
	}
}
