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

namespace Book_Seller_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        // GET: Admin/Products/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    var product = await _unit.ProductRepository.GetAsync(c => c.Id == id);
        //    return View(product);
        //}

        //// POST: Admin/Products/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ISBN,Author,ListPrice,Price,Price50,Price100")] Product product)
        //{
        //    if (id != product.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _unit.ProductRepository.Update(product);
        //            _unit.Save();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!await ProductExists(product.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        @TempData["success"] = "Product edited successfully";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(product);
        //}

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var product = await _unit.ProductRepository.GetAsync(c => c.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unit.ProductRepository.GetAsync(c => c.Id == id);
            _unit.ProductRepository.Delete(product);
            _unit.Save();
            @TempData["success"] = "Product deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _unit.ProductRepository.Exists(id);
        }
    }
}
