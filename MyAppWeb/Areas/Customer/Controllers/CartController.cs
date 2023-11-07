using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModels;
using Stripe.Checkout;
using System.Globalization;
using System.Security.Claims;

namespace MyAppWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        private readonly IUnitofWork _unitofwork;
        public CartVM VM { get; set; }
        public CartController(IUnitofWork unitofwork)
        {
            _unitofwork = unitofwork;
      

        }
     
        
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            VM = new CartVM()
            {
                ListofCart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claim.Value, includeproperties: "Product"),
                orderHeader = new MyApp.Models.OrderHeader()
            };
         

            foreach (var item in VM.ListofCart)
            {
                VM.orderHeader.OrderTotal += (item.Product.Price * item.Count);
            }

            return View(VM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            VM = new CartVM()
            {
                ListofCart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claim.Value, includeproperties: "Product"),
                orderHeader = new MyApp.Models.OrderHeader()
            };
            VM.orderHeader.ApplicationUser = _unitofwork.ApplicationUser.GetT(x => x.Id == claim.Value);

            VM.orderHeader.Name = VM.orderHeader.ApplicationUser.Name;
            VM.orderHeader.Phone = ((Microsoft.AspNetCore.Identity.IdentityUser<string>)VM.orderHeader.ApplicationUser).PhoneNumber;
            VM.orderHeader.Address = VM.orderHeader.ApplicationUser.Address;
            VM.orderHeader.City = VM.orderHeader.ApplicationUser.City;
            VM.orderHeader.State = VM.orderHeader.ApplicationUser.State;
            VM.orderHeader.PostalCode = VM.orderHeader.ApplicationUser.PinCode;


            foreach (var item in VM.ListofCart)
            {
                VM.orderHeader.OrderTotal += (item.Product.Price * item.Count);
            }

            return View(VM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Summary(CartVM vm)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            vm.ListofCart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claim.Value, includeproperties: "Product");
            // OrderHeader start
            vm.orderHeader.Orderstatus = OrderStatus.StatusPending;
            vm.orderHeader.PaymentStatus = PaymentStatus.StatusPending;
            vm.orderHeader.DateofTime = DateTime.Now;
            vm.orderHeader.ApplicationUserId = claim.Value;

            foreach (var item in vm.ListofCart)
            {
                vm.orderHeader.OrderTotal += (item.Product.Price * item.Count);
            }
            _unitofwork.OrderHeader.Add(vm.orderHeader);
            _unitofwork.Save();
            //--OrderHeader End

            //Order Detail Start
            foreach (var item in vm.ListofCart)
            {
                OrderDetail orderdetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = vm.orderHeader.Id,
                    Count = item.Count,
                    Price = item.Product.Price
                };
                _unitofwork.OrderDetail.Add(orderdetail);
                _unitofwork.Save();
            }
            //_unitofwork.Cart.DeleteRange(vm.ListofCart);
            //_unitofwork.Save();


            //stripe
            var domain = "https://localhost:7246/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>() ,
                Mode = "payment",
                SuccessUrl = domain + $"Customer/cart/OrderSuccess?id={vm.orderHeader.Id}",
                CancelUrl = domain + $"Customer/cart/Index",
            };


            //Order Detail Start
            foreach (var item in vm.ListofCart)
            {

                var lineItemsoption = new SessionLineItemOptions
                {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*item.Count),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        }
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(lineItemsoption);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitofwork.OrderHeader.PaymentStatus(vm.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();


            _unitofwork.Cart.DeleteRange(vm.ListofCart);
            _unitofwork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            return RedirectToAction("Index","Home");
        }

        public IActionResult OrderSuccess(int Id)
        {

            var orderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == Id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);
            if(session.PaymentStatus.ToLower()=="paid")
            {
                _unitofwork.OrderHeader.UpdateStatus(Id,OrderStatus.StatusApproved,PaymentStatus.StatusApproved);
            }
            List<Cart> cart = _unitofwork.Cart.GetAll(x=>x.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
            _unitofwork.Cart.DeleteRange(cart);
            _unitofwork.Save();


            return View(Id);
        }
            public IActionResult plus(int id)
        {
            var cart =_unitofwork.Cart.GetT(x=>x.Id== id);
            _unitofwork.Cart.IncrementCartItem(cart, 1);
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cart = _unitofwork.Cart.GetT(x => x.Id == id);
            if(cart.Count<=1)
            {
                _unitofwork.Cart.Delete(cart);
              
       
            }
            else
            {
                _unitofwork.Cart.DecrementCartItem(cart, 1);
            }
           
            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult delete(int id)
        {
            var cart = _unitofwork.Cart.GetT(x => x.Id == id);
           
            _unitofwork.Cart.Delete(cart);           

            _unitofwork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
