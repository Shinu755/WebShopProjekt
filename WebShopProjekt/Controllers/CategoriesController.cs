using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebShopProjekt.Data;
using WebShopProjekt.Models;

namespace WebShopProjekt.Controllers
{
    [Authorize(Policy = "ZahtjevajEmailMario")]
    public class CategoriesController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context , Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            var listaKategorija = _context.Categories.ToList();
            return View(listaKategorija);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                    category.ImagePath = uniqueFileName;
                }

                _context.Categories.Add(category);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
