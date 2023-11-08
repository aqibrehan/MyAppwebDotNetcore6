using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Infrastructure.IRepository;

using MyApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MyAppWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _unitofwork;

        public HomeController(ILogger<HomeController> logger, IUnitofWork unitofwork)
        {
            _logger = logger;
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitofwork.Product.GetAll(includeproperties: "Category");

            return View(products);
        }
        [HttpGet]
        public IActionResult Details(int? ProductId)
        {

            Cart cart = new Cart()
            {
                Product = _unitofwork.Product.GetT(x => x.Id== ProductId, includeproperties: "Category"),
                 Count=1,
                 ProductId=(int)ProductId
            };

            

            return View(cart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(Cart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            cart.ApplicationUserId = claim.Value;

            var cartItem = _unitofwork.Cart.GetT(x=>x.ProductId== cart.ProductId &&
            x.ApplicationUserId== claim.Value);

            
            if (ModelState.IsValid)
            {
                if (cartItem == null)
                {
                    _unitofwork.Cart.Add(cart);
                    _unitofwork.Save();
                    HttpContext.Session.SetInt32("SessionCart", _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count);
                
                }
                else
                {
                    _unitofwork.Cart.IncrementCartItem(cartItem,cart.Count);
                    _unitofwork.Save();
                }
          


            }



            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}