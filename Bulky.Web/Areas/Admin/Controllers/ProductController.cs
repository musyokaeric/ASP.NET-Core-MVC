﻿using Bulky.Data.Data;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var products = unitOfWork.Product.GetAll(includeProperties:"Category");
            return View(products);
        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> categories = unitOfWork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            // ViewBag.Categories = categories;

            ProductVM productVM = new()
            {
                Categories = categories,
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                // Create new product
                return View(productVM);
            }
            else
            {
                // Update product
                productVM.Product = unitOfWork.Product.Get(p => p.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(rootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        // delete the old image
                        var oldImagePath = Path.Combine(rootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"images\product\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    unitOfWork.Product.Update(productVM.Product);
                }
                
                unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.Categories = unitOfWork.Category.GetAll().Select(p => new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
                return View(productVM);
            }
        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0) return NotFound();

        //    var product = unitOfWork.Product.Get(p => p.Id == id);
        //    if (product == null) return NotFound();


        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    var product = unitOfWork.Product.Get(p => p.Id == id);
        //    if (product == null) return NotFound();

        //    unitOfWork.Product.Remove(product);
        //    unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";
        //    return RedirectToAction("Index");
        //}

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = unitOfWork.Product.GetAll(includeProperties: "Category");
            return Json(new { data = products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = unitOfWork.Product.Get(p => p.Id == id);
            if (product == null) return Json(new { success = false, message = "Error while deleting" });

            // delete the old image
            var oldImagePath = Path.Combine(webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            unitOfWork.Product.Remove(product);
            unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull" });
        }

        #endregion
    }

}
