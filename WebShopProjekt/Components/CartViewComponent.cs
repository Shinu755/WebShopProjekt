using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebShopProjekt.Models;

namespace WebShopProjekt.Components
{
    public class CartViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!string.IsNullOrEmpty(cartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }

            ViewBag.NumberOfItems = cart.Count;
            return View("CartView");
        }
    }
}
