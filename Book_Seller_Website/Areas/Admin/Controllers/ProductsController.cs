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

namespace Book_Seller_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unit;
        public ProductsController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        // GET: Admin/Products
        public IActionResult Index()
        {
            IEnumerable<Product> result = _unit.ProductRepository.GetAll();
            return View(result);
        }
        
     // GET: Admin/Products/Create
        public async Task<IActionResult> UpSert(int? id)
        {
            var product = await _unit.ProductRepository.GetAsync(c => c.Id == id);
            if (id == null || id == 0)
            {
                return View();
            }
            else
            {
                return View(product);
            }
         //   ViewBag.DanhMuc = new SelectList(_context.Categories, "Id", "Name");
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ISBN,Author,ListPrice,Price,Price50,Price100")] Product product, IFormFile? formFile)
        {
            if (ModelState.IsValid)
            {
                await _unit.ProductRepository.AddAsync(product);
                _unit.Save();
                @TempData["success"] = "Product created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
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
