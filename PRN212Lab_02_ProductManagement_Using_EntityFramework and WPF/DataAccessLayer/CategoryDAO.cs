using System.Collections.Generic;
using System.Linq;
using BusinessObjects;

namespace DataAccessLayer;

public class CategoryDAO
{
    public static List<Category> GetCategories()
    {
        using var context = new MyStoreContext();
        return context.Categories.ToList();
    }
}
