using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using System.Security.Claims;

namespace MyAppWeb.Areas.ViewComponents
{
    public class CartViewComponent:ViewComponent
    {
        private readonly IUnitofWork _unitofwork;

        public CartViewComponent(IUnitofWork unitofwork)
        {
            _unitofwork = unitofwork;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null) 
            {
                if(HttpContext.Session.GetInt32("SessionCart")!=null)
                {
                    return View(HttpContext.Session.GetInt32("SessionCart"));
                }
                else
                {
                    HttpContext.Session.SetInt32("SessionCart", _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count);

                    return View(HttpContext.Session.GetInt32("SessionCart"));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
           
        }
    }
}
