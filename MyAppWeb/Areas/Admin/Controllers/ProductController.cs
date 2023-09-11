using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Data;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using MyApp.Models;
using MyApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;

namespace MyAppWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IUnitofWork _unitofwork;
        private IWebHostEnvironment _hostingEnvironment;
        public ProductController(IUnitofWork UnitofWork, IWebHostEnvironment hostingEnvironment = null)
        {
            _unitofwork = UnitofWork;
            _hostingEnvironment = hostingEnvironment;
        }
        #region Apicall
        public IActionResult AllProducts()
        {
             var Products = _unitofwork.Product.GetAll(includeproperties:"Category");

            return Json(new {  data = Products });
        }
            #endregion
            [HttpGet]
        public IActionResult Index()
        {
            //ProductVM productVM = new ProductVM();

            //productVM.Products = _unitofwork.Product.GetAll();

            return View();
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
            ProductVM vm = new ProductVM()
            {
                product = new(),
                Categories = _unitofwork.Category.GetAll().Select(x =>
                 new SelectListItem()
                 {
                     Text = x.Name,
                     Value = x.Id.ToString()
                 })
            };
            if (id == null || id == 0)
            {
                return View(vm);
            }
            else
            {
                vm.product = _unitofwork.Product.GetT(x => x.Id == id);
                if (vm.product == null)
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
        public IActionResult CreateUpdate(ProductVM VM,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string filename = string.Empty;
                if (file!=null)
                {
                    string uploadDir = Path.Combine(_hostingEnvironment.WebRootPath, "ProductImage");
                     filename= Guid.NewGuid().ToString()+"-"+file.FileName;
                    string  filePath = Path.Combine(uploadDir, filename);

                    if(VM.product.ImageURl!=null)
                    {
                        var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, VM.product.ImageURl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }



                    using(var filestream = new FileStream(filePath,FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    VM.product.ImageURl = @"/ProductImage/"+ filename;
                   
                }
                if(VM.product.Id==0)
                {
                    _unitofwork.Product.Add(VM.product);
                    TempData["success"] = "Product Created Done!";
                }
                else
                {
                    _unitofwork.Product.Update(VM.product);
                    TempData["success"] = "Product Updated Done!";
                }
                


                _unitofwork.save();
             
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        //[HttpGet]
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    var category = _unitofwork.Category.GetT(x => x.Id == id);
        //    if (category == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(category);
        //}

        #region DeleteAPICALL

        [HttpDelete]
        
        public IActionResult Delete(int? id)
        {
            var product = _unitofwork.Product.GetT(x => x.Id == id);
            if (product == null)
            {
                return Json(new {success=false,message="Error in Fetch Data"});
            }
            else
            {
                var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, product.ImageURl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);                   
                }
                _unitofwork.Product.Delete(product);
                _unitofwork.save();
                TempData["success"] = "Product Deleted Done!";
                return Json(new { success = true, message = "Product Deleted Done!" });
            }  
        }
        #endregion
    }
}
