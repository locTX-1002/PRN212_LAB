using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public class CategoryDAO
    {
        public static List<Category> GetCategories()
        {
            using var db = new MyDbContext();
            return db.Categories.Include(c => c.Products).AsNoTracking().ToList();
        }

        public static Category? GetCategoryWithMostProducts()
        {
            using var db = new MyDbContext();
            return db.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .OrderByDescending(c => c.Products.Count)
                .FirstOrDefault();
        }

        public static List<Category> GetTopCategoriesByProductCount(int top)
        {
            using var db = new MyDbContext();
            return db.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .OrderByDescending(c => c.Products.Count)
                .Take(top)
                .ToList();
        }
    }
}
