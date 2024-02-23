using WebShopProjekt.Models;

namespace WebShopProjekt.ViewModels
{
    public class ProductsViewModel
    {
        public List<Product> TopSellingProducts { get; set; }
        public List<Product> NewestProducts { get; set; }
        public List<Product> DiscountedProducts { get; set; }
        public List<Product> HotProducts { get; set; }

    }
}
