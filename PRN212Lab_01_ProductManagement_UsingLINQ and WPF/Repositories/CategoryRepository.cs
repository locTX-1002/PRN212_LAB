using BusinessObjects;
using DataAccessLayer;
using System.Collections.Generic;

namespace Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        public List<Category> GetCategories() => CategoryDAO.GetCategories();
        public Category? GetCategoryWithMostProducts() => CategoryDAO.GetCategoryWithMostProducts();
        public List<Category> GetTopCategoriesByProductCount(int top) => CategoryDAO.GetTopCategoriesByProductCount(top);
    }
}
