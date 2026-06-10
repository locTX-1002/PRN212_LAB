using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public interface ICategoryService
    {
        public List<Category> GetCategories();
        public Category? GetCategoryWithMostProducts();
        public List<Category> GetTopCategoriesByProductCount(int top);
    }
}
