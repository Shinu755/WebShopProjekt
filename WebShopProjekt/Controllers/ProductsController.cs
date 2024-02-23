using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebShopProjekt.Data;
using WebShopProjekt.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebShopProjekt.Controllers
{
    [Authorize(Policy = "ZahtjevajEmailMario")]
    public class ProductsController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            var listaProizvoda = _context.Products.Include(x => x.images).ToList();
            return View(listaProizvoda);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            var product = await _context.Products
                                .Include(p => p.images)
                                .FirstOrDefaultAsync(p => p.Id == id);
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }


            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Include(x => x.images).Where(x => x.Id == id).FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }


            _context.Products.Remove(product);
            _context.SaveChanges();



            return RedirectToAction(nameof(Index));
        }
    

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,IsDiscount,DiscountPrice,NumberOfSoldItems,AddedDate,isHot,CategoryId")] Product product, List<IFormFile> images)
        {
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    product.images.Add(new ProductImage { ImagePath = uniqueFileName, });
                }
            }
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var listaKategorija = _context.Categories.ToList();
            ViewBag.Categories = listaKategorija;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product product, int categoryId, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                var category = _context.Categories.Where(x => x.Id == categoryId).FirstOrDefault();
                if (category == null)
                {
                    return NotFound();
                }
                product.AddedDate = DateTime.Now;
                if (images != null && images.Count > 0)
                {
                    foreach (var image in images)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }

                        //ProductImage productImage = new ProductImage();
                        //productImage.ImagePath = uniqueFileName;
                        //product.images.Add(productImage);

                        product.images.Add(new ProductImage { ImagePath = uniqueFileName, });
                    }
                }
                category.Products.Add(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        private bool ProductExists(int id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }


    }
}
