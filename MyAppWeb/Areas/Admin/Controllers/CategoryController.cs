﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Data;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using MyApp.Models;
using MyApp.Models.ViewModels;

namespace MyAppWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =WebSiteRole.Role_Admin)]
    public class CategoryController : Controller
    {
        private IUnitofWork _unitofwork;

        public CategoryController(IUnitofWork UnitofWork)
        {
            _unitofwork = UnitofWork;
        }
        [HttpGet]
        public IActionResult Index()
        {
            CategoryVM categoryVM = new CategoryVM();

            categoryVM.categories = _unitofwork.Category.GetAll();

            return View(categoryVM);
        }

        //[HttpGet]
        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(Category category)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitofwork.Category.Add(category);
        //        _unitofwork.save();
        //        TempData["success"] = "Category Created Done!";
        //        return RedirectToAction("Index");
        //    }
        //    return View(category);
        //}

        [HttpGet]
        public IActionResult CreateUpdate(int? id)
        {
            CategoryVM vm = new CategoryVM();
            if (id == null || id == 0)
            {
                return View(vm);
            }
            else
            {
                vm.category = _unitofwork.Category.GetT(x => x.Id == id);
                if (vm.category == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(vm);
                }
               
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUpdate(CategoryVM VM)
        {
            if (ModelState.IsValid)
            {
                if(VM.category.Id==0)
                {
                    _unitofwork.Category.Add(VM.category);
                    TempData["success"] = "Category Created Done!";
                }
                else
                {
                    _unitofwork.Category.Update(VM.category);
                    TempData["success"] = "Category Updated Done!";
                }
               
                _unitofwork.Save();
             
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = _unitofwork.Category.GetT(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteData(int? id)
        {
            var category = _unitofwork.Category.GetT(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitofwork.Category.Delete(category);
            _unitofwork.Save();
            TempData["success"] = "Category Deleted Done!";
            return RedirectToAction("Index");
        }
    }
}
