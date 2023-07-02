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
            _webHostEnvironment = webHostEnvironment; // lay duong dan wwwroot
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
                productVM.Product = await _unit.ProductRepository.GetAsync(c => c.Id == id, includeProperties: "ProductImages");
                // include theo quan he 1-1 hay 1-n, phai giong voi navigation() trong file snapshot hoac trong model
                return View(productVM);
            }
         //   ViewBag.DanhMuc = new SelectList(_context.Categories, "Id", "Name");
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpSert(ProductVM productVM, List<IFormFile>? formFiles)
        {
            if (ModelState.IsValid)
            {
				if (productVM.Product.Id == 0)
				{
					await _unit.ProductRepository.AddAsync(productVM.Product);
				}
				else
				{
					_unit.ProductRepository.Update(productVM.Product);
                    
                }
                _unit.Save();
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (formFiles != null)
                {
					foreach(var item in formFiles)
                    {
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName); // rename ten file 1 cach ngau nhien

                        //	string productPath = Path.Combine(wwwRootPath, @"images\");

                        string productPath = @"images\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);
                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }
						using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            item.CopyTo(fileStream);
                        }
                        ProductImage productImage = new ProductImage()
                        {
                            Image = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id
                        };
                        if(productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List<ProductImage>();
						}

                        // co the dung _unit.productimagerepository.add(productimage)
                        productVM.Product.ProductImages.Add(productImage);
                    }
                    _unit.ProductRepository.Update(productVM.Product);
                    _unit.Save();

                    // su dung khi co 1 anh, xoa anh cu chen anh moi
					//if (!string.IsNullOrEmpty(productVM.Product.ProductImages))
					//{
					//    var oldImgPath = Path.Combine(wwwRootPath, @"images\"+productVM.Product.ProductImages);
					//    if (System.IO.File.Exists(oldImgPath))
					//    {
					//        System.IO.File.Delete(oldImgPath);                        
					//    }
					//}
					//using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					//{
					//    formFile.CopyTo(fileStream);
					//}
					//productVM.Product.ProductImages = fileName;
				}
              
                 
                @TempData["success"] = "Product created/updated successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(productVM);
            }   
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unit.ProductImageRepository.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.Image))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.Image.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unit.ProductImageRepository.Delete(imageToBeDeleted);
                _unit.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(UpSert), new { id = productId });
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

            string productPath = @"images\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);
            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                // xoa file ben trong folder truoc roi moi xoa folder de tranh loi Directory is not empty
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }

            //    var oldImgPath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\" + productToBeDeleted.ProductImages);
            //if (System.IO.File.Exists(oldImgPath))
            //{
            //    System.IO.File.Delete(oldImgPath);
            //}


            _unit.ProductRepository.Delete(productToBeDeleted);
            _unit.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion 
    }
}
