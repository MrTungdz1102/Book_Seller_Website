using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Data;
using Book_Seller_Website.Models.Interface;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Book_Seller_Website.Utility;

namespace Book_Seller_Website.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _unit;

        public CategoriesController(IUnitOfWork unit)
        {
            _unit = unit;
        }

        // GET: Admin/Categories
        public IActionResult Index()
        {
            IEnumerable<Category> result =  _unit.CategoryRepository.GetAll();
            return View(result);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DisplayOrder,CreatedDateTime")] Category category)
        {
            if (ModelState.IsValid)
            {
                await _unit.CategoryRepository.AddAsync(category);
                _unit.Save();
                @TempData["success"] = "Category created successfully";
				return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = await _unit.CategoryRepository.GetAsync(c => c.Id == id);
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DisplayOrder,CreatedDateTime")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unit.CategoryRepository.Update(category);
                    _unit.Save();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
				@TempData["success"] = "Category edited successfully";
				return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id ==0)
            {
                return NotFound();
            }

            var category = await _unit.CategoryRepository.GetAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _unit.CategoryRepository.GetAsync(c => c.Id == id);
            _unit.CategoryRepository.Delete(category);
            _unit.Save();
			@TempData["success"] = "Category deleted successfully";
			return RedirectToAction(nameof(Index));
        }

        private async Task<bool> CategoryExists(int id)
        {
          return await _unit.CategoryRepository.Exists(id);
        }
    }
}
