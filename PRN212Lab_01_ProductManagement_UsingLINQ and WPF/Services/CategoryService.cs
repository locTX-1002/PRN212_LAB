using BusinessObjects;
using Repositories;
using System.Collections.Generic;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryService() : this(new CategoryRepository()) { }

        public CategoryService(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public List<Category> GetCategories() => categoryRepository.GetCategories();
        public Category? GetCategoryWithMostProducts() => categoryRepository.GetCategoryWithMostProducts();
        public List<Category> GetTopCategoriesByProductCount(int top) => categoryRepository.GetTopCategoriesByProductCount(top);
    }
}
