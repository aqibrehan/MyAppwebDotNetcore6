using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using MyApp.DataAccessLayer.Migrations;
using MyApp.Models;
using MyApp.Models.ViewModels;
using Stripe;
using Stripe.Checkout;
using Stripe.TestHelpers;
using System.Security.Claims;

namespace MyAppWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {

        private IUnitofWork _unitofwork;

        public OrderController(IUnitofWork unitofwork)
        {
            _unitofwork = unitofwork;
        }
        #region Apicall
        public IActionResult AllOrders(string status)
        {
         
            IEnumerable<OrderHeader> orderHeader;
        //    orderHeader = _unitofwork.OrderHeader.GetAll(includeproperties: "ApplicationUser");
            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                orderHeader = _unitofwork.OrderHeader.GetAll(includeproperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeader = _unitofwork.OrderHeader.GetAll(x => x.ApplicationUserId == claims.Value);
            }
            switch (status)
            {
                case "pending":
                    orderHeader = orderHeader.Where(x => x.PaymentStatus == PaymentStatus.StatusPending);
                    break;
                case "approved":
                    orderHeader = orderHeader.Where(x => x.PaymentStatus == PaymentStatus.StatusApproved);
                    break;
                case "underprocess":
                    orderHeader = orderHeader.Where(x => x.Orderstatus == OrderStatus.StatusInProcess);
                    break;
                case "shipped":
                    orderHeader = orderHeader.Where(x => x.Orderstatus == OrderStatus.StatusShipped);
                    break;
                default:
                    break;
            }


       
        

            return Json(new { data = orderHeader });
        }
        #endregion

        public IActionResult OrderDetails(int id)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == id, includeproperties: "ApplicationUser"),
                OrderDetail = _unitofwork.OrderDetail.GetAll(x => x.Id == id, includeproperties: "Product")
            };

            return View(orderVM);
        }
        [Authorize(Roles = WebSiteRole.Role_Admin+","+WebSiteRole.Role_Employee)]
        [HttpPost]
        public IActionResult OrderDetails(OrderVM vm)
        {
            var orderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id);
            orderHeader.Name= vm.OrderHeader.Name;
            orderHeader.Phone = vm.OrderHeader.Phone;
            orderHeader.Address = vm.OrderHeader.Address;
            orderHeader.City = vm.OrderHeader.City;
            orderHeader.State = vm.OrderHeader.State;
            orderHeader.PostalCode = vm.OrderHeader.PostalCode;
            if(vm.OrderHeader.Carrier!=null)
            {
                orderHeader.Carrier= vm.OrderHeader.Carrier;
            }
            if (vm.OrderHeader.TrackingNumber != null)
            {
                orderHeader.TrackingNumber = vm.OrderHeader.TrackingNumber;
            }
            _unitofwork.OrderHeader.Update(orderHeader);
            _unitofwork.Save();
            TempData["success"] = "Info Updated";
            return RedirectToAction("OrderDetails","Order",new {id=vm.OrderHeader.Id});
       

        }

        [Authorize(Roles = WebSiteRole.Role_Admin + "," + WebSiteRole.Role_Employee)]
        public IActionResult InProcess(OrderVM vm)
        {
          
            _unitofwork.OrderHeader.UpdateStatus(vm.OrderHeader.Id,OrderStatus.StatusInProcess);
            _unitofwork.Save();
            TempData["success"] = "Order Status Updated-Inprocess";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.Id });


        }
        [Authorize(Roles = WebSiteRole.Role_Admin + "," + WebSiteRole.Role_Employee)]
        public IActionResult Shipped(OrderVM vm)
        {
            var orderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id);
            orderHeader.Carrier = vm.OrderHeader.Carrier;
            orderHeader.TrackingNumber = vm.OrderHeader.TrackingNumber;
            orderHeader.Orderstatus = OrderStatus.StatusShipped;
            orderHeader.DateOfShipping = DateTime.Now;

            _unitofwork.OrderHeader.Update(orderHeader);
            _unitofwork.Save();
            TempData["success"] = "Order Status Updated-Shipped";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.Id });


        }
        [Authorize(Roles = WebSiteRole.Role_Admin + "," + WebSiteRole.Role_Employee)]
        public IActionResult CancelOrder(OrderVM vm)
        {
            var orderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id);

            if(orderHeader.PaymentStatus==PaymentStatus.StatusApproved)
            {
                var refund = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new Stripe.RefundService();
                Refund Refund = service.Create(refund);
                _unitofwork.OrderHeader.UpdateStatus(orderHeader.Id, OrderStatus.StatusCancelled, OrderStatus.StatusRefund);
            }
            else
            {
                _unitofwork.OrderHeader.UpdateStatus(orderHeader.Id, OrderStatus.StatusCancelled, OrderStatus.StatusCancelled);
            }


            _unitofwork.Save();
            TempData["success"] = "Order Status Cancelled";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.Id });

        }
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult PayNow(OrderVM vm)
        {

         var OrderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id ,includeproperties: "ApplicationUser");
          var OrderDetail = _unitofwork.OrderDetail.GetAll(x => x.Id == vm.OrderHeader.Id, includeproperties: "Product");
            
            //stripe
            var domain = "https://localhost:7246/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Customer/cart/OrderSuccess?id={vm.OrderHeader.Id}",
                CancelUrl = domain + $"Customer/cart/Index",
            };


            //Order Detail Start
            foreach (var item in OrderDetail)
            {

                var lineItemsoption = new SessionLineItemOptions
                {
                    // Provide the exact Price ID (for example, pr_1234) of the product you want to sell
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * item.Count),
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
            _unitofwork.OrderHeader.PaymentStatus(vm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            return RedirectToAction("Index", "Home");
        }
    }
}
