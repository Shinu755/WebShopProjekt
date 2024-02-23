using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using WebShopProjekt.Data;
using WebShopProjekt.Models;
using WebShopProjekt.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebShopProjekt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ProductsViewModel productsViewModel = new ProductsViewModel();
            productsViewModel.TopSellingProducts = _context.Products.Include(x => x.images).OrderByDescending(x => x.NumberOfSoldItems).Take(4).ToList();
            productsViewModel.NewestProducts = _context.Products.Include(x => x.images).OrderByDescending(x => x.AddedDate).Take(4).ToList();
            productsViewModel.DiscountedProducts = _context.Products.Include(x => x.images).Where(x => x.IsDiscount).OrderBy(x => Guid.NewGuid()).Take(4).ToList();
            productsViewModel.HotProducts = _context.Products.Include(x => x.images).Where(x => x.isHot).OrderBy(x => Guid.NewGuid()).Take(4).ToList();
            return View(productsViewModel);
        }

        [Authorize(Policy = "ZahtjevajEmailMario")]
        public IActionResult ControlPanel()
        {
            return View();
        }

        public IActionResult Categories()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }
        public IActionResult Products(int? categoryId, string searchTerm, string sortOrder, int? page)
        {

            var upit = _context.Products.Include(x => x.images).AsQueryable();

            if (categoryId != null)
            {
                upit = upit.Where(x => x.CategoryId == categoryId);
            };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                upit = upit.Where(x => x.Name.Contains(searchTerm) || x.Description.Contains(searchTerm));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    upit = upit.OrderByDescending(x => x.Name);
                    break;
                case "price":
                    upit = upit.OrderBy(x => x.Price);
                    break;
                case "price_desc":
                    upit = upit.OrderByDescending(x => x.Price);
                    break;
                case "newest":
                    upit = upit.OrderBy(x => x.AddedDate);
                    break;
                case "oldest":
                    upit = upit.OrderByDescending(x => x.AddedDate);
                    break;
                default:
                    upit = upit.OrderBy(x => x.Name);
                    break;
            }

            var listaproizvoda = upit.ToList();

            int pageSize = 1;
            int pageNumber = page ?? 1;

            int numberOfItems = listaproizvoda.Count();

            var listaPoStranici = listaproizvoda.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            ViewBag.CategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItem = numberOfItems;
            ViewBag.TotalPages = (int)Math.Ceiling((double)numberOfItems / pageSize);
            return View(listaproizvoda);

        }
        public IActionResult ProductDetails(int productId)
        {


            var products = _context.Products.Include(x => x.images).Where(x => x.Id == productId).FirstOrDefault();
            if (productId == null)
            {
                return NotFound();
            }
            return View(products);
        }

        public IActionResult AddToCart(int productId)
        {
            var CartJson = HttpContext.Session.GetString("Cart");
            List<CartItem> cart;

            if (!string.IsNullOrEmpty(CartJson))
            {
                cart = JsonConvert.DeserializeObject<List<CartItem>>(CartJson);
            }
            else
            {
                cart = new List<CartItem>();
            }

            var cartItem = cart.FirstOrDefault(x => x.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem { ProductId = productId, Quantity = 1});
            }
            CartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", CartJson);

            return RedirectToAction("Cart");

        }
        public IActionResult Cart()
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

            var products = _context.Products.Include(x => x.images).Where(x => cart.Select(p => p.ProductId).Contains(x.Id)).ToList();

            var cartViewModel = cart.Select(x => new CartViewModel
            {
                Product = products.First(p => p.Id == x.ProductId),
                Quantity = x.Quantity
            }).ToList();

            return View(cartViewModel);
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
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

            var cartItem = cart.FirstOrDefault(x => x.ProductId == productId);

            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
            }
            else if (cartItem != null && quantity <= 0)
            {
                cart.Remove(cartItem);
            }
            else
            {
                cart.Add(cartItem);
            }


            cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);

            return RedirectToAction("Cart");
        }
        [HttpPost]
        public ActionResult RemoveFromCart(int productId) 
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

            var cartItem = cart.FirstOrDefault(x => x.ProductId == productId);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }
            
            cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);

            return RedirectToAction("Cart");
        }

        public IActionResult Order()
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

            var products = _context.Products.Include(x => x.images).Where(x => cart.Select(p => p.ProductId).Contains(x.Id)).ToList();

            var cartViewModel = cart.Select(x => new CartViewModel
            {
                Product = products.First(p => p.Id == x.ProductId),
                Quantity = x.Quantity
            }).ToList();


            return View(cartViewModel);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Order(OrderViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            Order order = new Order()
            {
                UserId = userId,
                Address = model.Address,
                Phone = model.Phone,
                Zip = model.Zip,
                Delivery = model.delivery,
                Name = model.Name,
                LastName = model.LastName,
                City = model.City,
            };

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

            foreach (var item in cart)
            {
                var product = _context.Products.FirstOrDefault(x => x.Id == item.ProductId);

                if (product != null)
                {
                    product.NumberOfSoldItems += item.Quantity;
                    item.Price = product.Price;
                }
            }

            order.Items = cart;

            _context.orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return View("ThankYou");
        }
        [Authorize(Policy = "ZahtjevajEmailMario")]
        public IActionResult Orders()
        {
            var orders = _context.orders.Include(x => x.Items).ThenInclude(t => t.Product).ToList();

            return View(orders);
        }

        public IActionResult MyOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = _context.orders.Include(x => x.Items).ThenInclude(t => t.Product).ToList().Where(c => c.UserId == userId).ToList();

            return View(orders);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
