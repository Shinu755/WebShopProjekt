using Microsoft.EntityFrameworkCore.Storage;

namespace WebShopProjekt.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public bool IsDiscount { get; set; }
        public float DiscountPrice { get; set; }
        public int NumberOfSoldItems { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public bool isHot { get; set; }
        public int CategoryId { get; set; }
        public ICollection<ProductImage> images { get; set; } = new List<ProductImage>();
    }
}