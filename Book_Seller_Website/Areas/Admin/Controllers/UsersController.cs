﻿using Book_Seller_Website.Data;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Book_Seller_Website.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class UsersController : Controller
	{
		private readonly IUnitOfWork _unit;
        private readonly UserManager<IdentityUser> _userManager;
        public UsersController(IUnitOfWork unit, UserManager<IdentityUser> userManager)
		{
			_unit = unit;
            _userManager = userManager;
        }
		public IActionResult Index()
		{
			return View();
		}

		#region API CALLS

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			List<User> objList = _unit.UserRepository.GetAll(includeProperties: "Company").ToList();
            
			foreach(var item in objList)
			{
				if (item.CompanyId == null)
				{
					item.Company = new Company() { Name = "" };
				}
                // han che dung .GetAwaiter().GetResult() 
				// co the su dung rolemanager
                item.Role = string.Join(",",await _userManager.GetRolesAsync(item));
            }
			return Json(new { data = objList });
		}
        
		[HttpPost]
		public IActionResult LockUnlock([FromBody]string id)
		{
			var user = _unit.UserRepository.Get(u => u.Id == id);
			if (user == null)
			{
				return Json(new { success = false, message = "Error while locking/unlocking" });
			}
            if(user.LockoutEnd!= null&& user.LockoutEnd>DateTime.Now)
			{
				user.LockoutEnd = DateTime.Now;
			}
            else
			{
				user.LockoutEnd = DateTime.Now.AddYears(100);
			}
			_unit.UserRepository.Update(user); 
			_unit.Save();
			return Json(new { success = true, message = "Lock/Unlock Successful" });
		}
		#endregion
	}
}
