using Book_Seller_Website.Data;
using Book_Seller_Website.Data.ViewModel;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Book_Seller_Website.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class UsersController : Controller
	{
		private readonly IUnitOfWork _unit;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		public UsersController(IUnitOfWork unit, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_unit = unit;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> RoleManagment(string userId)
		{
			RoleManagementVM RoleVM = new RoleManagementVM()
            {
                User = _unit.UserRepository.Get(u => u.Id == userId, includeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
				{
					Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unit.CompanyRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
					Value = i.Id.ToString()
				}),
			};

            RoleVM.User.Role = _userManager.GetRolesAsync(_unit.UserRepository.Get(u => u.Id == userId))
                    .GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

		[HttpPost]
		public async Task<IActionResult> RoleManagment([Bind] RoleManagementVM roleVM)
		{
			var user = _unit.UserRepository.Get(x => x.Id == roleVM.User.Id);
			var oldRoles = string.Join(",", await _userManager.GetRolesAsync(user));
			// truong hop 1 user co nhieu role
			//foreach (var roleName in userRoles)
			//{
			//             var role = await _roleManager.FindByNameAsync(roleName);
			//             string oldRole =await _roleManager.GetRoleIdAsync(role);
			//}
			if (!(roleVM.User.Role == oldRoles))
			{
				if (roleVM.User.Role == SD.Role_Company)
				{
					user.CompanyId = roleVM.User.CompanyId;
				}
				if (oldRoles == SD.Role_Company)
				{
					// role cu la company, bay gio chuyen sang role khac khong phai company
					user.CompanyId = null;
				}
				_unit.UserRepository.Update(user);
				_unit.Save();

				await _userManager.RemoveFromRoleAsync(user, oldRoles);
				await _userManager.AddToRoleAsync(user, roleVM.User.Role);
			}
			else
			{
				if(oldRoles != SD.Role_Company)
				{
                    user.CompanyId = roleVM.User.CompanyId;
                }
                _unit.UserRepository.Update(user);
                _unit.Save();
            }
            return RedirectToAction("Index");
        }

            #region API CALLS

            [HttpGet]
		public async Task<IActionResult> GetAll()
		{
			List<User> objList = _unit.UserRepository.GetAll(includeProperties: "Company").ToList();

			foreach (var item in objList)
			{
				if (item.CompanyId == null)
				{
					item.Company = new Company() { Name = "" };
				}
				// han che dung .GetAwaiter().GetResult() 
				item.Role = string.Join(",", await _userManager.GetRolesAsync(item));
			}
			return Json(new { data = objList });
		}

		[HttpPost]
		public IActionResult LockUnlock([FromBody] string id)
		{
			var user = _unit.UserRepository.Get(u => u.Id == id);
			if (user == null)
			{
				return Json(new { success = false, message = "Error while locking/unlocking" });
			}
			if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
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
