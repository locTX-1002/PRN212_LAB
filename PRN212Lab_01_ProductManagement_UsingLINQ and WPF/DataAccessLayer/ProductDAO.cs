using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class ProductDAO
    {
        public static List<Product> GetProducts()
        {
            using var db = new MyDbContext();
            return db.Products.Include(p => p.Category).AsNoTracking().ToList();
        }

        public static async Task<List<Product>> GetProductsAsync()
        {
            using var db = new MyDbContext();
            return await db.Products.Include(p => p.Category).AsNoTracking().ToListAsync();
        }

        public static Product? GetProductById(int id)
        {
            using var db = new MyDbContext();
            return db.Products.Include(p => p.Category).AsNoTracking().FirstOrDefault(p => p.ProductId == id);
        }

        public static void SaveProduct(Product p)
        {
            using var db = new MyDbContext();
            db.Products.Add(p);
            db.SaveChanges();
        }

        public static void UpdateProduct(Product product)
        {
            using var db = new MyDbContext();
            var existing = db.Products.Find(product.ProductId);
            if (existing == null) return;
            existing.ProductName = product.ProductName;
            existing.UnitPrice = product.UnitPrice;
            existing.UnitsInStock = product.UnitsInStock;
            existing.CategoryId = product.CategoryId;
            db.SaveChanges();
        }

        public static void DeleteProduct(Product product)
        {
            using var db = new MyDbContext();
            var existing = db.Products.Find(product.ProductId);
            if (existing == null) return;
            db.Products.Remove(existing);
            db.SaveChanges();
        }
    }
}
