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
            return db.Categories.AsNoTracking().ToList();
        }
    }
}
