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
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unit;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductsController(IUnitOfWork unit, IWebHostEnvironment webHostEnvironment)
        {
            _unit = unit;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Products
        public IActionResult Index()
        {
            IEnumerable<Product> result = _unit.ProductRepository.GetAll(includeProperties:"Category");
            return View(result);
        }
        
     // GET: Admin/Products/Create
        public async Task<IActionResult> UpSert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unit.CategoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = await _unit.ProductRepository.GetAsync(c => c.Id == id);
                return View(productVM);
            }
         //   ViewBag.DanhMuc = new SelectList(_context.Categories, "Id", "Name");
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(ProductVM productVM, IFormFile? formFile)
        {
            if (ModelState.IsValid)
            {
               
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (formFile != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName); // rename ten file 1 cach ngau nhien
                    string productPath = Path.Combine(wwwRootPath, @"images\");
                    if (!string.IsNullOrEmpty(productVM.Product.ProductImages))
                    {
                        var oldImgPath = Path.Combine(wwwRootPath, @"images\"+productVM.Product.ProductImages);
                        if (System.IO.File.Exists(oldImgPath))
                        {
                            System.IO.File.Delete(oldImgPath);                        
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        formFile.CopyTo(fileStream);
                    }
                    productVM.Product.ProductImages = fileName;
                }
                if(productVM.Product.Id ==0)
                {
                    await _unit.ProductRepository.AddAsync(productVM.Product);
                }
                else
                {
                    _unit.ProductRepository.Update(productVM.Product);
                }
                _unit.Save();
                @TempData["success"] = "Product created/updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(productVM);
            }   
        }

      
       

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unit.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unit.ProductRepository.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImgPath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\" + productToBeDeleted.ProductImages);
            if (System.IO.File.Exists(oldImgPath))
            {
                System.IO.File.Delete(oldImgPath);
            }
            

            _unit.ProductRepository.Delete(productToBeDeleted);
            _unit.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion 
    }
}
