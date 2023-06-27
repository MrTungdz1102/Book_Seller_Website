using Book_Seller_Website.Data;
using Book_Seller_Website.Data.ViewModel;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
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
