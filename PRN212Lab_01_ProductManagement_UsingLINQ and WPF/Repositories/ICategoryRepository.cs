using BusinessObjects;
using System.Collections.Generic;

namespace Repositories
{
    public interface ICategoryRepository
    {
        List<Category> GetCategories();
    }
}
