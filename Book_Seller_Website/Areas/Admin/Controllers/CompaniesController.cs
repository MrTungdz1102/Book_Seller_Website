using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Book_Seller_Website.Data;
using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Data.ViewModel;
using Book_Seller_Website.Models.Repository;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Book_Seller_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompaniesController : Controller
    {
        private readonly IUnitOfWork _unit;
        public CompaniesController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        // GET: Admin/Companies
        public IActionResult Index()
        {
            // dung datatable thi khong can
          //  IEnumerable<Company> result = _unit.CompanyRepository.GetAll();
            return View();
        }
        
     // GET: Admin/Companies/Create
        public async Task<IActionResult> UpSert(int? id)
        { 
            if (id == null || id == 0)
            {
				//  return(new Company()) hoac return View() thi phai check cho the input hidden
				return View();
            }
            else
            {
                var company = await _unit.CompanyRepository.GetAsync(c => c.Id == id);
                return View(company);
            }
       
        }

        // POST: Admin/Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(Company company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id ==0)
                {
                    await _unit.CompanyRepository.AddAsync(company);
                }
                else
                {
                    _unit.CompanyRepository.Update(company);
                }
                _unit.Save();
                @TempData["success"] = "Product created/updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(company);
            }   
        }

      
       

        #region API CALLS
        
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objProductList = _unit.CompanyRepository.GetAll().ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unit.CompanyRepository.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unit.CompanyRepository.Delete(productToBeDeleted);
            _unit.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion 
    }
}
