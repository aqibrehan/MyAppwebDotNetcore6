using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Data;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using MyApp.Models;


namespace MyAppWeb.Controllers
{
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
            IEnumerable<Category> categories = _unitofwork.Category.GetAll();

            return View(categories);
        }

        [HttpGet]       
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid) 
            {
                _unitofwork.Category.Add(category);
                _unitofwork.save();
                TempData["success"] = "Category Created Done!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int?id)
        {
            if (id == null || id ==0)
            {
                return NotFound();
            }
           
            var category = _unitofwork.Category.GetT(x=>x.Id==id);
            if(category== null) 
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitofwork.Category.Update(category);
                _unitofwork.save();
                TempData["success"] = "Category Updated Done!";
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
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteData(int? id)
        {
            var category = _unitofwork.Category.GetT(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitofwork.Category.Delete(category);
            _unitofwork.save();
            TempData["success"] = "Category Deleted Done!";
            return RedirectToAction("Index");
        }
    }
}
