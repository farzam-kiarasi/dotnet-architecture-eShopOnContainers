using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopOnContainers.WebMVC.Services;
using Microsoft.eShopOnContainers.WebMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.WebMVC.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    public class CartController : Controller
    {
        private readonly IBasketService _basketSvc;
        private readonly ICatalogService _catalogSvc;
        private readonly ICouponService _couponSvc;
        private readonly IIdentityParser<ApplicationUser> _appUserParser;

        public CartController(IBasketService basketSvc, ICatalogService catalogSvc, IIdentityParser<ApplicationUser> appUserParser, ICouponService couponService)
        {
            _basketSvc = basketSvc;
            _catalogSvc = catalogSvc;
            _appUserParser = appUserParser;
            _couponSvc = couponService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = _appUserParser.Parse(HttpContext.User);
                var vm = await _basketSvc.GetBasket(user);

                return View(vm);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Dictionary<string, int> quantities, string couponCode, string action)
        {
            try
            {
                var user = _appUserParser.Parse(HttpContext.User);

                var vm = await _basketSvc.GetBasket(user);
                if (action == "[ Submit Coupon ]")
                {
                    var updatedBasket = await _couponSvc.Apply(vm, couponCode);
                    await _basketSvc.ApplyCoupon(updatedBasket);
                }
                if (action == "[ Update ]")
                {
                    var basket = await _basketSvc.SetQuantities(user, quantities);
                }

                if (action == "[ Checkout ]")
                {
                    return RedirectToAction("Create", "Order");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return View();
        }
        public async Task<IActionResult> AddToCart(CatalogItem productDetails)
        {
            try
            {
                if (productDetails?.Id != null)
                {
                    var user = _appUserParser.Parse(HttpContext.User);
                    await _basketSvc.AddItemToBasket(user, productDetails.Id);
                }
                return RedirectToAction("Index", "Catalog");
            }
            catch (Exception ex)
            {
                // Catch error when Basket.api is in circuit-opened mode                 
                HandleException(ex);
            }

            return RedirectToAction("Index", "Catalog", new { errorMsg = ViewBag.BasketInoperativeMsg });
        }

        private void HandleException(Exception ex)
        {
            ViewBag.BasketInoperativeMsg = $"Basket Service is inoperative {ex.GetType().Name} - {ex.Message}";
        }
    }
}
